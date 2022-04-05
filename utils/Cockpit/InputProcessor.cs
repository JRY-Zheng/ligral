using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Cockpit
{
    class KeyMouseInfo
    {
        [JsonPropertyName("X")]
        public double x {get; set;}
        [JsonPropertyName("Y")]
        public double y {get; set;}
        [JsonPropertyName("ZZ")]
        public double z {get; set;}
        [JsonPropertyName("A")]
        public int A {get; set;}
        [JsonPropertyName("S")]
        public int S {get; set;}
        [JsonPropertyName("W")]
        public int W {get; set;}
        [JsonPropertyName("D")]
        public int D {get; set;}
        [JsonPropertyName("H")]
        public int H {get; set;}
        [JsonPropertyName("J")]
        public int J {get; set;}
        [JsonPropertyName("K")]
        public int K {get; set;}
        [JsonPropertyName("L")]
        public int L {get; set;}
        [JsonPropertyName("YGV")]
        public int YGV {get; set;}
        [JsonPropertyName("TFC")]
        public int TFC {get; set;}
    }
    class Packet
    {
        [JsonPropertyName("label")]
        public int Label {get; set;}
        [JsonPropertyName("data")]
        public KeyMouseInfo Data { get; set; }
    }
    class InputProcessor
    {
        public bool stickHold = false;
        private Packet packet;
        private KeyMouseInfo info;
        private Line xStar;
        private Line yStar;
        private Brush starBrush = new SolidColorBrush() { Color = Colors.Black };
        private Brush disableBrush = new SolidColorBrush() { Color = Colors.Gray };
        private Brush activeBrush = new SolidColorBrush() { Color = Colors.White };
        private Brush disableBackground = new SolidColorBrush() { Color = Colors.White };
        private Brush activeBackground = new SolidColorBrush() { Color = Colors.Green };
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static IPAddress address = IPAddress.Parse("127.0.0.1");
        private MainWindow f;
        public InputProcessor(MainWindow window)
        {
            f = window;
            f.KeyInput.Focus();
            f.Register(OnDraw);
            f.Register(OnStickReturn);
            // f.Register(OnUDPSend);
            packet = new Packet();
            packet.Label = 0xffb0;
            info = new KeyMouseInfo();
            packet.Data = info;
            InitiateCanvas();
        }
        static IPEndPoint endPoint = new IPEndPoint(address, 8783);
        private void InitiateCanvas()
        {
            double w = f.StickContainer.ActualWidth;
            double h = f.StickContainer.ActualHeight;
            xStar = new Line() 
            {
                X1 = w/2 - 5,
                Y1 = h/2,
                X2 = w/2 + 5,
                Y2 = h/2,
                Stroke = starBrush,
                StrokeThickness = 1
            };
            yStar = new Line()
            {
                X1 = w/2,
                Y1 = h/2 - 5,
                X2 = w/2,
                Y2 = h/2 + 5,
                Stroke = starBrush,
                StrokeThickness = 1
            };
            f.StickContainer.Children.Add(xStar);
            f.StickContainer.Children.Add(yStar);
        }
        private void OnDraw(object sender, EventArgs e)
        {
            double w = f.StickContainer.ActualWidth;
            double h = f.StickContainer.ActualHeight;
            xStar.X1 = w*(info.x+1)/2 - 5;
            xStar.Y1 = h*(info.y+1)/2;
            xStar.X2 = w*(info.x+1)/2 + 5;
            xStar.Y2 = h*(info.y+1)/2;
            yStar.X1 = w*(info.x+1)/2;
            yStar.Y1 = h*(info.y+1)/2 - 5;
            yStar.X2 = w*(info.x+1)/2;
            yStar.Y2 = h*(info.y+1)/2 + 5;
            f.ZBar.Height = info.z*f.ZBarContainer.ActualHeight;
            f.XYStatus.Text = $"x:{info.x:0.00} y:{info.y:0.00} z:{info.z:0.00}";
            f.HStatus.Background = info.H!=0 ? activeBackground : disableBackground;
            f.HStatus.Foreground = info.H!=0 ? activeBrush : disableBrush;
            f.JStatus.Background = info.J!=0 ? activeBackground : disableBackground;
            f.JStatus.Foreground = info.J!=0 ? activeBrush : disableBrush;
            f.KStatus.Background = info.K!=0 ? activeBackground : disableBackground;
            f.KStatus.Foreground = info.K!=0 ? activeBrush : disableBrush;
            f.LStatus.Background = info.L!=0 ? activeBackground : disableBackground;
            f.LStatus.Foreground = info.L!=0 ? activeBrush : disableBrush;
            f.WStatus.Background = info.W!=0 ? activeBackground : disableBackground;
            f.WStatus.Foreground = info.W!=0 ? activeBrush : disableBrush;
            f.AStatus.Background = info.A!=0 ? activeBackground : disableBackground;
            f.AStatus.Foreground = info.A!=0 ? activeBrush : disableBrush;
            f.SStatus.Background = info.S!=0 ? activeBackground : disableBackground;
            f.SStatus.Foreground = info.S!=0 ? activeBrush : disableBrush;
            f.DStatus.Background = info.D!=0 ? activeBackground : disableBackground;
            f.DStatus.Foreground = info.D!=0 ? activeBrush : disableBrush;
            f.TFC0Status.Background = info.TFC==0 ? activeBackground : disableBackground;
            f.TFC0Status.Foreground = info.TFC==0 ? activeBrush : disableBrush;
            f.TFC1Status.Background = info.TFC==1 ? activeBackground : disableBackground;
            f.TFC1Status.Foreground = info.TFC==1 ? activeBrush : disableBrush;
            f.TFC2Status.Background = info.TFC==2 ? activeBackground : disableBackground;
            f.TFC2Status.Foreground = info.TFC==2 ? activeBrush : disableBrush;
            f.YGV0Status.Background = info.YGV==0 ? activeBackground : disableBackground;
            f.YGV0Status.Foreground = info.YGV==0 ? activeBrush : disableBrush;
            f.YGV1Status.Background = info.YGV==1 ? activeBackground : disableBackground;
            f.YGV1Status.Foreground = info.YGV==1 ? activeBrush : disableBrush;
            f.YGV2Status.Background = info.YGV==2 ? activeBackground : disableBackground;
            f.YGV2Status.Foreground = info.YGV==2 ? activeBrush : disableBrush;
        }
        private void OnStickReturn(object sender, EventArgs e)
        {
            if (!stickHold)
            {
                info.x *= 0.9;
                info.y *= 0.9;
            }
        }
        private void OnUDPSend(object sender, EventArgs e)
        {
            string packetString = JsonSerializer.Serialize<Packet>(packet);
            byte[] packetBytes = Encoding.UTF8.GetBytes(packetString);
            socket.SendTo(packetBytes, endPoint);
        }
        public void OnMouseMove(double x, double y)
        {
            if (stickHold)
            {
                info.x = x;
                info.y = y;
            }
        }
        public void OnMouseWheel(double dz)
        {
            info.z += dz;
            if (info.z>1) info.z = 1;
            else if (info.z<0) info.z = 0;
        }
        public void OnCtrlDown()
        {
            stickHold = true;
        }
        public void OnADown()
        {
            info.A = 1;
        }
        public void OnSDown()
        {
            info.S = 1;
        }
        public void OnWDown()
        {
            info.W = 1;
        }
        public void OnDDown()
        {
            info.D = 1;
        }
        public void OnHDown()
        {
            info.H = ~info.H;
        }
        public void OnJDown()
        {
            info.J = ~info.J;
        }
        public void OnKDown()
        {
            info.K = ~info.K;
        }
        public void OnLDown()
        {
            info.L = ~info.L;
        }
        public void OnYDown()
        {
            info.YGV = 0;
        }
        public void OnGDown()
        {
            info.YGV = 1;
        }
        public void OnVDown()
        {
            info.YGV = 2;
        }
        public void OnTDown()
        {
            info.TFC = 0;
        }
        public void OnFDown()
        {
            info.TFC = 1;
        }
        public void OnCDown()
        {
            info.TFC = 2;
        }
        public void OnCtrlUp()
        {
            stickHold = false;
        }
        public void OnAUp()
        {
            info.A = 0;
        }
        public void OnSUp()
        {
            info.S = 0;
        }
        public void OnWUp()
        {
            info.W = 0;
        }
        public void OnDUp()
        {
            info.D = 0;
        }
    }
}