using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ocelot.Models;
using Ocelot.ViewModels;

namespace Ocelot.Views.Panels
{
    public partial class HealAreaPanel : UserControl
    {
        private HealArea _currentHealArea;

        private OcelotViewModel _viewModel;

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

                // Position (does not display W because it is not used for heal areas)
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

        public void SetViewModel(OcelotViewModel viewModel)
        {
            _viewModel = viewModel;
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
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            e.Handled = !Regex.IsMatch(e.Text, $@"^[0-9{Regex.Escape(decimalSeparator)}\-]+$");
        }

        private void HealAreaNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentHealArea == null || _viewModel == null)
                return;

            string newName = HealAreaNameTextBox.Text.Trim();

            // Checks if the name already exists for another HealArea
            bool nameExists = _viewModel.HealPoint?.HealAreas
                .Any(h => h != _currentHealArea && h.HealAreaName == newName) ?? false;

            if (nameExists)
            {
                MessageBox.Show(
                    $"The name '{newName}' is already used by another Heal Area.",
                    "Duplicate Name",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Display the old name
                HealAreaNameTextBox.Text = _currentHealArea.HealAreaName;
                HealAreaNameTextBox.CaretIndex = HealAreaNameTextBox.Text.Length;
            }
            else
            {
                // The name is unique, it can be applied.
                _currentHealArea.HealAreaName = newName;
                _viewModel.RefreshTreeViewNames();
            }
        }

        private void PositionXTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentHealArea?.Position != null && float.TryParse(PositionXTextBox.Text, out float value))
            {
                _currentHealArea.Position.X = value;
                _viewModel?.UpdateOverlays();
            }
        }

        private void PositionYTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentHealArea?.Position != null && float.TryParse(PositionYTextBox.Text, out float value))
            {
                _currentHealArea.Position.Y = value;
                _viewModel?.UpdateOverlays();
            }
        }

        private void PositionZTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_currentHealArea?.Position != null && float.TryParse(PositionZTextBox.Text, out float value))
            {
                _currentHealArea.Position.Z = value;
                _viewModel?.UpdateOverlays();
            }
        }
    }
}