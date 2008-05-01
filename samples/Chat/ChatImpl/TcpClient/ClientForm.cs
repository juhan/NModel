using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
//using System.Data;
using Chat.Library;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Reflection; 

namespace Chat.TcpClient
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class ClientForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.RichTextBox messagesDisplay;
        private System.Windows.Forms.TextBox messageTextBox;
        private System.Windows.Forms.Button sendButton;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.Button sendPrivateButton;
        private Client client;
        public ClientForm(Client client)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.client = client;

            client.MessageReceived += new MessageReceivedEventHandler(client_MessageReceived);
            client.PrivateMessageReceived += new PrivateMessageReceivedEventHandler(client_PrivateMessageReceived);
            this.Text = client.Name;
            this.client.ServerChanged += new EventHandler(client_ServerChanged);
            updateEnabled();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }

                if (this.client.Server!=null)
                    this.client.Server.ExitClient(this.client);
            }

            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.messagesDisplay = new System.Windows.Forms.RichTextBox();
            this.messageTextBox = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.connectButton = new System.Windows.Forms.Button();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.sendPrivateButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // messagesDisplay
            // 
            this.messagesDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.messagesDisplay.Location = new System.Drawing.Point(8, 40);
            this.messagesDisplay.Name = "messagesDisplay";
            this.messagesDisplay.ReadOnly = true;
            this.messagesDisplay.Size = new System.Drawing.Size(407, 54);
            this.messagesDisplay.TabIndex = 2;
            this.messagesDisplay.Text = "";
            // 
            // messageTextBox
            // 
            this.messageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.messageTextBox.Location = new System.Drawing.Point(8, 102);
            this.messageTextBox.Name = "messageTextBox";
            this.messageTextBox.Size = new System.Drawing.Size(327, 20);
            this.messageTextBox.TabIndex = 0;
            // 
            // sendButton
            // 
            this.sendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.sendButton.Location = new System.Drawing.Point(343, 102);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(75, 23);
            this.sendButton.TabIndex = 1;
            this.sendButton.Text = "Send";
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(8, 8);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(112, 23);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect to Server";
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // disconnectButton
            // 
            this.disconnectButton.Location = new System.Drawing.Point(128, 8);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(144, 23);
            this.disconnectButton.TabIndex = 5;
            this.disconnectButton.Text = "Disconnect from Server";
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // sendPrivateButton
            // 
            this.sendPrivateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sendPrivateButton.Location = new System.Drawing.Point(279, 8);
            this.sendPrivateButton.Name = "sendPrivateButton";
            this.sendPrivateButton.Size = new System.Drawing.Size(128, 23);
            this.sendPrivateButton.TabIndex = 6;
            this.sendPrivateButton.Text = "Send Private Message";
            this.sendPrivateButton.Click += new System.EventHandler(this.sendPrivateButton_Click);
            // 
            // ClientForm
            // 
            this.AcceptButton = this.sendButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(423, 127);
            this.Controls.Add(this.sendPrivateButton);
            this.Controls.Add(this.disconnectButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.messageTextBox);
            this.Controls.Add(this.messagesDisplay);
            this.Name = "ClientForm";
            this.Text = "Chat Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void client_MessageReceived(Client c, Client sender, string text)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new MessageReceivedEventHandler(this.client_MessageReceived), new object[]{c, sender, text});
            else 
                display(text + "\r\n");
        }

        private void client_PrivateMessageReceived(Client c, Client sender, string text)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new PrivateMessageReceivedEventHandler(this.client_PrivateMessageReceived), new object[]{c, sender, text});
            else 
                display(sender.Name + ": " + text + "\r\n");
        }

        private void display(string s) 
        {
            this.messagesDisplay.AppendText(s);
                
            // scroll to end; somewhat tricky
            Message m = Message.Create(this.messagesDisplay.Handle, 277/*WM_VSCROLL*/, (IntPtr)7/*SB_BOTTOM*/, IntPtr.Zero); 
            MethodInfo wndProc = typeof(RichTextBox).GetMethod("WndProc", BindingFlags.Instance|BindingFlags.NonPublic);
            wndProc.Invoke(this.messagesDisplay, new object[]{m});
        }

        private void sendButton_Click(object sender, System.EventArgs e)
        {
            this.client.PostMessage(this.messageTextBox.Text);

            this.messageTextBox.Text = "";
        }

        private void connectButton_Click(object sender, System.EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Server server = (Server)Activator.GetObject(
                typeof(Server),
                "tcp://localhost:8085/ChatServer");
            if (server == null) 
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("could not locate server");
            }
            else 
                try 
                {
                    server.EnterClient(client);
                    this.Cursor = Cursors.Default;
                } 
                catch (Exception ex) 
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("an error occurred: " + ex.Message);
                }
        }

        private void disconnectButton_Click(object sender, System.EventArgs e)
        {
            Server server = this.client.Server;
            if (server!=null)
                server.ExitClient(client);
        }

        private void client_ServerChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new EventHandler(this.client_ServerChanged), new object[]{sender, e});
            else
                this.updateEnabled();
        }

        private void updateEnabled() 
        {
            bool connected = this.client.Server != null;
            this.connectButton.Enabled = !connected;
            this.disconnectButton.Enabled = connected;

            this.sendPrivateButton.Enabled = connected;
            this.messagesDisplay.Enabled = connected;
            this.messageTextBox.Enabled = connected;
            this.sendButton.Enabled = connected;
            if (connected)
                this.messageTextBox.Focus();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) 
        {
            IDictionary props = new Hashtable(); 
            props["port"] = 0; 
            BinaryClientFormatterSinkProvider clientFormatterProvider = new BinaryClientFormatterSinkProvider();
            BinaryServerFormatterSinkProvider serverFormatterProvider = new BinaryServerFormatterSinkProvider();
            serverFormatterProvider.TypeFilterLevel = TypeFilterLevel.Full;
            TcpChannel chan = new TcpChannel(props, clientFormatterProvider, serverFormatterProvider);
            ChannelServices.RegisterChannel(chan,false);
            string clientName = args.Length>0 ? args[0] : ("client#" + new Random().Next().ToString());
            Client client = new Client(clientName);

            if (args.Length>2) 
            {
                string uri = args[2];
                ClientCreationNotifier notifier = (ClientCreationNotifier)Activator.GetObject(
                    typeof(ClientCreationNotifier),
                    uri);
                try 
                {
                    //System.Diagnostics.Debugger.Break();//---
                    notifier.Notify(client);
                } 
                catch (Exception e) 
                {
                    MessageBox.Show(e.ToString());
                }
            }

            using (ClientForm clientForm = new ClientForm(client)) 
            {
                if (args.Length>1) 
                {
                    int pos = int.Parse(args[1]);
                    Rectangle screen = Screen.PrimaryScreen.WorkingArea;

                    clientForm.Location = new Point(
                        8+screen.Left + screen.Width/2*(pos&1), 
                        8+screen.Top + screen.Height/2*((pos&2)>>1));
                    //clientForm.Size = new Size(screen.Width/2-16, screen.Height/2-16);
                    clientForm.StartPosition = FormStartPosition.Manual;
                }

                Application.Run(clientForm);
            }
        }

        private void sendPrivateButton_Click(object sender, System.EventArgs e)
        {
            using (SendPrivateMessageDialog spmd = new SendPrivateMessageDialog()) 
            {
                this.Cursor = Cursors.WaitCursor;
                Server server = (Server)Activator.GetObject(
                    typeof(Server),
                    "tcp://localhost:8085/ChatServer");
                if (server == null) 
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("could not locate server");
                    return;
                }
                else 
                    try 
                    {
                        spmd.ClientNames = server.ClientNames;
                        this.Cursor = Cursors.Default;
                    } 
                    catch (Exception ex) 
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("an error occurred: " + ex.Message);
                        return;
                    }

                if (spmd.ShowDialog() == DialogResult.OK) 
                {
                    this.Cursor = Cursors.WaitCursor;
                    try 
                    {
                        server.PostPrivateMessage(this.client,  spmd.ClientName, spmd.MessageText);
                        this.Cursor = Cursors.Default;
                    } 
                    catch (Exception ex) 
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("an error occurred: " + ex.Message);
                        return;
                    }
                }
            }
        }
    }
}
