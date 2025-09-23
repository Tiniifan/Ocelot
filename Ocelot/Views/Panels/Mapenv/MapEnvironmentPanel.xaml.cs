using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ocelot.Models;

namespace Ocelot.Views.Panels
{
    public partial class MapEnvironmentPanel : UserControl
    {
        private Border _activeTab;

        private Mapenv _mapEnvironment;
        private MapenvShadowCol _selectedShadowCol;

        public MapEnvironmentPanel()
        {
            InitializeComponent();
            SetActiveTab(PropertyTab);
        }

        public void LoadData(Mapenv mapEnvironment)
        {
            _mapEnvironment = mapEnvironment;

            if (_mapEnvironment == null) return;

            // Load basic information
            MapenvNameTextBox.Text = _mapEnvironment.MapenvName ?? string.Empty;
            MapIDTextBox.Text = _mapEnvironment.MapID ?? string.Empty;

            // Load optional properties with checkboxes
            LoadOptionalProperty(_mapEnvironment.Default, DefaultCheckBox, DefaultTextBox);
            LoadOptionalProperty(_mapEnvironment.ParentID, ParentIDCheckBox, ParentIDTextBox);
            LoadOptionalProperty(_mapEnvironment.GroupID, GroupIDCheckBox, GroupIDTextBox);
            LoadOptionalProperty(_mapEnvironment.BtlMapID, BtlMapIDCheckBox, BtlMapIDTextBox);
            LoadOptionalProperty(_mapEnvironment.MapEffect, MapEffectCheckBox, MapEffectTextBox);
            LoadOptionalProperty(_mapEnvironment.MMScroll, MMScrollCheckBox, MMScrollTextBox);
            LoadOptionalProperty(_mapEnvironment.AntiMode, AntiModeCheckBox, AntiModeTextBox);
            LoadOptionalProperty(_mapEnvironment.SndBGM, SndBGMCheckBox, SndBGMTextBox);
            LoadOptionalProperty(_mapEnvironment.SndEnv, SndEnvCheckBox, SndEnvTextBox);

            // Load MM Model Position
            if (_mapEnvironment.MMModelPos != null)
            {
                MMModelPosCheckBox.IsChecked = true;
                MMModelPosGroupBox.Visibility = Visibility.Visible;
                MinXTextBox.Text = _mapEnvironment.MMModelPos.MinX.ToString(CultureInfo.InvariantCulture);
                MinYTextBox.Text = _mapEnvironment.MMModelPos.MinY.ToString(CultureInfo.InvariantCulture);
                MaxXTextBox.Text = _mapEnvironment.MMModelPos.MaxX.ToString(CultureInfo.InvariantCulture);
                MaxYTextBox.Text = _mapEnvironment.MMModelPos.MaxY.ToString(CultureInfo.InvariantCulture);
            }

            // Load Camera
            if (_mapEnvironment.Camera != null)
            {
                LoadOptionalProperty(_mapEnvironment.Camera.Fov, FovCheckBox, FovTextBox);
                LoadOptionalProperty(_mapEnvironment.Camera.Dist, DistCheckBox, DistTextBox);
                LoadOptionalProperty(_mapEnvironment.Camera.RotH, RotHCheckBox, RotHTextBox);
                LoadOptionalProperty(_mapEnvironment.Camera.RotV, RotVCheckBox, RotVTextBox);
                LoadOptionalProperty(_mapEnvironment.Camera.DptRngRate, DptRngRateCheckBox, DptRngRateTextBox);
                LoadOptionalProperty(_mapEnvironment.Camera.DptLvl, DptLvlCheckBox, DptLvlTextBox);
            }

            // Load Light
            if (_mapEnvironment.Light != null)
            {
                LoadOptionalProperty(_mapEnvironment.Light.MapLight, MapLightCheckBox, MapLightTextBox);

                if (_mapEnvironment.Light.BackgroundColor != null)
                {
                    BackgroundColorCheckBox.IsChecked = true;
                    BackgroundColorGroupBox.Visibility = Visibility.Visible;
                    BgColorRTextBox.Text = _mapEnvironment.Light.BackgroundColor.R.ToString(CultureInfo.InvariantCulture);
                    BgColorGTextBox.Text = _mapEnvironment.Light.BackgroundColor.G.ToString(CultureInfo.InvariantCulture);
                    BgColorBTextBox.Text = _mapEnvironment.Light.BackgroundColor.B.ToString(CultureInfo.InvariantCulture);
                }

                if (_mapEnvironment.Light.LightCollision != null)
                {
                    LightCollisionCheckBox.IsChecked = true;
                    LightCollisionGroupBox.Visibility = Visibility.Visible;
                    LightCollisionXTextBox.Text = _mapEnvironment.Light.LightCollision.X.ToString(CultureInfo.InvariantCulture);
                    LightCollisionYTextBox.Text = _mapEnvironment.Light.LightCollision.Y.ToString(CultureInfo.InvariantCulture);
                    LightCollisionZTextBox.Text = _mapEnvironment.Light.LightCollision.Z.ToString(CultureInfo.InvariantCulture);
                }

                if (_mapEnvironment.Light.ShadowCols != null && _mapEnvironment.Light.ShadowCols.Count > 0)
                {
                    ShadowColsCheckBox.IsChecked = true;
                    ShadowColsGroupBox.Visibility = Visibility.Visible;
                    LoadShadowColors();
                }
            }

            AttachEventHandlers();
        }

        private void LoadOptionalProperty<T>(T value, CheckBox checkBox, TextBox textBox)
        {
            if (value != null)
            {
                checkBox.IsChecked = true;
                textBox.Visibility = Visibility.Visible;
                textBox.Text = value.ToString();
            }
        }

        private void AttachEventHandlers()
        {
            // Basic information handlers
            MapenvNameTextBox.TextChanged += (s, e) =>
            {
                if (_mapEnvironment != null)
                    _mapEnvironment.MapenvName = MapenvNameTextBox.Text;
            };

            MapIDTextBox.TextChanged += (s, e) =>
            {
                if (_mapEnvironment != null)
                    _mapEnvironment.MapID = MapIDTextBox.Text;
            };

            DefaultTextBox.TextChanged += (s, e) =>
            {
                if (_mapEnvironment != null)
                    _mapEnvironment.Default = DefaultTextBox.Text;
            };

            ParentIDTextBox.TextChanged += (s, e) =>
            {
                if (_mapEnvironment != null)
                    _mapEnvironment.ParentID = ParentIDTextBox.Text;
            };

            GroupIDTextBox.TextChanged += (s, e) =>
            {
                if (_mapEnvironment != null && int.TryParse(GroupIDTextBox.Text, out int result))
                    _mapEnvironment.GroupID = result;
            };

            BtlMapIDTextBox.TextChanged += (s, e) =>
            {
                if (_mapEnvironment != null)
                    _mapEnvironment.BtlMapID = BtlMapIDTextBox.Text;
            };

            MapEffectTextBox.TextChanged += (s, e) =>
            {
                if (_mapEnvironment != null)
                    _mapEnvironment.MapEffect = MapEffectTextBox.Text;
            };

            MMScrollTextBox.TextChanged += (s, e) =>
            {
                if (_mapEnvironment != null && int.TryParse(MMScrollTextBox.Text, out int result))
                    _mapEnvironment.MMScroll = result;
            };

            AntiModeTextBox.TextChanged += (s, e) =>
            {
                if (_mapEnvironment != null && int.TryParse(AntiModeTextBox.Text, out int result))
                    _mapEnvironment.AntiMode = result;
            };

            SndBGMTextBox.TextChanged += (s, e) =>
            {
                if (_mapEnvironment != null)
                    _mapEnvironment.SndBGM = SndBGMTextBox.Text;
            };

            SndEnvTextBox.TextChanged += (s, e) =>
            {
                if (_mapEnvironment != null)
                    _mapEnvironment.SndEnv = SndEnvTextBox.Text;
            };

            // MM Model Position handlers
            MinXTextBox.TextChanged += (s, e) => UpdateMMModelPosition();
            MinYTextBox.TextChanged += (s, e) => UpdateMMModelPosition();
            MaxXTextBox.TextChanged += (s, e) => UpdateMMModelPosition();
            MaxYTextBox.TextChanged += (s, e) => UpdateMMModelPosition();

            // Camera handlers
            FovTextBox.TextChanged += (s, e) => UpdateCameraProperty("Fov", FovTextBox.Text);
            DistTextBox.TextChanged += (s, e) => UpdateCameraProperty("Dist", DistTextBox.Text);
            RotHTextBox.TextChanged += (s, e) => UpdateCameraProperty("RotH", RotHTextBox.Text);
            RotVTextBox.TextChanged += (s, e) => UpdateCameraProperty("RotV", RotVTextBox.Text);
            DptRngRateTextBox.TextChanged += (s, e) => UpdateCameraProperty("DptRngRate", DptRngRateTextBox.Text);
            DptLvlTextBox.TextChanged += (s, e) => UpdateCameraProperty("DptLvl", DptLvlTextBox.Text);

            // Light handlers
            MapLightTextBox.TextChanged += (s, e) => UpdateLightProperty();

            BgColorRTextBox.TextChanged += (s, e) => UpdateBackgroundColor();
            BgColorGTextBox.TextChanged += (s, e) => UpdateBackgroundColor();
            BgColorBTextBox.TextChanged += (s, e) => UpdateBackgroundColor();

            LightCollisionXTextBox.TextChanged += (s, e) => UpdateLightCollision();
            LightCollisionYTextBox.TextChanged += (s, e) => UpdateLightCollision();
            LightCollisionZTextBox.TextChanged += (s, e) => UpdateLightCollision();

            // Shadow color handlers
            ShadowIDTextBox.TextChanged += (s, e) =>
            {
                if (_selectedShadowCol != null)
                    _selectedShadowCol.ID = ShadowIDTextBox.Text;
            };

            ShadowXTextBox.TextChanged += (s, e) => UpdateShadowCollision();
            ShadowYTextBox.TextChanged += (s, e) => UpdateShadowCollision();
            ShadowZTextBox.TextChanged += (s, e) => UpdateShadowCollision();
        }

        private void UpdateMMModelPosition()
        {
            if (_mapEnvironment?.MMModelPos != null)
            {
                if (float.TryParse(MinXTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float minX))
                    _mapEnvironment.MMModelPos.MinX = minX;
                if (float.TryParse(MinYTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float minY))
                    _mapEnvironment.MMModelPos.MinY = minY;
                if (float.TryParse(MaxXTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float maxX))
                    _mapEnvironment.MMModelPos.MaxX = maxX;
                if (float.TryParse(MaxYTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float maxY))
                    _mapEnvironment.MMModelPos.MaxY = maxY;
            }
        }

        private void UpdateCameraProperty(string propertyName, string value)
        {
            if (_mapEnvironment?.Camera != null && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                switch (propertyName)
                {
                    case "Fov": _mapEnvironment.Camera.Fov = result; break;
                    case "Dist": _mapEnvironment.Camera.Dist = result; break;
                    case "RotH": _mapEnvironment.Camera.RotH = result; break;
                    case "RotV": _mapEnvironment.Camera.RotV = result; break;
                    case "DptRngRate": _mapEnvironment.Camera.DptRngRate = result; break;
                    case "DptLvl": _mapEnvironment.Camera.DptLvl = result; break;
                }
            }
        }

        private void UpdateLightProperty()
        {
            if (_mapEnvironment?.Light != null)
                _mapEnvironment.Light.MapLight = MapLightTextBox.Text;
        }

        private void UpdateBackgroundColor()
        {
            if (_mapEnvironment?.Light?.BackgroundColor != null)
            {
                if (float.TryParse(BgColorRTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float r))
                    _mapEnvironment.Light.BackgroundColor.R = r;
                if (float.TryParse(BgColorGTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float g))
                    _mapEnvironment.Light.BackgroundColor.G = g;
                if (float.TryParse(BgColorBTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float b))
                    _mapEnvironment.Light.BackgroundColor.B = b;
            }
        }

        private void UpdateLightCollision()
        {
            if (_mapEnvironment?.Light?.LightCollision != null)
            {
                if (float.TryParse(LightCollisionXTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                    _mapEnvironment.Light.LightCollision.X = x;
                if (float.TryParse(LightCollisionYTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                    _mapEnvironment.Light.LightCollision.Y = y;
                if (float.TryParse(LightCollisionZTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                    _mapEnvironment.Light.LightCollision.Z = z;
            }
        }

        private void UpdateShadowCollision()
        {
            if (_selectedShadowCol?.Collision != null)
            {
                if (float.TryParse(ShadowXTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                    _selectedShadowCol.Collision.X = x;
                if (float.TryParse(ShadowYTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                    _selectedShadowCol.Collision.Y = y;
                if (float.TryParse(ShadowZTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                    _selectedShadowCol.Collision.Z = z;
            }
        }

        private void LoadShadowColors()
        {
            ShadowColsTreeView.Items.Clear();

            if (_mapEnvironment?.Light?.ShadowCols != null)
            {
                foreach (var shadowCol in _mapEnvironment.Light.ShadowCols)
                {
                    var item = new TreeViewItem { Header = shadowCol.ID, Tag = shadowCol };
                    ShadowColsTreeView.Items.Add(item);
                }
            }
        }

        #region Tab Management

        private void TabClick(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == PropertyTab)
            {
                SetActiveTab(PropertyTab);
            }
            else if (border == CameraTab)
            {
                SetActiveTab(CameraTab);
            }
            else if (border == LightTab)
            {
                SetActiveTab(LightTab);
            }
        }

        private void SetActiveTab(Border selectedTab)
        {
            ResetTabStyles();
            _activeTab = selectedTab;
            selectedTab.Background = (Brush)FindResource("Theme.Accent.Brush");
            selectedTab.Opacity = 1.0;

            if (selectedTab == PropertyTab)
            {
                ShowPropertyContent();
            }
            else if (selectedTab == CameraTab)
            {
                ShowCameraContent();
            }
            else if (selectedTab == LightTab)
            {
                ShowLightContent();
            }
        }

        private void ResetTabStyles()
        {
            PropertyTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
            PropertyTab.Opacity = 0.8;
            CameraTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
            CameraTab.Opacity = 0.8;
            LightTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
            LightTab.Opacity = 0.8;
        }

        private void ShowPropertyContent()
        {
            PropertyContent.Visibility = Visibility.Visible;
            CameraContent.Visibility = Visibility.Collapsed;
            LightContent.Visibility = Visibility.Collapsed;
        }

        private void ShowCameraContent()
        {
            PropertyContent.Visibility = Visibility.Collapsed;
            CameraContent.Visibility = Visibility.Visible;
            LightContent.Visibility = Visibility.Collapsed;
        }

        private void ShowLightContent()
        {
            PropertyContent.Visibility = Visibility.Collapsed;
            CameraContent.Visibility = Visibility.Collapsed;
            LightContent.Visibility = Visibility.Visible;
        }

        #endregion

        #region Checkbox Event Handlers

        private void DefaultCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = DefaultCheckBox.IsChecked == true;
            DefaultTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment != null)
            {
                if (!isChecked)
                    _mapEnvironment.Default = null;
                else if (string.IsNullOrEmpty(_mapEnvironment.Default))
                    _mapEnvironment.Default = string.Empty;
            }
        }

        private void ParentIDCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = ParentIDCheckBox.IsChecked == true;
            ParentIDTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment != null)
            {
                if (!isChecked)
                    _mapEnvironment.ParentID = null;
                else if (string.IsNullOrEmpty(_mapEnvironment.ParentID))
                    _mapEnvironment.ParentID = string.Empty;
            }
        }

        private void GroupIDCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = GroupIDCheckBox.IsChecked == true;
            GroupIDTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment != null)
            {
                if (!isChecked)
                    _mapEnvironment.GroupID = null;
                else if (_mapEnvironment.GroupID == null)
                    _mapEnvironment.GroupID = 0;
            }
        }

        private void BtlMapIDCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = BtlMapIDCheckBox.IsChecked == true;
            BtlMapIDTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment != null)
            {
                if (!isChecked)
                    _mapEnvironment.BtlMapID = null;
                else if (string.IsNullOrEmpty(_mapEnvironment.BtlMapID))
                    _mapEnvironment.BtlMapID = string.Empty;
            }
        }

        private void MapEffectCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = MapEffectCheckBox.IsChecked == true;
            MapEffectTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment != null)
            {
                if (!isChecked)
                    _mapEnvironment.MapEffect = null;
                else if (string.IsNullOrEmpty(_mapEnvironment.MapEffect))
                    _mapEnvironment.MapEffect = string.Empty;
            }
        }

        private void MMScrollCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = MMScrollCheckBox.IsChecked == true;
            MMScrollTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment != null)
            {
                if (!isChecked)
                    _mapEnvironment.MMScroll = null;
                else if (_mapEnvironment.MMScroll == null)
                    _mapEnvironment.MMScroll = 0;
            }
        }

        private void AntiModeCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = AntiModeCheckBox.IsChecked == true;
            AntiModeTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment != null)
            {
                if (!isChecked)
                    _mapEnvironment.AntiMode = null;
                else if (_mapEnvironment.AntiMode == null)
                    _mapEnvironment.AntiMode = 0;
            }
        }

        private void SndBGMCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = SndBGMCheckBox.IsChecked == true;
            SndBGMTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment != null)
            {
                if (!isChecked)
                    _mapEnvironment.SndBGM = null;
                else if (string.IsNullOrEmpty(_mapEnvironment.SndBGM))
                    _mapEnvironment.SndBGM = string.Empty;
            }
        }

        private void SndEnvCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = SndEnvCheckBox.IsChecked == true;
            SndEnvTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment != null)
            {
                if (!isChecked)
                    _mapEnvironment.SndEnv = null;
                else if (string.IsNullOrEmpty(_mapEnvironment.SndEnv))
                    _mapEnvironment.SndEnv = string.Empty;
            }
        }

        private void MMModelPosCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = MMModelPosCheckBox.IsChecked == true;
            MMModelPosGroupBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment != null)
            {
                if (!isChecked)
                    _mapEnvironment.MMModelPos = null;
                else if (_mapEnvironment.MMModelPos == null)
                    _mapEnvironment.MMModelPos = new MMModelPos(null);
            }
        }

        private void FovCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = FovCheckBox.IsChecked == true;
            FovTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment?.Camera != null)
            {
                if (!isChecked)
                    _mapEnvironment.Camera.Fov = null;
                else if (_mapEnvironment.Camera.Fov == null)
                    _mapEnvironment.Camera.Fov = 0.0f;
            }
        }

        private void DistCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = DistCheckBox.IsChecked == true;
            DistTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment?.Camera != null)
            {
                if (!isChecked)
                    _mapEnvironment.Camera.Dist = null;
                else if (_mapEnvironment.Camera.Dist == null)
                    _mapEnvironment.Camera.Dist = 0.0f;
            }
        }

        private void RotHCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = RotHCheckBox.IsChecked == true;
            RotHTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment?.Camera != null)
            {
                if (!isChecked)
                    _mapEnvironment.Camera.RotH = null;
                else if (_mapEnvironment.Camera.RotH == null)
                    _mapEnvironment.Camera.RotH = 0.0f;
            }
        }

        private void RotVCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = RotVCheckBox.IsChecked == true;
            RotVTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment?.Camera != null)
            {
                if (!isChecked)
                    _mapEnvironment.Camera.RotV = null;
                else if (_mapEnvironment.Camera.RotV == null)
                    _mapEnvironment.Camera.RotV = 0.0f;
            }
        }

        private void DptRngRateCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = DptRngRateCheckBox.IsChecked == true;
            DptRngRateTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment?.Camera != null)
            {
                if (!isChecked)
                    _mapEnvironment.Camera.DptRngRate = null;
                else if (_mapEnvironment.Camera.DptRngRate == null)
                    _mapEnvironment.Camera.DptRngRate = 0.0f;
            }
        }

        private void DptLvlCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = DptLvlCheckBox.IsChecked == true;
            DptLvlTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment?.Camera != null)
            {
                if (!isChecked)
                    _mapEnvironment.Camera.DptLvl = null;
                else if (_mapEnvironment.Camera.DptLvl == null)
                    _mapEnvironment.Camera.DptLvl = 0.0f;
            }
        }

        private void MapLightCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = MapLightCheckBox.IsChecked == true;
            MapLightTextBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment?.Light != null)
            {
                if (!isChecked)
                    _mapEnvironment.Light.MapLight = null;
                else if (string.IsNullOrEmpty(_mapEnvironment.Light.MapLight))
                    _mapEnvironment.Light.MapLight = string.Empty;
            }
        }

        private void BackgroundColorCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = BackgroundColorCheckBox.IsChecked == true;
            BackgroundColorGroupBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment?.Light != null)
            {
                if (!isChecked)
                    _mapEnvironment.Light.BackgroundColor = null;
                else if (_mapEnvironment.Light.BackgroundColor == null)
                    _mapEnvironment.Light.BackgroundColor = new MapenvRGB(null, "BGColor");
            }
        }

        private void LightCollisionCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = LightCollisionCheckBox.IsChecked == true;
            LightCollisionGroupBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment?.Light != null)
            {
                if (!isChecked)
                    _mapEnvironment.Light.LightCollision = null;
                else if (_mapEnvironment.Light.LightCollision == null)
                    _mapEnvironment.Light.LightCollision = new MapenvXYZ(null, "LightCol");
            }
        }

        private void ShadowColsCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var isChecked = ShadowColsCheckBox.IsChecked == true;
            ShadowColsGroupBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;

            if (_mapEnvironment?.Light != null)
            {
                if (!isChecked)
                {
                    _mapEnvironment.Light.ShadowCols = null;
                    ShadowColsTreeView.Items.Clear();
                }
                else if (_mapEnvironment.Light.ShadowCols == null)
                {
                    _mapEnvironment.Light.ShadowCols = new System.Collections.Generic.List<MapenvShadowCol>();
                }
            }
        }

        #endregion

        #region Shadow Colors Management

        private void ShadowColsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Tag is MapenvShadowCol shadowCol)
            {
                _selectedShadowCol = shadowCol;
                ShadowEditPanel.Visibility = Visibility.Visible;

                ShadowIDTextBox.Text = shadowCol.ID ?? string.Empty;

                if (shadowCol.Collision != null)
                {
                    ShadowXTextBox.Text = shadowCol.Collision.X.ToString(CultureInfo.InvariantCulture);
                    ShadowYTextBox.Text = shadowCol.Collision.Y.ToString(CultureInfo.InvariantCulture);
                    ShadowZTextBox.Text = shadowCol.Collision.Z.ToString(CultureInfo.InvariantCulture);
                }
            }
            else
            {
                _selectedShadowCol = null;
                ShadowEditPanel.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Input Validation

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9\-]+$");
        }

        private void FloatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            e.Handled = !Regex.IsMatch(e.Text, $@"^[0-9{Regex.Escape(decimalSeparator)}\-]+$");
        }

        #endregion
    }
}