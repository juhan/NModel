using System;
using NModel.Conformance;
using NModel.Terms;

namespace ClientServerImpl
{
    public class Stepper: IStepper
    {
	const int port = 8000;
	const string host = "127.0.0.1"; // localhost

	Server s = new Server();
	Client c = new Client();

	public CompoundTerm DoAction(CompoundTerm action)
	{
	    switch (action.Name)
	    {
	        case("Tests"): return null; // first action in test seq.

	        case("ServerSocket"): 
		    s.Socket(); return null;
	        case("ServerBind"):
		    s.Bind(host,port); return null;
	        case("ServerListen"):
		    s.Listen(); return null;
	        case("ServerAccept"):
		    s.Accept(); return null;
 	        case("ServerReceive"):
                    s.Receive(); return null;
     	        case("ServerSend"):
		    // s.Send sends a double, not a string!
                    s.Send((double)((Literal) action.Arguments[0]).Value); 
		    return null;
	        case("ServerCloseConnection"):
		    s.CloseConnection(); return null;
	        case("ServerClose"):
		    s.Close(); return null;

	        case("ClientSocket"):
		    c.Socket(); return null;
    	        case("ClientConnect"):
		    c.Connect(host,port); return null;
	        case("ClientSend"):
    		    c.Send("T"); return null;
	        case("ClientReceive_Start"):
		    // c.Receive returns a double, not a string
		    return CompoundTerm.Create("ClientReceive_Finish",
					       c.Receive());
	        case("ClientClose"):
		    c.Close(); return null;

                default: throw new Exception("Unexpected action " + action);
            }
        }

        public void Reset()
        {
	    s = new Server(); 
	    c = new Client();
        }

        public static IStepper Create()
        {
            return new Stepper();
        }
    }
}
