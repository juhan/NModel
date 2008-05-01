//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace NModel.Terms
{
    /// <summary>
    /// A term that denotes a fresh logic variable in every context where it occurs.
    /// 
    /// The Any term is denoted by "_"
    /// </summary>
    [Serializable]
    public sealed class Any : Term
    {
        /// <summary>
        /// A term that denotes a distinct logic variable in each context where it occurs 
        /// </summary>
        readonly static Any v = new Any();

        private Any() { }

        /// <summary>
        /// Constructor for the "Any" term. This term denotes a distinct logic variable 
        /// in each context where it occurs.
        /// </summary>
        public static Any Value
        {
            get { return v; }
        }

        /// <summary>
        /// True if term consists only of concrete terms, false otherwise (that is, when the
        /// term contains one or more logic variables)
        /// </summary>
        public override bool IsGround { get { return false; } }

        /// <summary>
        /// The function symbol. For term f(1, 2), f is the function symbol.
        /// </summary>
        public override IComparable FunctionSymbol { get { return Any.Value; } }

        /// <summary>
        /// Returns an empty sequence of arguments.
        /// </summary>
        public override Sequence<Term> Arguments { get { return Sequence<Term>.EmptySequence; } }

        /// <summary>
        /// Pretty printing
        /// </summary>
        /// <returns>The string "_"</returns>
        public override string ToString()
        {
            return "_";
        }

        /// <summary>
        /// Same as ToString()
        /// </summary>
        public override string ToCompactString()
        {
            return "_";
        }
    }
}
