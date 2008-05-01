using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Microsoft.ModelProgram.Conformance
{
    /// <summary>
    /// This class allows to dynamically create delegate handlers for
    /// arbitrary delegate types, and to register and deregister such event handlers.
    /// </summary>
    internal class EventHandlerRepository
    {
        private static object @lock = new object();
        private static ModuleBuilder currentModuleBuilder = null;
        private static Hashtable eventHandlerBuilders = new Hashtable(); // EventInfo -> EventHandlerBuilder

        private static EventHandlerBuilder getEventHandlerBuilder(EventInfo @event)
        {
            if (@event == null)
                throw new System.ArgumentNullException("event");
            lock (@lock)
            {
                EventHandlerBuilder ehb = eventHandlerBuilders[@event] as EventHandlerBuilder;
                if (ehb == null)
                {
                    if (currentModuleBuilder == null)
                    {
                        AppDomain myDomain = Thread.GetDomain();
                        AssemblyName assemblyName = new AssemblyName();
                        assemblyName.Name = "EventHandlerRepositoryAssembly";
                        AssemblyBuilder assemblyBuilder = myDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                        currentModuleBuilder = assemblyBuilder.DefineDynamicModule("EventHandlerRepositoryModule");
                    }
                    ehb = new EventHandlerBuilder(currentModuleBuilder, @event);
                    eventHandlerBuilders[@event] = ehb;
                }
                return ehb;
            }
        }

      
        /// <summary>
        /// Create a delegate for the given event and instance
        /// that matches the delegate type of the event
        /// </summary>
        public static System.Delegate CreateAndAdd(EventInfo @event, IObservationCallback instance)
        {
            return getEventHandlerBuilder(@event).CreateAndAdd(instance);
        }

        /// <summary>
        /// Remove the event and the associated delegate
        /// </summary>
        public static void Remove(EventInfo @event, IObservationCallback instance, System.Delegate delegateInstance)
        {
            getEventHandlerBuilder(@event).Remove(instance, delegateInstance);
        }

        /// <summary>
        /// Reset the repository to its initial state,
        /// remove all events.
        /// </summary>
        public void Reset()
        {
            currentModuleBuilder = null;
            eventHandlerBuilders.Clear();
            EventHandlerBuilder.typeIndex = 0;
        }

        internal class EventHandlerBuilder
        {
            private ModuleBuilder module;
            private EventInfo forEvent;

            private FieldBuilder eventField = null;
            private FieldBuilder instanceField = null;
            private TypeBuilder type = null;
            private ConstructorBuilder constructorBuilder = null;
            private ConstructorInfo constructor = null;

            private bool created = false;

            //used to guaratee unique class names
            internal static int typeIndex = 0;

            //used to ensure thread safety
            private readonly object @lock = new object();

            /// <summary>
            /// Construct an event handler builder
            /// </summary>
            /// <param name="module">given module</param>
            /// <param name="forEvent">given event</param>
            public EventHandlerBuilder(ModuleBuilder module, EventInfo forEvent)
            {
                if (module == null)
                    throw new System.ArgumentNullException("module");
                if (forEvent == null)
                    throw new System.ArgumentNullException("forEvent");
                this.module = module;
                this.forEvent = forEvent;
            }

            /// <summary>
            /// Create and add the eventhandler for the given event
            /// </summary>
            /// <param name="instance"></param>
            /// <returns>the event handler</returns>
            internal System.Delegate CreateAndAdd(object instance)
            {
                lock (@lock)
                {
                    if (debug) DEBUG("CreateAndAdd " + this.forEvent.ToString() + " to instance=" + (instance == null ? "null" : instance.ToString()));

                    create();

                    if (this.constructor == null)
                    {
                        this.constructor = this.type.GetConstructor(new System.Type[] { typeof(EventInfo), typeof(object) });
                        if (this.constructor == null)
                            throw new System.InvalidOperationException("cannot find generated constructor for " + this.type.ToString());
                    }

                    object container = this.constructor.Invoke(new object[] { this.forEvent, instance });
                    Delegate delegateInstance = System.Delegate.CreateDelegate(this.forEvent.EventHandlerType, container, "OnEvent");
                    this.forEvent.AddEventHandler(instance, delegateInstance);
                    return delegateInstance;
                }
            }


            /// <summary>
            /// Remove the given event handler of the given event.
            /// </summary>
            /// <param name="instance"></param>
            /// <param name="delegateInstance"></param>
            internal void Remove(object instance, System.Delegate eventHandler)
            {
                lock (@lock)
                {
                    if (debug) DEBUG("Remove " + this.forEvent.ToString() + " from instance=" + instance == null ? "null" : instance.ToString());

                    this.forEvent.RemoveEventHandler(instance, eventHandler);
                }
            }

            /// <summary>
            /// Create and load the type for the given event
            /// </summary>
            internal System.Type CreateType()
            {
                lock (@lock)
                {
                    return create();
                }
            }

            #region Create the type
            private System.Type create()
            {
                build();
                if (!this.created)
                {
                    this.created = true;
                    this.type.CreateType(); //load the type
                }
                return this.type;
            }

            private void build()
            {
                if (this.type == null)
                {
                    this.type = module.DefineType("$EventContainer$" + forEvent.Name + "$" + (typeIndex++).ToString(),
                        TypeAttributes.Public);

                    this.eventField = type.DefineField("event", typeof(EventInfo), FieldAttributes.Private);
                    this.instanceField = type.DefineField("instance", typeof(object), FieldAttributes.Private);

                    addConstructorMethod();
                    AddObserveMethod();
                }
            }

            #region Dynamic constructor creation
            private void addConstructorMethod()
            {
                this.constructorBuilder = type.DefineConstructor(
                    MethodAttributes.Public | MethodAttributes.HideBySig,
                    CallingConventions.Standard,
                    new System.Type[] { typeof(EventInfo), typeof(object) });

                ILGenerator g = this.constructorBuilder.GetILGenerator();

                // call super-constructorBuilder
                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Call, typeof(object).GetConstructor(new System.Type[0]));

                // initialize field "event"
                g.Emit(OpCodes.Ldarg_0); // push "this"
                g.Emit(OpCodes.Ldarg_1); // push "event"
                g.Emit(OpCodes.Stfld, this.eventField);

                // initialize field "instance"
                g.Emit(OpCodes.Ldarg_0); // push "this"
                g.Emit(OpCodes.Ldarg_2); // push "instance"
                g.Emit(OpCodes.Stfld, this.instanceField);

                g.Emit(OpCodes.Ret);
            }
            #endregion

            #region Dynamic observer method creation

            private void AddObserveMethod()
            {
                MethodInfo eventInvokeMethod = this.forEvent.EventHandlerType.GetMethod("Invoke");
                ParameterInfo[] invokeParameters = eventInvokeMethod.GetParameters();
                System.Type[] parameters = new System.Type[invokeParameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                    parameters[i] = invokeParameters[i].ParameterType;
                System.Type returnType = eventInvokeMethod.ReturnType;

                AddMethodDynamically(type, "OnEvent", parameters, returnType);
            }

            private void AddMethodDynamically(TypeBuilder myTypeBld,
                     string mthdName,
                     Type[] parameters,
                     Type returnType)
            {

                MethodBuilder onEventMethodBuilder = myTypeBld.DefineMethod(
                                  mthdName,
                                  MethodAttributes.Public,
                                  returnType,
                                  parameters);

                int numParams = parameters.Length;

                ILGenerator g = onEventMethodBuilder.GetILGenerator();

                //declare local variable nr 0 to create object array
                g.DeclareLocal(typeof(object[]));
                g.Emit(OpCodes.Nop);
                //load EventInfo
                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Ldfld, this.eventField);
                //load instance
                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Ldfld, this.instanceField);

                //create array of parameters as objects
                EmitLdc(g, parameters.Length);
                g.Emit(OpCodes.Newarr, typeof(object));
                g.Emit(OpCodes.Stloc_0);

                //store the arguments into the array, box value types
                for (int i = 0; i < parameters.Length; i++)
                {
                    g.Emit(OpCodes.Ldloc_0);  //load array ref
                    EmitLdc(g, i);            //load offset
                    EmitLdarg(g, i + 1);      //load argument (arg 0 is the instance)
                    if (parameters[i].IsValueType)
                        g.Emit(OpCodes.Box, parameters[i]);
                    g.Emit(OpCodes.Stelem_Ref);
                }
                g.Emit(OpCodes.Ldloc_0);

                g.Emit(OpCodes.Call, typeof(EventHandlerRepositoryHelper).GetMethod("__OnEvent"));

                // return something if returnvalue is not void
                if (returnType.IsClass || returnType.IsInterface)
                    g.Emit(OpCodes.Ldnull);
                else if (returnType != typeof(void))
                    g.Emit(OpCodes.Ldc_I4_0);
                else
                    g.Emit(OpCodes.Nop);

                g.Emit(OpCodes.Ret);
            }

            static void EmitLdc(ILGenerator g, int i)
            {
                switch (i)
                {
                    case 0:
                        g.Emit(OpCodes.Ldc_I4_0);
                        break;
                    case 1:
                        g.Emit(OpCodes.Ldc_I4_1);
                        break;
                    case 2:
                        g.Emit(OpCodes.Ldc_I4_2);
                        break;
                    case 3:
                        g.Emit(OpCodes.Ldc_I4_3);
                        break;
                    case 4:
                        g.Emit(OpCodes.Ldc_I4_4);
                        break;
                    case 5:
                        g.Emit(OpCodes.Ldc_I4_5);
                        break;
                    case 6:
                        g.Emit(OpCodes.Ldc_I4_6);
                        break;
                    case 7:
                        g.Emit(OpCodes.Ldc_I4_7);
                        break;
                    case 8:
                        g.Emit(OpCodes.Ldc_I4_8);
                        break;
                    default:
                        g.Emit(OpCodes.Ldc_I4, i);
                        break;
                }
            }

            static void EmitLdarg(ILGenerator g, int i)
            {
                switch (i)
                {
                    case 0:
                        g.Emit(OpCodes.Ldarg_0);
                        break;
                    case 1:
                        g.Emit(OpCodes.Ldarg_1);
                        break;
                    case 2:
                        g.Emit(OpCodes.Ldarg_2);
                        break;
                    case 3:
                        g.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        g.Emit(OpCodes.Ldarg_S, i);
                        break;
                }
            }

            #endregion

            #endregion

            #region Debugging
            readonly bool debug = System.Environment.GetEnvironmentVariable("DEBUGEVENTHANDLERS") != null;
            private void DEBUG(string s) { Console.WriteLine("event handlers: " + s); }
            #endregion
        }
    }



}

