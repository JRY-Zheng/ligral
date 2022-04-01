using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using Ligral.Tools;
using Ligral.Tools.Protocols;

namespace FireWaterPlotter
{
    class Program
    {
        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static IPAddress address = IPAddress.Parse("127.0.0.1");
        static IPEndPoint localEndPoint = new IPEndPoint(address, 8784);
        static IPEndPoint remoteEndPoint = new IPEndPoint(address, 8785);
        static Dictionary<int, int> curveMap = new Dictionary<int, int>();
        static List<double> vector = new List<double>();
        static int counter = 0;
        static void Main(string[] args)
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1);
            socket.Bind(localEndPoint);
            while (true)
            {
                byte[] buffer = new byte[1024];
                EndPoint senderRemote = (EndPoint)localEndPoint;
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
                PacketLabel packetLabel = JsonSerializer.Deserialize<PacketLabel>(packetString);
                if (packetLabel.Label == FigureProtocol.CurveLabel)
                {
                    FigureProtocol.Curve curve = JsonSerializer.Deserialize<Packet<FigureProtocol.Curve>>(packetString).Data;
                    curveMap[curve.CurveHandle] = vector.Count;
                    vector.Add(0);
                }
                else if (packetLabel.Label == FigureProtocol.DataLabel)
                {
                    FigureProtocol.Data data = JsonSerializer.Deserialize<Packet<FigureProtocol.Data>>(packetString).Data;
                    int index = curveMap[data.CurveHandle];
                    vector[index] = data.YValue;
                    counter++;
                    if (counter >= vector.Count) 
                    {
                        counter = 0;
                        string vectorString = string.Join(',', vector)+'\n';
                        System.Console.Write(vectorString);
                        byte[] vectorBytes = Encoding.UTF8.GetBytes(vectorString);
                        socket.SendTo(vectorBytes, remoteEndPoint);
                    }
                }
            }
        }
    }
}
