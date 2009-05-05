//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NModel.Execution;
using NModel.Attributes;
using NModel.Terms;

namespace NModel.Utilities
{
    /// <summary>
    /// Some helper functions commonly used to retrieve information through reflection.
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Flags used for accessing all members and types
        /// </summary>
        const BindingFlags modelBindingFlags = BindingFlags.Public | BindingFlags.NonPublic
                                               | BindingFlags.Static | BindingFlags.Instance;
        /// <summary>
        /// Find the given type in one of the libraries.
        /// </summary>
        public static Type FindType(ICollection<Assembly> libs, string typeName)//???
        {
            try
            {
                foreach (Assembly lib in libs)
                {
                    Type t = lib.GetType(typeName, false);
                    if (t != null)
                        return t;
                }
                throw new ModelProgramUserException("Type '" + typeName + "' was not found.");
            }
            catch (ModelProgramUserException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ModelProgramUserException(e.Message);
            }
        }

        /// <summary>
        /// Find the method with the given name, given input argument types in the given type, that returns a value that implements the given interface type or subtype
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "NModel.ConfTester.ConformanceTesterException.#ctor(System.String)")]
        public static MethodInfo FindMethod(Type t, string methName, Type[] argTypes, Type interfaceOrSubType)
        {
            if (t == null)
                throw new ArgumentNullException("t");
            if (interfaceOrSubType == null)
                throw new ArgumentNullException("interfaceType");

            try
            {
                MethodInfo meth = t.GetMethod(methName, argTypes);
                if (meth == null || !meth.IsStatic)
                {
                    StringBuilder inputTypes = new StringBuilder();
                    inputTypes.Append("(");
                    for (int i = 0; i < argTypes.Length; i++)
                    {
                        if (i > 0) inputTypes.Append(",");
                        inputTypes.Append(argTypes[i].Name);
                    }
                    inputTypes.Append(")");
                    throw new ModelProgramUserException("The type '" + t.ToString() + "' does not contain a static public method '" + methName + inputTypes.ToString() + "'.");
                }

                Type returnType = meth.ReturnType;

                if (interfaceOrSubType.IsAssignableFrom(returnType))
                {
                    return meth;
                }

                throw new ModelProgramUserException("The method '" + t + "." + methName + "' has a return type that does not extend '" + interfaceOrSubType.Name + "'.");

            }
            catch (ModelProgramUserException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ModelProgramUserException(e.Message);
            }

        }

        /// <summary>
        /// Splits a fully qualified name typeName.methodName of a static method to its constituents,
        /// where typeName and methodName are nonempty strings.
        /// Assumes that fullName != null.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "NModel.ConfTester.ConformanceTesterException.#ctor(System.String)")]
        public static void SplitFullMethodName(string fullName, out string typeName, out string methodName)
        {
            if (fullName == null)
                throw new ArgumentNullException("fullName");

            int k = fullName.LastIndexOf(".");
            if (k <= 0 || k >= fullName.Length - 1)
                throw new ModelProgramUserException("Not a valid fully qualified static method name '" + fullName + "'.");

            typeName = fullName.Substring(0, k);
            methodName = fullName.Substring(k + 1);
        }

        /// <summary>
        /// Is <paramref name="m"/> an internal member like an iterator generated by the compiler?
        /// </summary>
        /// <param name="m">member info</param>
        /// <returns>True if <paramref name="m"/> is an internal, compiler-generated member info.</returns>
        public static bool IsCompilerGenerated(MemberInfo m)
        {
            if (null == m) return false;
            object[] attrs = m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true);
            return (attrs != null && attrs.Length > 0);
        }


        #region Reflection Helpers

        /// <summary>
        /// Enumerate methods of type <paramref name="t"/> that have an associated <c>ActionAttribute</c>.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMethodsForActions(Type t)
        {
            foreach (MethodInfo methodInfo in t.GetMethods(modelBindingFlags))
                if (ReflectionHelper.HasActionAttribute(methodInfo))
                    yield return methodInfo;
        }

        /// <summary>
        /// Does <paramref name="method"/> have at least one [Action] attribute?
        /// </summary>
        /// <param name="method">The method</param>
        /// <returns>True if <paramref name="method"/> is an action method</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static bool HasActionAttribute(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            object/*?*/[]/*?*/ attrs = method.GetCustomAttributes(typeof(ActionAttribute), true);
            return (attrs != null && attrs.Length > 0);
        }

        /// <summary>
        /// Iterates through the [Action] attributes of <paramref name="method"/>
        /// </summary>
        /// <param name="method">The method</param>
        /// <returns>Enumerated [Action] attributes</returns>
        public static IEnumerable<ActionAttribute> GetModelActionAttributes(MethodInfo method)
        {
            object[] attrs = method.GetCustomAttributes(typeof(ActionAttribute), true);
            if (attrs != null)
                foreach (object attr in attrs)
                    if (null != attr)
                        yield return (ActionAttribute)attr;
        }

        static readonly char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// Removes trailing digits 0..9 from a string
        /// </summary>
        /// <param name="s">The string</param>
        /// <returns>The trimmed string</returns>
        static string TrimDigits(string s)
        {
            return s.TrimEnd(digits);
        }

        /// <summary>
        /// Iterates through the enabling condition methods of <paramref name="actionMethod"/>. Each
        /// enabling condition is any method defined in the same class as <paramref name="actionMethod"/>
        /// whose name is <paramref name="actionMethod"/>.Name + "Enabled" + zero or more digits. Enabling conditions methods
        /// must return type <c>System.Boolean</c> and have a parameter list whose types are a prefix
        /// of <paramref name="inputParameters"/>
        /// </summary>
        /// <param name="actionMethod">The action method</param>
        /// <param name="inputParameterTypes">The input parameter types of <paramref name="actionMethod"/>, including
        /// the implicit <c>this</c> parameter if <paramref name="actionMethod"/> is an instance method.</param>
        /// <returns>Enumerated enabling condition methods.</returns>
        public static IEnumerable<MethodInfo> GetEnablingConditionMethods(MethodInfo actionMethod, Type[] inputParameterTypes)
        {
            Type t = actionMethod.DeclaringType;
            string enablingConditionName = actionMethod.Name + "Enabled";

            foreach (MethodInfo methodInfo in t.GetMethods(modelBindingFlags))
            {
                string trimmedMethodName = ReflectionHelper.TrimDigits(methodInfo.Name);
                if (string.Equals(trimmedMethodName, enablingConditionName))
                {
                    if (ReflectionHelper.HasActionAttribute(methodInfo))
                        throw new ModelProgramUserException("Enabling condition " + enablingConditionName + " must not have an [Action] attribute.");

                    if (!methodInfo.ReturnType.Equals(typeof(bool)))
                        throw new ModelProgramUserException("Enabling condition " + enablingConditionName + " must have Boolean return type");

                    if (!ReflectionHelper.ParametersArePrefix(methodInfo, inputParameterTypes))
                        throw new ModelProgramUserException("Enabling condition " + enablingConditionName + " does not match the input parameter types of " + actionMethod.Name);

                    yield return methodInfo;
                }
            }
        }

        /// <summary>
        /// Get descriptions of all requirement attributes attached to the member info.
        /// </summary>
        public static string[] GetEnablingConditionDocumentation(MemberInfo method)
        {
            List<string> documentationStrings = new List<string>();
            object[] attrs = method.GetCustomAttributes(typeof(RequirementAttribute), true);
            if (attrs != null)
                foreach (object attr in attrs)
                    if (null != attr)
                        documentationStrings.Add(((RequirementAttribute)attr).Documentation);
            return documentationStrings.ToArray();
        }

        // Requirements metrics     

        /// <summary>
        /// Get ids and descriptions of all requirement attributes attached to the member info.
        /// </summary>
        public static Pair<string, string>[] GetEnablingMethodsRequirements(MemberInfo method)
        {
            List<Pair<string, string>> reqs = new List<Pair<string, string>>();
            object[] attrs = method.GetCustomAttributes(typeof(RequirementAttribute), true);
            if (attrs != null)
                foreach (object attr in attrs)
                    if (null != attr)
                        reqs.Add(new Pair<string, string>(
                            ((RequirementAttribute)attr).Id, ((RequirementAttribute)attr).Documentation));
            return reqs.ToArray();
        }

        /// <summary>
        /// Return the enabling methods for a given action-method
        /// </summary>
        /// <param name="actionMethod"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetEnablingMethods(MethodInfo actionMethod)
        {
            Type t = actionMethod.DeclaringType;
            string enablingConditionName = actionMethod.Name + "Enabled";

            foreach (MethodInfo methodInfo in t.GetMethods(modelBindingFlags))
            {
                string trimmedMethodName = ReflectionHelper.TrimDigits(methodInfo.Name);
                if (string.Equals(trimmedMethodName, enablingConditionName))
                {
                    if (ReflectionHelper.HasActionAttribute(methodInfo))
                        throw new ModelProgramUserException("Enabling condition " + enablingConditionName + " must not have an [Action] attribute.");

                    if (!methodInfo.ReturnType.Equals(typeof(bool)))
                        throw new ModelProgramUserException("Enabling condition " + enablingConditionName + " must have Boolean return type");

                    yield return methodInfo;
                }
            }
        }

        /// <summary>
        /// Get ids and descriptions of all requirement attributes attached to the member info.
        /// </summary>
        public static Set<Pair<string, string>> GetRequirementsInMethod(MemberInfo method)
        {
            Set<Pair<string, string>> methodReqs = Set<Pair<string, string>>.EmptySet;
            object[] attrs = method.GetCustomAttributes(typeof(RequirementAttribute), true);
            if (attrs != null)
                foreach (object attr in attrs)
                    if (null != attr)
                        methodReqs = methodReqs.Add(new Pair<string, string>(
                            ((RequirementAttribute)attr).Id, ((RequirementAttribute)attr).Documentation));
            return methodReqs;
        }

        /// <summary>
        /// Are the input parameter types of <paramref name="methodInfo"/> a prefix of 
        /// the sequence <paramref name="inputParameterTypes"/>?
        /// </summary>
        /// <param name="methodInfo">The method</param>
        /// <param name="inputParameterTypes">The input types</param>
        /// <returns>True if prefix, false otherwise</returns>
        static bool ParametersArePrefix(MethodInfo methodInfo, Type[] inputParameterTypes)
        {
            Type[] types = ReflectionHelper.GetInputParameterTypes(methodInfo);

            if (types.Length > inputParameterTypes.Length)
                return false;

            for (int i = 0; i < types.Length; i += 1)
            {
                if (!Object.Equals(types[i], inputParameterTypes[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Does the method have a nonvoid return value, out parameters or byref parameters? In other words,
        /// does the method return any outputs?
        /// </summary>
        /// <param name="method">The method</param>
        /// <returns>True if there are no outputs, false if some outputs exist.</returns>
        public static bool HasNoOutputs(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            object/*?*/[]/*?*/ attrs = method.GetCustomAttributes(typeof(SplitAttribute), true);
            if (attrs != null && attrs.Length > 0)
                return false;

            if (!method.ReturnType.Equals(typeof(void)))
                return false;

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters != null)
            {
                foreach (ParameterInfo p in parameters)
                {
                    if (IsOutputParameter(p))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Is the parameter an output or byref parameter? 
        /// </summary>
        /// <param name="pInfo">The parameter</param>
        /// <returns>True if the parameter is an output</returns>
        /// <remarks>Note:in the case of a ref parameter it is both input and output.</remarks>
        public static bool IsOutputParameter(ParameterInfo pInfo)
        {
            if (pInfo == null)
                throw new ArgumentNullException("pInfo");

            Type pType = (Type)pInfo.ParameterType;
            return (pInfo.IsOut || pType.IsByRef);
        }

        // The list of the state variables that should be hidden from the viewer
        private static List<string> hiddenVars = new List<string>();
        /// <summary>
        /// The list of the state variables that should be hidden from the viewer
        /// </summary>
        public static List<string> HiddenVars
        {
            get { return ReflectionHelper.hiddenVars; }
        }


        /// <summary>
        /// Is the field a model variable?
        /// </summary>
        /// <param name="field">The field</param>
        /// <returns>True, if <paramref name="field"/> is a model variable.</returns>
        /// <remarks>A field is a model variable if all of the following conditions hold
        /// <list>
        /// <item>there is no [ExcludeFromState] attribute</item>
        /// <item>its type is not a compiler-generated type</item>
        /// <item>the field is not a compile-time constant</item>
        /// <item>the field is not a static readonly field</item>
        /// <item>the field is not a member of a compound value</item>
        /// </list>
        /// </remarks>
        public static bool IsModelVariable(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            object/*?*/[]/*?*/ attrs = field.GetCustomAttributes(typeof(ExcludeFromStateAttribute), true);
            object[] attrs2 = field.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true);
            bool isExcluded = attrs != null && attrs.Length > 0;

            // Fill the list of the state variables that should be hidden from the viewer
            object/*?*/[]/*?*/ hidenAttrs = field.GetCustomAttributes(typeof(HideFromViewerAttribute), true);
            if (hidenAttrs != null && hidenAttrs.Length > 0)
                hiddenVars.Add(field.Name);

            bool isCompilerGeneratedField = attrs2 != null && attrs2.Length > 0;
            bool isLiteral = field.IsLiteral;
            bool isStaticReadonly = field.IsInitOnly && field.IsStatic;
            bool isCompoundValueField = field.DeclaringType.IsSubclassOf(typeof(CompoundValue));
            return !isCompoundValueField && !isCompilerGeneratedField && !isExcluded
                && !isLiteral && !isStaticReadonly;
        }

        /// <summary>
        /// Iterate through the model variables of a class.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetModelVariables(Type t)
        {
            if (t.IsClass && !t.IsSubclassOf(typeof(CompoundValue)))
                foreach (FieldInfo field in t.GetFields(modelBindingFlags))
                {
                    if (ReflectionHelper.IsModelVariable(field))
                    {
                        if (field.FieldType.IsPrimitive ||
                             field.FieldType.IsEnum ||
                             field.FieldType == typeof(string) ||
                             ReflectionHelper.ImplementsIAbstractValue(field.FieldType))
                            yield return field;
                        else
                            throw new ModelProgramUserException(
                                "\nThe field '" + t.FullName + "." + field.Name + "' does not a have valid modeling type. " +
                                "\nA valid modeling type is either: a primitive type, an enum, a string, or a type that implements 'NModel.Internals.IAbstractValue'." +
                                "\nIn particular, collection types in 'System.Collections' and 'System.Collections.Generic' are not valid modeling types." +
                                "\nValid modeling types are collection types like 'Set' and 'Map' defined in the 'NModel' namespace, " +
                                "\nas well as user defined types that derive from 'CompoundValue'." +
                                "\nNB! If the field is not supposed to be a model variable, attach the attribute [ExcludeFromState] to it.");
                    }
                }
        }

        internal static bool ImplementsIAbstractValue(Type type)
        {
            foreach (Type t in type.GetInterfaces())
                if (t == typeof(NModel.Internals.IAbstractValue))
                    return true;
            return false;
        }

        /// <summary>
        /// Is the type <paramref name="t"/> included in the model <paramref name="modelName"/>?
        /// Additionally, if <paramref name="t"/> has one or more [Feature] attributes and 
        /// <paramref name="featureNames"/> is nonnull, does <paramref name="t"/> have a 
        /// [Feature] attribute whose name is found in <paramref name="featureNames"/>?
        /// </summary>
        /// <param name="t">The type to be tested</param>
        /// <param name="modelName">The name of the model</param>
        /// <param name="featureNames">Null if all features are to be included, or a nonnull set of
        /// feature names to be included.</param>
        /// <returns>True if <paramref name="t"/> is part of the model and feature set.</returns>
        public static bool IsInModel(Type t, string modelName, Set<string>/*?*/ featureNames)
        {
            if (!t.Namespace.Equals(modelName))
            {
                // Namespace does not does not match the model name: exclude the class
                return false;
            }
            else
            {
                // Class namespace matches model name and null featureNames:
                // include the class. (In other words, just ignore any [Feature] attribute; load all.)
                if (null == featureNames)
                    return true;
                else
                {
                    // Check for matching features
                    object/*?*/[]/*?*/ attrs = t.GetCustomAttributes(typeof(FeatureAttribute), true);
                    if (attrs != null && attrs.Length > 0)
                    {
                        foreach (object attrObj in attrs)
                        {
                            FeatureAttribute attr = (FeatureAttribute)attrObj;
                            string name = (string.IsNullOrEmpty(attr.Name) ? t.Name : attr.Name);

                            // Class has a matching [Feature] attribute: the name of the feature must
                            // be in the set of featureNames in order for it to be in the model.
                            if (featureNames.Contains(name)) return true;
                        }
                        // No matching feature attributes found.
                        return false;
                    }
                    else
                    {
                        // The class does not have any [Feature] attributes: include it.
                        return true;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the start and end action labels from an [Action] attribute
        /// </summary>
        /// <param name="method">The action method</param>
        /// <param name="actionAttribute">The action attribute of <paramref name="method"/></param>
        /// <param name="startActionLabel">Output parameter that returns the calculated start action label</param>
        /// <param name="finishActionLabel">Output parameter that returns the calculated finish action label. Null 
        /// if there is no fnish action label.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public static void GetActionLabel(MethodInfo method, ActionAttribute actionAttribute, out CompoundTerm startActionLabel, out CompoundTerm/*?*/ finishActionLabel)
        {
            // TODO: extend this so that explict start/finish names may be given.
            // Currently, this implements only part of the specification. We need
            // to support named parameters and explicit start/finish names.
            if (!string.IsNullOrEmpty(actionAttribute.Name) && !string.IsNullOrEmpty(actionAttribute.Start))
                throw new ModelProgramUserException("can't have positional argument name " + actionAttribute.Name + " and start action property " +
                                                    actionAttribute.Start + " specified in the same [Action] attribute.");

            if (string.IsNullOrEmpty(actionAttribute.Start) != string.IsNullOrEmpty(actionAttribute.Finish))
                if (!string.IsNullOrEmpty(actionAttribute.Start))
                    throw new ModelProgramUserException("can't have start property " + actionAttribute.Start +
                                                        " in [Action] attribute without matching Finish property");
                else
                    throw new ModelProgramUserException("can't have finish property " + actionAttribute.Finish +
                                                        " in [Action] attribute without matching Start property");


            bool isAtomic = ReflectionHelper.HasNoOutputs(method) && string.IsNullOrEmpty(actionAttribute.Finish);

            // if method has outputs and you want to provide individual parameters, you must use Start/Finish
            if (!isAtomic && !string.IsNullOrEmpty(actionAttribute.Name) &&
                actionAttribute.Name.Contains("("))
                throw new ModelProgramUserException("must provide explict Start/Finish properties for nonvoid action method " + actionAttribute.Name);

            string baseName = string.IsNullOrEmpty(actionAttribute.Name) ? method.Name : actionAttribute.Name;
            string startActionString = (!string.IsNullOrEmpty(actionAttribute.Start) ?
                                               actionAttribute.Start :
                                               (isAtomic ? baseName : (baseName + "_Start")));

            string[] inputParameterNames = GetInputParameterNames(method);
            startActionLabel = GetActionTerm(startActionString, inputParameterNames, true, Set<string>.EmptySet);


            if (!isAtomic)
            {
                string finishActionString = (!string.IsNullOrEmpty(actionAttribute.Finish) ?
                                                 actionAttribute.Finish : baseName + "_Finish");

                Set<string> optionalArguments = new Set<string>(inputParameterNames);
                finishActionLabel = GetActionTerm(finishActionString, GetOutputParameterNames(method), false, optionalArguments);
            }
            else
            {
                finishActionLabel = null;
            }
        }

        static Set<string> GetUnusedArguments(Set<string> defaultArguments, CompoundTerm action)
        {
            Set<string> result = defaultArguments;
            foreach (Term arg in action.Arguments)
            {
                if (!arg.Equals(Any.Value))
                {
                    Variable v = arg as Variable;
                    if (null == v)
                        throw new ModelProgramUserException("invalid argument for action " + action.ToString() +
                              ": " + arg.ToString());
                    string name = v.ToString();
                    if (!defaultArguments.Contains(name))
                        throw new ModelProgramUserException("invalid (possibly misspelled) argument for action " + action.ToString() +
                              ": " + arg.ToString() + ". Must be one of " + defaultArguments.ToString() + ".");

                    result = result.Remove(name);
                }
            }
            return result;
        }

        static void CheckForDuplicateArgument(CompoundTerm action)
        {
            Set<Term> argsSoFar = Set<Term>.EmptySet;

            foreach (Term arg in action.Arguments)
            {
                if (argsSoFar.Contains(arg))
                    throw new ModelProgramUserException("action label " + action.ToString() + " contains duplicate argument " + arg.ToString());
                else if (!Any.Value.Equals(arg))
                {
                    argsSoFar = argsSoFar.Add(arg);
                }
            }

        }

        static CompoundTerm GetActionTerm(string actionString, string[] defaultArguments, bool mustUseAllNames,
                                          Set<string> optionalArguments)
        {
            Set<string> possibleArguments = new Set<string>(defaultArguments).Union(optionalArguments);

            if (actionString.Contains("(") || actionString.Contains(")"))
            {
                try
                {
                    Term t = Term.Parse(actionString);
                    CompoundTerm ct = t as CompoundTerm;
                    if (null == ct)
                        throw new ModelProgramUserException("invalid action label syntax: " + actionString);
                    Set<string> unusedArguments = GetUnusedArguments(possibleArguments, ct);
                    Set<string> missingArguments = (mustUseAllNames ? unusedArguments.Difference(optionalArguments) : Set<string>.EmptySet);
                    if (mustUseAllNames && !missingArguments.IsEmpty)
                        throw new ModelProgramUserException("action label " + actionString +
                            " is missing required argument(s). Missing value(s) are: " + missingArguments.ToString());
                    if (mustUseAllNames)
                        CheckForDuplicateArgument(ct);
                    return ct;
                }
                catch (ArgumentException e)
                {
                    throw new ModelProgramUserException("invalid action label syntax: " + actionString +
                        ". Could not parse term: " + e.Message);
                }
            }
            else
            {
                int arity = defaultArguments.Length;
                Term[] args = new Term[arity];
                for (int i = 0; i < arity; i += 1)
                    args[i] = new Variable(defaultArguments[i]);
                return new CompoundTerm(new Symbol(actionString), args);
            }
        }

        /// <summary>
        /// Does the parameter provide an input?
        /// </summary>
        /// <param name="pInfo">The parameter</param>
        /// <returns>True if the parameter provides an input.</returns>
        public static bool IsInputParameter(ParameterInfo pInfo)
        {
            if (pInfo == null)
                throw new ArgumentNullException("pInfo");

            return !pInfo.IsOut;
        }

        /// <summary>
        /// Get an array of types that represents the inputs to the method.
        /// </summary>
        /// <param name="methodInfo">The method</param>
        /// <returns>An array of input types, in the same order as their corresponding parameters. The implicit
        /// <c>this</c> argument is included in the array for instance methods.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static Type[] GetInputParameterTypes(MethodInfo methodInfo)
        {
            List<Type> pTypes = new List<Type>();

            if (!methodInfo.IsStatic)
                pTypes.Add(methodInfo.DeclaringType);

            foreach (ParameterInfo pInfo in methodInfo.GetParameters())
            {
                Type pType = pInfo.ParameterType;
                if (ReflectionHelper.IsInputParameter(pInfo))
                    pTypes.Add(pType.IsByRef ? pType.GetElementType() : pType);
            }
            return pTypes.ToArray();
        }

        /// <summary>
        /// Get an array of names that identify the inputs to the method.
        /// </summary>
        /// <param name="methodInfo">The method</param>
        /// <returns>An array of string names, in the same order as their corresponding parameters. The implicit
        /// <c>this</c> argument is included in the array for instance methods.</returns>
        static string[] GetInputParameterNames(MethodInfo methodInfo)
        {
            List<string> parameterNames = new List<string>();

            if (!methodInfo.IsStatic)
                parameterNames.Add("this");

            foreach (ParameterInfo pInfo in methodInfo.GetParameters())
            {
                //Type pType = pInfo.ParameterType;
                if (ReflectionHelper.IsInputParameter(pInfo))
                    parameterNames.Add(pInfo.Name);
            }
            return parameterNames.ToArray();
        }

        /// <summary>
        /// Get an array of types that represents the outputs of the method
        /// </summary>
        /// <param name="actionMethod">The method</param>
        /// <returns>An array of output types, return value type followed by the output parameter types, 
        /// in parameter-list order.</returns>
        public static Type[] GetOutputParameterTypes(MethodInfo actionMethod)
        {
            List<Type> pTypes = new List<Type>();

            if (!actionMethod.ReturnType.Equals(typeof(void)))
                pTypes.Add(actionMethod.ReturnType);

            foreach (ParameterInfo pInfo in actionMethod.GetParameters())
            {
                Type pType = pInfo.ParameterType;
                bool isByRef = pType.IsByRef;

                if (IsOutputParameter(pInfo))
                    pTypes.Add(isByRef ? pType.GetElementType() : pType);
            }

            return pTypes.ToArray();
        }

        /// <summary>
        /// Get an array of names that identify the outputs to the method.
        /// </summary>
        /// <param name="methodInfo">The method</param>
        /// <returns>An array of string names, in the same order as their corresponding parameters. The return
        /// value (if present) is given the name "result".</returns>
        static string[] GetOutputParameterNames(MethodInfo methodInfo)
        {
            List<string> parameterNames = new List<string>();

            if (!methodInfo.ReturnType.Equals(typeof(void)))
                parameterNames.Add("result");

            foreach (ParameterInfo pInfo in methodInfo.GetParameters())
            {
                if (ReflectionHelper.IsOutputParameter(pInfo))
                    parameterNames.Add(pInfo.Name);
            }
            return parameterNames.ToArray();
        }

        /// <summary>
        /// Computes the mapping from the parameter positions of the action label to the 
        /// parameter positions of the .NET method that implements the action.
        /// </summary>
        /// <param name="actionLabel"></param>
        /// <param name="actionMethod"></param>
        /// <returns>An array of integers where each entry is the index in the parameter list
        /// of the .NET method. The special value -1 is used to indicate the implicit "this"
        /// argument of an instance method.</returns>
        internal static int[] GetInputParameterIndices(CompoundTerm actionLabel, MethodInfo actionMethod)
        {
            List<int> parameterIndices = new List<int>();

            foreach (Term arg in actionLabel.Arguments)
            {
                if (Any.Value == arg)
                    parameterIndices.Add(-2);
                else
                {
                    Variable v = arg as Variable;
                    if (null != v)
                    {
                        string name = v.ToString();
                        if ("this".Equals(name))
                            parameterIndices.Add(-1);

                        else
                        {
                            int index = 0;
                            bool foundMatch = false;
                            foreach (ParameterInfo pInfo in actionMethod.GetParameters())
                            {
                                if (ReflectionHelper.IsInputParameter(pInfo) && pInfo.Name.Equals(name))
                                {
                                    parameterIndices.Add(index);
                                    foundMatch = true;
                                    break;
                                }
                                index += 1;
                            }
                            if (!foundMatch)
                                throw new ModelProgramUserException("action label " + actionLabel.ToString() +
                                                                     " includes unrecognized input argument " + name);

                        }
                    }
                    else
                    {
                        throw new ModelProgramUserException("action label " + actionLabel.ToString() +
                             " may not include ground term " + arg.ToString());
                    }
                }
            }

            return parameterIndices.ToArray();
        }


        /// <summary>
        /// Computes the mapping from the parameter positions of the action label to the 
        /// output parameter positions of the .NET method that implements the action.
        /// </summary>
        /// <param name="actionLabel">The finish action label</param>
        /// <param name="actionMethod">The action method</param>
        /// <returns>An array of integers where each entry is the index in the parameter list
        /// of the .NET method. The special value -1 is used to indicate the position of the the
        /// return value. The special value -2 is used to indicate an ignored argument.</returns>
        internal static int[] GetOutputParameterIndices(CompoundTerm actionLabel, MethodInfo actionMethod)
        {
            List<int> parameterIndices = new List<int>();

            foreach (Term arg in actionLabel.Arguments)
            {
                if (Any.Value == arg)
                    parameterIndices.Add(-2);
                else
                {
                    Variable v = arg as Variable;
                    if (null != v)
                    {
                        string name = v.ToString();
                        if ("result".Equals(name))
                            parameterIndices.Add(-1);

                        else
                        {
                            int index = 0;
                            bool foundMatch = false;
                            foreach (ParameterInfo pInfo in actionMethod.GetParameters())
                            {
                                if (pInfo.Name.Equals(name))
                                {
                                    parameterIndices.Add(index);
                                    foundMatch = true;
                                    break;
                                }
                                index += 1;
                            }
                            // this test is dead code (was previously checked).
                            if (!foundMatch)
                                throw new ModelProgramUserException("action label " + actionLabel.ToString() +
                                                                     " includes unrecognized output argument " + name);

                        }
                    }
                    else
                    {
                        throw new ModelProgramUserException("action label " + actionLabel.ToString() +
                             " may not include ground term " + arg.ToString());
                    }
                }
            }

            return parameterIndices.ToArray();
        }
        #endregion

        /// <summary>
        /// Find the first assembly from the given list of assemblies that contains the given model program (namespace).
        /// </summary>
        /// <param name="assemblies">given assemblies</param>
        /// <param name="mp">given model program name (namespace)</param>
        /// <returns>The assembly that contains the given model program</returns>
        public static Assembly FindAssembly(List<Assembly> assemblies, string mp)
        {
            foreach (Assembly a in assemblies)
            {
                foreach (Type t in a.GetTypes())
                {
                    if (t.Namespace == mp)
                        return a;
                }
            }
            throw new ModelProgramUserException("The model program '" + mp + "' was not found.");
        }
    }
}
