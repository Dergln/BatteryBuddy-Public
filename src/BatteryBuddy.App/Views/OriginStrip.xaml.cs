using System.Windows;
using System.Windows.Controls;
using BatteryBuddy.App.Services;

namespace BatteryBuddy.App.Views
{
    /// <summary>Kompakter Herkunftshinweis; die vorhandene Buttonhoehe bestimmt seine Groesse.</summary>
    public partial class OriginStrip : UserControl
    {
        public OriginStrip()
        {
            InitializeComponent();
            IdeaImage.Source = AppAssets.OriginImage("idea");
            DevelopmentImage.Source = AppAssets.OriginImage("ai");
            TeamworkImage.Source = AppAssets.OriginImage("teamwork");
            foreach (var item in new[] { IdeaItem, DevelopmentItem, TeamworkItem })
            {
                var button = (Button)item.Parent;
                button.Click += (s, e) => AccessibleUi.OpenInformation(button,
                    System.Windows.Automation.AutomationProperties.GetName(button),
                    System.Windows.Automation.AutomationProperties.GetHelpText(button));
            }
            SetTexts();
        }

        public void SetTexts()
        {
            IdeaText.Text = Loc.Get("origin.idea");
            DevelopmentText.Text = Loc.Get("origin.development");
            TeamworkText.Text = Loc.Get("origin.refinement");
            // Bei wenig Platz bleibt der vollstaendige Text per Tooltip und Screenreader erreichbar.
            SetDescription(IdeaItem, IdeaText.Text, Loc.Get("origin.ideaDescription"));
            SetDescription(DevelopmentItem, DevelopmentText.Text, Loc.Get("origin.developmentDescription"));
            SetDescription(TeamworkItem, TeamworkText.Text, Loc.Get("origin.refinementDescription1"),
                Loc.Get("origin.refinementDescription2"), Loc.Get("origin.refinementDescription3"));
        }

        private static void SetDescription(DockPanel item, string title, params string[] paragraphs)
        {
            // Auch das Padding gehoert zum Hover-Ziel, nicht nur Bild und Beschriftung.
            FooterToolTips.Set((FrameworkElement)item.Parent, FooterToolTips.Text(paragraphs));
            AccessibleUi.Name((Button)item.Parent, title);
        }
    }
}
