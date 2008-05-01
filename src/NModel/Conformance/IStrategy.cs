//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
//using System.Text;
using NModel.Terms;
using NModel.Execution;
using NModel;

namespace NModel.Conformance
{
    /// <summary>
    /// A strategy is a stepper of a model program.
    /// Enabledness of actions is checked explicitly (exceptions are not thrown).
    /// Controllable actions can be enumerated.
    /// </summary>
    public interface IStrategy
    {
        /// <summary>
        /// Make a step according to the given action, the current state
        /// becomes the target state of this transition.
        /// The action is required to be enabled in the current state.
        /// </summary>
        void DoAction(CompoundTerm action);

        /// <summary>
        /// The action symbols of the model
        /// </summary>
        Set<Symbol> ActionSymbols { get;}

        /// <summary>
        /// The observable action symbols of the strategy
        /// </summary>
        Set<Symbol> ObservableActionSymbols { get; set;}

        /// <summary>
        /// Return to the initial state of the model.
        /// </summary>
        void Reset();

        /// <summary>
        /// Returns true if the given action is enabled in the current state.
        /// </summary>
        /// <param name="action">The action to be tested</param>
        /// <param name="failureReason">Null if result is true; otherwise a string describing the requirement that was not met.</param>
        /// <returns>True if <paramref name="action"/> is enabled; false otherwise.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        bool IsActionEnabled(CompoundTerm action, out string failureReason);

        /// <summary>
        /// The current state of the model.
        /// </summary>
        IState CurrentState { get;}

        /// <summary>
        /// Returns true if the state is an accepting state, and thus may be used
        /// to terminate a test case.
        /// </summary>
        bool IsInAcceptingState { get;}

        /// <summary>
        /// Select a concrete action that is enabled in the current state
        /// and whose action symbol is in the set <paramref name="actionSymbols"/>
        /// using a particular strategy.
        /// </summary>
        /// <param name="actionSymbols">set of candidate action symbols</param>
        /// <returns>the selected action or null if no choice is possible</returns>
        CompoundTerm/*?*/ SelectAction(Set<Symbol> actionSymbols);
    }
}
