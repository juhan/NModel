using System;
using System.Collections.Generic;
using System.Text;

using NModel.Algorithms;
using NModel;
using NModel.Terms;
using Transition = NModel.Triple<NModel.Terms.Term, NModel.Terms.CompoundTerm, NModel.Terms.Term>;
using Node = NModel.Terms.Term;
using NModel.Execution;
using NModel.Internals;
using NModel.Utilities;
using System.Reflection;



namespace NModel.Algorithms
{


    /// <summary>
    /// A class containing algorithms for performing reachability checks on models.
    /// </summary>
    public class Reachability
    {
        //internal Node initialNode;
        //Set<Node> nodes;
        //Set<Node> acceptingNodes;
        //Set<Node> errorNodes;
        //Set<Transition> transitions;
        //internal Set<Transition> groupingTransitions;
        ModelProgram modelProgram;

        /// <summary>
        /// A field to get or set the model program on which the reachability algorithm is run.
        /// </summary>
        public ModelProgram ModelProgram
        {
            get { return modelProgram; }
            set { modelProgram = value; }
        }
        //internal Dictionary<Node, IState> stateMap;
        //Dictionary<IState, Node> nodeMap;
        //Dictionary<Node, Dictionary<CompoundTerm, Node>> actionsExploredFromNode;
        //Set<Transition> hiddenTransitions;
        //internal int maxTransitions;
        internal bool excludeIsomorphicStates;
        //internal bool collapseExcludedIsomorphicStates;
        //int initTransitions;
        //CompoundTerm goal;



        /// <summary>
        /// Show all transitions from the given node and from the nodes that are reached from 
        /// the given node etc., until the state space is exhausted or the maximum number
        /// of transitions is reached.
        /// </summary>
        public ReachabilityResult CheckReachability()
        {

            uint transCnt = 0;
            uint stateCount = 0;
            //(firstExploration ? (initTransitions < 0 ? maxTransitions : initTransitions) : maxTransitions);
            //firstExploration = false;
            //excludeIsomorphicStates = false;

            if (excludeIsomorphicStates)
            {

                Set<IState> frontier = new Set<IState>(modelProgram.InitialState);
                //stateCount++;
                //Set<IState> visited = Set<IState>.EmptySet;
                StateContainer<IState> visited = new StateContainer<IState>(this.modelProgram,modelProgram.InitialState);
                stateCount++;
                // need to add a check about the initial state.
                Set<string> transitionPropertyNames = Set<string>.EmptySet;
                TransitionProperties transitionProperties;
                while (!frontier.IsEmpty)
                {
                    IState sourceIState = frontier.Choose(0);
                    frontier = frontier.Remove(sourceIState);
                    foreach (Symbol aSymbol in this.modelProgram.PotentiallyEnabledActionSymbols(sourceIState))
                    {
                        foreach (CompoundTerm action in this.modelProgram.GetActions(sourceIState, aSymbol))
                        {
                            //Node targetNode = GetTargetNode(sourceNode, action);
                            IState targetIState = modelProgram.GetTargetState(sourceIState, action, transitionPropertyNames, out transitionProperties);
                            //if (this.modelProgram.IsAccepting(targetIState))
                            //    this.acceptingNodes = this.acceptingNodes.Add(targetNode);
                            //if (!this.modelProgram.SatisfiesStateInvariant(targetState))
                            //    this.errorNodes = this.errorNodes.Add(targetNode);
                            //IState isomorphicState;
                            //Transition t;
                            transCnt++;
                            IState isomorphicState;
                            if (!visited.HasIsomorphic(targetIState, out isomorphicState))
                            {
                                frontier = frontier.Add(targetIState);
                                visited.Add(targetIState);
                                stateCount++;
                                //visited.Add(targetIState);
                                //t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, targetNode);
                            }
                            //else
                            //{
                            //    //if (collapseExcludedIsomorphicStates)
                            //    //    t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, nodeMap[isomorphicState]);
                            //    //else
                            //    {
                            //        Term isoNode = nodeMap[isomorphicState];
                            //        t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, targetNode);
                            //        if (!targetNode.Equals(sourceNode) && !targetNode.Equals(isoNode))
                            //            groupingTransitions = groupingTransitions.Add(new Triple<Term, CompoundTerm, Term>(targetNode, new CompoundTerm(new Symbol("IsomorphicTo"), new Sequence<Term>()), isoNode));
                            //    }
                            //}
                            //this.transitions = this.transitions.Add(t);
                            //this.hiddenTransitions = this.hiddenTransitions.Remove(t);
                        }
                    }
                }

                //Set<IState> frontier = new Set<IState>(stateMap[node]);
                //StateContainer<IState> visited = new StateContainer<IState>(this.modelProgram, stateMap[node]);
                //while (!frontier.IsEmpty && this.transitions.Count < transCnt)
                //{
                //    IState sourceIState = frontier.Choose(0);
                //    Node sourceNode = nodeMap[sourceIState];
                //    frontier = frontier.Remove(sourceIState);
                //    foreach (Symbol aSymbol in this.modelProgram.PotentiallyEnabledActionSymbols(sourceIState))
                //    {
                //        foreach (CompoundTerm action in this.modelProgram.GetActions(sourceIState, aSymbol))
                //        {
                //            Node targetNode = GetTargetNode(sourceNode, action);
                //            IState targetIState = stateMap[targetNode];
                //            IState isomorphicState;
                //            Transition t;
                //            if (!visited.HasIsomorphic(targetIState, out isomorphicState))
                //            {
                //                frontier = frontier.Add(targetIState);
                //                //visited = visited.Add(targetIState);
                //                visited.Add(targetIState);
                //                t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, targetNode);
                //            }
                //            else
                //            {
                //                if (collapseExcludedIsomorphicStates)
                //                    t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, nodeMap[isomorphicState]);
                //                else
                //                {
                //                    Term isoNode = nodeMap[isomorphicState];
                //                    t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, targetNode);
                //                    if (!targetNode.Equals(sourceNode) && !targetNode.Equals(isoNode))
                //                        groupingTransitions = groupingTransitions.Add(new Triple<Term, CompoundTerm, Term>(targetNode, new CompoundTerm(new Symbol("IsomorphicTo"), new Sequence<Term>()), isoNode));
                //                }
                //            }
                //            this.transitions = this.transitions.Add(t);
                //            this.hiddenTransitions = this.hiddenTransitions.Remove(t);
                //        }
                //    }
                //}
                ////Console.WriteLine(dashedTransitions.ToString());
                ////Console.WriteLine(visited.ToString());
            }
            else
            {

                Set<IState> frontier = new Set<IState>(modelProgram.InitialState);
                stateCount++;
                // need to add a check about the initial state.
                Set<IState> visited = new Set<IState>(modelProgram.InitialState);
                Set<string> transitionPropertyNames = Set<string>.EmptySet;
                TransitionProperties transitionProperties;
                while (!frontier.IsEmpty)
                {
                    IState sourceIState = frontier.Choose(0);
                    frontier = frontier.Remove(sourceIState);
                    foreach (Symbol aSymbol in this.modelProgram.PotentiallyEnabledActionSymbols(sourceIState))
                    {
                        foreach (CompoundTerm action in this.modelProgram.GetActions(sourceIState, aSymbol))
                        {
                            //Node targetNode = GetTargetNode(sourceNode, action);
                            IState targetIState = modelProgram.GetTargetState(sourceIState, action, transitionPropertyNames, out transitionProperties);
                            //Console.WriteLine(sourceIState.ToString());
                            //Console.WriteLine ("--- " + action + " --->");
                            //Console.WriteLine (targetIState.ToString());
                            //Console.WriteLine();
                            //Console.WriteLine();
                            //if (this.modelProgram.IsAccepting(targetIState))
                            //    this.acceptingNodes = this.acceptingNodes.Add(targetNode);
                            //if (!this.modelProgram.SatisfiesStateInvariant(targetState))
                            //    this.errorNodes = this.errorNodes.Add(targetNode);
                            //IState isomorphicState;
                            //Transition t;
                            transCnt++;
                            if (!visited.Contains(targetIState))
                            {
                                frontier = frontier.Add(targetIState);
                                visited = visited.Add(targetIState);
                                stateCount++;
                                //visited.Add(targetIState);
                                //t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, targetNode);
                            }
                            //else
                            //{
                            //    //if (collapseExcludedIsomorphicStates)
                            //    //    t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, nodeMap[isomorphicState]);
                            //    //else
                            //    {
                            //        Term isoNode = nodeMap[isomorphicState];
                            //        t = new Triple<Term, CompoundTerm, Term>(sourceNode, action, targetNode);
                            //        if (!targetNode.Equals(sourceNode) && !targetNode.Equals(isoNode))
                            //            groupingTransitions = groupingTransitions.Add(new Triple<Term, CompoundTerm, Term>(targetNode, new CompoundTerm(new Symbol("IsomorphicTo"), new Sequence<Term>()), isoNode));
                            //    }
                            //}
                            //this.transitions = this.transitions.Add(t);
                            //this.hiddenTransitions = this.hiddenTransitions.Remove(t);
                        }
                    }
                }





            }

            //Console.WriteLine("Checker results");
            //Console.WriteLine("Reached states: "+stateCount);
            //Console.WriteLine("Number of transitions: "+transCnt);
            ReachabilityResult result = new ReachabilityResult(stateCount, transCnt);
            return result;
        }


        /// <summary>
        /// A method that is used by the command line interface.
        /// </summary>
        /// <param name="args"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void RunWithCommandLineArguments(string[] args)
        {
            MCCmdLineParams settings = new MCCmdLineParams();
            if (!Parser.ParseArgumentsWithUsage(args, settings))
            {
                return;
            }

            

            #region load the libraries
            List<Assembly> libs = new List<Assembly>();
            try
            {

                if (settings.reference != null)
                {
                    foreach (string l in settings.reference)
                    {
                        libs.Add(System.Reflection.Assembly.LoadFrom(l));
                    }
                }
            }
            catch (Exception e)
            {
                throw new ModelProgramUserException(e.Message);
            }
            #endregion

            #region load the test cases if any
            CompoundTerm goal = null;
            if (!String.IsNullOrEmpty(settings.goal))
            {
                try
                {
                    System.IO.StreamReader goalReader =
                        new System.IO.StreamReader(settings.goal);
                    string goalAsString = goalReader.ReadToEnd();
                    goalReader.Close();
                    goal = CompoundTerm.Parse(goalAsString);
                }
                catch (Exception e)
                {
                    throw new ModelProgramUserException("Cannot create goal: " + e.Message);
                }
            }
            else
            {
                Console.WriteLine("No goal was specified, counting distinct states and transitions.");
                Console.WriteLine("Invalid end states check currently not enabled.");
            }
            #endregion

            #region create a model program for each model using the factory method and compose into product
            string mpMethodName;
            string mpClassName;
            ModelProgram mp = null;
            if (settings.model != null && settings.model.Length > 0)
            {
                if (libs.Count == 0)
                {
                    throw new ModelProgramUserException("No reference was provided to load models from.");
                }
                ReflectionHelper.SplitFullMethodName(settings.model[0], out mpClassName, out mpMethodName);
                Type mpType = ReflectionHelper.FindType(libs, mpClassName);
                MethodInfo mpMethod = ReflectionHelper.FindMethod(mpType, mpMethodName, Type.EmptyTypes, typeof(ModelProgram));
                try
                {
                    mp = (ModelProgram)mpMethod.Invoke(null, null);
                }
                catch (Exception e)
                {
                    throw new ModelProgramUserException("Invocation of '" + settings.model[0] + "' failed: " + e.ToString());
                }
                for (int i = 1; i < settings.model.Length; i++)
                {
                    ReflectionHelper.SplitFullMethodName(settings.model[i], out mpClassName, out mpMethodName);
                    mpType = ReflectionHelper.FindType(libs, mpClassName);
                    mpMethod = ReflectionHelper.FindMethod(mpType, mpMethodName, Type.EmptyTypes, typeof(ModelProgram));
                    ModelProgram mp2 = null;
                    try
                    {
                        mp2 = (ModelProgram)mpMethod.Invoke(null, null);
                    }
                    catch (Exception e)
                    {
                        throw new ModelProgramUserException("Invocation of '" + settings.model[i] + "' failed: " + e.ToString());
                    }
                    mp = new ProductModelProgram(mp, mp2);
                }
            }
            #endregion

            if (mp == null)
            {
                Console.WriteLine("ModelProgram was null");
                Console.WriteLine("Tried to instantiate:");
                if (settings.model!=null)
                    foreach (string s in settings.model)
                        Console.WriteLine(s);
                return;
            }
            Reachability mc = new Reachability();
            mc.excludeIsomorphicStates = settings.excludeIsomorphic;
            mc.modelProgram = mp;
            DateTime before = DateTime.Now;
            ReachabilityResult result = mc.CheckReachability();
            DateTime after = DateTime.Now;
            Console.WriteLine("Results of reachability checking:");
            Console.WriteLine();
            Console.WriteLine(" States reached: " + result.StateCount);
            Console.WriteLine(" Transitions covered: "+ result.TransitionCount);

        }

    }

    /// <summary>
    /// A class containing the result of performing reachability.
    /// Currently only the number of states and transitions visited.
    /// </summary>
    public sealed class ReachabilityResult {
        uint stateCount = 0;

        /// <summary>
        /// Field representing the count of states encountered in state space traversal.
        /// </summary>
        [CLSCompliantAttribute(false)]
        public uint StateCount
        {
            get { return stateCount; }
            set { stateCount = value; }
        }
        uint transitionCount = 0;

        /// <summary>
        /// Field representing the count of transitions encountered in state space traversal.
        /// </summary>
        [CLSCompliantAttribute(false)]
        public uint TransitionCount
        {
            get { return transitionCount; }
            set { transitionCount = value; }
        }

        /// <summary>
        /// A convenience constructor for producing a result with appropriate state and transition counts.
        /// </summary>
        /// <param name="stateCount"></param>
        /// <param name="transitionCount"></param>
        [CLSCompliantAttribute(false)]
        public ReachabilityResult(UInt32 stateCount, UInt32 transitionCount)
        {
            this.stateCount = stateCount;
            this.transitionCount = transitionCount;
        }

    
    }



    internal sealed class MCCmdLineParams
    {
        const int M = 1024 * 1024;

        public MCCmdLineParams()
        {
            model = null;
            reference = null;
            goal = null;
            maxMemory = 100 * M;
            timeLimit = 100;
            excludeIsomorphic = false;
            steps = 0;
            logfile = null;
            overwriteLog = true;
            randomSeed = 0;
            maxSteps = 0;
            fsm = null;
            startMCAction = "ModelCheck";
            waitAction = "Wait";
            timeoutAction = "Timeout";
        }

        [DefaultArgument(ArgumentType.MultipleUnique, HelpText = "Fully qualified names of factory methods returning an object that implements ModelProgram. Multiple models are composed into a product.")]
        public string[] model;

        [Argument(ArgumentType.AtLeastOnce | ArgumentType.MultipleUnique, ShortName = "r", HelpText = "Referenced assemblies.")]
        public string[] reference;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "g", HelpText = "Goal term.")]
        public string goal;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = 0, HelpText = "The desired number of steps that a single test run should have. After the number is reached, only cleanup tester actions are used and the test run continues until an accepting state is reached or the number of steps is MaxSteps (whichever occurs first). 0 implies no bound and a test case is executed until either a conformance failure occurs or no more actions are enabled. (If a testSuite is provided, this value is set to 0.)")]
        public int steps;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = 0, HelpText = "The maximum number of steps that a single test run can have. This value must be either 0, which means that there is no bound, or greater than or equal to steps.")]
        public int maxSteps;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "m", DefaultValue = 0, HelpText = "Maximum memory for the process to use.")]
        public int maxMemory;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "t", DefaultValue = 0, HelpText = "Maximum time to run (in seconds).")]
        public int timeLimit;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = 10000, HelpText = "The amount of time in milliseconds within which a tester action must return when passed to the implementation stepper.")]
        public int timeout = 10000;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "i", DefaultValue = false, HelpText = "Consider states with isomorphic structure to be visited.")]
        public bool excludeIsomorphic = false;
        
        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = true, HelpText = "Continue testing when a conformance failure occurs.")]
        public bool continueOnFailure = true;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "log", HelpText = "Filename where test results are logged. The console is used if no logfile is provided.")]
        public string logfile;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "seed", DefaultValue = 0, HelpText = "A number used to calculate the starting value for the pseudo-random number sequence that is used by the global choice controller to select tester actions. If a negative number is specified, the absolute value is used. If left unspecified or if 0 is provided a random number is generated as the seed.")]
        public int randomSeed;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = true, HelpText = "If true the log file is overwritten, otherwise the testresults are appended to the logfile")]
        public bool overwriteLog;

        [Argument(ArgumentType.MultipleUnique, ShortName = "", HelpText = "File name of a file containing the term representation fsm.ToTerm() of an fsm (object of type FSM). Multiple fsms are composed into a product.")]
        public string[] fsm;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = "Test", HelpText = "Name of start action of a test case. This value is used only if a testSuite is provided. The default 'Test' action sybmol is considered as an internal test action symbol. If another action symbol is provided it is not considered as being internal by default.")]
        public string startMCAction;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = "Wait", HelpText = "A name of an action that is used to wait for observable actions in a state where no controllable actions are enabled. A wait action is controllable and internal and must take one integer argument that determines the time to wait in milliseconds during which an observable action is expected. Only used with IAsyncStepper.")]
        public string waitAction;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = "Timeout", HelpText = "A name of an action that happens when a wait action has been executed and no obsevable action occurred within the time limit provided in the wait action. A timeout action is observable and takes no arguments. Only used with IAsyncStepper.")]
        public string timeoutAction;

        [Argument(ArgumentType.AtMostOnce, ShortName = "n", HelpText = "The name of the FSM")]
        public string fsmName = "";

        //[Argument(ArgumentType.Required, ShortName = "fsm", HelpText = "The name of the file containing the FSM")]
        //public string fsmFileName = "";

        //[Argument(ArgumentType.AtMostOnce, ShortName = "dot", HelpText = "The name of the file where Dot goes")]
        //public string dotFileName;

    }
}
