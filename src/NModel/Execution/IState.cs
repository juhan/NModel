//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using NModel.Terms;

namespace NModel.Execution
{
    /// <summary>
    /// <para>State is a value type that represents a distinct control and data configuration
    /// of a model program.</para>
    /// <para>Values are terms.</para>
    /// </summary>
    /// <note><c>IState</c> is a value type. <c>Equals()</c> and <c>GetHashCode()</c> must
    /// be overridden appropriately. <c>IComparable</c> must be implemented to allow state to
    /// appear in value collections such as sets and maps.</note>
    public interface IState : IComparable
    {
        /// <summary>
        /// A term denoting the control state of the model program in this state
        /// </summary>
        Term ControlMode { get; }
    }

    /// <summary>
    /// A state of a model program with data fields, in the spirit of 
    /// extended finite state machines (EFSMs).
    /// </summary>
    public interface IExtendedState : IState
    {
        /// <summary>
        /// Accessor for elements of data state
        /// </summary>
        /// <param name="i">An index in [0, this.LocationValuesCount) </param>
        /// <returns>The term representing the value of this location</returns>
        Term GetLocationValue(int i);

        /// <summary>
        /// The number of location values
        /// </summary>
        int LocationValuesCount { get; }

        /// <summary>
        /// A map of domain names to integers that repesent the next available id. This is used
        /// when generating labels of dynamically created instances.
        /// </summary>
        Map<Symbol, int> DomainMap { get; }

        /// <summary>
        /// The string name of the model program that produced this state. Used for printing; 
        /// not guaranteed to be distinct.
        /// </summary>
        string ModelName { get; }

        /// <summary>
        /// Returns the string name of the model program state variable indexed by <paramref name="locationId"/>.
        /// This is not guaranteed to be distinct.
        /// </summary>
        /// <param name="locationId">The ith location of this state</param>
        /// <returns>The string name of the model program state variable for <paramref name="locationId"/></returns>
        string GetLocationName(int locationId);
    }

    /// <summary>
    /// A state of a product machine.
    /// </summary>
    public interface IPairState : IState
    {
        /// <summary>
        /// The first state
        /// </summary>
        IState First { get; }

        /// <summary>
        /// The second state
        /// </summary>
        IState Second { get; }
    }
}
