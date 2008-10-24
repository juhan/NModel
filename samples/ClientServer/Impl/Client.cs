using System;
using System.Net;
using System.Net.Sockets;
using System.Threading; // Thread.Sleep

namespace ClientServerImpl
{
    // Wrapper for .NET client socket

    public class Client
    {
	Socket socket;
	
	const int BUFLEN = 4;
	byte[] receiveBuf = new byte[BUFLEN]; 

	// method Client.Socket assigns instance of System.Net.Sockets.Socket
	public void Socket() 
	{
	    socket = new Socket(AddressFamily.InterNetwork, 
				SocketType.Stream, ProtocolType.Tcp);
	}

	public void Connect(string ipAddr, int port) 
	{
	    socket.Connect(new IPEndPoint(IPAddress.Parse(ipAddr), port));
	}

	public void Send(string command)
	{
	    byte [] sendBuf = System.Text.Encoding.ASCII.GetBytes(command);
	    socket.Send(sendBuf);
	}

	public double Receive()
	{
	    int nbytes = socket.Receive(receiveBuf);
	    string response = 
		System.Text.Encoding.ASCII.GetString(receiveBuf,0,nbytes);
	    return Double.Parse(response); 
	}

	public void Close() {
	    socket.Close();
	} 

	public void Sleep(int seconds) 
	{
	    Thread.Sleep(1000*seconds); // convert seconds to milliseconds
	}
    }
}
