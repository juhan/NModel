using System;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using Chat.Library;

namespace Chat.TcpServer
{
    public class Program 
    {
        public static void Main(string [] args) 
        {
            int tries = 5;
            while (tries-- >0) 
            {
                try 
                {
                    IDictionary props = new Hashtable(); 
                    props["port"] = 8085; 
                    BinaryClientFormatterSinkProvider clientFormatterProvider = new BinaryClientFormatterSinkProvider();
                    BinaryServerFormatterSinkProvider serverFormatterProvider = new BinaryServerFormatterSinkProvider();
                    serverFormatterProvider.TypeFilterLevel = TypeFilterLevel.Full;
                    TcpChannel chan = new TcpChannel(props, clientFormatterProvider, serverFormatterProvider);
                    ChannelServices.RegisterChannel(chan,false);
                    RemotingConfiguration.RegisterWellKnownServiceType(
                        typeof(Server), 
                        "ChatServer", WellKnownObjectMode.Singleton);
                    break;
                } 
                catch 
                {
                    if (tries==0)
                        throw;
                    Console.WriteLine("couldn't start listening on TCP/IP port, will retry in a second");
                    tries--;
                    Thread.Sleep(1000);
                }
            }


            Console.WriteLine("Chat Server running.");
            Console.WriteLine("Hit enter to exit...");
            Console.ReadLine();
        }
    }
   
}
