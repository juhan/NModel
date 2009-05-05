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
    /// The interface to a model program. A model program is an abstraction of a program 
    /// in terms of a transition system. States of the program and transitions between
    /// them are explicit.
    /// </summary>
    public abstract class ModelProgram
    {

        #region Action signature

        /// <summary>
        /// Nonempty set of actions symbols.
        /// </summary>
        /// <returns>A value greater than or equal to zero.</returns>
        /// <remarks>A model program has a fixed vocabulary of action symbols.</remarks>
        public abstract Set<Symbol> ActionSymbols();

        /// <summary>
        /// Number of arguments associated with action symbol <paramref name="actionSymbol"/>
        /// </summary>
        /// <param name="actionSymbol">A symbol naming an action of this model program.</param>
        /// <returns>The number of arguments required in a <see cref="Term"/> invoking 
        /// this <paramref name="actionSymbol"/>. The value returned will be greater than or equal to zero.</returns>
        /// <remarks>
        /// Every action symbol has a fixed number of parameters associated with it. The number of parameters
        /// is called the action's <i>arity</i>.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if <paramref name="actionSymbol"/> is not in this
        /// model program's fixed vocabulary given by <see cref="ActionSymbols"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="actionSymbol"/> is null</exception>
        public abstract int ActionArity(Symbol actionSymbol);

        /// <summary>
        /// Gets the sort (i.e., abstract type) of the ith parameter of action <paramref name="actionSymbol"/>
        /// </summary>
        /// <param name="actionSymbol">A symbol naming an action of this model program.</param>
        /// <param name="parameterIndex">An integer in the interval [0, this.ActionArity(actionSymbol))</param>
        /// <returns>The sort (abstract type) of the ith parameter of action <paramref name="actionSymbol"/></returns>
        /// <remarks>
        /// <br><c>requires actionSymbol != null;</c></br>
        /// <br><c>requires this.ActionSymbols.Contains(actionSymbol);</c></br>
        /// <br><c>ensures parameterIndex &gt;= this.ActionArity(actionSymbol) ==&gt; result == new Symbol("Object");</c></br>
        /// </remarks>
        public abstract Symbol ActionParameterSort(Symbol actionSymbol, int parameterIndex);

        #endregion

        #region State signature
        /// <summary>
        /// Number of location values in the state signature of this model program
        /// </summary>
        /// <remarks>
        /// <br><c>ensures result >= 0;</c></br>
        /// </remarks>
        public abstract int LocationValueCount { get; }

        /// <summary>
        /// String name identifying the ith location value in the state signature of this model program
        /// </summary>
        /// <param name="i">An index in the interval [0,  LocationValueClount)</param>
        /// <returns>String name identifying the ith location value</returns>
        /// <remarks>
        /// <br><c>requires 0 &lt;= i &amp;&amp; i &lt; this.LocationValueCount;</c></br>
        /// </remarks>
        public abstract string LocationValueName(int i);

        /// <summary>
        /// Model program that provides the ith location value of the state signature of this model program.
        /// The result will be this model program, except under composition, when the result will the be 
        /// a leaf of the composition tree.
        /// <br><c>requires 0 &lt;= i &amp;&amp; i &lt; this.LocationValueCount;</c></br>
        /// </summary>
        /// <param name="i">An index in the interval [0,  LocationValueClount)</param>
        /// <returns>Model program that provides the ith location value.</returns>
        public abstract string LocationValueModelName(int i);

        /// <summary>
        /// Symbol denoting the sort (abstract type) of the ith location value in the state signature 
        /// of this model program
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public abstract Symbol LocationValueSort(int i);
        #endregion

        #region Initial state
        /// <summary>
        /// The initial state.
        /// </summary>
        /// <remarks>
        /// <br>ensures result != null;</br>
        /// <br>ensures result.ModelProgram == this; </br>
        /// </remarks>
        public abstract IState InitialState { get; }
        #endregion

        #region Enabledness of actions and parameter generation
        /// <summary>
        /// Checks whether a given action is potentially enabled 
        /// in this state.
        /// <br><c>requires state != null;</c></br>
        /// <br>requires state.ModelProgram == this; </br>
        /// <br>requires actionSymbol != null;</br>
        /// </summary>
        public abstract bool IsPotentiallyEnabled(IState state, Symbol actionSymbol);

        /// <summary>
        /// Enumerates the action symbols that are potentially enabled with respect to this
        /// control point and data state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public abstract Set<Symbol> PotentiallyEnabledActionSymbols(IState state);

  
        /// <summary>
        /// Checks if a ground label is enabled in this control state with respect to a data state
        /// 
        /// requires: data != null
        /// requires: label != null and label.IsGround
        /// </summary>
        public abstract bool IsEnabled(IState state, CompoundTerm action);

        /// <summary>
        /// Gets string descriptions of the enabling conditions
        /// </summary>
        /// <param name="state">The state in which the </param>
        /// <param name="action">The action whose enabling conditions will queried</param>
        /// <param name="returnFailures">If <c>true</c>, enabling conditions that fail in state 
        /// <paramref name="state"/> will be returned. If <c>false</c>, all enabling conditions
        /// that are satisfied will be returned.</param>
        /// <returns>Description strings for the enabling conditions of action <paramref name="action"/></returns>
        public abstract IEnumerable<string> GetEnablingConditionDescriptions(IState state, CompoundTerm action, bool returnFailures);

        #endregion

        #region Exploration
        /// <summary>
        /// Does this model program have an interface to parameter generation for this parameter?
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actionSymbol"></param>
        /// <param name="parameterIndex"></param>
        /// <returns>A set of terms representing the possible values</returns>
        public abstract bool HasActionParameterDomain(IState state, Symbol actionSymbol, int parameterIndex);

        /// <summary>
        /// Interface to parameter generation
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actionSymbol"></param>
        /// <param name="parameterIndex"></param>
        /// <returns>A set of terms representing the possible values</returns>
        public abstract Set<Term> ActionParameterDomain(IState state, Symbol actionSymbol, int parameterIndex);  // !unconconstrained && index < arity

        /// <summary>
        /// Gets the actions of an action symbol in the given state.
        /// Note: interface to parameter generation. May return non-ground terms.
        /// </summary>
        public abstract IEnumerable<CompoundTerm> GetActions(IState state, Symbol actionSymbol);
        #endregion

        #region Stepping

        /// <summary>
        /// Returns the names of meta-properties that may be collected
        /// during the calculation of the <see cref="GetTargetState"/>.
        /// </summary>
        /// <returns>The names of meta-properties to be collected
        /// during the calculation of the step.</returns>
        /// <seealso cref="GetTargetState"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public abstract Set<string> GetTransitionPropertyNames();

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        public abstract IState GetTargetState(IState startState, CompoundTerm action, Set<string> transitionPropertyNames,
                              out TransitionProperties transitionProperties);

        #endregion

        #region Halting condition
        /// <summary>
        /// The accepting status of a state of this model program. Valid runs of 
        /// a model program must terminate in an accepting state.
        /// <br>requires state != null;</br>
        /// <br>requires state.ModelProgram == this; </br>
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public abstract bool IsAccepting(IState state);
        #endregion

        #region State invariant
        /// <summary>
        /// Boolean value indicating whether all state invariant predicates
        /// defined by this model program are satisfied by <paramref name="state"/>. In general,
        /// failure to satisfy the state invariants indicates a modeling error.
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>True if <paramref name="state"/>satisfies all state invariants of 
        /// this model program; false otherwise.</returns>
        public abstract bool SatisfiesStateInvariant(IState state);
        #endregion

        #region State filter
        /// <summary>
        /// Boolean value indicating whether all state filter predicates
        /// defined by this model program are satisfied by <paramref name="state"/>. 
        /// States not satisfying a state filter are excluded during exploration.
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>True if <paramref name="state"/>satisfies all state filters of 
        /// this model program; false otherwise.</returns>
        public abstract bool SatisfiesStateFilter(IState state);
        #endregion

        /// <summary>
        /// Checks whether a sort is an abstract type. The corresponding set is created once in the constructor.
        /// </summary>
        /// <param name="s">A symbol denoting a sort (i.e. abstract type)</param>
        /// <returns>true if the sort is abstract, false otherwise.</returns>
        public abstract bool IsSortAbstract(Symbol s);

        /// <summary>
        /// Checks whether the field denoted by the index is a static field or a fieldmap.
        /// This check is only valid with the <see cref="LibraryModelProgram"/> type.
        /// </summary>
        /// <param name="i">Index of the field</param>
        /// <returns>true if the field is static</returns>
        public virtual bool IsFieldStatic(int i)
        {
            throw new InvalidOperationException("IsFieldStatic is not implemented in the current context.");
        }

    }

    /// <summary>
    /// Interface to provide a name.
    /// </summary>
    public interface IName
    {
        /// <summary>
        /// Provided name
        /// </summary>
        string Name { get;}
    }

}
