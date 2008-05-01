//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NModel.Algorithms;
using NModel.Terms;
using NModel.Internals;
using NModel.Attributes;
using NModel.Utilities;


namespace NModel.Execution
{
    /// <summary>
    /// Represents the kind of an action
    /// </summary>
    public enum ActionKind
    {
        /// <summary>
        /// Atomic action
        /// </summary>
        Atomic,
        /// <summary>
        /// Start action
        /// </summary>
        Start,
        /// <summary>
        /// Finish action
        /// </summary>
        Finish,
    }

    /// <summary>
    /// Creates a model program from an assembly
    /// </summary>
    public sealed class LibraryModelProgram : ModelProgram, IName
    {
        #region Fields

        /// <summary>
        /// Model name given by attribute. A model is a collection of class declarations 
        /// have the same the same [Model] attribute.
        /// </summary>
        readonly string name;

        /// <summary>
        /// .NET assembly that contains this model
        /// </summary>
        readonly Assembly modelAssembly;

        /// <summary>
        /// Runtime environment (or context) that allows terms to be interpreted with respect
        /// to this model program. For example, the context shows what type corresponds to a given 
        /// model sort.
        /// </summary>
        InterpretationContext context;

        /// <summary>
        /// "Dirty bit" used to indicate that the .NET state no longer matches the values given by the
        /// current state field.
        /// </summary>
        bool stateChangedPredicate = false;

        /// <summary>
        /// The most recent argument to "SetState". 
        /// </summary>
        IState/*?*/ currentState;

        /// <summary>
        /// Table of enabled actions for states with continuations given by suspension points.
        /// A state's control mode is the "readyControlMode" iff there are no continuations.
        /// </summary>
        Dictionary<IState, Dictionary<CompoundTerm, IState>> continuations = new Dictionary<IState, Dictionary<CompoundTerm, IState>>();

        /// <summary>
        /// The control mode that indicates that any enabled action may occur.
        /// </summary>
        static readonly Term readyControlMode = AbstractValue.GetTerm(Sequence<CompoundTerm>.EmptySequence);

        /// <summary>
        /// The .NET fields that form the basis of states of this model program. The ith state variable 
        /// corresponds to the ith instance field
        /// </summary>
        readonly Field[] stateFields;

        /// <summary>
        /// The name of each location of state. This matches positionally with stateFields 
        /// and stateVariables. This is a cache-- it is always equal to the names found in 
        /// stateVariables.
        /// </summary>
        readonly ValueArray<string> locationNames;

        /// <summary>
        /// Table of location names and sorts that make up state. 
        /// </summary>
        readonly StateVariable[] stateVariables;

        /// <summary>
        /// Table of actions to .NET reflection info. All actions (Start, Atomic and Finish) 
        /// are representated in the table. 
        /// </summary>
        readonly internal Dictionary<Symbol, ActionInfo> actionInfoMap;

        /// <summary>
        /// Cache: dictionary mapping start action symbols to corresponding finish action symbols
        /// </summary>
        readonly Dictionary<Symbol, Symbol> finishActionSymbols;

        /// <summary>
        /// The vocabulary of actions for this model program
        /// </summary>
        readonly Set<Symbol> actionSymbols;

        /// <summary>
        /// Accepting state conditions
        /// </summary>
        Dictionary<Type, StatePredicate> acceptingStateConditions;

        /// <summary>
        /// State invariants
        /// </summary>
        Dictionary<Type, StatePredicate> stateInvariants;

        /// <summary>
        /// State filters
        /// </summary>
        Dictionary<Type, StatePredicate> stateFilters;

        /// <summary>
        /// Field for caching the initial state;
        /// </summary>
        private IState initialState = null;

        /// <summary>
        /// A dictionary that to distinguish which sort is abstract.
        /// </summary>
        private Set<Symbol> abstractSorts;

        #endregion

        #region Queries
        /// <summary>
        /// Get the model assembly
        /// </summary>
        public Assembly ModelAssembly
        {
            get { return modelAssembly; }
        }

        /// <summary>
        /// Returns the model name of this model program.
        /// </summary>
        public string Name { get { return this.name; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Create an instance of LibraryModelProgram for a given assembly
        /// </summary>
        /// <param name="modelAssemblyFile">The full path to the assembly file.</param>
        /// <param name="modelName">Name of the model namespace to be loaded. 
        /// Only classes in the model namespace will be loaded.</param>
        /// <param name="featureNames">The names of features to be loaded. If null, all
        /// features will be loaded for the given modelName. See <see cref="FeatureAttribute"/>.</param>
        /// <exception cref="ModelProgramUserException">Thrown if there is a usage error in the given assembly.</exception>
        public LibraryModelProgram(string modelAssemblyFile, string/*?*/ modelName, Set<string>/*?*/ featureNames) :
             this(Assembly.LoadFrom(modelAssemblyFile), modelName, featureNames) { }
       
        /// <summary>
        /// Create an instance of LibraryModelProgram for a given assembly
        /// </summary>
        /// <param name="modelAssemblyFile">The full path to the assembly file.</param>
        /// <param name="modelName">Name of the model namespace to be loaded. 
        /// Only classes in the model namespace will be loaded.</param>
        /// <exception cref="ModelProgramUserException">Thrown if there is a usage error in the given assembly.</exception>
        public LibraryModelProgram(string modelAssemblyFile, string/*?*/ modelName) : 
            this(Assembly.LoadFrom(modelAssemblyFile), modelName, null) {}

        /// <summary>
        /// Create an instance of LibraryModelProgram for a given assembly
        /// </summary>
        /// <param name="modAssembly">Loaded assembly</param>
        /// <param name="modelName">Name of the model namespace to be loaded. 
        /// Only classes in the model namespace will be loaded.</param>
        /// <exception cref="ModelProgramUserException">Thrown if there is a usage error in the given assembly.</exception>
        public LibraryModelProgram(Assembly modAssembly, string modelName)
            :
            this(modAssembly, modelName, null) { }

        /// <summary>
        /// Create an instance of LibraryModelProgram for a given assembly
        /// </summary>
        /// <param name="modAssembly">Loaded assembly</param>
        /// <param name="modelName">Name of the model namespace to be loaded. 
        /// Only classes in the model namespace will be loaded.</param>
        /// <param name="featureNames">The names of features to be loaded. If null, all
        /// features will be loaded for the given modelName. See <see cref="FeatureAttribute"/>.</param>
        /// <exception cref="ModelProgramUserException">Thrown if there is a usage error in the given assembly.</exception>
        public LibraryModelProgram(Assembly modAssembly, string modelName, Set<string>/*?*/ featureNames)
        {
            if (string.IsNullOrEmpty(modelName))
                throw new ArgumentNullException("modelName");

            InterpretationContext context = (null == featureNames ? 
                new InterpretationContext() :
                new InterpretationContext(featureNames));

            Type/*?*/[]/*?*/ allTypes = modAssembly.GetTypes();
            List<Field> stateVars = new List<Field>();
            Dictionary<Symbol, ActionInfo> aInfoMap = new Dictionary<Symbol, ActionInfo>();
            Dictionary<Type, StatePredicate> acceptingStateConditions = new Dictionary<Type, StatePredicate>();
            Dictionary<Type, StatePredicate> stateInvariants = new Dictionary<Type, StatePredicate>();
            Dictionary<Type, StatePredicate> stateFilters = new Dictionary<Type, StatePredicate>();
            //Dictionary<Type, TransitionPropertyGenerator> transitionPropertyGenerators = new Dictionary<Type, TransitionPropertyGenerator>();
            bool modelIsEmpty = true;

            #region Get state variables, actions, invariants, accepting state conditions, and state filters

            abstractSorts = new Set<Symbol>();

            foreach (Type t in allTypes)
            {
                try
                {
                    // ignore any compiler-generated types, such as iterators.
                    if (ReflectionHelper.IsCompilerGenerated(t))
                        continue;

                    // Collect  state variables, actions, invariants and accepting state conditions.
                    if (ReflectionHelper.IsInModel(t, modelName, featureNames))
                    {
                        // Register the sort for this type
                        context.RegisterSortType(AbstractValue.TypeSort(t), t);

                        // Check if the sort is abstract
                        if (AbstractValue.IsTypeAbstractSort(t)) abstractSorts=abstractSorts.Add(AbstractValue.TypeSort(t));

                        // Only extract variables and actions from class types.
                        if (!t.IsClass)
                            continue;

                        // clear flag that detects model namespace spelling errors
                        modelIsEmpty = false;

                        // Collect state variables
                        foreach (FieldInfo field in ReflectionHelper.GetModelVariables(t))
                            stateVars.Add(new Field(field));

                        Set<string> actionMethodNames = Set<string>.EmptySet;  // used to detect duplicates

                        // Collect actions
                        foreach (MethodInfo methodInfo in ReflectionHelper.GetMethodsForActions(t))
                        {
                            try
                            {
                                if (actionMethodNames.Contains(methodInfo.Name))
                                    throw new ModelProgramUserException("Duplicate action method name '" + methodInfo.Name + "' found. Action methods may not use overloaded names.");

                                if (!methodInfo.IsStatic)
                                {
                                    //check that the the declaring class is a labeled instance
                                    //or else say that probably the static keyword is missing

                                    if (methodInfo.DeclaringType.BaseType == null ||
                                        methodInfo.DeclaringType.BaseType.Name != "LabeledInstance`1" ||
                                        methodInfo.DeclaringType.BaseType.GetGenericArguments()[0] != methodInfo.DeclaringType)
                                        throw new ModelProgramUserException("Since the action method '" + methodInfo.Name + "' is non-static, the class '" + methodInfo.DeclaringType.Name + "' must directly inherit from 'LabeledInstance<" + methodInfo.DeclaringType.Name + ">'." + 
                                                                            "\nDid you perhaps forget to declare the method 'static'?");
                                }

                                //check that the action parameter types are valid modeling types
                                foreach (ParameterInfo pInfo in methodInfo.GetParameters())
                                        if (!(pInfo.ParameterType.IsPrimitive ||
                                             pInfo.ParameterType.IsEnum ||
                                             pInfo.ParameterType == typeof(string) ||
                                             ReflectionHelper.ImplementsIAbstractValue(pInfo.ParameterType)))
                                            throw new ModelProgramUserException(
                                                "\nThe parameter '" + pInfo.Name + "' of '" + methodInfo.Name + "' does not a have valid modeling type. " +
                                                "\nA valid modeling type is either: a primitive type, an enum, a string, or a type that implements 'NModel.Internals.IAbstractValue'." +
                                                "\nIn particular, collection types in 'System.Collections' and 'System.Collections.Generic' are not valid modeling types." +
                                                "\nValid modeling types are collection types like 'Set' and 'Map' defined in the 'NModel' namespace, " +
                                                "\nas well as user defined types that derive from 'CompoundValue'.");

                                actionMethodNames = actionMethodNames.Add(methodInfo.Name);

                                Method method = new Method(methodInfo);
                                foreach (ActionAttribute actionAttribute in ReflectionHelper.GetModelActionAttributes(methodInfo))
                                {
                                    CompoundTerm/*?*/ startActionLabel;
                                    CompoundTerm/*?*/ finishActionLabel;
                                    ReflectionHelper.GetActionLabel(methodInfo, actionAttribute, out startActionLabel, out finishActionLabel);
                                    ActionMethodFinish/*?*/ finishActionMethod = null;
                                    if (finishActionLabel != null)
                                    {
                                        finishActionMethod = InsertActionMethodFinish(method, finishActionLabel, aInfoMap);
                                    } 
                                    if (startActionLabel != null)
                                    {
                                        InsertActionMethodStart(method, startActionLabel, finishActionMethod, aInfoMap);
                                    }
                                }
                            }
                            catch (ModelProgramUserException e)
                            {
                                string msg = "method " + methodInfo.Name + ", " + e.Message;
                                throw new ModelProgramUserException(msg);
                            }
                        }

                        // to do: collect transition properties

                        // Collect state invariants
                        //StatePredicate sp1 = StatePredicate.GetPredicates(t, GetStateInvariantMethodNames(t));
                        //if (null != sp1)
                        //    stateInvariants.Add(t, sp1);

                        // Collect accepting state conditions
                        StatePredicate sp2 = StatePredicate.GetAcceptingStateCondition(t);
                        if (null != sp2)
                            acceptingStateConditions.Add(t, sp2);

                        // Collect state invariants
                        StatePredicate sp3 = StatePredicate.GetStateInvariant(t);
                        if (null != sp3)
                            stateInvariants.Add(t, sp3);

                        //collect state filters
                        StatePredicate sp4 = StatePredicate.GetStateFilter(t);
                        if (null != sp4)
                            stateFilters.Add(t, sp4);


                    }
                }
                catch (ModelProgramUserException e)
                {
                    string msg = "In class " + t.Name + ", " + e.Message;
                    throw new ModelProgramUserException(msg);
                }
            }

            if (modelIsEmpty)
                throw new ModelProgramUserException("No classes found in model namespace " + modelName + ". Did you misspell?");
            

            #endregion

            // todo: Collect "sorts" for each type. Walk type tree of state variables and 
            // action arguments to do this.
 

            Symbol[] aSymbols = new Symbol[aInfoMap.Keys.Count];
            int j = 0;
            foreach (Symbol a in aInfoMap.Keys)
                aSymbols[j++] = a;

            Field[] sFields = stateVars.ToArray();
            StateVariable[] sVars = new StateVariable[sFields.Length];
            string[] lNames = new string[sVars.Length];
            ValueArray<string> locNames;
            for (int i = 0; i < sVars.Length; i++)
            {
                sVars[i] = sFields[i].stateVariable;
                lNames[i] = sFields[i].stateVariable.Name;
            }

            locNames = new ValueArray<string>(lNames);

            string nameExt = "";
            if (featureNames != null && featureNames.Count > 0)
            {
                nameExt += "[";
                for (int i = 0; i < featureNames.Count; i++)
                {
                    nameExt += featureNames.Choose(i);
                    if (i < featureNames.Count - 1)
                        nameExt += ",";
                }
                nameExt += "]";
            }

            this.name = modelName + nameExt;
            // this.generator = generator;
            this.stateFields = sFields;
            this.locationNames = locNames;
            this.stateVariables = sVars;
            this.actionSymbols = new Set<Symbol>(aSymbols);
            this.actionInfoMap = aInfoMap;
            this.finishActionSymbols = LibraryModelProgram.CreateStartFinishMap(aInfoMap);
            this.modelAssembly = modAssembly;
            this.context = context;
            this.currentState = GetInitialState();
            this.stateChangedPredicate = false;
            this.acceptingStateConditions = acceptingStateConditions;
            this.stateInvariants = stateInvariants;
            this.stateFilters = stateFilters;
        }

        static Dictionary<Symbol, Symbol> CreateStartFinishMap(Dictionary<Symbol, ActionInfo> aInfoMap)
        {
            Dictionary<Symbol, Symbol> result = new Dictionary<Symbol, Symbol>();
            foreach (KeyValuePair<Symbol, ActionInfo> kv in aInfoMap)
            {
                ActionInfo aInfo = kv.Value;
                if (aInfo.Kind == ActionKind.Start)
                {
                    Symbol/*?*/ finishAction = null;
                    foreach (ActionMethod am in aInfo.ActionMethods)
                    {                       
                        ActionMethodStart amStart = am as ActionMethodStart;
                        if (null != amStart)
                        {
                            if (amStart.Kind == ActionKind.Start)
                            {
                                if (null == finishAction)
                                    finishAction = amStart.FinishAction;
                                else if (!finishAction.Equals(amStart.FinishAction))
                                    throw new ModelProgramUserException("inconsistently named finish actions " +
                                                finishAction.ToString() + " and " + amStart.FinishAction.ToString() +
                                                " were found for start action " + amStart.actionLabel.ToString());
                            }
                        }
                        else
                            throw new InvalidOperationException("Invalid internal state in LibraryModelProgram");
                    }
                    if (null == finishAction)
                        throw new InvalidOperationException("Invalid internal state in LibraryModelProgram");
                    else 
                        result.Add(kv.Key, finishAction);
                }
            }
            return result;
        }

        static void RegisterModelTypeSorts(InterpretationContext context)
        {
            // TODO: implement
            throw new NotImplementedException(context.ToString());

            //// Only include sorts for types attributed with a matching model program 
            //// name, or not attributed with any model program name. (Some types are private 
            //// to a particular model program; some types are shared by all model programs in the assembly.)
            //if ((modelClassName != null || !hasName) && !ReflectionHelper.IsCompilerGenerated(t))
            //{
            //    Symbol sort = AbstractValue.TypeSort(t);
            //    context.RegisterSortType(sort, t);
            //}
        }
        #endregion

        /// <summary>
        /// Adds an <see>ActionMethod</see> entry for <paramref name="startActionLabel"/> in the dictionary.
        /// </summary>
        static void InsertActionMethodStart(Method method, CompoundTerm startActionLabel, ActionMethodFinish/*?*/ finishActionMethod, Dictionary<Symbol, ActionInfo> aInfoMap)
        {
            bool isAtomic = (null == finishActionMethod);
            int[] inParamIndices = ReflectionHelper.GetInputParameterIndices(startActionLabel, method.methodInfo);
            int arity = startActionLabel.Arguments.Count;
            ActionMethod am = new ActionMethodStart(startActionLabel, method, arity, finishActionMethod, inParamIndices);
            ActionKind kind = isAtomic ? ActionKind.Atomic : ActionKind.Start;

            ActionInfo aInfo;
            Symbol startActionSymbol = startActionLabel.Symbol;
            List<ActionMethod> actionMethods = new List<ActionMethod>();
            if (aInfoMap.TryGetValue(startActionSymbol, out aInfo))
            {
                if (aInfo.Arity != arity)
                {
                    throw new ModelProgramUserException("Mismatched number of action arguments");
                }
                
                // An action composed of several action methods is split if any of its
                // action methods is split.
                if (aInfo.Kind == ActionKind.Start)
                    kind = ActionKind.Start;

                foreach (ActionMethod am1 in aInfo.ActionMethods)
                    actionMethods.Add(am1);
            }

            actionMethods.Add(am);
            aInfoMap[startActionSymbol] = new ActionInfo(arity, null, kind, actionMethods);
        }


        /// <summary>
        /// Adds an <see>ActionMethod</see> entry for <paramref name="finishActionLabel"/> in the dictionary.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="finishActionLabel"></param>
        /// <param name="aInfoMap"></param>
        static ActionMethodFinish InsertActionMethodFinish(Method method, CompoundTerm finishActionLabel, Dictionary<Symbol, ActionInfo> aInfoMap)
        {
            int[] outParamIndices = ReflectionHelper.GetOutputParameterIndices(finishActionLabel, method.methodInfo);
            int arity = finishActionLabel.Arguments.Count;
            ActionMethodFinish am = new ActionMethodFinish(finishActionLabel, method, arity, outParamIndices);
            
            
            ActionInfo aInfo;
            Symbol finishActionSymbol = finishActionLabel.Symbol;
            List<ActionMethod> actionMethods = new List<ActionMethod>();
            if (aInfoMap.TryGetValue(finishActionSymbol, out aInfo))
            {
                //if (aInfo.Arity != arity)
                //{
                //    throw new ModelProgramUserException("Mismatched number of output action arguments");
                //}
                foreach (ActionMethod am1 in aInfo.ActionMethods)
                    actionMethods.Add(am1);  
            }
         
            actionMethods.Add(am);                
            aInfoMap[finishActionSymbol] = new ActionInfo(arity, null, ActionKind.Finish, actionMethods);
            return am;
        }

        /// <summary>
        /// Returns the kind of the action symbol
        /// </summary>
        public ActionKind ActionSymbolKind(Symbol actionSymbol)
        {
            if (null == actionSymbol)
                throw new ArgumentNullException("actionSymbol");

            if (!this.ActionSymbols().Contains(actionSymbol))
                throw new ArgumentException("symbol " + actionSymbol.ToString() + "not found.");

            ActionInfo aInfo;
            if (this.actionInfoMap.TryGetValue(actionSymbol, out aInfo))
            {
                return aInfo.Kind;
            }
            else
                throw new ArgumentException("symbol " + actionSymbol.ToString() + " not found.");
        }
        

        /// <summary>
        /// Create a library model program from a given assembly, given a model 
        /// in that assembly, and a given set of features within that model.
        /// </summary>
        /// <param name="assembly">loaded assembly</param>
        /// <param name="model">model namespace within the assembly</param>
        /// <param name="features">features within the model namespace</param>
        /// <returns>Model porogram including the given features</returns>
        public static LibraryModelProgram Create(Assembly assembly, string model, Set<string> features)
        {
            return new LibraryModelProgram(assembly, model, features);
        }

        /// <summary>
        /// Create a library model program from a given type t.
        /// The namespace of t is the name of the model program.
        /// Only listed features are included in the model program.
        /// </summary>
        public static LibraryModelProgram Create(Type t, params string[] features)
        {
            return new LibraryModelProgram(t.Assembly, t.Namespace, new Set<string>(features));
        }

        #region State Save/Restore Operations

        // TO DO: Maintain pool of preallocated instances
        // Map<string, Sequence<LabeledInstance>> typeInstances = Map<string, Sequence<LabeledInstance>>.EmptyMap;

        //TBD: Locations and State variables map one-to-one here
        //in general locations may be non-nullary (not supported here)
        internal void SetState(IState state1)
        {
            if (stateChangedPredicate || (object)state1 != (object)currentState)
            {
                try
                {
                    context.SetAsActive();

                    // to do: change signature of method to take IExtendedState
                    SimpleState state = state1 as SimpleState;
                    if (state == null) throw new ArgumentException("Internal error");

                    context.ResetPoolFields();  // to do: consider eliminating this for performance
                    context.IdPool = state.DomainMap;  // restore the idPool
                    // TO DO: Calculate domain maps for each labeled instance sort; need to cache this.

                    // restore field values
                    for (int i = 0; i < state.LocationValuesCount; i++)
                    {
                        Term termValue = state.GetLocationValue(i);
                        IComparable interpretedValue = context.InterpretTerm(termValue);
                        this.stateFields[i].SetValue(interpretedValue);
                    }
                    stateChangedPredicate = false;
                    currentState = state1;
                }
                finally
                {
                    context.ClearAsActive();
                }
            }
        }


        /// <summary>
        /// Gets the initial state of the ModelProgram. 
        /// </summary>
        /// <returns>The initial state</returns>
        IState GetInitialState()
        {
            if (initialState == null)
                // This branch is in fact invoked by the constructor.
                initialState = GetState(readyControlMode);

            return initialState;
        }

        internal IState GetState(Term newControlMode)
        {
            Term[] vals = new Term[this.stateFields.Length];
            for (int i = 0; i < vals.Length; i++)
            {
                IComparable/*?*/ val = this.stateFields[i].GetValue(this.context);
                vals[i] = AbstractValue.GetTerm(val);
            }
            return SimpleState.CreateState(newControlMode, vals, context.IdPool, this.name, this.locationNames);
        }
        #endregion

        #region ModelProgram Members

        #region Action signature

        /// <summary>
        /// Action vocabulary
        /// </summary>
        /// <returns></returns>
        public override Set<Symbol> ActionSymbols()
        {
            return actionSymbols;
        }

        /// <summary>
        /// Number of arguments taken by the action symbol
        /// </summary>
        public override int ActionArity(Symbol actionSymbol)
        {
            if (null == actionSymbol)
                throw new ArgumentNullException("actionSymbol");

            ActionInfo info;
            if (actionInfoMap.TryGetValue(actionSymbol, out info))
            {
                return info.Arity;
            }
            else
            {
                throw new ArgumentException("actionSymbol");
            }
        }

        /// <summary>
        /// Parameter sort of the given parameter index and given action symbol
        /// </summary>
        public override Symbol ActionParameterSort(Symbol actionSymbol, int parameterIndex)
        {
            if (null == actionSymbol)
                throw new ArgumentNullException("actionSymbol");

            ActionInfo info;
            if (actionInfoMap.TryGetValue(actionSymbol, out info))
            {
                if (info.ParameterSorts.Length > parameterIndex)
                {
                    // TODO: make sure parameterSorts is initialized in the constructor
                    return info.ParameterSorts[parameterIndex];
                }
                else
                {
                    throw new ArgumentException("parameterIndex");
                }
            }
            else
            {
                throw new ArgumentException("actionSymbol");
            }
        }

        //public ActionRole GetActionRole(Symbol actionSymbol)
        //{
        //    // to do: save this info from a yet-to-be defined attribute.
        //    throw new Exception("The method or operation is not implemented.");
        //} 
        #endregion

        #region State signature

        /// <summary>
        /// Number of locations (state variables)
        /// </summary>
        public override int LocationValueCount
        {
            get { return stateVariables.Length; }
        }

        /// <summary>
        /// Name of the i'th state variable
        /// </summary>
        public override string LocationValueName(int i)
        {
            if (0 < i && i <= stateVariables.Length)
            {
                return stateVariables[i].Name;
            }
            else
            {
                throw new ArgumentOutOfRangeException("i");
            }
        }

        /// <summary>
        /// Returns this.Name
        /// </summary>
        public override string LocationValueModelName(int i)
        {
            return this.Name;
        }

        /// <summary>
        /// Sort of the i'th variable
        /// </summary>
        public override Symbol LocationValueSort(int i)
        {
            if (0 < i && i <= stateVariables.Length)
            {
                return stateVariables[i].VariableSort;
            }
            else
            {
                throw new ArgumentOutOfRangeException("i");
            }

        }
        #endregion

        #region Initial state
        /// <summary>
        /// The initial state.
        /// <br>ensures result != null;</br>
        /// <br>ensures result.ModelProgram == this; </br>
        /// </summary>
        public override IState InitialState
        {
            get { return this.GetInitialState(); }
        }
        #endregion

        Set<IComparable> GetInstances(Type t)
        {
            // to do: walk structures instead of using pool
            return new Set<IComparable>(this.context.InstancePoolValues(AbstractValue.TypeSort(t)));
        }

        /// <summary>
        /// Evaluates the conjunction of all the accepting state conditions in the given state
        /// </summary>
        /// <param name="state">given state</param>
        /// <returns>true if the state is an accepting state</returns>
        public override bool IsAccepting(IState state)
        {
            if (this.acceptingStateConditions.Count == 0)
                return true;

            SetState(state);
            context.SetAsActive();
            try
            {
                // Ensure that state is "ready" mode
                if (!state.ControlMode.Equals(readyControlMode))
                    return false;

                // Check all accepting state conditions given by model.
                foreach (KeyValuePair<Type, StatePredicate> kv in this.acceptingStateConditions)
                {
                    Type t = kv.Key;
                    StatePredicate pred = kv.Value;
                    if (!pred.Holds(this.context, GetInstances(t)))
                        return false;
                }

                return true;
            }
            finally
            {
                context.ClearAsActive();
            }
        }


        /// <summary>
        /// Boolean value indicating whether all state invariant predicates
        /// defined by this model program are satisfied by <paramref name="state"/>. In general,
        /// failure to satisfy the state invariants indicates a modeling error.
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>True if <paramref name="state"/>satisfies all state invariants of 
        /// this model program; false otherwise.</returns>
        public override bool SatisfiesStateInvariant(IState state)
        {
            if (this.stateInvariants.Count == 0)
                return true;

            SetState(state);
            context.SetAsActive();
            try
            {
                // Ensure that state is "ready" mode
                if (!state.ControlMode.Equals(readyControlMode))
                    return true;

                foreach (KeyValuePair<Type, StatePredicate> kv in this.stateInvariants)
                {
                    Type t = kv.Key;
                    StatePredicate pred = kv.Value;
                    if (!pred.Holds(this.context, GetInstances(t)))
                        return false;
                }

                return true;
            }
            finally
            {
                context.ClearAsActive();
            }
        }

        /// <summary>
        /// Boolean value indicating whether all state filter predicates
        /// defined by this model program are satisfied by <paramref name="state"/>. 
        /// States that do not satisfy a filter are excluded during exploration.
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>True if <paramref name="state"/>satisfies all state filters of 
        /// this model program; false otherwise.</returns>
        public override bool SatisfiesStateFilter(IState state)
        {
            if (this.stateFilters.Count == 0)
                return true;

            SetState(state);
            context.SetAsActive();
            try
            {
                // Ensure that state is "ready" mode
                if (!state.ControlMode.Equals(readyControlMode))
                    return true;

                foreach (KeyValuePair<Type, StatePredicate> kv in this.stateFilters)
                {
                    Type t = kv.Key;
                    StatePredicate pred = kv.Value;
                    if (!pred.Holds(this.context, GetInstances(t)))
                        return false;
                }

                return true;
            }
            finally
            {
                context.ClearAsActive();
            }
        }

        /// <summary>
        /// Checks whether a sort is an abstract type. The corresponding set is created once in the constructor.
        /// </summary>
        /// <param name="s">A symbol denoting a sort (i.e. abstract type)</param>
        /// <returns>true if the sort is abstract, false otherwise.</returns>
        public override bool IsSortAbstract(Symbol s)
        {
            return abstractSorts.Contains(s);
        }


        CompoundTerm CurrentStackFrame(IState state)
        {
            // assumption: state is of type SimpleState
            // Stack frame may have nested actions
            Term stackTerm = state.ControlMode;
            Sequence<CompoundTerm> stack = (Sequence<CompoundTerm>)context.InterpretTerm(stackTerm);
            if (stack.IsEmpty)
                throw new ArgumentException("Unexpected control mode-- empty stack");
            return stack.Head;
        }

        Term PopStackFrame(Term controlMode)
        {
            Sequence<CompoundTerm> stack = (Sequence<CompoundTerm>)context.InterpretTerm(controlMode);
            if (stack.IsEmpty)
                throw new ArgumentException("Internal error-- attempt to pop empty control mode stack");
            else
                return AbstractValue.GetTerm(stack.Tail);
        }

        Term PushStackFrame(Term controlMode, CompoundTerm action)
        {
            Sequence<CompoundTerm> stack = (Sequence<CompoundTerm>)context.InterpretTerm(controlMode);
            Sequence<CompoundTerm> newStack = stack.AddFirst(action);
            return AbstractValue.GetTerm(newStack);
        }

        private void AddContinuation(IState startState, CompoundTerm action, IState endState)
        {
            Dictionary<CompoundTerm, IState> stateContinuations;
            if (!this.continuations.TryGetValue(startState, out stateContinuations))
            {
                stateContinuations = new Dictionary<CompoundTerm, IState>();
                this.continuations[startState] = stateContinuations;
            }

            stateContinuations[action] = endState;
        }

        private IState GetContinuationTargetState(IState startState, CompoundTerm action)
        {
            Dictionary<CompoundTerm, IState> stateContinuations;
            if (!this.continuations.TryGetValue(startState, out stateContinuations))
            {
                throw new InvalidOperationException("Continuation start state not found");
            }

            IState targetState;
            if (!stateContinuations.TryGetValue(action, out targetState))
            {
                throw new InvalidOperationException("Continuation action not found");
            }
            return targetState;
        }

        #region Enabledness of actions and parameter generation
        /// <summary>
        /// Returns true if the given action symbol is possibly enabled in the given state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actionSymbol"></param>
        /// <returns></returns>
        public override bool IsPotentiallyEnabled(IState state, Symbol actionSymbol)
        {
            // Case 1: ready mode
            if (state.ControlMode.Equals(readyControlMode))
            {
                return IsPotentiallyEnabled_Internal(state, actionSymbol);
            }
            // Case 2: intermediate step.
            else
            {
                // to do: handle multiple continuations by checking if actionSymbol
                // is found in dictionary of continuations.

                // Temp: just handle start/finish idiom for now
                if (ActionSymbolKind(actionSymbol) != ActionKind.Finish)
                    return false;

                CompoundTerm startAction = CurrentStackFrame(state);
                Symbol startActionSymbol = startAction.Symbol;

                return ActionSymbolKind(startActionSymbol) == ActionKind.Start &&           // must be in the middle of an action
                       actionSymbol.Equals(this.finishActionSymbols[startActionSymbol]);    // start and finish symbols must match names
            }
        }

        //assumes that state has been set
        internal bool IsPotentiallyEnabled_Internal(IState state, Symbol actionSymbol)
        {
            // to do: check to see if current state has continuations; otherwise eval.
            if (ActionSymbolKind(actionSymbol) == ActionKind.Finish) return false;

            ActionInfo/*?*/ a;
            if (this.actionInfoMap.TryGetValue(actionSymbol, out a))
            {
                SetState(state);
                this.context.SetAsActive();
                try
                {
                    
                    return a.IsPotentiallyEnabled(this.context);
                }
                finally
                {
                    this.context.ClearAsActive();
                }
            }
            return false;

        }

        /// <summary>
        /// Returns all the possibly (potentially) enabled action symbols in the given state
        /// </summary>
        public override Set<Symbol> PotentiallyEnabledActionSymbols(IState state)
        {
            return new Set<Symbol>(PotentiallyEnabledActionSymbols_1(state));
        }

        private IEnumerable<Symbol> PotentiallyEnabledActionSymbols_1(IState state)
        {
            // Case 1: ready mode
            if (state.ControlMode.Equals(readyControlMode))
            {

                foreach (Symbol actionSymbol in actionInfoMap.Keys)
                {
                    if (ActionSymbolKind(actionSymbol) != ActionKind.Finish)
                    {
                        SetState(state);
                        this.context.SetAsActive();
                        try
                        {
                            ActionInfo a = this.actionInfoMap[actionSymbol];
                            if (a.IsPotentiallyEnabled(this.context))
                            {
                                yield return actionSymbol;
                            }
                        }
                        finally
                        {
                            this.context.ClearAsActive();
                        }
                    }
                }
            }
            // Case 2: intermediate step.
            else
            {
                // to do: extend to handle suspension points by iterating through continuations
                CompoundTerm startAction = CurrentStackFrame(state);
                Symbol startActionSymbol = startAction.Symbol;
                if (ActionSymbolKind(startActionSymbol) == ActionKind.Start)     // must be in the middle of an action
                {
                    yield return this.finishActionSymbols[startActionSymbol];
                }
                else
                    throw new ArgumentException("Unexpected stack frame format.");
            }
        }

        /// <summary>
        /// Returns true if the library model program has missing parameter domains
        /// </summary>
        public bool HasMissingParameterDomains(IState state)
        {
            foreach (Symbol actionSymbol in this.ActionSymbols())
                for (int parameterIndex = 0; parameterIndex < this.ActionArity(actionSymbol); parameterIndex += 1)
                    if (!HasActionParameterDomain(state, actionSymbol, parameterIndex))
                        if (!HasActionParameterDomain(state, actionSymbol, parameterIndex))
                            return true;
            return false;
        }

        /// <summary>
        /// Enumerates the missing parameter domains in the given state
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<Pair<Symbol, int>> MissingParameterDomains(IState state)
        {
            foreach (Symbol actionSymbol in this.ActionSymbols())
                for (int parameterIndex = 0; parameterIndex < this.ActionArity(actionSymbol); parameterIndex += 1)
                    if (!HasActionParameterDomain(state, actionSymbol, parameterIndex))
                        yield return new Pair<Symbol, int>(actionSymbol, parameterIndex);
        }

        /// <summary>
        /// Does this model program have an interface to parameter generation for this parameter?
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actionSymbol"></param>
        /// <param name="parameterIndex"></param>
        /// <returns>A set of terms representing the possible values</returns>
        public override bool HasActionParameterDomain(IState state, Symbol actionSymbol, int parameterIndex)
        {
            ActionInfo actionInfo;
            if (!actionInfoMap.TryGetValue(actionSymbol, out actionInfo))
                throw new ArgumentException();

            if (ActionSymbolKind(actionSymbol) == ActionKind.Start || ActionSymbolKind(actionSymbol) == ActionKind.Atomic)
            {
                if (!(0 <= parameterIndex && parameterIndex < actionInfo.Arity))
                    return false;
                    // throw new ArgumentOutOfRangeException("parameterIndex");

                foreach(ActionMethod am in actionInfo.ActionMethods)
                    if (am.HasActionParameterDomain(parameterIndex))
                        return true;                       
                return false;
            }
            else
            {
                return true;
            }
        }

        static Set<Term> IntersectWithAny(Set<Term> s1 , Set<Term> s2)
        {
            if (s1.Contains(Any.Value))
                return s2;
            else if (s2.Contains(Any.Value))
                return s1;
            else
                return s1.Intersect(s2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actionSymbol"></param>
        /// <param name="parameterIndex"></param>
        /// <returns></returns>
        public override Set<Term> ActionParameterDomain(IState state, Symbol actionSymbol, int parameterIndex)
        {
            // 1- get domain from attr
            ActionInfo actionInfo;
            if (!actionInfoMap.TryGetValue(actionSymbol, out actionInfo))
                throw new InvalidOperationException();

            if (ActionSymbolKind(actionSymbol) == ActionKind.Start || ActionSymbolKind(actionSymbol) == ActionKind.Atomic)
            {
                // merge parameter generators of each action method
                if (0 <= parameterIndex && parameterIndex < actionInfo.Arity)
                {
                    Set<Term>/*?*/ values = null;
                   
                    SetState(state);
                    this.context.SetAsActive();
                    try
                    {
                        foreach(ActionMethod am in actionInfo.ActionMethods)
                        {
                            ParameterGenerator/*?*/ parameterGenerator = am.GetParameterGenerator(parameterIndex);

                            if (null != parameterGenerator)
                            {
                                Set<Term> newValues = parameterGenerator();
 
                                values = (null == values) ? newValues : IntersectWithAny(values ,newValues);
                            }
                        }
                    }
                    finally
                    {
                        this.context.ClearAsActive();
                    }
                    if (null == values)
                        throw new ModelProgramUserException("Parameter "+ parameterIndex + " of action '" + actionSymbol + "' does not have a parameter generator." + 
                            "\nFor instance based actions, parameter 0 is the implicit 'this' parameter;" +
                            "\nthe generator is added to the class using [Domain(\"new\")] or [Domain(M)] attribute where M is a custom parameter generator.");
                    return values;

                }
                else
                {
                    throw new ArgumentOutOfRangeException("parameterIndex");
                }
            }
            else
            {
                Set<Term> result = Set<Term>.EmptySet;
                Dictionary<CompoundTerm, IState> stateContinuations;
                if (this.continuations.TryGetValue(state, out stateContinuations))
                {
                    foreach (CompoundTerm action in stateContinuations.Keys)
                    {
                        if (0 <= parameterIndex && parameterIndex < action.Arguments.Count)
                        {
                            result = result.Add(action.Arguments[parameterIndex]);
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Returns true if the given action is enabeld in the given state
        /// </summary>
        public override bool IsEnabled(IState state, CompoundTerm action)
        {
            // Case 1: ready mode
            if (state.ControlMode.Equals(readyControlMode))
            {
                if (ActionSymbolKind(action.Symbol) == ActionKind.Finish) return false;
                ActionInfo/*?*/ a;
                if (this.actionInfoMap.TryGetValue(action.FunctionSymbol as Symbol, out a))
                {
                    SetState(state);
                    this.context.SetAsActive();
                    //BOOGIE: a is known to be nonnull at this point
                    try
                    {
                        return (/*^(ActionInfo)^*/a).IsEnabled(this.context, action.Arguments);
                    }
                    finally
                    {
                        this.context.ClearAsActive();
                    }
                }
                return false;
            }
            // Case 2: intermediate step
            else
            {
                Dictionary<CompoundTerm, IState> stateContinuations;
                if (this.continuations.TryGetValue(state, out stateContinuations))
                {
                    return stateContinuations.ContainsKey(action);
                }
                return false;
            }
        }

        /// <summary>
        /// Gets string descriptions of the enabling conditions
        /// </summary>
        /// <param name="state">The state in which the </param>
        /// <param name="action">The action whose enabling conditions will queried</param>
        /// <param name="returnFailures">If <c>true</c>, enabling conditions that fail in state 
        /// <paramref name="state"/> will be returned. If <c>false</c>, all enabling conditions
        /// that are satisfied will be returned.</param>
        /// <returns>An array of description strings for the enabling conditions of action <paramref name="action"/></returns>
        public override IEnumerable<string> GetEnablingConditionDescriptions(IState state, CompoundTerm action, bool returnFailures)
        {
            // Case 1: ready mode
            if (state.ControlMode.Equals(readyControlMode))
            {
                if (ActionSymbolKind(action.Symbol) == ActionKind.Finish)
                {
                    if (returnFailures) yield return "Finish action symbol not enabled in ready control mode.";
                }
                else
                {
                    ActionInfo/*?*/ a;
                    if (this.actionInfoMap.TryGetValue(action.FunctionSymbol as Symbol, out a))
                    {
                        SetState(state);
                        this.context.SetAsActive();
                        try
                        {
                            foreach (string s in a.GetEnablingConditionDescriptions(this.context, action.Arguments, returnFailures))
                                yield return s;
                        }
                        finally
                        {
                            this.context.ClearAsActive();
                        }
                    }
                }
            }
            // Case 2: intermediate step
            else
            {
                Dictionary<CompoundTerm, IState> stateContinuations;
                if (this.continuations.TryGetValue(state, out stateContinuations))
                {
                    bool enabled = stateContinuations.ContainsKey(action);
                    if (enabled && !returnFailures) 
                        yield return "Return value of finish action is the expected value";
                    else if (!enabled && returnFailures)
                    {
                        foreach (CompoundTerm continuation in stateContinuations.Keys)
                            yield return "Unexpected return value of finish action, expected: " +
                                         continuation.ToString();                                          
                    }
                }
                if (returnFailures)
                    yield return "Unexpected finish action";
            }
        }
        #endregion

        /// <summary>
        /// Enumerates all the enabled actions in the given state with the given action symbol.
        /// </summary>
        public override IEnumerable<CompoundTerm> GetActions(IState state, Symbol actionSymbol)
        {
            // Case 1: ready mode
            if (state.ControlMode.Equals(readyControlMode))
            {
                if (ActionSymbolKind(actionSymbol) == ActionKind.Finish) yield break;
                //enumerate over the parameter domains, check 
                //enabling conditions and yield the actions
                ActionInfo/*?*/ actionMethod;
                // SetState(state);
                if (this.IsPotentiallyEnabled_Internal(state, actionSymbol)) // CODE REVIEW: Is this call redundant? Appears to be.
                {
                    if (this.actionInfoMap.TryGetValue(actionSymbol, out actionMethod))
                    {
                        //BOOGIE: actionMethod is known to be nonnull at this point
                        if ((/*^(ActionInfo)^*/actionMethod).IsPotentiallyEnabled(this.context))
                        {
                            foreach (Sequence<Term> args in AllParameterCombinations(state, actionSymbol))
                            {
                                // State may need to be reset if someone calls GetActions recursively.
                                SetState(state);
                                this.context.SetAsActive();
                                try
                                {
                                    if ((/*^(ActionInfo)^*/actionMethod).IsEnabled(this.context, args))
                                        yield return new CompoundTerm(actionSymbol, args);
                                }
                                finally
                                {
                                    this.context.ClearAsActive();
                                }
                            }
                        }
                    }
                }
            }
            // Case 2: intermediate step.
            else
            {
                Dictionary<CompoundTerm, IState> stateContinuations;
                if (this.continuations.TryGetValue(state, out stateContinuations))
                {
                    foreach (CompoundTerm action in stateContinuations.Keys)
                        yield return action;
                }
                else
                    throw new InvalidOperationException("Expected continuation not found");
            }
        }

        IEnumerable<Sequence<Term>> AllParameterCombinations(IState state, Symbol actionSymbol)
        {
            int arity = this.ActionArity(actionSymbol);
            //if (arity > 0)
            //{
            Sequence<Set<Term>> args = Sequence<Set<Term>>.EmptySequence;
            for (int i = 0; i < arity; i++)
            {
                args = args.AddLast(this.ActionParameterDomain(state, actionSymbol, i));
            }
            return CartesianProduct(args);
            //}
            // return Set<Sequence<Term>>.EmptySet;

        }

        static IEnumerable<Sequence<Term>> CartesianProduct(Sequence<Set<Term>> args)
        {
            if (args.Count == 0)
                yield return Sequence<Term>.EmptySequence;
            else if (args.Count == 1)
                foreach (Term term in args.Head)
                    yield return new Sequence<Term>(term);
            else if (args.Count > 0)
            {
                foreach (Sequence<Term> tuple in CartesianProduct(args.Tail))
                    foreach (Term term in args.Head)
                        yield return tuple.AddFirst(term);
            }
        }

        /// <summary>
        /// Returns the names of meta-properties that may be collected
        /// during the calculation of the <see cref="GetTargetState"/>.
        /// </summary>
        /// <returns>The names of meta-properties to be collected
        /// during the calculation of the step.</returns>
        /// <seealso cref="GetTargetState"/>
        public override Set<string> GetTransitionPropertyNames()
        {
            // to do: implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Produces the target state that results from invoking <paramref name="action"/>
        /// in the context of <paramref name="startState"/>.
        /// </summary>
        /// <param name="startState">The state in which the action is invoked</param>
        /// <param name="action">The action to be invoked</param>
        /// <param name="transitionPropertyNames">The names of meta-properties to be collected
        /// during the calculation of the step.</param>
        /// <param name="transitionProperties">Output parameter that will contain a 
        /// map of property names to property values. Each property value multiset of
        /// terms. For example, the property value might be the value of a Boolean function
        /// that controls state filtering. Or, it might correspond to the "coverage" of the model that results from this
        /// step. In this case, the value might denote the line numbers or blocks of the 
        /// model program that were exercised in this step, or a projection of the state 
        /// space or a reference to section numbers of a requirements document to indicate
        /// that the functionality defined by that section was exercised.</param>
        /// <returns>The state that results from the invocation of <paramref name="action"/>
        /// in <paramref name="startState"/>.</returns>
        /// <seealso cref="GetTransitionPropertyNames"/>
        public override IState GetTargetState(IState startState, CompoundTerm action, Set<string> transitionPropertyNames,
                              out TransitionProperties transitionProperties)
        {
            // to do: implement coverage points by callback and add them to transitionProperties
            transitionProperties = new TransitionProperties();
            //Bag<Term> coveragePoints = Bag<Term>.EmptyBag;
            // to do: for all values in action args, call AbstractValue.FinalizeImport()
            if (startState.ControlMode.Equals(readyControlMode))
            {
                ActionKind kind = ActionSymbolKind(action.Symbol);
                SimpleState startState1 = startState as SimpleState;
                if (null == startState1) throw new InvalidOperationException("Unexpected type");
                if ((kind == ActionKind.Start) &&
                    actionInfoMap.ContainsKey(action.Symbol))
                {
                    // to do: add invocPoint to state
                    // throw new Exception("Not Implemented");

                    // Refactor: action invocation point becomes just a state.
                    //ActionInvocationPoint invocPoint =
                    //    new ActionInvocationPoint(this, action, startState);


                    SimpleState midState = startState1.ReplaceControlMode(PushStackFrame(startState.ControlMode, action));

                    MachineStep step = DoStep(midState);

                    this.AddContinuation(midState, step.Action, step.TargetState);
                    transitionProperties.AddProperty("CoveragePoints", this.context.GetCoveragePoints());
                    return midState;
                }
                else if (kind == ActionKind.Atomic)
                {
                    // to do: push action onto stack
                    // throw new NotImplementedException();

                    SimpleState intermediateState = startState1.ReplaceControlMode(PushStackFrame(startState.ControlMode, action));
                    MachineStep step = DoStep(intermediateState);
                    transitionProperties.AddProperty("CoveragePoints", this.context.GetCoveragePoints());
                    return step.TargetState;
                }
                else
                {
                    throw new ArgumentException("Invalid action");
                }
            }
            // Case 2: intermediate step
            else
            {
                return GetContinuationTargetState(startState, action);
            }
        }

        IEnumerable<Symbol> StartActionSymbols
        {
            get
            {
                foreach (Symbol actionSymbol in this.actionSymbols)
                {
                    if (ActionSymbolKind(actionSymbol) == ActionKind.Start)
                        yield return actionSymbol;
                }
            }
        }

        #endregion

        #region Stepping

        MachineStep DoStep(IState startState)
        {
            CompoundTerm currentAction = CurrentStackFrame(startState);
            ActionInfo actionInfo;
            if (!actionInfoMap.TryGetValue(currentAction.Symbol, out actionInfo))
                throw new InvalidOperationException("Internal error-- can't invoke action symbol");

            // execute the method
            SetState(startState);
            this.context.SetAsActive();
            try
            {
                CompoundTerm/*?*/ finishAction = null;
                foreach (ActionMethod am in actionInfo.ActionMethods)
                {
                    CompoundTerm/*?*/ newResultTerm = am.DoStep(this.context, currentAction);
                    if (null == finishAction) 
                        finishAction = newResultTerm;
                    else if (null != newResultTerm)
                    {
                        if (!Object.Equals(finishAction, newResultTerm))
                            throw new ModelProgramUserException("Methods for action " + currentAction.Symbol.Name +
                                                                " returned inconsistent results " +
                                                                finishAction.ToString()  +
                                                                " and " +
                                                                newResultTerm.ToString());                                                              

                    }                    
                }

                IState endState = GetState(PopStackFrame(startState.ControlMode));
                return new MachineStep(finishAction, endState);
            }
            finally
            {
                this.stateChangedPredicate = true;
                this.context.ClearAsActive();
            }
        }
        #endregion

    }

    /// <summary>
    /// The value type of machine steps.
    /// </summary>
    public sealed class MachineStep : CompoundValue
    {
        readonly CompoundTerm action;
        readonly IState targetState;

        // must be kept in sync with list of fields!
        /// <summary>
        /// Enumerates the constituents of this compound value.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IComparable> FieldValues()
        {
            yield return this.action;
            yield return this.targetState;
        }

        /// <summary>
        /// The action label of the step.
        /// </summary>
        public CompoundTerm Action
        {
            get { return action; }
        }

        /// <summary>
        /// The target data state of the step.
        /// </summary>
        public IState TargetState
        {
            get { return targetState; }
        }

        /// <summary>
        /// A step is an action term and a target state.
        /// </summary>
        /// <param name="action">The action term that was invoked</param>
        /// <param name="targetState">The state that resulted from the invocation</param>
        public MachineStep(CompoundTerm action, IState targetState)
        {
            this.action = action;
            this.targetState = targetState;
        }
    }


}

