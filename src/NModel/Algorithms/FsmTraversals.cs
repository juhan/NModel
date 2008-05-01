//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Terms;

namespace NModel.Algorithms
{
    /// <summary>
    /// Provides utilities for fsm traversal and analysis
    /// </summary>
    public class FsmTraversals
    {
        /// <summary>
        /// It is not possible to create an instance of this class
        /// </summary>
        private FsmTraversals() { }

        /// <summary>
        /// Generate a test suite from the given finite automaton.
        /// Dead states and transitions involving dead states are eliminated first,
        /// where a dead state is a state from which an accepting state is not reachable.
        /// The test suite will have the property that each action sequence in it 
        /// will lead to an accepting state of the finite automaton.
        /// The test suite provides transition coverage of all the alive transitions of the fa.
        /// The Rural Chinese Postman algorithm is used to compute the sequences.
        /// </summary>
        /// <returns>The resulting test suite</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Sequence<Sequence<CompoundTerm>> GenerateTestSequences(FSM fa)
        {
            //Eliminate dead states from the fa
            Set<Term> deadStates = GetDeadStates(fa);
            Set<Term> aliveStates = fa.States.Difference(deadStates);
            Set<Triple<Term, CompoundTerm, Term>> aliveTransitions =
                fa.Transitions.Select(delegate(Triple<Term, CompoundTerm, Term> trans)
                {
                    return aliveStates.Contains(trans.First) &&
                           aliveStates.Contains(trans.Third);
                });
            if (aliveTransitions.IsEmpty) //test suite cannot be generated
                return Sequence<Sequence<CompoundTerm>>.EmptySequence;

            //Build a graph from the alive transitions
            Term[] states = new Term[aliveStates.Count];
            Dictionary<Term, int> stateToVertexMap = new Dictionary<Term, int>();
            states[0] = fa.InitialState;
            stateToVertexMap[fa.InitialState] = 0;
            int i = 1;
            foreach (Term state in aliveStates.Remove(fa.InitialState))
            {
                states[i] = state;
                stateToVertexMap[state] = i++;
            }

            //create edges that must be traversed
            GraphTraversals.Edge[] mustEdges = new GraphTraversals.Edge[aliveTransitions.Count];
            Triple<Term, CompoundTerm, Term>[] mustTransitions = new Triple<Term, CompoundTerm, Term>[aliveTransitions.Count];
            i = 0;
            foreach (Triple<Term, CompoundTerm, Term> trans in aliveTransitions)
            {
                GraphTraversals.Edge edge = new GraphTraversals.Edge(stateToVertexMap[trans.First],
                    stateToVertexMap[trans.Third], i);
                mustEdges[i] = edge;
                mustTransitions[i++] = trans;
            }

            //add an optional edge with label -1 from every accepting state 
            //to the initial state, this corresponds to a reset action
            GraphTraversals.Edge[] optionalEdges =
                new GraphTraversals.Edge[fa.AcceptingStates.Count];
            i = 0;
            foreach (Term accState in fa.AcceptingStates)
            {
                int accVertex = stateToVertexMap[accState];
                optionalEdges[i++] = new GraphTraversals.Edge(accVertex, 0, -1); //-1 = reset
            }

            //at this point it is known that g is strongly connected and has no dead states
            //so a postman tour exists, compute a postman tour
            GraphTraversals.Graph g =
                new GraphTraversals.Graph(0, mustEdges, optionalEdges,
                                          GraphTraversals.WeakClosureEnum.DoNotClose);
            List<GraphTraversals.Edge> postmanTour =
                new List<GraphTraversals.Edge>(g.GetRuralChinesePostmanTour());

            #region normalize the tour so that it ends in an accepting state and has a reset at the end as a "watchdog"
            //if the last edge has not label -1, i.e. the edge leading back 
            //to the initial state is not a reset edge from an accepting state
            //and the last state is not an accepting state
            //then extend the path to an accepting state, from the beginning of the path
            //notice that there must be at least one accepting state, so such an extesion 
            //is indeed possible
            GraphTraversals.Edge lastEdge = postmanTour[postmanTour.Count - 1];
            if (lastEdge.label >= 0) //if the last edge is a reset, we are done
            {
                //the last edge leads back to the initial state, because this is a tour
                if (fa.AcceptingStates.Contains(fa.InitialState))
                    postmanTour.Add(new GraphTraversals.Edge(0, 0, -1)); //add a watchdog
                else
                {
                    //create an extesion to the tour from the initial state to the first accepting state
                    List<GraphTraversals.Edge> extension = new List<GraphTraversals.Edge>();
                    foreach (GraphTraversals.Edge edge in postmanTour)
                    {
                        extension.Add(edge);
                        if (fa.AcceptingStates.Contains(states[edge.target]))
                        {
                            //the end state of the edge is accepting, so we are done
                            extension.Add(new GraphTraversals.Edge(0, 0, -1)); //add a watchdog
                            break;
                        }

                    }
                    postmanTour.AddRange(extension);
                }
            }
            #endregion

            #region break up the tour into sequences of transition ids separated by the reset edge
            List<List<int>> paths = new List<List<int>>();
            List<int> path = new List<int>();
            for (int k = 0; k < postmanTour.Count; k++)
            {
                //presense of the watchdog at the end of the tour is assumed here
                GraphTraversals.Edge edge = postmanTour[k];
                if (edge.label < 0) //encountered reset, end of path
                {
                    paths.Add(path);
                    path = new List<int>();
                }
                else
                {
                    path.Add(edge.label);
                }
            }
            #endregion

            #region map the paths into action sequences
            Sequence<Sequence<CompoundTerm>> res = Sequence<Sequence<CompoundTerm>>.EmptySequence;
            foreach (List<int> path1 in paths)
            {
                Sequence<CompoundTerm> transSeq = Sequence<CompoundTerm>.EmptySequence;
                foreach (int transId in path1)
                    transSeq = transSeq.AddLast(mustTransitions[transId].Second);
                res = res.AddLast(transSeq);
            }
            #endregion

            return res;
        }

        /// <summary>
        /// Generates a finite automaton that encodes the given test sequences.
        /// The provided action symbols must be a superset of the action symbols that 
        /// appear in the sequences.
        /// The additional action symbol given by testcaseName is 
        /// added to the set of action symbols of the generated automaton.
        /// The accepting states of the automaton are the end states of all the test sequences.
        /// </summary>
        /// <param name="testcaseName">the name of a test sequence is used as the first action name parameterized with an integer</param>
        /// <param name="testseqs">given test sequences as sequences of actions</param>
        /// <param name="actionSymbols">action symbols in the vocabulary</param>
        /// <returns>a finite automaton encoding of the test sequences</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static FSM GenerateTestSequenceAutomaton(string testcaseName, Sequence<Sequence<CompoundTerm>> testseqs, Set<Symbol> actionSymbols)
        {
            Set<Term> acceptingStates = Set<Term>.EmptySet;
            Set<Term> states = Set<Term>.EmptySet;

            Symbol testCaseActionSymbol = new Symbol(testcaseName);
            Literal initialState = new Literal(0);
            states = states.Add(initialState);

            #region generate transitions and accepting states
            Set<Triple<Term, CompoundTerm, Term>> transitions =
                Set<Triple<Term, CompoundTerm, Term>>.EmptySet;
            for (int i = 0; i < testseqs.Count; i++)
            {
                //the i'th test sequence start action
                CompoundTerm startTestAction = 
                    new CompoundTerm(testCaseActionSymbol,
                                     new Sequence<Term>(new Literal(i)));

                transitions = transitions.Add(new Triple<Term, CompoundTerm, Term>(
                    initialState,startTestAction, IntermediateState.State(i,0)));

                Sequence<CompoundTerm> testseq = testseqs[i];

                //the final step state of the i'th test sequence is an accepting state
                acceptingStates = acceptingStates.Add(IntermediateState.State(i, testseq.Count));
                states = states.Add(IntermediateState.State(i, testseq.Count));

                for (int j = 0; j < testseq.Count; j++)
                {
                    if (!actionSymbols.Contains(testseq[j].Symbol))
                        throw new ArgumentException("Not all action symbols in test sequences appear in actionSymbols", "actionSymbols");
                    states = states.Add(IntermediateState.State(i, j));
                    transitions = transitions.Add(new Triple<Term, CompoundTerm, Term>(
                        IntermediateState.State(i, j), testseq[j], IntermediateState.State(i, j + 1)));
                }

            }
            #endregion

            return new FSM(initialState, states, transitions,
                acceptingStates, actionSymbols.Add(testCaseActionSymbol));
        }


        /// <summary>
        /// Get all states in the given finite automaton from which no accepting state is reachable.
        /// </summary>
        /// <param name="fa">given finite automaton</param>
        /// <returns>all dead states in the finite automaton</returns>
        public static Set<Term> GetDeadStates(FSM fa)
        {
            //build a graph from fa
            Dictionary<Term, int> stateToVertexMap = new Dictionary<Term, int>();
            stateToVertexMap[fa.InitialState] = 0;
            int i = 1;
            foreach (Term state in fa.States.Remove(fa.InitialState))
                stateToVertexMap[state] = i++;

            //create edges that correspond to the transitions
            GraphTraversals.Edge[] edges = new GraphTraversals.Edge[fa.Transitions.Count];
            Triple<Term, CompoundTerm, Term>[] transitions = new Triple<Term, CompoundTerm, Term>[fa.Transitions.Count];
            i = 0;
            foreach (Triple<Term, CompoundTerm, Term> trans in fa.Transitions)
            {
                edges[i] = new GraphTraversals.Edge(stateToVertexMap[trans.First],
                    stateToVertexMap[trans.Third], i);
                transitions[i++] = trans;
            }

            GraphTraversals.Graph g =
                new GraphTraversals.Graph(0, edges, new GraphTraversals.Edge[] { },
                                          GraphTraversals.WeakClosureEnum.DoNotClose);

            int[] acceptingVertices = new int[fa.AcceptingStates.Count];
            i = 0;
            foreach (Term accState in fa.AcceptingStates)
                acceptingVertices[i++] = stateToVertexMap[accState];

            GraphTraversals.HSet deadVertices = g.DeadStates(acceptingVertices);

            return fa.States.Select(delegate(Term state)
                 { return deadVertices.Contains(stateToVertexMap[state]); });
        }

        /// <summary>
        /// Represents the intermediate state of a test sequence
        /// </summary>
        internal class IntermediateState : CompoundValue
        {
            private IntermediateState() { }

            static Symbol s = Symbol.Parse("S");
            internal static Term State(int testCaseNr, int stepNr)
            {
                return new CompoundTerm(s, new Sequence<Term>(new Literal(testCaseNr), new Literal(stepNr)));
            }
        }

    }

}
