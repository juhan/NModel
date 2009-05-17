//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Terms;
using System.Reflection;

namespace NModel.Internals
{
    /// <summary>
    /// Runtime context needed to interpret Terms as .NET objects and to pretty print .NET objects as Terms.
    /// See <see cref="InterpretationContext.SetAsActive"/>.
    /// </summary>
    public class InterpretationContext
    {
        /// <summary>
        /// Set of feature keywords that apply in this context.
        /// </summary>
        readonly Set<string> features;

        /// <summary>
        /// Per-sort dictionary of instance counters. The range denotes
        /// the highest id value used so far for a given sort.
        /// The idPool is considered part of model state. It is an 
        /// error if any location of model state contains a value with
        /// object id <i>x</i> such that idPool[<i>x</i>.Sort] &lt; <i>x</i>.Id
        /// </summary>
        Map<Symbol, int> idPool;

        /// <summary>
        /// Object pool for a model program. This is a cache for optimization. It allows
        /// model program implementations to reuse instances across states. This means that 
        /// structures may be cached from state to state.
        /// </summary>
        Dictionary<Symbol, Dictionary<ObjectId, LabeledInstance>> pool;

        /// <summary>
        /// A mapping of sorts (abstract types used to connect model programs) and the .NET types 
        /// that implement them in the current context. This is part of runtime context because 
        /// two model programs may choose different .NET types for the same sort. This dictionary is 
        /// used when interpreting a term representation as a runtime value.
        /// </summary>
        Dictionary<Symbol, Type> sortType;

        /// <summary>
        /// Cache of terms and their interpretations in this context. Note that interpretations are fixed.
        /// </summary>
        Dictionary<Term, IComparable> termValues;

        /// <summary>
        /// Sequence of recorded choices used when reexecuting to a given state.
        /// </summary>
        Sequence<CompoundTerm> choiceOracle;

        Bag<Term> coveragePoints;

        /// <summary>
        /// Runtime context needed to interpret Terms as .NET objects and to pretty print .NET objects as Terms.
        /// Model programs will typically contain one instance of this class.
        /// </summary>
        public InterpretationContext() 
        {
            this.features = Set<string>.EmptySet;
            Clear(); 
        }


        /// <summary>
        /// Runtime context needed to interpret Terms as .NET objects and to pretty print .NET objects as Terms.
        /// Model programs will typically contain one instance of this class.
        /// </summary>
        public InterpretationContext(Set<string> features)
        {
            this.features = features;
            Clear();
        }

        /// <summary>
        /// Cleanup function to free memory prior to disposal of context object.
        /// </summary>
        public void Clear()
        {
            this.idPool = Map<Symbol, int>.EmptyMap;
            this.pool = new Dictionary<Symbol, Dictionary<ObjectId, LabeledInstance>>();
            this.sortType = new Dictionary<Symbol, Type>(); ;
            this.termValues = new Dictionary<Term, IComparable>();
            this.choiceOracle = Sequence<CompoundTerm>.EmptySequence;
            this.coveragePoints = Bag<Term>.EmptyBag;
        }

        /// <summary>
        /// Set of feature keywords that apply in this context.
        /// </summary>
        public Set<string> Features
        {

            get
            {
                return features;
            }
        }

        /// <summary>
        /// Object pool for a model program. This is a cache for optimization. It allows
        /// model program implementations to reuse instances across states. This means that 
        /// structures may be cached from state to state.
        /// </summary>
        public Map<Symbol, int> IdPool
        {
            get 
            { 
                return idPool; 
            }
            set 
            {                 
                idPool = value;             
            }
        }

        /// <summary>
        /// Modifies the current context to ensure that the next object id for <paramref name="label.ObjectSort"/> 
        /// will be greater than <paramref name="label.Id"/>. This method modifies the current
        /// model state. Unlike <see cref="ResetId"/>, this method is incapable of reducing the
        /// value of the next object id. It can only increase it or leave it unchanged.
        /// </summary>
        /// <param name="label">The an object of type <see cref="ObjectId"/> containing the sort (abstract type) and the id of the element to be included</param>
        public void EnsureId(ObjectId label)
        {
            Symbol sort = label.ObjectSort;
            int maxIdValue = label.Id;
            {
                if (maxIdValue < 0) throw new ArgumentOutOfRangeException("maxIdValue");

                if (maxIdValue > 0)
                {
                    int currentMax;
                    if (idPool.TryGetValue(sort, out currentMax))
                    {
                        idPool = idPool.Override(sort, currentMax > maxIdValue ? currentMax : maxIdValue);
                    }
                    else 
                    {
                        idPool = idPool.Add(sort, maxIdValue);
                    }
                }
            }
        }

        /// <summary>
        /// Modifies the current context so that the next object id for <paramref name="sort"/>
        /// is equal <paramref name="maxIdValue"/> plus 1. This method modifies the current
        /// model state.
        /// </summary>
        /// <param name="sort">The sort (abstract type) to modify</param>
        /// <param name="maxIdValue">The id of the maximum element</param>
        public void ResetId(Symbol sort, int maxIdValue)
        {
            if (maxIdValue > 0)
                idPool = idPool.Override(sort, maxIdValue);
            else if (maxIdValue == 0)
                idPool = idPool.RemoveKey(sort);
            else
                throw new ArgumentOutOfRangeException("maxIdValue");
        }

        /// <summary>
        /// Accesses the mapping of sorts (abstract types used to connect model programs) and the .NET types 
        /// that implement them in the current context. This is part of runtime context because 
        /// two model programs may choose different .NET types for the same sort. This mapping is 
        /// used when interpreting a term representation as a runtime value.
        /// </summary> 
        /// <param name="sort">The sort to lookup</param>
        /// <param name="t">The associated type, if found</param>
        /// <returns>True if the an association of sort-to-type was found. See also <see cref="RegisterSortType"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public bool SortTypeTryGetValue(Symbol sort, out Type/*?*/ t)
        {
            return sortType.TryGetValue(sort, out t);
        }

        /// <summary>
        /// Get the default type of the sort
        /// </summary>
        public Type DefaultSortType(Symbol sort)
        {            
            Type result;
            if (sortType.TryGetValue(sort, out result))
                return result;

            string[] namespaces = new string[] { "NModel", "NModel.Internals", "NModel.Terms", "System" };

            if (sort.DomainParameters != null && sort.DomainParameters.Count > 0)
            {
                int nParameters = sort.DomainParameters.Count;
                Symbol genericSort = new Symbol(sort.Name, Sequence<Symbol>.EmptySequence);
                Type genericTypeDefinition = DefaultSortType(genericSort);
                Type[] args = new Type[nParameters];
                int i = 0;
                foreach (Symbol argSort in sort.DomainParameters)
                {
                    args[i] = DefaultSortType(argSort);
                    i += 1;
                }
                result = genericTypeDefinition.MakeGenericType(args); 
            }
            else if (sort.DomainParameters != null)
            {                
                // to do: fix problem of missing arity
                int[] nParameters = new int[] { 1, 2, 3, 4 };
                // assert sort.DomainParameters.Count == 0;
                Type/*?*/ genericType = null;
                foreach (string ns in namespaces)
                {
                    foreach (int i in nParameters)
                    {
                        genericType = Type.GetType(ns + "." + sort.Name + "`" + i.ToString());
                        if (genericType != null)
                            break;
                    }
                    if (genericType != null)
                        break;
                }
                if (genericType == null)
                    throw new ArgumentException("Can't find type for sort " + sort.ToString());

                result = genericType;        
            }
            else if (AbstractValue.IsLiteralSort(sort))
            {
                result = AbstractValue.GetLiteralSortType(sort);
            }
            else
            {
                foreach (string ns in namespaces)
                {
                    result = Type.GetType(ns + "." + sort.Name);
                    if (result != null)
                        break;
                }
            }
            if (result == null)
                // If the execution got here it means that it didn't find the type of sort 
                // in this model and in the NModel framework.
                // Check for the type in the AppDomain assemblies - 
                // Is it a type of another model program that is being composed with this one        
                result = getSortTypeFromAppDomainAssemblies(namespaces, sort.Name);                             

            if (result == null)
                throw new ArgumentException("No default type for sort " + sort.ToString());            

            this.RegisterSortType(sort, result);
            return result;
        }

        /// <summary>
        /// Check for the type in the AppDomain assemblies - 
        /// Is it a type of another model program that is being composed with this one
        /// Exclude from checking all the namespaces that have been checked already
        /// </summary>
        /// <param name="excludeNameSpaces"></param>
        /// <param name="sortTypeName"></param>
        /// <returns>Type - the type of the given sort, or null if a matching type was not found</returns>
        private Type getSortTypeFromAppDomainAssemblies(string[] excludeNameSpaces, string sortTypeName)
        {
            Type thisSortType = null;

            // Get all the assemblies loaded into this AppDomain.
            foreach (Assembly thisAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                int i;
                // Exclude all the namespaces that have been checked already
                for (i = 0; i < excludeNameSpaces.Length; ++i)
                {
                    if (thisAssembly.GetName().Name == excludeNameSpaces[i])
                        break;
                }
                if (i == excludeNameSpaces.Length)
                {
                    foreach (Type t in thisAssembly.GetExportedTypes())
                    {
                        if (t.Name == sortTypeName)
                        {
                            // Found it!
                            thisSortType = t;
                            break;
                        }
                    }                    
                }
                if (thisSortType != null)
                    break;
            }
            // TODO: Value types need different handling. Exclude for now.
            if (thisSortType != null && thisSortType.IsValueType)
                thisSortType = null;
                
            return thisSortType;
        }

        ///// <summary>
        ///// Accesses the mapping of .NET types to sorts (abstract types used to connect model programs). This 
        ///// is part of runtime context because two model programs may choose different .NET types for the same sort. 
        ///// This mapping is used when printing the term representation of a runtime value.
        ///// </summary> 
        ///// <param name="t">The .NET type</param>
        ///// <param name="sort">The associated sort, if found</param>
        ///// <returns>True if an association of type-to-sort was found. See also <see cref="RegisterSortType"/>.</returns>
        //public bool TypeSortTryGetValue(Type t, out Symbol/*?*/ sort)
        //{
        //    return typeSort.TryGetValue(t, out sort);
        //}

        /// <summary>
        /// Initializes this context with a (sort, type) pair.
        /// </summary>
        /// <param name="sort">The sort associated with <paramref name="t"/></param>
        /// <param name="t">The .NET type associated with <paramref name="sort"/></param>
        public void RegisterSortType(Symbol sort, Type t)
        {
            if (sortType.ContainsKey(sort))
            {
                if (!sortType[sort].Equals(t))
                    throw new InvalidOperationException("Sort " + sort.ToString() + " has already been registered in this context with a different type.");
            }
            else
            {
                sortType.Add(sort, t);
            }
        }

        /// <summary>
        /// Accesses this context's object pool. The object pool is a cache for optimization. It allows
        /// model program implementations to reuse instances across states. This means that 
        /// structures may be cached from state to state.
        /// </summary>
        /// <param name="label">The object id to look up</param>
        /// <param name="value">The associated instance, if found</param>
        /// <returns>True if the object id has a value in the pool; false otherwise</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public bool InstancePoolTryGetValue(ObjectId label, out LabeledInstance/*?*/ value)
        {
            Dictionary<ObjectId, LabeledInstance> objPool;
            value = null;
            return pool.TryGetValue(label.ObjectSort, out objPool)
                   && objPool.TryGetValue(label, out value);
        }

        /// <summary>
        /// Returns all of the objects of this instance pool that are relevant in the current context
        /// </summary>
        /// <param name="sort"></param>
        /// <returns></returns>
        public IEnumerable<IComparable> InstancePoolValues(Symbol sort)
        {
            int maxId;
            if (!idPool.TryGetValue(sort, out maxId))
                maxId = -1;

            Dictionary<ObjectId, LabeledInstance> objPool;
            if (pool.TryGetValue(sort, out objPool))
                foreach (LabeledInstance obj in objPool.Values)
                    if (obj.Label.Id <= maxId)
                        yield return obj;
        }

        /// <summary>
        /// Inserts a new value in this context's object pool. See <see cref="InstancePoolTryGetValue"/>.
        /// </summary>
        /// <param name="value"></param>
        public void RegisterValue(LabeledInstance value)
        {
            ObjectId label = value.Label;
            Symbol sort = label.ObjectSort;

            Dictionary<ObjectId, LabeledInstance> objPool;
            if (pool.TryGetValue(sort, out objPool))
            {
                if (objPool.ContainsKey(label))
                    throw new InvalidOperationException("Instance with label " + label.ToString() + " already exists in this context; use static Create() method to construct instead of operator new.");
                else
                    objPool.Add(label, value);
            }
            else
            {
                objPool = new Dictionary<ObjectId, LabeledInstance>();
                objPool.Add(label, value);
                pool.Add(sort, objPool);
            }
        }

        /// <summary>
        /// Reset the pool fields
        /// </summary>
        public void ResetPoolFields()
        {
            foreach (Dictionary<ObjectId, LabeledInstance> objPool in pool.Values)
                foreach (LabeledInstance obj in objPool.Values)
                {
                obj.Initialize();
                }
        }

        #region Current Interpretation Context

        /// <summary>
        /// The current context. This is used when creating LabeledInstance objects to assign labels.
        /// </summary>
        static InterpretationContext currentContext = new InterpretationContext();

        /// <summary>
        /// Previous contexts, as a stack. Contexts are pushed and popped.
        /// </summary>
        static LinkedList<InterpretationContext> contextStack = new LinkedList<InterpretationContext>();

        /// <summary>
        /// Pushes this context as the active context for LabeledInstance creation. Must be paired with a 
        /// <see cref="ClearAsActive"/> call.
        /// </summary>
        public void SetAsActive()
        {
            InterpretationContext.contextStack.AddFirst(InterpretationContext.currentContext);
            InterpretationContext.currentContext = this;
            this.coveragePoints = Bag<Term>.EmptyBag;
        }

        /// <summary>
        /// Adds a new coverage point to the current context. 
        /// </summary>
        /// <param name="coveragePoint">Term denoting a coverage point.</param>
        public void AddCoveragePoint(Term coveragePoint)
        {
            this.coveragePoints = this.coveragePoints.Add(coveragePoint);
        }

        /// <summary>
        /// Returns the coverage points encountered in the current context since the last
        /// set or clear context operation.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Bag<Term> GetCoveragePoints()
        {
            return this.coveragePoints;
        }

        /// <summary>
        /// Pops this context. Must have been preceded by a <see cref="SetAsActive"/> call.
        /// </summary>
        public void ClearAsActive()
        {
            if (InterpretationContext.currentContext == this && InterpretationContext.contextStack.Count > 0)
            {
                InterpretationContext.currentContext = InterpretationContext.contextStack.First.Value;
                InterpretationContext.contextStack.RemoveFirst();
            }
            else
            {
                throw new InvalidOperationException("Mismatched SetAsActive()/ClearAsActive() invocations");
            }
        }

        /// <summary>
        /// Returns the last interpretation context pushed by a <see cref="SetAsActive"/> call.
        /// </summary>
        /// <returns>The current context</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static InterpretationContext GetCurrentContext()
        {
            return currentContext;
        }

        //public IComparable Interpret(Term term)
        //{
        //    // Case 1: Any term is self interpreting
        //    Any a = term as Any;
        //    if (a != null)
        //    {
        //        return a;
        //    }

        //    // Case 2: Literal value
        //    Literal l = term as Literal;
        //    if (l != null)
        //    {
        //        return l.Value;
        //    }

        //    // Case 3: Cached compound term
        //    IComparable result;
        //    if (this.termValues.TryGetValue(term, out result))
        //        return result;

        //    // Case 4: New compound term
        //    CompoundTerm ct = term as CompoundTerm;
        //    if (ct != null)
        //    {
        //        Symbol sort = ct.FunctionSymbol1;
        //        Sequence<IComparable> values = ct.Arguments.Convert<IComparable>(delegate(Term t) { return this.Interpret(t); });

        //        result = this.ConstructValue(sort, values);
        //        this.termValues[term] = result;
        //        return result;
        //    }

        //    // Case 5: Error
        //    throw new ArgumentException("Can't interpret term " + term.ToString());
        //}

        ///// <summary>
        ///// Instantiates a CompoundValue or LabeledInstance with the values given.
        ///// </summary>
        ///// <param name="sort">The sort</param>
        ///// <param name="values">The arguments</param>
        ///// <returns>The constructed value</returns>
        //public IComparable ConstructValue(Symbol sort, Sequence<IComparable> values)
        //{
        //    Type type;
        //    if (this.SortTypeTryGetValue(sort, out type))
        //    {
        //        if (type.IsSubclassOf(typeof(LabeledInstance)))
        //        {
        //            if (values.Count == 1 && args.Head is int)
        //            {
        //                MethodInfo m = type.GetMethod("ImportElement");
        //                d = delegate(Sequence<IComparable> args1)
        //                {
        //                    object o = m.Invoke(null, new object[] { args1.Head });
        //                    return (IComparable)o;
        //                };

        //                deserializers[type] = d;
        //                result = this.Deserialize(d, args);
        //                cache[term] = result;
        //                return result;
        //            }
        //            else
        //                throw new ArgumentException("Can't interpret term"); // to do: throw/catch exception.
        //        }
        //        else if (type.IsSubclassOf(typeof(CompoundValue)))
        //        {
        //            // look for constructor with same number of arguments.
        //            ConstructorInfo[] ctorInfos = type.GetConstructors();
        //            if (ctorInfos != null)
        //            {
        //                int count = args.Count;
        //                foreach (ConstructorInfo ctorInfo in ctorInfos)
        //                {
        //                    ParameterInfo[] paramInfos = ctorInfo.GetParameters();
        //                }

        //            }
        //            else
        //            { // fail 
        //            }
        //        }
        //    }
        //    else
        //    {

        //    }
        //}

        /// <summary>
        /// Delegate for a value contructor
        /// </summary>
        public delegate IComparable ValueConstructor(Sequence<IComparable> arguments);

        /// <summary>
        /// Cached constructor functions (to avoid metadata lookup overhead on a per-call basis)
        /// </summary>
        Dictionary<Symbol, ValueConstructor> valueConstructors = new Dictionary<Symbol,ValueConstructor>();

        /// <summary>
        /// Add a value constructor for the given sort
        /// </summary>
        public void AddValueConstructor(Symbol sort, ValueConstructor constructor)
        {
            valueConstructors.Add(sort, constructor);
        }

        static Symbol quoteSymbol = new Symbol("Quote");

        /// <summary>
        /// Interpret the term as a .NET value in this context
        /// </summary>
        public IComparable InterpretTerm(Term term)
        {
            Any a = term as Any;
            if (a != null)
            {
                return a;
            }

            Literal l = term as Literal;
            if (l != null)
            {
                return l.Value;
            }

            //IComparable result;
            CompoundTerm ct = term as CompoundTerm;
            if (ct != null)
            {
                Symbol sort = ct.Symbol;
                // Case 1: Quoted term. No evaluation needed
                if (quoteSymbol.Equals(sort))
                {
                    return ct.Arguments.Head;
                }
                // Case 2: reconstruct value from sort.
                else
                {
                    int arity = ct.Arguments.Count;
                    ValueConstructor ctor = GetValueConstructor(sort, arity);
                    Sequence<IComparable> args = ct.Arguments.Convert<IComparable>(InterpretTerm);
                    return ctor(args);
                }
            }

            throw new ArgumentException("Can't interpret term " + term.ToString());
        }

        static Type collectionValueType = typeof(CollectionValue);

        ValueConstructor GetValueConstructor(Symbol sort, int arity)
        {        
            ValueConstructor ctor;
            if (!valueConstructors.TryGetValue(sort, out ctor))
            {
                Type type = this.DefaultSortType(sort);

                if (type.IsSubclassOf(typeof(LabeledInstance)))
                {
                    // assert arity == 1;
                    ctor = LabeledInstanceConstructor(type);
                }
                else if (type.IsSubclassOf(typeof(System.Enum)))
                {
                    ctor = delegate(Sequence<IComparable> args1)
                    {
                        return (IComparable)System.Enum.Parse(type, (string) args1.Head);
                    };
                }
                else if (collectionValueType.IsAssignableFrom(type))
                {
                    MethodInfo m = type.GetMethod("ConstructValue", BindingFlags.NonPublic | BindingFlags.Static);
                    ctor = delegate(Sequence<IComparable> args1)
                    {
                        object o = m.Invoke(null, new object[] { args1 });
                        return (IComparable)o;
                    };
                    return ctor;
                }
                else // if (type.IsSubclassOf(typeof(CompoundValue)))
                {
                    // look for constructor with same number of arguments.
                    ConstructorInfo[] ctorInfos = type.GetConstructors();
                    if (ctorInfos != null)
                    {
                        foreach (ConstructorInfo ctorInfo in ctorInfos)
                        {
                            ParameterInfo[] paramInfos = ctorInfo.GetParameters();
                            if ((paramInfos == null && arity == 0) ||
                                (paramInfos != null && paramInfos.Length == arity))
                            {
                                ctor = delegate(Sequence<IComparable> args1)
                                {
                                    IComparable[] args = new IComparable[args1.Count];
                                    args1.CopyTo(args, 0);
                                    object o = ctorInfo.Invoke(args);
                                    return (IComparable)o;
                                };
                                break;
                            }
                        }
                    }
                }
                if (ctor == null)
                  throw new InvalidOperationException("No constructors with arity " +arity+ " found for type " + type.ToString());
                
                valueConstructors[sort] = ctor;
            }
            return ctor;
        }

        static ValueConstructor LabeledInstanceConstructor(Type t)
        {
            ValueConstructor ctor;
            // assert arity == 1
            Type li = typeof(LabeledInstance<>);
            Type li2 = li.MakeGenericType(new Type[]{t});
            MethodInfo m = li2.GetMethod("ImportElement", BindingFlags.Static | BindingFlags.Public);  
            ctor = delegate(Sequence<IComparable> args1)
            {
                object o = m.Invoke(null, new object[] { args1.Head });
                return (IComparable)o;
            };
            return ctor;
        }
        #endregion

        /// <summary>
        /// Choose a term from the given set
        /// </summary>
        public CompoundTerm Choose(Set<CompoundTerm> choices)
        {
            if (choiceOracle.Count == 0)
                throw new ChoiceException(choices);
            else if (choices.Contains(choiceOracle.Head))
            {
                CompoundTerm result = choiceOracle.Head;
                choiceOracle = choiceOracle.Tail;
                return result;
            }
            else
            {
                throw new InvalidOperationException("Value " + choiceOracle.Head.ToString() + " not found in " + choices.ToString());
            }
        }

    }

    /// <summary>
    /// Exception that is thrown when no choice is possible
    /// </summary>
    [Serializable]
    public class ChoiceException : Exception
    {
        Set<CompoundTerm> choices;

        /// <summary>
        /// Constructs a ChoiceException for a given set of choices
        /// </summary>
        public ChoiceException(Set<CompoundTerm> choices) : base()
        {
            this.choices = choices;
        }

        /// <summary>
        /// Constructs a ChoiceException for a given message
        /// </summary>
        public ChoiceException(string msg) : base(msg)
        {
        }

        /// <summary>
        /// Constructs a ChoiceException for a given message and inner exception
        /// </summary>
        public ChoiceException(string msg, Exception e)
            : base(msg, e)
        {
        }

        /// <summary>
        /// Constructs a ChoiceException
        /// </summary>
        public ChoiceException()
            : base()
        {
        }

        /// <summary>
        /// Constructs a ChoiceException
        /// </summary>
        protected ChoiceException(System.Runtime.Serialization.SerializationInfo si, System.Runtime.Serialization.StreamingContext sc)
            : base(si,sc)
        {
        }


        /// <summary>
        /// Get object data for the choice exception
        /// </summary>
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
    

}
