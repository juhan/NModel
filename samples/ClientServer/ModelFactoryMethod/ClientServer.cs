using NModel;
using NModel.Attributes;
using NModel.Execution;

namespace ClientServer
{
    public enum Socket { None, Created, Bound, Listening, Connecting, 
			 Connected, Disconnected, Closed }

    public enum Phase { Send, ServerReceive, ClientReceive }

    public static class ClientServer
    {
	const double EmptyBuffer = double.MaxValue;
	const double Temp2 = 99.9;   // Temperature, 2 digits
	const double Temp3 = 100.0;  // Temperature, 3 digits

	// Control state
	public static Socket serverSocket = Socket.None;
	public static Socket clientSocket = Socket.None;
	public static Phase phase = Phase.Send;

	// Data state
	public static double clientBuffer = EmptyBuffer;

	// For testing
	[AcceptingStateCondition]
	static bool BothClosed() 
	{
	    return (serverSocket == Socket.Closed
		    && clientSocket == Socket.Closed);
	}

	// Server enabling conditions and actions
	
	public static bool ServerSocketEnabled() 
	{ 
	    return (serverSocket == Socket.None); 
	}

	[Action] 
	public static void ServerSocket() 
	{ 
	    serverSocket = Socket.Created; 
	}

	public static bool ServerBindEnabled() 
	{ 
	    return (serverSocket == Socket.Created); 
	}

	[Action] 
	public static void ServerBind() 
	{ 
	    serverSocket = Socket.Bound; 
	}

	public static bool ServerListenEnabled() 
	{ 
	    return (serverSocket == Socket.Bound); 
	}

	[Action] 
        public static void ServerListen() 
	{ 
	    serverSocket = Socket.Listening; 
	}

	public static bool ServerAcceptEnabled()
	{
	    return (serverSocket == Socket.Listening
		    && clientSocket == Socket.Connecting);
	}

	[Action]
	public static void ServerAccept() 
	{ 
	    serverSocket = Socket.Connected; clientSocket = Socket.Connected;
	}

	public static bool ServerReceiveEnabled() 
	{ 
	    return (serverSocket == Socket.Connected
		    && phase == Phase.ServerReceive);
	}

	
	// No parameter needed here, client always sends same thing
	[Action] 
        public static void ServerReceive() 
	{ 
	    phase = Phase.Send;
	}

	public static bool ServerSendEnabled() 
	{ 
	    return (serverSocket == Socket.Connected 
		    && phase == Phase.Send
		    && clientSocket == Socket.Connected);
	}

	// Parameter here, server can send different temperatures
        [Action]
	public static void ServerSend([Domain("Temperatures")] double datum) 
	{ 
	    clientBuffer = datum;
	    phase = Phase.ClientReceive;
	}

	// Domain for ServerSend parameter t
	static Set<double> Temperatures() 
	{ 
	    return new Set<double>(Temp2, Temp3);
	}

	public static bool ServerCloseConnectionEnabled() 
	{ 
	    return (serverSocket == Socket.Connected); 
	}

	[Action]
	public static void ServerCloseConnection() 
	{ 
	    serverSocket = Socket.Disconnected; 
	}

	// Prevent Client crashing - does sending to closed partner crash?
	public static bool ServerCloseEnabled() 
	{ 
	    return (serverSocket != Socket.None 
		    // && serverSocket != Socket.Listening
		    && serverSocket != Socket.Connected
		    && serverSocket != Socket.Closed);
	}

	[Action] 
	public static void ServerClose() 
	{ 
	    serverSocket = Socket.Closed; 
	}

	// Client enabling conditions and actions

	public static bool ClientSocketEnabled() 
	{ 
	    return (clientSocket == Socket.None); 
	}

	[Action]
	public static void ClientSocket() 
	{ 
	    clientSocket = Socket.Created; 
	}

	public static bool ClientConnectEnabled()
	{
	    return (clientSocket == Socket.Created
		    && serverSocket == Socket.Listening);
	}

	[Action] 
        public static void ClientConnect() 
	{ 
	    clientSocket = Socket.Connecting; 
	}

	public static bool ClientSendEnabled()
	{ 
	    return (clientSocket == Socket.Connected 
		    && phase == Phase.Send);
	}

	// No parameter needed here, client always sends the same thing
	[Action]
	public static void ClientSend() 
	{ 
	    phase = Phase.ServerReceive; 
	}

	public static bool ClientReceiveEnabled() 
	{ 
	    return (clientSocket == Socket.Connected 
		    && phase == Phase.ClientReceive);
	}                                        

	// Return value needed here, server sends different values
	[Action]
	public static double ClientReceive() 
	{ 
	    double t = clientBuffer;
	    clientBuffer = EmptyBuffer;
	    phase = Phase.Send;
	    return t;
	}

	public static bool ClientCloseEnabled() 
	{ 
	    return (clientSocket == Socket.Connected 
		    && phase == Phase.Send);
	}

	[Action] public static void ClientClose() 
	{ 
	    clientSocket = Socket.Closed; 
	}
    }


    public static class Factory
    {
        public static ModelProgram Create()
        {
            return new LibraryModelProgram(typeof(Factory).Assembly, "ClientServer");
        }
    }
}
