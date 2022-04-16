using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using System;
using System.Windows;

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
        public double BetaValue {get; set;}
        public double AlphaValue {get; set;}
        public double LongPosition1 {get; set;} = 0.3;
        public double LongPosition2 {get; set;} = 0.9;
        public double MediumPosition1 {get; set;} = 0.25;
        public double MediumPosition2 {get; set;} = 0.75;
        public double ShortPosition1 {get; set;} = 0.3;
        public double ShortPosition2 {get; set;} = 0.7;
        public double LabelPosition {get; set;} = 0.05;
        public double PointerTop {get; set;} = -0.2;
        public double PointerHeight {get; set;} = 0.05;
        public double PointerWidth {get; set;} = 0.05;
        public double LabelHeight {get; set;} = 0.1;
        public int LabelPrecision {get; set;} = 1;
        public double LabelTextLeft {get; set;} = 0.3;
        public string LabelZero {get; set;} = "0";
        private List<double> LongTicks = new List<double>();
        private List<double> MediumTicks = new List<double>();
        private List<double> ShortTicks = new List<double>();
        private List<double> LongTickValues = new List<double>();
        private List<Line> lines = new List<Line>();
        private List<TextBlock> labels = new List<TextBlock>();
        private Polygon pointer;
        private Polygon slider;
        private Polygon flightPath;
        private Polygon labelBackground;
        private TextBlock MainLabel;
        private TextBlock UpperLabel;
        private TextBlock LowerLabel;
        private bool labelEnabled = false;
        static Brush tickBrush = Brushes.White;
        public Indicator(Canvas canvas, double upper, double lower=0)
        {
            this.canvas = canvas;
            this.UpperBound = upper;
            this.LowerBound = lower;
            this.Window = (UpperBound-LowerBound)/5;
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
            LongTickValues.Clear();
            while (tickValue < valueAtBottom)
            {
                tickValue += step;
                tickIndex ++;
            }
            while (tickValue <= valueAtTop && tickValue <= UpperBound)
            {
                double tick = (tickValue-valueAtTop)/(valueAtBottom-valueAtTop);
                if (!labelEnabled || tick<0.5-LabelHeight/2 || tick>0.5+LabelHeight/2)
                {
                    switch (tickIndex % 4)
                    {
                    case 0:
                        LongTicks.Add(tick); 
                        LongTickValues.Add(tickValue);
                        break;
                    case 2:
                        MediumTicks.Add(tick); break;
                    case 1:
                    case 3:
                        ShortTicks.Add(tick); break;
                    }
                }
                tickValue += step;
                tickIndex ++;
            }
        }
        public void PeriodisedTicks()
        {
            double valueAtBottom = CurrentValue - Window/2;
            double valueAtTop = CurrentValue + Window/2;
            double tickValue = LowerBound;
            double step = Interval/4;
            double tickIndex = 0;
            ShortTicks.Clear();
            MediumTicks.Clear();
            LongTicks.Clear();
            LongTickValues.Clear();
            while (tickValue < valueAtBottom)
            {
                tickValue += step;
                tickIndex ++;
            }
            while (tickValue-step >= valueAtBottom)
            {
                tickValue -= step;
                tickIndex --;
            }
            while (tickValue <= valueAtTop)
            {
                double tick = (valueAtBottom-tickValue)/(valueAtBottom-valueAtTop);
                switch (tickIndex % 4)
                {
                case 0:
                    LongTicks.Add(tick); 
                    LongTickValues.Add((tickValue-LowerBound)%(UpperBound-LowerBound)+LowerBound);
                    break;
                case 2:
                case -2:
                    MediumTicks.Add(tick); break;
                case 1:
                case 3:
                case -3:
                case -1:
                    ShortTicks.Add(tick); break;
                }
                tickValue += step;
                tickIndex ++;
            }
        }
        public void DrawLinearTicks()
        {
            int i = 0;
            while (i < ShortTicks.Count + MediumTicks.Count + LongTicks.Count)
            {
                Line line;
                if (i < lines.Count)
                {
                    line = lines[i];
                }
                else
                {
                    line = new Line() {Stroke = tickBrush, StrokeThickness = 1};
                    lines.Add(line);
                }
                if (i<ShortTicks.Count)
                {
                    line.X1 = canvas.ActualWidth*ShortPosition1;
                    line.X2 = canvas.ActualWidth*ShortPosition2;
                    line.Y1 = canvas.ActualHeight*ShortTicks[i];
                    line.Y2 = canvas.ActualHeight*ShortTicks[i];
                }
                else if (i<ShortTicks.Count+MediumTicks.Count)
                {
                    int ii = i - ShortTicks.Count;
                    line.X1 = canvas.ActualWidth*MediumPosition1;
                    line.X2 = canvas.ActualWidth*MediumPosition2;
                    line.Y1 = canvas.ActualHeight*MediumTicks[ii];
                    line.Y2 = canvas.ActualHeight*MediumTicks[ii];
                }
                else if (i<ShortTicks.Count+MediumTicks.Count+LongTicks.Count)
                {
                    int ii = i - ShortTicks.Count - MediumTicks.Count;
                    line.X1 = canvas.ActualWidth*LongPosition1;
                    line.X2 = canvas.ActualWidth*LongPosition2;
                    line.Y1 = canvas.ActualHeight*LongTicks[ii];
                    line.Y2 = canvas.ActualHeight*LongTicks[ii];
                }
                if (!canvas.Children.Contains(line))
                {
                    canvas.Children.Add(line);
                }
                i++;
            }
            while (i < lines.Count)
            {
                if (canvas.Children.Contains(lines[i]))
                {
                    canvas.Children.Remove(lines[i]);
                }
                i++;
            }
            i = 0;
            while (i < LongTicks.Count)
            {
                TextBlock label;
                if (i < labels.Count)
                {
                    label = labels[i];
                    label.Text = LongTickValues[i].ToString();
                    Canvas.SetTop(label, canvas.ActualHeight*LongTicks[i]-label.ActualHeight/2);
                }
                else
                {
                    label = new TextBlock();
                    labels.Add(label);
                    label.Text = LongTickValues[i].ToString();
                    Canvas.SetTop(label, canvas.ActualHeight*LongTicks[i]-label.ActualHeight/2);
                    Canvas.SetLeft(label, LabelPosition*canvas.ActualWidth);
                    label.Foreground = tickBrush;
                }
                if (!canvas.Children.Contains(label))
                {
                    canvas.Children.Add(label);
                }
                i++;
            }
            while (i < labels.Count)
            {
                if (canvas.Children.Contains(labels[i]))
                {
                    canvas.Children.Remove(labels[i]);
                }
                i++;
            }
        }
        public void DrawRadiusTicks()
        {
            int i = 0;
            double radius = canvas.ActualHeight;
            double windowAngle = Window/360*Math.PI;
            double x0 = canvas.ActualWidth/2;
            double y0 = canvas.ActualHeight;
            while (i < ShortTicks.Count + MediumTicks.Count + LongTicks.Count)
            {
                Line line;
                if (i < lines.Count)
                {
                    line = lines[i];
                }
                else
                {
                    line = new Line() {Stroke = tickBrush, StrokeThickness = 1};
                    lines.Add(line);
                }
                if (i<ShortTicks.Count)
                {
                    double angle = windowAngle*(ShortTicks[i]*2-1);
                    line.X1 = x0+radius*ShortPosition1*Math.Sin(angle);
                    line.X2 = x0+radius*ShortPosition2*Math.Sin(angle);
                    line.Y1 = y0-radius*ShortPosition1*Math.Cos(angle);
                    line.Y2 = y0-radius*ShortPosition2*Math.Cos(angle);
                }
                else if (i<ShortTicks.Count+MediumTicks.Count)
                {
                    int ii = i - ShortTicks.Count;
                    double angle = windowAngle*(MediumTicks[ii]*2-1);
                    line.X1 = x0+radius*MediumPosition1*Math.Sin(angle);
                    line.X2 = x0+radius*MediumPosition2*Math.Sin(angle);
                    line.Y1 = y0-radius*MediumPosition1*Math.Cos(angle);
                    line.Y2 = y0-radius*MediumPosition2*Math.Cos(angle);
                }
                else if (i<ShortTicks.Count+MediumTicks.Count+LongTicks.Count)
                {
                    int ii = i - ShortTicks.Count - MediumTicks.Count;
                    double angle = windowAngle*(LongTicks[ii]*2-1);
                    line.X1 = x0+radius*LongPosition1*Math.Sin(angle);
                    line.X2 = x0+radius*LongPosition2*Math.Sin(angle);
                    line.Y1 = y0-radius*LongPosition1*Math.Cos(angle);
                    line.Y2 = y0-radius*LongPosition2*Math.Cos(angle);
                }
                if (!canvas.Children.Contains(line))
                {
                    canvas.Children.Add(line);
                }
                i++;
            }
            while (i < lines.Count)
            {
                if (canvas.Children.Contains(lines[i]))
                {
                    canvas.Children.Remove(lines[i]);
                }
                i++;
            }
            i = 0;
            while (i < LongTicks.Count)
            {
                TextBlock label;
                if (i < labels.Count)
                {
                    label = labels[i];
                    label.Text = LongTickValues[i].ToString();
                }
                else
                {
                    label = new TextBlock();
                    labels.Add(label);
                    label.Foreground = tickBrush;
                    label.Text = LongTickValues[i].ToString();
                }
                double angle = windowAngle*(LongTicks[i]*2-1);
                Canvas.SetLeft(label, x0+radius*LabelPosition*Math.Sin(angle)-label.ActualHeight/2);
                Canvas.SetTop(label, y0-radius*LabelPosition*Math.Cos(angle)-label.ActualWidth/2);
                if (!canvas.Children.Contains(label))
                {
                    canvas.Children.Add(label);
                }
                i++;
            }
            while (i < labels.Count)
            {
                if (canvas.Children.Contains(labels[i]))
                {
                    canvas.Children.Remove(labels[i]);
                }
                i++;
            }
        }
        public void DrawPointer()
        {
            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            var top = new Point(w/2, h*PointerTop);
            var bottomLeft = new Point(w*(1-PointerWidth)/2, h*(PointerTop+PointerHeight));
            var bottomRight = new Point(w*(1+PointerWidth)/2, h*(PointerTop+PointerHeight));
            if (!canvas.Children.Contains(pointer))
            {
                pointer = new Polygon() {Fill=tickBrush};
                pointer.Points.Add(top);
                pointer.Points.Add(bottomLeft);
                pointer.Points.Add(bottomRight);
                canvas.Children.Add(pointer);
            }
            else
            {
                pointer.Points[0] = top;
                pointer.Points[1] = bottomLeft;
                pointer.Points[2] = bottomRight;
            }
        }
        public void DrawLabelBackground(bool direction)
        {
            labelEnabled = true;
            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            double labelTop = 0.5-LabelHeight/2;
            double labelBottom = 0.5+LabelHeight/2;
            var pointInfo = new List<(double, double)>() 
            {
                (0.01, labelTop), (0.93, labelTop), (0.93, 0.48),
                (0.99, 0.5), (0.93, 0.52), (0.93, labelBottom),
                (0.01, labelBottom)
            };
            if (!direction)
            {
                pointInfo = pointInfo.ConvertAll(p => (1-p.Item1, p.Item2));
            }
            var points = pointInfo.ConvertAll(p => new Point(w*p.Item1, h*p.Item2));
            if (!canvas.Children.Contains(labelBackground))
            {
                labelBackground = new Polygon() 
                {
                    Stroke = tickBrush,
                    StrokeThickness = 2
                };
                foreach (var point in points)
                {
                    labelBackground.Points.Add(point);
                }
                canvas.Children.Add(labelBackground);
                MainLabel = new TextBlock();
                MainLabel.Foreground = tickBrush;
                canvas.Children.Add(MainLabel);
                UpperLabel = new TextBlock();
                UpperLabel.Foreground = tickBrush;
                canvas.Children.Add(UpperLabel);
                LowerLabel = new TextBlock();
                LowerLabel.Foreground = tickBrush;
                canvas.Children.Add(LowerLabel);
            }
            else
            {
                for (int i=0; i<points.Count; i++)
                {
                    labelBackground.Points[i] = points[i];
                }
            }
            UpdateLabel();
        }
        public void UpdateLabel()
        {
            if (!labelEnabled) return;
            int b = 10*LabelPrecision;
            int v = (int) CurrentValue/LabelPrecision*LabelPrecision;
            int m = (int)(CurrentValue/b+0.05);
            int l = v-v/b*b;
            int u = (l+LabelPrecision)%b;
            double r = (CurrentValue-v)/LabelPrecision;
            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            int bias = (u<0 || m<0 || l<0) ? 1 : 0;
            var mText = ((u<0 || m<0 || l<0)?"-":"")+Math.Abs(m).ToString();
            MainLabel.Text = mText.PadLeft(4);
            UpperLabel.Text = u==0?LabelZero:Math.Abs(u).ToString();
            LowerLabel.Text = v==0?LabelZero:Math.Abs(l).ToString();
            Canvas.SetTop(MainLabel, h/2-MainLabel.ActualHeight/2);
            Canvas.SetLeft(MainLabel, w*LabelTextLeft);
            Canvas.SetTop(UpperLabel, h/2+UpperLabel.ActualHeight*(-1.5+r+bias));
            Canvas.SetLeft(UpperLabel, w*LabelTextLeft+MainLabel.ActualWidth);
            Canvas.SetTop(LowerLabel, h/2+UpperLabel.ActualHeight*(-0.5+r+bias));
            Canvas.SetLeft(LowerLabel, w*LabelTextLeft+MainLabel.ActualWidth);
        }
        public void DrawBetaSlider()
        {
            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            double interval = 0.005;
            double height = 0.01;
            double top = PointerTop+PointerHeight+interval;
            double topwidth = PointerWidth*(1+interval/PointerHeight);
            double bottomwidth = PointerWidth*(1+(interval+height)/PointerHeight);
            double bias = BetaValue*topwidth/Math.PI*6;
            var topLeft = new Point(w*(1+topwidth+bias)/2, h*top);
            var topRight = new Point(w*(1-topwidth+bias)/2, h*top);
            var bottomLeft = new Point(w*(1-bottomwidth+bias)/2, h*(top+height));
            var bottomRight = new Point(w*(1+bottomwidth+bias)/2, h*(top+height));
            if (!canvas.Children.Contains(slider))
            {
                slider = new Polygon() {Fill=tickBrush};
                slider.Points.Add(topLeft);
                slider.Points.Add(topRight);
                slider.Points.Add(bottomLeft);
                slider.Points.Add(bottomRight);
                canvas.Children.Add(slider);
            }
            else
            {
                slider.Points[0] = topLeft;
                slider.Points[1] = topRight;
                slider.Points[2] = bottomLeft;
                slider.Points[3] = bottomRight;
            }
        }
        public void DrawFlightPath()
        {
            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            double fpa = CurrentValue - AlphaValue*180/Math.PI;
            double beta = BetaValue*180/Math.PI;
            double x = 0.5+beta/Window;
            double y = 0.5+fpa/Window;
            double width = 0.03;
            double height = 0.04;
            var top = new Point(w*x, h*(y-height));
            var right = new Point(w*(x+width), h*y);
            var bottom = new Point(w*x, h*(y+height));
            var left = new Point(w*(x-width), h*y);
            if (!canvas.Children.Contains(flightPath))
            {
                flightPath = new Polygon() 
                {
                    Stroke = tickBrush,
                    StrokeThickness = 1
                };
                flightPath.Points.Add(top);
                flightPath.Points.Add(right);
                flightPath.Points.Add(bottom);
                flightPath.Points.Add(left);
                canvas.Children.Add(flightPath);
            }
            else
            {
                flightPath.Points[0] = top;
                flightPath.Points[1] = right;
                flightPath.Points[2] = bottom;
                flightPath.Points[3] = left;
            }
        }
    }
}