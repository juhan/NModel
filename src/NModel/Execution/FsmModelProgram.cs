//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Terms;
using Transition = NModel.Triple<NModel.Terms.Term, NModel.Terms.CompoundTerm, NModel.Terms.Term>;


namespace NModel.Execution
{
    #region FSM Control State
    public sealed class FsmState : CompoundValue, IState
    {
        /// <summary>
        /// DFA states are sets of NFA states. The set represents all of the possible
        /// NFA states that are possible given the trace so far.
        /// </summary>
        readonly Set<Term> automatonStates;

        // must be kept in sync with list of fields
        public override IEnumerable<IComparable> FieldValues()
        {
            yield return this.automatonStates;
        }

        public Set<Term> AutomatonStates { get { return this.automatonStates; } }

        public FsmState(Set<Term> automatonStates)
        {
            this.automatonStates = automatonStates;
        }

        #region IState Members

        public Term ControlMode
        {
            get
            {
                if (this.automatonStates.Count == 1)
                    //if the state is singleton set, just use the term in that state
                    return this.automatonStates.Choose();
                else
                    return new CompoundTerm(new Symbol("Set", new Symbol("Term")), new Sequence<Term>(this.automatonStates));
            }
        }

        //public Term GetLocationValue(int i)
        //{
        //    throw new ArgumentOutOfRangeException("i");
        //}

        //public int LocationValuesCount
        //{
        //    get { return 0; }
        //}

        //public Map<string, int> DomainMap
        //{
        //    get { return Map<string, int>.EmptyMap; }
        //}

        #endregion
    }
    #endregion
    /// <summary>
    /// A model program constructed from a (potentially nondeterministic finite automaton). The internal
    /// states of the model program are chosen so that actions are deterministic.
    /// </summary>
    public class FsmModelProgram : ModelProgram, IName
    {
  
        #region Parameter id 
        /// <summary>
        /// Value type of parameter indices of a finite automaton-- a triple (state, actionSymbol, integerIndex)
        /// </summary>
        sealed class Parameter : CompoundValue
        {
            readonly FsmState state;
            readonly Symbol actionSymbol;
            readonly int index;
            
            // must be kept in sync with list of fields!
            /// <summary>
            /// Enumerates the constituents of this compound value.
            /// </summary>
            /// <returns></returns>
            public override IEnumerable<IComparable> FieldValues()
            {
                yield return this.state;
                yield return this.actionSymbol;
                yield return this.index;
            }

            /// <summary>
            /// The start state of the step.
            /// </summary>
            public FsmState State
            {
                get { return state; }
            }

            /// <summary>
            /// The action label of the step.
            /// </summary>
            public Symbol ActionSymbol
            {
                get { return actionSymbol; }
            }

            /// <summary>
            /// The target data state of the step.
            /// </summary>
            public int Index
            {
                get { return index; }
            }

            public Parameter(FsmState state, Symbol actionSymbol, int index)
            {
                this.state = state;
                this.actionSymbol = actionSymbol;
                this.index = index;
            }
        }
        #endregion

        FSM automaton;                         // underlying NFA
        Dictionary<Symbol, int> actionArities;             // cache: number of parameters required by each symbol
        Dictionary<Parameter, Set<Term>> parameterDomains; // cache: per-argument domains
        Dictionary<Term, Set<Symbol>> potentiallyEnabled;  // cache: actions per NFA state

        readonly string name;


        /// <summary>
        /// Constructs a model program from a (potentially) nondeterministic finite automaton. The internal
        /// states of the model program are chosen so that actions are deterministic.
        /// </summary>
        /// <param name="automaton">The underlying finite automaton that will provide the steps</param>
        /// <param name="modelName">A string identifying this model program</param>
        public FsmModelProgram(FSM automaton, string modelName)
        {
            Dictionary<Term, Set<Symbol>> potentiallyEnabled = new Dictionary<Term, Set<Symbol>>();
            Dictionary<Symbol, int> actionArities = new Dictionary<Symbol, int>();

            foreach (Transition transition in automaton.Transitions)
            {
                // collect action symbol
                CompoundTerm action = transition.Second as CompoundTerm;
                if (action == null)
                    throw new ArgumentException("Expected CompoundTerm, saw " + transition.Second.GetType().ToString());

                Symbol actionSymbol = action.Symbol;

                // collect arity
                int existingArity;
                if (!actionArities.TryGetValue(actionSymbol, out existingArity))
                    actionArities.Add(actionSymbol, action.Arguments.Count);
                else if (existingArity != action.Arguments.Count)
                    throw new ArgumentException("Inconsistent arity for action symbol " + actionSymbol.ToString());

                // collect potentially enabled
                Set<Symbol> actions1;
                if (!potentiallyEnabled.TryGetValue(transition.First, out actions1))
                    actions1 = Set<Symbol>.EmptySet;
                potentiallyEnabled[transition.First] = actions1.Add(actionSymbol);
            }

            if (automaton.Transitions.IsEmpty)
                throw new ArgumentException("Finite automaton must contain at least one transition");

            foreach (Symbol sym in automaton.Vocabulary)
            {
                if (!actionArities.ContainsKey(sym))
                    actionArities[sym] = 0;
            }

            this.automaton = automaton;
            this.actionArities = actionArities;
            this.parameterDomains = new Dictionary<Parameter, Set<Term>>();
            this.potentiallyEnabled = potentiallyEnabled;
            this.name = modelName;
        }

        #region ModelProgram Members

        /// <summary>
        /// Nonempty set of actions symbols.
        /// <br><c>ensures result.Count &gt; 0</c></br>
        /// </summary>
        public override Set<Symbol> ActionSymbols()
        {
            return this.automaton.Vocabulary;
        }

        /// <summary>
        /// Number of arguments associated with action symbol <paramref name="actionSymbol"/>
        /// <br><c>requires actionSymbol != null;</c></br>
        /// <br><c>requires this.ActionSymbols.Contains(actionSymbol);</c></br>
        /// <br><c>ensures result &gt;= 0</c></br>
        /// </summary>
        /// <param name="actionSymbol">A symbol naming an action of this model program.</param>
        /// <returns>The number of arguments required in a <see cref="Term"/> invoking this <paramref name="actionSymbol"/></returns>
        public override int ActionArity(Symbol actionSymbol)
        {
           int arity;
           if (this.actionArities.TryGetValue(actionSymbol, out arity))
               return arity;
           else
               throw new ArgumentException("Action symbol " + actionSymbol.ToString() + " not in signature of this model program.");
        }

        static Symbol AnySort = new Symbol("Object");

        /// <summary>
        /// Gets the sort (i.e., abstract type) of the ith parameter of action <paramref name="actionSymbol"/>
        /// <br><c>requires actionSymbol != null;</c></br>
        /// <br><c>requires this.ActionSymbols.Contains(actionSymbol);</c></br>
        /// </summary>
        /// <param name="actionSymbol">A symbol naming an action of this model program.</param>
        /// <param name="parameterIndex">An integer in the interval [0, this.ActionArity(actionSymbol))</param>
        /// <returns>The sort (abstract type) of the ith parameter of action <paramref name="actionSymbol"/></returns>
        public override Symbol ActionParameterSort(Symbol actionSymbol, int parameterIndex)
        {
            return AnySort;
        }

        //public ActionRole GetActionRole(Symbol actionSymbol)
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}

        /// <summary>
        /// Number of location values in the state signature of this model program
        /// <br><c>ensures result >= 0;</c></br>
        /// </summary>
        public override int LocationValueCount
        {
            get { return 0; }
        }

        /// <summary>
        /// String name identifying the ith location value in the state signature of this model program
        /// <br><c>requires 0 &lt;= i &amp;&amp; i &lt; this.LocationValueCount;</c></br>
        /// </summary>
        /// <param name="i">An index in the interval [0,  LocationValueClount)</param>
        /// <returns>String name identifying the ith location value</returns>
        public override string LocationValueName(int i)
        {
           throw new ArgumentOutOfRangeException("i");
        }

        /// <summary>
        /// It is an error to call this method.
        /// Throws NotImplementedException.
        /// </summary>
        public override string LocationValueModelName(int i)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the sort representing a set of terms
        /// </summary>
        public override Symbol LocationValueSort(int i)
        {
            return Symbol.Parse("Set<Term>");
        }

        /// <summary>
        /// Initial state
        /// </summary>
        public override IState InitialState
        {
            get { return new FsmState(new Set<Term>(this.automaton.InitialState)); }
        }

        /// <summary>
        /// Returns true if the action symbol is potentially enabled in the given state
        /// </summary>
        public override bool IsPotentiallyEnabled(IState state, Symbol actionSymbol)
        {
            FsmState fs = state as FsmState;
            if (fs == null)
                throw new ArgumentException("Invalid state");

            if (!this.ActionSymbols().Contains(actionSymbol))
                throw new ArgumentException("Action symbol " + actionSymbol.ToString() + " not in signature.");

            Set<Symbol> stateActionSymbols;
            foreach (Term automatonState in fs.AutomatonStates)
                if (this.potentiallyEnabled.TryGetValue(automatonState, out stateActionSymbols) &&
                   stateActionSymbols.Contains(actionSymbol))
                    return true;
            return false;
        }

        /// <summary>
        /// Returns the set of all potentially enabled action symbols
        /// </summary>
        public override Set<Symbol> PotentiallyEnabledActionSymbols(IState state)
        {
            FsmState fs = state as FsmState;
            if (fs == null)
                throw new ArgumentException("Invalid state");

            Set<Symbol> result = Set<Symbol>.EmptySet;
            foreach (Term automatonState in fs.AutomatonStates)
            {
                Set<Symbol> stateActionSymbols;
                if (potentiallyEnabled.TryGetValue(automatonState, out stateActionSymbols))
                    result = result.Union(stateActionSymbols);
            }
            return result;
        }

        /// <summary>
        /// Does this model program have an interface to parameter generation for this parameter?
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actionSymbol"></param>
        /// <param name="parameterIndex"></param>
        /// <returns>A set of terms representing the possible values</returns>
        public override bool HasActionParameterDomain(IState state, Symbol actionSymbol, int parameterIndex)
        {
            FsmState fs = state as FsmState;
            if (fs == null)
                throw new ArgumentException("Invalid state");

            if (!this.ActionSymbols().Contains(actionSymbol))
                throw new ArithmeticException("Symbol " + actionSymbol.ToString() + "not in signature.");

            if (parameterIndex >= ActionArity(actionSymbol))
                return false;

            Parameter p = new Parameter(fs, actionSymbol, parameterIndex);
            Set<Term> domain;
            if (!parameterDomains.TryGetValue(p, out domain))
            {
                domain = Set<Term>.EmptySet;
                foreach (Term automatonState in fs.AutomatonStates)
                    domain = domain.Union(GetAutomatonParameterDomain(automatonState, actionSymbol, parameterIndex));
                this.parameterDomains[p] = domain;
            }
            // return parameterDomains.ContainsKey(p);
            return !domain.IsEmpty;
        }

        /// <summary>
        /// Action parameter domain of the given action and parameter index in the given state
        /// </summary>
        public override Set<Term> ActionParameterDomain(IState state, Symbol actionSymbol, int parameterIndex)
        {
            FsmState fs = state as FsmState;
            if (fs == null)
                throw new ArgumentException("Invalid state");

            if (!this.ActionSymbols().Contains(actionSymbol))
                throw new ArithmeticException("Symbol " + actionSymbol.ToString() + "not in signature.");

            if (parameterIndex >= ActionArity(actionSymbol))
                return new Set<Term>(Any.Value);

            Parameter p = new Parameter(fs, actionSymbol, parameterIndex);
            Set<Term> domain;
            if (!parameterDomains.TryGetValue(p, out domain))
            {
                domain = Set<Term>.EmptySet;
                foreach(Term automatonState in fs.AutomatonStates)
                    domain = domain.Union(GetAutomatonParameterDomain(automatonState, actionSymbol, parameterIndex));
                this.parameterDomains[p] = domain;
            }
            return domain;               
        }

        Set<Term> GetAutomatonParameterDomain(Term startState, Symbol actionSymbol, int parameterIndex)
        {
            Set<Term> result = Set<Term>.EmptySet;
            Set<Transition> outgoing = this.automaton.OutgoingTransitions(startState);
            foreach (Transition transition in outgoing)
            {
                CompoundTerm ct = transition.Second as CompoundTerm;
                if (ct == null || parameterIndex >= ActionArity(actionSymbol))
                    throw new InvalidOperationException("Internal error");
                if (actionSymbol.Equals(ct.Symbol))
                    result = result.Add(ct.Arguments[parameterIndex]);
            }
            return result;
        }

        static bool IsCompatibleTerm(CompoundTerm action, CompoundTerm fsmAction)
        {
            if (!action.Symbol.Equals(fsmAction.Symbol))
                return false;
            IEnumerator<Term> fsmActionArgs = fsmAction.Arguments.GetEnumerator();
            IEnumerator<Term> actionArgs = action.Arguments.GetEnumerator();
            while (fsmActionArgs.MoveNext())
            {
                if (!actionArgs.MoveNext())
                    return false;   // action is shorter than fsmAction

                if (!actionArgs.Current.Equals(Any.Value) &&
                    !fsmActionArgs.Current.Equals(Any.Value) &&
                    !actionArgs.Current.Equals(fsmActionArgs.Current))
                    return false;  // mismatch in arguments
            }
            return true;
        }

        /// <summary>
        /// Returns true if the action is enabled in the given state
        /// </summary>
        public override bool IsEnabled(IState state, CompoundTerm action)
        {
            FsmState fs = state as FsmState;
            if (fs == null)
                throw new ArgumentException("Invalid state");

            foreach (Term automatonState in fs.AutomatonStates)
            {
                Set<Transition> outgoing = this.automaton.OutgoingTransitions(automatonState);
                foreach (Transition t in outgoing)
                {
                    if (IsCompatibleTerm(action, t.Second))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets string descriptions of the enabling conditions
        /// </summary>
        /// <param name="state">The state in which the </param>
        /// <param name="action">The action whose enabling conditions will queried</param>
        /// <param name="returnFailures">If <c>true</c>, enabling conditions that fail in state 
        /// <paramref name="state"/> will be returned. If <c>false</c>, all enabling conditions
        /// that are satisfied will be returned.</param>
        /// <returns>An array of description strings for the enabling conditions of action <paramref name="action"/></returns>
        public override IEnumerable<string> GetEnablingConditionDescriptions(IState state, CompoundTerm action, bool returnFailures)
        {
            yield break;
        }

        /// <summary>
        /// Enumerate all the enabled actions with the given symbol in the given state
        /// </summary>
        public override IEnumerable<CompoundTerm> GetActions(IState state, Symbol actionSymbol)
        {
            FsmState fs = state as FsmState;
            if (fs == null)
                throw new ArgumentException("Invalid state");

            Set<CompoundTerm> result = Set<CompoundTerm>.EmptySet;

            foreach (Term automatonState in fs.AutomatonStates)
            {
                Set<Transition> outgoing = this.automaton.OutgoingTransitions(automatonState);
                foreach (Transition t in outgoing)
                {
                    CompoundTerm ct = t.Second as CompoundTerm;
                    if (ct == null) throw new InvalidOperationException("Internal error");
                    if (Object.Equals(ct.Symbol, actionSymbol))
                        result = result.Add(ct);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the names of meta-properties that may be collected
        /// during the calculation of the <see cref="GetTargetState"/>.
        /// </summary>
        /// <returns>The names of meta-properties to be collected
        /// during the calculation of the step.</returns>
        /// <seealso cref="GetTargetState"/>
        public override Set<string> GetTransitionPropertyNames()
        {
            // to do: implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Produces the target state that results from invoking <paramref name="action"/>
        /// in the context of <paramref name="startState"/>.
        /// </summary>
        /// <param name="startState">The state in which the action is invoked</param>
        /// <param name="action">The action to be invoked</param>
        /// <param name="transitionPropertyNames">The names of meta-properties to be collected
        /// during the calculation of the step.</param>
        /// <param name="transitionProperties">Output parameter that will contain a 
        /// map of property names to property values. Each property value multiset of
        /// terms. For example, the property value might be the value of a Boolean function
        /// that controls state filtering. Or, it might correspond to the "coverage" of the model that results from this
        /// step. In this case, the value might denote the line numbers or blocks of the 
        /// model program that were exercised in this step, or a projection of the state 
        /// space or a reference to section numbers of a requirements document to indicate
        /// that the functionality defined by that section was exercised.</param>
        /// <returns>The state that results from the invocation of <paramref name="action"/>
        /// in <paramref name="startState"/>.</returns>
        /// <seealso cref="GetTransitionPropertyNames"/>
        public override IState GetTargetState(IState startState, CompoundTerm action, Set<string> transitionPropertyNames,
                              out TransitionProperties transitionProperties)
        {
            transitionProperties = new TransitionProperties();
            FsmState fs = startState as FsmState;
            if (fs == null)
                throw new ArgumentException("Invalid state");

            Set<Term> targetAutomatonStates = Set<Term>.EmptySet;
            foreach (Term automatonState in fs.AutomatonStates)
            {
                Set<Transition> outgoing = this.automaton.OutgoingTransitions(automatonState);
                foreach (Transition t in outgoing)
                {
                    CompoundTerm ct = t.Second as CompoundTerm;
                    if (ct == null) throw new InvalidOperationException("Internal error, invalid transition. FSM transition action symbol is null in "+t.ToString());
                    if (IsCompatibleTerm(action, ct)) //(Object.Equals(ct, action))
                    {
                        targetAutomatonStates = targetAutomatonStates.Add(t.Third);
                    }
                }
            }

            if (targetAutomatonStates.Equals(Set<Term>.EmptySet))
                throw new ArgumentException("Action not enabled: " + action.ToString());

            return new FsmState(targetAutomatonStates);
        }

        /// <summary>
        /// Returns true if the given state is an accepting state
        /// </summary>
        public override bool IsAccepting(IState state)
        {
            FsmState fs = state as FsmState;
            if (fs == null)
                throw new ArgumentException("Invalid state");

            return this.automaton.AcceptingStates.Exists(delegate(Term s) { return fs.AutomatonStates.Contains(s); });
        }

        /// <summary>
        /// Boolean value indicating whether all state invariant predicates
        /// defined by this model program are satisfied by <paramref name="state"/>. In general,
        /// failure to satisfy the state invariants indicates a modeling error.
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>Returs true</returns>
        public override bool SatisfiesStateInvariant(IState state)
        {
            return true;
        }

        /// <summary>
        /// Boolean value indicating whether all state filter predicates
        /// defined by this model program are satisfied by <paramref name="state"/>. 
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>Returns true</returns>
        public override bool SatisfiesStateFilter(IState state)
        {
            return true;
        }

        /// <summary>
        /// Checks whether a sort is an abstract type. The corresponding set is created once in the constructor.
        /// </summary>
        /// <param name="s">A symbol denoting a sort (i.e. abstract type)</param>
        /// <returns>Returns always false for <see cref="FsmModelProgram"/></returns>
        public override bool IsSortAbstract(Symbol s)
        {
            return false;
        }

        #endregion

        #region IName Members

        /// <summary>
        /// Returns the name of this model program.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        #endregion
    }
}
