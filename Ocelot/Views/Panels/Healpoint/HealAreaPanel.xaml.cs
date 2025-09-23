using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using Ocelot.Models;

namespace Ocelot.Views.Panels
{
    public partial class HealAreaPanel : UserControl
    {
        private HealArea _currentHealArea;

        public HealAreaPanel()
        {
            InitializeComponent();
        }

        public void LoadData(HealArea healArea)
        {
            _currentHealArea = healArea;

            if (healArea != null)
            {
                HealAreaNameTextBox.Text = healArea.HealAreaName ?? string.Empty;

                // Position (n'affiche pas W car il n'est pas utilisé pour les heal areas)
                if (healArea.Position != null)
                {
                    PositionXTextBox.Text = healArea.Position.X.ToString("F6");
                    PositionYTextBox.Text = healArea.Position.Y.ToString("F6");
                    PositionZTextBox.Text = healArea.Position.Z.ToString("F6");
                }
                else
                {
                    PositionXTextBox.Text = "0";
                    PositionYTextBox.Text = "0";
                    PositionZTextBox.Text = "0";
                }
            }
            else
            {
                ClearAllFields();
            }
        }

        private void ClearAllFields()
        {
            HealAreaNameTextBox.Text = string.Empty;
            PositionXTextBox.Text = "0";
            PositionYTextBox.Text = "0";
            PositionZTextBox.Text = "0";
        }

        private void FloatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permet seulement les chiffres, le point décimal et le signe moins
            Regex regex = new Regex(@"^[0-9.\-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        // Events pour la sauvegarde des modifications (optionnel, pour le futur)
        private void HealAreaNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentHealArea != null)
            {
                _currentHealArea.HealAreaName = HealAreaNameTextBox.Text;
            }
        }

        private void PositionXTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentHealArea?.Position != null && float.TryParse(PositionXTextBox.Text, out float value))
            {
                _currentHealArea.Position.X = value;
            }
        }

        private void PositionYTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentHealArea?.Position != null && float.TryParse(PositionYTextBox.Text, out float value))
            {
                _currentHealArea.Position.Y = value;
            }
        }

        private void PositionZTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentHealArea?.Position != null && float.TryParse(PositionZTextBox.Text, out float value))
            {
                _currentHealArea.Position.Z = value;
            }
        }
    }
}