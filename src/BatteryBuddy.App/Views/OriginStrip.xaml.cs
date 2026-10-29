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
            SetTexts();
        }

        public void SetTexts()
        {
            IdeaText.Text = Loc.Get("origin.idea");
            DevelopmentText.Text = Loc.Get("origin.development");
            TeamworkText.Text = Loc.Get("origin.refinement");
            // Bei wenig Platz bleibt der vollstaendige Text per Tooltip und Screenreader erreichbar.
            SetDescription(IdeaItem, Loc.Get("origin.ideaDescription"));
            SetDescription(DevelopmentItem, Loc.Get("origin.developmentDescription"));
            SetDescription(TeamworkItem, Loc.Get("origin.refinementDescription1"),
                Loc.Get("origin.refinementDescription2"), Loc.Get("origin.refinementDescription3"));
        }

        private static void SetDescription(DockPanel item, params string[] paragraphs)
        {
            // Auch das Padding gehoert zum Hover-Ziel, nicht nur Bild und Beschriftung.
            FooterToolTips.Set((FrameworkElement)item.Parent, FooterToolTips.Text(paragraphs));
        }
    }
}
