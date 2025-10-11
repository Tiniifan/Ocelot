using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.Models
{
    public class OverlayShapeData
    {
        public OverlayShapeType ShapeType { get; set; }
        public System.Windows.Point Point { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }
        public double Angle { get; set; }
        public System.Windows.Media.Brush Brush { get; set; }
    }

}
