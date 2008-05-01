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

namespace NModel.Execution
{
    internal class StatePredicate
    {
        public class Predicate
        {
            public readonly bool isStatic;
            public readonly MethodInfo method;
            public string documentationString;

            public Predicate(bool isStatic, MethodInfo method, string documentationString)
            {
                this.isStatic = isStatic;
                this.method = method;
                this.documentationString = documentationString;
            }

            public string Documentation { get { return documentationString; } }
        }

        Type type;
        List<Predicate> predicates;

        StatePredicate(Type type, List<Predicate> predicates)
        {
            this.type = type;
            this.predicates = predicates;
        }

        internal Type Type { get { return type; } }

        public static StatePredicate GetPredicates(Type type, List<string> methodNames)                                          
        {
            List<Predicate> predicates = new List<Predicate>();

            foreach (string methodName in methodNames)
            {
               MethodInfo pred = type.GetMethod(methodName,
                                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, 
                                                null, new Type[0], null);
               if (null != pred)
               {
                   if (pred.ReturnType != typeof(bool))
                       throw new ArgumentException("state predicate did not return bool: " + methodName);
                   Predicate p = new Predicate(pred.IsStatic, pred, "");
                   predicates.Add(p); 
               }
               else
               {
                   throw new ArgumentException("Couldn't find method for state predicate: " + methodName);
               }
            }  

            return (predicates.Count > 0) ? new StatePredicate(type, predicates) : null;
        }

        public static StatePredicate GetAcceptingStateCondition(Type t)
        {
            //System.Diagnostics.Debugger.Break();
            List<Predicate> predicates = new List<Predicate>();
            foreach (MethodInfo mInfo in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                object[] attrs = mInfo.GetCustomAttributes(typeof(AcceptingStateConditionAttribute), false);
                if (attrs == null || attrs.Length == 0)
                    continue;
                if (mInfo.ReturnType != typeof(bool))
                    throw new ModelProgramUserException("Accepting state condition '" + mInfo.Name + "' in '" + t.FullName + "' is not Boolean");
                if (!mInfo.IsStatic)
                    throw new ModelProgramUserException("Accepting state condition '" + mInfo.Name + "' in '" + t.FullName + "' is not static");
                ParameterInfo[] paramInfos = mInfo.GetParameters();
                if (paramInfos != null && paramInfos.Length > 0)
                    throw new ModelProgramUserException("Accepting state condition '" + mInfo.Name + "' in '" + t.FullName + "' is not parameterless");
                predicates.Add(new Predicate(true, mInfo, ""));
            }
            //check also if there are properties that are marked as accepting state conditions
            foreach (PropertyInfo pInfo in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                object[] attrs = pInfo.GetCustomAttributes(typeof(AcceptingStateConditionAttribute), false);
                if (attrs == null || attrs.Length == 0)
                    continue;
                MethodInfo mInfo = null;
                mInfo = pInfo.GetGetMethod();
                if (mInfo == null)
                    mInfo = pInfo.GetGetMethod(true);
                if (mInfo == null)
                    throw new ModelProgramUserException("Accepting state condition '" + pInfo.Name + "' in '" + t.FullName + "' has no get accessor");
                if (!mInfo.IsStatic)
                    throw new ModelProgramUserException("Accepting state condition '" + pInfo.Name + "' in '" + t.FullName + "' is not static");
                if (pInfo.PropertyType != typeof(bool))
                    throw new ModelProgramUserException("Accepting state condition '" + pInfo.Name + "' in '" + t.FullName + "' is not Boolean");
                ParameterInfo[] paramInfos = mInfo.GetParameters();
                if (paramInfos != null && paramInfos.Length > 0)
                    throw new ModelProgramUserException("Accepting state condition '" + pInfo.Name + "' in '" + t.FullName + "' is not parameterless");
                predicates.Add(new Predicate(true,mInfo, ""));
            }
            return new StatePredicate(t, predicates);
        }

        public static StatePredicate GetStateInvariant(Type t)
        {
            //System.Diagnostics.Debugger.Break();
            List<Predicate> predicates = new List<Predicate>();
            foreach (MethodInfo mInfo in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                object[] attrs = mInfo.GetCustomAttributes(typeof(StateInvariantAttribute), false);
                if (attrs == null || attrs.Length == 0)
                    continue;
                if (mInfo.ReturnType != typeof(bool))
                    throw new ModelProgramUserException("State invariant '" + mInfo.Name + "' in '" + t.FullName + "' is not Boolean");
                if (!mInfo.IsStatic)
                    throw new ModelProgramUserException("State invariant '" + mInfo.Name + "' in '" + t.FullName + "' is not static");
                ParameterInfo[] paramInfos = mInfo.GetParameters();
                if (paramInfos != null && paramInfos.Length > 0)
                    throw new ModelProgramUserException("State invariant '" + mInfo.Name + "' in '" + t.FullName + "' is not parameterless");
                predicates.Add(new Predicate(true, mInfo, ""));
            }
            //check also if there are properties that are marked as state invariants
            foreach (PropertyInfo pInfo in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                object[] attrs = pInfo.GetCustomAttributes(typeof(StateInvariantAttribute), false);
                if (attrs == null || attrs.Length == 0)
                    continue;
                MethodInfo mInfo = null;
                mInfo = pInfo.GetGetMethod();
                if (mInfo == null)
                    mInfo = pInfo.GetGetMethod(true);
                if (mInfo == null)
                    throw new ModelProgramUserException("State invariant '" + pInfo.Name + "' in '" + t.FullName + "' has no get accessor");
                if (!mInfo.IsStatic)
                    throw new ModelProgramUserException("State invariant '" + pInfo.Name + "' in '" + t.FullName + "' is not static");
                if (pInfo.PropertyType != typeof(bool))
                    throw new ModelProgramUserException("State invariant '" + pInfo.Name + "' in '" + t.FullName + "' is not Boolean");
                ParameterInfo[] paramInfos = mInfo.GetParameters();
                if (paramInfos != null && paramInfos.Length > 0)
                    throw new ModelProgramUserException("State invariant '" + pInfo.Name + "' in '" + t.FullName + "' is not parameterless");
                predicates.Add(new Predicate(true, mInfo, ""));
            }
            return new StatePredicate(t, predicates);
        }

        public static StatePredicate GetStateFilter(Type t)
        {
            //System.Diagnostics.Debugger.Break();
            List<Predicate> predicates = new List<Predicate>();
            foreach (MethodInfo mInfo in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                object[] attrs = mInfo.GetCustomAttributes(typeof(StateFilterAttribute), false);
                if (attrs == null || attrs.Length == 0)
                    continue;
                if (mInfo.ReturnType != typeof(bool))
                    throw new ModelProgramUserException("State filter '" + mInfo.Name + "' in '" + t.FullName + "' is not Boolean");
                if (!mInfo.IsStatic)
                    throw new ModelProgramUserException("State filter '" + mInfo.Name + "' in '" + t.FullName + "' is not static");
                ParameterInfo[] paramInfos = mInfo.GetParameters();
                if (paramInfos != null && paramInfos.Length > 0)
                    throw new ModelProgramUserException("State filter '" + mInfo.Name + "' in '" + t.FullName + "' is not parameterless");
                predicates.Add(new Predicate(true, mInfo, ""));
            }
            //check also if there are properties that are marked as state invariants
            foreach (PropertyInfo pInfo in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                object[] attrs = pInfo.GetCustomAttributes(typeof(StateFilterAttribute), false);
                if (attrs == null || attrs.Length == 0)
                    continue;
                MethodInfo mInfo = null;
                mInfo = pInfo.GetGetMethod();
                if (mInfo == null)
                    mInfo = pInfo.GetGetMethod(true);
                if (mInfo == null)
                    throw new ModelProgramUserException("State filter '" + pInfo.Name + "' in '" + t.FullName + "' has no get accessor");
                if (!mInfo.IsStatic)
                    throw new ModelProgramUserException("State filter '" + pInfo.Name + "' in '" + t.FullName + "' is not static");
                if (pInfo.PropertyType != typeof(bool))
                    throw new ModelProgramUserException("State filter '" + pInfo.Name + "' in '" + t.FullName + "' is not Boolean");
                ParameterInfo[] paramInfos = mInfo.GetParameters();
                if (paramInfos != null && paramInfos.Length > 0)
                    throw new ModelProgramUserException("State filter '" + pInfo.Name + "' in '" + t.FullName + "' is not parameterless");
                predicates.Add(new Predicate(true, mInfo, ""));
            }
            return new StatePredicate(t, predicates);
        }

        internal bool Holds(InterpretationContext c, Set<IComparable> domain)
        {
            c.SetAsActive();
            try
            {
                //check each predicate
                foreach (Predicate pred in predicates)
                {
                    if (pred.isStatic)
                    {
                        if (!((bool)pred.method.Invoke(null, new object[0])))
                            return false;
                    }
                    else
                    {
                        // if instance-based, check predicate for each instance in domain
                        foreach (IComparable obj in domain)
                        {
                           if (!((bool)pred.method.Invoke(obj, new object[0])))
                                return false;
                        }
                    }
                }
                return true;
            }
            finally
            {
                c.ClearAsActive();
            }
        }
    }
}
