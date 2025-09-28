using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using Ocelot.Models;
using Ocelot.Views.Panels;
using System;
using System.Windows.Shapes;

namespace Ocelot.Views
{
    public partial class OcelotMainContent : UserControl
    {
        private Border _activeTab;
        private bool _isDragging = false;
        private System.Windows.Point _lastMousePosition;

        // Data containers
        public Mapenv MapEnvironment { get; set; }
        public Funcpoint FunctionPoint { get; set; }
        public Healpoint HealPoint { get; set; }
        public List<NPCBase> NPCs { get; set; }
        public List<TBoxConfig> TreasureBoxes { get; set; }
        public List<EncountInfo> Encounters { get; set; }
        public Dictionary<int, List<NPCAppear>> NPCAppearDict { get; set; }
        public Dictionary<int, List<NPCTalkConfig>> NPCTalkDict { get; set; }

        // Reusable panel instances
        private MapEnvironmentPanel _mapEnvironmentPanel;
        private NPCPanel _npcPanel;
        private TreasureBoxPanel _treasureBoxPanel;
        private TalkPanel _talkPanel;
        private FuncpointPanel _funcpointPanel;
        private HealPointPanel _healPointPanel;
        private HealAreaPanel _healAreaPanel;
        private AppearPanel _appearPanel;
        private EncounterPanel _encounterPanel;
        private EventTriggerPanel _eventTriggerPanel;
        private EventSEPanel _eventSEPanel;
        private EventMapJumpPanel _eventMapJumpPanel;

        private double _currentZoom = 2.5;
        private bool _overlaysVisible = true;
        private bool _npcModeShowAll = false;
        private string _currentFolderName;

        private Event _selectedEvent;
        private HealArea _selectedHealArea;
        private NPCBase _selectedNPC;

        public bool ShowFunctionPoints { get; set; } = true;
        public bool ShowHealPoints { get; set; } = true;
        public bool ShowNPCs { get; set; } = true;
        public bool ShowTreasureBoxes { get; set; } = true;
        public bool ShowEncounters { get; set; } = true;

        private Dictionary<int, int> _selectedNPCAppearIndices = new Dictionary<int, int>();

        private NPCJsonData _npcJsonData;
        private Dictionary<string, int> _npcNameCounts = new Dictionary<string, int>();

        public OcelotMainContent()
        {
            InitializeComponent();
            InitializePanels();
            PopulateTreeView();
            SetActiveTab(MinimapTab);

            NPCAppearDict = new Dictionary<int, List<NPCAppear>>();
            NPCTalkDict = new Dictionary<int, List<NPCTalkConfig>>();

            if (ImageScrollViewer != null)
            {
                SetZoom(_currentZoom);
            }
        }

        private void InitializePanels()
        {
            _mapEnvironmentPanel = new MapEnvironmentPanel();
            _npcPanel = new NPCPanel();
            _treasureBoxPanel = new TreasureBoxPanel();
            _talkPanel = new TalkPanel();
            _funcpointPanel = new FuncpointPanel();
            _healPointPanel = new HealPointPanel();
            _healAreaPanel = new HealAreaPanel();
            _appearPanel = new AppearPanel();
            _encounterPanel = new EncounterPanel();
            _eventTriggerPanel = new EventTriggerPanel();
            _eventSEPanel = new EventSEPanel();
            _eventMapJumpPanel = new EventMapJumpPanel();
        }

        public void SetCurrentFolderName(string folderName)
        {
            _currentFolderName = folderName;
        }

        public void SetNPCJsonData(NPCJsonData npcJsonData)
        {
            _npcJsonData = npcJsonData;
        }

        private string GetNPCName(NPCBase npcBase)
        {
            string defaultID = npcBase.ID.ToString("X8");

            if (_npcJsonData?.chara_type == null) return defaultID;

            int npcType = npcBase.Type;
            int modelHead = npcBase.ModelHead;

            Dictionary<string, string> targetDict = null;

            if (npcType == (int)NPCType.Player)
                targetDict = _npcJsonData.chara_type.player;
            else if (npcType == (int)NPCType.NPC)
                targetDict = _npcJsonData.chara_type.npc;
            else if (npcType == (int)NPCType.NPCOther)
                targetDict = _npcJsonData.chara_type.other;

            if (targetDict != null && targetDict.TryGetValue(modelHead.ToString(), out string name))
            {
                // Manage duplicates
                string key = $"{name}_{npcType}";
                if (_npcNameCounts.ContainsKey(key))
                {
                    _npcNameCounts[key]++;
                    return $"{name} ({_npcNameCounts[key]})";
                }
                else
                {
                    _npcNameCounts[key] = 1;
                    return name;
                }
            }

            return defaultID;
        }

        #region Image Management
        public void LoadImage(Bitmap bitmap)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    MinimapImage.Source = bitmapImage;

                    // Adjust the canvas size to the image dimensions
                    EventOverlayCanvas.Width = bitmapImage.PixelWidth;
                    EventOverlayCanvas.Height = bitmapImage.PixelHeight;

                    _currentZoom = 2.5;
                    SetZoom(_currentZoom);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetZoom(double zoom)
        {
            if (MinimapImage.LayoutTransform is ScaleTransform scale)
            {
                scale.ScaleX = zoom;
                scale.ScaleY = zoom;
            }
            else
            {
                MinimapImage.LayoutTransform = new ScaleTransform(zoom, zoom);
            }

            // Apply the same zoom to the overlay canvas
            if (EventOverlayCanvas.LayoutTransform is ScaleTransform canvasScale)
            {
                canvasScale.ScaleX = zoom;
                canvasScale.ScaleY = zoom;
            }
            else
            {
                EventOverlayCanvas.LayoutTransform = new ScaleTransform(zoom, zoom);
            }
        }

        public void LoadMapenv(Mapenv mapenv)
        {
            MapEnvironment = mapenv;

            UpdateBoundingBoxDisplay();

            PopulateTreeView();

            if (_overlaysVisible)
            {
                UpdateOverlays();
            }
        }

        public void LoadFuncpoint(Funcpoint funcpoint)
        {
            FunctionPoint = funcpoint;

            PopulateTreeView();

            if (_overlaysVisible)
            {
                UpdateOverlays();
            }
        }

        public void LoadHealpoint(Healpoint healpoint)
        {
            HealPoint = healpoint;

            PopulateTreeView();

            if (_overlaysVisible)
            {
                UpdateOverlays();
            }
        }

        public void LoadNPCData(List<NPCBase> npcBases, Dictionary<int, List<NPCAppear>> npcAppearDict, Dictionary<int, List<NPCTalkConfig>> npcTalkDict, string folderName)
        {
            NPCs = npcBases;
            NPCAppearDict = npcAppearDict;
            NPCTalkDict = npcTalkDict;
            _currentFolderName = folderName;

            PopulateTreeView();

            if (_overlaysVisible)
            {
                UpdateOverlays();
            }
        }

        private void UpdateBoundingBoxDisplay()
        {
            if (MapEnvironment?.MMModelPos != null)
            {
                var mmPos = MapEnvironment.MMModelPos;
                BoundingBoxText.Text = $"MMModelPos: MinX={mmPos.MinX}, MinY={mmPos.MinY}, MaxX={mmPos.MaxX}, MaxY={mmPos.MaxY}";
            }
            else
            {
                BoundingBoxText.Text = "MMModelPos: Not loaded";
            }
        }

        #endregion

        #region Event Drawing on Minimap

        private void UpdateOverlays()
        {
            ClearEventOverlays();

            if (!_overlaysVisible)
                return;

            // Draw all events in green (if enabled)
            if (ShowFunctionPoints && FunctionPoint?.Events != null)
            {
                foreach (var eventData in FunctionPoint.Events)
                {
                    DrawEventOnMinimap(eventData, _selectedEvent == eventData ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green);
                }
            }

            // Draw all HealAreas in sky blue (if enabled)
            if (ShowHealPoints && HealPoint?.HealAreas != null)
            {
                foreach (var healArea in HealPoint.HealAreas)
                {
                    DrawHealAreaOnMinimap(healArea, _selectedHealArea == healArea ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.SkyBlue);
                }
            }

            // Draw NPCs in dark blue (if enabled)
            if (ShowNPCs && NPCs != null && NPCAppearDict != null)
            {
                DrawNPCsOnMinimap();
            }

            // TODO: Add the TreasureBoxes and Encounters:
            if (ShowTreasureBoxes && TreasureBoxes != null)
            {
                // TODO
            }

            if (ShowEncounters && Encounters != null)
            {
                // TODO
            }
        }

        private void DrawNPCsOnMinimap()
        {
            if (NPCs == null || NPCAppearDict == null || MapEnvironment?.MMModelPos == null)
                return;

            var boundaries = new int[]
            {
                (int)MapEnvironment.MMModelPos.MinX,
                (int)MapEnvironment.MMModelPos.MinY,
                (int)MapEnvironment.MMModelPos.MaxX,
                (int)MapEnvironment.MMModelPos.MaxY
            };

            foreach (var npc in NPCs)
            {
                if (!NPCAppearDict.TryGetValue(npc.ID, out var appears) || appears.Count == 0)
                    continue;

                // If a specific NPC is selected and it is not the correct one, skip
                if (_selectedNPC != null && _selectedNPC.ID != npc.ID && !_npcModeShowAll)
                    continue;

                if (_npcModeShowAll || (_selectedNPC != null && _selectedNPC.ID == npc.ID))
                {
                    // Display all positions of the NPC (or selected NPC)
                    for (int i = 0; i < appears.Count; i++)
                    {
                        var appear = appears[i];

                        // Determine the color: red if it is the current position selectioned, dark blue otherwise
                        var brush = System.Windows.Media.Brushes.DarkBlue;
                        if (_selectedNPC != null && _selectedNPC.ID == npc.ID)
                        {
                            if (_selectedNPCAppearIndices.ContainsKey(npc.ID) && _selectedNPCAppearIndices[npc.ID] == i)
                            {
                                brush = System.Windows.Media.Brushes.Red;
                            }
                        }

                        var point = NPCPointToMiniMapPoint(boundaries, appear.LocationX, appear.LocationY,
                            (int)EventOverlayCanvas.Width, (int)EventOverlayCanvas.Height);
                        DrawPointOnCanvas(point, brush);
                    }
                }
                else
                {
                    // Display only the first position
                    var firstAppear = appears[0];

                    // The first npc position is red if the NPC is selected, dark blue otherwise
                    var brush = (_selectedNPC != null && _selectedNPC.ID == npc.ID) ?
                        System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.DarkBlue;

                    var point = NPCPointToMiniMapPoint(boundaries, firstAppear.LocationX, firstAppear.LocationY,
                        (int)EventOverlayCanvas.Width, (int)EventOverlayCanvas.Height);
                    DrawPointOnCanvas(point, brush);
                }
            }
        }

        private void DrawEventOnMinimap(Event eventData, System.Windows.Media.Brush brush)
        {
            if (eventData?.Position == null || MapEnvironment?.MMModelPos == null)
                return;

            // Convert event coordinates to minimap coordinates
            var boundaries = new int[]
            {
                (int)MapEnvironment.MMModelPos.MinX,
                (int)MapEnvironment.MMModelPos.MinY,
                (int)MapEnvironment.MMModelPos.MaxX,
                (int)MapEnvironment.MMModelPos.MaxY
            };
            var minimapPoint = NPCPointToMiniMapPoint(boundaries, eventData.Position.X, eventData.Position.Y,
                                                     (int)EventOverlayCanvas.Width, (int)EventOverlayCanvas.Height);

            // Determine the type of event and draw the appropriate shape
            if (eventData.Area is BoxLimiter box)
            {
                DrawBoxOnCanvas(minimapPoint, box, boundaries, brush);
            }
            else if (eventData.Area is CircleLimiter circle)
            {
                DrawCircleOnCanvas(minimapPoint, circle, boundaries, brush);
            }
            else
            {
                DrawPointOnCanvas(minimapPoint, brush);
            }
        }

        private void DrawHealAreaOnMinimap(HealArea healArea, System.Windows.Media.Brush brush)
        {
            if (healArea?.Position == null || MapEnvironment?.MMModelPos == null)
                return;

            // Convert heal area coordinates to minimap coordinates
            var boundaries = new int[]
            {
                (int)MapEnvironment.MMModelPos.MinX,
                (int)MapEnvironment.MMModelPos.MinY,
                (int)MapEnvironment.MMModelPos.MaxX,
                (int)MapEnvironment.MMModelPos.MaxY
            };
            var minimapPoint = NPCPointToMiniMapPoint(boundaries, healArea.Position.X, healArea.Position.Y,
                                                     (int)EventOverlayCanvas.Width, (int)EventOverlayCanvas.Height);

            DrawPointOnCanvas(minimapPoint, brush);
        }

        private void DrawBoxOnCanvas(System.Windows.Point center, BoxLimiter box, int[] boundaries, System.Windows.Media.Brush brush)
        {
            // Convert the box dimensions to minimap pixels
            double scaleX = (double)EventOverlayCanvas.Width / (boundaries[2] - boundaries[0]);
            double scaleY = (double)EventOverlayCanvas.Height / (boundaries[3] - boundaries[1]);

            double pixelWidth = box.Width * scaleX;
            double pixelHeight = box.Height * scaleY;

            var rectangle = new System.Windows.Shapes.Rectangle
            {
                Width = pixelWidth,
                Height = pixelHeight,
                Fill = System.Windows.Media.Brushes.Transparent,
                Stroke = brush,
                StrokeThickness = 2,
                Opacity = 0.7
            };

            // Position the rectangle (centered on the point)
            Canvas.SetLeft(rectangle, center.X - pixelWidth / 2);
            Canvas.SetTop(rectangle, center.Y - pixelHeight / 2);

            if (box.Angle != 0)
            {
                rectangle.RenderTransform = new RotateTransform(box.Angle, pixelWidth / 2, pixelHeight / 2);
            }

            EventOverlayCanvas.Children.Add(rectangle);
        }

        private void DrawCircleOnCanvas(System.Windows.Point center, CircleLimiter circle, int[] boundaries, System.Windows.Media.Brush brush)
        {
            // Convert the radius into pixels on the minimap
            double scaleX = (double)EventOverlayCanvas.Width / (boundaries[2] - boundaries[0]);
            double pixelRadius = circle.Radius * scaleX;

            var ellipse = new Ellipse
            {
                Width = pixelRadius * 2,
                Height = pixelRadius * 2,
                Fill = System.Windows.Media.Brushes.Transparent,
                Stroke = brush,
                StrokeThickness = 2,
                Opacity = 0.7
            };

            // Position the circle (centered on the point)
            Canvas.SetLeft(ellipse, center.X - pixelRadius);
            Canvas.SetTop(ellipse, center.Y - pixelRadius);

            EventOverlayCanvas.Children.Add(ellipse);
        }

        private void DrawPointOnCanvas(System.Windows.Point point, System.Windows.Media.Brush brush)
        {
            // Simple point

            var ellipse = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = brush,
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(ellipse, point.X - 3);
            Canvas.SetTop(ellipse, point.Y - 3);

            EventOverlayCanvas.Children.Add(ellipse);
        }

        private static System.Windows.Point NPCPointToMiniMapPoint(int[] boundaries, float pointX, float pointY, int mapWidth, int mapHeight)
        {
            int minX = boundaries[0];
            int minY = boundaries[1];
            int maxX = boundaries[2];
            int maxY = boundaries[3];

            int rangeX = maxX - minX;
            int rangeY = maxY - minY;

            if (rangeX == 0) rangeX = 1;
            if (rangeY == 0) rangeY = 1;

            double scaleX = (double)mapWidth / rangeX;
            double scaleY = (double)mapHeight / rangeY;

            int mapX = (int)((pointX - minX) * scaleX);
            int mapY = (int)((pointY - minY) * scaleY);

            return new System.Windows.Point(mapX, mapY);
        }

        private void ClearEventOverlays()
        {
            if (EventOverlayCanvas != null && EventOverlayCanvas.Children.Count > 0)
                EventOverlayCanvas.Children.Clear();
        }

        private void ClearOverlaysButton_Click(object sender, RoutedEventArgs e)
        {
            _overlaysVisible = !_overlaysVisible;

            var button = sender as Button;
            if (button != null)
            {
                button.Content = _overlaysVisible ? "Hide Overlays" : "Show Overlays";
            }

            UpdateOverlays();
        }

        private void ToggleNPCModeButton_Click(object sender, RoutedEventArgs e)
        {
            _npcModeShowAll = !_npcModeShowAll;

            var button = sender as Button;
            if (button != null)
            {
                button.Content = _npcModeShowAll ? "Show First NPC Only" : "Show All NPC Appears";
            }

            if (_overlaysVisible)
            {
                UpdateOverlays();
            }
        }

        private void FunctionPointsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShowFunctionPoints = true;
            UpdateOverlays();
        }

        private void FunctionPointsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowFunctionPoints = false;
            UpdateOverlays();
        }

        private void HealPointsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShowHealPoints = true;
            UpdateOverlays();
        }

        private void HealPointsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowHealPoints = false;
            UpdateOverlays();
        }

        private void NPCsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShowNPCs = true;
            UpdateOverlays();
        }

        private void NPCsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowNPCs = false;
            UpdateOverlays();
        }

        private void TreasureBoxesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShowTreasureBoxes = true;
            UpdateOverlays();
        }

        private void TreasureBoxesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowTreasureBoxes = false;
            UpdateOverlays();
        }

        private void EncountersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShowEncounters = true;
            UpdateOverlays();
        }

        private void EncountersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowEncounters = false;
            UpdateOverlays();
        }

        #endregion

        #region Image Interaction Events
        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            _currentZoom *= 1.2;
            _currentZoom = Math.Min(5.0, _currentZoom);
            SetZoom(_currentZoom);
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            _currentZoom /= 1.2;
            _currentZoom = Math.Max(0.1, _currentZoom);
            SetZoom(_currentZoom);
        }

        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _lastMousePosition = e.GetPosition(ImageScrollViewer);
            MinimapImage.CaptureMouse();
            MinimapImage.Cursor = Cursors.SizeAll;
        }

        private void Image_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            MinimapImage.ReleaseMouseCapture();
            MinimapImage.Cursor = Cursors.Hand;
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.RightButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(ImageScrollViewer);
                var deltaX = currentPosition.X - _lastMousePosition.X;
                var deltaY = currentPosition.Y - _lastMousePosition.Y;

                ImageScrollViewer.ScrollToHorizontalOffset(ImageScrollViewer.HorizontalOffset - deltaX);
                ImageScrollViewer.ScrollToVerticalOffset(ImageScrollViewer.VerticalOffset - deltaY);

                _lastMousePosition = currentPosition;
            }
        }

        private void MinimapImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            _currentZoom *= zoomFactor;

            _currentZoom = Math.Max(0.1, Math.Min(5.0, _currentZoom));

            SetZoom(_currentZoom);
            e.Handled = true;
        }

        private void EventOverlayCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MinimapImage_MouseWheel(MinimapImage, e);
        }

        private void EventOverlayCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image_MouseRightButtonDown(MinimapImage, e);
        }

        private void EventOverlayCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image_MouseRightButtonUp(MinimapImage, e);
        }

        private void EventOverlayCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Image_MouseMove(MinimapImage, e);
        }
        #endregion

        private void PopulateTreeView()
        {
            MainTreeView.Items.Clear();

            if (MapEnvironment != null)
            {
                var mapenvItem = new TreeViewItem { Header = $"{MapEnvironment.MapID}_mapenv", Tag = "Mapenv" };
                MainTreeView.Items.Add(mapenvItem);
            }

            if (FunctionPoint != null)
            {
                var funcptItem = new TreeViewItem
                {
                    Header = $"{FunctionPoint.MapID}_funcpt",
                    Tag = new { Type = "FuncpointRoot", Data = FunctionPoint }
                };

                var eventNode = new TreeViewItem { Header = "Event", Tag = new { Type = "EventCategory", Data = FunctionPoint } };
                var eventEvents = FunctionPoint.Events?.Where(e => e.EventName.StartsWith("KO") || e.EventName.StartsWith("EV")).ToList() ?? new List<Event>();
                foreach (var evt in eventEvents)
                {
                    var eventItem = new TreeViewItem { Header = evt.EventName, Tag = new { Type = "Event", Data = evt } };
                    eventNode.Items.Add(eventItem);
                }
                funcptItem.Items.Add(eventNode);

                var mapJumpNode = new TreeViewItem { Header = "Map Jump", Tag = new { Type = "MapJumpCategory", Data = FunctionPoint } };
                var mapJumpEvents = FunctionPoint.Events?.Where(e => e.EventName.StartsWith("MJ")).ToList() ?? new List<Event>();
                foreach (var evt in mapJumpEvents)
                {
                    var mapJumpItem = new TreeViewItem { Header = evt.EventName, Tag = new { Type = "MapJump", Data = evt } };
                    mapJumpNode.Items.Add(mapJumpItem);
                }
                funcptItem.Items.Add(mapJumpNode);

                var soundEffectNode = new TreeViewItem { Header = "Sound Effect", Tag = new { Type = "SoundEffectCategory", Data = FunctionPoint } };
                var soundEffectEvents = FunctionPoint.Events?.Where(e => e.EventName.StartsWith("MS")).ToList() ?? new List<Event>();
                foreach (var evt in soundEffectEvents)
                {
                    var soundEffectItem = new TreeViewItem { Header = evt.EventName, Tag = new { Type = "SoundEffect", Data = evt } };
                    soundEffectNode.Items.Add(soundEffectItem);
                }
                funcptItem.Items.Add(soundEffectNode);

                MainTreeView.Items.Add(funcptItem);
            }

            if (HealPoint != null)
            {
                var healptItem = new TreeViewItem
                {
                    Header = $"{HealPoint.MapID}_healpt",
                    Tag = new { Type = "HealpointRoot", Data = HealPoint }
                };

                foreach (var healArea in HealPoint.HealAreas)
                {
                    var healAreaItem = new TreeViewItem
                    {
                        Header = healArea.HealAreaName,
                        Tag = new { Type = "HealArea", Data = healArea }
                    };
                    healptItem.Items.Add(healAreaItem);
                }

                MainTreeView.Items.Add(healptItem);
            }

            if (NPCs?.Count > 0)
            {
                var npcRootItem = new TreeViewItem
                {
                    Header = $"{_currentFolderName}_npc",
                    Tag = new { Type = "NPCRoot", Data = NPCs }
                };

                foreach (var npc in NPCs)
                {
                    var npcItem = new TreeViewItem
                    {
                        Header = GetNPCName(npc),
                        Tag = new { Type = "NPC", Data = npc }
                    };

                    var appearsNode = new TreeViewItem
                    {
                        Header = "Appears",
                        Tag = new { Type = "AppearsCategory", NPC = npc }
                    };

                    if (NPCAppearDict != null && NPCAppearDict.TryGetValue(npc.ID, out var appears))
                    {
                        for (int i = 0; i < appears.Count; i++)
                        {
                            var appearItem = new TreeViewItem
                            {
                                Header = $"Appear_{i}",
                                Tag = new { Type = "NPCAppear", NPC = npc, Appear = appears[i], Index = i }
                            };
                            appearsNode.Items.Add(appearItem);
                        }
                    }
                    npcItem.Items.Add(appearsNode);

                    var talksNode = new TreeViewItem
                    {
                        Header = "Talks",
                        Tag = new { Type = "TalksCategory", NPC = npc }
                    };

                    if (NPCTalkDict != null && NPCTalkDict.TryGetValue(npc.ID, out var talks))
                    {
                        for (int i = 0; i < talks.Count; i++)
                        {
                            var talkItem = new TreeViewItem
                            {
                                Header = $"Talk_{i}",
                                Tag = new { Type = "NPCTalk", NPC = npc, Talk = talks[i], Index = i }
                            };
                            talksNode.Items.Add(talkItem);
                        }
                    }
                    npcItem.Items.Add(talksNode);

                    npcRootItem.Items.Add(npcItem);
                }

                MainTreeView.Items.Add(npcRootItem);
            }

            if (TreasureBoxes?.Count > 0)
            {
                var treasureItem = new TreeViewItem { Header = "Treasure Boxes", Tag = "TreasureBoxes" };
                foreach (var treasure in TreasureBoxes)
                {
                    var treasureNodeItem = new TreeViewItem { Header = treasure.TBoxID.ToString("X8"), Tag = treasure };
                    treasureItem.Items.Add(treasureNodeItem);
                }
                MainTreeView.Items.Add(treasureItem);
            }

            if (Encounters?.Count > 0)
            {
                var encounterItem = new TreeViewItem { Header = "Encounters", Tag = "Encounters" };
                foreach (var encounter in Encounters)
                {
                    var encounterNodeItem = new TreeViewItem { Header = encounter.TeamID.ToString("X8"), Tag = encounter };
                    encounterItem.Items.Add(encounterNodeItem);
                }
                MainTreeView.Items.Add(encounterItem);
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem selectedItem)
            {
                UpdateRightPanel(selectedItem);

                _selectedEvent = null;
                _selectedHealArea = null;
                _selectedNPC = null;

                if (selectedItem.Tag != null && selectedItem.Tag.GetType().GetProperty("Type") != null)
                {
                    dynamic data = selectedItem.Tag;
                    string type = data.Type;

                    if (type == "Event" || type == "MapJump" || type == "SoundEffect")
                    {
                        _selectedEvent = data.Data;
                    }
                    else if (type == "HealArea")
                    {
                        _selectedHealArea = data.Data;
                    }
                    else if (type == "NPC")
                    {
                        _selectedNPC = data.Data;
                        _selectedNPCAppearIndices.Remove(_selectedNPC.ID);
                    }
                    else if (type == "NPCAppear")
                    {
                        _selectedNPC = data.NPC;
                        _selectedNPCAppearIndices[_selectedNPC.ID] = data.Index;
                    }
                    else if (type == "NPCTalk")
                    {
                        if (_selectedNPC == null || _selectedNPC.ID != data.NPC.ID)
                        {
                            _selectedNPC = data.NPC;
                            _selectedNPCAppearIndices.Remove(_selectedNPC.ID);
                        }
                    }
                }

                if (_overlaysVisible)
                {
                    UpdateOverlays();
                }
            }
        }

        private void UpdateRightPanel(TreeViewItem selectedItem)
        {
            var tag = selectedItem.Tag;

            if (tag is string stringTag)
            {
                switch (stringTag)
                {
                    case "Mapenv": ShowMapEnvironmentPanel(); break;
                    default: ShowDefaultPanel(); break;
                }
            }
            else if (tag != null && tag.GetType().GetProperty("Type") != null)
            {
                dynamic data = tag;
                string type = data.Type;

                switch (type)
                {
                    case "FuncpointRoot":
                        ShowFuncpointPanel(data.Data);
                        break;
                    case "HealpointRoot":
                        ShowHealPointPanel(data.Data);
                        break;
                    case "HealArea":
                        ShowHealAreaPanel(data.Data);
                        break;
                    case "Event":
                        ShowEventPanel(data.Data);
                        break;
                    case "MapJump":
                        ShowEventMapJumpPanel(data.Data);
                        break;
                    case "SoundEffect":
                        ShowEventSEPanel(data.Data);
                        break;
                    case "NPCRoot":
                        ShowDefaultPanel();
                        break;
                    case "NPC":
                        ShowNPCPanel(data.Data);
                        break;
                    case "NPCAppear":
                        ShowAppearPanel(data.NPC, data.Appear, data.Index);
                        break;
                    case "NPCTalk":
                        ShowTalkPanel(data.NPC, data.Talk, data.Index);
                        break;
                    case "EventCategory":
                    case "MapJumpCategory":
                    case "SoundEffectCategory":
                    case "AppearsCategory":
                    case "TalksCategory":
                        ShowDefaultPanel();
                        break;
                }
            }
            else if (tag is NPCBase npc) { ShowNPCPanel(npc); }
            else if (tag != null && tag.GetType().GetProperty("Appear") != null)
            {
                dynamic data = tag;
                ShowAppearPanel(data.NPC, data.Appear, data.Index);
            }
            else if (tag != null && tag.GetType().GetProperty("Talk") != null)
            {
                dynamic data = tag;
                ShowTalkPanel(data.NPC, data.Talk, data.Index);
            }
            else if (tag is TBoxConfig treasureBox) { ShowTreasureBoxPanel(treasureBox); }
            else if (tag is EncountInfo encounter) { ShowEncounterPanel(encounter); }
            else { ShowDefaultPanel(); }
        }

        #region Panel Display Methods

        private void ShowMapEnvironmentPanel()
        {
            RightPanelHeader.Text = "Map Environment";
            _mapEnvironmentPanel.LoadData(MapEnvironment);
            RightPanelContent.Content = _mapEnvironmentPanel;
        }

        private void ShowFuncpointPanel(Funcpoint funcpoint)
        {
            RightPanelHeader.Text = $"{funcpoint.MapID}_funcpt";
            _funcpointPanel.LoadData(funcpoint);
            RightPanelContent.Content = _funcpointPanel;
        }

        private void ShowEventPanel(Event eventData)
        {
            RightPanelHeader.Text = $"Event: {eventData.EventName}";
            _eventTriggerPanel.LoadData(eventData);
            RightPanelContent.Content = _eventTriggerPanel;
        }

        private void ShowEventMapJumpPanel(Event eventData)
        {
            RightPanelHeader.Text = $"Map Jump: {eventData.EventName}";
            _eventMapJumpPanel.LoadData(eventData);
            RightPanelContent.Content = _eventMapJumpPanel;
        }

        private void ShowEventSEPanel(Event eventData)
        {
            RightPanelHeader.Text = $"Sound Effect: {eventData.EventName}";
            _eventSEPanel.LoadData(eventData);
            RightPanelContent.Content = _eventSEPanel;
        }

        private void ShowNPCPanel(NPCBase npc)
        {
            RightPanelHeader.Text = $"NPC: {npc.ID:X8}";
            _npcPanel.LoadData(npc);
            RightPanelContent.Content = _npcPanel;
        }

        private void ShowAppearPanel(NPCBase npc, NPCAppear appear, int index)
        {
            RightPanelHeader.Text = $"Appear {index} for NPC: {npc.ID:X8}";
            _appearPanel.LoadData(appear);
            RightPanelContent.Content = _appearPanel;
        }

        private void ShowTalkPanel(NPCBase npc, NPCTalkConfig talk, int index)
        {
            RightPanelHeader.Text = $"Talk {index} for NPC: {npc.ID:X8}";
            _talkPanel.LoadData(talk);
            RightPanelContent.Content = _talkPanel;
        }

        private void ShowTreasureBoxPanel(TBoxConfig treasureBox)
        {
            RightPanelHeader.Text = $"Treasure Box: {treasureBox.TBoxID:X8}";
            _treasureBoxPanel.LoadData(treasureBox);
            RightPanelContent.Content = _treasureBoxPanel;
        }

        private void ShowHealPointPanel(Healpoint healpoint)
        {
            RightPanelHeader.Text = $"{healpoint.MapID}_healpt";
            _healPointPanel.LoadData(healpoint);
            RightPanelContent.Content = _healPointPanel;
        }

        private void ShowHealAreaPanel(HealArea healArea)
        {
            RightPanelHeader.Text = $"Heal Area: {healArea.HealAreaName}";
            _healAreaPanel.LoadData(healArea);
            RightPanelContent.Content = _healAreaPanel;
        }

        private void ShowEncounterPanel(EncountInfo encounter)
        {
            RightPanelHeader.Text = $"Encounter: {encounter.TeamID:X8}";
            _encounterPanel.LoadData(encounter);
            RightPanelContent.Content = _encounterPanel;
        }

        private void ShowDefaultPanel()
        {
            RightPanelHeader.Text = "Properties";
            var defaultText = new TextBlock
            {
                Text = "Select an item from the tree",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (System.Windows.Media.Brush)FindResource("Theme.Text.SecondaryBrush"),
                FontStyle = FontStyles.Italic,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(20)
            };
            RightPanelContent.Content = defaultText;
        }

        #endregion

        #region Tab Management
        private void TabClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border tab)
            {
                SetActiveTab(tab);
            }
        }

        private void SetActiveTab(Border selectedTab)
        {
            ResetTabStyles();

            _activeTab = selectedTab;

            selectedTab.Background = (System.Windows.Media.Brush)FindResource("Theme.Accent.Brush");
            selectedTab.Opacity = 1.0;

            if (selectedTab == MinimapTab)
            {
                ShowMinimapContent();
            }
            else if (selectedTab == View3DTab)
            {
                Show3DContent();
            }
        }

        private void ResetTabStyles()
        {
            MinimapTab.Background = (System.Windows.Media.Brush)FindResource("Theme.Control.BackgroundBrush");
            MinimapTab.Opacity = 0.8;

            View3DTab.Background = (System.Windows.Media.Brush)FindResource("Theme.Control.BackgroundBrush");
            View3DTab.Opacity = 0.8;
        }

        private void ShowMinimapContent()
        {
            MinimapContent.Visibility = Visibility.Visible;
            View3DContent.Visibility = Visibility.Collapsed;
        }

        private void Show3DContent()
        {
            MinimapContent.Visibility = Visibility.Collapsed;
            View3DContent.Visibility = Visibility.Visible;
        }
        #endregion

        #region Public Properties and Events
        public TreeView TreeView => MainTreeView;
        public TextBox SearchBox => SearchTextBox;
        public Grid MinimapContainer => MinimapContent;
        public Grid View3DContainer => View3DContent;

        public MMModelPos CurrentMMModelPos => MapEnvironment?.MMModelPos;

        public event RoutedEventHandler SearchTextChanged;
        public event RoutedEventHandler TreeViewSelectionChanged;

        private void OnSearchTextChanged()
        {
            SearchTextChanged?.Invoke(this, new RoutedEventArgs());
        }

        private void OnTreeViewSelectionChanged()
        {
            TreeViewSelectionChanged?.Invoke(this, new RoutedEventArgs());
        }
        #endregion
    }
}