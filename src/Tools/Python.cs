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
        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static IPAddress address = IPAddress.Parse("127.0.0.1");
        static int port = 8781;
        static Thread thread;
        static IPEndPoint endPoint;
        private Logger logger = new Logger("Python");
        static Logger staticLogger = new Logger("PythonBackend");
        static int waitingSeconds = 0;
        static bool received = false;
        static bool running = true;
        private bool started = false;
        private string waitingCommand = "";
        static Python()
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
            endPoint = new IPEndPoint(address, port);
            try
            {
                socket.Bind(endPoint);
            }
            catch(SocketException e)
            {
                throw staticLogger.Error(new LigralException($"Socket binding error:\n{e.Message}"));
            }
            Solver.Stopped += StopListening;
            thread = new Thread(() => Listen());
            thread.Start();
        }
        public void Start()
        {
            Settings settings = Settings.GetInstance();
            pythonProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = settings.PythonExecutable,
                    Arguments = "-m impython",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                }
            };
            try
            {
                pythonProcess.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                throw logger.Error(new LigralException($"{settings.PythonExecutable} is not installed or not callable, or impython is not installed."));
            }
            Assembly assembly = Assembly.Load("Ligral");
            Stream stream = assembly.GetManifestResourceStream("process.py");
            StreamReader reader = new StreamReader(stream);
            string script = reader.ReadToEnd();
            pythonProcess.StandardInput.Write(script);
            pythonProcess.StandardInput.Write("\n$exec\n");
            int currentWaitingSeconds = waitingSeconds;
            received = false;
            while (waitingSeconds<currentWaitingSeconds+10)
            {
                if (received)
                {
                    logger.Debug("python process started");
                    started = true;
                    pythonProcess.StandardInput.Write("\n$exec\n__status__()\n$exec\n");
                    return;
                }
            }
            throw logger.Error(new LigralException($"Error occurred when starting python process"));
        }
        static void StopListening() 
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
            pythonProcess.StandardInput.Write(command);
            pythonProcess.StandardInput.Write("\n$exec\n__status__()\n$exec\n");
            pythonProcess.StandardInput.Flush();
            logger.Debug("Command executed: " + command);
        }
        static void Listen()
        {
            while (running)
            {
                byte[] buffer = new byte[65536];
                EndPoint senderRemote = (EndPoint) endPoint;
                try
                {
                    socket.ReceiveFrom(buffer, ref senderRemote);
                }
                catch (SocketException)
                {
                    waitingSeconds++;
                    received = false;
                    continue;
                }
                string packetString = Encoding.UTF8.GetString(buffer.TakeWhile(x => x != 0).ToArray());
                if (packetString.Length == 0) 
                {
                    throw staticLogger.Error(new LigralException("Cannot get response from python: response is empty"));
                }
                Response response;
                try
                {
                    response = JsonSerializer.Deserialize<Response>(packetString);
                }
                catch (System.Exception e)
                {
                    throw staticLogger.Error(new LigralException($"Invalid response: {packetString}\n{e.Message}"));
                }
                if (!response.Success)
                {
                    throw staticLogger.Error(new LigralException($"Error occurred when executing python: {response.Message}"));
                }
                else if (response.Message != null && response.Message.Length > 0)
                {
                    staticLogger.Prompt(response.Message);
                }
                else
                {
                    staticLogger.Debug("Response received, but no message is contained.");
                }
                waitingSeconds = 0;
                received = true;
            }
        }
    }
}