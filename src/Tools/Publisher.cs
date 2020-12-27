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
    class Publisher
    {
        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static IPAddress address = IPAddress.Parse(Settings.GetInstance().IPAddress);
        static IPEndPoint endPoint = new IPEndPoint(address, Settings.GetInstance().Port);
        private static int count = 0;
        private List<Subscriber> hooks = new List<Subscriber>();
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
            hooks.ForEach(hook => hook.Invoke<T>(data));
            await Task.Run(() => socket.SendTo(packetBytes, endPoint));
        }
        public void AddHooks(Subscriber subscriber)
        {
            hooks.Add(subscriber);
        }
    }
}