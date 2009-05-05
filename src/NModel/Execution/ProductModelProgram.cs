//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Algorithms;
using NModel.Terms;

namespace NModel.Execution
{
    /// <summary>
    /// Represents a model program that is a product composition of other model programs
    /// </summary>
    public class ProductModelProgram : ModelProgram
    {
        ModelProgram m1;
        ModelProgram m2;   
        ComposedSignature signature;           // cache: equals m1.ActionSymbols union m2.ActionSymbols

        /// <summary>
        /// Constructs m1 * m2
        /// </summary>
        public ProductModelProgram(ModelProgram m1, ModelProgram m2)
        {            
            this.signature = new ComposedSignature(m1, m2);
            this.m1 = m1;
            this.m2 = m2;
        }

        /// <summary>
        /// The first operand of the composed model program M1 * M2
        /// </summary>
        public ModelProgram M1 { get { return this.m1; } }

        /// <summary>
        /// The second operand of the composed model program M1 * M2
        /// </summary>
        public ModelProgram M2 { get { return this.m2; } }

        #region Helper Methods

        static IState M1Reduct(PairState productState)
        {
            return productState.First;
        }

        static IState M2Reduct(PairState productState)
        {
            return productState.Second;
        } 
        #endregion

        #region ModelProgram Members

        #region Action signature

        /// <summary>
        /// Nonempty set of actions symbols.
        /// <br><c>ensures result.Count &gt; 0</c></br>
        /// </summary>
        public override Set<Symbol> ActionSymbols()
        {
             return this.signature.actionSymbols;
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
            if (this.signature.IsShared(actionSymbol))
                return Math.Max(m1.ActionArity(actionSymbol), m2.ActionArity(actionSymbol));

            else if (this.signature.IsOnlyInM1(actionSymbol))
                return m1.ActionArity(actionSymbol);

            else if (this.signature.IsOnlyInM2(actionSymbol))
                return m2.ActionArity(actionSymbol);
            else
                throw new ArgumentException("Invalid argument-- action symbol " + actionSymbol.ToString() + " not in signature.");
        }

        /// <summary>
        /// Gets the sort (i.e., abstract type) of the ith parameter of action <paramref name="actionSymbol"/>
        /// <br><c>requires actionSymbol != null;</c></br>
        /// <br><c>requires this.ActionSymbols.Contains(actionSymbol);</c></br>
        /// <br><c>requires 0 &lt;= parameterIndex &amp;&amp; parameterIndex &lt; this.ActionArity(actionSymbol);</c></br>
        /// </summary>
        /// <param name="actionSymbol">A symbol naming an action of this model program.</param>
        /// <param name="parameterIndex">An integer in the interval [0, this.ActionArity(actionSymbol))</param>
        /// <returns>The sort (abstract type) of the ith parameter of action <paramref name="actionSymbol"/></returns>
        public override Symbol ActionParameterSort(Symbol actionSymbol, int parameterIndex)
        {
            if (this.signature.IsShared(actionSymbol) || this.signature.IsOnlyInM1(actionSymbol))
                return m1.ActionParameterSort(actionSymbol, parameterIndex);
            else if (this.signature.IsOnlyInM2(actionSymbol))
                return m2.ActionParameterSort(actionSymbol, parameterIndex);
            else
                throw new ArgumentException("Invalid argument-- action symbol " + actionSymbol.ToString() + " not in signature.");       
        }

        ///// <summary>
        ///// Is <paramref name="actionSymbol"/> controllable or observable?
        ///// <br><c>requires actionSymbol != null;</c></br>
        ///// <br><c>requires this.ActionSymbols.Contains(actionSymbol);</c></br> 
        ///// </summary>
        ///// <param name="actionSymbol">A symbol naming an action of this model program</param>
        ///// <returns>"Controlled" or "Observed"</returns>
        //public ActionRole GetActionRole(Symbol actionSymbol)
        //{
        //    if (this.signature.IsShared(actionSymbol) || this.signature.IsOnlyInM1(actionSymbol))
        //        return m1.GetActionRole(actionSymbol);
        //    else if (this.signature.IsOnlyInM2(actionSymbol))
        //        return m2.GetActionRole(actionSymbol);
        //    else
        //        throw new ArgumentException("Invalid argument-- action symbol " + actionSymbol.ToString() + " not in signature.");
        //}

        #endregion

        #region State signature
        /// <summary>
        /// Number of location values in the state signature of this model program
        /// <br><c>ensures result >= 0;</c></br>
        /// </summary>
        public override int LocationValueCount
        {
            get
            {
                return m1.LocationValueCount + m2.LocationValueCount;
            }
        }

        /// <summary>
        /// String name identifying the ith location value in the state signature of this model program
        /// <br><c>requires 0 &lt;= i &amp;&amp; i &lt; this.LocationValueCount;</c></br>
        /// </summary>
        /// <param name="i">An index in the interval [0,  LocationValueClount)</param>
        /// <returns>String name identifying the ith location value</returns>
        public override string LocationValueName(int i)
        {
            if (i >= 0 && i < m1.LocationValueCount)
                return m1.LocationValueName(i);
            else if (i >= 0 && i < m1.LocationValueCount + m2.LocationValueCount)
                return m2.LocationValueName(i - m1.LocationValueCount);
            else
                throw new ArgumentOutOfRangeException("i");
        }

        /// <summary>
        /// Model program that provides the ith location value of the state signature of this model program.
        /// The result will be this model program, except under composition, when the result will the be 
        /// a leaf of the composition tree.
        /// <br><c>requires 0 &lt;= i &amp;&amp; i &lt; this.LocationValueCount;</c></br>
        /// </summary>
        /// <param name="i">An index in the interval [0,  LocationValueClount)</param>
        /// <returns>Model program that provides the ith location value.</returns>
        public override string LocationValueModelName(int i)
        {
            if (i >= 0 && i < m1.LocationValueCount)
                return m1.LocationValueModelName(i);
            else if (i >= 0 && i < m1.LocationValueCount + m2.LocationValueCount)
                return m2.LocationValueModelName(i - m1.LocationValueCount);
            else
                throw new ArgumentOutOfRangeException("i");
        }

        /// <summary>
        /// Symbol denoting the sort (abstract type) of the ith location value in the state signature 
        /// of this model program
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override Symbol LocationValueSort(int i)
        {
            if (i >= 0 && i < m1.LocationValueCount)
                return m1.LocationValueSort(i);
            else if (i >= 0 && i < m1.LocationValueCount + m2.LocationValueCount)
                return m2.LocationValueSort(i - m1.LocationValueCount);
            else
                throw new ArgumentOutOfRangeException("i");
        }
        #endregion

        #region Initial state
        /// <summary>
        /// Initial state
        /// </summary>
        public override IState InitialState
        {
            get { return PairState.CreateState(m1.InitialState, m2.InitialState); }
        }
        #endregion

        #region Enabledness of actions and parameter generation
        /// <summary>
        /// Checks whether a given action is potentially enabled 
        /// in this state.
        /// <br><c>requires state != null;</c></br>
        /// <br>requires state.ModelProgram == this; </br>
        /// <br>requires actionSymbol != null;</br>
        /// </summary>
        public override bool IsPotentiallyEnabled(IState state, Symbol actionSymbol)
        {
            PairState ps = state as PairState;
            if (ps == null) throw new ArgumentException("Unexpected type-- expected PairState");

            if (!this.signature.Contains(actionSymbol))
                return false;
            
            IState m1State = M1Reduct(ps);
            IState m2State = M2Reduct(ps);
            if (this.signature.IsShared(actionSymbol))
                return
                    m1.IsPotentiallyEnabled(m1State, actionSymbol) &&
                    m2.IsPotentiallyEnabled(m2State, actionSymbol);
            else if (this.signature.IsOnlyInM1(actionSymbol))
                return m1.IsPotentiallyEnabled(m1State, actionSymbol);
            else
                return m2.IsPotentiallyEnabled(m2State, actionSymbol);            
        }

        /// <summary>
        /// Enumerates the action symbols that are potentially enabled with respect to this
        /// control point and data state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public override Set<Symbol> PotentiallyEnabledActionSymbols(IState state)
        {
            PairState ps = state as PairState;
            if (ps == null)
                throw new ArgumentException("Unexpected type-- expected PairState");

            Set<Symbol> res = Set<Symbol>.EmptySet;
            foreach (Symbol actionSymbol in this.signature.actionSymbols)
                if (this.IsPotentiallyEnabled(ps, actionSymbol))
                    res = res.Add(actionSymbol);    
            return res;
        }

        static Set<Term> AnyDomain = new Set<Term>(Any.Value);

        /// <summary>
        /// Does this model program have an interface to parameter generation for this parameter?
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actionSymbol"></param>
        /// <param name="parameterIndex"></param>
        /// <returns>A set of terms representing the possible values</returns>
        public override bool HasActionParameterDomain(IState state, Symbol actionSymbol, int parameterIndex)
        {
            PairState ps = state as PairState;
            if (ps == null)
                throw new ArgumentException("Unexpected type-- expected PairState");

            if (!this.signature.Contains(actionSymbol))
                throw new ArgumentException("Unexpected action symbol-- must be in signature");

            bool m1HasDomain = (this.signature.IsShared(actionSymbol) || this.signature.IsOnlyInM1(actionSymbol));
            bool m2HasDomain = (this.signature.IsShared(actionSymbol) || this.signature.IsOnlyInM2(actionSymbol));

            return (m1HasDomain ? m1.HasActionParameterDomain(M1Reduct(ps), actionSymbol, parameterIndex) : false)||
                   (m2HasDomain ? m2.HasActionParameterDomain(M2Reduct(ps), actionSymbol, parameterIndex) : false);
        }

        /// <summary>
        /// Get the value domain of the given action parameter in the given state
        /// </summary>
        public override Set<Term> ActionParameterDomain(IState state, Symbol actionSymbol, int parameterIndex)
        {
            PairState ps = state as PairState;
            if (ps == null)
                throw new ArgumentException("Unexpected type-- expected PairState");

             if (!this.signature.Contains(actionSymbol))
                throw new ArgumentException("Unexpected action symbol-- must be in signature");

            // TO DO: include case where no parameter domain is present (as opposed to case where Any is specified)
            bool m1HasDomain = (this.signature.IsShared(actionSymbol) || this.signature.IsOnlyInM1(actionSymbol));
            bool m2HasDomain = (this.signature.IsShared(actionSymbol) || this.signature.IsOnlyInM2(actionSymbol));

            m1HasDomain = m1HasDomain && (m1.ActionArity(actionSymbol) > parameterIndex) && m1.HasActionParameterDomain(M1Reduct(ps), actionSymbol, parameterIndex);
            m2HasDomain = m2HasDomain && (m2.ActionArity(actionSymbol) > parameterIndex) && m2.HasActionParameterDomain(M2Reduct(ps), actionSymbol, parameterIndex); 

            Set<Term> d1 = m1HasDomain ? m1.ActionParameterDomain(M1Reduct(ps), actionSymbol, parameterIndex) : AnyDomain;
            Set<Term> d2 = m2HasDomain ? m2.ActionParameterDomain(M2Reduct(ps), actionSymbol, parameterIndex) : AnyDomain;

            if (d1.Equals(AnyDomain))
                return d2;
            else if (d2.Equals(AnyDomain))
                return d1;
            else
                return d1.Intersect(d2);
        }

        /// <summary>
        /// Returns true if the action is enabled in the given state
        /// </summary>
        public override bool IsEnabled(IState state, CompoundTerm action)
        //^ requires IsPotentiallyEnabled(state, action.FunctionSymbol1);
        {
            PairState ps = state as PairState;
            if (ps == null)
                throw new ArgumentException("Unexpected type-- expected PairState");

            if (action == null)
                throw new ArgumentNullException("action");
  
            Symbol actionSymbol = action.Symbol;
            if (this.signature.IsShared(actionSymbol))
                return m1.IsEnabled(M1Reduct(ps), action) && m2.IsEnabled(M2Reduct(ps), action);
            else if (this.signature.IsOnlyInM1(actionSymbol))
                return m1.IsEnabled(M1Reduct(ps), action);
            else if (this.signature.IsOnlyInM2(actionSymbol))
                return m2.IsEnabled(M2Reduct(ps), action);
            else
                throw new ArgumentException("Invalid argument-- action symbol " + actionSymbol.ToString() + " not in signature.");
        
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
            PairState ps = state as PairState;
            if (ps == null)
                throw new ArgumentException("Unexpected type-- expected PairState");

            Symbol actionSymbol = action.Symbol;
            if (m1.ActionSymbols().Contains(actionSymbol))
                foreach (string s in m1.GetEnablingConditionDescriptions(M1Reduct(ps), action, returnFailures))
                    yield return s;

            if (m2.ActionSymbols().Contains(actionSymbol))
                foreach (string s in m2.GetEnablingConditionDescriptions(M2Reduct(ps), action, returnFailures))
                    yield return s;
        }

        /// <summary>
        /// Gets all enabled actions in the given state that have the given action symbol
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actionSymbol"></param>
        /// <returns></returns>
        public override IEnumerable<CompoundTerm> GetActions(IState state, Symbol actionSymbol)
        {
            PairState ps = state as PairState;
            if (ps == null)
                throw new ArgumentException("Unexpected type-- expected PairState");

            if (actionSymbol == null)
                throw new ArgumentNullException("actionSymbol");

           
            if (this.signature.IsOnlyInM1(actionSymbol))
            {
                IState M1State = M1Reduct(ps);
                foreach (CompoundTerm a in m1.GetActions(M1State, actionSymbol))
                    yield return a;
            }
            else if (this.signature.IsOnlyInM2(actionSymbol))
            {
                IState M2State = M2Reduct(ps);
                foreach (CompoundTerm a in m2.GetActions(M2State, actionSymbol))
                    yield return a;
            }
            else if (this.signature.IsShared(actionSymbol))
            {
                IState m1State = M1Reduct(ps);
                IState m2State = M2Reduct(ps);
                int arity = this.ActionArity(actionSymbol);
                if (arity > 0)
                {
                    Sequence<Set<Term>> args = Sequence<Set<Term>>.EmptySequence;
                    for (int i = 0; i < arity; i++)
                    {
                        args = args.AddLast(this.ActionParameterDomain(state, actionSymbol, i));
                    }

                    IEnumerable<Sequence<Term>> cartesianProduct = CartesianProduct(args);
                   
                    foreach (Sequence<Term> arglist in cartesianProduct)
                    {
                        CompoundTerm action = new CompoundTerm(actionSymbol, arglist);
                        if (m1.IsEnabled(m1State, action) && m2.IsEnabled(m2State, action))
                            yield return action;
                    }
                }
                else
                {
                    CompoundTerm action = new CompoundTerm(actionSymbol, Sequence<Term>.EmptySequence);
                    if (m1.IsEnabled(m1State, action) && m2.IsEnabled(m2State, action))
                        yield return action;
                }
            }
            else
                throw new ArgumentException("Invalid argument-- action symbol " + actionSymbol.ToString() + " not in signature.");
        
        }

        private IEnumerable<Sequence<Term>> CartesianProduct(Sequence<Set<Term>> args)
        {
            if (args.Count == 0)
                yield return Sequence<Term>.EmptySequence;
            else if (args.Count == 1)
                foreach (Term term in args.Head)
                    yield return new Sequence<Term>(term);
            else if (args.Count > 0)
            {
                foreach (Sequence<Term> tuple in CartesianProduct(args.Tail))
                    foreach (Term term in args.Head)
                        yield return tuple.AddFirst(term);
            }
        }
        #endregion

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
            // it is assumed that the action is enabled in the given state
        
            PairState ps = startState as PairState;
            if (ps == null)
                throw new ArgumentException("Unexpected type-- expected PairState");
            
            Symbol actionSymbol = action.Symbol;
            if (!this.signature.Contains(actionSymbol))
                throw new ArgumentException("Invalid argument-- action symbol " + actionSymbol.ToString() + " not in signature.");
            
            bool doM1 = this.signature.IsShared(actionSymbol) || this.signature.IsOnlyInM1(actionSymbol);
            bool doM2 = this.signature.IsShared(actionSymbol) || this.signature.IsOnlyInM2(actionSymbol);

            TransitionProperties m1TransitionProperties = new TransitionProperties();
            TransitionProperties m2TransitionProperties = new TransitionProperties(); 
            IState targetState1 = doM1 ? m1.GetTargetState(M1Reduct(ps), action, transitionPropertyNames, out m1TransitionProperties) : M1Reduct(ps);
            IState targetState2 = doM2 ? m2.GetTargetState(M2Reduct(ps), action, transitionPropertyNames, out m2TransitionProperties) : M2Reduct(ps);

            transitionProperties = m1TransitionProperties.Union(m2TransitionProperties);
            return PairState.CreateState(targetState1, targetState2);
        }



        //public IEnumerable<Step> GetAllSteps(IState state)
        //{
        //    PairState ps = state as PairState;
        //    if (ps == null)
        //        throw new ArgumentException("Unexpected type-- expected PairState");

        //    foreach (Symbol actionSymbol in this.ActionSymbols())
        //    {
        //        if (IsPotentiallyEnabled(state, actionSymbol))
        //            foreach(CompoundTerm action in this.GetActions(state, actionSymbol))
        //                foreach (Step step in this.GetSteps(state, action))
        //                    yield return step;
        //    }
        //}


        /// <summary>
        /// Returns true if all the component states are accepting states
        /// </summary>
        public override bool IsAccepting(IState state)
        {
            PairState ps = state as PairState;
            if (ps == null) throw new ArgumentException("Unexpected type-- expected PairState");

            return m1.IsAccepting(ps.First) && m2.IsAccepting(ps.Second);
        }

        /// <summary>
        /// Boolean value indicating whether all state invariant predicates
        /// defined by this model program are satisfied by <paramref name="state"/>. In general,
        /// failure to satisfy the state invariants indicates a modeling error.
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>True if <paramref name="state"/>satisfies all state invariants of 
        /// this model program; false otherwise.</returns>
        public override bool SatisfiesStateInvariant(IState state)
        {
            PairState ps = state as PairState;
            if (ps == null) throw new ArgumentException("Unexpected type-- expected PairState");

            return m1.SatisfiesStateInvariant(ps.First) && m2.SatisfiesStateInvariant(ps.Second);
        }

        /// <summary>
        /// Boolean value indicating whether all state filter predicates
        /// defined by this model program are satisfied by <paramref name="state"/>. 
        /// States not satisfying a state filter are excluded during exploration.
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>True if <paramref name="state"/>satisfies all state filters of 
        /// this model program; false otherwise.</returns>
        public override bool SatisfiesStateFilter(IState state)
        {
            PairState ps = state as PairState;
            if (ps == null) throw new ArgumentException("Unexpected type-- expected PairState");

            return m1.SatisfiesStateFilter(ps.First) && m2.SatisfiesStateFilter(ps.Second);
        }

        /// <summary>
        /// Checks whether a sort is an abstract type. The corresponding set is created once in the constructor.
        /// </summary>
        /// <param name="s">A symbol denoting a sort (i.e. abstract type)</param>
        /// <returns>Returns true if the sort corresponding to the symbol is abstract.</returns>
        public override bool IsSortAbstract(Symbol s)
        {
            return m1.IsSortAbstract(s) || m2.IsSortAbstract(s);
        }


        #endregion

        #region ComposedSignature helper class
        internal class ComposedSignature
        {
            internal Set<Symbol> actionSymbols;
            internal Set<Symbol> m1ActionSymbols;
            internal Set<Symbol> m2ActionSymbols;
            internal Set<Symbol> sharedActionSymbols;

            internal bool IsShared(Symbol actionSymbol)
            {
                return sharedActionSymbols.Contains(actionSymbol);
            }

            internal bool IsOnlyInM1(Symbol actionSymbol)
            {
                return m1ActionSymbols.Contains(actionSymbol);
            }

            internal bool IsOnlyInM2(Symbol actionSymbol)
            {
                return m2ActionSymbols.Contains(actionSymbol);
            }

            internal bool Contains(Symbol actionSymbol)
            {
                return this.actionSymbols.Contains(actionSymbol);
            }

            internal ComposedSignature(ModelProgram m1, ModelProgram m2)
            {
                Set<Symbol> allActionSymbols = m1.ActionSymbols().Union(m2.ActionSymbols());

                this.actionSymbols = allActionSymbols;
                this.m1ActionSymbols = allActionSymbols.Difference(m2.ActionSymbols());
                this.m2ActionSymbols = allActionSymbols.Difference(m1.ActionSymbols());
                this.sharedActionSymbols = m1.ActionSymbols().Intersect(m2.ActionSymbols());
            }
        } 
        #endregion


    }
}
