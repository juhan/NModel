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
    /// A compound term is a syntactic structure composed of a function symbol applied to zero or term arguments.
    /// 
    ///       f(t1, t2, t3, ...)
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2229:ImplementSerializationConstructors"), Serializable]
    public class CompoundTerm : Term, ISerializable
    {
        Symbol functionSymbol;
        Sequence<Term> arguments;

        /// <summary>
        /// Constructs a compound term, a syntactic structure composed of a function symbol applied
        /// to zero or term arguments.
        /// 
        ///       f(t1, t2, t3, ...)
        /// 
        /// </summary>
        /// <param name="functionSymbol">The function symbol</param>
        /// <param name="arguments">The arguments</param>
        public CompoundTerm(Symbol functionSymbol, params Term[] arguments)
        {
            this.functionSymbol = functionSymbol;
            this.arguments = new Sequence<Term>(arguments);
        }

        /// <summary>
        /// Utility function to create an compound term from a string <paramref name="name"/>
        /// and arguments. The arguments will be converted to term representations.
        /// An argument that is alredy a term is not coverted but left as is.
        /// </summary>
        /// <param name="name">The name of the function symbol.</param>
        /// <param name="args">The .NET values to be converted to terms.</param>
        /// <returns>The created compound term</returns>
        static public CompoundTerm Create(string name, params IComparable[] args)
        {
          Sequence<Term> termArgs = Sequence<Term>.EmptySequence;
          foreach (IComparable arg in args)
          {
            Term t = arg as Term;
            if (t == null)
              termArgs = termArgs.AddLast(NModel.Internals.AbstractValue.GetTerm(arg));
            else
              termArgs = termArgs.AddLast(t);
          }

          return new CompoundTerm(new Symbol(name), termArgs);
        }

        /// <summary>
        /// Constructs a compound term, a syntactic structure composed of a function symbol applied
        /// to zero or term arguments.
        /// 
        ///       f(t1, t2, t3, ...)
        /// 
        /// </summary>
        /// <param name="functionSymbol">The function symbol</param>
        /// <param name="arguments">The arguments</param>
        public CompoundTerm(Symbol functionSymbol, Sequence<Term> arguments)
        {
            this.functionSymbol = functionSymbol;
            this.arguments = arguments;
        }

        /// <summary>
        /// True if term consists only of concrete terms, false otherwise (that is, when the
        /// term contains one or more logic variables)
        /// </summary>
        public override bool IsGround { get { return arguments.Forall(delegate(Term t) { return t.IsGround; }); } }

        /// <summary>
        /// The function symbol as an IComparable. For term f(1, 2), f is the function symbol.
        /// </summary>
        public override IComparable FunctionSymbol { get { return functionSymbol; } }

        /// <summary>
        /// The function symbol. For term f(1, 2), f is the function symbol.
        /// Gets the symbol as a <see cref="Symbol"/> rather than 
        /// upcasting it to <see cref="IComparable"/>.
        /// </summary>
        public Symbol Symbol { get { return functionSymbol; } }

        /// <summary>
        /// Gets the sequence of arguments of this compound term
        /// </summary>
        public override Sequence<Term> Arguments { get { return this.arguments; } }

        // Term this[int i] { get { return this.arguments[i]; } }

        /// <summary>
        /// Pretty printing of terms
        /// </summary>
        /// <returns>A string in the form f(t1, t2, ...)</returns>
        public override string  ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(functionSymbol.ToString());
            sb.Append("(");
            bool isFirst = true;
            foreach (IComparable obj in this.arguments)
            {
                if (!isFirst) sb.Append(", ");
                sb.Append(obj == null ? "null" : obj.ToString());
                isFirst = false;
            }
            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// Pretty printing of terms with function symbols that do not indidate underlying .Net type
        /// </summary>
        /// <returns>A string in the form f(t1, t2, ...)</returns>
        public override string ToCompactString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(functionSymbol.Name);
            sb.Append("(");
            bool isFirst = true;
            foreach (IComparable obj in this.arguments)
            {
                if (!isFirst) sb.Append(", ");
                Term objT = obj as Term;
                sb.Append(obj == null ? "null" : (objT != null ? objT.ToCompactString() : obj.ToString()));
                isFirst = false;
            }
            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// Parse the string into a compound term.
        /// </summary>
        /// <param name="s">given string representing a compound term</param>
        /// <returns>compound term represented by the string</returns>
        new public static CompoundTerm Parse(string s)
        {
            return (CompoundTerm)Term.Parse(s);
        }

        /// <summary>
        /// Apply the given substitution to this term.
        /// All the variables in this term that appear in the 
        /// substitution are replaced by the terms the variables 
        /// are mapped to by the substitution.
        /// </summary>
        public override Term Substitute(Map<Variable, Term> substitution)
        {
            if (this.IsGround)
                return this;
            else
                return new CompoundTerm(this.functionSymbol, SubstituteInSubterms(this.arguments, substitution));

        }

        private Sequence<Term> SubstituteInSubterms(Sequence<Term> sequence, Map<Variable, Term> substitution)
        {
            Sequence<Term> result = Sequence<Term>.EmptySequence;
            foreach (Term subterm in sequence)
            {
                result = result.AddLast(subterm.Substitute(substitution));
            }
            return result;
        }

        #region ISerializable Members

        /// <summary>
        /// Serialization (implementation of <see cref="ISerializable"/>).
        /// </summary>
        /// <param name="info">Serialization data record</param>
        /// <param name="context">Serialization context</param>
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            info.AddValue("rep", this.ToString());
        }

        private CompoundTerm(SerializationInfo info, StreamingContext context)
        {
            CompoundTerm t =  (CompoundTerm) Term.Parse(info.GetString("rep"));
            this.functionSymbol = t.functionSymbol;
            this.arguments = t.arguments;
        }


        /// <summary>
        /// The string name of the function symbol
        /// </summary>
        public string Name
        {
            get { return this.Symbol.Name; }
        }

        /// <summary>
        /// Gets the interpretation of the k'th argument
        /// </summary>
      public IComparable this[int k]
      {
        get
        {
          if (k < 0 || k >= this.arguments.Count)
            throw new InvalidOperationException("Invalid argument index: " + k);
          try
          {
            return CompoundValue.InterpretTerm(this.Arguments[k]);
          }
          catch (ArgumentException)
          {
            return this.Arguments[k];
          }
        }
      }

        #endregion
    }
}
