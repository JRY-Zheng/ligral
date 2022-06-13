/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using System.Threading;
using Ligral.Simulation;

namespace Ligral.Tools
{
    public struct Response 
    {
        [JsonPropertyName("success")]
        public bool Success {get; set;}
        [JsonPropertyName("message")]
        public string Message {get; set;}
    }
    public class Python
    {
        private Process pythonProcess;
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private IPAddress address = IPAddress.Parse("127.0.0.1");
        private static int nextPort = 18589;
        private Thread thread;
        static int GetPort()
        {
            int port = nextPort;
            nextPort += 2;
            return port;
        }
        private int sendingPort;
        private int receivingPort;
        private IPEndPoint sendingEndPoint;
        private IPEndPoint receivingEndPoint;
        private Logger logger = new Logger("Python");
        private bool started = false;
        private bool running = false;
        private string waitingCommand = "";
        public void Start()
        {
            Settings settings = Settings.GetInstance();
            sendingPort = GetPort();
            receivingPort = sendingPort + 1;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
            sendingEndPoint = new IPEndPoint(address, sendingPort);
            receivingEndPoint = new IPEndPoint(address, receivingPort);
            try
            {
                socket.Bind(receivingEndPoint);
            }
            catch(SocketException e)
            {
                throw logger.Error(new LigralException($"Address: {receivingEndPoint.Address} port: {receivingEndPoint.Port}.\n{e.Message}"));
            }
            pythonProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = settings.PythonExecutable,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                }
            };
            try
            {
                pythonProcess.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                throw logger.Error(new LigralException($"{settings.PythonExecutable} is not installed or not callable."));
            }
            Assembly assembly = Assembly.Load("Ligral");
            Stream stream = assembly.GetManifestResourceStream("process.py");
            StreamReader reader = new StreamReader(stream);
            string script = reader.ReadToEnd();
            pythonProcess.StandardInput.WriteLine($"recv_port = {sendingPort}");
            pythonProcess.StandardInput.Write(script);
            pythonProcess.StandardInput.Close();
            running = true;
            try
            {
                GetResponse(allowWaitSecond:10);
            }
            catch (LigralException)
            {
                throw logger.Error(new LigralException($"Error occurred when starting python process"));
            }
            started = true;
            logger.Debug("python process started");
            Solver.Stopped += Stop;
            thread = new Thread(() => GetResponse());
            thread.Start();
        }
        public void Stop() 
        {
            running = false;
        }
        public void Execute(string command)
        {
            if (!started)
            {
                waitingCommand += command;
                return;
            }
            else if (waitingCommand != "")
            {
                command = waitingCommand + command;
                waitingCommand = "";
            }
            byte[] buffer = Encoding.UTF8.GetBytes(command);
            socket.SendTo(buffer, sendingEndPoint);
        }
        private void GetResponse(int allowWaitSecond = 0)
        {
            int waitSecond = 0;
            while (running)
            {
                byte[] buffer = new byte[65536];
                EndPoint senderRemote = (EndPoint) receivingEndPoint;
                try
                {
                    socket.ReceiveFrom(buffer, ref senderRemote);
                    waitSecond = 0;
                }
                catch (SocketException e)
                {
                    if (allowWaitSecond == 0)
                    {
                        continue;
                    }
                    waitSecond++;
                    if (allowWaitSecond > waitSecond)
                    {
                        continue;
                    }
                    throw logger.Error(new LigralException($"Cannot get response from python: {e.Message}"));
                }
                string packetString = Encoding.UTF8.GetString(buffer.TakeWhile(x => x != 0).ToArray());
                if (packetString.Length == 0) 
                {
                    throw logger.Error(new LigralException("Cannot get response from python: response is empty"));
                }
                Response response;
                try
                {
                    response = JsonSerializer.Deserialize<Response>(packetString);
                }
                catch (System.Exception e)
                {
                    throw logger.Error(new LigralException($"Invalid response: {packetString}\n{e.Message}"));
                }
                if (!response.Success)
                {
                    throw logger.Error(new LigralException($"Error occurred when executing python: {response.Message}"));
                }
                else if (response.Message != null && response.Message.Length > 0)
                {
                    logger.Prompt(response.Message);
                }
                if (allowWaitSecond > 0) return;
            }
        }
    }
}