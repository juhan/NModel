//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NModel.Terms;
using NModel.Internals;



namespace NModel
{
    /// <summary>
    /// Data record with structural equality. 
    /// </summary>
    /// <remarks><para>A <c>CompoundValue</c> is similar to a .NET <c>struct</c>,
    /// but unlike a <c>struct</c> subtypes of <c>CompoundValue</c> may implement tree structures. By construction, such 
    /// values may be recursive but must have no cycles. (From the point of view of mathematical logic,
    /// values of this type can be thought of as a <i>term</i> whose <i>function symbol</i> is 
    /// the type and whose <i>arguments</i> are the field values.)</para>
    /// <para><c>CompoundValues</c> are commonly used in modeling abstract state. They should be used instead of
    /// mutable classes whenever practical because they can greatly reduce the amount of analysis required
    /// when comparing if two program states are equal. They also tend to promote a clear style, since
    /// they are read-only data structures. Aliased updates can thus be avoided.</para>
    /// <para>
    /// Invariant: All field values must have a fixed order and contain elements of type <see cref="IComparable"/>
    /// that satisfy the predicate <see cref="AbstractValue.IsAbstractValue"/>. All fields of subtypes of this 
    /// class must be readonly.</para>
    /// </remarks>
    /// <example>
    /// <para>Here is an example of how to define a data record with structural equality using <c>Compoundvalue</c>:</para>
    /// <code>
    /// public class MyPoint : CompoundValue
    /// {
    ///    public readonly int x;
    ///    public readonly int y;
    /// 
    ///    public MyPoint(int x, int y) { this.x = x; this.y = y; }
    /// }
    /// </code>
    /// <para>You can improve the runtime performance of a user-defined subtype of <c>CompoundValue</c> by overriding
    /// the default <see cref="CompoundValue.FieldValues"/> method. This is optional.</para>
    /// <code>
    /// public class MyPoint : CompoundValue
    /// {
    ///    public readonly int x;
    ///    public readonly int y;
    /// 
    ///    public MyPoint(int x, int y) { this.x = x; this.y = y; }
    /// 
    ///    public override IEnumerable&lt;IComparable&gt; FieldValues()
    ///    {
    ///         yield return this.x;
    ///         yield return this.y;
    ///    }
    /// }
    /// </code>
    /// </example>
    [Serializable]
    abstract public class CompoundValue : AbstractValue //, IComparable
    {
        /// <summary>
        /// Structural equality testing. Objects are equal if they are of the same type
        /// and if each field (given by the enumeration <see cref="FieldValues" />) is equal
        /// for the two values.
        /// </summary>
        /// <param name="obj">The value to compare to this value</param>
        /// <returns>True if the values are equal, false otherwise.</returns>
        public override bool Equals(object/*?*/ obj)
        {
            CompoundValue other = obj as CompoundValue;
            if ((object)other == null) return false;
            if ((object)this == obj) return true;
            if (this.GetType() != obj.GetType()) return false;

            IEnumerator<IComparable> fields1 = this.FieldValues().GetEnumerator();
            IEnumerator<IComparable> fields2 = other.FieldValues().GetEnumerator();
            while (true)
            {
                bool hasNext1 = fields1.MoveNext();
                bool hasNext2 = fields2.MoveNext();
                if (hasNext1 ^ hasNext2) return false; // differing lengths
                if (!hasNext1) return true;
                if (!Object.Equals(fields1.Current, fields2.Current)) return false;
            }
        }

        /// <summary>
        /// Term equality
        /// </summary>
        /// <param name="o1">The first value</param>
        /// <param name="o2">The second value</param>
        /// <returns>True if <paramref name="o1"/> equals <paramref name="o2"/> under
        /// term ordering and false otherwise.</returns>
        public static bool operator ==(CompoundValue/*?*/ o1, CompoundValue o2/*?*/)
        {
            return Object.Equals(o1, o2);
        }

        /// <summary>
        /// Inequality of terms
        /// </summary>
        /// <param name="o1">The first value</param>
        /// <param name="o2">The second value</param>
        /// <returns>False if <paramref name="o1"/> equals <paramref name="o2"/> under
        /// term ordering and true otherwise.</returns>
        public static bool operator !=(CompoundValue o1/*?*/, CompoundValue o2/*?*/)
        {
            return !Object.Equals(o1, o2);
        }

        /// <summary>
        /// Term less than
        /// </summary>
        /// <param name="o1">The first value</param>
        /// <param name="o2">The second value</param>
        /// <returns>True if <paramref name="o1"/> is less than <paramref name="o2"/> 
        /// under term ordering and false otherwise.</returns>
        public static bool operator <(CompoundValue o1/*?*/, CompoundValue o2/*?*/)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return o1 == null ? ((object)o2 != null) : (o1.CompareTo(o2) == -1);
        }

        /// <summary>
        /// Term less than or equal
        /// </summary>
        /// <param name="o1">The first value</param>
        /// <param name="o2">The second value</param>
        /// <returns>True if <paramref name="o1"/> is less than or equal <paramref name="o2"/> 
        /// under term ordering and false otherwise.</returns>
        public static bool operator <=(CompoundValue o1/*?*/, CompoundValue o2/*?*/)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return o1 == null ? true : (o1.CompareTo(o2) < 1);
        }

        /// <summary>
        /// Term greater than
        /// </summary>
        /// <param name="o1">The first value</param>
        /// <param name="o2">The second value</param>
        /// <returns>True if <paramref name="o1"/> is greater than <paramref name="o2"/> 
        /// under term ordering and false otherwise.</returns>
        public static bool operator >(CompoundValue o1/*?*/, CompoundValue o2/*?*/)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return o1 == null ? false : (o1.CompareTo(o2) == 1);
        }

        /// <summary>
        /// Term greater than or equal
        /// </summary>
        /// <param name="o1">The first value</param>
        /// <param name="o2">The second value</param>
        /// <returns>True if <paramref name="o1"/> is greater than or equal <paramref name="o2"/> 
        /// under term ordering and false otherwise.</returns>
        public static bool operator >=(CompoundValue o1/*?*/, CompoundValue o2/*?*/)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return o1 == null ? ((object)o2 == null) : (o1.CompareTo(o2) > -1);
        }

        /// <summary>
        /// Hash code (computed structurally) by combining the hash codes of each field value
        /// given by <see cref="FieldValues" />.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return HashAlgorithms.ComputeEnumeratedHash(this.GetType().ToString().GetHashCode(), FieldValues().GetEnumerator());
        }

        /// <summary>
        /// Pretty printing
        /// </summary>
        /// <returns>A formatted string showing the type and fields of this value</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(this.Sort.Name);
            // PrettyPrinter.FormatType(sb, this.GetType());
            sb.Append("(");
            bool isFirst = true;
            foreach (object obj in this.FieldValues())
            {
                if (!isFirst) sb.Append(", ");
                PrettyPrinter.Format(sb, obj);
                isFirst = false;
            }
            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// Returns an enumeration of all (readonly) field values of this compound value in a fixed order.
        /// </summary>
        /// <returns>An enumeration of the field values of this compound value.</returns>
        /// <remarks>
        /// This method is not normally called directly within a model. Instead, it is invoked internally by the 
        /// modeling library as part of <see cref="CompoundValue.Equals"/> and <see cref="CompoundValue.GetHashCode"/>.
        /// <para>If you define your own data record types using the <c>CompoundValue</c> class you may
        /// want to consider overriding <see cref="CompoundValue.FieldValues"/> to improve performance.</para>
        /// <note>Invariants: This is a pure function and has the further requirement that
        /// the values of any two invocations must always be pairwise equal, regardless of the context in 
        /// which the methods is invoked. In other words, no state update may change the values returned by this
        /// enumeration, and the order of enumeration must be fixed.</note>
        /// </remarks>
        /// <example>
        /// <para>Here is an example of how to override <c>FieldValues</c> from within a derived class.</para>
        /// <code>
        /// public class MyPoint : CompoundValue
        /// {
        ///    public readonly int x;
        ///    public readonly int y;
        /// 
        ///    public MyPoint(int x, int y) { this.x = x; this.y = y; }
        /// 
        ///    public override IEnumerable&lt;IComparable&gt; FieldValues()
        ///    {
        ///         yield return this.x;
        ///         yield return this.y;
        ///    }
        /// }
        /// </code>
        /// </example>
        public virtual IEnumerable<IComparable> FieldValues()
        //^ ensures Forall(IEnumerable obj in result; AbstractValue.IsAbstractValue(obj));
        {
            foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                yield return (IComparable) field.GetValue(this);
        }

        /// <summary>
        /// Returns the term representation of this value.
        /// </summary>
        public override Term AsTerm
        {
            get
            {
                Symbol sort = this.Sort;
                Sequence<Term> arguments = Sequence<Term>.EmptySequence;
                foreach (IComparable val in this.FieldValues())
                    arguments = arguments.AddLast(AbstractValue.GetTerm(val));
                return new CompoundTerm(sort, arguments);
            }
        }

        /// <summary>
        /// Called internally to distinguish between reserve elements that are part of state and 
        /// those in the background set of values. Not used directly in models.
        /// </summary>
        public override void FinalizeImport()
        {
            foreach(IComparable obj in this.FieldValues())
                AbstractValue.FinalizeImport(obj);
        }

        /// <summary>
        /// Determines if this value has an object id (that is, is of type <see cref="LabeledInstance"/>),
        /// or has a subvalue that has an object id (for example, a set of instances).
        /// </summary>
        /// <returns>True if this value has an object id or contains a value with an object id.</returns>
        public override bool ContainsObjectIds() 
        {
            foreach (IComparable obj in this.FieldValues())
                if (AbstractValue.ContainsObjectIds(obj))
                    return true;
            return false;
        }

        #region IComparable Members

        /// <summary>
        /// Term order. Comparision is based on type and recursively on fields.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>-1 if less than, 0 if equal, 1 if greater than</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="obj"/> is nonnull but is not an <c>CompoundValue</c>.</exception>
        public override int CompareTo(object/*?*/ obj)
        //^ requires obj != null ==> obj is IComparable;
        //^ ensures result == 0 <==> Object.Equals(this, obj);
        //^ ensures result == -1 || result == 0 || result == 1;
        {
            // Case 1: other obj is null
            if (obj == null) return 1;    // nonnull is bigger than null


            CompoundValue other = obj as CompoundValue; 
            if ((object)other == null)
                throw new ArgumentException(MessageStrings.LocalizedFormat(MessageStrings.CompoundValueRequired, obj.GetType().ToString()));


            // Case 2: types aren't the same, do type comparison in dictionary order
            Type t1 = this.GetType();
            Type t2 = obj.GetType();
            if (t1 != t2)
                return t1.ToString().CompareTo(t2.ToString()); 

            // Case 3: types are the same, look for first field that is not equal
            IEnumerator<IComparable> fields1 = this.FieldValues().GetEnumerator();
            IEnumerator<IComparable> fields2 = other.FieldValues().GetEnumerator();
            while (true)
            {
                bool hasNext1 = fields1.MoveNext();
                bool hasNext2 = fields2.MoveNext();
                if (hasNext1 & !hasNext2) return 1;  // longer comes after shorter
                if (!hasNext1 & hasNext2) return -1; // shorter comes before longer
                if (!hasNext1) return 0;             // items are equal
                IComparable c1 = fields1.Current as IComparable;
                IComparable c2 = fields2.Current as IComparable;
                if (fields1.Current != null && c1 == null) 
                    throw new ArgumentException(MessageStrings.LocalizedFormat(MessageStrings.ComparableTypeRequired, fields1.Current.GetType().ToString()));
                if (fields2.Current != null && c2 == null) 
                    throw new ArgumentException(MessageStrings.LocalizedFormat(MessageStrings.ComparableTypeRequired, fields2.Current.GetType().ToString()));
                int fieldCompare = HashAlgorithms.CompareValues(c1, c2);
                if (fieldCompare != 0) return fieldCompare;
            }
        }

        #endregion
    }
}
