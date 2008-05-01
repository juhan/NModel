using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.Modeling;
using Microsoft.Modeling.Terms;

namespace Microsoft.NModel.Conformace
{
    /// <summary>
    /// Observes and records actions from an IUT
    /// </summary>
    public interface IObservationCallback
    {
        /// <summary>
        /// Enqueue an action
        /// </summary>
        void Enqueue(Term action);
        /// <summary>
        /// Maps an event to the corresponding action symbol
        /// </summary>
        Symbol GetActionSymbol(EventInfo e);
    }
    /// <summary>
    /// Converts an observation into an term and enqueues it.
    /// </summary>
    public class EventHandlerRepositioryHelper
    {
        /// <summary>
        /// No instance of this class can be created
        /// </summary>
        private EventHandlerRepositioryHelper() { }

        /// <summary>
        /// Enqueue the observed invocation in the observation queue of the machine
        /// </summary>
        /// <param name="e">event corresponding to the action</param>
        /// <param name="o">IObservationCallback ob</param>
        /// <param name="args">parameters of the action</param>
        public static void __OnEvent(EventInfo e, object o, object[] args)
        {
            IObservationCallback oc = o as IObservationCallback;
            if (oc == null || e == null)
                throw new Exception("__OnEvent");
            Symbol actionSymbol = oc.GetActionSymbol(e);
            Term obs = ConstructInvocationTerm(actionSymbol, args);
            oc.Enqueue(obs);
        }

        static Term ConstructInvocationTerm(Symbol actionSymbol, object[] args)
        {
            throw new NotImplementedException("ConstructInvocationTerm");
        }

    }
}
