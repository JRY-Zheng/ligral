/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
    public struct PacketLabel
    {
        [JsonPropertyName("label")]
        public int Label {get; set;}
    }
    public class Subscriber
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
                foreach (var subscriber in subscribers)
                {
                    subscriber.Invoke(packetLabel, packetString);
                }
            }
        }
        protected virtual bool Invoke(PacketLabel packetLabel, string packetString)
        {
            return false;
        }
        public virtual bool Receive<T>(T datastruct) where T: struct
        {
            return false;
        }
    }
}