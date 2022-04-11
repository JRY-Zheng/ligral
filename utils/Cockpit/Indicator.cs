using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;

namespace Cockpit
{
    class Indicator
    {
        private Canvas canvas;
        public double UpperBound {get; set;}
        public double LowerBound {get; set;}
        public double Window {get; set;}
        public double Interval {get; set;}
        public double CurrentValue {get; set;}
        private List<double> LongTicks = new List<double>();
        private List<double> MediumTicks = new List<double>();
        private List<double> ShortTicks = new List<double>();
        static Brush tickBrush = Brushes.White;
        public Indicator(Canvas canvas, double upper, double lower=0)
        {
            this.canvas = canvas;
            this.UpperBound = upper;
            this.LowerBound = lower;
            this.Window = (UpperBound-LowerBound)/10;
            this.Interval = Window/5;
        }
        public void NormalisedTicks()
        {
            double valueAtBottom = CurrentValue - Window/2;
            double valueAtTop = CurrentValue + Window/2;
            double tickValue = LowerBound;
            double step = Interval/4;
            double tickIndex = 0;
            ShortTicks.Clear();
            MediumTicks.Clear();
            LongTicks.Clear();
            while (tickValue < valueAtBottom)
            {
                tickValue += step;
                tickIndex ++;
            }
            while (tickValue < valueAtTop && tickValue < UpperBound)
            {
                double tick = (tickValue-valueAtTop)/(valueAtBottom-valueAtTop);
                switch (tickIndex % 4)
                {
                case 0:
                    LongTicks.Add(tick); break;
                case 2:
                    MediumTicks.Add(tick); break;
                case 1:
                case 3:
                    ShortTicks.Add(tick); break;
                }
                tickValue += step;
                tickIndex ++;
            }
        }
        public void DrawLinearTicks()
        {
            int i = 0;
            List<Line> unusedLine = new List<Line>();
            foreach (var element in canvas.Children)
            {
                if (element is Line line) 
                {
                    if (i<ShortTicks.Count)
                    {
                        line.X1 = canvas.ActualWidth*0.3;
                        line.X2 = canvas.ActualWidth*0.7;
                        line.Y1 = canvas.ActualHeight*ShortTicks[i];
                        line.Y2 = canvas.ActualHeight*ShortTicks[i];
                    }
                    else if (i<ShortTicks.Count+MediumTicks.Count)
                    {
                        int ii = i - ShortTicks.Count;
                        line.X1 = canvas.ActualWidth*0.25;
                        line.X2 = canvas.ActualWidth*0.75;
                        line.Y1 = canvas.ActualHeight*MediumTicks[ii];
                        line.Y2 = canvas.ActualHeight*MediumTicks[ii];
                    }
                    else if (i<ShortTicks.Count+MediumTicks.Count+LongTicks.Count)
                    {
                        int ii = i - ShortTicks.Count - MediumTicks.Count;
                        line.X1 = canvas.ActualWidth*0.1;
                        line.X2 = canvas.ActualWidth*0.9;
                        line.Y1 = canvas.ActualHeight*LongTicks[ii];
                        line.Y2 = canvas.ActualHeight*LongTicks[ii];
                    }
                    else
                    {
                        unusedLine.Add(line);
                    }
                    i++;
                }
                else
                {
                    continue;
                }
            }
            foreach (Line line in unusedLine)
            {
                canvas.Children.Remove(line);
            }
            while (i<ShortTicks.Count+MediumTicks.Count+LongTicks.Count)
            {
                Line line = new Line(){Stroke = tickBrush, StrokeThickness = 1};
                if (i<ShortTicks.Count)
                {
                    line.X1 = canvas.ActualWidth*0.3;
                    line.X2 = canvas.ActualWidth*0.7;
                    line.Y1 = canvas.ActualHeight*ShortTicks[i];
                    line.Y2 = canvas.ActualHeight*ShortTicks[i];
                }
                else if (i<ShortTicks.Count+MediumTicks.Count)
                {
                    int ii = i - ShortTicks.Count;
                    line.X1 = canvas.ActualWidth*0.25;
                    line.X2 = canvas.ActualWidth*0.75;
                    line.Y1 = canvas.ActualHeight*ShortTicks[ii];
                    line.Y2 = canvas.ActualHeight*ShortTicks[ii];
                }
                else
                {
                    int ii = i - ShortTicks.Count - MediumTicks.Count;
                    line.X1 = canvas.ActualWidth*0.1;
                    line.X2 = canvas.ActualWidth*0.8;
                    line.Y1 = canvas.ActualHeight*ShortTicks[ii];
                    line.Y2 = canvas.ActualHeight*ShortTicks[ii];
                }
                canvas.Children.Add(line);
                i++;
            }
        }
    }
}