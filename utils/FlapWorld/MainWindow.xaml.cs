using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlapWorld
{
    struct Packet
    {
        [JsonPropertyName("label")]
        public int Label {get; set;}
        [JsonPropertyName("data")]
        public Pose Data { get; set; }
    }
    struct Pose
    {
        [JsonPropertyName("x")]
        public double X {get; set;}
        [JsonPropertyName("y")]
        public double Y {get; set;}
        [JsonPropertyName("theta")]
        public double Theta {get; set;}
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double dpm = 2;
        private double xm = 10;
        private double ym = 100;
        private double x0m = 0;
        private double y0m = 0;
        private Brush gridBrush = new SolidColorBrush() { Color = Colors.Gray };
        private Brush bodyBrush = new SolidColorBrush() { Color = Colors.Red };
        private bool running = false;
        private ThreadStart threadStart;
        private Thread thread;
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPAddress address;
        IPEndPoint endPoint;
        private DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 100) };
        public MainWindow()
        {
            InitializeComponent();
            address = IPAddress.Parse("127.0.0.1");
            endPoint = new IPEndPoint(address, 8784);
            threadStart = new ThreadStart(Serve);
            thread = new Thread(threadStart);
            thread.Start();
            timer.Tick +=new EventHandler(Timer_Tick);
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            Draw();
        }
        public void Serve()
        {
            if (running) return;
            running = true;
            EndPoint senderRemote = (EndPoint)endPoint;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1);
            try
            {
                socket.Bind(endPoint);
            }
            catch(SocketException)
            {
                
            }
            while (running)
            {
                byte[] buffer = new byte[1024];
                try
                {
                    socket.ReceiveFrom(buffer, ref senderRemote);
                }
                catch (SocketException)
                {
                    continue;
                }
                string packetString = Encoding.UTF8.GetString(buffer.TakeWhile(x => x != 0).ToArray());
                if (packetString.Length == 0) continue;
                Packet packet = JsonSerializer.Deserialize<Packet>(packetString);
                xm = packet.Data.X;
                ym = packet.Data.Y;
            }
        }

        private void Draw()
        {
            WorldCanvas.Children.Clear();
            double wd = WorldCanvas.ActualWidth;
            double hd = WorldCanvas.ActualHeight;
            double wm = wd / dpm;
            double hm = hd / dpm;
            if ((x0m - xm) > wm * 0.4) x0m = xm + wm * 0.4;
            if ((x0m - xm) < -wm * 0.4) x0m = xm - wm * 0.4;
            if ((y0m - ym) > hm * 0.4) y0m = ym + hm * 0.4;
            if ((y0m - ym) < -hm * 0.4) y0m = ym - hm * 0.4;
            double x0d = - x0m * dpm + wd / 2;
            double y0d = - y0m * dpm + hd / 2;
            double xd = x0d;
            double yd = y0d;
            int iw = (int) Math.Log10(wm);
            int ih = (int) Math.Log10(hm);
            int ind = Math.Min(iw, ih);
            double gm = Math.Pow(10, ind);
            double gd = gm * dpm;
            while (Math.Max(wd / gd, hd / gd) > 6) gd *= 2;
            while (Math.Min(wd / gd, hd / gd) < 3) gd /= 2;
            while (xd - gd > 0) xd -= gd;
            while (yd - gd > 0) yd -= gd;
            while (xd < 0) xd += gd;
            while (yd < 0) yd += gd;
            while (yd < hd)
            {
                Line line = new Line()
                {
                    X1 = 0,
                    Y1 = yd,
                    X2 = wd,
                    Y2 = yd,
                    Stroke = gridBrush,
                    StrokeThickness = 1
                };
                WorldCanvas.Children.Add(line);
                yd += gd;
            }
            while (xd < wd)
            {
                Line line = new Line()
                {
                    X1 = xd,
                    Y1 = 0,
                    X2 = xd,
                    Y2 = hd,
                    Stroke = gridBrush,
                    StrokeThickness = 1
                };
                WorldCanvas.Children.Add(line);
                xd += gd;
            }
            Line body = new Line()
            {
                X1 = xm * dpm + x0d,
                Y1 = ym * dpm + y0d,
                X2 = xm * dpm + x0d - 5,
                Y2 = ym * dpm + y0d - 5,
                Stroke = bodyBrush,
                StrokeThickness = 1
            };
            WorldCanvas.Children.Add(body);
        }

        private void WorldCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Draw();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw();
        }

        private void WorldCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) dpm *= 1.01;
            else if (e.Delta < 0) dpm /= 1.01;
            else return;
            Draw();
        }
    }
}
