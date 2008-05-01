using System;
using System.Collections;

namespace Chat.Library
{
    [System.Serializable]
    public delegate void MessageReceivedEventHandler(Client c, Client sender, string text);

    [System.Serializable]
    public delegate void PrivateMessageReceivedEventHandler(Client c, Client sender, string text);

    public class Client : MarshalByRefObject
    {
        private string name;
        private Server server;

        public Client(string name) 
        {
            if (name==null)
                throw new ArgumentNullException("text");

            this.name = name;
            this.server = null;
        }

        public void PostMessage(string text) 
        {
            if (text==null)
                throw new ArgumentNullException("text");

            if (this.server==null)
                throw new InvalidOperationException("cannot PostText before having entered a server");

            this.server.PostMessage(this, text);
        }

        public void PostPrivateMessage(string receiver, string text) 
        {
            if (receiver==null)
                throw new ArgumentNullException("receiver");

            if (text==null)
                throw new ArgumentNullException("text");

            if (this.server==null)
                throw new InvalidOperationException("cannot PostText before having entered a server");

            this.server.PostPrivateMessage(this, receiver, text);
        }

        public void ReceiveMessage(Client sender, string text) 
        {
            if (text==null)
                throw new ArgumentNullException("text");

            MessageReceivedEventHandler textReceived = this.MessageReceived;
            if (textReceived!=null)
                textReceived(this, sender, text);
        }

        public void ReceivePrivateMessage(Client sender, string text) 
        {
            if (sender==null)
                throw new ArgumentNullException("sender");

            if (text==null)
                throw new ArgumentNullException("text");

            PrivateMessageReceivedEventHandler textReceived = this.PrivateMessageReceived;
            if (textReceived!=null)
                textReceived(this, sender, text);
        }

        public event MessageReceivedEventHandler MessageReceived;
        public event PrivateMessageReceivedEventHandler PrivateMessageReceived;

        public string Name 
        {
            get 
            {
                return this.name;
            }
        }

        public Server Server 
        {
            get 
            {
                return this.server;
            }
            set 
            {
                if (this.server != value) 
                {
                    this.server = value;
                    EventHandler eventHandler = this.ServerChanged;
                    if (eventHandler!=null)
                        eventHandler(this, new EventArgs());
                }
            }
        }

        public event EventHandler ServerChanged;
    }

    [System.Serializable]
    public delegate void ClientCreatedEventHandler(Client client);

    public class ClientCreationNotifier : MarshalByRefObject 
    {
        public ClientCreationNotifier() {}

        public void Notify(Client clientCreated) 
        {
            ClientCreatedEventHandler cc = this.ClientCreated;
            if (cc!=null)
                cc(clientCreated);
        }

        public event ClientCreatedEventHandler ClientCreated;
    }
}
