/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Ligral.Simulation;

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
        private static bool running = false;
        protected static List<Subscriber> subscribers = new List<Subscriber>();
        protected Logger logger;
        private static Logger subscriberLogger = new Logger("Subscriber");
        private static ThreadStart threadStart = new ThreadStart(Serve);
        private static Thread thread = new Thread(threadStart);
        private static bool started = false;
        private Dictionary<PacketLabel, Action<string>> labelAbilities = new Dictionary<PacketLabel, Action<string>>();
        private Dictionary<Type, object> typeAbilities = new Dictionary<Type, object>();
        public Subscriber()
        {
            Id = count;
            count++;
            subscribers.Add(this);
            logger = new Logger(GetType().Name);
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
            // if (socket.IsBound)
            // {
            //     socket.Shutdown(SocketShutdown.Receive);
            //     socket.Close();
            // }
            // thread.Suspend();
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
        public static void Serve()
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
                socket.ReceiveFrom(buffer, ref senderRemote);
                string packetString = Encoding.UTF8.GetString(buffer.TakeWhile(x => x != 0).ToArray());
                PacketLabel packetLabel = JsonSerializer.Deserialize<PacketLabel>(packetString);
                foreach (var subscriber in subscribers)
                {
                    try
                    {
                        subscriber.Invoke(packetLabel, packetString);
                    }
                    catch (LigralException)
                    {
                        subscriberLogger.Throw();
                    }
                    catch (Exception e)
                    {
                        subscriberLogger.Fatal(e);
                        running = false;
                        break;
                    }
                }
            }
        }
        protected virtual bool Invoke(PacketLabel packetLabel, string packetString)
        {
            if (labelAbilities.ContainsKey(packetLabel))
            {
                labelAbilities[packetLabel](packetString);
                return true;
            }
            return false;
        }
        public virtual bool Receive<T>(T datastruct) where T: struct
        {
            Type type = datastruct.GetType();
            if (typeAbilities.ContainsKey(type))
            {
                Action<T> action = typeAbilities[type] as Action<T>;
                action(datastruct);
            }
            return false;
        }
        public void AddAbility(PacketLabel packetLabel, Action<string> action)
        {
            labelAbilities.Add(packetLabel, action);
        }
        public void AddAbility<T>(Action<T> action) where T: struct
        {
            T t = new T();
            typeAbilities.Add(t.GetType(), action);
        }
    }
}