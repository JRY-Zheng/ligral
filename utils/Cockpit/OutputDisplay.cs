using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;

namespace Cockpit
{
    class AircraftInfo
    {
        [JsonPropertyName("theta")]
        public double theta {get; set;}
        [JsonPropertyName("phi")]
        public double phi {get; set;}
        [JsonPropertyName("psi")]
        public double psi {get; set;}
        [JsonPropertyName("alpha")]
        public double alpha {get; set;}
        [JsonPropertyName("beta")]
        public double beta {get; set;}
        [JsonPropertyName("V")]
        public double V {get; set;}
        [JsonPropertyName("h")]
        public double h {get; set;}
    }
    class OutputDisplay
    {
        private Canvas primaryDisplay;
        private MainWindow f;
        private Packet<AircraftInfo> packet;
        private AircraftInfo info;
        private Polygon land;
        private Polygon sky;
        public OutputDisplay(MainWindow window)
        {
            f = window;
            primaryDisplay = window.PrimaryDisplay;
            packet = new Packet<AircraftInfo>();
            packet.Label = 0xffb0;
            info = new AircraftInfo();
            packet.Data = info;
            InitiatePolygon();
            f.Register(OnDrawPolygon);
        }
        private Point GetPoint(double x, double y)
        {
            double w = primaryDisplay.ActualWidth;
            double h = primaryDisplay.ActualHeight;
            return new Point(x*w, y*h);
        }
        private void InitiatePolygon()
        {
            land = new Polygon();
            land.Fill = System.Windows.Media.Brushes.OrangeRed;
            land.StrokeThickness = 0;
            land.Points = new PointCollection();
            land.Points.Add(GetPoint(0, 0.5));
            land.Points.Add(GetPoint(1, 0.5));
            land.Points.Add(GetPoint(1, 1));
            land.Points.Add(GetPoint(0, 1));
            primaryDisplay.Children.Add(land);
            sky = new Polygon();
            sky.Fill = System.Windows.Media.Brushes.RoyalBlue;
            sky.StrokeThickness = 0;
            sky.Points = new PointCollection();
            sky.Points.Add(GetPoint(0, 0.5));
            sky.Points.Add(GetPoint(1, 0.5));
            sky.Points.Add(GetPoint(1, 0));
            sky.Points.Add(GetPoint(0, 0));
            primaryDisplay.Children.Add(sky);
        }
        private void OnDrawPolygon(object sender, EventArgs e)
        {
            double t = -info.theta/Math.PI*2+0.5;
            double yl = t - Math.Tan(info.phi)/2;
            double yr = 2*t-yl;
            land.Points[0] = GetPoint(0, yl);
            land.Points[1] = GetPoint(1, yr);
            land.Points[2] = GetPoint(1, 1);
            land.Points[3] = GetPoint(0, 1);
            sky.Points[0] = GetPoint(0, yl);
            sky.Points[1] = GetPoint(1, yr);
            sky.Points[2] = GetPoint(1, 0);
            sky.Points[3] = GetPoint(0, 0);
        }
    }
}