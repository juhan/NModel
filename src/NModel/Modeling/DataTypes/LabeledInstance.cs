//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using NModel.Terms;
using NModel.Internals;
using NModel.Attributes;

namespace NModel
{
    namespace Internals
    {
        /// <summary>
        /// An objectId is a compound value in the form ObjectId(sort, i) where sort is 
        /// a symbol giving the abstract type and i is an integer index that 
        /// identifies distinct values in the sort.
        /// </summary>
        public sealed class ObjectId : CompoundValue
        {
            readonly Symbol sort;
            readonly int id;

            /// <summary>
            /// The fields of this compound value
            /// </summary>
            /// <returns>The fields</returns>
            public override IEnumerable<IComparable> FieldValues()
            {
                return new IComparable[] { sort, id };
                //yield return sort;
                //yield return id;
            }

            /// <summary>
            /// Constructs an object Id
            /// </summary>
            /// <param name="sort">A symbol denoting an abstract type</param>
            /// <param name="id">An integer index that 
            /// identifies distinct values in the sort</param>
            public ObjectId(Symbol sort, int id)
            //^ requires sort != null;
            {
                this.sort = sort;
                this.id = id;
            }

            /// <summary>
            /// Returns a symbol denoting an abstract type
            /// </summary>
            public Symbol ObjectSort { get { return sort; } }

            /// <summary>
            /// An integer index that identifies distinct values in the sort
            /// </summary>
            public int Id { get { return id; } }
        }

        /// <summary>
        /// Base class of model instances. Supports labeling using user-provided object ids.
        /// Users should not inherit this class directly; instead, when defining class T,
        /// inherit from LabeledInstance&lt;T&gt;.
        /// </summary>
        public class LabeledInstance : AbstractValue
        {
            [ExcludeFromState]
            ObjectId objectId;

            /// <summary>
            /// Creates a labeled instance with a given object id
            /// </summary>
            protected LabeledInstance(ObjectId objectId)
            {
                this.objectId = objectId;
            }

            /// <summary>
            /// Initialize the labeled instance.
            /// The base behavior is empty.
            /// </summary>
            public virtual void Initialize() { }

            /// <summary>
            /// Determines if this value has an object id (that is, is of type <see cref="LabeledInstance"/>),
            /// or has a subvalue that has an object id (for example, a set of instances).
            /// </summary>
            /// <returns>True if this value has an object id or contains a value with an object id.</returns>
            public override bool ContainsObjectIds() { return true; }

            ///// <summary>
            ///// Returns the sort associated with type <paramref name="t"/>
            ///// </summary>
            ///// <param name="t">A .NET type</param>
            ///// <returns>A symbol denoting the abstract type implemented by type <paramref name="t"/> in the current context.</returns>
            //public static Symbol TypeSort(Type t)
            //{
            //    Symbol/*?*/ sort;
            //    if (InterpretationContext.GetCurrentContext().TypeSortTryGetValue(t, out sort))
            //    {
            //        return sort;
            //    }
            //    else
            //    {
            //        StringBuilder sb = new StringBuilder();
            //        PrettyPrinter.FormatType(sb, t);
            //        sort = Symbol.Parse(sb.ToString());

            //        object/*?*/[]/*?*/ attrs = t.GetCustomAttributes(typeof(SortAttribute), true);
            //        if (attrs != null && attrs.Length > 0)
            //        {
            //            SortAttribute attr = (SortAttribute)attrs[0];                   

            //            string name = attr.Name;
            //            if (!string.IsNullOrEmpty(name)) 
            //            {
            //                // if attribute contains a name, use it
            //                sort = new Symbol(name, sort.DomainParameters);
            //            }
            //        }           

            //        InterpretationContext.GetCurrentContext().RegisterSortType(sort, t);
            //        return sort;
            //    }
            //}

            /// <summary>
            /// Returns the term representation of this value.
            /// </summary>
            public override Term AsTerm
            {
                get
                {
                    Symbol sort = this.Sort;
                    Sequence<Term> arguments = new Sequence<Term>(new Literal(this.Label.Id));
                    return new CompoundTerm(sort, arguments);
                }
            }

            #region InterpretationContext


            /// <summary>
            /// Increment the instance counter for this sort.
            /// </summary>
            /// <returns>The incremented count</returns>
            public static int GetNextId(Symbol sort)
            {
                if (sort == null) throw new ArgumentNullException("sort");

                int newId = PeekNextId(sort);
                InterpretationContext.GetCurrentContext().ResetId(sort, newId);
                return newId;
            }

            /// <summary>
            /// Return the next available instance counter for this type and sort (without changing state).
            /// </summary>
            /// <note>This is used by parameter generation when a new instance may appear as an action argument.</note>
            /// <returns>The incremented count</returns>
            public static int PeekNextId(Symbol sort)
            {
                if (sort == null) throw new ArgumentNullException("sort");


                int newId;
                return (InterpretationContext.GetCurrentContext().IdPool.TryGetValue(sort, out newId)) ? newId + 1 : 1;

            }

            /// <summary>
            /// Peek at what the next term is for the given sort
            /// </summary>
            public static Term PeekNextLabelTerm(Symbol sort)
            {
                return new CompoundTerm(sort, new Literal(PeekNextId(sort)));
            }


            #endregion

            #region Properties

            /// <summary>
            /// Gets the object id
            /// </summary>
            public ObjectId Label { get { return objectId; } }

            #endregion

            /// <summary>
            /// Called internally to distinguish between reserve elements that are part of state and 
            /// those in the background set of values. Not used directly in models.
            /// </summary>
            public override void FinalizeImport()
            {
                InterpretationContext.GetCurrentContext().EnsureId(this.objectId);
            }

            #region Equality, hashing and comparison
            /// <summary>
            /// Equality of labeled instances by object id
            /// </summary>
            public override bool Equals(object/*?*/ obj)
            {
                LabeledInstance other = obj as LabeledInstance;
                if ((object)other == null) return false;
                return Object.Equals(this.objectId, other.objectId);
            }


            /// <summary>
            /// Get the hashcode of the labeled instance
            /// </summary>
            public override int GetHashCode()
            {
                return TypedHash<LabeledInstance>.ComputeHash(objectId.ObjectSort, objectId.Id);
            }

            /// <summary>
            /// Return the string representation of the labeled instance.
            /// </summary>
            public override string ToString()
            {
                return objectId.ObjectSort.ToString() + "(" + objectId.Id.ToString() + ")";
            }

            ///// <summary>
            ///// The operator == for labeled instances is the same as Object.Equals
            ///// </summary>
            //public static bool operator ==(LabeledInstance/*?*/ o1, LabeledInstance/*?*/ o2)
            //{
            //    return Object.Equals(o1, o2);
            //}

            ///// <summary>
            ///// The operator != for labeled instances is the same as !Object.Equals
            ///// </summary>
            //public static bool operator !=(LabeledInstance/*?*/ o1, LabeledInstance/*?*/ o2)
            //{
            //    return !Object.Equals(o1, o2);
            //}

            ///// <summary>
            ///// o1 is less than o2 if o1.CompareTo(o2) == -1
            ///// </summary>
            //public static bool operator <(LabeledInstance/*?*/ o1, LabeledInstance/*?*/ o2)
            ////^ ensures result == 0 ==> Object.Equals(o1, o2);
            //{
            //    return o1 == null ? ((object)o2 != null) : (o1.CompareTo(o2) == -1);
            //}

            ///// <summary>
            ///// o1 is greater than o2 if o2 is less than o1
            ///// </summary>
            //public static bool operator >(LabeledInstance/*?*/ o1, LabeledInstance/*?*/ o2)
            ////^ ensures result == 0 ==> Object.Equals(o1, o2);
            //{
            //    return o1 != null && (o1.CompareTo(o2) == 1);
            //}

            ///// <summary>
            ///// o1 is less than or equal to o2 if o1.CompareTo(o2) is less than 1
            ///// </summary>
            //public static bool operator <=(LabeledInstance/*?*/ o1, LabeledInstance/*?*/ o2)
            ////^ ensures result == 0 ==> Object.Equals(o1, o2);
            //{
            //    return o1 == null || (o1.CompareTo(o2) < 1);
            //}

            ///// <summary>
            ///// o1 is greater than or equal to o2 if o2 is less than or equal to o1
            ///// </summary>
            //public static bool operator >=(LabeledInstance/*?*/ o1, LabeledInstance/*?*/ o2)
            ////^ ensures result == 0 ==> Object.Equals(o1, o2);
            //{
            //    return o1 == null ? ((object)o2 == null) : (o1.CompareTo(o2) > -1);
            //}

            /// <summary>
            /// Compares this labeled instance to a given object (null ius the minimal element)
            /// Throws an ArgumentException if the argument is not null and not a labaled instance 
            /// </summary>
            public override int CompareTo(object/*?*/ obj)
            //^ requires obj != null ==> obj is IComparable;
            //^ ensures result == 0 <==> Object.Equals(this, obj);
            //^ ensures result == -1 || result == 0 || result == 1;
            {
                if (obj == null) return 1;    // null is the minimal element   
                LabeledInstance other = obj as LabeledInstance;
                if ((object)other == null)
                    throw new ArgumentException(MessageStrings.LocalizedFormat(MessageStrings.LabeledInstanceRequired, obj.GetType().ToString()));
                return this.objectId.CompareTo(other.objectId);
            }
            #endregion

        }
    }

    /// <summary>
    /// Base class whose derived classes have abstract object ids.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LabeledInstance<T> : LabeledInstance where T : LabeledInstance, new()
    {
        /// <summary>
        /// The sort (abstract type) of T. Sorts are used to match types across model programs.
        /// </summary>
        /// <returns>The sort associated with type T</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static Symbol GetSort() 
        { 
            return TypeSort(typeof(T)); 
        }

        /// <summary>
        /// Static constructor (to replace creation by operator "new") of labeled instances.
        /// Allocates a new object (or reuses an existing element from a per-sort pool if available).
        /// </summary>
        /// <returns>A new labeled instance of type T</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static T Create()
        {
            int i = GetNextId(GetSort());
            return ImportElement(i);
        }

        /// <summary>
        /// Object creation "lookahead". Returns the next object that will be
        /// returned by the <see cref="Create"/> method in the current state.
        /// This method does not change the current state of the model program.
        /// </summary>
        /// <note>
        /// Used by parameter generation when a action parameter takes a new instance</note>
        /// <returns>The next instance of type T</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static T PeekNext()
        {
            return ImportElement(PeekNextId(GetSort()));
        }

        /// <summary>
        /// Imports the ith value of this type but does not update the domain map for this type.
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns>The <c>LabeledInstance</c> that represents the ith value of the type</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow")]
        public static T ImportElement(int i) 
        {
           Symbol sort = GetSort();
           ObjectId id = new ObjectId(sort, i);
           LabeledInstance/*?*/ val;
           if (InterpretationContext.GetCurrentContext().InstancePoolTryGetValue(id, out val))
           {
               return (T)val;
           }
           else
           {
               // cleverness alert: We import an element from the reserve without changing state 
               // (represented by the idPool of the current context). To do this we allow a state
               // change and then immediately undo it.
               Map<Symbol, int> oldIdPool = InterpretationContext.GetCurrentContext().IdPool;
               InterpretationContext.GetCurrentContext().ResetId(sort, i - 1);
               T val2 = new T();
               InterpretationContext.GetCurrentContext().IdPool = oldIdPool;
               InterpretationContext.GetCurrentContext().RegisterValue(val2);
               return val2;
           }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public LabeledInstance() : base(new ObjectId(GetSort(), GetNextId(GetSort())))  
        {        
        }
    }
}
