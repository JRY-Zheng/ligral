using System;
using System.Linq;
using System.Collections.Generic;
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
    struct Point3
    {
        public double X {get; set;}
        public double Y {get; set;}
        public double Z {get; set;}
        public Point3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
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
        private AircraftInfo info {get {return packet.Data;}}
        private Polygon land;
        private Polygon sky;
        private Polygon leftBar;
        private Polygon rightBar;
        private Canvas AirspeedIndicatorCanvas;
        private Indicator airspeedIndicator;
        private Indicator altimeter;
        private Indicator headingIndicator;
        private Indicator pitchIndicator;
        private Indicator rollIndicator;
        private Canvas AltimeterCanvas;
        private Canvas PitchIndicatorCanvas;
        private Canvas RollIndicatorCanvas;
        private Canvas HeadingIndicatorCanvas;
        private RotateTransform rollTransform = new RotateTransform();
        static Point Center = new Point(0, 0);
        static Point TopLeft = new Point(-1, -1);
        static Point TopRight = new Point(1, -1);
        static Point BottomLeft = new Point(-1, 1);
        static Point BottomRight = new Point(1, 1);
        static Point3 XAxis = new Point3(1, 0, 0);
        static Point3 YAxis = new Point3(0, 1, 0);
        static Point3 ZAxis = new Point3(0, 0, 1);
        // static double SkylineDistance = 1;
        static double SkylineHalfLength = 3;
        static Brush transparentBlack = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static IPAddress address = IPAddress.Parse("127.0.0.1");
        static IPEndPoint endPoint = new IPEndPoint(address, 8784);
        public OutputDisplay(MainWindow window)
        {
            f = window;
            primaryDisplay = window.PrimaryDisplay;
            packet = new Packet<AircraftInfo>();
            packet.Label = 0xffb0;
            packet.Data = new AircraftInfo();
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1);
            socket.Bind(endPoint);
            f.RegisterInitialTask(InitiatePolygon);
            f.RegisterInitialTask(InitiateCenterBar);
            f.RegisterEventTriggedTask(RedrawCenterBar);
            f.RegisterInitialTask(InitiateCanvas);
            f.RegisterEventTriggedTask(RedrawCanvas);
            f.RegisterPeriodicTask(OnDrawPolygon);
            f.RegisterPeriodicTask(OnUDPReceive);
            f.RegisterPeriodicTask(OnDrawIndicators);
        }
        private void OnUDPReceive(object sender, EventArgs e)
        {
            byte[] buffer = new byte[1024];
            EndPoint senderRemote = (EndPoint)endPoint;
            try
            {
                socket.ReceiveFrom(buffer, ref senderRemote);
            }
            catch (SocketException)
            {
                return;
            }
            string packetString = Encoding.UTF8.GetString(buffer.TakeWhile(x => x != 0).ToArray());
            if (packetString.Length == 0) return;
            var _packet = JsonSerializer.Deserialize<Packet<AircraftInfo>>(packetString);
            if (packet.Label == _packet.Label) packet.Data = _packet.Data;
        }
        private double GetRatio()
        {
            double w = primaryDisplay.ActualWidth;
            double h = primaryDisplay.ActualHeight;
            if (w>h) 
            {
                return h/w;
            }
            else
            {
                return w/h;
            }
        }
        private Point GetPoint(double x, double y)
        {
            double w = primaryDisplay.ActualWidth;
            double h = primaryDisplay.ActualHeight;
            if (w>h) 
            {
                return new Point((x+1)/2*w, (y+1)/2*w-(w-h)/2);
            }
            else
            {
                return new Point((x+1)/2*h-(h-w)/2, (y+1)/2*h);
            }
        }
        private Point GetPoint(Point p)
        {
            return GetPoint(p.X, p.Y);
        }
        private Point GetConservativePoint(double x, double y)
        {
            double w = primaryDisplay.ActualWidth;
            double h = primaryDisplay.ActualHeight;
            if (w>h) 
            {
                return new Point((x+1)/2*h+(w-h)/2, (y+1)/2*h);
            }
            else
            {
                return new Point((x+1)/2*w, (y+1)/2*w+(h-w)/2);
            }
        }
        private Point GetConservativePoint(Point p)
        {
            return GetConservativePoint(p.X, p.Y);
        }
        private Point? GetCrossing(Point p1, Point p2, Point pa, Point pb)
        {
            double num = p1.Y*p2.X-p1.X*p2.Y + pa.X*(p2.Y-p1.Y) + pa.Y*(p1.X-p2.X);
            double den = -((pa.Y-pb.Y)*(p2.X-p1.X) - (pa.X-pb.X)*(p2.Y-p1.Y));
            if (Math.Abs(num)<0.001) num = 0;
            double v = num/den;
            if (v<-0 || v>=1) return null;
            Point p = new Point(pa.X+(pb.X-pa.X)*v, pa.Y+(pb.Y-pa.Y)*v);
            if (Math.Abs(p.X-1)<0.001) p.X = 1;
            else if (Math.Abs(p.X+1)<0.001) p.X = -1;
            if (Math.Abs(p.Y-1)<0.001) p.Y = 1;
            else if (Math.Abs(p.Y+1)<0.001) p.Y = -1;
            return p;
        }
        private List<Point> GetCrossingOnBorder(Point pa, Point pb)
        {
            List<Point> points = new List<Point>();
            var p1 = GetCrossing(pa, pb, TopLeft, TopRight);
            if (p1 != null) points.Add((Point)p1);
            var p2 = GetCrossing(pa, pb, BottomLeft, TopLeft);
            if (p2 != null) points.Add((Point)p2);
            var p3 = GetCrossing(pa, pb, TopRight, BottomRight);
            if (p3 != null) points.Add((Point)p3);
            var p4 = GetCrossing(pa, pb, BottomRight, BottomLeft);
            if (p4 != null) points.Add((Point)p4);
            return points;
        }
        private Point GetCornerClockWise(Point p)
        {
            if (p.X<-1 || p.X>1 || p.Y<-1 || p.Y>1 || (Math.Abs(p.X)!=1 && Math.Abs(p.Y)!=1))
            {
                throw new InvalidOperationException("point not on the border");
            }
            if (p.X == 1 && p.Y != 1) return BottomRight;
            if (p.X != -1 && p.Y == 1) return BottomLeft;
            if (p.X == -1 && p.Y != -1) return TopLeft;
            if (p.X != 1 && p.Y == -1) return TopRight;
            throw new InvalidOperationException("Unknown error");
        }
        private Point BodyToEarth(Point p)
        {
            double x = p.X;
            double y = p.Y + info.theta;
            double cosphi = Math.Cos(info.phi);
            double sinphi = Math.Sin(info.phi);
            return new Point(x*cosphi+y*sinphi, -x*sinphi+y*cosphi);
        }
        private Point3 BodyToEarth(Point3 pe)
        {
            double costheta = Math.Cos(info.theta);
            double sintheta = Math.Sin(info.theta);
            double cosphi = Math.Cos(info.phi);
            double sinphi = Math.Sin(info.phi);
            double cospsi = Math.Cos(info.psi);
            double sinpsi = Math.Sin(info.psi);
            double m11 = costheta*cospsi;
            double m12 = costheta*sinpsi;
            double m13 = sintheta;
            double m21 = -cosphi*sinpsi+sinphi*sintheta*cospsi;
            double m22 = cosphi*cospsi+sinphi*sintheta*sinpsi;
            double m23 = sinphi*costheta;
            double m31 = sinphi*sinpsi+cosphi*sintheta*cospsi;
            double m32 = -sinphi*cospsi+cosphi*sintheta*sinpsi;
            double m33 = cosphi*costheta;
            double x = pe.X*m11+pe.Y*m12+pe.Z*m13;
            double y = pe.X*m21+pe.Y*m22+pe.Z*m23;
            double z = pe.X*m31+pe.Y*m32+pe.Z*m33;
            Point3 pb = new Point3(x, y, z);
            return pb;
        }
        private Point3 Yaw(Point3 p)
        {
            double cospsi = Math.Cos(info.psi);
            double sinpsi = Math.Sin(info.psi);
            double x = cospsi*p.X + -sinpsi*p.Y;
            double y = sinpsi*p.X + cospsi*p.Y;
            return new Point3(x, y, p.Z);
        }
        private bool IsLand(Point p, Point pl, Point pr)
        {
            double t1 = Math.Atan2((pr.Y - pl.Y), (pr.X - pl.X))*180/Math.PI;
            double t2 = Math.Atan2((p.Y - pl.Y), (p.X - pl.X))*180/Math.PI;
            double t = t2 - t1;
            return t < -180 || (t >= 0 && t < 180);
        }
        private void InitiatePolygon(object sender, RoutedEventArgs e)
        {
            land = new Polygon();
            land.Fill = System.Windows.Media.Brushes.OrangeRed;
            land.StrokeThickness = 0;
            land.Points = new PointCollection();
            land.Points.Add(GetPoint(-1, 0));
            land.Points.Add(GetPoint(1, 0));
            land.Points.Add(GetPoint(1, 1));
            land.Points.Add(GetPoint(-1, 1));
            primaryDisplay.Children.Add(land);
            sky = new Polygon();
            sky.Fill = System.Windows.Media.Brushes.RoyalBlue;
            sky.StrokeThickness = 0;
            sky.Points = new PointCollection();
            sky.Points.Add(GetPoint(-1, 0));
            sky.Points.Add(GetPoint(1, 0));
            sky.Points.Add(GetPoint(1, 0));
            sky.Points.Add(GetPoint(-1, 0));
            primaryDisplay.Children.Add(sky);
        }
        private void CopyPoints(PointCollection pFrom, PointCollection pTo)
        {
            while (pTo.Count > pFrom.Count)
            {
                pTo.RemoveAt(0);
            }
            int i = 0;
            for (; i<pTo.Count; i++)
            {
                pTo[i] = pFrom[i];
            }
            for (; i<pFrom.Count; i++)
            {
                pTo.Add(pFrom[i]);
            }
        }
        private void OnDrawPolygon(object sender, EventArgs e)
        {
            PointCollection landPC = new PointCollection();
            PointCollection skyPC = new PointCollection();
            // Point3 pl3 = new Point3(SkylineDistance, -SkylineHalfLength, 0);
            // Point3 pr3 = new Point3(SkylineDistance, SkylineHalfLength, 0);
            // pl3 = Yaw(pl3);
            // pr3 = Yaw(pr3);
            // pl3 = BodyToEarth(pl3);
            // pr3 = BodyToEarth(pr3);
            // Point pl = new Point(pl3.Y, pl3.Z);
            // Point pr = new Point(pr3.Y, pr3.Z);
            Point pl = new Point(-SkylineHalfLength, 0);
            Point pr = new Point(SkylineHalfLength, 0);
            pl = BodyToEarth(pl);
            pr = BodyToEarth(pr);
            var points = GetCrossingOnBorder(pl, pr);
            if (points.Count == 0)
            {
                if (IsLand(Center, pl, pr))
                {
                    landPC.Add(TopLeft);
                    landPC.Add(TopRight);
                    landPC.Add(BottomRight);
                    landPC.Add(BottomLeft);
                }
                else
                {
                    skyPC.Add(TopLeft);
                    skyPC.Add(TopRight);
                    skyPC.Add(BottomRight);
                    skyPC.Add(BottomLeft);
                }
            }
            else if (points.Count == 2)
            {
                Point corner1 = GetCornerClockWise(points[0]);
                Point corner2 = GetCornerClockWise(points[1]);
                bool corner1IsLand = IsLand(corner1, pl, pr);
                PointCollection pc1 = corner1IsLand ? landPC : skyPC;
                PointCollection pc2 = corner1IsLand ? skyPC : landPC;
                pc1.Add(points[0]);
                while (corner1 != corner2)
                {
                    pc1.Add(corner1);
                    corner1 = GetCornerClockWise(corner1);
                }
                pc1.Add(points[1]);
                pc2.Add(points[1]);
                while (pc1[1]!=corner2)
                {
                    pc2.Add(corner2);
                    corner2 = GetCornerClockWise(corner2);
                }
                pc2.Add(points[0]);
            }
            for (int i=0; i<landPC.Count; i++)
            {
                landPC[i] = GetPoint(landPC[i]);
            }
            for (int i=0; i<skyPC.Count; i++)
            {
                skyPC[i] = GetPoint(skyPC[i]);
            }
            CopyPoints(landPC, land.Points);
            CopyPoints(skyPC, sky.Points);
        }
        private void InitiateCenterBar(object sender, RoutedEventArgs e)
        {
            leftBar = new Polygon();
            leftBar.Fill = System.Windows.Media.Brushes.Black;
            leftBar.StrokeThickness = 0;
            leftBar.Points = new PointCollection();
            leftBar.Points.Add(GetConservativePoint(-0.5, 0));
            leftBar.Points.Add(GetConservativePoint(-0.2, 0));
            leftBar.Points.Add(GetConservativePoint(-0.2, 0.05));
            leftBar.Points.Add(GetConservativePoint(-0.21, 0.05));
            leftBar.Points.Add(GetConservativePoint(-0.21, 0.025));
            leftBar.Points.Add(GetConservativePoint(-0.5, 0.025));
            primaryDisplay.Children.Add(leftBar);
            rightBar = new Polygon();
            rightBar.Fill = System.Windows.Media.Brushes.Black;
            rightBar.StrokeThickness = 0;
            rightBar.Points = new PointCollection();
            rightBar.Points.Add(GetConservativePoint(0.5, 0));
            rightBar.Points.Add(GetConservativePoint(0.2, 0));
            rightBar.Points.Add(GetConservativePoint(0.2, 0.05));
            rightBar.Points.Add(GetConservativePoint(0.21, 0.05));
            rightBar.Points.Add(GetConservativePoint(0.21, 0.025));
            rightBar.Points.Add(GetConservativePoint(0.5, 0.025));
            primaryDisplay.Children.Add(rightBar);
        }
        private void RedrawCenterBar(object sender, SizeChangedEventArgs e)
        {
            if (!f.IsLoaded) return;
            leftBar.Points[0] = GetConservativePoint(-0.5, 0);
            leftBar.Points[1] = GetConservativePoint(-0.2, 0);
            leftBar.Points[2] = GetConservativePoint(-0.2, 0.05);
            leftBar.Points[3] = GetConservativePoint(-0.21, 0.05);
            leftBar.Points[4] = GetConservativePoint(-0.21, 0.025);
            leftBar.Points[5] = GetConservativePoint(-0.5, 0.025);
            rightBar.Points[0] = GetConservativePoint(0.5, 0);
            rightBar.Points[1] = GetConservativePoint(0.2, 0);
            rightBar.Points[2] = GetConservativePoint(0.2, 0.05);
            rightBar.Points[3] = GetConservativePoint(0.21, 0.05);
            rightBar.Points[4] = GetConservativePoint(0.21, 0.025);
            rightBar.Points[5] = GetConservativePoint(0.5, 0.025);
        }
        private void InitiateCanvas(object sender, RoutedEventArgs e)
        {
            AirspeedIndicatorCanvas = new Canvas();
            AirspeedIndicatorCanvas.Background = transparentBlack;
            primaryDisplay.Children.Add(AirspeedIndicatorCanvas);
            airspeedIndicator = new Indicator(AirspeedIndicatorCanvas, 500);
            AirspeedIndicatorCanvas.Loaded += (_, _) => airspeedIndicator.DrawLabelBackground(true);
            AltimeterCanvas = new Canvas();
            AltimeterCanvas.Background = transparentBlack;
            primaryDisplay.Children.Add(AltimeterCanvas);
            altimeter = new Indicator(AltimeterCanvas, 40000) 
            {
                LabelPosition = 0.6,
                LongPosition1 = 0.1,
                LongPosition2 = 0.6,
                Window = 10000,
                Interval = 2000,
                LabelPrecision = 100
            };
            AltimeterCanvas.Loaded += (_, _) => altimeter.DrawLabelBackground(false);
            PitchIndicatorCanvas = new Canvas();
            // PitchIndicator.Background = transparentBlack;
            primaryDisplay.Children.Add(PitchIndicatorCanvas);
            PitchIndicatorCanvas.RenderTransform = rollTransform;
            pitchIndicator = new Indicator(PitchIndicatorCanvas, 90, -90)
            {
                Window = 180/Math.PI*2*0.4*GetRatio(),
                Interval = 10,
                ShortPosition1 = 0.3,
                ShortPosition2 = 0.7,
                MediumPosition1 = 0.25,
                MediumPosition2 = 0.75,
                LongPosition1 = 0.2,
                LongPosition2 = 0.9,
                LabelPosition = 0.05
            };
            PitchIndicatorCanvas.Loaded += (_, _) => pitchIndicator.DrawPointer();
            RollIndicatorCanvas = new Canvas();
            // RollIndicator.Background = transparentBlack;
            primaryDisplay.Children.Add(RollIndicatorCanvas);
            rollIndicator = new Indicator(RollIndicatorCanvas, 360)
            {
                Window = 120,
                Interval = 30,
                ShortPosition1 = 0.88,
                ShortPosition2 = 0.9,
                MediumPosition1 = 0.85,
                MediumPosition2 = 0.9,
                LongPosition1 = 0.8,
                LongPosition2 = 0.9,
                LabelPosition = 0.95
            };
            rollIndicator.PeriodisedTicks();
            RollIndicatorCanvas.Loaded += (_, _) => rollIndicator.DrawRadiusTicks();
            HeadingIndicatorCanvas = new Canvas();
            // HeadingIndicatorCanvas.Background = transparentBlack;
            primaryDisplay.Children.Add(HeadingIndicatorCanvas);
            headingIndicator = new Indicator(HeadingIndicatorCanvas, 360)
            {
                Window = 120,
                Interval = 30,
                ShortPosition1 = 0.8,
                ShortPosition2 = 0.9,
                MediumPosition1 = 0.75,
                MediumPosition2 = 0.9,
                LongPosition1 = 0.7,
                LongPosition2 = 0.9,
                LabelPosition = 0.95,
                PointerTop = 0.3,
                PointerWidth = 0.04,
                PointerHeight = 0.08
            };
            HeadingIndicatorCanvas.Loaded += (_, _) => headingIndicator.DrawPointer();
            RedrawCanvas(null, null);
        }
        private void RedrawCanvas(object sender, SizeChangedEventArgs e)
        {
            if (!f.IsLoaded) return;
            Point topLeft = GetConservativePoint(-1,-0.8);
            Point bottonRight = GetConservativePoint(-0.65,0.8);
            Canvas.SetLeft(AirspeedIndicatorCanvas, topLeft.X);
            Canvas.SetTop(AirspeedIndicatorCanvas, topLeft.Y);
            AirspeedIndicatorCanvas.Width = bottonRight.X-topLeft.X;
            AirspeedIndicatorCanvas.Height = bottonRight.Y-topLeft.Y;
            if (AirspeedIndicatorCanvas.IsLoaded) airspeedIndicator.DrawLabelBackground(true);
            topLeft = GetConservativePoint(0.65,-0.8);
            bottonRight = GetConservativePoint(1,0.8);
            Canvas.SetLeft(AltimeterCanvas, topLeft.X);
            Canvas.SetTop(AltimeterCanvas, topLeft.Y);
            AltimeterCanvas.Width = bottonRight.X-topLeft.X;
            AltimeterCanvas.Height = bottonRight.Y-topLeft.Y;
            if (AltimeterCanvas.IsLoaded) altimeter.DrawLabelBackground(false);
            topLeft = GetConservativePoint(-0.4,-0.4);
            bottonRight = GetConservativePoint(0.4,0.4);
            Canvas.SetLeft(PitchIndicatorCanvas, topLeft.X);
            Canvas.SetTop(PitchIndicatorCanvas, topLeft.Y);
            PitchIndicatorCanvas.Width = bottonRight.X-topLeft.X;
            PitchIndicatorCanvas.Height = bottonRight.Y-topLeft.Y;
            if (PitchIndicatorCanvas.IsLoaded) pitchIndicator.DrawPointer();
            rollTransform.CenterX = PitchIndicatorCanvas.Width/2;
            rollTransform.CenterY = PitchIndicatorCanvas.Height/2;
            topLeft = GetConservativePoint(-0.4,-0.8);
            bottonRight = GetConservativePoint(0.4,0);
            Canvas.SetLeft(RollIndicatorCanvas, topLeft.X);
            Canvas.SetTop(RollIndicatorCanvas, topLeft.Y);
            RollIndicatorCanvas.Width = bottonRight.X-topLeft.X;
            RollIndicatorCanvas.Height = bottonRight.Y-topLeft.Y;
            if (RollIndicatorCanvas.IsLoaded) rollIndicator.DrawRadiusTicks();
            topLeft = GetConservativePoint(-0.6,0.7);
            bottonRight = GetConservativePoint(0.6,1.3);
            Canvas.SetLeft(HeadingIndicatorCanvas, topLeft.X);
            Canvas.SetTop(HeadingIndicatorCanvas, topLeft.Y);
            HeadingIndicatorCanvas.Width = bottonRight.X-topLeft.X;
            HeadingIndicatorCanvas.Height = bottonRight.Y-topLeft.Y;
            if (HeadingIndicatorCanvas.IsLoaded) headingIndicator.DrawPointer();
        }
        private void OnDrawIndicators(object sender, EventArgs e)
        {
            airspeedIndicator.CurrentValue = info.V;
            airspeedIndicator.NormalisedTicks();
            airspeedIndicator.DrawLinearTicks();
            airspeedIndicator.UpdateLabel();
            altimeter.CurrentValue = info.h;
            altimeter.NormalisedTicks();
            altimeter.DrawLinearTicks();
            altimeter.UpdateLabel();
            pitchIndicator.CurrentValue = info.theta*180/Math.PI;
            pitchIndicator.BetaValue = info.beta;
            pitchIndicator.AlphaValue = info.alpha;
            pitchIndicator.NormalisedTicks();
            pitchIndicator.DrawLinearTicks();
            pitchIndicator.DrawBetaSlider();
            pitchIndicator.DrawFlightPath();
            rollTransform.Angle = -info.phi*180/Math.PI;
            headingIndicator.CurrentValue = info.psi*180/Math.PI;
            headingIndicator.PeriodisedTicks();
            headingIndicator.DrawRadiusTicks();
        }
    }
}