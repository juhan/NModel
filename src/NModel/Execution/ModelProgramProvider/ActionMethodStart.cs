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
    internal class ActionMethodStart : ActionMethod
    {
        readonly ActionMethodFinish/*?*/ finishActionMethod;  // corresponding finish action method
        readonly int[] inputArgumentIndices;  // method arg position is index[ith action arg];
                                              // -1 means implicit this param
                                              // -2 means Any.Value ("_")

        public ActionMethodStart(CompoundTerm actionLabel, Method method, int arity, ActionMethodFinish/*?*/ finishActionMethod, int[] inputArgumentIndices)
            : base(actionLabel, method, arity)
        {
            this.finishActionMethod = finishActionMethod;
            this.inputArgumentIndices = inputArgumentIndices;
        }

        public override ActionKind Kind
        {
            get
            {
                return finishActionMethod == null ? ActionKind.Atomic : ActionKind.Start;
            }
        }

        public Symbol/*?*/ FinishAction
        {
            get
            {
                if (null == this.finishActionMethod)
                    return null;
                else
                {
                    return this.finishActionMethod.actionLabel.Symbol;
                }
            }
        }

        public override bool HasActionParameterDomain(int parameterIndex)
        {
            if (0 <= parameterIndex && parameterIndex < this.inputArgumentIndices.Length)
            {
                int methodParameterIndex = this.inputArgumentIndices[parameterIndex];
                if (methodParameterIndex == -1)
                    return (this.method.thisParameterGenerator != null);
                else if (methodParameterIndex == -2)
                    return true;     // domain is Set<Term>(Any.Value)
                else
                    return (this.method.inputParameterGenerators[methodParameterIndex] != null);
            }
            else
                throw new ArgumentOutOfRangeException("parameterIndex");
        }

        static readonly Set<Term> anyDomain = new Set<Term>(Any.Value);

        static Set<Term> AnyDomainParameterGenerator() { return anyDomain; }

        public override ParameterGenerator/*?*/ GetParameterGenerator(int parameterIndex)
        {
            int methodParameterIndex = this.inputArgumentIndices[parameterIndex];
            if (methodParameterIndex == -1)
                return this.method.thisParameterGenerator;
            if (methodParameterIndex == -2)
                return AnyDomainParameterGenerator;
            else
                return this.method.inputParameterGenerators[methodParameterIndex];
        }

        internal override bool IsPotentiallyEnabled(InterpretationContext c)
        {
            return this.method.IsPotentiallyEnabled(c);
        }

        internal override bool IsEnabled(InterpretationContext c, Sequence<Term> args)
        {
            IComparable/*?*/ thisArg = null;
            IComparable[] methodArgs = ConvertTermArgumentsToMethodArguments(c, args, out thisArg);
            return this.method.IsEnabled(c, thisArg, methodArgs); 
        }

        internal override IEnumerable<string> GetEnablingConditionDescriptions(InterpretationContext c, Sequence<Term> args, bool returnFailures)
        {
            IComparable/*?*/ thisArg = null;
            IComparable[] methodArgs = ConvertTermArgumentsToMethodArguments(c, args, out thisArg);
            return this.method.GetEnablingConditionDescriptions(c, thisArg, methodArgs, returnFailures);
        }        

        IComparable/*?*/[] ConvertTermArgumentsToMethodArguments(InterpretationContext c, Sequence<Term> termArguments, out IComparable thisArg)
        {
            int nMethodArgs = this.method.parameterInfos.Length;
            IComparable/*?*/[] methodArgs = new IComparable[nMethodArgs];
            thisArg = null;

            int k = 0;
            int nParametersUsed = this.inputArgumentIndices.Length;
            foreach (Term arg in termArguments)
            {
                if (k >= nParametersUsed)
                    break;

                int methodArgIndex = this.inputArgumentIndices[k++];
                IComparable argInterpretation = c.InterpretTerm(arg);  // convert from term to .NET runtime object                 
                
                if (-1 == methodArgIndex)
                    thisArg = argInterpretation;
                else if (methodArgIndex >= 0)
                    methodArgs[methodArgIndex] = argInterpretation;
            }
            return methodArgs;
        }

        public override CompoundTerm DoStep(InterpretationContext c, CompoundTerm action)
        {
            // Result of invocation must be a value term (must support IComparable) 
            
            IComparable/*?*/ thisArg;
            IComparable/*?*/[] methodArgs = this.ConvertTermArgumentsToMethodArguments(c, action.Arguments, out thisArg);

            foreach (IComparable/*?*/ o in methodArgs)
                AbstractValue.FinalizeImport(o);

            object/*?*/ resultObj = this.method.methodInfo.Invoke(thisArg, methodArgs);
            CompoundTerm/*?*/ finishAction = null;
            
            // Handle output args and return value
            if (null != this.finishActionMethod)
            {
                int nOutputs = this.finishActionMethod.actionLabel.Arguments.Count;
                Sequence<Term> outputs = Sequence<Term>.EmptySequence;

                for (int i = 0; i < nOutputs; i += 1)
                {
                    int outputArgIndex = this.finishActionMethod.outputArgumentIndices[i];
                    if (-2 == outputArgIndex) // "any" placeholder
                        outputs = outputs.AddLast(Any.Value);
                    else
                    {
                        object output = (-1 == outputArgIndex ? resultObj : methodArgs[outputArgIndex]);


                        IComparable outputAsComparable;
                        if (null == output)
                            outputAsComparable = null;
                        else
                        {
                            outputAsComparable = output as IComparable;
                            if (null == outputAsComparable)
                                throw new InvalidOperationException(MessageStrings.LocalizedFormat(MessageStrings.ComparableResultRequired, action.ToString(), output.ToString()));
                        }
                        outputs = outputs.AddLast(AbstractValue.GetTerm(outputAsComparable));
                    }
                }
                finishAction = new CompoundTerm(this.FinishAction, outputs);
            }

            return finishAction;
        }

        internal override bool SatisfiesInvariant()
        {
            return base.SatisfiesInvariant() &&
                   (null != this.finishActionMethod ? (this.finishActionMethod.method == this.method) : true);
        }

    }
}
