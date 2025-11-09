using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Controls;
using Ocelot.Models;
using System.Linq;

namespace Ocelot.Views.Panels
{
    public partial class AppearPanel : UserControl
    {
        private NPCAppear _appear;

        public AppearPanel()
        {
            InitializeComponent();
        }

        public void LoadData(NPCAppear appear)
        {
            _appear = appear;

            if (_appear == null) return;

            LocationXTextBox.Text = _appear.LocationX.ToString();
            LocationYTextBox.Text = _appear.LocationY.ToString();
            LocationZTextBox.Text = _appear.LocationZ.ToString();
            RotationTextBox.Text = _appear.Rotation.ToString();
            StandAnimationTextBox.Text = _appear.StandAnimation;
            TalkAnimationTextBox.Text = _appear.TalkAnimation;
            UnkAnimationTextBox.Text = _appear.UnkAnimation;
            LookAtPlayerCheckBox.IsChecked = _appear.LookAtThePlayer == 2;

            if (_appear.PhaseAppear is string phaseAppearStr)
            {
                PhaseAppearTextBox.Text = phaseAppearStr;
            }
            else
            {
                PhaseAppearTextBox.Text = string.Empty;
            }

            AttachEventHandlers();
        }

        private void AttachEventHandlers()
        {
            LocationXTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null && float.TryParse(LocationXTextBox.Text, out float result))
                    _appear.LocationX = result;
            };

            LocationYTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null && float.TryParse(LocationYTextBox.Text, out float result))
                    _appear.LocationY = result;
            };

            LocationZTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null && float.TryParse(LocationZTextBox.Text, out float result))
                    _appear.LocationZ = result;
            };

            RotationTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null && float.TryParse(RotationTextBox.Text, out float result))
                    _appear.Rotation = result;
            };

            StandAnimationTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null)
                    _appear.StandAnimation = StandAnimationTextBox.Text;
            };

            TalkAnimationTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null)
                    _appear.TalkAnimation = TalkAnimationTextBox.Text;
            };

            UnkAnimationTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null)
                    _appear.UnkAnimation = UnkAnimationTextBox.Text;
            };

            LookAtPlayerCheckBox.Checked += (s, e) =>
            {
                if (_appear != null)
                    _appear.LookAtThePlayer = 2;
            };

            LookAtPlayerCheckBox.Unchecked += (s, e) =>
            {
                if (_appear != null)
                    _appear.LookAtThePlayer = 0;
            };

            PhaseAppearTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null)
                {
                    if (string.IsNullOrEmpty(PhaseAppearTextBox.Text))
                    {
                        _appear.PhaseAppear = 0;
                    }
                    else
                    {
                        _appear.PhaseAppear = PhaseAppearTextBox.Text;
                    }
                }
            };
        }

        private void FloatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;

            var fullText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            e.Handled = !float.TryParse(fullText,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.CurrentCulture,
                out _);
        }

        private void OpenConditionEditor_Click(object sender, RoutedEventArgs e)
        {
            if (_appear == null) return;

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

            // If PhaseAppear is a non-empty string, add it as an argument
            if (_appear.PhaseAppear is string phaseAppearStr && !string.IsNullOrEmpty(phaseAppearStr))
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

                    // Clean up output: ignore warnings
                    string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    string base64Line = lines.LastOrDefault(line => !line.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase));

                    if (base64Line == null)
                        base64Line = string.Empty;

                    // Process the result
                    if (base64Line == "-1")
                    {
                        // Return -1: set PhaseAppear to 0
                        PhaseAppearTextBox.Text = string.Empty;
                    }
                    else if (!string.IsNullOrEmpty(base64Line))
                    {
                        // Base64 return: set PhaseAppear to base64
                        PhaseAppearTextBox.Text = base64Line;
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

    }
}