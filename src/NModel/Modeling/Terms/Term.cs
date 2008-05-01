//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace NModel.Terms
{
    /// <summary>
    /// A syntactic entity
    /// </summary>
    [Serializable]
    public abstract class Term : CompoundValue
    {
        /// <summary>
        /// True if term consists only of concrete terms, false otherwise (that is, when the
        /// term contains one or more logic variables)
        /// </summary>
        public abstract bool IsGround { get; }

        /// <summary>
        /// The function symbol. For term f(1, 2), f is the function symbol.
        /// </summary>
        public abstract IComparable FunctionSymbol { get; }

        /// <summary>
        /// A sequence of the arguments of this term. For term f(1, 2), 1 and 2 are the arguments.
        /// </summary>
        public abstract Sequence<Term> Arguments { get; }

        /// <summary>
        /// Pretty printing of terms with function symbols that do not indidate underlying .Net type
        /// Used for viewing in State Viewer.
        /// </summary>
        public abstract string ToCompactString();

        /// <summary>
        /// Reads a string like "f(1, "abc", File(1))" and returns the corresponding abstract syntax tree. 
        /// </summary>
        /// <param name="representation">The text representation of the term</param>
        /// <returns>The abstract syntax tree representation of the term</returns>
        public static Term Parse(string representation)
        {
            return TermReader.Read(representation);
        }

        /// <summary>
        /// Replace all the variables in this term with terms given by the substitution
        /// </summary>
        public virtual Term Substitute(Map<Variable, Term> substitution)
        {
            return this;
        }
    }
}
