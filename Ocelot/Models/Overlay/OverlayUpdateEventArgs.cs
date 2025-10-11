using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocelot.ViewModels;
using System.Windows.Media.Imaging;

namespace Ocelot.Models
{
    public class OverlayUpdateEventArgs : EventArgs
    {
        public BitmapSource MinimapSource { get; set; }
        public double OverlayCanvasWidth { get; set; }
        public double OverlayCanvasHeight { get; set; }
        public double CurrentZoom { get; set; }
        public bool OverlaysVisible { get; set; }
        public List<OverlayShapeData> ShapesToDraw { get; set; } = new List<OverlayShapeData>();
    }
}
