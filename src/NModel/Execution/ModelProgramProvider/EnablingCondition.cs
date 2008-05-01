//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NModel.Internals;
using NModel.Terms;
using NModel.Attributes;
using NModel.Utilities;

namespace NModel.Execution
{
    internal class EnablingCondition
    {
        public class Predicate
        {
            public readonly bool isStatic;
            public readonly int arity;
            public readonly MethodInfo method;
            public readonly string[] description;

            public Predicate(MethodInfo method, string[] description)
            {
               this.isStatic = method.IsStatic;
               this.arity = method.GetParameters().Length;
               this.method = method;
               this.description = description;
            }

            public bool Holds(IComparable/*?*/ thisArg, IComparable[]/*?*/ arguments)
            {

                //TBD: add contract that Invoke on a method info with 
                //boolean result returns a non-null boxed boolean
                //TODO: make work for instance methods and static methods

                int arity1 = this.arity;

                if (this.isStatic)
                {
                    IComparable/*?*/[]/*?*/ pArgs = null;
                    if (null != arguments)
                    {
                        // static precondition
                        int startIndex = (null == thisArg ? 0 : 1);
                        pArgs = new IComparable[arity1 + startIndex];
                        if (startIndex > 0)
                            pArgs[0] = thisArg;
                        for (int i = startIndex; i < arity1 + startIndex; i++)
                            pArgs[i] = arguments[i];
                    }
                    return (bool)this.method.Invoke(null, pArgs);    
                }
                else
                {
                    // instance-based precondition
                    if (null == thisArg)
                        throw new InvalidOperationException("Null object for invocation of instance-based enabling condition.");

                    IComparable/*?*/[]/*?*/ pArgs = new IComparable[arity1];
                    for (int i = 0; i < arity1; i++)
                        pArgs[i] = arguments[i];
                    return (bool)this.method.Invoke(thisArg, pArgs);
                }

            }
        }

        List<Predicate> predicates;
        // IComparable/*?*/[] defaultInputParameters;


        public static List<Predicate> GetPredicates(MethodInfo method, Type[] inputParameterTypes)
        {
            List<Predicate> result = new List<Predicate>();
            //bool isStatic = method.IsStatic;
            //Type type = method.DeclaringType;

            foreach(MethodInfo pred in ReflectionHelper.GetEnablingConditionMethods(method, inputParameterTypes)) 
            {
               string[] documentation = ReflectionHelper.GetEnablingConditionDocumentation(pred);
               Predicate p = new Predicate(pred, documentation);
               result.Add(p);
            }

            // For now, allow methods not to have enabling conditions
            //if (result.Count == 0)
            //    throw new ModelProgramUserException("Enabling condition method " + method.Name + "Enabled() was required but not found.");

            return result;
        }

        /// <summary>
        /// Creates an enabling condition for an action method.
        /// </summary>
        internal EnablingCondition(bool parameterless, Type[]/*?*/ inputParameterTypes, MethodInfo method)
        {
            //Type type = method.DeclaringType; // model.GetType();
            //bool isStatic = method.IsStatic;

            //Type[] paramTypes = (parameterless ? new Type[] { } : inputParameterTypes);
            //List<MethodInfo> preds = new List<MethodInfo>();
            //List<int> arities = new List<int>();
            //// to do: allow instance methods to be used as enabling condtions of instance-based actions.
            
            //foreach (EnablingConditionAttribute attr in ActionInfo.GetEnablingConditionAttributes(method))
            //{
            //    Type/*?*/[] tmp = new Type/*?*/[paramTypes.Length];
            //    for (int i = 0; i < paramTypes.Length; i++) tmp[i] = paramTypes[i];
            //    MethodInfo/*?*/ pred = type.GetMethod(attr.MethodName, 
            //                                          BindingFlags.Public | BindingFlags.NonPublic |
            //         BindingFlags.Static | BindingFlags.Instance, null, tmp, null);
            //    if (pred != null && pred.ReturnType == typeof(bool) &&
            //        (mayDependOnState || IsStateIndependent(pred)))
            //    {
            //        preds.Add(pred);
            //        arities.Add(pred.GetParameters().Length);
            //    }
            //}

            List<Predicate> preds = GetPredicates(method, inputParameterTypes);

            if (parameterless)
            {
                List<Predicate> newPreds = new List<Predicate>();
                foreach (Predicate p in preds)
                    if (p.arity == 0 && p.isStatic) newPreds.Add(p);
                preds = newPreds;
            }

            //initialize the fields
            this.predicates = preds;
            // this.arities = arities;
            //this.model = model;
            //IComparable[] defaultInputParameters = new IComparable[inputParameterTypes.Length];
            //for (int i = 0; i < inputParameterTypes.Length; i += 1)
            //{
            //    //Type t = inputParameterTypes[i];
            //    //if (t.IsValueType)
            //    //{
            //    //    ConstructorInfo ci = t.GetConstructor(new Type[0]);
            //    //    defaultInputParameters[i] = (IComparable)ci.Invoke(new object[0]);
            //    //}
            //    //else
            //    {
            //        defaultInputParameters[i] = null;
            //    }
            //}
            //this.defaultInputParameters = defaultInputParameters;
        }

        //static private bool ContainsAnyValue(object/*?*/[] arguments)
        //{
        //    for (int i = 0; i < arguments.Length; i++)
        //    {
        //        if (arguments[i] == Any.Value) return true;
        //    }
        //    return false;
        //}

        internal bool Holds(InterpretationContext c, IComparable/*?*/ thisArg, IComparable[]/*?*/ arguments)
        {
            c.SetAsActive();
            try
            {
                //check each predicate
                foreach (Predicate pred in predicates)
                    if (!pred.Holds(thisArg, arguments)) return false;
                return true;
            }
            finally
            {
                c.ClearAsActive();
            }
        }

        internal IEnumerable<string> GetEnablingConditionDescriptions(InterpretationContext c, IComparable/*?*/ thisArg, IComparable[]/*?*/ arguments, bool returnFailures)
        {
            foreach (Predicate pred in predicates)
            {
                c.SetAsActive();
                try
                {
                    bool holds = pred.Holds(thisArg, arguments);
                    if ((returnFailures && !holds) || (!returnFailures && holds))
                        foreach (string s in pred.description)
                            yield return s;
                }
                finally
                {
                    c.ClearAsActive();
                }
            }
        }

        //TBD: use attribute to decide
        static private bool IsStateIndependent(MethodInfo/*?*/ method)
        {
            if (method == null)
                return true;
            else
                return false;
        }

    }
}
