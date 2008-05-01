using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Attributes;
using NModel.Execution;
using MessageQueue = NModel.Sequence<string>;

namespace Chat.Model
{
    /// <summary>
    /// <para>This class expresses the behavioral contract of a simple chat application.</para>
    /// 
    /// <para>The possible actions are given by each method with an [Action] attribute.</para>
    /// 
    /// <para>Actions are enabled when all of the enabling conditions return "true". Enabling conditions
    /// are given by attribute and refer to a method (static or instance-based) defined within the current 
    /// (innermost) class scope. The parameters of the enabling methods must be a pairwise match 
    /// of those of the action method, although fewer parameters may be given
    /// in the enabling method.</para>
    /// 
    /// <para>The parameter domains are specified using any of three attributes: [_], [New] and [Domain]. 
    /// [_] indicates that any value is permitted. In this case, the parameter must be unreferenced by the model.
    /// [New] indicates that a new value of the type will be imported by parameter generation. This is used, for example,
    /// when creating new dynamic instances.
    /// [Domain] names a parameter-generating function defined in the current class scope. The domain method
    /// returns a set of the given type.</para>
    /// 
    /// <para>Action methods may be static or instance-based.</para>
    /// 
    /// <para>The state of the model consists of all fields defined by any class with a [Model] attribute
    /// that contains the name of the model. State may be distributed among multiple classes. Both static
    /// and instance fields contribute to state. The [ExcludeFromState] attribute may be used to mark fields
    /// (such as debugging fields) that should not be part of model state.</para>
    /// </summary>
    public static class Contract
    {
        [TransitionProperty("StateInvariant")]
        static bool IsOK() { return true; }

        [TransitionProperty("AcceptingStateCondition")]
        static bool IsAccepting() { return true; }

        /// <summary>
        /// The set of all clients present in the chat model.
        /// </summary>
        /// <returns>The set of all clients present in the chat model.</returns>
        static Set<Client> ClientDomain()
        {
            return Client.Domain;
        }

        static public int MembersCount // Number of clients that have entered the session.
        {
            get
            {
                Predicate<Client> p = delegate(Client c) { return c.entered; };
                return Client.Domain.Select(p).Count;
            }
        }

        #region Create Action

        static public bool CreateEnabled() { return true; }

        static public bool CreateEnabled(Client c) { return !Client.Domain.Contains(c); }
        
        /// <summary>
        /// Create(c) allows a new client with new id to come into existence. 
        /// New clients are initially not part of the session (enter must be called for this to happen).
        /// </summary>
        [Action("Create")]
        static public void Create([Domain("new")] Client c)
        {
            foreach (Client d in Client.Domain)
            {
                d.unreceivedMsgs = d.unreceivedMsgs.Add(c, Sequence<string>.EmptySequence);
                c.unreceivedMsgs = c.unreceivedMsgs.Add(d, Sequence<string>.EmptySequence);
            }
            Client.Domain = Client.Domain.Add(c);
        }
        #endregion

        #region Enter Action

        static public bool EnterEnabled() { return MembersCount < Client.Domain.Count; }

        static public bool EnterEnabled(Client c) { return Client.Domain.Contains(c) && !c.entered; }

        /// <summary>
        /// A client that is not already a member of the chat session may enter the session. 
        /// The client c becomes a member of the chat session when Enter(c) is called.
        /// </summary>
        [Action("Enter")]
        static public void Enter([Domain("ClientDomain")] Client c)
        {
            c.entered = true;
        }

        #endregion

        #region Send Action

        static public bool SendEnabled() { return MembersCount > 1; }

        static public bool SendEnabled(Client c) { return Client.Domain.Contains(c) && c.entered; }

        /// <summary>
        /// A member of the chat session may send a new message to all the other members to receive 
        /// by invoking the Send() method. This adds a new message at the end of the queue of 
        /// pending messages for each receiver. 
        /// </summary>
        [Action("Send")]
        static public void Send([Domain("ClientDomain")] Client sender, string message)
        {
            foreach (Client c in Client.Domain)
                //send only to members other than the sender
                if (c.entered && sender != c)
                {
                    MessageQueue queue = c.unreceivedMsgs[sender].AddLast(message);
                    c.unreceivedMsgs = c.unreceivedMsgs.Override(sender, queue);
                }
        }


        #endregion

        #region Receive Action

        static bool ReceiveEnabled()
        {
            foreach (Client c in Client.Domain)
            {
                if (c.entered)
                    foreach (Sequence<string> q in c.unreceivedMsgs.Values)
                        if (q.Count > 0) return true;
            }
            return false;
        }

        public static bool ReceiveEnabled(Client sender, string message, Client recipient)
        {
            return Client.Domain.Contains(sender) &&
                   Client.Domain.Contains(recipient) &&
                   sender.entered &&
                   recipient.entered &&
                   sender != recipient &&
                   recipient.unreceivedMsgs[sender].Count > 0 &&
                   recipient.unreceivedMsgs[sender].Head == message;
        }

        /// <summary>
        /// Receive() is a notification callback that occurs whenever the chat server 
        /// forwards a particular message from a particular sender to a particular recipient. 
        /// Receive() can be observed when the recipient's state 
        /// (given by the unreceivedMsgs field) allows it. 
        /// The message received must be the current message of the sender. 
        /// This requirement ensures the "local consistency" property described above.
        /// When a Receive() notification is observed, the delivered message 
        /// is removed from the current queue of messages from the sender that 
        /// are pending for this recipient.
        /// </summary>
        [Action("Receive")]
        public static void Receive(
            [Domain("ClientDomain")] Client sender,
            [Domain("Receive_Message_Domain")] string message,
            [Domain("ClientDomain")] Client recipient)
        {
            MessageQueue q = recipient.unreceivedMsgs[sender].Tail;
            recipient.unreceivedMsgs = recipient.unreceivedMsgs.Override(sender, q);
        }

        public static Set<string> Receive_Message_Domain()
        {
            Set<string> result = Set<string>.EmptySet;
            foreach (string s in PendingMessages())
                result = result.Add(s);
            return result;
        }

        static IEnumerable<string> PendingMessages()
        {
            foreach (Client c in Client.Domain)
                foreach (Sequence<string> queue in c.unreceivedMsgs.Values)
                    foreach (string message in queue)
                        yield return message;
        }
        #endregion

        /// <summary>
        /// Factory method
        /// </summary>
        /// <returns>ChatModel</returns>
        public static ModelProgram Make()
        {
            return new LibraryModelProgram(typeof(Contract).Assembly, typeof(Contract).Namespace);
        }
    }

    /// <summary>
    /// Client objects are chat clients that have been created
    /// </summary>
    [Sort("Client")]
    public class Client : LabeledInstance<Client>
    {
        public static Set<Client> Domain = Set<Client>.EmptySet;

        public bool entered = false;

        public Map<Client, Sequence<string>> unreceivedMsgs = Map<Client, Sequence<string>>.EmptyMap;

        public Client() : base() { }
        public override void Initialize()
        {
            this.entered = false;
            this.unreceivedMsgs = Map<Client, Sequence<string>>.EmptyMap;
        }

        [TransitionProperty("StateInvariant")]
        static bool IsOK() { return true; }

        [TransitionProperty("AcceptingStateCondition")]
        static bool IsAccepting() 
        {
            foreach (Client c1 in Domain)
            {
                foreach (Pair<Client, Sequence<string>> kv in c1.unreceivedMsgs)
                    if (kv.Second.Count > 0) return false;
            }
            return true;
        }
    }
}
