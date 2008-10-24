using System;
using System.Net;
using System.Net.Sockets; 

namespace ClientServerImpl
{
    // Wrapper for .NET server socket

    public class Server
    {
	Socket listenerSocket;    // assigned by Socket
	Socket connectionSocket;  // assigned by Accept

	const int BUFLEN = 40;
	byte[] receiveBuf = new byte[BUFLEN];

	// method Client.Socket assigns instance of System.Net.Sockets.Socket
	public void Socket() 
	{
	    listenerSocket = new Socket(AddressFamily.InterNetwork, 
					SocketType.Stream, ProtocolType.Tcp);
	}

	public void Bind(string ipAddr, int port) {
	    listenerSocket.Bind(new IPEndPoint(IPAddress.Parse(ipAddr), port));
	}

	public void Listen() {
	    const int backlog = 0;
	    listenerSocket.Listen(backlog);
	}

	// Socket.Accept returns connectionSocket used by Send, Receive, etc.
	public void Accept() {
	    connectionSocket = listenerSocket.Accept();
	}

	public string Receive()
	{
	    int nbytes = connectionSocket.Receive(receiveBuf);
	    string command = 
		System.Text.Encoding.ASCII.GetString(receiveBuf,0,nbytes);
	    return command;  // command.Length = 0 means connection closed
	}

	public void Send(double datum)
	{
	    string response = String.Format("{0:F1}", datum); //1 decimal digit
	    byte [] sendBuf = System.Text.Encoding.ASCII.GetBytes(response);
	    connectionSocket.Send(sendBuf);
	}

	public void CloseConnection() 
	{
	    connectionSocket.Close();
	}

	public void Close() 
        {
	    listenerSocket.Close();
	}
    }
}
