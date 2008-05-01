//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace NModel.Terms
{
    /// <summary>
    /// A term that denotes a named logic variable
    /// </summary>
    [Serializable]
    public sealed class Variable : Term
    {
        string name;

        /// <summary>
        /// Construct a logical variable as a nonground term
        /// </summary>
        public Variable(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// True if term consists only of concrete terms, false otherwise (that is, when the
        /// term contains one or more logic variables)
        /// </summary>
        public override bool IsGround { get { return false; } }

        /// <summary>
        /// Returns the name of the variable.
        /// </summary>
        public override IComparable FunctionSymbol { get { return name; } }

        /// <summary>
        /// Returns the empty sequence
        /// </summary>
        public override Sequence<Term> Arguments { get { return Sequence<Term>.EmptySequence; } }

        /// <summary>
        /// Pretty printing
        /// </summary>
        /// <returns>The variable name</returns>
        public override string ToString()
        {
            return name;
        }

        /// <summary>
        /// Same as ToString()
        /// </summary>
        public override string ToCompactString()
        {
            return name;
        }


        /// <summary>
        /// Returns the term given by the substitution if this variable 
        /// appears in the substitution, returns this variable otherwise.
        /// </summary>
        public override Term Substitute(Map<Variable, Term> substitution)
        {
            if (substitution.ContainsKey(this))
                return substitution[this];
            else
                return this;
        }
    }

}
