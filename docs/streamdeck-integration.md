# Stream-Deck-Integration

BatteryBuddy kann seinen aktuellen Gerätestatus an das separate BatteryBuddy-Stream-Deck-Plugin weitergeben. Dazu wird kein HTTP-Server und keine Netzwerkverbindung geöffnet.

## Sicherheit und Datenschutz

- Die Pipe `\\.\pipe\BatteryBuddy.StreamDeck.v1` ist eine lokale Windows-Named-Pipe. Windows weist Remote-Clients ab.
- Die Pipe-ACL erlaubt nur den Windows-Benutzer, unter dem BatteryBuddy läuft.
- Die Datenrichtung ist ausschließlich von BatteryBuddy zum Plugin. Die Pipe wird vom Server nur zum Schreiben und vom Plugin nur zum Lesen geöffnet; es gibt keine Befehle oder Steuerungsmöglichkeit.
- Es werden nur Geräte-ID, bereinigter Anzeigename, Kategorie, Ladestand (auch `null` für unbekannt), Ladestatus, aufgelöste Anzeigefarbe, letzter Gerätekontakt sowie Zeitpunkt/Vollständigkeit des Snapshots übertragen. Providerdiagnosen und Geräteverbindungskennungen werden nicht übertragen.
- Statusdaten werden für diese Integration nicht gespeichert oder protokolliert. Ein bereits kompromittierter Prozess desselben Windows-Benutzers kann die lesbaren Geräteinformationen ebenfalls auslesen.
- BatteryBuddy wird standardmäßig ohne Administratorrechte gestartet. Wird es manuell erhöht gestartet, bleibt der Kanal trotzdem ausschließlich lesend; ein niedriger privilegierter Plugin-Prozess kann damit keine Aktion in BatteryBuddy auslösen.

## Protokoll, Version 1

Jede UTF-8-Zeile ist ein JSON-Frame. Snapshots enthalten höchstens 128 Geräte und sind auf 64 KiB begrenzt. Alle zehn Sekunden wird bei bestehender Verbindung zusätzlich ein kleiner Heartbeat gesendet, damit getrennte Clients zeitnah erkannt werden.

```json
{
  "version": 1,
  "type": "snapshot",
  "snapshotTimeUtc": "2026-11-05T13:00:00.0000000Z",
  "isComplete": true,
  "devices": [
    {
      "id": "bt:AABBCCDDEEFF",
      "name": "Headset",
      "category": "Headset",
      "level": 85,
      "isCharging": false,
      "lastContactUtc": "2026-11-05T12:59:00.0000000Z",
      "color": "#6CE06C"
    }
  ]
}
```

`level` liegt zwischen 0 und 100 oder ist `null`, wenn kein Wert bekannt ist. `isComplete` kennzeichnet frühe Zwischenergebnisse, die BatteryBuddy gegebenenfalls beim allerersten Scan veröffentlicht. Beim Verbinden sendet BatteryBuddy den zuletzt verfügbaren Snapshot; danach folgen aktualisierte Snapshots. Nach einem Verbindungsabbruch kann ein Client die Pipe neu öffnen und erhält wieder den aktuellen Stand.

`category` ist die wirksame Gerätekategorie wie in der Oberfläche und im Tray: Eine gespeicherte manuelle Zuordnung hat Vorrang vor der automatischen Erkennung. Ohne Zuordnung beziehungsweise nach dem Zurücksetzen auf „Automatisch“ gilt wieder die erkannte Kategorie. Das Speichern veröffentlicht die Änderung auch an bereits verbundene Clients, ohne eine neue Akkuabfrage abzuwarten; Geräte-ID und erkannte Ursprungskategorie bleiben unverändert.

`color` ist ein additives, für ältere Version-1-Publisher optionales Feld im Format `#RRGGBB`. BatteryBuddy löst zuerst das gespeicherte Geräteprofil oder die globalen Standardwerte über `AppSettings.GetDevice` auf und verwendet denselben `ThresholdEvaluator` wie die Tray-Anzeige: kritische, niedrige und mittlere Schwellen gelten einschließlich ihrer Grenzwerte. Die Farbe stammt aus der bestehenden Palette für dunkle Hintergründe, passend zu Stream-Deck-Tasten; ohne bekannten Ladestand wird neutrales Grau übertragen. Auch das Speichern geänderter Einstellungen veröffentlicht den aktuellen Snapshot erneut, ohne eine neue Akkuabfrage abzuwarten. Das Plugin dupliziert keine Schwellenlogik und akzeptiert nur sechsstellige Hexfarben, keine Bilder oder URLs. Ältere Plugins können das zusätzliche Feld ignorieren.

Heartbeat-Frames enthalten keine Gerätedaten und sehen so aus:

```json
{"version":1,"type":"heartbeat"}
```
