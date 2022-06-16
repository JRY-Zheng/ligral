/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

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
    public class EndPointGroup<T> : List<T>
    {
        public IPEndPoint CharEndPoint;
        public EndPointGroup(IPEndPoint endPoint)
        {
            CharEndPoint = endPoint;
        }
        public bool Equal(IPEndPoint endPoint)
        {
            return CharEndPoint.Address.Equals(endPoint.Address) &&
                CharEndPoint.Port == endPoint.Port;
        }
    }
    public class Subscriber
    {
        // static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static IPAddress defaultAddress = IPAddress.Parse(Settings.GetInstance().IPAddress);
        static IPEndPoint defaultEndPoint = new IPEndPoint(defaultAddress, Settings.GetInstance().ListeningPort);
        private static int count = 0;
        public int Id;
        private static bool running = false;
        protected static List<EndPointGroup<Subscriber>> subscribers = new List<EndPointGroup<Subscriber>>();
        protected Logger logger;
        private static Logger subscriberLogger = new Logger("Subscriber");
        // private static ThreadStart threadStart = new ThreadStart(Serve);
        private static List<Thread> threads = new List<Thread>();// = new Thread(threadStart);
        private static bool started = false;
        private Dictionary<PacketLabel, Action<string>> labelAbilities = new Dictionary<PacketLabel, Action<string>>();
        private Dictionary<Type, object> typeAbilities = new Dictionary<Type, object>();
        public Subscriber()
        {
            Id = count;
            count++;
            Register(defaultEndPoint, this);
            logger = new Logger(GetType().Name);
            if (!started) 
            {
                Solver.Starting += Start;
                started = true;
            }
        }
        public Subscriber(string address, int port)
        {
            Id = count;
            count++;
            IPAddress ipAddress = IPAddress.Parse(address);
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            Register(endPoint, this);
            logger = new Logger(GetType().Name);
            if (!started) 
            {
                Solver.Starting += Start;
                started = true;
            }
        }
        public static void Register(IPEndPoint endPoint, Subscriber subscriber)
        {
            var endPointGroup = subscribers.FirstOrDefault(endPointGroup => endPointGroup.Equal(endPoint));
            if (endPointGroup == null)
            {
                endPointGroup = new EndPointGroup<Subscriber>(endPoint);
                subscribers.Add(endPointGroup);
                var thread = new Thread(() => Serve(endPoint));
                threads.Add(thread);
            }
            endPointGroup.Add(subscriber);
        }
        public static void Start()
        {
            // started = true;
            Solver.Stopped += Stop;
            foreach (var thread in threads) 
            {
                thread.Start();
                // Console.WriteLine(thread.ThreadState);
            }
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
            var group = subscribers.FirstOrDefault(group=>group.Contains(this));
            if (group != null)
            {
                group.Remove(this);
                if (group.Count == 0)
                {
                    subscribers.Remove(group);
                }
            }
            if (subscribers.Count == 0)
            {
                Stop();
            }
        }
        public static void Serve(IPEndPoint endPoint)
        {
            // if (running) return;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            running = true;
            var group = subscribers.First(group => group.Equal(endPoint));
            EndPoint senderRemote = (EndPoint)endPoint;
            // Console.WriteLine(group.CharEndPoint.Port);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1);
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
                foreach (var subscriber in group)
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