//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using NModel.Internals;
using NModel.Terms;

namespace NModel
{
    /// <summary>
    /// Binary tuples with structural equality.
    /// </summary>
    public struct Pair<T, S> : IAbstractValue where T : IComparable where S : IComparable       
        
    {
        T first;
        S second;

        /// <summary>
        /// The first value
        /// </summary>
        public T First { get { return first; } set { first = value; } }

        /// <summary>
        /// The second value
        /// </summary>
        public S Second { get { return second; } set { second = value; } }

        /// <summary>
        /// Initializes a new instance of a pair with the given arguments
        /// </summary>
        public Pair(T first, S second) 
        { 
            this.first = first; 
            this.second = second; 
        }

        #region Equality
        /// <summary>
        /// True, if value is a pair and is structurally equal to this, false otherwise [Time: see ==]
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Pair<T, S> && this == (Pair<T, S>)obj;
        }

        /// <exclude />
        public override int GetHashCode()
        {
            return TypedHash<Pair<T, S>>.ComputeHash(first, second);
        }

        /// <summary>
        /// Deep structural equality on Pairs
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool operator ==(Pair<T, S> x, Pair<T, S> y)
        {
            return Object.Equals(x.first, y.first) && Object.Equals(x.second, y.second);
        }

        /// <summary>
        /// Deep structural inequality on Pairs
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool operator !=(Pair<T, S> x, Pair<T, S> y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Term inequality (less than)
        /// </summary>
        /// <param name="o1">The first term</param>
        /// <param name="o2">The second term</param>
        /// <returns>True, if under term ordering, <paramref name="o1"/> is less than <paramref name="o2" />; false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool operator <(Pair<T, S> o1, Pair<T, S> o2)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return (o1.CompareTo(o2) == -1);
        }

        /// <summary>
        /// Term inequality (less than or equals)
        /// </summary>
        /// <param name="o1">The first term</param>
        /// <param name="o2">The second term</param>
        /// <returns>True, if under term ordering, <paramref name="o1"/> is less than <paramref name="o2"/>; false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool operator <=(Pair<T, S> o1, Pair<T, S> o2)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return (o1.CompareTo(o2) < 1);
        }

        /// <summary>
        /// Term greater than
        /// </summary>
        /// <param name="o1">The first value</param>
        /// <param name="o2">The second value</param>
        /// <returns>True if <paramref name="o1"/> is greater than <paramref name="o2"/> 
        /// under term ordering and false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool operator >(Pair<T, S> o1, Pair<T, S> o2)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return (o1.CompareTo(o2) == 1);
        }

        /// <summary>
        /// Term greater than or equal
        /// </summary>
        /// <param name="o1">The first value</param>
        /// <param name="o2">The second value</param>
        /// <returns>True if <paramref name="o1"/> is greater than or equal <paramref name="o2"/> 
        /// under term ordering and false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool operator >=(Pair<T, S> o1, Pair<T, S> o2)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return (o1.CompareTo(o2) > -1);
        }
        #endregion


        /// <summary>
        /// Formats this triple as "Triple&lt;T1,T2&gt;(first, second)"
        /// </summary>
        /// <returns>The formatted string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Pair(");
            PrettyPrinter.Format(sb, this.first);
            sb.Append(", ");
            PrettyPrinter.Format(sb, this.second);         
            sb.Append(")");
            return sb.ToString();
        }

        #region IComparable Members

        /// <summary>
        /// Term order. Comparision is pairwise using the term ordering of <see cref="HashAlgorithms.CompareValues" />.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>-1 if less than, 0 if equal, 1 if greater than</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="obj"/> is not of the same type as this triple.</exception>
        /// <seealso cref="HashAlgorithms.CompareValues" />
        public int CompareTo(object obj)
        {
            if ((object)obj == null) return 1;
            if (!(obj is Pair<T, S>)) throw new ArgumentException("Incomparable argument");
            Pair<T, S> other = (Pair<T,S>)obj;
         
            int f1 = HashAlgorithms.CompareValues(this.first, other.first);
            if (f1 != 0) return f1;
            int f2 = HashAlgorithms.CompareValues(this.second, other.second);
            //^ assert f2 == 0 ==> Object.Equals(this, other);
            return f2;
        }

        #endregion

        #region IAbstractValue Members

        /// <summary>
        /// Returns the term representation of this value.
        /// </summary>
        public Term AsTerm
        {
            get 
            {
                return new CompoundTerm(AbstractValue.TypeSort(this.GetType()),
                                        new Sequence<Term>(AbstractValue.GetTerm(this.first),
                                                           AbstractValue.GetTerm(this.second)));
            }
        }

        /// <summary>
        /// Called internally to distinguish between reserve elements that are part of state and 
        /// those in the background set of values. Not used directly in models.
        /// </summary>
        public void FinalizeImport()
        {
            AbstractValue.FinalizeImport(this.first);
            AbstractValue.FinalizeImport(this.second);
        }

        /// <summary>
        /// Determines if this value has an object id (that is, is of type <see cref="LabeledInstance"/>),
        /// or has a subvalue that has an object id (for example, a set of instances).
        /// </summary>
        /// <returns>True if this value has an object id or contains a value with an object id.</returns>
        public bool ContainsObjectIds()
        {
            return AbstractValue.ContainsObjectIds(this.first) || AbstractValue.ContainsObjectIds(this.second);
        }

        #endregion
    }

}
