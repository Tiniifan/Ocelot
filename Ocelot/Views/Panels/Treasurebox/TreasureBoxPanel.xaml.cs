using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ocelot.Models;

namespace Ocelot.Views.Panels
{
    public partial class TreasureBoxPanel : UserControl
    {
        private TBoxConfig _treasureBox;

        public TreasureBoxPanel()
        {
            InitializeComponent();
            InitializeComboBoxes();
        }

        private void InitializeComboBoxes()
        {
            // Initialize Type ComboBox
            foreach (TBoxType type in Enum.GetValues(typeof(TBoxType)))
            {
                var item = new ComboBoxItem { Content = type.ToString(), Tag = type };
                TypeComboBox.Items.Add(item);
            }

            // Set default mode
            ModeComboBox.SelectedIndex = 0; // Normal
        }

        public void LoadData(TBoxConfig treasureBox)
        {
            _treasureBox = treasureBox;

            if (_treasureBox == null) return;

            // Load basic information
            IDTextBox.Text = _treasureBox.TBoxID.ToString("X8");

            // Set Type ComboBox selection
            foreach (ComboBoxItem item in TypeComboBox.Items)
            {
                if (item.Tag is TBoxType type && type == _treasureBox.TboxType)
                {
                    TypeComboBox.SelectedItem = item;
                    break;
                }
            }

            // Load position
            LocationXTextBox.Text = _treasureBox.LocationX.ToString(CultureInfo.InvariantCulture);
            LocationYTextBox.Text = _treasureBox.LocationY.ToString(CultureInfo.InvariantCulture);
            LocationZTextBox.Text = _treasureBox.LocationZ.ToString(CultureInfo.InvariantCulture);
            RotationTextBox.Text = _treasureBox.Rotation.ToString(CultureInfo.InvariantCulture);

            // Load items
            TBoxFlagTextBox.Text = _treasureBox.TBoxFlag.ToString();

            // Update items container
            UpdateItemsContainer();

            // Attach event handlers
            AttachEventHandlers();
        }

        private void AttachEventHandlers()
        {
            IDTextBox.TextChanged += (s, e) =>
            {
                if (_treasureBox != null && int.TryParse(IDTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
                    _treasureBox.TBoxID = result;
            };

            LocationXTextBox.TextChanged += (s, e) =>
            {
                if (_treasureBox != null && float.TryParse(LocationXTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    _treasureBox.LocationX = result;
            };

            LocationYTextBox.TextChanged += (s, e) =>
            {
                if (_treasureBox != null && float.TryParse(LocationYTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    _treasureBox.LocationY = result;
            };

            LocationZTextBox.TextChanged += (s, e) =>
            {
                if (_treasureBox != null && float.TryParse(LocationZTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    _treasureBox.LocationZ = result;
            };

            RotationTextBox.TextChanged += (s, e) =>
            {
                if (_treasureBox != null && float.TryParse(RotationTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    _treasureBox.Rotation = result;
            };

            TBoxFlagTextBox.TextChanged += (s, e) =>
            {
                if (_treasureBox != null && int.TryParse(TBoxFlagTextBox.Text, out int result))
                    _treasureBox.TBoxFlag = result;
            };
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_treasureBox != null && TypeComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is TBoxType type)
            {
                _treasureBox.TboxType = type;
            }
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateItemsContainer();
        }

        private void UpdateItemsContainer()
        {
            ItemsContainer.Children.Clear();

            if (_treasureBox == null) return;

            bool isNormal = ModeComboBox.SelectedIndex == 0;

            if (isNormal)
            {
                // Normal mode - single item
                var itemTextBox = new TextBox
                {
                    Text = _treasureBox.ItemIDLight.ToString("X"),
                    Style = (Style)FindResource("ImaginationLabeledTextBox"),
                    Tag = "Item"
                };
                itemTextBox.PreviewTextInput += HexTextBox_PreviewTextInput;
                itemTextBox.TextChanged += (s, e) =>
                {
                    if (int.TryParse(itemTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
                        _treasureBox.ItemIDLight = result;
                };

                ItemsContainer.Children.Add(itemTextBox);
            }
            else
            {
                // Exclusive mode - two items
                var lightTextBox = new TextBox
                {
                    Text = _treasureBox.ItemIDLight.ToString("X"),
                    Style = (Style)FindResource("ImaginationLabeledTextBox"),
                    Tag = "Item ID Light"
                };
                lightTextBox.PreviewTextInput += HexTextBox_PreviewTextInput;
                lightTextBox.TextChanged += (s, e) =>
                {
                    if (int.TryParse(lightTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
                        _treasureBox.ItemIDLight = result;
                };

                ItemsContainer.Children.Add(lightTextBox);

                var shadowTextBox = new TextBox
                {
                    Text = _treasureBox.ItemIDShadow.ToString("X"),
                    Style = (Style)FindResource("ImaginationLabeledTextBox"),
                    Tag = "Item ID Shadow"
                };
                shadowTextBox.PreviewTextInput += HexTextBox_PreviewTextInput;
                shadowTextBox.TextChanged += (s, e) =>
                {
                    if (int.TryParse(shadowTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
                        _treasureBox.ItemIDShadow = result;
                };

                ItemsContainer.Children.Add(shadowTextBox);
            }
        }

        #region Input Validation

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9\-]+$");
        }

        private void HexTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9A-Fa-f]+$");
        }

        private void FloatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            e.Handled = !Regex.IsMatch(e.Text, $@"^[0-9{Regex.Escape(decimalSeparator)}\-]+$");
        }

        #endregion
    }
}