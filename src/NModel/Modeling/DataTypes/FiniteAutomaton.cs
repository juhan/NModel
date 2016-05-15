//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Terms;
using Transition = NModel.Triple<NModel.Terms.Term, NModel.Terms.CompoundTerm, NModel.Terms.Term>;
using System.Diagnostics.CodeAnalysis;

namespace NModel
{
    /// <summary>
    /// Data type that denotes a (possibly nondeterministic) finite automaton.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased")]
    public class FSM //: CompoundValue 
    { 
        readonly Term initialState;
        readonly Set<Term> states;
        readonly Set<Triple<Term, CompoundTerm, Term>> transitions;
        readonly Set<Term> acceptingStates;
        readonly Set<Symbol> actionSymbols;
        bool isDet;


        /// <summary>
        /// Create an FSM from a string representation of a compound term fsm of the form 
        /// FSM(initialState, acceptingStates, transitions [, vocabulary])
        /// where:  
        ///  - initialState is a term, 
        ///  - acceptingStates is a compound term of the form AcceptingStates(p1,...,pk) 
        ///  - transitions is a compound term of the form Transitions(t1,...,tl)
        ///    where each ti is compound term of the form t(q,a,q') where 
        ///    q and q' are states and a is a compound term 
        ///  - vocabulary is optional, if present it must be 
        ///    a compound term of the form Vocabulary(s1,...,sn) where 
        ///    each si is a string literal
        /// </summary>
        /// <param name="fsm">string representation of an FSM</param>
        /// <returns>FSM corresponding to fsm</returns>
        public static FSM FromString(string fsm) {
            return FromTerm(CompoundTerm.Parse(fsm));
        }


        /// <summary>
        /// Create an FSM from a compound term fsm of the form 
        /// FSM(initialState, acceptingStates, transitions [, vocabulary])
        /// where:  
        ///  - initialState is a term, 
        ///  - acceptingStates is a compound term of the form AcceptingStates(p1,...,pk) 
        ///  - transitions is a compound term of the form Transitions(t1,...,tl)
        ///    where each ti is compound term of the form t(q,a,q') where 
        ///    q and q' are states and a is a compound term 
        ///  - vocabulary is optional, if present it must be 
        ///    a compound term of the form Vocabulary(s1,...,sn) where 
        ///    each si is a string literal
        /// </summary>
        /// <param name="fsm">term representation of an FSM</param>
        /// <returns>FSM corresponding to fsm</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static FSM FromTerm(CompoundTerm fsm)
        {
            if (fsm.Arguments.Count < 3 || !(fsm.Arguments[1] is CompoundTerm) || 
                !(fsm.Arguments[2] is CompoundTerm) || 
                ((CompoundTerm)fsm.Arguments[2]).Arguments.Exists(delegate(Term t) 
                   {return !(t is CompoundTerm) || ((CompoundTerm)t).Arguments.Count != 3 || !(((CompoundTerm)t).Arguments[1] is CompoundTerm);}))
                throw new ArgumentException("Cannot create an FSM from given compound term, missing or invalid arguments");
            if (fsm.Arguments.Count > 3 && (!(fsm.Arguments[3] is CompoundTerm) || 
                ((CompoundTerm)fsm.Arguments[3]).Arguments.Exists(delegate(Term t) 
                   {return !(t is Literal) || !(((Literal)t).Value is String);})))
                throw new ArgumentException("Cannot create an FSM from given compound term, invalid vocabulary term");
            Term initialState = fsm.Arguments[0];
            Set<Term> acceptingStates = new Set<Term>(fsm.Arguments[1].Arguments);
            Set<Term> transTerms = new Set<Term>(fsm.Arguments[2].Arguments);
            Set<Transition> transitions =
                transTerms.Convert<Transition>(delegate(Term t){
                         return new Transition(t.Arguments[0], (CompoundTerm)t.Arguments[1], t.Arguments[2]);});
            Set<Term> states = acceptingStates.Add(initialState);
            Sequence<String> vocabularyNames = (fsm.Arguments.Count < 4 ? Sequence<String>.EmptySequence : 
                fsm.Arguments[3].Arguments.Convert<String>(delegate (Term t) {return (String)((Literal)t).Value;}));
            Set<Symbol> vocabulary = new Set<String>(vocabularyNames).Convert<Symbol>(Symbol.Parse);
            //extend the vocabulary and add states
            foreach (Transition t in transitions)
            {
                states = states.Add(t.First).Add(t.Third);
                vocabulary = vocabulary.Add(t.Second.Symbol);
            }
            return new FSM(initialState, states, transitions, acceptingStates, vocabulary);
        }

        /// <summary>
        /// Create a term representation of this FSM of the form
        /// FSM(initialState, acceptingStates, transitions, vocabulary)
        /// where:  
        ///  - initialState is a term, 
        ///  - acceptingStates is a compound term of the form AcceptingStates(p1,...,pk) 
        ///  - transitions is a compound term of the form Transitions(t1,...,tl)
        ///    where each ti is compound term of the form t(q,a,q') where 
        ///    q and q' are states and a is a compound term 
        ///  - vocabulary is a compound term of the form Vocabulary(s1,...,sn) where 
        ///    each si is a string literal
        /// </summary>
        /// <returns>the term representation of this FSM</returns>
        public CompoundTerm ToTerm()
        {
            CompoundTerm accstates = new CompoundTerm(Symbol.Parse("AcceptingStates"), new Sequence<Term>(this.acceptingStates));
            CompoundTerm vocab = new CompoundTerm(Symbol.Parse("Vocabulary"),
                new Sequence<Term>(this.actionSymbols.Convert<Term>(delegate(Symbol s) { return new Literal(s.Name); })));
            Sequence<Term> trans = Sequence<Term>.EmptySequence;
            foreach (Transition t in this.transitions)
                trans = trans.AddLast(new CompoundTerm(Symbol.Parse("t"), new Sequence<Term>(t.First, t.Second, t.Third)));
            CompoundTerm ts = new CompoundTerm(Symbol.Parse("Transitions"), trans);
            return new CompoundTerm(Symbol.Parse("FSM"), new Sequence<Term>(this.initialState,
                accstates, ts, vocab));
        }

        /// <summary>
        /// String representation of this FSM is ToTerm().ToString()
        /// </summary>
        /// <returns>ToTerm().ToString()</returns>
        public override string ToString()
        {
            return ToTerm().ToString();
        }

        /// <summary>
        /// The initial state
        /// </summary>
        public Term InitialState { get { return initialState; } }

        /// <summary>
        /// Returns true if the finite automaton is deterministic
        /// </summary>
        public bool IsDeterministic
        {
            get
            {

                return isDet;
            }
        }

        /// <summary>
        /// The set of all states of this automaton
        /// </summary>
        public Set<Term> States {get {return states;}}

        /// <summary>
        /// The set of all transitions of this automaton. A transition is a triple
        /// (<i>start-state</i>, <i>transition-label</i>, <i>end-state</i>).
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Set<Triple<Term, CompoundTerm, Term>> Transitions { get { return transitions; } }

        /// <summary>
        /// The set of accepting states of this automaton. A valid trace is a sequence of 
        /// transitions that ends in an accepting state. In other words, an accepting state
        /// is a valid termination state.
        /// </summary>
        public Set<Term> AcceptingStates { get { return acceptingStates; } }

        /// <summary>
        /// The set of symbols that are included in the vocabulary of this automaton.
        /// </summary>
        /// <remarks>The function symbols of the transitions of this automaton may a subset of the action symbols. 
        /// This occurs when some of the actions in the vocabulary are never enabled.</remarks>
        public Set<Symbol> Vocabulary { get { return actionSymbols; } }

        /// <summary>
        /// Constructs a finite automaton value.
        /// </summary>
        /// <param name="initialState">The initial state</param>
        /// <param name="states">The set of all states of this automaton</param>
        /// <param name="transitions">The set of all transitions of this automaton. A transition is a triple
        /// (<i>start-state</i>, <i>transition-label</i>, <i>end-state</i>).</param>
        /// <param name="acceptingStates">The set of accepting states of this automaton.</param>
        /// <note> All arguments must be nonnull. The states that appear in <paramref name="transitions"/> and 
        /// <paramref name="acceptingStates"/> must also be elements of <paramref name="states"/>. 
        /// </note>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FSM(Term initialState, Set<Term> states, Set<Triple<Term, CompoundTerm, Term>> transitions,
            Set<Term> acceptingStates)
        {
            if (initialState == null) throw new ArgumentNullException("initialState");
            if (states == null) throw new ArgumentNullException("states");
            if (transitions == null) throw new ArgumentNullException("transitions");
            if (acceptingStates == null) throw new ArgumentNullException("acceptingStates");

            foreach(Triple<Term, CompoundTerm, Term> transition in transitions)
                if (transition == null || transition.First == null || transition.Second == null || transition.Third == null) 
                    throw new ArgumentException("Transitions may not contain null values");           

            if (!states.Contains(initialState)) throw new ArgumentException("Initial state not in states");

            if (!states.IsSupersetOf(transitions.Convert<Term>(delegate(Triple<Term, CompoundTerm, Term> transition) { return transition.First; })))
                throw new ArgumentException("Start state not in states");

            if (!states.IsSupersetOf(transitions.Convert<Term>(delegate(Triple<Term, CompoundTerm, Term> transition) { return transition.Third; })))
                throw new ArgumentException("End state not in states");

            if (!states.IsSupersetOf(acceptingStates))
                throw new ArgumentException("Accepting state not in states");

            Set<Symbol> actionSymbols = transitions.Convert<Symbol>(delegate(Triple<Term, CompoundTerm, Term> transition) { return transition.Second.Symbol; });

            this.initialState = initialState;
            this.states = states;
            this.transitions = transitions;
            this.acceptingStates = acceptingStates;
            this.actionSymbols = actionSymbols;
            this.isDet = CheckIfDeterministic(transitions);
        }

        /// <summary>
        /// Constructs a finite automaton from given transitions.
        /// Each transition must be a string representation of a compound term with 
        /// three arguments, argument 0 is the source state, argument 1 is a compound term 
        /// representing an action, and argument 2 is the target state.
        /// The source state of the first transition is the initial state.
        /// The set of action symbols is derived from all the actions.
        /// All states are accepting.
        /// </summary>
        /// <param name="transitions">String representations of transitions</param>
        /// <returns>The resulting finite automaton</returns>
        public static FSM CreateFromTransitions(params string[] transitions)
        {
            if (transitions == null)
                throw new ArgumentNullException("transitions");
            if (transitions.Length == 0)
                throw new ArgumentException("Finite automaton cannot be constructed from an empty set of transitions", "transitions");
            Term initialState = null;
            Set<Term> states = Set<Term>.EmptySet;
            Set<Symbol> actionSymbols = Set<Symbol>.EmptySet;
            Set<Triple<Term, CompoundTerm, Term>> transitionSet =
                Set<Triple<Term, CompoundTerm, Term>>.EmptySet;
            try
            {
                for (int i = 0; i < transitions.Length; i++)
                {
                    string trans = transitions[i];
                    CompoundTerm t = (CompoundTerm)Term.Parse(trans);
                    Term fromState = t.Arguments[0];
                    CompoundTerm action = (CompoundTerm)t.Arguments[1];
                    Term toState = t.Arguments[2];
                    if (i == 0)
                        initialState = fromState;
                    transitionSet = transitionSet.Add(new Triple<Term, CompoundTerm, Term>(fromState, action, toState));
                    states = states.Add(fromState);
                    states = states.Add(toState);
                    actionSymbols = actionSymbols.Add(action.Symbol);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Finite automaton cannot be constructed", "transitions", e);
            }
            return new FSM(initialState, states, transitionSet, states, actionSymbols);
        }

        /// <summary>
        /// Constructs a finite automaton from given transitions.
        /// Each transition must be a string representation of a compound term with 
        /// three arguments, argument 0 is the source state, argument 1 is a compound term 
        /// representing an action, and argument 2 is the target state.
        /// The source state of the first transition is the initial state.
        /// The set of action symbols is derived from all the actions.
        /// There are no accepting states.
        /// </summary>
        /// <param name="transitions">String representations of transitions</param>
        /// <returns>The resulting finite automaton</returns>
        public static FSM Create(params string[] transitions)
        {
            if (transitions == null)
                throw new ArgumentNullException("transitions");
            if (transitions.Length == 0)
                throw new ArgumentException("Finite automaton cannot be constructed from an empty set of transitions", "transitions");
            Term initialState = null;
            Set<Term> states = Set<Term>.EmptySet;
            Set<Symbol> actionSymbols = Set<Symbol>.EmptySet;
            Set<Triple<Term, CompoundTerm, Term>> transitionSet =
                Set<Triple<Term, CompoundTerm, Term>>.EmptySet;
            try
            {
                for (int i = 0; i < transitions.Length; i++)
                {
                    string trans = transitions[i];
                    CompoundTerm t = (CompoundTerm)Term.Parse(trans);
                    Term fromState = t.Arguments[0];
                    CompoundTerm action = (CompoundTerm)t.Arguments[1];
                    Term toState = t.Arguments[2];
                    if (i == 0)
                        initialState = fromState;
                    transitionSet = transitionSet.Add(new Triple<Term, CompoundTerm, Term>(fromState, action, toState));
                    states = states.Add(fromState);
                    states = states.Add(toState);
                    actionSymbols = actionSymbols.Add(action.Symbol);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Finite automaton cannot be constructed", "transitions", e);
            }
            return new FSM(initialState, states, transitionSet, Set<Term>.EmptySet, actionSymbols);
        }

        /// <summary>
        /// Extend this FSM with new transitions.
        /// Each transition must be a string representation of a compound term with 
        /// three arguments, argument 0 is the source state, argument 1 is a compound term 
        /// representing an action, and argument 2 is the target state.
        /// </summary>
        /// <param name="trans">string representations of transitions</param>
        /// <returns>the fsm extended with the given transitions</returns>
        public FSM Extend(params string[] trans)
        {
            FSM fsm = FSM.Create(trans);
            return new FSM(this.initialState,
                this.states.Union(fsm.states),
                this.transitions.Union(fsm.Transitions),
                this.acceptingStates,
                this.actionSymbols.Union(fsm.actionSymbols));
        }

        /// <summary>
        /// Mark the given states as accepting states.
        /// The existing accepting states also remain as accepting states.
        /// All states must exist in this fsm.
        /// </summary>
        /// <param name="newAccStates">string representations of new accepting states</param>
        /// <returns>fsm where the given states are marked as accepting states</returns>
        public FSM Accept(params string[] newAccStates)
        {
            Set<Term> accStates = new Set<string>(newAccStates).Convert<Term>(Term.Parse);
            if (!accStates.Difference(this.states).IsEmpty)
                throw new ArgumentException("Unexpected states in newAccStates: " + accStates.Difference(this.states).ToString());
            return new FSM(this.initialState, this.states, this.transitions,
                this.acceptingStates.Union(accStates), this.actionSymbols);
        }


        /// <summary>
        /// Constructs a finite automaton value.
        /// </summary>
        /// <param name="initialState">The initial state</param>
        /// <param name="states">The set of all states of this automaton</param>
        /// <param name="transitions">The set of all transitions of this automaton. A transition is a triple
        /// (<i>start-state</i>, <i>transition-label</i>, <i>end-state</i>).</param>
        /// <param name="acceptingStates">The set of accepting states of this automaton.</param>
        /// <param name="actionSymbols">The set of symbols that are included in the vocabulary of this automaton.</param>
        /// <note> All arguments must be nonnull. The states that appear in <paramref name="transitions"/> and 
        /// <paramref name="acceptingStates"/> must also be elements of <paramref name="states"/>. The function
        /// symbol of each transition label must be an element of <paramref name="actionSymbols"/>.
        /// </note>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FSM(Term initialState, 
                               Set<Term> states, 
                               Set<Triple<Term, CompoundTerm, Term>> transitions,
                               Set<Term> acceptingStates, 
                               Set<Symbol> actionSymbols)
        {
            if (initialState == null) throw new ArgumentNullException("initialState");
            if (states == null) throw new ArgumentNullException("states");
            if (transitions == null) throw new ArgumentNullException("transitions");
            if (acceptingStates == null) throw new ArgumentNullException("acceptingStates");
            if (actionSymbols == null) throw new ArgumentNullException("actionSymbols");

            if (!states.Contains(initialState)) throw new ArgumentException("Initial state not in states");

            if (!states.IsSupersetOf(transitions.Convert<Term>(delegate(Triple<Term, CompoundTerm, Term> transition) { return transition.First; })))
                throw new ArgumentException("Start state not in states");

            if (!states.IsSupersetOf(transitions.Convert<Term>(delegate(Triple<Term, CompoundTerm, Term> transition) { return transition.Third; })))
                throw new ArgumentException("End state not in states");

            if (!states.IsSupersetOf(acceptingStates))
                throw new ArgumentException("Accepting state not in states");

            Set<Symbol> symbolsUsed = transitions.Convert<Symbol>(delegate(Triple<Term, CompoundTerm, Term> transition) { return transition.Second.Symbol; });
            if (!symbolsUsed.IsSubsetOf(actionSymbols))
                throw new ArgumentException("Symbol used in transition label was not found in actionSymbols.");

            this.initialState = initialState;
            this.states = states;
            this.transitions = transitions;
            this.acceptingStates = acceptingStates;
            this.actionSymbols = actionSymbols;
            this.isDet = CheckIfDeterministic(transitions);
        }

        static bool CheckIfDeterministic(Set<Triple<Term, CompoundTerm, Term>> transitions)
        {
            Dictionary<Pair<Term, CompoundTerm>, Term> trans = new Dictionary<Pair<Term, CompoundTerm>, Term>();
            foreach (Triple<Term, CompoundTerm, Term> t in transitions)
            {
                Pair<Term, CompoundTerm> from = new Pair<Term, CompoundTerm>(t.First, t.Second);
                if (trans.ContainsKey(from))
                    return false;
                else
                    trans.Add(from, t.Third);
            }
            return true;
        }

        /// <summary>
        /// The set of transitions that begin in <paramref name="startState"/>
        /// </summary>
        /// <param name="startState">A state of this automaton</param>
        /// <returns>The set of transitions that begin in <paramref name="startState"/></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Set<Triple<Term, CompoundTerm, Term>> OutgoingTransitions(Term startState)
        {
            return transitions.Select(delegate(Triple<Term, CompoundTerm, Term> transition) { return Object.Equals(startState, transition.First); });
        }

        /// <summary>
        /// Create a new finite automaton from the given one by expanding the 
        /// signature with the new symbols.
        /// </summary>
        /// <param name="symbs">the new symbols</param>
        /// <returns>the expanded finite automaton</returns>
        public FSM Expand(params string[] symbs)
        {
            Set<Symbol> symbols = new Set<string>(symbs).Convert<Symbol>(Symbol.Parse);
            return new FSM(this.initialState, this.states,
                this.transitions, this.acceptingStates, this.actionSymbols.Union(symbols));
        }
    }
}
