using System;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BatteryBuddy.Core.Logic;
using BatteryBuddy.Core.Settings;
using Microsoft.Win32.SafeHandles;

namespace BatteryBuddy.App.Services
{
    /// <summary>Publishes BatteryBuddy snapshots to a same-user, local, read-only named-pipe client.</summary>
    internal sealed class StreamDeckPipePublisher : IDisposable
    {
        internal const string PipeName = "BatteryBuddy.StreamDeck.v1";
        private const int MaxDevices = 128;
        private const int MaxFrameBytes = 64 * 1024;
        private const int PipeBufferBytes = 64 * 1024;
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(10);
        private const string HeartbeatPayload = "{\"version\":1,\"type\":\"heartbeat\"}";

        private readonly Action<Exception> _onError;
        private readonly string _pipeName;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly SemaphoreSlim _snapshotChanged = new SemaphoreSlim(0, 1);
        private readonly object _snapshotLock = new object();
        private string _latestPayload;
        private bool _changePending;
        private Task _runTask;

        public StreamDeckPipePublisher(Action<Exception> onError, string pipeName = PipeName)
        {
            _onError = onError ?? throw new ArgumentNullException(nameof(onError));
            if (string.IsNullOrWhiteSpace(pipeName) || pipeName.IndexOfAny(new[] { '\\', '/', ':' }) >= 0)
                throw new ArgumentException("The pipe name is invalid.", nameof(pipeName));
            _pipeName = pipeName;
        }

        public void Start()
        {
            if (_runTask != null)
                throw new InvalidOperationException("The Stream Deck publisher has already been started.");

            _runTask = Task.Run(() => RunAsync(_cts.Token));
        }

        public void Publish(MonitorSnapshot snapshot, AppSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (snapshot == null)
                return;
            if (snapshot.Devices.Count > MaxDevices)
                throw new InvalidOperationException("The Stream Deck battery snapshot exceeds the device count limit.");

            var message = new SnapshotMessage
            {
                Version = 1,
                Type = "snapshot",
                SnapshotTimeUtc = FormatUtcTimestamp(snapshot.Time),
                IsComplete = snapshot.IsComplete,
                Devices = snapshot.Devices.Select(device => new DeviceMessage
                {
                    Id = device.Id,
                    Name = DeviceNames.Display(device),
                    Category = device.Category.ToString(),
                    Level = device.Level,
                    IsCharging = device.IsCharging,
                    LastContactUtc = FormatUtcTimestamp(device.LastContact),
                    Color = FormatColor(ThresholdEvaluator.Evaluate(device.Level, settings.GetDevice(device.Id)))
                }).ToArray()
            };

            var serializer = new DataContractJsonSerializer(typeof(SnapshotMessage),
                new DataContractJsonSerializerSettings { MaxItemsInObjectGraph = MaxDevices * 10 + 8 });
            byte[] frame;
            using (var buffer = new MemoryStream())
            {
                serializer.WriteObject(buffer, message);
                if (buffer.Length + 1 > MaxFrameBytes)
                    throw new InvalidOperationException("The Stream Deck battery snapshot exceeds the message size limit.");
                frame = buffer.ToArray();
            }
            var payload = Encoding.UTF8.GetString(frame);

            lock (_snapshotLock)
            {
                _latestPayload = payload;
                if (!_changePending)
                {
                    _changePending = true;
                    _snapshotChanged.Release();
                }
            }
        }

        private static string FormatUtcTimestamp(DateTimeOffset value)
        {
            return value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);
        }

        private static string FormatColor(BatteryBuddy.Core.Models.ChargeLevel level)
        {
            // Stream Deck keys have a dark background, independent of the Windows taskbar theme.
            var color = TrayIconRenderer.LevelColor(level, false);
            return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using (var pipe = CreatePipe(_pipeName))
                    using (cancellationToken.Register(pipe.Dispose))
                    {
                        try
                        {
                            // The registration owns cancellation; a second token callback can call CancelIoEx on the closed handle.
                            await pipe.WaitForConnectionAsync().ConfigureAwait(false);
                            await SendSnapshotsAsync(pipe, cancellationToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        catch (IOException) when (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        catch (IOException) when (!cancellationToken.IsCancellationRequested)
                        {
                            // A client disconnect is normal; recreate the guarded first pipe instance.
                        }
                    }
                }
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _onError(ex);
            }
        }

        private async Task SendSnapshotsAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
        {
            using (var writer = new StreamWriter(pipe, new UTF8Encoding(false), 4096, true) { AutoFlush = true })
            {
                var payload = GetLatestPayload();
                if (payload != null)
                    await writer.WriteLineAsync(payload).ConfigureAwait(false);

                while (true)
                {
                    var changed = await _snapshotChanged.WaitAsync(HeartbeatInterval, cancellationToken).ConfigureAwait(false);
                    lock (_snapshotLock)
                    {
                        if (changed)
                            _changePending = false;
                        payload = changed ? _latestPayload : HeartbeatPayload;
                    }

                    if (payload != null)
                        await writer.WriteLineAsync(payload).ConfigureAwait(false);
                }
            }
        }

        private string GetLatestPayload()
        {
            lock (_snapshotLock)
                return _latestPayload;
        }

        [DataContract]
        private sealed class SnapshotMessage
        {
            [DataMember(Name = "version", Order = 1)]
            public int Version { get; set; }

            [DataMember(Name = "type", Order = 2)]
            public string Type { get; set; }

            [DataMember(Name = "snapshotTimeUtc", Order = 3)]
            public string SnapshotTimeUtc { get; set; }

            [DataMember(Name = "isComplete", Order = 4)]
            public bool IsComplete { get; set; }

            [DataMember(Name = "devices", Order = 5)]
            public DeviceMessage[] Devices { get; set; }
        }

        [DataContract]
        private sealed class DeviceMessage
        {
            [DataMember(Name = "id", Order = 1)]
            public string Id { get; set; }

            [DataMember(Name = "name", Order = 2)]
            public string Name { get; set; }

            [DataMember(Name = "category", Order = 3)]
            public string Category { get; set; }

            [DataMember(Name = "level", Order = 4)]
            public int? Level { get; set; }

            [DataMember(Name = "isCharging", Order = 5)]
            public bool IsCharging { get; set; }

            [DataMember(Name = "lastContactUtc", Order = 6)]
            public string LastContactUtc { get; set; }

            [DataMember(Name = "color", Order = 7)]
            public string Color { get; set; }
        }

        private static NamedPipeServerStream CreatePipe(string pipeName)
        {
            var userSid = WindowsIdentity.GetCurrent().User;
            if (userSid == null)
                throw new InvalidOperationException("The current Windows user SID is unavailable.");

            var sddl = "O:" + userSid.Value + "D:P(A;;GR;;;" + userSid.Value + ")";
            if (!ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, 1, out var descriptor, out _))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                var attributes = new SecurityAttributes
                {
                    Length = Marshal.SizeOf(typeof(SecurityAttributes)),
                    SecurityDescriptor = descriptor,
                    InheritHandle = false
                };
                var handle = CreateNamedPipe(
                    @"\\.\pipe\" + pipeName,
                    PipeAccessOutbound | FileFlagOverlapped | FileFlagFirstPipeInstance,
                    PipeTypeByte | PipeReadModeByte | PipeWait | PipeRejectRemoteClients,
                    1,
                    PipeBufferBytes,
                    0,
                    0,
                    ref attributes);

                if (handle.IsInvalid)
                {
                    var error = Marshal.GetLastWin32Error();
                    handle.Dispose();
                    throw new System.ComponentModel.Win32Exception(error);
                }

                try
                {
                    return new NamedPipeServerStream(PipeDirection.Out, true, false, handle);
                }
                catch
                {
                    handle.Dispose();
                    throw;
                }
            }

            finally
            {
                LocalFree(descriptor);
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            try { _snapshotChanged.Release(); }
            catch (SemaphoreFullException) { }

            try
            {
                if (_runTask != null && !_runTask.Wait(TimeSpan.FromSeconds(2)))
                    _onError(new TimeoutException("The Stream Deck pipe publisher did not stop within two seconds."));
            }
            catch (AggregateException ex) { _onError(ex.GetBaseException()); }

            _snapshotChanged.Dispose();
            _cts.Dispose();
        }

        private const uint PipeAccessOutbound = 0x00000002;
        private const uint FileFlagOverlapped = 0x40000000;
        private const uint FileFlagFirstPipeInstance = 0x00080000;
        private const uint PipeTypeByte = 0x00000000;
        private const uint PipeReadModeByte = 0x00000000;
        private const uint PipeWait = 0x00000000;
        private const uint PipeRejectRemoteClients = 0x00000008;

        [StructLayout(LayoutKind.Sequential)]
        private struct SecurityAttributes
        {
            public int Length;
            public IntPtr SecurityDescriptor;
            [MarshalAs(UnmanagedType.Bool)]
            public bool InheritHandle;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
            string stringSecurityDescriptor,
            uint stringSDRevision,
            out IntPtr securityDescriptor,
            out uint securityDescriptorSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateNamedPipeW")]
        private static extern SafePipeHandle CreateNamedPipe(
            string name,
            uint openMode,
            uint pipeMode,
            uint maxInstances,
            uint outBufferSize,
            uint inBufferSize,
            uint defaultTimeout,
            ref SecurityAttributes securityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr memory);
    }
}
