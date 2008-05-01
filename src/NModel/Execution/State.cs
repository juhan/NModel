//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using NModel.Algorithms;
using NModel.Terms;
using NModel.Internals;

//^ using Microsoft.Contracts;

namespace NModel.Execution
{
    /// <summary>
    /// SimpleState implements a basic mapping of fields to values.
    /// </summary>
    public sealed class SimpleState : CompoundValue, IExtendedState
    {
        readonly int hashCode;
        readonly Term controlMode;
        readonly ValueArray<Term> locationValues;
        readonly Map<Symbol, int> domainMap;
        readonly string modelName;
        readonly ValueArray<string> locationNames;


        #region Properties and Accessors
        /// <summary>
        /// A term denoting the control state of the machine in this state
        /// </summary>
        public Term ControlMode
        {
            get { return controlMode; }
        }

        /// <summary>
        /// A map of sorts (abstract types) to integers that repesent the next available id
        /// </summary>
        public Map<Symbol, int> DomainMap
        {
            get { return domainMap; }
        }

        /// <summary>
        /// Accessor for elements of data state
        /// </summary>
        /// <param name="i">An index in [0, this.LocationValuesCount) </param>
        /// <returns>The term representing the value of this location</returns>
        public Term GetLocationValue(int i)
        // requires 0 <= i && i < this.LocationValuesCount;
        {
            return this.locationValues[i];
        }

        /// <summary>
        /// The number of location values
        /// </summary>
        public int LocationValuesCount
        {
            get { return this.locationValues.Length; }
        } 
        #endregion

        #region Constructors
        /// <summary>
        /// Since states are immutable, we intern states in a table to improve the performance of
        /// subsequent comparisons for equality. To prevent a memory leak, we use a weak cache that does not prevent
        /// the garbage collector from reclaiming an otherwise unreferenced state.
        /// 
        /// This will optimize comparison at the expense of more work during construction. The assumption is 
        /// that states will be compared many more times than they will be constructed.
        /// </summary>
        static WeakCache<SimpleState, SimpleState> cache = new WeakCache<SimpleState, SimpleState>(
            delegate(SimpleState newState) { return newState; }
            );

        /// <summary>
        /// Creates a state.
        /// </summary>
        /// <param name="controlMode">The control state of the machine</param>
        /// <param name="fieldValues">The values of each data field. This array is captured; the method
        /// assumes that the array passed as an argument will never be modified.</param>
        /// <param name="domainMap">A mapping of domain names to integers that represent the next available id.</param>
        /// <param name="modelName">name of the model</param>
        /// <param name="locationNames">names of locations</param>
        /// <returns>A state with the given control mode, field values and domain map.</returns>
        public static SimpleState CreateState(Term controlMode,
                                              Term[] fieldValues,
                                              Map<Symbol, int> domainMap,
                                              string modelName,
                                              ValueArray<string> locationNames
                                              )
        {
            SimpleState newState = new SimpleState(controlMode, fieldValues, domainMap, modelName, locationNames);
            return cache.Get(newState);
        }

        /// <summary>
        /// Retuns a new simple state where the control mode has been replaced with the new one
        /// </summary>
        public SimpleState ReplaceControlMode(Term newControlMode)
        {
            SimpleState newState = new SimpleState(newControlMode, this.locationValues, this.domainMap, this.modelName, this.locationNames);
            return cache.Get(newState);
        }

        /// <summary>
        /// Can be used when no instance fields are present
        /// </summary>
        public static SimpleState CreateState(Term controlMode, Term[] fieldValues, string modelName, ValueArray<string> locationNames)
        {
            return CreateState(controlMode, fieldValues, Map<Symbol, int>.EmptyMap, modelName, locationNames);
        }

        /// <summary>
        /// <para>The constructor of a State value</para>
        /// <para>This constructor should only be called by an implementer of ModelProgram.</para>
        /// </summary>
        /// <param name="fieldValues">
        /// An array of values that denote the interpretation of fields in this state. Fields
        /// are identified positionally. <see>class StateCollection</see>
        /// <para>Since the caller guarantess to transfer ownership no clone of the input argument is used here.</para>
        /// </param>
        /// <param name="controlMode">control mode of the simple state</param>
        /// <param name="domainMap">domain map</param>
        /// <param name="locationNames">string names of locations</param>
        /// <param name="modelName">name of the model</param>
        public SimpleState(Term controlMode,
                           Term[] fieldValues,
                           Map<Symbol, int> domainMap,
                           string modelName,
                           ValueArray<string> locationNames)
        //^ requires forall {int i in (0:fieldValues.Length); Owner.Same(fieldValues, fieldValues[i])};
        {
            ValueArray<Term> newFieldValues = new ValueArray<Term>(fieldValues);
            this.controlMode = controlMode;
            this.locationValues = newFieldValues;
            this.domainMap = domainMap;
            this.modelName = modelName;
            this.locationNames = locationNames;
            this.hashCode = TypedHash<SimpleState>.ComputeHash(controlMode, newFieldValues, domainMap, modelName, locationNames);

        }

        private SimpleState(Term controlMode,
                   ValueArray<Term> locationValues,
                   Map<Symbol, int> domainMap,
                   string modelName,
                   ValueArray<string> locationNames)
        //^ requires forall {int i in (0:fieldValues.Length); Owner.Same(fieldValues, fieldValues[i])};
        {
            this.controlMode = controlMode;
            this.locationValues = locationValues;
            this.domainMap = domainMap;
            this.modelName = modelName;
            this.locationNames = locationNames;
            this.hashCode = TypedHash<SimpleState>.ComputeHash(controlMode, locationValues, domainMap, modelName, locationNames);
        }

        /// <summary>
        /// Constructs a simple state with the given arguments
        /// </summary>
        public SimpleState(Term controlMode, Term[] fieldValues, string modelName, ValueArray<string> locationNames)
        //^ requires forall {int i in (0:fieldValues.Length); Owner.Same(fieldValues, fieldValues[i])};
        {
            ValueArray<Term> newFieldValues = new ValueArray<Term>(fieldValues);
            this.controlMode = controlMode;
            this.locationValues = newFieldValues;
            this.domainMap = Map<Symbol, int>.EmptyMap;
            this.modelName = modelName;
            this.locationNames = locationNames;
            this.hashCode = TypedHash<SimpleState>.ComputeHash(controlMode, newFieldValues, Map<Symbol, int>.EmptyMap, modelName, locationNames);
        } 
        #endregion

        #region CompoundValue overrides
        /// <summary>
        /// Override of Object.GetHashCode(). 
        /// </summary>
        /// <returns>The hash value associated with this state.</returns>
        public override int GetHashCode()
        {
            return this.hashCode;
        }

        /// <summary>
        /// Override of Object.Equal for States.
        /// </summary>
        /// <param name="obj">The second object.</param>
        /// <returns></returns>
        public override bool Equals(object/*?*/ obj)
        {
            if (obj == null)
                return false;
            else if ((object)this == obj)
                return true;
            else if (this.GetHashCode() != obj.GetHashCode())
                return false;
            else
            {
               return base.Equals(obj);
            }
        } 

        /// <summary>
        /// Field values
        /// </summary>
        /// <returns>Structure field values</returns>
        public override IEnumerable<IComparable> FieldValues()
        {
 	       yield return controlMode;
           yield return locationValues;
           yield return domainMap;
        }
        #endregion


        #region IExtendedState Members

        /// <summary>
        /// Get the model name
        /// </summary>
        public string ModelName
        {
            get { return modelName; }
        }

        /// <summary>
        /// Get the name of the location for the given location id
        /// </summary>
        public string GetLocationName(int locationId)
        {
            if (this.locationNames == null)
                return "Field(" + locationId.ToString() + ")";
            if (0 <= locationId && locationId < this.locationNames.Length)
                return locationNames[locationId];
            else
                throw new ArgumentOutOfRangeException("locationId");
        }

        #endregion
    }

    /// <summary>
    /// Represents a pair of states.
    /// Pair states are used in product model programs.
    /// </summary>
    public sealed class PairState : CompoundValue, IPairState
    {
        readonly IState first;
        readonly IState second;

        #region Properties
        /// <summary>
        /// First state of the pair state
        /// </summary>
        public IState First
        {
            get { return this.first; }
        }

        /// <summary>
        /// Second state of the pair state
        /// </summary>
        public IState Second
        {
            get { return this.second; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Since states are immutable, we intern states in a table to improve the performance of
        /// subsequent comparisons for equality. To prevent a memory leak, we use a weak cache that does not prevent
        /// the garbage collector from reclaiming an otherwise unreferenced state.
        /// 
        /// This will optimize comparison at the expense of more work during construction. The assumption is 
        /// that states will be compared many more times than they will be constructed.
        /// </summary>
        static WeakCache<PairState, IState, IState> cache = new WeakCache<PairState, IState, IState>(
          delegate(IState first, IState second) { return new PairState(first, second); }
        );

        /// <summary>
        /// Create a pair state from two states
        /// </summary>
        public static PairState CreateState(IState first, IState second)
        {
            return cache.Get(first, second);
        }

        /// <summary>
        /// Create a pair state from two states
        /// </summary>
        public PairState(IState first, IState second)
            : base()
        {
            this.first = first;
            this.second = second;
        }
        #endregion

        #region CompoundValue overrides

        /// <summary>
        /// Get the field values in the first and the second states
        /// </summary>
        public override IEnumerable<IComparable> FieldValues()
        {
            yield return this.first;
            yield return this.second;
        } 

        #endregion

        #region IState Members

        /// <summary>
        /// Control mode of the state
        /// </summary>
        public Term ControlMode
        {
            get
            {
                return new CompoundTerm(Symbol.Parse("Pair<Term, Term>"), first.ControlMode, second.ControlMode);
            }
        }

        //public int LocationValuesCount
        //{
        //    get
        //    {
        //        return first.LocationValuesCount + second.LocationValuesCount;
        //    }
        //}

        //public Term GetLocationValue(int i)
        ////^ requires 0 <= i && i < this.LocationValuesCount;
        //{
        //    if (i < first.LocationValuesCount)
        //        return first.GetLocationValue(i);
        //    else
        //        return second.GetLocationValue(i - first.LocationValuesCount);
        //}

        //public Map<string, int> DomainMap
        //{
        //    get
        //    {
        //        // amazingly hard without language support for comprehensions
        //        // merge two maps taking the larger of the range values
        //        bool d1IsLarger = (this.First.DomainMap.Count > this.Second.DomainMap.Count);
        //        Map<string, int> d1 = (d1IsLarger ? this.First.DomainMap : this.Second.DomainMap);
        //        Map<string, int> d2 = (d1IsLarger ? this.Second.DomainMap : this.First.DomainMap);
        //        Map<string, int> result = d1;
        //        foreach (Pair<string, int> keyValue in d2)
        //        {
        //            int value1;
        //            bool found = d1.TryGetValue(keyValue.First, out value1);
        //            if (!found || (found && keyValue.Second > value1))
        //            {
        //                result = result.Override(keyValue);
        //            }
        //        }
        //        return result;
        //    }
        //}

        #endregion
    }

    /// <summary>
    /// Locations are labels that identify elements of state.
    /// There is one location per field in the case of static fields.
    /// </summary>
    public class Location
    {
        //readonly StateVariable stateVariable;
        Term[]/*?*/ values;

        /// <summary>
        /// Creates a location with the given values
        /// </summary>
        public Location(/* StateVariable stateVariable, */ Term[]/*?*/ values)
        {
            // this.stateVariable = stateVariable;
            this.values = values;
        }

        ///// <summary>
        ///// The state variable of the location
        ///// </summary>
        //public StateVariable StateVariable
        //{
        //    get { return stateVariable; }
        //}

        /// <summary>
        /// Get values of the location
        /// </summary>
        /// <returns></returns>
        public Term[]/*?*/ Values()
        {
            return values;
        }
    }

    /// <summary>
    /// State variables are dynamic function symbols 
    /// that are used to form locations
    /// </summary>
    public sealed class StateVariable : CompoundValue
    {
        readonly string name;
        readonly Symbol sort;
        //int Arity;

        /// <summary>
        /// Yields first the name and then the sort
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IComparable> FieldValues()
        {
            yield return name;
            yield return sort;
        }

        /// <summary>
        /// Constructs a state variable with given name and sort
        /// </summary>
        public StateVariable(string name, Symbol sort)
        {
            this.name = name;
            this.sort = sort;
        }
        /// <summary>
        /// Name of the state variable
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Sort of the state variable
        /// </summary>
        public Symbol VariableSort
        {
            get { return sort; }
        }
    }

    /// <summary>
    /// A collection is a pool of states. 
    /// </summary>
    public class StateCollection : IEnumerable<IState>
    {
        // invariant Forall{s in States.Keys; s.LocationValues.Count == this.Fields.Count};
        // invariant Forall{(k, v) in States; k != null && k.Equals(v)};
        /// <summary>
        /// Implements a set of states as a dictionary that maps keys to themselves.
        /// </summary>
        Dictionary<IState, IState> States;   
        
        /// <summary>
        /// For all i, j in range, this.Fields[i] is the location of this.States[j].LocationValues[i]
        /// </summary>
        readonly Location[] Fields;        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields"></param>
        public StateCollection(Location[] fields)
        {
            this.States = new Dictionary<IState, IState>();
            this.Fields = fields;
        }

        /// <summary>
        /// Adds a state to the state space and canonicalizes the reference
        /// The caller can use the returned value in place of the argument 
        /// for better efficiency. (It is guaranteed that arg.Equals(result) == true.)
        /// </summary>
        /// <param name="newState">The state to be added</param>
        /// <returns>The canonical representation of the added state</returns>
        public IState InternState(IState newState)
        // requires newState.LocationValues.Count == this.Fields.Count;
        // ensures newState.Equals(result);
        {
            // Cleverness alert: this consolidates representations by returning a prior
            // representation if it exists and the new representation otherwise. 
            IState/*?*/ internedState;
            if (!States.TryGetValue(newState, out internedState))
            {
                internedState = newState;
                States[newState] = newState;
            }
            //^ assume internedState != null;
            return internedState;
        }

        
        #region IEnumerable<State> Members

        /// <summary>
        /// Enumerates all the states in this collection
        /// </summary>
        public IEnumerator<IState> GetEnumerator()
        {
            foreach (IState state in States.Values)
            {
                yield return state;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
