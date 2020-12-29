using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using System;
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
        private bool running = true;
        public Subscriber()
        {
            Id = count;
            count++;
        }
        public virtual void Stop()
        {
            running = false;
        }
        public async void Serve()
        {
            running = true;
            EndPoint senderRemote = (EndPoint)endPoint;
            try
            {
                socket.Bind(endPoint);
            }
            catch(SocketException e)
            {
                throw new LigralException($"Address: {endPoint.Address} port: {endPoint.Port}.\n{e.Message}");
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
                    Receive(figureConfigPacket.Data);
                    break;
                case FigureProtocol.PlotConfigLabel:
                    var plotConfigPacket = JsonSerializer.Deserialize<Packet<FigureProtocol.PlotConfig>>(packetString);
                    Receive(plotConfigPacket.Data);
                    break;
                case FigureProtocol.ShowCommandLabel:
                    var showCommandPacket = JsonSerializer.Deserialize<Packet<FigureProtocol.ShowCommand>>(packetString);
                    Receive(showCommandPacket.Data);
                    break;
                case FigureProtocol.DataFileLabel:
                    var dataFilePacket = JsonSerializer.Deserialize<Packet<FigureProtocol.DataFile>>(packetString);
                    Receive(dataFilePacket.Data);
                    break;
                case FigureProtocol.DataLabel:
                    var dataPacket = JsonSerializer.Deserialize<Packet<FigureProtocol.Data>>(packetString);
                    Receive(dataPacket.Data);
                    break;
                case FigureProtocol.CurveLabel:
                    var curvePacket = JsonSerializer.Deserialize<Packet<FigureProtocol.Curve>>(packetString);
                    Receive(curvePacket.Data);
                    break;
                }
            }
        }
        public bool Invoke<T>(T dataPacket) where T: struct
        {
            switch (dataPacket)
            {
            case FigureProtocol.FigureConfig figureConfig:
                Receive(figureConfig);
                return true;
            case FigureProtocol.PlotConfig plotConfig:
                Receive(plotConfig);
                return true;
            case FigureProtocol.ShowCommand showCommand:
                Receive(showCommand);
                return true;
            case FigureProtocol.DataFile dataFile:
                Receive(dataFile);
                return true;
            case FigureProtocol.Data data:
                Receive(data);
                return true;
            case FigureProtocol.Curve curve:
                Receive(curve);
                return true;
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