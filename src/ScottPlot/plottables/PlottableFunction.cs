﻿using ScottPlot.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace ScottPlot
{
    public class PlottableFunction : Plottable
    {
        private readonly Func<double, double?> function;
        private readonly double lineWidth;
        private readonly double markerSize;

        public PlottableFunction(Func<double, double?> function, Color color, double lineWidth, double markerSize, string label, MarkerShape markerShape, LineStyle lineStyle)
        {
            this.function = function;
            this.color = color;
            this.lineWidth = lineWidth;
            this.markerSize = markerSize;
            this.label = label;
            this.markerShape = markerShape;
            this.lineStyle = lineStyle;
        }

        public override AxisLimits2D GetLimits()
        {
            double max = double.NegativeInfinity;
            double min = double.PositiveInfinity;

            foreach (double x in DataGen.Range(-10, 10, .1))
            {
                double? y = function(x);
                if (y != null)
                {
                    max = Math.Max(max, y.Value);
                    min = Math.Min(min, y.Value);
                }
            }

            double[] limits = { -10, 10, min, max };

            return new Config.AxisLimits2D(limits);
        }


        public override void Render(Settings settings)
        {
            double step = settings.xAxisUnitsPerPixel;

            double minRenderedX = settings.axes.limits[0];
            double maxRenderedX = settings.axes.limits[1];

            int maxSeriesLength = (int)Math.Ceiling((maxRenderedX - minRenderedX) / step);

            List<double> xList = new List<double>();
            List<double> yList = new List<double>();

            for (int i = 0; i < maxSeriesLength; i++)
            {
                double x = i * step + minRenderedX;
                double? y;
                try
                {
                    y = function(x);
                }
                catch (Exception e) //Domain error, such log(-1) or 1/0
                {
                    Debug.WriteLine(e);
                    continue;
                }

                if (y.HasValue)
                {
                    xList.Add(x);
                    yList.Add(y.Value);
                }
            }


            PlottableScatter scatter = new PlottableScatter(xList.ToArray(), yList.ToArray(), color, lineWidth, markerSize, label, null, null, 0, 0, false, markerShape, lineStyle);
            scatter.Render(settings);
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
