using ScottPlot.Config;
using ScottPlot.Diagnostic.Attributes;
using ScottPlot.Drawing;
using ScottPlot.plottables;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace ScottPlot
{
    public class PlottableText : Plottable, IPlottable
    {

        private double x;
        public double X 
        {
            get => x;
            set
            {
                if (double.IsInfinity(value) || double.IsNaN(value))
                    ValidationErrorMessage = "X must be a rational number";
                    // we just set last error message here, but can throw or write debug message here
                else
                    x = value;
            }
        }

        private double y;
        public double Y
        {
            get => y;
            set
            {
                if (double.IsInfinity(value) || double.IsNaN(value))
                    ValidationErrorMessage = "Y must be a rational number";
                    // we just set last error message here, but can throw or write debug message here
                else
                    y = value;
            }
        }

        private double rotation;
        public double Rotation 
        {
            get => rotation;
            set
            {
                if (double.IsInfinity(Rotation) || double.IsNaN(Rotation))
                    ValidationErrorMessage = "rotation must be a rational number";
                else
                    rotation = value;
            }
        }
        public string Text { get; set;}
        public TextAlignment Alignment { get; set; }
        public bool Frame { get; set; }
        public Color FrameColor { get; set; }
        public string Label { get; set; }

        public Color FontColor { get; set; }
        public string FontName {get; set;}

        private float fontSize;
        public float FontSize 
        {
            get => fontSize;
            set
            {
                if (value >= 1)
                    fontSize = value;
                else
                    ValidationErrorMessage = "font must be at least size 1";
            }
        }
        public bool FontBold { get; set; }

        public PlottableText() { }

        //[Obsolete("use the new constructor with data-only arguments", true)]
        public PlottableText(string text, double x, double y, Color color, string fontName, double fontSize, bool bold, string label, TextAlignment alignment, double rotation, bool frame, Color frameColor)
        {
            this.Text = text ?? throw new Exception("Text cannot be null");
            this.X = x;
            this.Y = y;
            this.Rotation = rotation;
            this.Label = label;
            this.Alignment = alignment;
            this.Frame = frame;
            this.FrameColor = frameColor;

            (FontColor, FontName, FontSize, FontBold) = (color, fontName, (float)fontSize, bold);
        }

        public override string ToString() => $"PlottableText \"{Text}\" at ({X}, {Y})";

        public override AxisLimits2D GetLimits() => new AxisLimits2D(X, X, Y, Y);

        public override void Render(Settings settings) => throw new NotImplementedException("Use the other Render method");

        public override int GetPointCount() => 1;

        public override LegendItem[] GetLegendItems() => null; // never show in legend

        public string ValidationErrorMessage { get; private set; }
        public bool IsValidData(bool deepValidation = false)
        {
            StringBuilder sb = new StringBuilder();

            if (double.IsInfinity(X) || double.IsNaN(X))
                sb.AppendLine("X must be a rational number");

            if (double.IsInfinity(Y) || double.IsNaN(Y))
                sb.AppendLine("Y must be a rational number");

            if (double.IsInfinity(Rotation) || double.IsNaN(Rotation))
                sb.AppendLine("rotation must be a rational number");

            if (FontSize < 1)
                sb.AppendLine("font must be at least size 1");

            ValidationErrorMessage = sb.ToString();

            return ValidationErrorMessage.Length == 0;
        }

        private (float pixelX, float pixelY) ApplyAlignmentOffset(float pixelX, float pixelY, float stringWidth, float stringHeight)
        {
            switch (Alignment)
            {
                case TextAlignment.lowerCenter:
                    return (pixelX - stringWidth / 2, pixelY - stringHeight);
                case TextAlignment.lowerLeft:
                    return (pixelX, pixelY - stringHeight);
                case TextAlignment.lowerRight:
                    return (pixelX - stringWidth, pixelY - stringHeight);
                case TextAlignment.middleLeft:
                    return (pixelX, pixelY - stringHeight / 2);
                case TextAlignment.middleRight:
                    return (pixelX - stringWidth, pixelY - stringHeight / 2);
                case TextAlignment.upperCenter:
                    return (pixelX - stringWidth / 2, pixelY);
                case TextAlignment.upperLeft:
                    return (pixelX, pixelY);
                case TextAlignment.upperRight:
                    return (pixelX - stringWidth, pixelY);
                case TextAlignment.middleCenter:
                    return (pixelX - stringWidth / 2, pixelY - stringHeight / 2);
                default:
                    throw new InvalidEnumArgumentException("that alignment is not recognized");
            }
        }

        public void Render(PlotDimensions dims, Bitmap bmp)
        {
            using (Graphics gfx = Graphics.FromImage(bmp))
            using (var fontBrush = new SolidBrush(FontColor))
            using (var frameBrush = new SolidBrush(FrameColor))
            using (var font = GDI.Font(FontName, FontSize, FontBold))
            {
                float pixelX = dims.GetPixelX(x);
                float pixelY = dims.GetPixelY(y);
                SizeF stringSize = GDI.MeasureString(gfx, Text, font);
                RectangleF stringRect = new RectangleF(0, 0, stringSize.Width, stringSize.Height);

                if (rotation == 0)
                    (pixelX, pixelY) = ApplyAlignmentOffset(pixelX, pixelY, stringSize.Width, stringSize.Height);

                gfx.TranslateTransform(pixelX, pixelY);
                gfx.RotateTransform((float)Rotation);

                if (Frame)
                    gfx.FillRectangle(frameBrush, stringRect);

                gfx.DrawString(Text, font, fontBrush, new PointF(0, 0));

                gfx.ResetTransform();
            }
        }
    }
}
