using System;
using System.Collections.Generic;
using System.Text;
using NModel.Attributes;
using NModel.Execution;
using NModel;

namespace Chat.Scenario
{
    public class ChatScenario 
    {
        static readonly Set<string> allModes = new Set<string>("Uninitialized", "Creating", "Entering", "Messaging");

        static public string mode = "Uninitialized";
        static public int nrOfClients = 2;
        static public int nrOfSends = 2;

        [TransitionProperty("StateInvariant")]
        static bool IsOK()
        {
            return allModes.Contains(mode);
        }

        [TransitionProperty("AcceptingStateCondition")]
        static bool IsAccepting()
        {
            return (mode == "Messaging");
        }

        #region action Setup
        static bool SetupEnabled()
        {
            return mode == "Uninitialized";
        }       
        
        [Action("Setup"), Split]
        static public void Setup()
        {
            mode = "Creating";
        }


        #endregion

        #region action Create

        static bool CreateEnabled() { return mode == "Creating"; }

        [Action]
        static public void Create()
        {
            nrOfClients -= 1;
            if (nrOfClients == 0) mode = "Entering";
        }

        #endregion

        #region action Enter

        static bool EnterEnabled() { return mode == "Entering"; }        
        
        [Action]
        static public void Enter()
        {
            nrOfClients += 1;
            if (nrOfClients == 2) mode = "Messaging";
        }

        #endregion

        #region action Send

        static Set<string> SomeMessages()
        {
            return new Set<string>("hi", "bye");
        }        
        
        static bool SendEnabled()
        {
            return mode == "Messaging" && (nrOfSends > 0); 
        }

        [Action("Send(_, message)")]
        static public void Send([Domain("SomeMessages")] string message)
        {
            nrOfSends -= 1;
        }

        #endregion

        #region action Receive

        static bool ReceiveEnabled() { return mode == "Messaging"; }  
        
        [Action("Receive(_, message, _)")]
        static public void Receive(   
            [Domain("SomeMessages")]string message
            )
        {
        }


        #endregion

        /// <summary>
        /// Factory method
        /// </summary>
        /// <returns>ChatScenario</returns>
        public static ModelProgram Make()
        {
            return new LibraryModelProgram(typeof(ChatScenario).Assembly, typeof(ChatScenario).Namespace);
        }

    }

    /// <summary>
    /// Scenario client
    /// </summary>
    [Sort("Client")]
    public class SClient : LabeledInstance<SClient> { }
}
