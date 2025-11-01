using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ocelot.Models;

namespace Ocelot.Views.Panels
{
    public partial class TalkPanel : UserControl
    {
        private NPCTalkConfig _talk;
        private TextBox _eventValueTextBox;

        public TalkPanel()
        {
            InitializeComponent();
            InitializeComboBoxes();
        }

        private void InitializeComboBoxes()
        {
            // Initialize Event Type ComboBox
            foreach (EventType eventType in Enum.GetValues(typeof(EventType)))
            {
                var item = new ComboBoxItem { Content = eventType.ToString(), Tag = eventType };
                EventTypeComboBox.Items.Add(item);
            }
        }

        public void LoadData(NPCTalkConfig talk)
        {
            _talk = talk;

            if (_talk == null) return;

            // Load basic information
            AutoTurnCheckBox.IsChecked = _talk.AutoTurn == 2;

            // Set Event Type ComboBox selection
            foreach (ComboBoxItem item in EventTypeComboBox.Items)
            {
                if (item.Tag is EventType eventType && (int)eventType == _talk.EventType)
                {
                    EventTypeComboBox.SelectedItem = item;
                    break;
                }
            }

            // Load condition
            if (_talk.EventCondition is string phaseAppearStr)
            {
                ConditionTextBox.Text = phaseAppearStr;
            }
            else
            {
                ConditionTextBox.Text = string.Empty;
            }

            // Update event value control
            UpdateEventValueControl();

            AttachEventHandlers();
        }

        private void AttachEventHandlers()
        {
            ConditionTextBox.TextChanged += (s, e) =>
            {
                if (_talk != null)
                {
                    if (string.IsNullOrEmpty(ConditionTextBox.Text))
                    {
                        _talk.EventCondition = 0;
                    }
                    else
                    {
                        _talk.EventCondition = ConditionTextBox.Text;
                    }
                }
            };
        }

        private void AutoTurnCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (_talk != null)
            {
                _talk.AutoTurn = AutoTurnCheckBox.IsChecked == true ? 2 : 0;
            }
        }

        private void EventTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_talk != null && EventTypeComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is EventType eventType)
            {
                _talk.EventType = (int)eventType;
                UpdateEventValueControl();
            }
        }

        private void UpdateEventValueControl()
        {
            EventValueContainer.Children.Clear();

            if (_talk == null) return;

            var eventValuePanel = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };

            var eventTypeLabel = new TextBlock
            {
                Text = "Event Value",
                Margin = new Thickness(0, 0, 0, 2),
                Foreground = (System.Windows.Media.Brush)FindResource("Theme.Text.PrimaryBrush")
            };
            eventValuePanel.Children.Add(eventTypeLabel);

            _eventValueTextBox = new TextBox
            {
                Style = (Style)FindResource("ImaginationTextBox"),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var eventType = (EventType)_talk.EventType;

            if (eventType == EventType.CompetitiveRoute)
            {
                _eventValueTextBox.Text = _talk.EventValue.ToString();

                _eventValueTextBox.TextChanged += (s, e) =>
                {
                    _talk.EventValue = _eventValueTextBox.Text;
                };
            }
            else if (eventType == EventType.Text)
            {
                _eventValueTextBox.Text = Convert.ToInt32(_talk.EventValue).ToString("X");
                _eventValueTextBox.PreviewTextInput += HexTextBox_PreviewTextInput;
                _eventValueTextBox.TextChanged += (s, e) =>
                {
                    if (int.TryParse(_eventValueTextBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
                        _talk.EventValue = result;
                };
            }
            else
            {
                if (_talk.EventValue != null)
                {
                    _eventValueTextBox.Text = _talk.EventValue.ToString();
                    _eventValueTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
                    _eventValueTextBox.TextChanged += (s, e) =>
                    {
                        if (int.TryParse(_eventValueTextBox.Text, out int result))
                            _talk.EventValue = result;
                    };
                }
            }

            eventValuePanel.Children.Add(_eventValueTextBox);
            EventValueContainer.Children.Add(eventValuePanel);
        }

        private void OpenConditionEditor_Click(object sender, RoutedEventArgs e)
        {
            if (_talk == null) return;

            // Check if the ./Tools/inz_cond folder exists
            string toolsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "inz_cond");
            string exePath = Path.Combine(toolsPath, "inz_cond_gui.exe");

            if (!Directory.Exists(toolsPath) || !File.Exists(exePath))
            {
                MessageBox.Show(
                    "inz_cond_gui.exe must be in the inz_cond folder within the Tools folder.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            // Prepare the arguments
            string arguments = "-o";

            // If EventCondition is a non-empty string, add it as an argument
            if (_talk.EventCondition is string phaseAppearStr && !string.IsNullOrEmpty(phaseAppearStr))
            {
                arguments += $" {phaseAppearStr}";
            }

            // Create the process
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = toolsPath
            };

            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    // Wait for the process to finish
                    process.WaitForExit();

                    // Read the output
                    string output = process.StandardOutput.ReadToEnd().Trim();

                    // Process the result
                    if (output == "-1")
                    {
                        // Return -1: set PhaseAppear to 0
                        ConditionTextBox.Text = string.Empty;
                    }
                    else if (!string.IsNullOrEmpty(output))
                    {
                        // Base64 return: set PhaseAppear to base64
                        ConditionTextBox.Text = output;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error when launching inz_cond_gui.exe:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
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