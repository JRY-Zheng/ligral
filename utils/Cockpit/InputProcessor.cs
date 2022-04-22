using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace Cockpit
{
    class KeyMouseInfo
    {
        [JsonPropertyName("roll")]
        public double roll {get; set;}
        [JsonPropertyName("pitch")]
        public double pitch {get; set;}
        [JsonPropertyName("yaw")]
        public double yaw {get; set;}
        [JsonPropertyName("throttle")]
        public double throttle {get; set;}
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
        [JsonPropertyName("SW1")]
        public int SW1 {get; set;}
        [JsonPropertyName("SW2")]
        public int SW2 {get; set;}
    }
    class Packet<T>
    {
        [JsonPropertyName("label")]
        public int Label {get; set;}
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
    class InputProcessor
    {
        public bool stickHold = false;
        private int yawHold = 0;
        private Packet<KeyMouseInfo> packet;
        private KeyMouseInfo info;
        private Line xStar;
        private Line yStar;
        private Brush starBrush = new SolidColorBrush() { Color = Colors.Black };
        private Brush disableBrush = new SolidColorBrush() { Color = Colors.Gray };
        private Brush activeBrush = new SolidColorBrush() { Color = Colors.White };
        private Brush disableBackground = new SolidColorBrush() { Color = Colors.White };
        private Brush activeBackground = new SolidColorBrush() { Color = Colors.Lime };
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static IPAddress address = IPAddress.Parse("127.0.0.1");
        static IPEndPoint endPoint = new IPEndPoint(address, 8783);
        private MainWindow f;
        public InputProcessor(MainWindow window)
        {
            f = window;
            f.KeyInput.Focus();
            f.RegisterPeriodicTask(OnDraw);
            f.RegisterPeriodicTask(OnStickReturn);
            f.RegisterPeriodicTask(OnUDPSend);
            packet = new Packet<KeyMouseInfo>();
            packet.Label = 0xffb0;
            info = new KeyMouseInfo();
            packet.Data = info;
            f.RegisterInitialTask(InitiateCanvas);
        }
        private void InitiateCanvas(object sender, RoutedEventArgs e)
        {
            double w = f.PrimaryDisplay.ActualWidth;
            double h = f.PrimaryDisplay.ActualHeight;
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
            f.PrimaryDisplay.Children.Add(xStar);
            f.PrimaryDisplay.Children.Add(yStar);
        }
        private void OnDraw(object sender, EventArgs e)
        {
            double w = f.PrimaryDisplay.ActualWidth;
            double h = f.PrimaryDisplay.ActualHeight;
            xStar.X1 = w*(info.roll+1)/2 - 5;
            xStar.Y1 = h*(info.pitch+1)/2;
            xStar.X2 = w*(info.roll+1)/2 + 5;
            xStar.Y2 = h*(info.pitch+1)/2;
            yStar.X1 = w*(info.roll+1)/2;
            yStar.Y1 = h*(info.pitch+1)/2 - 5;
            yStar.X2 = w*(info.roll+1)/2;
            yStar.Y2 = h*(info.pitch+1)/2 + 5;
            f.ZBar.Height = info.throttle*f.ZBarContainer.ActualHeight;
            f.WBar.Width = Math.Abs(info.yaw)*f.WBarContainer.ActualWidth/2;
            f.WBarLeft.Width = info.yaw>0?f.WBarContainer.ActualWidth/2:f.WBarContainer.ActualWidth/2-f.WBar.Width;
            f.XYStatus.Text = $"x:{info.roll:0.00} p:{info.pitch:0.00} y:{info.yaw:0.00} t:{info.throttle:0.00}";
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
            f.TFC0Status.Background = info.SW2==0 ? activeBackground : disableBackground;
            f.TFC0Status.Foreground = info.SW2==0 ? activeBrush : disableBrush;
            f.TFC1Status.Background = info.SW2==1 ? activeBackground : disableBackground;
            f.TFC1Status.Foreground = info.SW2==1 ? activeBrush : disableBrush;
            f.TFC2Status.Background = info.SW2==2 ? activeBackground : disableBackground;
            f.TFC2Status.Foreground = info.SW2==2 ? activeBrush : disableBrush;
            f.YGV0Status.Background = info.SW1==0 ? activeBackground : disableBackground;
            f.YGV0Status.Foreground = info.SW1==0 ? activeBrush : disableBrush;
            f.YGV1Status.Background = info.SW1==1 ? activeBackground : disableBackground;
            f.YGV1Status.Foreground = info.SW1==1 ? activeBrush : disableBrush;
            f.YGV2Status.Background = info.SW1==2 ? activeBackground : disableBackground;
            f.YGV2Status.Foreground = info.SW1==2 ? activeBrush : disableBrush;
        }
        private void OnStickReturn(object sender, EventArgs e)
        {
            if (!stickHold)
            {
                info.roll *= 0.9;
                info.pitch *= 0.9;
            }
            if (yawHold != 0)
            {
                info.yaw += yawHold*0.05;
                if (info.yaw>1) info.yaw = 1;
                else if (info.yaw<-1) info.yaw = -1;
            }
            else
            {
                info.yaw *= 0.9;
            }
        }
        private void OnUDPSend(object sender, EventArgs e)
        {
            string packetString = JsonSerializer.Serialize<Packet<KeyMouseInfo>>(packet);
            byte[] packetBytes = Encoding.UTF8.GetBytes(packetString);
            socket.SendTo(packetBytes, endPoint);
        }
        public void OnMouseMove(double x, double y)
        {
            if (stickHold)
            {
                info.roll = x;
                info.pitch = y;
            }
        }
        public void OnMouseWheel(double dz)
        {
            info.throttle += dz;
            if (info.throttle>1) info.throttle = 1;
            else if (info.throttle<0) info.throttle = 0;
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
        public void OnLeftDown()
        {
            yawHold = -1;
        }
        public void OnRightDown()
        {
            yawHold = 1;
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
            info.SW1 = 0;
        }
        public void OnGDown()
        {
            info.SW1 = 1;
        }
        public void OnVDown()
        {
            info.SW1 = 2;
        }
        public void OnTDown()
        {
            info.SW2 = 0;
        }
        public void OnFDown()
        {
            info.SW2 = 1;
        }
        public void OnCDown()
        {
            info.SW2 = 2;
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
        public void OnLeftUp()
        {
            yawHold = 0;
        }
        public void OnRightUp()
        {
            yawHold = 0;
        }
    }
}