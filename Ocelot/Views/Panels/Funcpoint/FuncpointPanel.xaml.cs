using System.Linq;
using System.Windows.Controls;
using Ocelot.Models;

namespace Ocelot.Views.Panels
{
    public partial class FuncpointPanel : UserControl
    {
        private Funcpoint _currentFuncpoint;

        public FuncpointPanel()
        {
            InitializeComponent();
        }

        public void LoadData(Funcpoint funcpoint)
        {
            _currentFuncpoint = funcpoint;

            if (funcpoint != null)
            {
                FuncpointNameTextBox.Text = funcpoint.FuncpointName ?? "";
                MapIDTextBox.Text = funcpoint.MapID ?? "";

                // Summary of events
                if (funcpoint.Events != null && funcpoint.Events.Count > 0)
                {
                    var eventCount = funcpoint.Events.Where(e => e.EventName.StartsWith("KO") || e.EventName.StartsWith("EV")).Count();
                    var mapJumpCount = funcpoint.Events.Where(e => e.EventName.StartsWith("MJ")).Count();
                    var soundEffectCount = funcpoint.Events.Where(e => e.EventName.StartsWith("MS")).Count();

                    EventsSummaryTextBlock.Text = $"Total Events: {funcpoint.Events.Count}\n" +
                                                  $"• Event Triggers: {eventCount}\n" +
                                                  $"• Map Jumps: {mapJumpCount}\n" +
                                                  $"• Sound Effects: {soundEffectCount}";
                }
                else
                {
                    EventsSummaryTextBlock.Text = "No events found";
                }
            }
        }
    }
}