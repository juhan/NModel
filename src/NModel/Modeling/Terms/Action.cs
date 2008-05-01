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
  /// Instances of this class are actions.
  /// Actions are compound terms.
  /// </summary>
  [Serializable]
  public class Action : CompoundTerm
  {
    /// <summary>
    /// Constructs an action for a  given action symbol and sequence of arguments.
    /// </summary>
    /// <param name="f">action symbol</param>
    /// <param name="args">arguments of the action</param>
    public Action(Symbol f, Sequence<Term> args) : base(f, args) { }
    /// <summary>
    /// Constructs an action for a  given action symbol and a params array of arguments.
    /// </summary>
    /// <param name="f">action symbol</param>
    /// <param name="args">arguments of the action</param>
    public Action(Symbol f, params Term[] args) : base(f, args) { }

    /// <summary>
    /// Utility function to create an action from a string name
    /// and a params array of values. 
    /// Values that are not terms are converted to terms.
    /// </summary>
    /// <param name="name">The name of the action symbol</param>
    /// <param name="args">The .NET values to be converted to terms.</param>
    /// <returns>The created action</returns>
    new static public Action Create(string name, params IComparable[] args)
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

      return new Action(new Symbol(name), termArgs);
    }

    /// <summary>
    /// Parse the string into an action.
    /// </summary>
    /// <param name="s">given string representing of an action</param>
    /// <returns>action represented by the string</returns>
    new public static Action Parse(string s)
    {
      return (Action)CompoundTerm.Parse(s);
    }
  }
}
