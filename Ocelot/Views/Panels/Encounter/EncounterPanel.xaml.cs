using Ocelot.Models;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ocelot.Views.Panels
{
    public partial class EncounterPanel : UserControl
    {
        private Border _activeTab;
        private bool _isInternalChange = false;

        public EncounterPanel()
        {
            InitializeComponent();
            SetActiveTab(NPCTab);
        }

        public void LoadData(EncountInfo encounter)
        {
            // NPC
            PlayerID1TextBox.Text = encounter.PlayerID1.ToString("X8");
            LocationXTextBox.Text = encounter.LocationX.ToString();
            LocationYTextBox.Text = encounter.LocationY.ToString();
            LocationZTextBox.Text = encounter.LocationZ.ToString();
            RotationTextBox.Text = encounter.Rotation.ToString();
            ChasePlayerCheckBox.IsChecked = encounter.ChasePlayer == 2;
            UniformNPCTextBox.Text = encounter.Uniform.ToString("X8");
            BootsNPCTextBox.Text = encounter.Boots.ToString("X8");

            // Team
            TeamIDTextBox.Text = encounter.TeamID.ToString("X8");
            PlayerID1TeamTextBox.Text = PlayerID1TextBox.Text; // lié
            PlayerID2TextBox.Text = encounter.PlayerID2.ToString("X8");
            PlayerID3TextBox.Text = encounter.PlayerID3.ToString("X8");
            PlayerID4TextBox.Text = encounter.PlayerID4.ToString("X8");
            PlayerID5TextBox.Text = encounter.PlayerID5.ToString("X8");
            UniformTeamTextBox.Text = UniformNPCTextBox.Text; // lié
            BootsTeamTextBox.Text = BootsNPCTextBox.Text;     // lié

            // Text
            TextIDAskTextBox.Text = encounter.TextIDAsk.ToString("X8");
            TextIDWaitTextBox.Text = encounter.TextIDWait.ToString("X8");
            TextIDLostTextBox.Text = encounter.TextIDLost.ToString("X8");
            TextIDWinTextBox.Text = encounter.TextIDWin.ToString("X8");
            ConditionTextBox.Text = encounter.Condition;
        }

        private void TabClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border selectedTab)
            {
                SetActiveTab(selectedTab);
            }
        }

        private void SetActiveTab(Border selectedTab)
        {
            ResetTabStyles();
            _activeTab = selectedTab;
            selectedTab.Background = (Brush)FindResource("Theme.Accent.Brush");
            selectedTab.Opacity = 1.0;

            NPCContent.Visibility = selectedTab == NPCTab ? Visibility.Visible : Visibility.Collapsed;
            TeamContent.Visibility = selectedTab == TeamTab ? Visibility.Visible : Visibility.Collapsed;
            TextContent.Visibility = selectedTab == TextTab ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ResetTabStyles()
        {
            NPCTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
            NPCTab.Opacity = 0.8;
            TeamTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
            TeamTab.Opacity = 0.8;
            TextTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
            TextTab.Opacity = 0.8;
        }

        private void FloatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var fullText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !Regex.IsMatch(fullText, @"^-?[0-9]*(\.[0-9]*)?$");
        }

        private void HexTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9a-fA-F]+");
        }
    }
}