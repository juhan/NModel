//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Terms;
using Action = NModel.Terms.CompoundTerm;
using System.Threading;
using NModel.Execution;

namespace NModel.Conformance
{
    /// <summary>
    /// Provides a basic strategy for a model program.
    /// Selects actions randomly.
    /// </summary>
    public class Strategy : IStrategy
    {
        /// <summary>
        /// Model program of the model stepper
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected ModelProgram modelProgram;
        /// <summary>
        /// Current state of the model stepper
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IState currState;
        /// <summary>
        /// Initial state of the model stepper
        /// </summary>
        IState initialState;
        /// <summary>
        /// Action symbols in the vocabulary
        /// </summary>
        internal Set<Symbol> AllActionSymbols
        {
            get
            {
                return modelProgram.ActionSymbols();
            }
        }

        Set<string> coverageNames = Set<string>.EmptySet;

        /// <summary>
        /// Names of coverage points used by the strategy
        /// </summary>
        public Set<string> CoverageNames
        {
            get { return coverageNames; }
            set { coverageNames = value; }
        }

        /// <summary>
        /// Construct a strategy with random action selection from the given model program.
        /// </summary>
        /// <param name="modelProgram">model program</param>
        public Strategy(ModelProgram modelProgram)
        {
            if (modelProgram == null)
                throw new ArgumentNullException("modelProgram");
            this.modelProgram = modelProgram;
            this.currState = modelProgram.InitialState;
            this.initialState = modelProgram.InitialState;
        }

        /// <summary>
        /// Create a strategy with random action selection from the given model program.
        /// </summary>
        /// <param name="modelProgram">model program</param>
        /// <param name="coverage">coverage point names (this argument is ignored)</param>
        public static IStrategy Create(ModelProgram modelProgram, string[] coverage)
        {
            Strategy s = new Strategy(modelProgram);
            s.CoverageNames = new Set<string>(coverage);
            return s;
        }

        /// <summary>
        /// Yields all the enabled actions with the given action symbols 
        /// in the current model state. 
        /// </summary>
        /// <param name="actionSymbols">given action symbols</param>
        /// <returns></returns>
        public IEnumerable<Action> GetEnabledActions(Set<Symbol> actionSymbols)
        {
            foreach (Symbol s in actionSymbols)
                if (modelProgram.ActionSymbols().Contains(s))
                    foreach (Action a in modelProgram.GetActions(currState, s))
                        yield return a;
        }


        #region IStrategy Members

        /// <summary>
        /// Returns true if the action is enabled in the current state.
        /// If the action is not enabled, provides a reason in <paramref name="failureReason"/>.
        /// </summary>
        /// <param name="action">action whose enabledness is being checked</param>
        /// <param name="failureReason">failure reason if the action is not enabled</param>
        /// <returns>true if the action is enabled, false otherwise</returns>
        public bool IsActionEnabled(CompoundTerm action, out string failureReason)
        {
            if (!AllActionSymbols.Contains(action.Symbol))
            {
                failureReason = "Action symbol '" + action.Symbol.ToString() + "' not enabled in the model";
                return false;
            }
            else
            {
                bool isEnabled = modelProgram.IsEnabled(currState, action);
                if (!isEnabled)
                {
                    failureReason = "Action '" + ConformanceTester.MakeQuotedString(action.ToString()) + "' not enabled in the model";
                    
                    foreach (string s in modelProgram.GetEnablingConditionDescriptions(currState, action, true))
                    {
                        failureReason += "\n";
                        failureReason += s;
                    }
                    return false;
                }
                else
                {
                    failureReason = "";
                    return true;
                }
            }
        }

        /// <summary>
        /// The current state
        /// </summary>
        public IState CurrentState
        {
            get { return currState; }
        }

        /// <summary>
        /// Update the current state to the target state of the action from the current state.
        /// Can be overwritten in a derived class to record history that affects action selection strategy.
        /// </summary>
        /// <param name="action">given action</param>
        public virtual void DoAction(CompoundTerm action)
        {
            //history.Update(this.CurrentState, action);
            TransitionProperties transitionProperties;
            currState = modelProgram.GetTargetState(currState, action, Set<string>.EmptySet, out transitionProperties);
        }

        /// <summary>
        /// Reset the model stepper to the initial state of the model.
        /// Can be overwritten in a derived class to record history that affects action selection strategy.
        /// </summary>
        public virtual void Reset()
        {
            currState = initialState;
        }

        /// <summary>
        /// Returns true if the current state is an accepting state.
        /// </summary>
        public bool IsInAcceptingState
        {
            get { return modelProgram.IsAccepting(currState); }
        }

        /// <summary>
        /// The action symbols in the vocabulary.
        /// </summary>
        public Set<Symbol> ActionSymbols
        {
            get { return AllActionSymbols; }
        }

        /// <summary>
        /// Select an action that is enabled in the current state
        /// and whose action symbol is in the set <paramref name="actionSymbols"/>.
        /// Default action selection strategy is random.
        /// </summary>
        /// <param name="actionSymbols">set of candidate action symbols</param>
        /// <returns>the chosen action or null if no choice is possible</returns>
        /// <remarks>Can be overwritten in a dervied class to encorporate different 
        /// action selection strategies.</remarks>
        public virtual CompoundTerm SelectAction(Set<Symbol> actionSymbols)
        {
            if (actionSymbols == null)
                throw new ArgumentNullException("actionSymbols");

            if (actionSymbols.IsEmpty)
                return null;

            Sequence<CompoundTerm> actions = new Sequence<CompoundTerm>(this.GetEnabledActions(actionSymbols));
            
            if (actions.IsEmpty)
                return null;

            return actions.Choose();
        }

        internal Set<Symbol> observableActionSymbols = Set<Symbol>.EmptySet;
        /// <summary>
        /// Observable action symbols of the strategy
        /// </summary>
        public Set<Symbol> ObservableActionSymbols
        {
            get
            {
                return observableActionSymbols;
            }
            set
            {
                observableActionSymbols = value;
            }
        }
        #endregion
    }
}
