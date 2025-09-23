using System.Linq;
using System.Windows.Controls;
using Ocelot.Models;

namespace Ocelot.Views.Panels
{
    public partial class HealPointPanel : UserControl
    {
        private Healpoint _currentHealpoint;

        public HealPointPanel()
        {
            InitializeComponent();
        }

        public void LoadData(Healpoint healpoint)
        {
            _currentHealpoint = healpoint;

            if (healpoint != null)
            {
                HealpointNameTextBox.Text = healpoint.HealpointName ?? string.Empty;
                MapIDTextBox.Text = healpoint.MapID ?? string.Empty;

                // Résumé des HealAreas
                if (healpoint.HealAreas != null && healpoint.HealAreas.Count > 0)
                {
                    var summary = $"Total Heal Areas: {healpoint.HealAreas.Count}\n\n";
                    summary += string.Join("\n", healpoint.HealAreas.Select(ha =>
                        $"• {ha.HealAreaName} at ({ha.Position?.X:F2}, {ha.Position?.Y:F2}, {ha.Position?.Z:F2})"));

                    HealAreasSummaryTextBlock.Text = summary;
                }
                else
                {
                    HealAreasSummaryTextBlock.Text = "No Heal Areas found.";
                }
            }
            else
            {
                ClearAllFields();
            }
        }

        private void ClearAllFields()
        {
            HealpointNameTextBox.Text = string.Empty;
            MapIDTextBox.Text = string.Empty;
            HealAreasSummaryTextBlock.Text = string.Empty;
        }

        // Events pour la sauvegarde des modifications (optionnel, pour le futur)
        private void HealpointNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentHealpoint != null)
            {
                _currentHealpoint.HealpointName = HealpointNameTextBox.Text;
            }
        }

        private void MapIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentHealpoint != null)
            {
                _currentHealpoint.MapID = MapIDTextBox.Text;
            }
        }
    }
}