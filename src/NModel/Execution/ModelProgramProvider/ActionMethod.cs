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
    delegate Set<Term> ParameterGenerator();

    /// <summary>
    /// Action symbol that maps to a .Net method
    /// </summary>
    internal abstract class ActionMethod
    {
        abstract public ActionKind Kind { get; }

        public CompoundTerm actionLabel;
        public Method method;
        public int arity;            // number of action arguments recognized

        // invariant: this.arity <= aInfo.arity when this in aInfo.actionMethods

        protected ActionMethod(CompoundTerm actionLabel, Method method, int arity)
        {
            this.actionLabel = actionLabel;
            this.method = method;
            this.arity = arity;
        }

        public int Arity { get { return arity; } }


        internal abstract bool IsPotentiallyEnabled(InterpretationContext c);


        internal abstract bool IsEnabled(InterpretationContext c, Sequence<Term> args);

        internal abstract IEnumerable<string> GetEnablingConditionDescriptions(InterpretationContext c, Sequence<Term> args, bool returnFailures);
 
        internal static Type[] GetOutputParameterTypes(MethodInfo actionMethod)
        {
            List<Type> pTypes = new List<Type>();
            foreach (ParameterInfo pInfo in actionMethod.GetParameters())
            {
                //TBD: Need contract that ParameterInfo.ParameterType != null
                Type pType = /*^(Type)^*/ pInfo.ParameterType;
                if (pType.IsByRef)
                {
                    //TBD: Need contract that (pType.IsByRef) => pType.GetElementType() != null
                    pType = /*^(Type)^*/ pType.GetElementType();
                }

                if (ReflectionHelper.IsOutputParameter(pInfo)) pTypes.Add(pType);
            }

            //TBD: need contract that actionMethod.ReturnType != null
            if (actionMethod.ReturnType != typeof(void))
                pTypes.Add(/*^(Type)^*/actionMethod.ReturnType);
            //TBD: Need contract that pTypes.ToArray() returns Type![]!
            return /*^(Type![]!)^*/ pTypes.ToArray();
        }

        //internal static EnablingConditionAttribute[] GetEnablingConditionAttributes(MemberInfo method)
        //{
        //    object/*?*/[]/*?*/ attrs =
        //        method.GetCustomAttributes(typeof(EnablingConditionAttribute), true);
        //    if (attrs == null || attrs.Length == 0)
        //        return new EnablingConditionAttribute[] { };
        //    else
        //        return (EnablingConditionAttribute[])attrs;
        //}

        public abstract bool HasActionParameterDomain(int parameterIndex);

        public abstract ParameterGenerator/*?*/ GetParameterGenerator(int parameterIndex);

        public abstract CompoundTerm DoStep(InterpretationContext c, CompoundTerm action);

        internal virtual bool SatisfiesInvariant()
        {
            return null != this.method &&
                   null != this.actionLabel;
        }


    }





}
