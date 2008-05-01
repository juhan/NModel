//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NModel.Terms;
using NModel.Internals;
using NModel.Attributes;
using NModel.Utilities;

namespace NModel.Execution
{
    internal class ActionMethodFinish : ActionMethod
    {        
        public int[] outputArgumentIndices;

        public ActionMethodFinish(CompoundTerm actionLabel, Method method, int arity, int[] outputArgumentIndices)
            : base(actionLabel, method, arity)
        {
            this.outputArgumentIndices = outputArgumentIndices;
        }
 
        public override ActionKind Kind { get { return ActionKind.Finish; } }

        public override bool HasActionParameterDomain(int parameterIndex)
        {
            return (0 <= parameterIndex && parameterIndex < this.outputArgumentIndices.Length);
        }

        // TODO: maybe return delegate that invokes continuation 
        public override ParameterGenerator/*?*/ GetParameterGenerator(int parameterIndex)
        {
            return null;
        }

        public override CompoundTerm DoStep(InterpretationContext c, CompoundTerm action)
        {
            throw new InvalidOperationException("DoStep cannot be called on type ActionMethodFinish.");
        }

        internal override bool IsPotentiallyEnabled(InterpretationContext c)
        {
            throw new InvalidOperationException("Shouldn't be invoked.");
        }

        internal override bool IsEnabled(InterpretationContext c, Sequence<Term> args)
        {
            throw new InvalidOperationException("Shouldn't be invoked.");
        }

        internal override IEnumerable<string> GetEnablingConditionDescriptions(InterpretationContext c, Sequence<Term> args, bool returnFailures)
        {
            throw new InvalidOperationException("Shouldn't be invoked.");
        } 
    }
}
