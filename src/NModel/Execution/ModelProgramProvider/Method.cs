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
    internal class Method
    {
        public readonly MethodInfo methodInfo;
        //public readonly bool isStatic;
        public readonly ParameterInfo[] parameterInfos;
        //public readonly Type/*?*/ instanceType;
        public readonly Type[] parameterTypes;
        //public readonly Type/*?*/ returnType;
        public readonly ParameterGenerator/*?*/ thisParameterGenerator;
        public readonly ParameterGenerator/*?*/[] inputParameterGenerators;

        readonly EnablingCondition parameterlessEnablingCondition;
        readonly EnablingCondition enablingCondition;      

        public Method(MethodInfo methodInfo)
        {
            //this.instanceType = null;
            this.parameterTypes = null;
            this.methodInfo = methodInfo;
            //this.isStatic = methodInfo.IsStatic;
            this.parameterInfos = methodInfo.GetParameters();
            //this.returnType = methodInfo.ReturnType.Equals(typeof(void)) ? null : methodInfo.ReturnType; 

            this.thisParameterGenerator = Method.GetThisParameterGenerator(methodInfo);
            this.inputParameterGenerators = Method.GetInputParameterGenerators(methodInfo); ;

            Type[] inputParameterTypes = ReflectionHelper.GetInputParameterTypes(methodInfo);
            this.parameterlessEnablingCondition = new EnablingCondition(true, inputParameterTypes, methodInfo);
            this.enablingCondition = new EnablingCondition(false, inputParameterTypes, methodInfo);          
        }

        internal bool IsPotentiallyEnabled(InterpretationContext c)
        {
            bool res = parameterlessEnablingCondition.Holds(c, null, null);
            return res;
        }

        internal bool IsEnabled(InterpretationContext c, IComparable/*?*/ thisArg, IComparable[] args)
        {
            return enablingCondition.Holds(c, thisArg, args);
        }

        internal IEnumerable<string> GetEnablingConditionDescriptions(InterpretationContext c, IComparable/*?*/ thisArg, IComparable[]/*?*/ arguments, bool returnFailures)
        {
            foreach (string s in parameterlessEnablingCondition.GetEnablingConditionDescriptions(c, null, null, returnFailures))
                yield return s;

            foreach (string s in enablingCondition.GetEnablingConditionDescriptions(c, thisArg, arguments, returnFailures))
                yield return s;
        }

        private static ParameterGenerator CreateDomainParameterGenerator(MethodInfo actionMethod, DomainAttribute attr, Type pType)
        {
            if (!string.IsNullOrEmpty(attr.Name))
            {
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
                Type type = actionMethod.DeclaringType;
                MethodInfo/*?*/ paramGenMethod = type.GetMethod(attr.Name, flags);

                //try to see if it is a property
                if (paramGenMethod == null)
                    paramGenMethod = type.GetMethod("get_" + attr.Name, flags);
                //check that the type is correct
                //finally try to see if it is a field
                FieldInfo paramField = null;
                if (paramGenMethod == null)
                    paramField = type.GetField(attr.Name, flags);
                if (paramGenMethod != null)
                {
                    ParameterInfo[] parameters = paramGenMethod.GetParameters();
                    if (parameters != null && parameters.Length > 0)
                        throw new ModelProgramUserException("Parameter generator '" +
                            attr.Name + "' must not depend on arguments.");
                    if (!IsValidElementTypeOfEnumerationType(paramGenMethod.ReturnType, pType))
                        throw new ModelProgramUserException("Parameter generator '" + 
                            attr.Name + "' does not support IEnumerable<T> such that values of type T can be assigned to a parameter of type " + pType);
                }
                if (paramField != null)
                {
                    if (!IsValidElementTypeOfEnumerationType(paramField.FieldType, pType))
                        throw new ModelProgramUserException("Parameter generator '" +
                            attr.Name + "' does not support IEnumerable<T> such that values of type T can be assigned to a parameter of type " + pType);
                }

                if (paramGenMethod == null && paramField == null)
                {
                    MemberInfo[] membersWithSameName = 
                        type.GetMember(attr.Name, flags | BindingFlags.Instance);
                    if (membersWithSameName != null && membersWithSameName.Length > 0)
                        throw new ModelProgramUserException("Parameter generator " + attr.Name + " given by [Domain] attribute must be declared static.");
                    else
                        throw new ModelProgramUserException("Could not find parameter generator '" + attr.Name + "' specified by [Domain] attribute.");
                }
                else 
                {
                    return delegate()
                    {
                        object o = (paramField == null ?
                            paramGenMethod.Invoke(null, null) :
                            paramField.GetValue(null));
                        IEnumerable values = o as IEnumerable;
                        if (null == values)
                            throw new InvalidOperationException("Parameter generator " + attr.Name + " of wrong type or returned null"); // to do: make into user error

                        Set<Term> result = Set<Term>.EmptySet;
                        foreach (object value in values)
                            result = result.Add(AbstractValue.GetTerm((IComparable)value));

                        return result;
                    };
                }
            }
            else
            {
                throw new ModelProgramUserException("[Domain] attribute missing required name");
            }
        }

        private static bool IsValidElementTypeOfEnumerationType(Type enumType, Type elemType)
        {
            Type enumInterf = (enumType.Name == "IEnumerable`1" ? enumType : enumType.GetInterface("IEnumerable`1"));
            if (enumInterf == null)
                return false;
            Type[] paramTypes = enumInterf.GetGenericArguments();
            if (paramTypes != null && paramTypes.Length > 0 &&
                elemType.IsAssignableFrom(paramTypes[0]))
                return true;
            return false;
        }

        internal static ParameterGenerator/*?*/ GetThisParameterGenerator(MethodInfo methodInfo)
        {
            if (!methodInfo.IsStatic)
            {
                object[] attrs = methodInfo.DeclaringType.GetCustomAttributes(typeof(DomainAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    DomainAttribute attr = (DomainAttribute)attrs[0];
                    string attrName = attr.Name;
                    ParameterGenerator parameterGenerator;
                    if (string.Equals("new", attrName))
                    {
                        Symbol sort = AbstractValue.TypeSort(methodInfo.DeclaringType);
                        parameterGenerator = delegate() { return new Set<Term>(LabeledInstance.PeekNextLabelTerm(sort)); };
                    }
                    else
                        parameterGenerator = CreateDomainParameterGenerator(methodInfo, attr, methodInfo.DeclaringType);
                    return parameterGenerator;
                }
    
            }
            return null;
        }

        internal static ParameterGenerator[] GetInputParameterGenerators(MethodInfo methodInfo)
        {
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            int nParameters = parameterInfos.Length;
            ParameterGenerator/*?*/[] result = new ParameterGenerator[nParameters];
            
            for (int i = 0; i < nParameters; i += 1)
            {             
                ParameterInfo pInfo = parameterInfos[i];
                Type pType = pInfo.ParameterType;
                if (pType.IsByRef)
                {
                    pType = pType.GetElementType();
                }
                ParameterGenerator/*?*/ parameterGenerator = null;

                if (ReflectionHelper.IsInputParameter(pInfo))
                {
                    object/*?*/[]/*?*/ attrs1 = pInfo.GetCustomAttributes(typeof(DomainAttribute), true);
                    // object/*?*/[]/*?*/ attrs2 = pInfo.GetCustomAttributes(typeof(_Attribute), true);
                    // object/*?*/[]/*?*/ attrs3 = pInfo.GetCustomAttributes(typeof(NewAttribute), true);

                    bool hasDomainAttr = (attrs1 != null && attrs1.Length > 0);
                    
                    if (hasDomainAttr)
                    {
                        DomainAttribute attr = (DomainAttribute)attrs1[0];
                        string attrName = attr.Name;

                        if (string.Equals("new", attrName))
                        {
                            Symbol sort = AbstractValue.TypeSort(pType);
                            parameterGenerator = delegate() { return new Set<Term>(LabeledInstance.PeekNextLabelTerm(sort)); };
                        }
                        else
                            parameterGenerator = CreateDomainParameterGenerator(methodInfo, attr, pType);
                    }
                    else  
                    {
                        parameterGenerator = Method.GetDefaultParameterGenerator(pType);
                    }
                }
                result[i] = parameterGenerator;
            }
            return result;
        }

        static ParameterGenerator/*?*/ GetDefaultParameterGenerator(Type pType)
        {
            if (typeof(bool).Equals(pType))
            {
                Set<Term> values = new Set<Term>(AbstractValue.GetTerm(true), AbstractValue.GetTerm(false));
                return delegate() { return values; };
            }

            if (pType.IsSubclassOf(typeof(System.Enum)))
            {
                Set<Term> values = Set<Term>.EmptySet;
                foreach (System.Enum rawValue in System.Enum.GetValues(pType))
                    values = values.Add(AbstractValue.GetTerm(rawValue));

                return delegate() { return values; };
            }

            return null;
        }
    }
}
