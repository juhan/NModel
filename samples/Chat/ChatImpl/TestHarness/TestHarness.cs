using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using Chat.Library;
using System.Collections.Generic;

namespace Chat.TestHarness
{
    public class Global
    {
        #region Harness State
        static bool channelCreated = false;

        static string serverFileName;
        static string clientFileName;

        static Process serverProcess;
        static Server server;
        static int clientsEverCreatedCount;
        static ArrayList clientProcesses;
        static string chatClientCreatedUri;
        #endregion

        #region Setup and Teardown
        public static void Setup()
        {
            if (!channelCreated)
            {
                channelCreated = true;
                IDictionary props = new Hashtable();
                props["port"] = 8086;
                BinaryClientFormatterSinkProvider clientFormatterProvider = new BinaryClientFormatterSinkProvider();
                BinaryServerFormatterSinkProvider serverFormatterProvider = new BinaryServerFormatterSinkProvider();
                serverFormatterProvider.TypeFilterLevel = TypeFilterLevel.Full;
                TcpChannel chan = new TcpChannel(props, clientFormatterProvider, serverFormatterProvider);
                ChannelServices.RegisterChannel(chan,false);
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(ClientCreationNotifier),
                    "ChatClientCreated", WellKnownObjectMode.Singleton);
                chatClientCreatedUri = "tcp://localhost:8086/ChatClientCreated";

                ClientCreationNotifier ccn = (ClientCreationNotifier)Activator.GetObject(
                    typeof(ClientCreationNotifier),
                    chatClientCreatedUri);
                ccn.ClientCreated += new ClientCreatedEventHandler(ClientCreated);
            }

            serverFileName = locateProgram(@"Chat.TcpServer.exe");
            clientFileName = locateProgram(@"Chat.TcpClient.exe");

            if (isProcessAlive("Chat.TcpServer"))
            {
                killProcess("Chat.TcpServer");
                Thread.Sleep(1000);
                if (isProcessAlive("Chat.TcpServer"))
                    throw new InvalidOperationException("old server still running");
            }

            serverProcess = Process.Start(serverFileName);

            server = (Server)Activator.GetObject(
                typeof(Server),
                "tcp://localhost:8085/ChatServer");
            if (server == null)
                throw new InvalidOperationException("Could not locate server");

            clientProcesses = new ArrayList();
            clientsEverCreatedCount = 0;
        }

        private static bool isProcessAlive(string name)
        {
            foreach (Process p in Process.GetProcesses())
                if (p.ProcessName == name)
                    return true;
            return false;
        }

        private static void killProcess(string name)
        {
            foreach (Process p in Process.GetProcesses())
                if (p.ProcessName == name)
                    p.Kill();
        }

        public static void Teardown()
        {
            if (clientProcesses != null)
            {
                foreach (Process clientProcess in clientProcesses)
                    clientProcess.Kill();
            }
            if (serverProcess != null)
            {
                serverProcess.Kill();
            }

            clientProcesses = null;
            serverProcess = null;
            clientsEverCreatedCount = 0;
            server = null;
        }

        #region helper functions
        private static string locateProgram(string path)
        {
            for (int i = 0; i < 10; i++)
            {
                if (File.Exists(path))
                    return path;
                path = Path.Combine("..", path);
            }
            throw new System.InvalidOperationException("cannot find " + path);
        }
        #endregion
        #endregion

        #region Wrapper functions around implementation
        // When no name is provided, a new client name is created

        public static Client CreateClient()
        {
            string name = "client#" + clientsEverCreatedCount.ToString();
            return CreateClient(name);
        }

        public static Client CreateClient(string name)
        {
            Process clientProcess = Process.Start(clientFileName, name + " " + clientsEverCreatedCount + " " + chatClientCreatedUri);
            clientsEverCreatedCount++;

            clientProcesses.Add(clientProcess);

            // look for new client
            while (true)
            {
                Thread.Sleep(20);
                if (lastCreatedClient != null)
                {
                    Client client = lastCreatedClient;
                    lastCreatedClient = null;
                    client.MessageReceived += new MessageReceivedEventHandler(new MessageReceivedWrapper().MessageReceived);
                    client.PrivateMessageReceived += new PrivateMessageReceivedEventHandler(new PrivateMessageReceivedWrapper().PrivateMessageReceived);
                    return client;
                }
            }
        }

        #region helper functions
        // we get a call back with the new client after the client-process has started up
        static Client lastCreatedClient = null;
        public static void ClientCreated(Client client)
        {
            //System.Diagnostics.Debugger.Break();//+++
            lastCreatedClient = client;
        }

        // because of a remoting issue, we need an instance wrapper for static events
        public class MessageReceivedWrapper : MarshalByRefObject
        {
            public void MessageReceived(Client c, Client sender, string text)
            {
                Global.MessageReceived(c, sender, text);
            }
        }

        public class PrivateMessageReceivedWrapper : MarshalByRefObject
        {
            public void PrivateMessageReceived(Client c, Client sender, string text)
            {
                Global.PrivateMessageReceived(c, sender, text);
            }
        }
        #endregion

        public static void Enter(Client c)
        {
            server.EnterClient(c);
        }

        public static bool TryEnter(Client c)
        {
            try
            {
                server.EnterClient(c);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Exit(Client c)
        {
            server.ExitClient(c);
        }

        public static void MessageReceived(Client c, Client sender, string text)
        {
            //Console.WriteLine("got message for " + c.Name + " from " + sender.Name + ": " + text);
            //Notify the ChatStepper
            ChatStepper.Receive(sender, text, c);
        }

        public static void PrivateMessageReceived(Client c, Client sender, string text)
        {
            //Console.WriteLine("got private message for " + c.Name + " from " + sender.Name + ": " + text);
            //Notify the ChatStepper
            ChatStepper.PrivateReceive(sender, text, c);
        }
        #endregion
    }
}

