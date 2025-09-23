using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ocelot.Models;

namespace Ocelot.Views.Panels
{
    public partial class NPCPanel : UserControl
    {
        private NPCBase _npc;

        public NPCPanel()
        {
            InitializeComponent();
            InitializeComboBoxes();
        }

        private void InitializeComboBoxes()
        {
            // Initialize Type ComboBox
            foreach (NPCType type in Enum.GetValues(typeof(NPCType)))
            {
                var item = new ComboBoxItem { Content = type.ToString(), Tag = type };
                TypeComboBox.Items.Add(item);
            }

            // Initialize Icon ComboBox
            foreach (IconType icon in Enum.GetValues(typeof(IconType)))
            {
                var item = new ComboBoxItem { Content = icon.ToString(), Tag = icon };
                IconComboBox.Items.Add(item);
            }
        }

        public void LoadData(NPCBase npc)
        {
            _npc = npc;

            if (_npc == null) return;

            // Load basic information
            IDTextBox.Text = _npc.ID.ToString("X8");
            ModelHeadTextBox.Text = _npc.ModelHead.ToString();
            UniformNumberTextBox.Text = _npc.UniformNumber.ToString();
            BootsNumberTextBox.Text = _npc.BootsNumber.ToString();
            GloveNumberTextBox.Text = _npc.GloveNumber.ToString();

            // Set ComboBox selections
            foreach (ComboBoxItem item in TypeComboBox.Items)
            {
                if (item.Tag is NPCType type && (int)type == _npc.Type)
                {
                    TypeComboBox.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in IconComboBox.Items)
            {
                if (item.Tag is IconType icon && (int)icon == _npc.Icon)
                {
                    IconComboBox.SelectedItem = item;
                    break;
                }
            }

            // Attach event handlers
            AttachEventHandlers();
        }

        private void AttachEventHandlers()
        {
            IDTextBox.TextChanged += (s, e) =>
            {
                if (_npc != null && int.TryParse(IDTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
                    _npc.ID = result;
            };

            ModelHeadTextBox.TextChanged += (s, e) =>
            {
                if (_npc != null && int.TryParse(ModelHeadTextBox.Text, out int result))
                    _npc.ModelHead = result;
            };

            UniformNumberTextBox.TextChanged += (s, e) =>
            {
                if (_npc != null && int.TryParse(UniformNumberTextBox.Text, out int result))
                    _npc.UniformNumber = result;
            };

            BootsNumberTextBox.TextChanged += (s, e) =>
            {
                if (_npc != null && int.TryParse(BootsNumberTextBox.Text, out int result))
                    _npc.BootsNumber = result;
            };

            GloveNumberTextBox.TextChanged += (s, e) =>
            {
                if (_npc != null && int.TryParse(GloveNumberTextBox.Text, out int result))
                    _npc.GloveNumber = result;
            };
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_npc != null && TypeComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is NPCType type)
            {
                _npc.Type = (int)type;
            }
        }

        private void IconComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_npc != null && IconComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is IconType icon)
            {
                _npc.Icon = (int)icon;
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

        #endregion
    }
}