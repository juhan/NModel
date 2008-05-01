//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace NModel.Terms
{

    /// <summary>
    /// Function symbol. This syntactic element is the first entry of a <see cref="CompoundTerm"/>. 
    /// A symbol is not itself a term.
    /// </summary>
    [Serializable]
    public sealed class Symbol : CompoundValue
    {
        readonly string name;

        /// <summary>
        /// If null, indicates a nongeneric type.
        /// If the empty sequence, indicates a generic type definition
        /// If nonempty, indicates an instantiated generic type
        /// </summary>
        [NonSerialized]
        readonly Sequence<Symbol>/*?*/ domainParameters;

        /// <summary>
        /// Returns the name and then the domain parameters
        /// </summary>
        public override IEnumerable<IComparable> FieldValues()
        {
            yield return name;
            yield return domainParameters;
        }

        /// <summary>
        /// Constructs a symbol
        /// </summary>
        /// <param name="name">The symbol name</param>
        public Symbol(string name)
        {
            if ("_".Equals(name))
                throw new ArgumentException("Symbol may not use the reserved token \"_\"");
            // To do: check that name contains no whitespace and is otherwise well formed
            // To do: possibly maintain a symbol table as an optimization (speeds equality checks 
            // but introduces a memory leak.
            this.name = name;
            this.domainParameters = null;
        }

        /// <summary>
        /// Constructs a symbol with given name and domain parameters
        /// </summary>
        public Symbol(string name, params Symbol[] domainParameters)
        {
            if ("_".Equals(name))
                throw new ArgumentException("Symbol may not use the reserved token \"_\"");
            // To do: check that name contains no whitespace and is otherwise well formed
            // To do: possibly maintain a symbol table as an optimization (speeds equality checks 
            // but introduces a memory leak.
            this.name = name;
            this.domainParameters = new Sequence<Symbol>(domainParameters);
        }


        /// <summary>
        /// Constructs a symbol with given name and domain parameters
        /// </summary>
        public Symbol(string name, Sequence<Symbol> domainParameters)
        {
            if ("_".Equals(name))
                throw new ArgumentException("Symbol may not use the reserved token \"_\"");
            // To do: check that name contains no whitespace and is otherwise well formed
            // To do: possibly maintain a symbol table as an optimization (speeds equality checks 
            // but introduces a memory leak.
            this.name = name;
            this.domainParameters = domainParameters;
        }

        /// <summary>
        /// Parses a symbol from the given string
        /// </summary>
        public static Symbol Parse(string representation)
        {
            if (representation == null)
                throw new ArgumentNullException("representation");
            try
            {
                Term term = Term.Parse(representation + "()");
                CompoundTerm ct = (CompoundTerm)term;
                return ct.Symbol;
            }
            catch (ArgumentException)
            {
                throw;
            }
        }
        /// <summary>
        /// The string name of the "name" component of the symbol
        /// </summary>
        public string Name
        {
             get { return name; }
        }

        /// <summary>
        /// The string name of the "name" component of the symbol.
        /// </summary>
        public string ShortName
        {
            get { return name; }
        }

        /// <summary>
        /// The domain parameters component of the symbol. These are used to give generic sorts.
        /// </summary>
        public Sequence<Symbol> DomainParameters
        {
            get { return domainParameters; }
        }

        /// <summary>
        /// The string name of the symbol. This is includes both the "name" component and the "suffix"
        /// </summary>
        public string FullName
        {
            get { 
                
                StringBuilder sb = new StringBuilder();
                PrettyPrint(sb);
                return sb.ToString();           
            }
        }


        void PrettyPrint(StringBuilder sb)
        {
            sb.Append(name);
            if (this.domainParameters != null)
            {
                sb.Append("<");
                bool isFirst = true;
                foreach (Symbol symValue in this.domainParameters)
                    {
                        if (!isFirst) sb.Append(", ");
                        symValue.PrettyPrint(sb);
                        isFirst = false;
                    }
                sb.Append(">");
            }
        }

        /// <summary>
        /// Pretty printing
        /// </summary>
        /// <returns>Prints the symbol name</returns>
        public override string ToString()
        {
            return FullName;
        }
    }
}
