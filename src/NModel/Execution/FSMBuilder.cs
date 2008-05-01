//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Terms;

namespace NModel.Execution
{
    /// <summary>
    /// A delegate for desciding if a transition is to be included 
    /// in the gererated finite automaton or not.
    /// </summary>
    /// <param name="startState">start state of the transition</param>
    /// <param name="action">action label of the transition</param>
    /// <param name="endState">end state of the transition</param>
    /// <param name="evaluatedProperties">evaluated properties, the value of each property is a bag of terms</param>
    /// <returns>returns true if the transition is to be included in the finite automaton, returns false otherwise</returns>
    public delegate bool TransitionPredicate(IState startState, CompoundTerm action, IState endState, Map<string,Bag<Term>> evaluatedProperties);


    /// <summary>
    /// Utility for exploring model programs. 
    /// </summary>
    public sealed class FSMBuilder
    {
        ModelProgram modelProgram;
        TransitionPredicate transitionPredicate;


        bool IncludeTransition(IState startState, CompoundTerm action, IState endState, Map<string, Bag<Term>> props)
        {
            //check first that the endstate satisfies the filters
            if (!modelProgram.SatisfiesStateFilter(endState))
                return false;

            //check then that the additional predicate holds
            if (transitionPredicate == null)
                return true;
            else
                return transitionPredicate(startState, action, endState, props);
        }

        /// <summary>
        /// Builder that can create a finite automaton by exploring the possible steps of a model program.
        /// </summary>
        /// <param name="m">Model program to be explored</param>
        public FSMBuilder(ModelProgram m) : this(m,null)
        {
        }

        /// <summary>
        /// Builder that can create a finite automaton by exploring the possible steps of a model program.
        /// </summary>
        /// <param name="m">Model program to be explored</param>
        /// <param name="transitionPredicate">User defined predicate that returns true if a 
        /// transition is to be included in the gererated finite automaton</param>
        public FSMBuilder(ModelProgram m, TransitionPredicate transitionPredicate)
        {
            this.modelProgram = m;
            this.transitionPredicate = transitionPredicate;
        }


        class Transition
        {
            public IState startState;
            public CompoundTerm action;
            public IState targetState;

            public Transition(IState startState, CompoundTerm action, IState targetState)
            {
                this.startState = startState;
                this.action = action;
                this.targetState = targetState;
            }
        }

         /// <summary>
        /// Explores the model associated with this instance.
        /// The dictionary stateMap (if not null) is used to record the mapping 
        /// from generated finite automata states to IStates.
        /// Explore(null) is the same as Explore()
        /// </summary>
        /// <returns>A list of transitions. Each transition is a start state,
        /// an action label and an end state.</returns>
        public FSM Explore(Dictionary<Term,IState> generatedStateMap)
        {
            Set<Symbol> actionSymbols = modelProgram.ActionSymbols();
            IState initialState = modelProgram.InitialState;

            Dictionary<IState, int> states = new Dictionary<IState, int>();
            LinkedList<IState> frontier = new LinkedList<IState>();
            frontier.AddFirst(initialState);
            int nextStateId = 0;

            Dictionary<Transition, Transition> transitions = new Dictionary<Transition, Transition>();

            while (frontier.Count > 0)
            {
                IState startState = frontier.First.Value;
                frontier.RemoveFirst();
                if (!states.ContainsKey(startState)) states.Add(startState, nextStateId++);

                foreach (Symbol actionSymbol in actionSymbols)
                {
                    if (modelProgram.IsPotentiallyEnabled(startState, actionSymbol))
                    {
                        IEnumerable<CompoundTerm> actions = modelProgram.GetActions(startState, actionSymbol);
                        foreach (CompoundTerm action in actions)
                        {
                            TransitionProperties transitionProperties;
                            IState targetState = modelProgram.GetTargetState(startState, action, Set<string>.EmptySet, out transitionProperties);
                            if (IncludeTransition(startState, action, targetState, transitionProperties.Properties))
                            {
                                Transition t = new Transition(startState, action, targetState);
                                transitions.Add(t, t);
                                if (!states.ContainsKey(targetState) && !frontier.Contains(targetState))
                                    frontier.AddFirst(targetState);
                            }
                        }
                    }
                    
                }
            }
            Term automatonInitialState = new Literal(states[initialState]);

            Set<Term> automatonStates = Set<Term>.EmptySet;
            Set<Term> acceptingStates = Set<Term>.EmptySet;
            foreach (KeyValuePair<IState, int> kv in states)
            {
                Term automatonState = new Literal(kv.Value);
                automatonStates = automatonStates.Add(automatonState);
                if (modelProgram.IsAccepting(kv.Key))
                    acceptingStates = acceptingStates.Add(automatonState);
                if (generatedStateMap != null)
                    generatedStateMap[automatonState] = kv.Key;
            }

            Set<Triple<Term, CompoundTerm, Term>> automatonTransitions = Set<Triple<Term, CompoundTerm, Term>>.EmptySet;
            foreach (KeyValuePair<Transition, Transition> kv in transitions)
            {
                Transition t = kv.Key;
                automatonTransitions = automatonTransitions.Add(new Triple<Term, CompoundTerm, Term>(new Literal(states[t.startState]),
                                                                                             t.action,
                                                                                             new Literal(states[t.targetState])));
            }
            return new FSM(automatonInitialState, automatonStates, automatonTransitions, acceptingStates);
        }

        /// <summary>
        /// Explores the model associated with this instance.
        /// </summary>
        /// <returns>A list of transitions. Each transition is a start state,
        /// an action label and an end state.</returns>
        public FSM Explore()
        {
            return Explore(null);
        }

        /// <summary>
        /// Indicates whether to take the left or the right branch in a binary tree construction (such as a pair state)
        /// </summary>
        public enum Branch
        {
            /// <summary>
            /// Left branch
            /// </summary>
            Left, 
            /// <summary>
            /// Right branch
            /// </summary>
            Right
        }
        /// <summary>
        /// Returns a finite automaton that is the projection of an automaton with product states.
        /// </summary>
        /// <param name="finiteAutomaton">The product automaton</param>
        /// <param name="projectedSymbols">Project to these action symbols</param>
        /// <param name="treePosition">A sequence of enum values indicating the position in the product tree of the state to project. 
        /// For example, Sequence&lt;Branch.Left, Branch.Right&gt; indicates the right leaf of the left branch from the root.</param>
        /// <param name="stateMap">Dictionary of state terms (of <paramref name="finiteAutomaton"/>) that contain the associated IState values.
        /// May be null if the out parameter <paramref name="reductStateMap"/> is not required to be created.</param>
        /// <param name="reductStateMap">Output that is produced by projecting the states of the <paramref name="stateMap"/>, if nonnull.</param>
        /// <returns>The projected automaton</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#")]
        public static FSM ProjectFromProduct(FSM finiteAutomaton, Set<Symbol> projectedSymbols, Sequence<Branch> treePosition, Dictionary<Term, IState>/*?*/ stateMap, out Dictionary<Term, IState>/*?*/ reductStateMap)
        {

            Set<IState> projectedIStates = finiteAutomaton.States.Convert<IState>(delegate(Term t) { return GetReduct(treePosition, stateMap[t]); });
            Set<IState> projectedAcceptingIStates = finiteAutomaton.AcceptingStates.Convert<IState>(delegate(Term t) { return GetReduct(treePosition, stateMap[t]); });

            reductStateMap = new Dictionary<Term, IState>();
            Dictionary<IState, Term>  reverseMap = new Dictionary<IState, Term>();
          

            Term initialState = new Literal(0);
            IState initialStateReduct = GetReduct(treePosition, stateMap[finiteAutomaton.InitialState]);
            reductStateMap[initialState] = initialStateReduct;
            reverseMap[initialStateReduct] = initialState;

            int i = 1;
            foreach (IState s in projectedIStates.Remove(initialStateReduct))
            {
                Term t = new Literal(i++);
                reductStateMap[t] = s;
                reverseMap[s] = t;
            }
            Set<Term> states = new Set<Term>(reductStateMap.Keys);
            Set<Term> acceptingStates = projectedAcceptingIStates.Convert<Term>(delegate(IState x){return reverseMap[x];});

            Set<Triple<Term, CompoundTerm, Term>> transitions =
                finiteAutomaton.Transitions.Convert<Triple<Term, CompoundTerm, Term>>(
                delegate(Triple<Term, CompoundTerm, Term> t)
                {
                    return new Triple<Term, CompoundTerm, Term>(reverseMap[GetReduct(treePosition, stateMap[t.First])],
                                                          t.Second,
                                                          reverseMap[GetReduct(treePosition, stateMap[t.Third])]);
                });
            Set<Triple<Term, CompoundTerm, Term>> transitions1 =
                transitions.Select(delegate(Triple<Term, CompoundTerm, Term> t)
                    { return projectedSymbols.Contains(t.Second.Symbol); });
            return new FSM(initialState, states, transitions1, acceptingStates, projectedSymbols);
        }

        //requires that the tree position is valid for the given IState
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        static IState GetReduct(Sequence<Branch> treePosition, IState state)
        {
            if (treePosition.IsEmpty) return state;
            else if (treePosition.Head == Branch.Left) return GetReduct(treePosition.Tail, ((IPairState)state).First);
            else return GetReduct(treePosition.Tail, ((IPairState)state).Second);
        }

    }
}

