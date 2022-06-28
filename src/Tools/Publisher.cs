/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using Ligral.Simulation;

namespace Ligral.Tools
{
    public struct Packet<T> where T:struct
    {
        [JsonPropertyName("label")]
        public int Label {get; set;}
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
    public class Publisher
    {
        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static IPAddress defaultAddress = IPAddress.Parse(Settings.GetInstance().IPAddress);
        static IPEndPoint defaultEndPoint = new IPEndPoint(defaultAddress, Settings.GetInstance().SendingPort);
        private IPEndPoint endPoint;
        private Logger logger;
        private static int count = 0;
        private static List<Subscriber> hooks = new List<Subscriber>();
        public int Id;
        // private static ThreadStart threadStart = new ThreadStart(Serve);
        private static List<Thread> threads = new List<Thread>();// new Thread(threadStart);
        private static bool started = false;
        private static bool running = false;
        private static Logger staticLogger = new Logger("Publisher");
        private static List<EndPointGroup<byte[]>> packets = new List<EndPointGroup<byte[]>>();
        public Publisher()
        {
            Id = count;
            count++;
            logger = new Logger($"Publisher{Id}");
            endPoint = defaultEndPoint;
            InitThread(defaultEndPoint);
            if (!started) 
            {
                Solver.Starting += Start;
                started = true;
            }
        }
        public Publisher(string address, int port)
        {
            Id = count;
            count++;
            logger = new Logger($"Publisher{Id}");
            IPAddress ipAddress = IPAddress.Parse(address);
            endPoint = new IPEndPoint(ipAddress, port);
            InitThread(endPoint);
            if (!started) 
            {
                Solver.Starting += Start;
                started = true;
            }
        }
        private void InitThread(IPEndPoint endPoint)
        {
            var endPointGroup = packets.FirstOrDefault(endPointGroup => endPointGroup.Equal(endPoint));
            if (endPointGroup == null)
            {
                endPointGroup = new EndPointGroup<byte[]>(endPoint);
                packets.Add(endPointGroup);
                var thread = new Thread(() => Serve(endPoint));
                threads.Add(thread);
            }
        }
        public static void Start()
        {
            // started = true;
            Solver.Exited += Stop;
            foreach (var thread in threads) 
            {
                thread.Start();
                // Console.WriteLine(thread.ThreadState);
            }
        }
        public static void Stop()
        {
            running = false;
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
        public static void Serve(IPEndPoint endPoint)
        {
            // if (running) return;
            var endPointGroup = packets.First(endPointGroup => endPointGroup.Equal(endPoint));
            running = true;
            while (running || endPointGroup.Count != 0)
            {
                try
                {
                    var packetBytes = endPointGroup.First();
                    if (packetBytes is null) continue;
                    endPointGroup.RemoveAt(0);
                    socket.SendTo(packetBytes, endPoint);
                    staticLogger.Debug($"Packet ({packetBytes.Length}) is sent.");
                }
                catch (System.InvalidOperationException){}
            }
        }
        public void Send<T>(int label, T data) where T:struct
        {
            var endPointGroup = packets.First(endPointGroup => endPointGroup.Equal(endPoint));
            Packet<T> packet = new Tools.Packet<T>(){Label = label, Data = data};
            string packetString = JsonSerializer.Serialize<Packet<T>>(packet);
            byte[] packetBytes = Encoding.UTF8.GetBytes(packetString);
            foreach (var hook in hooks)
            {
                if (hook.Receive<T>(data)) 
                {
                    logger.Debug($"Packet: {label:X} transmitted via hook.");
                    return;
                }
            }
            endPointGroup.Add(packetBytes);
            logger.Debug($"Packet: {label:X} in queue.");
        }
        public void Send(int label, Dictionary<string, JsonObject> data)
        {
            var endPointGroup = packets.First(endPointGroup => endPointGroup.Equal(endPoint));
            Dictionary<string, JsonObject> packet = new Dictionary<string, JsonObject>();
            packet["label"] = new JsonNumber() { Value = label };
            packet["data"] = new JsonDict() { Value = data };
            JsonDict dict = new JsonDict() { Value = packet };
            string packetString = dict.ToString();
            byte[] packetBytes = Encoding.UTF8.GetBytes(packetString);
            endPointGroup.Add(packetBytes);
            logger.Debug($"Packet: {label:X} in queue.");
        }
        public static void AddHooks(Subscriber subscriber)
        {
            hooks.Add(subscriber);
        }
        public static bool ContainsHooks(Subscriber subscriber)
        {
            return hooks.Contains(subscriber);
        }
        public static void RemoveHooks(Subscriber subscriber)
        {
            hooks.Remove(subscriber);
        }
    }
}