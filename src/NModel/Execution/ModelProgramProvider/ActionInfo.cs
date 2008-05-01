//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using NModel.Internals;
using NModel.Terms;

namespace NModel.Execution
{
    /// <summary>
    /// Action symbol that maps to a .Net method
    /// </summary>
    internal class ActionInfo
    {
        readonly int arity;                          // the number of parameters of this action (>= 0)
        readonly Symbol[] parameterSorts;            // the "sort" (abstract type) of each parameter
        readonly ActionKind kind;                    // Start, Atomic or Finish
        readonly List<ActionMethod> actionMethods;   // the preconditions and updates for this action

        public ActionInfo(int arity, Symbol[] parameterSorts, ActionKind kind, List<ActionMethod> actionMethods)
        {
            this.arity = arity;
            this.parameterSorts = parameterSorts;
            this.kind = kind;
            this.actionMethods = actionMethods;
        }

        public int Arity { get { return arity; } }
        public Symbol[] ParameterSorts { get { return (Symbol[])parameterSorts.Clone(); } }
        public ActionKind Kind { get { return kind; } }
        public IEnumerable<ActionMethod> ActionMethods { get { return actionMethods; } }

        public bool IsAtomic
        {
            get
            {
                return (this.kind == ActionKind.Atomic);
            }
        }

        // debug
        internal bool SatisfiesInvariant()
        {
            foreach (ActionMethod am in this.actionMethods)
            {
                if ((this.kind == ActionKind.Finish) != (am.Kind == ActionKind.Finish))
                    return false;
            }
            if (this.parameterSorts == null) return false;
            if (this.parameterSorts.Length != this.arity) return false;
            return true;
        }

        public bool IsPotentiallyEnabled(InterpretationContext c)
        {
            foreach (ActionMethod am in this.actionMethods)
                if (!am.IsPotentiallyEnabled(c))
                    return false;
            return true;
        }

        public IEnumerable<ParameterGenerator> GetParameterGenerators(int parameterIndex)
        {
            foreach (ActionMethod am in this.actionMethods)
            {
                ParameterGenerator/*?*/ paramGen = am.GetParameterGenerator(parameterIndex);
                if (null != paramGen)
                    yield return paramGen;
            }
        }

        public bool IsEnabled(InterpretationContext c, Sequence<Term> args)
        {
            foreach (ActionMethod am in this.actionMethods)
                if (!am.IsEnabled(c, args))
                    return false;
            return true;
        }

        public IEnumerable<string> GetEnablingConditionDescriptions(InterpretationContext c, Sequence<Term> args, bool returnFailures)
        {
             foreach (ActionMethod am in this.actionMethods)
                foreach (string s in am.GetEnablingConditionDescriptions(c, args, returnFailures))
                    yield return s;
        }

    }
}