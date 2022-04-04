using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Cockpit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
    public partial class MainWindow : Window
    {
        private bool stickHold = false;
        private DispatcherTimer timer;
        private Packet packet;
        private KeyMouseInfo info;
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static IPAddress address = IPAddress.Parse("127.0.0.1");
        static IPEndPoint endPoint = new IPEndPoint(address, 8783);
        private Line xStar;
        private Line yStar;
        private Brush starBrush = new SolidColorBrush() { Color = Colors.Black };
        private Brush disableBrush = new SolidColorBrush() { Color = Colors.Gray };
        private Brush activeBrush = new SolidColorBrush() { Color = Colors.White };
        private Brush disableBackground = new SolidColorBrush() { Color = Colors.White };
        private Brush activeBackground = new SolidColorBrush() { Color = Colors.Green };
        public MainWindow()
        {
            InitializeComponent();
            KeyInput.Focus();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Tick += OnDraw;
            timer.Tick += OnStickReturn;
            timer.Tick += OnUDPSend;
            timer.Start();
            packet = new Packet();
            packet.Label = 0xffb0;
            info = new KeyMouseInfo();
            packet.Data = info;
            InitiateCanvas();
        }
        private void InitiateCanvas()
        {
            xStar = new Line() 
            {
                X1 = StickContainer.ActualWidth/2 - 5,
                Y1 = StickContainer.ActualHeight/2,
                X2 = StickContainer.ActualWidth/2 + 5,
                Y2 = StickContainer.ActualHeight/2,
                Stroke = starBrush,
                StrokeThickness = 1
            };
            yStar = new Line()
            {
                X1 = StickContainer.ActualWidth/2,
                Y1 = StickContainer.ActualHeight/2 - 5,
                X2 = StickContainer.ActualWidth/2,
                Y2 = StickContainer.ActualHeight/2 + 5,
                Stroke = starBrush,
                StrokeThickness = 1
            };
            StickContainer.Children.Add(xStar);
            StickContainer.Children.Add(yStar);
        }
        private void OnDraw(object sender, EventArgs e)
        {
            xStar.X1 = StickContainer.ActualWidth*(info.x+1)/2 - 5;
            xStar.Y1 = StickContainer.ActualHeight*(info.y+1)/2;
            xStar.X2 = StickContainer.ActualWidth*(info.x+1)/2 + 5;
            xStar.Y2 = StickContainer.ActualHeight*(info.y+1)/2;
            yStar.X1 = StickContainer.ActualWidth*(info.x+1)/2;
            yStar.Y1 = StickContainer.ActualHeight*(info.y+1)/2 - 5;
            yStar.X2 = StickContainer.ActualWidth*(info.x+1)/2;
            yStar.Y2 = StickContainer.ActualHeight*(info.y+1)/2 + 5;
            ZBar.Height = info.z*ZBarContainer.ActualHeight;
            XYStatus.Text = $"x:{info.x:0.00} y:{info.y:0.00} z:{info.z:0.00}";
            HStatus.Background = info.H!=0 ? activeBackground : disableBackground;
            HStatus.Foreground = info.H!=0 ? activeBrush : disableBrush;
            JStatus.Background = info.J!=0 ? activeBackground : disableBackground;
            JStatus.Foreground = info.J!=0 ? activeBrush : disableBrush;
            KStatus.Background = info.K!=0 ? activeBackground : disableBackground;
            KStatus.Foreground = info.K!=0 ? activeBrush : disableBrush;
            LStatus.Background = info.L!=0 ? activeBackground : disableBackground;
            LStatus.Foreground = info.L!=0 ? activeBrush : disableBrush;
            WStatus.Background = info.W!=0 ? activeBackground : disableBackground;
            WStatus.Foreground = info.W!=0 ? activeBrush : disableBrush;
            AStatus.Background = info.A!=0 ? activeBackground : disableBackground;
            AStatus.Foreground = info.A!=0 ? activeBrush : disableBrush;
            SStatus.Background = info.S!=0 ? activeBackground : disableBackground;
            SStatus.Foreground = info.S!=0 ? activeBrush : disableBrush;
            DStatus.Background = info.D!=0 ? activeBackground : disableBackground;
            DStatus.Foreground = info.D!=0 ? activeBrush : disableBrush;
            TFC0Status.Background = info.TFC==0 ? activeBackground : disableBackground;
            TFC0Status.Foreground = info.TFC==0 ? activeBrush : disableBrush;
            TFC1Status.Background = info.TFC==1 ? activeBackground : disableBackground;
            TFC1Status.Foreground = info.TFC==1 ? activeBrush : disableBrush;
            TFC2Status.Background = info.TFC==2 ? activeBackground : disableBackground;
            TFC2Status.Foreground = info.TFC==2 ? activeBrush : disableBrush;
            YGV0Status.Background = info.YGV==0 ? activeBackground : disableBackground;
            YGV0Status.Foreground = info.YGV==0 ? activeBrush : disableBrush;
            YGV1Status.Background = info.YGV==1 ? activeBackground : disableBackground;
            YGV1Status.Foreground = info.YGV==1 ? activeBrush : disableBrush;
            YGV2Status.Background = info.YGV==2 ? activeBackground : disableBackground;
            YGV2Status.Foreground = info.YGV==2 ? activeBrush : disableBrush;
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
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (stickHold)
            {
                var element = sender as Canvas;
                var p = e.GetPosition(element);
                info.x = p.X/element.ActualWidth*2-1;
                info.y = p.Y/element.ActualHeight*2-1;
            }
        }
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            info.z += e.Delta * 0.0001;
            if (info.z>1) info.z = 1;
            else if (info.z<0) info.z = 0;
        }
        private void OnKeyInputLostFocus(object sender, RoutedEventArgs e)
        {
            (sender as UIElement).Focus();
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
            case Key.LeftCtrl:
            case Key.RightCtrl:
                stickHold = true;
                break;
            case Key.A:
                info.A = 1;
                break;
            case Key.S:
                info.S = 1;
                break;
            case Key.W:
                info.W = 1;
                break;
            case Key.D:
                info.D = 1;
                break;
            case Key.H:
                info.H = ~info.H;
                break;
            case Key.J:
                info.J = ~info.J;
                break;
            case Key.K:
                info.K = ~info.K;
                break;
            case Key.L:
                info.L = ~info.L;
                break;
            case Key.Y:
                info.YGV = 0;
                break;
            case Key.G:
                info.YGV = 1;
                break;
            case Key.V:
                info.YGV = 2;
                break;
            case Key.T:
                info.TFC = 0;
                break;
            case Key.F:
                info.TFC = 1;
                break;
            case Key.C:
                info.TFC = 2;
                break;
            }
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
            case Key.LeftCtrl:
            case Key.RightCtrl:
                stickHold = false;
                break;
            case Key.A:
                info.A = 0;
                break;
            case Key.S:
                info.S = 0;
                break;
            case Key.W:
                info.W = 0;
                break;
            case Key.D:
                info.D = 0;
                break;
            }
        }
    }
}
