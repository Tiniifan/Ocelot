using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ocelot.Models;
using EventTrigger = Ocelot.Models.EventTrigger;

namespace Ocelot.Views.Panels
{
    public partial class EventTriggerPanel : UserControl
    {
        private Event _currentEvent;
        private EventTrigger _currentEventTrigger;
        private bool _isInternalChange = false;
        private Border _activeTab;

        public EventTriggerPanel()
        {
            InitializeComponent();
            SetActiveTab(PropertiesTab);
            InitializeControlsVisibility();
        }

        public void LoadData(Event eventData)
        {
            _currentEvent = eventData;
            _currentEventTrigger = eventData?.EventDef?.EventObject as EventTrigger;

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

                    // EventTrigger properties
                    if (_currentEventTrigger != null)
                    {
                        EventTypeTextBox.Text = _currentEventTrigger.EventType ?? "";

                        // EventID
                        if (_currentEventTrigger.EventID.HasValue)
                        {
                            EventIDCheckBox.IsChecked = true;
                            EventIDTextBox.Text = _currentEventTrigger.EventID.Value.ToString();
                        }
                        else
                        {
                            EventIDCheckBox.IsChecked = false;
                            EventIDTextBox.Text = "";
                        }

                        // Stop
                        if (_currentEventTrigger.Stop.HasValue)
                        {
                            StopCheckBox.IsChecked = _currentEventTrigger.Stop.Value;
                        }
                        else
                        {
                            StopCheckBox.IsChecked = false;
                        }

                        // ItemID
                        if (_currentEventTrigger.ItemID.HasValue)
                        {
                            ItemIDCheckBox.IsChecked = true;
                            ItemIDTextBox.Text = _currentEventTrigger.ItemID.Value.ToString();
                        }
                        else
                        {
                            ItemIDCheckBox.IsChecked = false;
                            ItemIDTextBox.Text = "";
                        }

                        // GetBit
                        if (_currentEventTrigger.GetBit.HasValue)
                        {
                            GetBitCheckBox.IsChecked = true;
                            GetBitTextBox.Text = _currentEventTrigger.GetBit.Value.ToString();
                        }
                        else
                        {
                            GetBitCheckBox.IsChecked = false;
                            GetBitTextBox.Text = "";
                        }

                        // EV_T_BIT
                        if (_currentEventTrigger.EV_T_BIT.HasValue)
                        {
                            EVTBitCheckBox.IsChecked = true;
                            EVTBitTextBox.Text = _currentEventTrigger.EV_T_BIT.Value.ToString();
                        }
                        else
                        {
                            EVTBitCheckBox.IsChecked = false;
                            EVTBitTextBox.Text = "";
                        }

                        // EV_F_BIT
                        if (_currentEventTrigger.EV_F_BIT.HasValue)
                        {
                            EVFBitCheckBox.IsChecked = true;
                            EVFBitTextBox.Text = _currentEventTrigger.EV_F_BIT.Value.ToString();
                        }
                        else
                        {
                            EVFBitCheckBox.IsChecked = false;
                            EVFBitTextBox.Text = "";
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

            // EventID
            EventIDTextBox.Visibility = EventIDCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            // ItemID
            ItemIDTextBox.Visibility = ItemIDCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            // GetBit
            GetBitTextBox.Visibility = GetBitCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            // EV_T_BIT
            EVTBitTextBox.Visibility = EVTBitCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            // EV_F_BIT
            EVFBitTextBox.Visibility = EVFBitCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
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
        private void EventIDCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void StopCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void ItemIDCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void GetBitCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void EVTBitCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();
        private void EVFBitCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateControlsVisibility();

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