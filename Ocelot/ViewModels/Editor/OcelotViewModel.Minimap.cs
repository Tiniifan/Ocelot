using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using Ocelot.Models;
using StudioElevenGUI.ViewModels;
using Brushes = System.Windows.Media.Brushes;

namespace Ocelot.ViewModels
{
    public partial class OcelotViewModel : BaseViewModel
    {
        private BitmapSource _minimapSource;
        public BitmapSource MinimapSource
        {
            get => _minimapSource;
            set => SetProperty(ref _minimapSource, value);
        }

        private double _overlayCanvasWidth;
        public double OverlayCanvasWidth
        {
            get => _overlayCanvasWidth;
            set => SetProperty(ref _overlayCanvasWidth, value);
        }

        private double _overlayCanvasHeight;
        public double OverlayCanvasHeight
        {
            get => _overlayCanvasHeight;
            set => SetProperty(ref _overlayCanvasHeight, value);
        }

        private double _currentZoom = 1.0;
        public double CurrentZoom
        {
            get => _currentZoom;
            set => SetProperty(ref _currentZoom, value);
        }

        private bool _overlaysVisible = true;
        public bool OverlaysVisible
        {
            get => _overlaysVisible;
            set
            {
                if (SetProperty(ref _overlaysVisible, value))
                {
                    UpdateOverlays();
                    OverlaysVisibilityText = value ? "Hide Overlays" : "Show Overlays";
                }
            }
        }

        private string _overlaysVisibilityText = "Hide Overlays";
        public string OverlaysVisibilityText
        {
            get => _overlaysVisibilityText;
            set => SetProperty(ref _overlaysVisibilityText, value);
        }

        private bool _npcModeShowAll = false;
        public bool NpcModeShowAll
        {
            get => _npcModeShowAll;
            set
            {
                if (SetProperty(ref _npcModeShowAll, value))
                {
                    if (OverlaysVisible) UpdateOverlays();
                    NpcModeText = value ? "Show First NPC Only" : "Show All NPC Appears";
                }
            }
        }

        private string _npcModeText = "Show All NPC Appears";
        public string NpcModeText
        {
            get => _npcModeText;
            set => SetProperty(ref _npcModeText, value);
        }

        private bool _showFunctionPoints;
        public bool ShowFunctionPoints
        {
            get => _showFunctionPoints;
            set
            {
                if (SetProperty(ref _showFunctionPoints, value)) UpdateOverlays();
            }
        }

        private bool _showHealPoints;
        public bool ShowHealPoints
        {
            get => _showHealPoints;
            set
            {
                if (SetProperty(ref _showHealPoints, value)) UpdateOverlays();
            }
        }

        private bool _showNPCs;
        public bool ShowNPCs
        {
            get => _showNPCs;
            set
            {
                if (SetProperty(ref _showNPCs, value)) UpdateOverlays();
            }
        }

        private bool _showTreasureBoxes;
        public bool ShowTreasureBoxes
        {
            get => _showTreasureBoxes;
            set
            {
                if (SetProperty(ref _showTreasureBoxes, value)) UpdateOverlays();
            }
        }

        private bool _showEncounters;
        public bool ShowEncounters
        {
            get => _showEncounters;
            set
            {
                if (SetProperty(ref _showEncounters, value)) UpdateOverlays();
            }
        }

        // Commandes pour les actions de l'utilisateur
        public ICommand ZoomInCommand { get; private set; }
        public ICommand ZoomOutCommand { get; private set; }
        public ICommand ToggleOverlaysVisibilityCommand { get; private set; }
        public ICommand ToggleNpcModeCommand { get; private set; }

        // Événement pour demander à la vue de dessiner les overlays
        public event EventHandler<OverlayUpdateEventArgs> RequestOverlayUpdate;
        public event EventHandler<double> RequestSetZoom; // Pour la vue afin d'appliquer le zoom

        public void InitializeMinimapCommand()
        {
            ZoomInCommand = new RelayCommand(param => ZoomIn());
            ZoomOutCommand = new RelayCommand(param => ZoomOut());
            ToggleOverlaysVisibilityCommand = new RelayCommand(param => OverlaysVisible = !OverlaysVisible);
            ToggleNpcModeCommand = new RelayCommand(param => NpcModeShowAll = !NpcModeShowAll);
        }

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

                    MinimapSource = bitmapImage; // Met à jour la source d'image pour la vue

                    OverlayCanvasWidth = bitmapImage.PixelWidth;
                    OverlayCanvasHeight = bitmapImage.PixelHeight;

                    CurrentZoom = 2.5; // Initialiser le zoom
                    UpdateOverlays(); // Dessiner les overlays après le chargement
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetZoom(double zoom)
        {
            _currentZoom = zoom; // Met à jour la variable privée
            OnPropertyChanged(nameof(CurrentZoom)); // Notifie la vue du changement
            RequestSetZoom?.Invoke(this, _currentZoom); // Demande à la vue d'appliquer le zoom
            UpdateOverlays(); // Redessine les overlays si nécessaire (peut ne pas être nécessaire si le zoom est appliqué via RenderTransform)
        }

        private void ZoomIn()
        {
            CurrentZoom *= 1.2;
            CurrentZoom = Math.Min(5.0, CurrentZoom);
            SetZoom(CurrentZoom);
        }

        private void ZoomOut()
        {
            CurrentZoom /= 1.2;
            CurrentZoom = Math.Max(0.1, CurrentZoom);
            SetZoom(CurrentZoom);
        }

        // Gère le zoom de la molette de la souris, appelé par la vue
        public void HandleMouseWheelZoom(int delta)
        {
            double zoomFactor = delta > 0 ? 1.1 : 0.9;
            CurrentZoom *= zoomFactor;
            CurrentZoom = Math.Max(0.1, Math.Min(5.0, CurrentZoom));
            SetZoom(CurrentZoom);
        }

        public void UpdateOverlays()
        {
            var shapesToDraw = new List<OverlayShapeData>();

            if (!_overlaysVisible)
            {
                // Si les overlays ne sont pas visibles, on envoie une liste vide pour tout effacer.
                RequestOverlayUpdate?.Invoke(this, new OverlayUpdateEventArgs
                {
                    MinimapSource = this.MinimapSource,
                    OverlayCanvasWidth = this.OverlayCanvasWidth,
                    OverlayCanvasHeight = this.OverlayCanvasHeight,
                    CurrentZoom = this.CurrentZoom,
                    OverlaysVisible = false,
                    ShapesToDraw = shapesToDraw
                });
                return;
            }

            // Draw all events in green (if enabled)
            if (ShowFunctionPoints && FunctionPoint?.Events != null)
            {
                foreach (var eventData in FunctionPoint.Events)
                {
                    shapesToDraw.AddRange(GetEventShapes(eventData, _selectedEvent == eventData ? Brushes.Red : Brushes.Green));
                }
            }

            // Draw all HealAreas in sky blue (if enabled)
            if (ShowHealPoints && HealPoint?.HealAreas != null)
            {
                foreach (var healArea in HealPoint.HealAreas)
                {
                    shapesToDraw.AddRange(GetHealAreaShapes(healArea, _selectedHealArea == healArea ? Brushes.Red : Brushes.SkyBlue));
                }
            }

            // Draw NPCs in dark blue (if enabled)
            if (ShowNPCs && NPCs != null && NPCAppearDict != null)
            {
                shapesToDraw.AddRange(GetNPCShapes());
            }

            // TODO: Add the TreasureBoxes and Encounters:
            if (ShowTreasureBoxes && TreasureBoxes != null)
            {
                // TODO: Add logic to get TreasureBox shapes
            }

            if (ShowEncounters && Encounters != null)
            {
                // TODO: Add logic to get Encounter shapes
            }

            // Demander à la vue de dessiner ces formes
            RequestOverlayUpdate?.Invoke(this, new OverlayUpdateEventArgs
            {
                MinimapSource = this.MinimapSource,
                OverlayCanvasWidth = this.OverlayCanvasWidth,
                OverlayCanvasHeight = this.OverlayCanvasHeight,
                CurrentZoom = this.CurrentZoom,
                OverlaysVisible = true,
                ShapesToDraw = shapesToDraw
            });
        }

        private List<OverlayShapeData> GetNPCShapes()
        {
            var shapes = new List<OverlayShapeData>();
            if (NPCs == null || NPCAppearDict == null || MapEnvironment?.MMModelPos == null)
                return shapes;

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

                if (_selectedNPC != null && _selectedNPC.ID != npc.ID && !NpcModeShowAll)
                    continue;

                if (NpcModeShowAll || (_selectedNPC != null && _selectedNPC.ID == npc.ID))
                {
                    for (int i = 0; i < appears.Count; i++)
                    {
                        var appear = appears[i];
                        var brush = Brushes.DarkBlue;
                        if (_selectedNPC != null && _selectedNPC.ID == npc.ID)
                        {
                            if (_selectedNPCAppearIndices.ContainsKey(npc.ID) && _selectedNPCAppearIndices[npc.ID] == i)
                            {
                                brush = Brushes.Red;
                            }
                        }

                        var point = NPCPointToMiniMapPoint(boundaries, appear.LocationX, appear.LocationY,
                            (int)OverlayCanvasWidth, (int)OverlayCanvasHeight);
                        shapes.Add(new OverlayShapeData { ShapeType = OverlayShapeType.Point, Point = point, Brush = brush });
                    }
                }
                else
                {
                    var firstAppear = appears[0];
                    var brush = (_selectedNPC != null && _selectedNPC.ID == npc.ID) ? Brushes.Red : Brushes.DarkBlue;
                    var point = NPCPointToMiniMapPoint(boundaries, firstAppear.LocationX, firstAppear.LocationY,
                        (int)OverlayCanvasWidth, (int)OverlayCanvasHeight);
                    shapes.Add(new OverlayShapeData { ShapeType = OverlayShapeType.Point, Point = point, Brush = brush });
                }
            }
            return shapes;
        }

        private List<OverlayShapeData> GetEventShapes(Event eventData, System.Windows.Media.Brush brush)
        {
            var shapes = new List<OverlayShapeData>();
            if (eventData?.Position == null || MapEnvironment?.MMModelPos == null)
                return shapes;

            var boundaries = new int[]
            {
                (int)MapEnvironment.MMModelPos.MinX,
                (int)MapEnvironment.MMModelPos.MinY,
                (int)MapEnvironment.MMModelPos.MaxX,
                (int)MapEnvironment.MMModelPos.MaxY
            };
            var minimapPoint = NPCPointToMiniMapPoint(boundaries, eventData.Position.X, eventData.Position.Y,
                                                     (int)OverlayCanvasWidth, (int)OverlayCanvasHeight);

            if (eventData.Area is BoxLimiter box)
            {
                double scaleX = OverlayCanvasWidth / (boundaries[2] - boundaries[0]);
                double scaleY = OverlayCanvasHeight / (boundaries[3] - boundaries[1]);
                double pixelWidth = box.Width * scaleX;
                double pixelHeight = box.Height * scaleY;
                shapes.Add(new OverlayShapeData
                {
                    ShapeType = OverlayShapeType.Rectangle,
                    Point = minimapPoint,
                    Width = pixelWidth,
                    Height = pixelHeight,
                    Angle = box.Angle,
                    Brush = brush
                });
            }
            else if (eventData.Area is CircleLimiter circle)
            {
                double scaleX = OverlayCanvasWidth / (boundaries[2] - boundaries[0]);
                double pixelRadius = circle.Radius * scaleX;
                shapes.Add(new OverlayShapeData
                {
                    ShapeType = OverlayShapeType.Circle,
                    Point = minimapPoint,
                    Radius = pixelRadius,
                    Brush = brush
                });
            }
            else
            {
                shapes.Add(new OverlayShapeData { ShapeType = OverlayShapeType.Point, Point = minimapPoint, Brush = brush });
            }
            return shapes;
        }

        private List<OverlayShapeData> GetHealAreaShapes(HealArea healArea, System.Windows.Media.Brush brush)
        {
            var shapes = new List<OverlayShapeData>();
            if (healArea?.Position == null || MapEnvironment?.MMModelPos == null)
                return shapes;

            var boundaries = new int[]
            {
                (int)MapEnvironment.MMModelPos.MinX,
                (int)MapEnvironment.MMModelPos.MinY,
                (int)MapEnvironment.MMModelPos.MaxX,
                (int)MapEnvironment.MMModelPos.MaxY
            };
            var minimapPoint = NPCPointToMiniMapPoint(boundaries, healArea.Position.X, healArea.Position.Y,
                                                     (int)OverlayCanvasWidth, (int)OverlayCanvasHeight);

            shapes.Add(new OverlayShapeData { ShapeType = OverlayShapeType.Point, Point = minimapPoint, Brush = brush });
            return shapes;
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

            // Utiliser System.Windows.Point pour éviter les problèmes de casting ultérieurs
            double mapX = (pointX - minX) * scaleX;
            double mapY = (pointY - minY) * scaleY;

            return new System.Windows.Point(mapX, mapY);
        }
    }
}