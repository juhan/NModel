using System;
using System.Collections.Generic;
using NModel;
using NModel.Terms;
using NModel.Conformance;
using Chat.Library;

namespace Chat.TestHarness
{
    /// <summary>
    /// Implements the stepper interface required by the conformance engine
    /// </summary>
    public class ChatStepper : IAsyncStepper
    {
        public ChatStepper()
        {
        }

        //factory method
        public static ChatStepper Create()
        {
            return new ChatStepper();
        }

        static ObserverDelegate observer;

        internal static void CallObserver(CompoundTerm action)
        {
            if (observer != null)
            {
                //System.Windows.Forms.MessageBox.Show(action.ToString());
                observer(action);
            }
        }

        static Dictionary<string, Client> clients = new Dictionary<string, Client>();

        internal static void Receive(Client sender, string message, Client receiver)
        {
            if (sender != receiver) //ignore messages sent back to the sender
            {
                CompoundTerm action = new CompoundTerm(new Symbol("Receive"),
                    new Sequence<Term>(Term.Parse(sender.Name),
                    new Literal(message), Term.Parse(receiver.Name)));
                CallObserver(action);
            }
        }

        internal static void PrivateReceive(Client sender, string message, Client receiver)
        {
            CompoundTerm action = new CompoundTerm(new Symbol("PrivateReceive"),
                new Sequence<Term>(Term.Parse(sender.Name), 
                new Literal(message), Term.Parse(receiver.Name)));
            CallObserver(action);
        }

        #region IStepper Members

        public CompoundTerm DoAction(String actionString) { return DoAction((CompoundTerm)Term.Parse(actionString)); }

        public CompoundTerm DoAction(CompoundTerm action)
        {
            switch (action.FunctionSymbol.ToString())
            {
                case "Setup_Start":
                    lock (this)
                    {
                        Global.Setup();
                        return CompoundTerm.Create("Setup_Finish");
                    }
                case "Create":
                    lock (this) //avoid simultaneous creation of clients
                    {
                        //System.Diagnostics.Debugger.Break();
                        //System.Windows.Forms.MessageBox.Show("Going to create client " + action.Arguments[0].ToString());//???
                        clients[action.Arguments[0].ToString()] =
                            Global.CreateClient(action.Arguments[0].ToString());
                       // //System.Windows.Forms.MessageBox.Show("Created client " + action.Arguments[0].ToString());//???
                    }
                    return null;
                case "Enter":
                    lock (this) //avoid simultaneous entering of clients
                    {
                        Global.Enter(clients[action.Arguments[0].ToString()]);
                    }
                    return null;
                case "Exit":
                    lock (this)
                    {
                        Global.Exit(clients[action.Arguments[0].ToString()]);
                    }
                    return null;
                case "Send":
                    lock (this)
                    {
                        string message = (string)((Literal)action.Arguments[1]).Value;
                        clients[action.Arguments[0].ToString()].PostMessage(message);
                    }
                    return null;
                default:
                    throw new Exception("Unexpected action: " + action);

            }
        }

        public void Reset()
        {
            lock (this)
            {
                clients.Clear();
                Global.Teardown();
            }
        }

        public void SetObserver(ObserverDelegate obs)
        {
            observer = obs;
        }

        #endregion
    }
}