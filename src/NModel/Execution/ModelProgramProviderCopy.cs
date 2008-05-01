using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Microsoft.Modeling.Execution
{
    public delegate IEnumerable<IList> ParameterGenerator(ActionSymbol symbol);

    /// <summary>
    /// Creates a model program from an assembly
    /// </summary>
    public sealed class ModelProgramProvider : IModelProgram, IControlPoint
    {
        Assembly modelAssembly;
        State initialState;

        StateField[] stateFields;
        StateVariable[] stateVariables;

        Dictionary<ActionSymbol, ActionInfo> actionMethodMap = 
            new Dictionary<ActionSymbol, ActionInfo>();
        ActionSymbol[] actionSymbols;
        /// <summary>
        /// Get MethodInfo associated with a given action symbol
        /// </summary>
        /// <param name="actionSymbol">Given action symbol</param>
        public MethodInfo GetMethodInfo(ActionSymbol actionSymbol)
        {
            ActionInfo actionInfo;
            if (actionMethodMap.TryGetValue(actionSymbol, out actionInfo))
                return actionInfo.method;
            return null;
        }

        // map from loaded enum types to corresponding internal representation
        Dictionary<Type, EnumDomain> domainMap = new Dictionary<Type, EnumDomain>();

        ParameterGenerator/*?*/ generator;
        /// <summary>
        /// External parameter generator used for generating action input arguments
        /// </summary>
        public ParameterGenerator/*?*/ ParameterGenerator
        {
            get
            {
                return generator;
            }
            set
            {
                generator = value;
            }
        }

        internal EnumDomain GetEnumDomain(Type t)
        {
            if (domainMap.ContainsKey(t)) 
                return domainMap[t];
            else
            {
                EnumDomain d = new EnumDomain(t);
                domainMap[t] = d;
                return d;
            }
        }

        /// <summary>
        /// Create an instance of ModelProgramProvider for a given assembly
        /// </summary>
        /// <param name="modelAssemblyFile">The full path to the assembly file.</param>
        /// <param name="generator">Optional parameter generator.</param>
        public ModelProgramProvider(string modelAssemblyFile, ParameterGenerator/*?*/ generator)
        {
            modelAssembly = Assembly.LoadFrom(modelAssemblyFile);
            this.generator = generator;
            Initialize();
            initialState = GetState();
        }

        /// <summary>
        /// Create an instance of ModelProgramProvider for a given assembly
        /// </summary>
        /// <param name="modelAssemblyFile">The full path to the assembly file.</param>
        public ModelProgramProvider(string modelAssemblyFile)
        {
            modelAssembly = Assembly.LoadFrom(modelAssemblyFile);
            Initialize();
            initialState = GetState();
        }

        void Initialize()
        {
            Type/*?*/[]/*?*/ allTypes = modelAssembly.GetTypes();
            List<StateField> stateVars = new List<StateField>() ;
            //extract those classes that have the Model attribute
            foreach (Type t in allTypes)
            {
                if (IsModelClass(t))
                {
                    //create an instance of the model class
                    object model = CreateModelInstance(t);

                    //create a state variable for all model variable fields
                    foreach (FieldInfo field in t.GetFields())
                    {
                        if (IsModelVariable(field))
                        {
                            stateVars.Add(new StateField(model,field));
                        }
                    }

                    //create actions
                    foreach (MethodInfo method in t.GetMethods())
                    {
                        if (IsModelActionSymbol(method))
                        {
                            //TBD : split up into a Start and a Finish action symbols
                            ActionInfo am = new ActionInfo(model, method, ActionKind.Atomic, this);
                            this.actionMethodMap[am.actionSymbol] = am;
                        }
                    }
                }
            }
            this.actionSymbols = new ActionSymbol[this.actionMethodMap.Keys.Count];
            int j =0;
            foreach (ActionSymbol a in actionMethodMap.Keys) 
               this.actionSymbols[j++] = a;

            this.stateFields = stateVars.ToArray();
            this.stateVariables = new StateVariable[this.stateFields.Length];
            for (int i = 0; i < this.stateVariables.Length; i++)
                this.stateVariables[i] = this.stateFields[i].stateVariable;
        }

        #region getting and setting state
        //TBD: Locations and State variables map one-to-one here
        //in general locations may be non-nullary (not supported here)
        void SetState(State state)
        {
            object[] vals = state.GetLocationValues();
            for (int i = 0; i < vals.Length; i++)
            {
                this.stateFields[i].Value = vals[i];
            }
        }

        State GetState()
        {
            object[] vals = new object[this.stateFields.Length];
            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = this.stateFields[i].Value;
            }
            return new State(vals);
        }
        #endregion

        #region private helper functions that use reflection
        private static bool IsModelActionSymbol(MethodInfo method)
        {
            object[] attrs = method.GetCustomAttributes(typeof(ModelActionAttribute), true);
            return (attrs != null && attrs.Length > 0);
        }

        private static bool IsModelVariable(FieldInfo field)
        {
            //only instance fields can be model variables
            if (field.IsStatic) return false;
            object/*?*/[]/*?*/ attrs = field.GetCustomAttributes(typeof(ModelVariableAttribute),true);
            return (attrs != null && attrs.Length > 0);
        }

        static object CreateModelInstance(Type t)
        {
            ConstructorInfo cinfo = t.GetConstructor(System.Type.EmptyTypes);
            return cinfo.Invoke(null);
        }

        static bool IsModelClass(Type t)
        {
            object/*?*/[]/*?*/ attrs = t.GetCustomAttributes(typeof(ModelAttribute), true);
            return (attrs != null && attrs.Length > 0);
        }
        #endregion

        #region IModelProgram Members

        public ActionSymbol[] ActionSymbols()
        {
            List<ActionSymbol> res = new List<ActionSymbol>(actionMethodMap.Keys);
            return res.ToArray();
        }

        public StateVariable[] StateVariables()
        {
            return stateVariables;
        }

        public IControlPoint InitialControlPoint
        {
            get { return this; }
        }

        public State InitialState
        {
            get { return initialState; }
        }

        #endregion

        #region IControlPoint Members

        public bool IsAccepting
        {
            get { return true ; }
        }

        public IModelProgram ModelProgram
        {
            get { return this; }
        }

        public bool IsPotentiallyEnabled(State state, ActionSymbol actionSymbol)
        {
            ActionInfo a = this.actionMethodMap[actionSymbol];
            SetState(state);
            return a.IsPotentiallyEnabled();
        }

        public bool IsEnabled(State state, Action action)
        {
            ActionInfo a = this.actionMethodMap[action.ActionSymbol];
            SetState(state);
            return a.IsEnabled(action.Arguments());
        }

        public IEnumerable<Action> GetActions(State state, ActionSymbol actionSymbol)
        {
            //enumerate over the parameter domains, check 
            //enabling conditions and yield the actions
            ActionInfo actionMethod = this.actionMethodMap[actionSymbol];
            SetState(state);
            List<Action> res = new List<Action>();
            foreach (object[] args in actionMethod.inputParameterCombinations.GetParameters())
            {
                if (actionMethod.IsEnabled(args))
                    res.Add(new Action(actionSymbol, args));
            }
            return res;
        }

        public IEnumerable<ModelStep> GetSteps(State state, params Action[] actions)
        {
            foreach (Action action in actions)
            {
                SetState(state);
                ActionInfo actionMethod = actionMethodMap[action.ActionSymbol];
                actionMethod.Execute(action.Arguments());
                State targetState = GetState();
                yield return new ModelStep(action, this, targetState);
            }
        }

        #endregion
    }

    /// <summary>
    /// A state variable that is an instance field of a .Net class
    /// </summary>
    internal class StateField
    {
        internal StateVariable stateVariable;
        object model;
        internal FieldInfo field;

        public StateField(object model, FieldInfo field)
        {
            this.stateVariable = new StateVariable(GetStateVariableName(field));
            this.model = model;
            this.field = field;
        }

        public object Value
        {
            get { return field.GetValue(model); }
            set { field.SetValue(model, value); }
        }

        static string GetStateVariableName(FieldInfo field)
        {
            return field.Name;
        }
    }

    /// <summary>
    /// Action symbol that maps to a .Net method
    /// </summary>
    internal class ActionInfo
    {
        internal ActionSymbol actionSymbol;
        internal object model;
        internal MethodInfo method;
        EnablingCondition parameterlessEnablingCondition;
        EnablingCondition enablingCondition;
        internal InputParameterCombinations inputParameterCombinations;
        internal Type[] inputParameterTypes;
        //isInputParameter[i] is true <==> the parameter i of the method is an input parameter
        internal ParameterInfo[] parameterInfos;  
        internal object[] defaultInputParameters;

        internal ActionInfo(object model, MethodInfo method, ActionKind kind, ModelProgramProvider mpp)
        {
            this.actionSymbol = new ActionSymbol(GetActionName(method), GetActionArity(method, kind), kind);
            this.model = model;
            this.method = method;
            this.inputParameterTypes = GetInputParameterTypes(method);
            this.parameterlessEnablingCondition = new EnablingCondition(this, true, true);
            this.enablingCondition = new EnablingCondition(this, true, false);
            this.parameterInfos = method.GetParameters();

            defaultInputParameters = new object[inputParameterTypes.Length];
            for (int i = 0; i < defaultInputParameters.Length; i++)
                defaultInputParameters[i] =
                    (mpp.GetEnumDomain(inputParameterTypes[i]).Values.Length == 0 ? 0 :
                     mpp.GetEnumDomain(inputParameterTypes[i]).Values[0]);

            //Create input parameter generators
            InitializeInputParameterCombinations(mpp, new EnablingCondition(this, false, false));
        }

        private void InitializeInputParameterCombinations(ModelProgramProvider mpp, EnablingCondition stateIndependentCondition)
        {
            List<InputParameterDomain> paramDomains = new List<InputParameterDomain>();
            foreach (ParameterInfo paramInfo in parameterInfos)
            {
                if (!paramInfo.IsOut)
                {
                    EnumDomain D = mpp.GetEnumDomain(paramInfo.ParameterType);
                    paramDomains.Add(new InputParameterDomain(D, IsParmeterPlaceholder(paramInfo)));
                }
            }
            this.inputParameterCombinations = new InputParameterCombinations(mpp.ParameterGenerator(this.actionSymbol), paramDomains.ToArray(), stateIndependentCondition);
        }

        static bool IsParmeterPlaceholder(ParameterInfo pInfo)
        {
            object[] attrs = pInfo.GetCustomAttributes(typeof(ModelParameterAttribute), true);
            if (attrs == null || attrs.Length == 0) return false;
            else
            {
                ModelParameterAttribute attr = attrs[0] as ModelParameterAttribute;
                return attr.Placeholder;
            }
        }

        internal bool IsPotentiallyEnabled()
        {
            return parameterlessEnablingCondition.Holds(null);
        }

        internal bool IsEnabled(object[] args)
        {
            return enablingCondition.Holds(args);
        }

        static string GetActionName(MethodInfo method)
        {
            //TBD: extract from Action attribute if provided
            return method.Name;
        }

        static int GetActionArity(MethodInfo method, ActionKind kind)
        {
            int arity = 0;
            ParameterInfo[] args = method.GetParameters();
            switch (kind)
            {
                case ActionKind.Start:
                    foreach (ParameterInfo arg in args)
                        if (!arg.IsOut) arity += 1;
                    break;
                case ActionKind.Finish:
                    foreach (ParameterInfo arg in args)
                        if (arg.IsOut) arity += 1;
                    //if (method.ReturnType != typeof(void))
                    arity += 1;
                    break;
                default: //return 
                    arity = args.Length;
                    break;
            }
            return arity;
        }

        internal void Execute(object[] inputParameters)
        {
            object[] allParameters = new object[parameterInfos.Length];
            int inputParamNr = 0;
            for (int paramNr = 0; paramNr < allParameters.Length; paramNr++)
            {
                if (!parameterInfos[paramNr].IsOut)
                {
                    if (inputParameters[inputParamNr] == Any.Value)
                        allParameters[paramNr] = defaultInputParameters[inputParamNr];
                    else
                        allParameters[paramNr] = inputParameters[inputParamNr];
                    inputParamNr += 1;
                }
            }
            //TBD: output parameters and return values
            //new control point is created where the 
            //only enabled action is the corresponding Finish action
            method.Invoke(model, allParameters);
        }

        internal static Type[] GetInputParameterTypes(MethodInfo actionMethod)
        {
            List<Type> pTypes = new List<Type>();
            foreach (ParameterInfo pInfo in actionMethod.GetParameters())
            {
                if (!pInfo.IsOut) pTypes.Add(pInfo.ParameterType);
            }
            return pTypes.ToArray();
        }

        internal static ModelRequirementAttribute[] GetModelRequirementAttributes(MemberInfo method)
        {
            object[] attrs =
                method.GetCustomAttributes(typeof(ModelRequirementAttribute), true);
            if (attrs == null || attrs.Length == 0)
                return new ModelRequirementAttribute[] { };
            else
                return attrs as ModelRequirementAttribute[];
        }
    }

    #region Parameter generation
    /// <summary>
    /// Represents a collection of enum values for a given enum
    /// </summary>
    internal class EnumDomain
    {
        object[] values;

        internal EnumDomain(Type T)
        //^ requires T.IsEnum;
        {
            List<object> vals = new List<object>();
            foreach (FieldInfo v in T.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                vals.Add(v.GetValue(null));
            }
            values = vals.ToArray();
        }
        internal object[] Values
        {
            get { return values; }
        }
    }

    /// <summary>
    /// Represents possible input argumets for a given action method
    /// </summary>
    internal class InputParameterDomain
    {
        internal object[] values;

        internal InputParameterDomain(EnumDomain domain, bool useAny)
        {
            if (domain.Values.Length <= 1)
                //it does not matter if useAny == true
                //because the domain has at most one element
                values = domain.Values;
            else if (useAny)
                //only use Any.Value that denotes an arbitrary element
                values = new object[] { Any.Value };
            else
                values = domain.Values;
        }
    }

    /// <summary>
    /// Provides sequences of input parameter combinations for a given action method
    /// </summary>
    internal class InputParameterCombinations
    {
        InputParameterDomain[] domains;
        List<object[]>/*?*/ precomputedParameterCombinations;
        EnablingCondition stateIndependentCondition;
        IEnumerable<IList>/*?*/ userProvidedParameterGenerator;

        internal InputParameterCombinations(IEnumerable<IList>/*?*/ userProvidedParameterGenerator, InputParameterDomain[] domains, EnablingCondition stateIndependentCondition)
        {
            this.userProvidedParameterGenerator = userProvidedParameterGenerator;
            this.domains = domains;
            this.stateIndependentCondition = stateIndependentCondition;
        }

        internal IEnumerable<object[]> GetParameters()
        {
            if (userProvidedParameterGenerator == null)
            {
                if (precomputedParameterCombinations == null)
                    PrecomputeParameterCombinations();
                foreach (object[] args in precomputedParameterCombinations)
                    yield return args;
            }
            else
            {
                foreach (IList argsList in userProvidedParameterGenerator)
                {
                    object[] args = new object[this.domains.Length];
                    argsList.CopyTo(args, 0);
                    if (stateIndependentCondition.Holds(args))
                        yield return args;
                }
            }
        }

        private void PrecomputeParameterCombinations()
        //^ ensures precomputedParameterCombinations != null;
        {
            int k = MaxNumberOfCombinations();
            precomputedParameterCombinations = new List<object[]>(k);
            if (k > 0)
            {
                int[] elemIDs = new int[domains.Length];
                do
                {
                    object[] args = new object[domains.Length];
                    for (int j = 0; j < domains.Length; j++)
                        args[j] = domains[j].values[elemIDs[j]];
                    if (stateIndependentCondition.Holds(args))
                        precomputedParameterCombinations.Add(args);
                } 
                while (IncrElemIDs(elemIDs));
            }
        }

        /// <summary>
        /// Increment in lexicographic order.
        /// Suppose there are 3 domains with sizes 3 x 2 x 3
        /// the generated elemIDs occur in the following order
        /// [0,0,0] -> [0,0,1] -> [0,0,2] -> 
        /// [0,1,0] -> [0,1,1] -> [0,1,2] -> 
        /// [1,0,0] -> [1,0,1] -> ...
        /// </summary>
        /// <param name="elemIDs">given sequence of element ids</param>
        /// <returns>true iff the sequence could be incremented</returns>
        private bool IncrElemIDs(int[] elemIDs)
        //^ requires elemIDs.Length == domains.Length;
        //^ requires elemIDs.Length > 0;
        {
            if (domains.Length == 0) return false;
            int i = elemIDs.Length - 1;
            while (elemIDs[i] == domains[i].values.Length -1)
            {
                elemIDs[i] = 0;
                i -= 1;
                if (i < 0) return false; //reached maximum
            }
            elemIDs[i] += 1;
            return true;
        }

        /// <summary>
        /// Maximum number of elements in the cartesian product of all domains.
        /// Returns 0 if one of the domains is empty.
        /// </summary>
        private int MaxNumberOfCombinations()
        //^ ensures result >= 0;
        {
            int res = 1;
            for (int i = 0; i < domains.Length; i++)
            {
                res = res * domains[i].values.Length;
            }
            return res;
        }
    }
    #endregion

    #region Enabling conditions

    internal class EnablingCondition
    {
        ActionInfo actionInfo;
        List<MethodInfo> predicates = new List<MethodInfo>();

        /// <summary>
        /// Creates an enabling condition for an action method.
        /// If the action method takes no input parameters or parameterless = true 
        /// then mayDependOnState should be true,
        /// otherwise the condition will either always hold or never hold.
        /// </summary>
        internal EnablingCondition(ActionInfo actionInfo, bool mayDependOnState, bool parameterless)
        {
            this.actionInfo = actionInfo;
            Type type = actionInfo.model.GetType();
            Type[] paramTypes = (parameterless ? Type.EmptyTypes : actionInfo.inputParameterTypes);
            foreach (ModelRequirementAttribute attr in ActionInfo.GetModelRequirementAttributes(actionInfo.method))
            {
                MethodInfo pred = type.GetMethod(attr.MethodName,paramTypes);
                if (pred != null && pred.ReturnType == typeof(bool) && 
                    (mayDependOnState || IsStateIndependent(pred)))
                {
                    predicates.Add(pred);
                }
            }
        }

        static private bool ContainsAnyValue(object[]/*?*/ arguments)
        {
            if (arguments == null) return false;
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == Any.Value) return true;
            }
            return false;
        }

        internal bool Holds(object[] arguments)
        {
            //replace possible Any.Value occurrences with default arguments
            object[] args;
            if (ContainsAnyValue(arguments))
            {
                args = new object[arguments.Length];
                for (int i = 0; i < arguments.Length; i++)
                    args[i] = (arguments[i] == Any.Value ? actionInfo.defaultInputParameters[i] : arguments[i]);
            }
            else
                args = arguments;
            //check each predicate
            foreach (MethodInfo pred in predicates)
                if (!((bool)pred.Invoke(actionInfo.model, args)))
                    return false;
            return true;
        }

        //TBD: use attribute to decide
        static private bool IsStateIndependent(MethodInfo method)
        {
            if (method == null) 
                return true; //dummy that uses method to avoid FxCop warning
            else
               return false;
        }

    }

    #endregion
}
