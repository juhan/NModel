using System;
using System.Collections;
using System.Threading;

namespace Chat.Library
{
    public class Server : MarshalByRefObject
    {
        private ArrayList clients;

        public Server() 
        {
            this.clients = new ArrayList();
        }

        public string[] ClientNames 
        {
            get 
            {
                lock (this) 
                {
                    string[] clientNames = new string[clients.Count];
                    for (int i=0; i<clients.Count; i++)
                        clientNames[i] = ((Client)clients[i]).Name;
                    return clientNames;
                }
            }
        }

        public void EnterClient(Client client) 
        {
            if (client==null)
                throw new ArgumentNullException("client");
            lock (this) 
            {
                if (client.Server != null)
                    throw new InvalidOperationException("client has already entered a server");

                string name = client.Name;
                foreach (Client c in clients)
                    if (c.Name == name)
                        throw new InvalidOperationException("client with that name has already entered");
                clients.Add(client);

                client.Server = this;

                Console.WriteLine("client entered: {0}", name);
            }
        }

        public void ExitClient(Client client) 
        {
            if (client==null)
                throw new ArgumentNullException("client");
            lock (this)
            {
                if (client.Server == null)
                    throw new InvalidOperationException("client has not entered a server");

                client.Server = null;
                clients.Remove(client);

                Console.WriteLine("client left: {0}", client.Name);
            }
        }

        public void PostMessage(Client sender, string text) 
        {
            if (sender==null)
                throw new ArgumentNullException("sender");
            if (text==null)
                throw new ArgumentNullException("text");

            lock (this) 
            {
                Console.WriteLine("message posted by {0}: {1}", sender.Name, text);
                foreach (Client c in clients) 
                {
                    ClientReceiveMessageSender crms = new ClientReceiveMessageSender(false, c, sender, text);
                    Thread t = new Thread(new ThreadStart(crms.Run));
                    t.Start();
                }
            }
        }

        public void PostPrivateMessage(Client sender, string receiver, string text) 
        {
            if (sender==null)
                throw new ArgumentNullException("sender");
            if (receiver==null)
                throw new ArgumentNullException("receiver");
            if (text==null)
                throw new ArgumentNullException("text");

            lock (this) 
            {
                Console.WriteLine("private message for {0} posted by {1}: {2}", receiver, sender.Name, text);
                foreach (Client c in clients) 
                    if (c.Name == receiver)
                    {
                        ClientReceiveMessageSender crms = new ClientReceiveMessageSender(true, c, sender, text);
                        Thread t = new Thread(new ThreadStart(crms.Run));
                        t.Start();
                    }
            }
        }

        class ClientReceiveMessageSender 
        {
            bool privateMessage;
            Client c;
            Client sender;
            string text;

            public ClientReceiveMessageSender(bool privateMessage, Client c, Client sender, string text) 
            {
                this.privateMessage = privateMessage;
                this.c = c;
                this.sender = sender;
                this.text = text;
            }
            public void Run() 
            {
                Thread.Sleep(new Random().Next(100));
                if (privateMessage)
                    c.ReceivePrivateMessage(sender, text);
                else
                    c.ReceiveMessage(sender, text);
            }
        }

        public Client GetClient(string name) 
        {
            if (name==null)
                throw new ArgumentNullException("name");

            lock (this) 
            {
                foreach (Client c in clients) 
                    if (c.Name==name)
                        return c;
                return null;
            }
        }
    }
}
