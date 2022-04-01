/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
        static IPAddress address = IPAddress.Parse(Settings.GetInstance().IPAddress);
        static IPEndPoint endPoint = new IPEndPoint(address, Settings.GetInstance().SendingPort);
        private static int count = 0;
        private static List<Subscriber> hooks = new List<Subscriber>();
        public int Id;
        private static ThreadStart threadStart = new ThreadStart(Serve);
        private static Thread thread = new Thread(threadStart);
        private static bool started = false;
        private static bool running = false;
        private static List<byte[]> packets = new List<byte[]>();
        public Publisher()
        {
            Id = count;
            count++;
            if (!started)
            {
                Start();
            }
        }
        public static void Start()
        {
            started = true;
            Solver.Exited += Stop;
            thread.Start();
        }
        public static void Stop()
        {
            running = false;
        }
        public static void Serve()
        {
            if (running) return;
            running = true;
            while (running)
            {
                try
                {
                    var packetBytes = packets.First();
                    if (packetBytes is null) continue;
                    packets.RemoveAt(0);
                    socket.SendTo(packetBytes, endPoint);
                }
                catch (System.InvalidOperationException){}
            }
        }
        public void Send<T>(int label, T data) where T:struct
        {
            Packet<T> packet = new Tools.Packet<T>(){Label = label, Data = data};
            string packetString = JsonSerializer.Serialize<Packet<T>>(packet);
            byte[] packetBytes = Encoding.UTF8.GetBytes(packetString);
            foreach (var hook in hooks)
            {
                if (hook.Receive<T>(data)) return;
            }
            packets.Add(packetBytes);
        }
        public void Send(int label, Dictionary<string, JsonObject> data)
        {
            Dictionary<string, JsonObject> packet = new Dictionary<string, JsonObject>();
            packet["label"] = new JsonNumber() { Value = label };
            packet["data"] = new JsonDict() { Value = data };
            JsonDict dict = new JsonDict() { Value = packet };
            string packetString = dict.ToString();
            byte[] packetBytes = Encoding.UTF8.GetBytes(packetString);
            packets.Add(packetBytes);
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