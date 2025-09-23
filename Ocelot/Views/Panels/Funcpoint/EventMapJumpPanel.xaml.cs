using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ocelot.Models;

namespace Ocelot.Views.Panels
{
    public partial class EventMapJumpPanel : UserControl
    {
        private Event _currentEvent;
        private EventMapJump _currentEventMapJump;
        private bool _isInternalChange = false;
        private Border _activeTab;

        public EventMapJumpPanel()
        {
            InitializeComponent();
            SetActiveTab(PropertiesTab);
            InitializeControlsVisibility();
        }

        public void LoadData(Event eventData)
        {
            _currentEvent = eventData;
            _currentEventMapJump = eventData?.EventDef?.EventObject as EventMapJump;

            _isInternalChange = true;

            if (eventData != null)
            {
                // Properties tab
                PtreeTypeTextBox.Text = eventData.PtreType ?? "";
                EventNameTextBox.Text = eventData.EventName ?? "";

                // Position
                if (eventData.Position != null)
                {
                    PositionXTextBox.Text = eventData.Position.X.ToString();
                    PositionYTextBox.Text = eventData.Position.Y.ToString();
                    PositionZTextBox.Text = eventData.Position.Z.ToString();

                    if (eventData.Position.W.HasValue)
                    {
                        PositionWCheckBox.IsChecked = true;
                        PositionWTextBox.Text = eventData.Position.W.Value.ToString();
                    }
                    else
                    {
                        PositionWCheckBox.IsChecked = false;
                        PositionWTextBox.Text = "";
                    }
                }

                // Area
                LoadAreaData(eventData.Area);

                // Definition tab
                if (eventData.EventDef != null)
                {
                    // TBoxBitCheck
                    if (eventData.EventDef.TBoxBitCheck.HasValue)
                    {
                        TBoxBitCheckCheckBox.IsChecked = true;
                        TBoxBitCheckTextBox.Text = eventData.EventDef.TBoxBitCheck.Value.ToString();
                    }
                    else
                    {
                        TBoxBitCheckCheckBox.IsChecked = false;
                        TBoxBitCheckTextBox.Text = "";
                    }

                    // BtnCheck
                    if (eventData.EventDef.BtnCheck.HasValue)
                    {
                        BtnCheckCheckBox.IsChecked = eventData.EventDef.BtnCheck.Value;
                    }
                    else
                    {
                        BtnCheckCheckBox.IsChecked = false;
                    }

                    // EventMapJump properties
                    if (_currentEventMapJump != null)
                    {
                        // JumpTo properties
                        if (_currentEventMapJump.JumpTo != null)
                        {
                            JumpToMapIDTextBox.Text = _currentEventMapJump.JumpTo.MapID ?? "";

                            // MoveRot
                            if (_currentEventMapJump.JumpTo.MoveRot.HasValue)
                            {
                                MoveRotCheckBox.IsChecked = true;
                                MoveRotTextBox.Text = _currentEventMapJump.JumpTo.MoveRot.Value.ToString();
                            }
                            else
                            {
                                MoveRotCheckBox.IsChecked = false;
                                MoveRotTextBox.Text = "";
                            }

                            // MapMotion (nullable string)
                            if (!string.IsNullOrEmpty(_currentEventMapJump.JumpTo.MapMotion))
                            {
                                MapMotionCheckBox.IsChecked = true;
                                MapMotionTextBox.Text = _currentEventMapJump.JumpTo.MapMotion;
                            }
                            else
                            {
                                MapMotionCheckBox.IsChecked = false;
                                MapMotionTextBox.Text = "";
                            }

                            // FadeFrame
                            if (_currentEventMapJump.JumpTo.FadeFrame.HasValue)
                            {
                                FadeFrameCheckBox.IsChecked = true;
                                FadeFrameTextBox.Text = _currentEventMapJump.JumpTo.FadeFrame.Value.ToString();
                            }
                            else
                            {
                                FadeFrameCheckBox.IsChecked = false;
                                FadeFrameTextBox.Text = "";
                            }

                            // SeName (nullable string)
                            if (!string.IsNullOrEmpty(_currentEventMapJump.JumpTo.SeName))
                            {
                                JumpToSeNameCheckBox.IsChecked = true;
                                JumpToSeNameTextBox.Text = _currentEventMapJump.JumpTo.SeName;
                            }
                            else
                            {
                                JumpToSeNameCheckBox.IsChecked = false;
                                JumpToSeNameTextBox.Text = "";
                            }

                            // SeFrame
                            if (_currentEventMapJump.JumpTo.SeFrame.HasValue)
                            {
                                SeFrameCheckBox.IsChecked = true;
                                SeFrameTextBox.Text = _currentEventMapJump.JumpTo.SeFrame.Value.ToString();
                            }
                            else
                            {
                                SeFrameCheckBox.IsChecked = false;
                                SeFrameTextBox.Text = "";
                            }
                        }

                        // STDPos
                        if (_currentEventMapJump.STDPos != null)
                        {
                            STDPosXTextBox.Text = _currentEventMapJump.STDPos.X.ToString();
                            STDPosYTextBox.Text = _currentEventMapJump.STDPos.Y.ToString();
                            STDPosZTextBox.Text = _currentEventMapJump.STDPos.Z.ToString();

                            if (_currentEventMapJump.STDPos.W.HasValue)
                            {
                                STDPosWCheckBox.IsChecked = true;
                                STDPosWTextBox.Text = _currentEventMapJump.STDPos.W.Value.ToString();
                            }
                            else
                            {
                                STDPosWCheckBox.IsChecked = false;
                                STDPosWTextBox.Text = "";
                            }
                        }
                    }
                }
            }

            _isInternalChange = false;
            UpdateControlsVisibility();
        }

        private void LoadAreaData(IArea area)
        {
            BoxLimiterPanel.Visibility = Visibility.Collapsed;
            CircleLimiterPanel.Visibility = Visibility.Collapsed;

            if (area is BoxLimiter box)
            {
                AreaTypeComboBox.SelectedIndex = 0;
                WidthTextBox.Text = box.Width.ToString();
                HeightTextBox.Text = box.Height.ToString();
                BoxAngleTextBox.Text = box.Angle.ToString();
                BoxLimiterPanel.Visibility = Visibility.Visible;
            }
            else if (area is CircleLimiter circle)
            {
                AreaTypeComboBox.SelectedIndex = 1;
                RadiusTextBox.Text = circle.Radius.ToString();
                CircleAngleTextBox.Text = circle.Angle.ToString();
                CircleLimiterPanel.Visibility = Visibility.Visible;
            }
            else
            {
                AreaTypeComboBox.SelectedIndex = -1;
            }
        }

        private void InitializeControlsVisibility()
        {
            UpdateControlsVisibility();
        }

        private void UpdateControlsVisibility()
        {
            // Position W
            PositionWTextBox.Visibility = PositionWCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            // TBoxBitCheck
            TBoxBitCheckTextBox.Visibility = TBoxBitCheckCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            // JumpTo properties
            MoveRotTextBox.Visibility = MoveRotCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            MapMotionTextBox.Visibility = MapMotionCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            FadeFrameTextBox.Visibility = FadeFrameCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            JumpToSeNameTextBox.Visibility = JumpToSeNameCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            SeFrameTextBox.Visibility = SeFrameCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            // STD Pos W
            STDPosWTextBox.Visibility = STDPosWCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TabClick(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;

            if (border == PropertiesTab)
            {
                SetActiveTab(PropertiesTab);
            }
            else if (border == DefinitionTab)
            {
                SetActiveTab(DefinitionTab);
            }
        }

        private void SetActiveTab(Border selectedTab)
        {
            ResetTabStyles();
            _activeTab = selectedTab;
            selectedTab.Background = (Brush)FindResource("Theme.Accent.Brush");
            selectedTab.Opacity = 1.0;

            if (selectedTab == PropertiesTab)
            {
                ShowPropertiesContent();
            }
            else if (selectedTab == DefinitionTab)
            {
                ShowDefinitionContent();
            }
        }

        private void ResetTabStyles()
        {
            PropertiesTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
            PropertiesTab.Opacity = 0.8;
            DefinitionTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
            DefinitionTab.Opacity = 0.8;
        }

        private void ShowPropertiesContent()
        {
            PropertiesContent.Visibility = Visibility.Visible;
            DefinitionContent.Visibility = Visibility.Collapsed;
        }

        private void ShowDefinitionContent()
        {
            PropertiesContent.Visibility = Visibility.Collapsed;
            DefinitionContent.Visibility = Visibility.Visible;
        }

        private void AreaTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInternalChange || _currentEvent == null) return;

            BoxLimiterPanel.Visibility = Visibility.Collapsed;
            CircleLimiterPanel.Visibility = Visibility.Collapsed;

            var selectedIndex = AreaTypeComboBox.SelectedIndex;

            if (selectedIndex == 0)
            {
                if (!(_currentEvent.Area is BoxLimiter))
                {
                    _currentEvent.Area = new BoxLimiter(null);
                }
                BoxLimiterPanel.Visibility = Visibility.Visible;
            }
            else if (selectedIndex == 1)
            {
                if (!(_currentEvent.Area is CircleLimiter))
                {
                    _currentEvent.Area = new CircleLimiter(null);
                }
                CircleLimiterPanel.Visibility = Visibility.Visible;
            }
            else
            {
                _currentEvent.Area = null;
            }
        }

        // Checkbox change handlers
        private void PositionWCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void TBoxBitCheckCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void BtnCheckCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void MoveRotCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void MapMotionCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void FadeFrameCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void JumpToSeNameCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void SeFrameCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void STDPosWCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();

        private void FloatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var fullText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !Regex.IsMatch(fullText, @"^-?[0-9]*(\.[0-9]*)?$");
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]+");
        }
    }
}