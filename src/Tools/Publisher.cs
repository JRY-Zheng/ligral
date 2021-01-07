/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ligral.Tools
{
    struct Packet<T> where T:struct
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
        static IPEndPoint endPoint = new IPEndPoint(address, Settings.GetInstance().Port);
        private static int count = 0;
        private static List<Subscriber> hooks = new List<Subscriber>();
        public int Id;
        public Publisher()
        {
            Id = count;
            count++;
        }
        public async void Send<T>(int label, T data) where T:struct
        {
            Packet<T> packet = new Tools.Packet<T>(){Label = label, Data = data};
            string packetString = JsonSerializer.Serialize<Packet<T>>(packet);
            byte[] packetBytes = Encoding.UTF8.GetBytes(packetString);
            foreach (var hook in hooks)
            {
                if (hook.Invoke<T>(data)) return;
            }
            await Task.Run(() => socket.SendTo(packetBytes, endPoint));
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