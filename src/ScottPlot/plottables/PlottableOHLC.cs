﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ScottPlot
{
    public class PlottableOHLC : Plottable
    {
        public OHLC[] ohlcs;
        bool displayCandles;
        Pen penUp;
        Pen penDown;
        Brush brushUp;
        Brush brushDown;

        public PlottableOHLC(OHLC[] ohlcs, bool displayCandles = true)
        {
            this.ohlcs = ohlcs;
            pointCount = ohlcs.Length;
            this.displayCandles = displayCandles;

            Color colorUp = Color.DarkGreen;
            Color colorDown = Color.Red;

            penUp = new Pen(colorUp);
            penDown = new Pen(colorDown);
            brushUp = new SolidBrush(colorUp);
            brushDown = new SolidBrush(colorDown);
        }

        public override string ToString()
        {
            return $"PlottableOHLC with {pointCount} points";
        }

        public override Config.AxisLimits2D GetLimits()
        {
            double[] limits = new double[4];
            limits[0] = ohlcs[0].time;
            limits[1] = ohlcs[0].time;
            limits[2] = ohlcs[0].low;
            limits[3] = ohlcs[0].high;

            for (int i = 1; i < ohlcs.Length; i++)
            {
                if (ohlcs[i].time < limits[0])
                    limits[0] = ohlcs[i].time;
                if (ohlcs[i].time > limits[1])
                    limits[1] = ohlcs[i].time;
                if (ohlcs[i].low < limits[2])
                    limits[2] = ohlcs[i].low;
                if (ohlcs[i].high > limits[3])
                    limits[3] = ohlcs[i].high;
            }

            // TODO: use features of 2d axis
            return new Config.AxisLimits2D(limits);
        }

        public override void Render(Settings settings)
        {
            if (displayCandles)
                RenderCandles(settings);
            else
                RenderOhlc(settings);
        }

        private double GetSmallestSpacing()
        {
            double smallestSpacing = double.PositiveInfinity;
            for (int i = 1; i < ohlcs.Length; i++)
                smallestSpacing = Math.Min(ohlcs[i].time - ohlcs[i - 1].time, smallestSpacing);
            return smallestSpacing;
        }

        public void RenderCandles(Settings settings)
        {
            double fractionalTickWidth = .7;
            double spacingPx = GetSmallestSpacing() * settings.xAxisScale;
            float boxWidth = (float)(spacingPx / 2 * fractionalTickWidth);

            List<PointF> ohlcsHigherPoints = new List<PointF>();
            List<PointF> ohlcsLowerPoints = new List<PointF>();

            List<RectangleF> ohlcsHigherRects = new List<RectangleF>();
            List<RectangleF> ohlcsLowerRects = new List<RectangleF>();

            foreach (OHLC ohlc in ohlcs)
            {
                // the wick below the box
                PointF wickLowBot = settings.GetPixel(ohlc.time, ohlc.low);
                PointF wickLowTop = settings.GetPixel(ohlc.time, ohlc.lowestOpenClose);

                // the wick above the box
                PointF wickHighBot = settings.GetPixel(ohlc.time, ohlc.highestOpenClose);
                PointF wickHighTop = settings.GetPixel(ohlc.time, ohlc.high);

                // the candle
                PointF boxLowerLeft = settings.GetPixel(ohlc.time, ohlc.lowestOpenClose);
                PointF boxUpperRight = settings.GetPixel(ohlc.time, ohlc.highestOpenClose);

                if (ohlc.closedHigher)
                {
                    ohlcsHigherPoints.Add(wickLowBot);
                    ohlcsHigherPoints.Add(wickLowTop);

                    ohlcsHigherPoints.Add(wickHighBot);
                    ohlcsHigherPoints.Add(wickHighTop);

                    ohlcsHigherRects.Add(new RectangleF(boxLowerLeft.X - boxWidth, boxUpperRight.Y, boxWidth * 2, boxLowerLeft.Y - boxUpperRight.Y));
                }
                else
                {
                    ohlcsLowerPoints.Add(wickLowBot);
                    ohlcsLowerPoints.Add(wickLowTop);

                    ohlcsLowerPoints.Add(wickHighBot);
                    ohlcsLowerPoints.Add(wickHighTop);

                    ohlcsLowerRects.Add(new RectangleF(boxLowerLeft.X - boxWidth, boxUpperRight.Y, boxWidth * 2, boxLowerLeft.Y - boxUpperRight.Y));
                }
            }
            penUp.Width = 2;
            penDown.Width = 2;
            settings.dataBackend.DrawLinesPaired(penUp, ohlcsHigherPoints.ToArray());
            settings.dataBackend.DrawLinesPaired(penDown, ohlcsLowerPoints.ToArray());

            settings.dataBackend.FillRectangles(brushUp, ohlcsHigherRects.ToArray());
            settings.dataBackend.FillRectangles(brushDown, ohlcsLowerRects.ToArray());
        }

        public void RenderOhlc(Settings settings)
        {
            double fractionalTickWidth = 1;
            double spacingPx = GetSmallestSpacing() * settings.xAxisScale;
            float boxWidth = (float)(spacingPx / 2 * fractionalTickWidth);

            var ohlcsPoints = ohlcs.Select(x =>
            {
                PointF wickTop = settings.GetPixel(x.time, x.low);
                PointF wickBot = settings.GetPixel(x.time, x.high);

                // open and close lines
                float xPx = wickTop.X;
                float yPxOpen = (float)settings.GetPixel(0, x.open).Y;
                float yPxClose = (float)settings.GetPixel(0, x.close).Y;
                var res = new PointF[]
                {
                    wickBot, wickTop,
                    new PointF(xPx - boxWidth, yPxOpen), new PointF(xPx, yPxOpen),
                    new PointF(xPx + boxWidth, yPxClose), new PointF(xPx, yPxClose)
                };
                return new Tuple<OHLC, PointF[]>(x, res);
            });

            var ohlcsHigher = ohlcsPoints.Where(t => t.Item1.closedHigher).Select(x => x.Item2).SelectMany(x => x);
            var ohlcsLower = ohlcsPoints.Where(t => !t.Item1.closedHigher).Select(x => x.Item2).SelectMany(x => x);

            penUp.Width = 2;
            penDown.Width = 2;
            settings.dataBackend.DrawLinesPaired(penUp, ohlcsHigher.ToArray());
            settings.dataBackend.DrawLinesPaired(penDown, ohlcsLower.ToArray());
        }
    }
}
