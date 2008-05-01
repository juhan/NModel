//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Terms;
using NModel;
using NModel.Internals;

namespace NModel
{
    /// <summary>
    /// Static class with callbacks into the execution framework. Used for recording coverage points.
    /// </summary>
    public static class Execute
    {

        //static public T Choose<T>(Symbol action, Set<T> choices) where T : IComparable
        //{
        //    Set<CompoundTerm> actionChoices = choices.Convert<CompoundTerm>(delegate(T choice)
        //    {
        //        return new CompoundTerm(action, AbstractValue.GetTerm(choice));
        //    });
        //    CompoundTerm chosenTerm = ChooseAction(actionChoices);
        //    T result = (T)AbstractValue.InterpretTerm(chosenTerm);
        //    return result;
        //}

        //static public CompoundTerm ChooseAction(Set<CompoundTerm> choices)
        //{
        //    return InterpretationContext.GetCurrentContext().Choose(choices);
        //}

        //static public CompoundTerm ChooseAction(params CompoundTerm[] choices)
        //{
        //    return InterpretationContext.GetCurrentContext().Choose(new Set<CompoundTerm>(choices));
        //}

        //static public IComparable DoAction(string actionName, params IComparable[] args)
        //{
        //    throw new Exception("Not implemented");
        //}

        //static public CompoundTerm GetAction(string actionName, params IComparable[] args)
        //{
        //    Sequence<Term> termArgs = new Sequence<IComparable>(args).Convert<Term>(AbstractValue.GetTerm);
        //    return new CompoundTerm(new Symbol(actionName), termArgs); 
        //}

        //static public void StartChoice() 
        //{
        //    throw new Exception("Not implemented");
        //}

        //static public void EnableAction(string actionName, params IComparable[] args) 
        
        //{
        //    throw new Exception("Not implemented");
        //}

        //static public IComparable ChooseAction() 
        //{
        //    throw new Exception("Not implemented");
        //}

        /// <summary>
        /// Signals execution framework that a user-defined coverage point has been passed. Coverage points
        /// may be used to guide execution so that relevant aspects of the model are represented in
        /// analysis and testing. For example, coverage points may include user-level requirements, projections
        /// of the current state or execution paths of the model program source.
        /// </summary>
        /// <param name="coveragePoint">A term representing a user-defined coverage point.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="coveragePoint"/> is null</exception>
        static public void AddCoveragePoint(Term coveragePoint)
        {
            if (null == coveragePoint)
                throw new ArgumentNullException("coveragePoint");
            InterpretationContext.GetCurrentContext().AddCoveragePoint(coveragePoint);
        }

        /// <summary>
        /// Signals execution framework that a user-defined coverage point has been passed. Coverage points
        /// may be used to guide execution so that relevant aspects of the model are represented in
        /// analysis and testing. For example, coverage points may include user-level requirements, projections
        /// of the current state or execution paths of the model program source.
        /// </summary>
        /// <param name="coveragePointValue">A value whose corresponding term is a user-defined coverage point.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="coveragePointValue"/> does not satisfy the 
        /// <see cref="AbstractValue.IsAbstractValue"/> condition</exception>
        /// <seealso cref="AbstractValue.GetTerm"/>
        static public void AddCoveragePoint(IComparable coveragePointValue)
        {
            if (!AbstractValue.IsAbstractValue(coveragePointValue))
                throw new ArgumentException(MessageStrings.CoveragePointTypeError);
            InterpretationContext.GetCurrentContext().AddCoveragePoint(AbstractValue.GetTerm(coveragePointValue));
        }

        /// <summary>
        /// The set of feature keywords that apply in the current execution context.
        /// </summary>
        static public Set<string> Features
        {
            get
            {
                return InterpretationContext.GetCurrentContext().Features;
            }
        }
    }
}

