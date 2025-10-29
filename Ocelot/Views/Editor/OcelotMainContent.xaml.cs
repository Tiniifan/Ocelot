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
using StudioElevenGUI.ViewModels;
using Ocelot.ViewModels;
using Point = System.Windows.Point;
using Brush = System.Windows.Media.Brush;
using Ocelot.ViewModels.TreeView;

namespace Ocelot.Views
{
    public partial class OcelotMainContent : UserControl
    {
        private Border _activeTab;

        private Point _lastMousePosition;
        private bool _isDragging;

        public OcelotViewModel ViewModel => DataContext as OcelotViewModel;

        public OcelotMainContent()
        {
            InitializeComponent();

            SetActiveTab(MinimapTab);

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.RequestOverlayUpdate += ViewModel_RequestOverlayUpdate;
            }
        }

        public void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ZoomInCommand.Execute(null);
        }

        public void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ZoomOutCommand.Execute(null);
        }

        public void ClearOverlaysButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ToggleOverlaysVisibilityCommand.Execute(null);
        }

        public void ToggleNPCModeButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ToggleNpcModeCommand.Execute(null);
        }

        public void FunctionPointsCheckBox_Checked(object sender, RoutedEventArgs e) => ViewModel.ShowFunctionPoints = true;
        public void FunctionPointsCheckBox_Unchecked(object sender, RoutedEventArgs e) => ViewModel.ShowFunctionPoints = false;
        public void HealPointsCheckBox_Checked(object sender, RoutedEventArgs e) => ViewModel.ShowHealPoints = true;
        public void HealPointsCheckBox_Unchecked(object sender, RoutedEventArgs e) => ViewModel.ShowHealPoints = false;
        public void NPCsCheckBox_Checked(object sender, RoutedEventArgs e) => ViewModel.ShowNPCs = true;
        public void NPCsCheckBox_Unchecked(object sender, RoutedEventArgs e) => ViewModel.ShowNPCs = false;
        public void TreasureBoxesCheckBox_Checked(object sender, RoutedEventArgs e) => ViewModel.ShowTreasureBoxes = true;
        public void TreasureBoxesCheckBox_Unchecked(object sender, RoutedEventArgs e) => ViewModel.ShowTreasureBoxes = false;
        public void EncountersCheckBox_Checked(object sender, RoutedEventArgs e) => ViewModel.ShowEncounters = true;
        public void EncountersCheckBox_Unchecked(object sender, RoutedEventArgs e) => ViewModel.ShowEncounters = false;


        // Gestion du déplacement de la carte
        public void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _lastMousePosition = e.GetPosition(ImageScrollViewer);
            MinimapImage.CaptureMouse();
            MinimapImage.Cursor = Cursors.SizeAll;
        }

        public void Image_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            MinimapImage.ReleaseMouseCapture();
            MinimapImage.Cursor = Cursors.Hand;
        }

        public void Image_MouseMove(object sender, MouseEventArgs e)
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

        public void MinimapImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ViewModel?.HandleMouseWheelZoom(e.Delta);
            e.Handled = true;
        }

        public void EventOverlayCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MinimapImage_MouseWheel(MinimapImage, e);
        }

        public void EventOverlayCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image_MouseRightButtonDown(MinimapImage, e);
        }

        public void EventOverlayCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image_MouseRightButtonUp(MinimapImage, e);
        }

        public void EventOverlayCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Image_MouseMove(MinimapImage, e);
        }

        private void ViewModel_RequestOverlayUpdate(object sender, OverlayUpdateEventArgs e)
        {
            ClearEventOverlays();

            if (!e.OverlaysVisible)
            {
                // Si les overlays ne sont pas visibles, on ne dessine rien et on sort.
                return;
            }

            MinimapImage.Source = e.MinimapSource;

            foreach (var overlayShapeData in e.ShapesToDraw)
            {
                Shape shape = null;
                switch (overlayShapeData.ShapeType)
                {
                    case OverlayShapeType.Point:
                        shape = CreatePoint(overlayShapeData.Point, overlayShapeData.Brush);
                        break;
                    case OverlayShapeType.Rectangle:
                        shape = CreateRectangle(overlayShapeData.Point, overlayShapeData.Width, overlayShapeData.Height, overlayShapeData.Angle, overlayShapeData.Brush);
                        break;
                    case OverlayShapeType.Circle:
                        shape = CreateCircle(overlayShapeData.Point, overlayShapeData.Radius, overlayShapeData.Brush);
                        break;
                }

                if (shape != null)
                {
                    EventOverlayCanvas.Children.Add(shape);
                }
            }
        }

        private void ViewModel_RequestUpdateOverlayText(object sender, string newText)
        {
            if (ClearOverlaysButton != null)
            {
                ClearOverlaysButton.Content = newText;
            }
        }

        private void ViewModel_RequestUpdateNpcModeText(object sender, string newText)
        {
            if (ToggleNPCModeButton != null)
            {
                ToggleNPCModeButton.Content = newText;
            }
        }

        private void ClearEventOverlays()
        {
            EventOverlayCanvas.Children.Clear();
        }

        private Ellipse CreatePoint(System.Windows.Point point, System.Windows.Media.Brush brush)
        {
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
            return ellipse;
        }

        private System.Windows.Shapes.Rectangle CreateRectangle(System.Windows.Point center, double width, double height, double angle, System.Windows.Media.Brush brush)
        {
            var rectangle = new System.Windows.Shapes.Rectangle
            {
                Width = width,
                Height = height,
                Fill = System.Windows.Media.Brushes.Transparent,
                Stroke = brush,
                StrokeThickness = 2,
                Opacity = 0.7
            };
            Canvas.SetLeft(rectangle, center.X - width / 2);
            Canvas.SetTop(rectangle, center.Y - height / 2);

            if (angle != 0)
            {
                rectangle.RenderTransform = new RotateTransform(angle, width / 2, height / 2);
            }
            return rectangle;
        }

        private Ellipse CreateCircle(System.Windows.Point center, double radius, System.Windows.Media.Brush brush)
        {
            var ellipse = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = System.Windows.Media.Brushes.Transparent,
                Stroke = brush,
                StrokeThickness = 2,
                Opacity = 0.7
            };
            Canvas.SetLeft(ellipse, center.X - radius);
            Canvas.SetTop(ellipse, center.Y - radius);
            return ellipse;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is OcelotViewModel viewModel)
            {
                viewModel.SelectedTreeViewItem = e.NewValue as TreeViewItemViewModel;
            }
        }

        private void TabClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border tab)
            {
                SetActiveTab(tab);

                // Mettre à jour le ViewModel
                if (DataContext is OcelotViewModel vm)
                {
                    vm.SetTabCommand.Execute(tab.Name);
                }
            }
        }

        private void SetActiveTab(Border selectedTab)
        {
            ResetTabStyles();

            _activeTab = selectedTab;

            selectedTab.Background = (Brush)FindResource("Theme.Accent.Brush");
            selectedTab.Opacity = 1.0;

            if (selectedTab.Name == "MinimapTab")
            {
                ShowMinimapContent();
            }
            else if (selectedTab.Name == "View3DTab")
            {
                Show3DContent();
            }
        }

        private void ResetTabStyles()
        {
            MinimapTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
            MinimapTab.Opacity = 0.8;

            View3DTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
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
    }
}