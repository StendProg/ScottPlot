using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Pen = System.Drawing.Pen;

namespace ScottPlot
{
    public partial class WpfPlotOverlay : WpfPlot
    {
        private System.Windows.Controls.Image Overlay;

        public List<IPlottable> OverlayPlottables = new List<IPlottable>();
        public WpfPlotOverlay()
        {
            Overlay = new System.Windows.Controls.Image() { Name = "Overlay" };
            canvasPlot.Children.Add(Overlay);
            InitializeComponent();
        }

        public override void Render(bool skipIfCurrentlyRendering = false, bool lowQuality = false, bool recalculateLayout = false, bool processEvents = false)
        {
            base.Render(skipIfCurrentlyRendering, lowQuality, recalculateLayout, processEvents);
            RenderOverlay();
        }
        
        public void RenderOverlay()
        {
            var bmp = new Bitmap((int)plt.GetSettings(false).Width, (int)plt.GetSettings(false).Height);
            
            var gfx = Graphics.FromImage(bmp);

            foreach (IPlottable p in OverlayPlottables)
            {
                PlotDimensions dims = plt.GetSettings(false).GetPlotDimensions(p.XAxisIndex, p.YAxisIndex);
                p.Render(dims, bmp);
            }

            gfx.DrawLine(new Pen(System.Drawing.Color.Blue), new PointF(0, 0), new PointF(50, 50));
            if (Overlay != null)
                Overlay.Source = BmpImageFromBmp(bmp);
        }

        protected override IDraggable GetDraggableUnderMouse(double pixelX, double pixelY, int snapDistancePixels = 5)
        {
            double snapWidth = plt.GetSettings(false).XAxis.Dims.UnitsPerPx * snapDistancePixels;
            double snapHeight = plt.GetSettings(false).YAxis.Dims.UnitsPerPx * snapDistancePixels;

            foreach (IDraggable draggable in OverlayPlottables.Where( p => p is IDraggable))
                if (draggable.IsUnderMouse(plt.GetCoordinateX((float)pixelX), plt.GetCoordinateY((float)pixelY), snapWidth, snapHeight))
                    if (draggable.DragEnabled)
                        return draggable;

            return null;
        }

        protected override void RenderMovedDraggable()
        {
            RenderOverlay();
        }

        protected override void MouseMovedWithoutInteraction(MouseEventArgs e)
        {
            // set the cursor based on what's beneath it
            var draggableUnderCursor = GetDraggableUnderMouse(GetPixelPosition(e).X, GetPixelPosition(e).Y);
            var spCursor = (draggableUnderCursor is null) ? ScottPlot.Cursor.Arrow : draggableUnderCursor.DragCursor;
            Overlay.Cursor = GetCursor(spCursor);
        }
    }
}
