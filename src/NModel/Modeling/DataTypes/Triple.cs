//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using NModel.Terms;
using NModel.Internals;

namespace NModel
{
    /// <summary>
    /// Triples with structural equality.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public struct Triple<T, S, R> : IAbstractValue
        where T : IComparable
        where S : IComparable
        where R : IComparable
    {
        T first;
        S second;
        R third;

        /// <summary>
        /// The first element
        /// </summary>
        public T First { get { return first; } set { first = value; } }

        /// <summary>
        /// The second element
        /// </summary>
        public S Second { get { return second; } set { second = value; } }

        /// <summary>
        /// The third element
        /// </summary>
        public R Third { get { return third; } set { third = value; } }

        /// <summary>
        /// Initializes a new instance of a triple with the given arguments
        /// </summary>
        public Triple(T first, S second, R third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }

        #region Equality
        /// <summary>
        /// True, if value is a triple and is structurally equal to this, false otherwise 
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Triple<T, S, R> && this == (Triple<T, S, R>)obj;
        }

        /// <summary>
        /// Hash code (computed structurally)
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return TypedHash<Triple<T, S, R>>.ComputeHash(first, second, third);
        }

        /// <summary>
        /// Structural equality
        /// </summary>
        /// <param name="x">The first value</param>
        /// <param name="y">The second value</param>
        /// <returns>True if <paramref name="x"/> is structurally equivalent to <paramref name="y"/>, false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool operator ==(Triple<T, S, R> x, Triple<T, S, R> y)
        {
            return Object.Equals(x.first, y.first) && Object.Equals(x.second, y.second) && Object.Equals(x.third, y.third);
        }

        /// <summary>
        /// Structural inequality
        /// </summary>
        /// <param name="x">The first value</param>
        /// <param name="y">The second value</param>
        /// <returns>False if <paramref name="x"/> is structurally equivalent to <paramref name="y"/>, true otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool operator !=(Triple<T, S, R> x, Triple<T, S, R> y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Term less than
        /// </summary>
        /// <param name="o1">The first value</param>
        /// <param name="o2">The second value</param>
        /// <returns>True if <paramref name="o1"/> is less than <paramref name="o2"/> 
        /// under term ordering and false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool operator <(Triple<T, S, R> o1, Triple<T, S, R> o2)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return (o1.CompareTo(o2) == -1);
        }

        /// <summary>
        /// Term less than or equal
        /// </summary>
        /// <param name="o1">The first value</param>
        /// <param name="o2">The second value</param>
        /// <returns>True if <paramref name="o1"/> is less than or equal <paramref name="o2"/> 
        /// under term ordering and false otherwise.</returns>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static bool operator <=(Triple<T, S, R> o1, Triple<T, S, R> o2)
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
        public static bool operator >(Triple<T, S, R> o1, Triple<T, S, R> o2)
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
        public static bool operator >=(Triple<T, S, R> o1, Triple<T, S, R> o2)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return (o1.CompareTo(o2) > -1);
        }
        #endregion


        /// <summary>
        /// Formats this triple as "Triple&lt;T1,T2,T2&gt;(first, second, third)"
        /// </summary>
        /// <returns>The formatted string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Triple(");
            PrettyPrinter.Format(sb, this.first);
            sb.Append(", ");
            PrettyPrinter.Format(sb, this.second);
            sb.Append(", ");
            PrettyPrinter.Format(sb, this.third);
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
            if (!(obj is Triple<T, S, R>)) throw new ArgumentException("Incomparable argument");
            Triple<T, S, R> other = (Triple<T, S, R>)obj;

            int f1 = HashAlgorithms.CompareValues(this.first, other.first);
            if (f1 != 0) return f1;
            int f2 = HashAlgorithms.CompareValues(this.second, other.second);
            if (f2 != 0) return f2;
            int f3 = HashAlgorithms.CompareValues(this.third, other.third);
            //^ assert f3 == 0 ==> Object.Equals(this, other);
            return f3;
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
                                                           AbstractValue.GetTerm(this.second),
                                                           AbstractValue.GetTerm(this.third)));
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
            AbstractValue.FinalizeImport(this.third);
        }

        /// <summary>
        /// Determines if this value has an object id (that is, is of type <see cref="LabeledInstance"/>),
        /// or has a subvalue that has an object id (for example, a set of instances).
        /// </summary>
        /// <returns>True if this value has an object id or contains a value with an object id.</returns>
        public bool ContainsObjectIds()
        {
            return AbstractValue.ContainsObjectIds(this.first) || 
                   AbstractValue.ContainsObjectIds(this.second) ||
                   AbstractValue.ContainsObjectIds(this.third);
        }

        #endregion
    }

}
