//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Terms;
using NModel.Internals;
using NModel.Attributes;

namespace NModel.Internals
{
    /// <summary>
    /// An abstract value can be represented by a term.
    /// </summary>
    /// <note>This interface is implemented by all classes and structures that are part of model state or
    /// are used as parameters to model actions. Built-in value types provided by .NET (such as <c>int</c>,
    /// <c>bool</c>, etc. and the .NET <c>string</c> type may also be used in model state and as arguments 
    /// to model action.</note>
    public interface IAbstractValue : IComparable
    {
        /// <summary>
        /// Returns the term representation of this value.
        /// </summary>
        Term AsTerm { get; }

        /// <summary>
        /// Determines if this value has an object id (that is, is of type <see cref="LabeledInstance"/>),
        /// or has a subvalue that has an object id (for example, a set of instances).
        /// </summary>
        /// <returns>True if this value has an object id or contains a value with an object id.</returns>
        bool ContainsObjectIds();

        /// <summary>
        /// Called internally to distinguish between reserve elements that are part of state and 
        /// those in the background set of values. Not used directly in models.
        /// </summary>
        void FinalizeImport();
    }

    /// <summary>
    /// Base class for functionality shared by <see cref="CompoundValue"/> and <see cref="LabeledInstance"/>.
    /// This class also contains static methods for converting from .NET values to terms.
    /// Terms may be converted back to .NET values using <see cref="Internals.InterpretationContext.InterpretTerm"/>.
    /// </summary>
    [Serializable]
    abstract public class AbstractValue : IAbstractValue 
    {
        /// <summary>
        /// Cache of type -> sort mapping. Sorts are the abstract types used to connect model programs.
        /// </summary>
        static Dictionary<Type, Symbol> typeSorts = new Dictionary<Type,Symbol>();

        /// <summary>
        /// Predicate that can be used to test whether a value can be used
        /// as part of model state or as a parameter value in a model action. 
        /// </summary>
        /// <param name="value">The value to be tested.</param>
        /// <returns>True if the value is one of the .NET built-in value types (int, bool), 
        /// a string, an enum value or is a value of a type that provides the <c>IAbstractValue</c>
        /// interface. False otherwise.</returns>
        public static bool IsAbstractValue(object value)
        {
            return (null == value) || (IsAbstractValueType(value.GetType()));
        }

        /// <summary>
        /// Predicate that can be used to test whether a value of type <paramref name="t"/> can be used
        /// as part of model state or as a parameter value in a model action. 
        /// </summary>
        /// <param name="t">The type to be tested</param>
        /// <returns>True if <paramref name="t"/> is one of the .NET built-in value types (int, bool, etc.), 
        /// the string type, an enum or a type that provides the <c>IAbstractValue</c>
        /// interface. False otherwise.</returns>
        public static bool IsAbstractValueType(Type t)
        {
            return
                // Case 1: Literal value type (int, string, bool, etc.)
                (GetLiteralTypes().ContainsKey(t))

                // Case 2: enum value type
                || t.IsEnum 

                // Case 3: subtype of IAbstractValue
                || typeof(IAbstractValue).IsAssignableFrom(t);
        }

        /// <summary>
        /// Checks whether a type represents abstract objects in the model program.
        /// There are currently two ways to make a type to represent abstract objects:
        /// <list type="">
        /// <item>Make a type derive from <see cref="LabeledInstance"/>;</item>
        /// <item>Attribute a <see cref="CompoundValue"/> or an enumeration with [Abstract] attribute.</item>
        /// </list>
        /// </summary>
        /// <param name="t">The type</param>
        /// <returns>true if the type represents abstract objects in the model program.</returns>
        public static bool IsTypeAbstractSort(Type t) {
            if (t == null)
                throw new ArgumentNullException("t");
            //Check if the sort is a subtype of LabeledInstance
            if (t.IsSubclassOf(typeof(LabeledInstance)))
            {
                return true;
            }
            //Check if the sort has AbstractAttribute defined
            foreach (Attribute attr in (t.GetCustomAttributes(true)))
            {
                if (attr.GetType() == typeof(AbstractAttribute))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the sort (abstract type) of this value
        /// </summary>
        public Symbol Sort
        {
            get
            {
                return TypeSort(this.GetType());
            }                
        }

        static Term nullSymbol = new Literal((string)null);
        static Symbol quoteSymbol = new Symbol("Quote");

        /// <summary>
        /// Produces the term representation for <paramref name="value"/>. See <see cref="Term"/>.
        /// </summary>
        /// <param name="value">The value to be represented as a term, either a .NET built-in structure type
        /// or a type that implements <see cref="IAbstractValue"/>.</param>
        /// <returns>The term that represents <paramref name="value"/></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static Term GetTerm(IComparable/*?*/ value)
        {   
            // Case 1: null value
            if (value == null)
                return nullSymbol;
            
            // Case 2: Literal value (int, string, bool, etc.)
            Type t = value.GetType();
            if (GetLiteralTypes().ContainsKey(t))
                return new Literal(value);

            // Case 3: enum value-- encode as sort applied to string arg
            else if (value is System.Enum)
            {
                Symbol sort = TypeSort(value.GetType());

                // to do: also handle Flags attribute for enums by encoding value as a set of strings
                string label = value.ToString();
                return new CompoundTerm(sort, new Literal(label));
            }

            // Case 4: Term value-- return quoted term
            else if (value is Term)
            {
                return new CompoundTerm(quoteSymbol, (Term)value);
            }

            // Case 5: value is of type IAbstractValue-- invoke property to get term
            else
            {
                IAbstractValue av = value as IAbstractValue;
                if (av != null)
                    return av.AsTerm;
                else
                {
                    // Case 6: fail
                    throw new ArgumentException("AbstractValue.GetTerm(): No term can be produced for value " + value.ToString() + ". Type must implement IAbstractValue interface.");
                }
            }
        }

        /// <summary>
        /// Returns the interpretation of the term <paramref name="t"/> in the current context.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IComparable InterpretTerm(Term t)
        {
            return InterpretationContext.GetCurrentContext().InterpretTerm(t);
        }

        /// <summary>
        /// For a given type, returns the associated sort (abstract type). This is calculated 
        /// by using the value of the "Sort" attribute, or by constructing a symbol from
        /// the type name if the attribute is not present. See <see cref="SortAttribute"/>.
        /// </summary>
        /// <param name="t">The type</param>
        /// <returns>The associated sort</returns>
        public static Symbol TypeSort(Type t)
        {
            Symbol sort;

            // Case 1: use cached sort for this type
            if (typeSorts.TryGetValue(t, out sort))
                return sort;

            string sortName;

            // Case 2: if instantiated generic type, use sort name from generic type instantiation 
            //         and recurse to fill in domain parameters
            if (t.IsGenericType && !t.IsGenericTypeDefinition)
            {
                Symbol genericSort = TypeSort(t.GetGenericTypeDefinition());
                Sequence<Symbol> domainParameters = Sequence<Symbol>.EmptySequence;
                foreach (Type typeArg in t.GetGenericArguments())
                    domainParameters = domainParameters.AddLast(TypeSort(typeArg));

                sort = new Symbol(genericSort.Name, domainParameters);
            }

            // Case 3: Build new sort by looking up "sort" attribute.
            //         This is either a nongeneric type or a generic type definition (uninstantiated)
            else
            {
                string sortAttrName = GetSortAttributeString(t);
                if (!String.IsNullOrEmpty(sortAttrName))
                {
                    // use name given by "Sort" attribute
                    sortName = sortAttrName;
                }
                else
                {
                    // else use name of type (taking care to remove extra markings for generics)
                    StringBuilder sb = new StringBuilder();
                    PrettyPrinter.FormatTypeName(sb, t);
                    sortName = sb.ToString();
                }

                // if generic type definition, tag symbol with empty "<>"
                if (t.IsGenericType && t.IsGenericTypeDefinition)
                {
                    sort = new Symbol(sortName, Sequence<Symbol>.EmptySequence);
                }
                else
                {
                    sort = new Symbol(sortName);
                }
            }
            typeSorts[t] = sort;
            return sort;
        }

        static string/*?*/ GetSortAttributeString(Type t)
        {
            object/*?*/[]/*?*/ attrs = t.GetCustomAttributes(typeof(SortAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                SortAttribute attr = (SortAttribute)attrs[0];
                return attr.Name;
            }
            return null;
        }

        static Dictionary<Type, string> literalTypes;
        static Dictionary<string, Type> literalTypeNameStrings;

        /// <summary>
        /// Returns a dictionary of .NET types that can be converted into literal terms (see <see cref="Terms.Literal"/>).
        /// The key of the dictionary is the type, the values are the names of the associated sorts.
        /// </summary>
        /// <returns>The dictionary of sorts</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static Dictionary<Type, string> GetLiteralTypes()
        {
            if (literalTypes == null)
                InitializeBuiltInStructureTypeStrings();

            return literalTypes;
        }

        static void InitializeBuiltInStructureTypeStrings()
        {
            Type[] types = new Type[]{ typeof(bool), typeof(byte), typeof(char), typeof(double), 
                                       typeof(float), typeof(int), typeof(long), typeof(sbyte),
                                       typeof(short), typeof(string), typeof(uint), typeof(ulong),
                                       typeof(ushort)};

            string[] shortNames = new string[]{ "bool", "byte", "char", "double", 
                                           "float", "int", "long", "sbyte",
                                           "short", "string", "uint", "ulong",
                                           "ushort"};

            int nTypes = types.Length;
            // string[] typeNames = new string[nTypes];

            Dictionary<Type, string> table1 = new Dictionary<Type, string>(nTypes);
            Dictionary<string, Type> table2 = new Dictionary<string, Type>(nTypes);
            for (int i = 0; i < types.Length; i += 1)
            {
                table1.Add(types[i], shortNames[i]);
                table2.Add(shortNames[i], types[i]);        
            }

            literalTypes = table1;
            literalTypeNameStrings = table2;
        }

        /// <summary>
        /// Does the sort denote a literal type? See <see cref="GetLiteralTypes()"/>.
        /// </summary>
        /// <param name="sort">The sort</param>
        /// <returns>True if the sort denotes a literal type. False otherwise.</returns>
        public static bool IsLiteralSort(Symbol sort)
        {
            return GetLiteralTypes().ContainsValue(sort.Name);
        }

        /// <summary>
        /// Returns the .NET type for <paramref name="sort"/>. 
        /// </summary>
        /// <param name="sort">The sort. </param>
        /// <returns>The associated type.</returns>
        public static Type GetLiteralSortType(Symbol sort)
        {
            GetLiteralTypes();
            Type result;
            if (!literalTypeNameStrings.TryGetValue(sort.Name, out result))
                throw new ArgumentException("Sort " + sort.Name + " does not represent a literal type.");
            return result;
        }

        /// <summary>
        /// Called internally to bring a background element into the current state
        /// </summary>
        /// <param name="obj">The element in import</param>
        public static void FinalizeImport(IComparable obj)
        {
            IAbstractValue val = obj as IAbstractValue;
            if (null != val)
            {
                val.FinalizeImport();
            }
        }

        /// <summary>
        /// Determines if this value has an object id (that is, is of type <see cref="LabeledInstance"/>),
        /// or has a subvalue that has an object id (for example, a set of instances).
        /// </summary>
        /// <returns>True if this value has an object id or contains a value with an object id.</returns>
        /// <remarks> This method is invoked by the modeling library when determining whether two states are isomorphic.
        /// </remarks>
        public static bool ContainsObjectIds(IComparable obj)
        {
            if (null == obj) return false;
            Type t = obj.GetType();
            if (GetLiteralTypes().ContainsKey(t) || t.IsEnum) return false;
            IAbstractValue av = obj as IAbstractValue;
            if (null == av) return false;  // maybe throw exception?
            else return av.ContainsObjectIds();
        }
        
        #region IAbstractValue Members

        /// <summary>
        /// Returns the term representation of this value.
        /// </summary>
        public abstract Term AsTerm { get; }

        /// <summary>
        /// Determines if this value has an object id (that is, is of type <see cref="LabeledInstance"/>),
        /// or has a subvalue that has an object id (for example, a set of instances).
        /// </summary>
        /// <returns>True if this value has an object id or contains a value with an object id.</returns>
        public abstract bool ContainsObjectIds();

        /// <summary>
        /// Called internally to distinguish between reserve elements that are part of state and 
        /// those in the background set of values. Not used directly in models.
        /// </summary>
        public abstract void FinalizeImport();       

        #endregion

        #region IComparable Members

        /// <exclude />
        public abstract int CompareTo(object obj);

        #endregion

        /// <exclude />
        public abstract override bool Equals(object obj);

        /// <exclude />
        public abstract override int GetHashCode();


        #region comparison operators
        /// <summary>
        /// The operator == for abstract values is the same as Object.Equals
        /// </summary>
        public static bool operator ==(AbstractValue o1, AbstractValue o2)
        {
            return Object.Equals(o1, o2);
        }

        /// <summary>
        /// The operator != for abstract values is the same as !Object.Equals
        /// </summary>
        public static bool operator !=(AbstractValue o1, AbstractValue o2)
        {
            return !Object.Equals(o1, o2);
        }

        /// <summary>
        /// o1 is less than o2 iff either o1 is null and o2 is not null or else o1.CompareTo(o2) == -1
        /// </summary>
        public static bool operator <(AbstractValue o1, AbstractValue o2)
        {
            return o1 == null ? ((object)o2 != null) : (o1.CompareTo(o2) == -1);
        }

        /// <summary>
        /// o1 is greater than o2 iff o2 is less than o1 
        /// </summary>
        public static bool operator >(AbstractValue o1, AbstractValue o2)
        {
            return o2 == null ? ((object)o1 != null) : (o2.CompareTo(o1) == -1);
        }

        /// <summary>
        /// o1 is less than or equal to o2 if o1.CompareTo(o2) is less than 1
        /// </summary>
        public static bool operator <=(AbstractValue/*?*/ o1, AbstractValue/*?*/ o2)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return o1 == null || (o1.CompareTo(o2) < 1);
        }

        /// <summary>
        /// o1 is greater than or equal to o2 if o2 is less than or equal to o1
        /// </summary>
        public static bool operator >=(AbstractValue/*?*/ o1, AbstractValue/*?*/ o2)
        //^ ensures result == 0 ==> Object.Equals(o1, o2);
        {
            return o1 == null ? ((object)o2 == null) : (o1.CompareTo(o2) > -1);
        }

        #endregion
    }
}
