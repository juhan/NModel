//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Terms;

namespace NModel.Conformance
{

    /// <summary>
    /// Delegate that is used by the implementation to notify about observable actions.
    /// </summary>
    /// <param name="action">observable action</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public delegate void ObserverDelegate(CompoundTerm action);

    /// <summary>
    /// Must be implemented by an IUT for conformance testing
    /// </summary>
    public interface IStepper
    {

        /// <summary>
        /// Make a step according to the given action, the current state
        /// becomes the target state of this transition.
        /// If the action is not enabled an exception is thrown and the 
        /// resulting state is undefined.
        /// An action on null may be returned.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        CompoundTerm DoAction(CompoundTerm action);

        /// <summary>
        /// Return to the initial state.
        /// If Reset is not enabled in the current state, an exception is thrown 
        /// and the resulting state is undefined
        /// and is thus not guaranteed to be the initial state
        /// </summary>
        void Reset();
    }


    /// <summary>
    /// Must be implemented by an IUT for conformance testing with asynchronous observables
    /// </summary>
    public interface IAsyncStepper : IStepper
    {
        /// <summary>
        /// Sets the observer callback. The observer is called by the IUT each time an 
        /// observable action happens.
        /// </summary>
        /// <param name="observer">the provided observer</param>
        void SetObserver(ObserverDelegate observer);
    }
}
