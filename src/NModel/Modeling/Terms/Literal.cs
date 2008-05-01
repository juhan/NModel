//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Internals;

namespace NModel.Terms
{
    /// <summary>
    /// A syntactic unit that denotes a .NET literal (string, int, double, bool, etc.)
    /// </summary>
    [Serializable]
    public sealed class Literal : Term
    {
        object/*?*/ value;

        /// <summary>
        /// Constructs a Literal term from a bool
        /// </summary>
        /// <param name="value">The .NET literal</param>
        public Literal(bool value)
        {
            this.value = value;
        }

        /// <summary>
        /// Constructs a Literal term from a byte
        /// </summary>
        /// <param name="value">The .NET literal</param>
        public Literal(byte value)
        {
            this.value = value;
        }

        /// <summary>
        /// Constructs a Literal term from a char
        /// </summary>
        /// <param name="value">The .NET literal</param>
        public Literal(char value)
        {
            this.value = value;
        }

        /// <summary>
        /// Constructs a Literal term from a double
        /// </summary>
        /// <param name="value">The .NET literal</param>
        public Literal(double value)
        {
            this.value = value;
        }

        /// <summary>
        /// Constructs a Literal term from a float
        /// </summary>
        /// <param name="value">The .NET literal</param>
        public Literal(float value)
        {
            this.value = value;
        }

        /// <summary>
        /// Constructs a Literal term from an int
        /// </summary>
        /// <param name="value">The .NET literal</param>
        public Literal(int value)
        {
            this.value = value;
        }

        /// <summary>
        /// Constructs a Literal term from a long
        /// </summary>
        /// <param name="value">The .NET literal</param>
        public Literal(long value)
        {
            this.value = value;
        }

        /// <summary>
        /// Constructs a Literal term from a short
        /// </summary>
        /// <param name="value">The .NET literal</param>
        public Literal(short value)
        {
            this.value = value;
        }

        /// <summary>
        /// Constructs a Literal term from a string
        /// </summary>
        /// <param name="value">The .NET literal</param>
        public Literal(string value)
        {
            this.value = value;
        }

        /// <summary>
        /// Constructs a Literal term from a ushort
        /// </summary>
        /// <param name="value">The .NET literal</param>
        public Literal(Enum value)
        {
            this.value = value;
        }

        /// <summary>
        /// Constructs a Literal term from a built-in type.
        /// </summary>
        /// <param name="value">The .NET literal</param>
        internal Literal(IComparable value)
        {
            this.value = value;
        }

        /// <summary>
        /// The literal value (in terms of a .NET type).
        /// </summary>
        public IComparable/*?*/ Value
        {
            get { return (IComparable)value; }
        }

        /// <summary>
        /// The function symbol. For term f(1, 2), f is the function symbol.
        /// </summary>
        public override IComparable FunctionSymbol { get { return (IComparable)value; } }

        /// <summary>
        /// 
        /// </summary>
        public override Sequence<Term> Arguments { get { return Sequence<Term>.EmptySequence; } }

        /// <summary>
        /// True if term consists only of concrete terms, false otherwise (that is, when the
        /// term contains one or more logic variables)
        /// </summary>
        public override bool IsGround { get { return true; } }

        /// <summary>
        /// The print representation of the Literal
        /// </summary>
        /// <returns>The literal value</returns>
        public override string ToString()
        {
            if (value == null)
                return "null";
            else if (value is string)
                return QuotedString(value.ToString());
            else if (value is bool)
                return (bool)value ? "true" : "false";
            else if (value is int)
                return value.ToString();
            else
            {
                Type t = value.GetType();
                string symName;
                if (AbstractValue.GetLiteralTypes().TryGetValue(t, out symName))
                    return symName + "(" + QuotedString(value.ToString()) + ")";
                else if (value is System.Enum)
                    return "Enum(" + QuotedString(value.ToString()) + ")"; 
                else
                    return value.ToString();
            }            
        }

        /// <summary>
        /// The compact print representation of a literal is the same as ToString() of the literal
        /// </summary>
        public override string ToCompactString()
        {
            return ToString();
        }

        // bug: not enough, need to escape internal double quotes
        // to do: swap out with .NET string quoting lib function
        static string QuotedString(string s)
        {
            return "\"" + s + "\"";
        }
    }
}
