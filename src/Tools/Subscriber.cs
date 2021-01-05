/* Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Ligral.Tools.Protocols;

namespace Ligral.Tools
{
    struct PacketLabel
    {
        [JsonPropertyName("label")]
        public int Label {get; set;}
    }
    class Subscriber
    {
        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static IPAddress address = IPAddress.Parse(Settings.GetInstance().IPAddress);
        static IPEndPoint endPoint = new IPEndPoint(address, Settings.GetInstance().Port);
        private static int count = 0;
        public int Id;
        private static bool running = true;
        protected static List<Subscriber> subscribers = new List<Subscriber>();
        protected Logger logger;
        private static Logger subscriberLogger = new Logger("Subscriber");
        public Subscriber()
        {
            Id = count;
            count++;
            subscribers.Add(this);
            logger = new Logger(GetType().Name);
        }
        public static void Stop()
        {
            running = false;
            if (socket.IsBound)
            {
                socket.Shutdown(SocketShutdown.Receive);
                socket.Close();
            }
        }
        public virtual void Unsubscribe()
        {
            if (subscribers.Contains(this))
            {
                subscribers.Remove(this);
            }
            if (subscribers.Count == 0)
            {
                Stop();
            }
        }
        public static async void Serve()
        {
            if (running) return;
            running = true;
            EndPoint senderRemote = (EndPoint)endPoint;
            try
            {
                socket.Bind(endPoint);
            }
            catch(SocketException e)
            {
                throw subscriberLogger.Error(new LigralException($"Address: {endPoint.Address} port: {endPoint.Port}.\n{e.Message}"));
            }
            while (running)
            {
                byte[] buffer = new byte[1024];
                await Task.Run(()=>socket.ReceiveFrom(buffer, ref senderRemote));
                string packetString = Encoding.UTF8.GetString(buffer.TakeWhile(x => x != 0).ToArray());
                PacketLabel packetLabel = JsonSerializer.Deserialize<PacketLabel>(packetString);
                switch (packetLabel.Label)
                {
                case FigureProtocol.FigureConfigLabel:
                    var figureConfigPacket = JsonSerializer.Deserialize<Packet<FigureProtocol.FigureConfig>>(packetString);
                    foreach (Subscriber subscriber in subscribers)
                    {
                        if (subscriber.Receive(figureConfigPacket.Data))
                        {
                            break;
                        }
                    }
                    break;
                case FigureProtocol.PlotConfigLabel:
                    var plotConfigPacket = JsonSerializer.Deserialize<Packet<FigureProtocol.PlotConfig>>(packetString);
                    foreach (Subscriber subscriber in subscribers)
                    {
                        if (subscriber.Receive(plotConfigPacket.Data))
                        {
                            break;
                        }
                    };
                    break;
                case FigureProtocol.ShowCommandLabel:
                    var showCommandPacket = JsonSerializer.Deserialize<Packet<FigureProtocol.ShowCommand>>(packetString);
                    foreach (Subscriber subscriber in subscribers)
                    {
                        if (subscriber.Receive(showCommandPacket.Data))
                        {
                            break;
                        }
                    };
                    break;
                case FigureProtocol.DataFileLabel:
                    var dataFilePacket = JsonSerializer.Deserialize<Packet<FigureProtocol.DataFile>>(packetString);
                    foreach (Subscriber subscriber in subscribers)
                    {
                        if (subscriber.Receive(dataFilePacket.Data))
                        {
                            break;
                        }
                    };
                    break;
                case FigureProtocol.DataLabel:
                    var dataPacket = JsonSerializer.Deserialize<Packet<FigureProtocol.Data>>(packetString);
                    foreach (Subscriber subscriber in subscribers)
                    {
                        if (subscriber.Receive(dataPacket.Data))
                        {
                            break;
                        }
                    };
                    break;
                case FigureProtocol.CurveLabel:
                    var curvePacket = JsonSerializer.Deserialize<Packet<FigureProtocol.Curve>>(packetString);
                    foreach (Subscriber subscriber in subscribers)
                    {
                        if (subscriber.Receive(curvePacket.Data))
                        {
                            break;
                        }
                    };
                    break;
                }
            }
        }
        public bool Invoke<T>(T dataPacket) where T: struct
        {
            switch (dataPacket)
            {
            case FigureProtocol.FigureConfig figureConfig:
                return Receive(figureConfig);
            case FigureProtocol.PlotConfig plotConfig:
                return Receive(plotConfig);
            case FigureProtocol.ShowCommand showCommand:
                return Receive(showCommand);
            case FigureProtocol.DataFile dataFile:
                return Receive(dataFile);
            case FigureProtocol.Data data:
                return Receive(data);
            case FigureProtocol.Curve curve:
                return Receive(curve);
            default:
                return false;
            }
        }
        protected virtual bool Receive(FigureProtocol.FigureConfig figureConfig)
        {
            return false;
        }
        protected virtual bool Receive(FigureProtocol.PlotConfig plotConfig)
        {
            return false;
        }
        protected virtual bool Receive(FigureProtocol.ShowCommand showCommand)
        {
            return false;
        }
        protected virtual bool Receive(FigureProtocol.DataFile dataFile)
        {
            return false;
        }
        protected virtual bool Receive(FigureProtocol.Data data)
        {
            return false;
        }
        protected virtual bool Receive(FigureProtocol.Curve curve)
        {
            return false;
        }
    }
}