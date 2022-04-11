using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using System;

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
        public double LongPosition1 {get; set;} = 0.3;
        public double LongPosition2 {get; set;} = 0.9;
        public double MediumPosition1 {get; set;} = 0.25;
        public double MediumPosition2 {get; set;} = 0.75;
        public double ShortPosition1 {get; set;} = 0.3;
        public double ShortPosition2 {get; set;} = 0.7;
        public double LabelPosition {get; set;} = 0.05;
        private List<double> LongTicks = new List<double>();
        private List<double> MediumTicks = new List<double>();
        private List<double> ShortTicks = new List<double>();
        private List<double> LongTickValues = new List<double>();
        private List<Line> lines = new List<Line>();
        private List<TextBlock> labels = new List<TextBlock>();
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
    }
}