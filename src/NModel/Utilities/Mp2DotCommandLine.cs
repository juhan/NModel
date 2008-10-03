using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Reflection;
using NModel.Terms;
using NModel.Algorithms;
using NModel.Execution;
using Transition = NModel.Triple<NModel.Terms.Term, NModel.Terms.CompoundTerm, NModel.Terms.Term>;

namespace NModel.Utilities.Graph
{
    // This file is similar to ModelProgramViewerCommandLine.cs, in namespace NModel.Visualization
    // Here we are different namespace so we can keep the same type names

    /// <summary>
    /// Represents a commandline utility that starts up mp2dot and writes out dot script for
    /// a product composition of provided model programs.
    /// </summary>
    public static class CommandLineViewer
    {
        /// <summary>
        /// Provides programmatic access to the commandline utility 'mp2dot.exe'.
        /// </summary>
        /// <param name="args">command line arguments: model program(s), optional settings for the viewer</param>
        /// <remarks>The settings are displayed when 'mpv.exe /?' is executed from the command line without arguments.</remarks>
        public static void RunWithCommandLineArguments(params string[] args)
        {
            ProgramSettings settings = new ProgramSettings();
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

            #region create a model program from given namespace and feature names
            if (settings.mp != null && settings.mp.Length > 0)
            {
                if (libs.Count == 0)
                {
                    throw new ModelProgramUserException("No reference was provided to load models from.");
                }
                //parse the model program name and the feature names for each entry
                foreach (string mps in settings.mp)
                {
                    //the first element is the model program, the remaining ones are 
                    //feature names
                    string[] mpsSplit = mps.Split(new string[] { "[", "]", "," },
                        StringSplitOptions.RemoveEmptyEntries);
                    if (mpsSplit.Length == 0)
                    {
                        throw new ModelProgramUserException("Invalid model program specifier '" + mps + "'.");
                    }
                    string mpName = mpsSplit[0];
                    Assembly mpAssembly = ReflectionHelper.FindAssembly(libs, mpName);
                    Set<string> mpFeatures = new Set<string>(mpsSplit).Remove(mpName);
                    ModelProgram mp1 = new LibraryModelProgram(mpAssembly, mpName, mpFeatures);
                    mp = (mp == null ? mp1 : new ProductModelProgram(mp, mp1));
                }
            }

            #endregion

            #region load the test cases if any
            Sequence<Sequence<CompoundTerm>> testcases = Sequence<Sequence<CompoundTerm>>.EmptySequence;
            if (!String.IsNullOrEmpty(settings.testSuite))
            {
                try
                {
                    System.IO.StreamReader testSuiteReader =
                        new System.IO.StreamReader(settings.testSuite);
                    string testSuiteAsString = testSuiteReader.ReadToEnd();
                    testSuiteReader.Close();
                    CompoundTerm testSuite = CompoundTerm.Parse(testSuiteAsString);
                    foreach (CompoundTerm testCaseTerm in testSuite.Arguments)
                    {
                        Sequence<CompoundTerm> testCase =
                            testCaseTerm.Arguments.Convert<CompoundTerm>(delegate(Term t) { return (CompoundTerm)t; });
                        testcases = testcases.AddLast(testCase);
                    }
                }
                catch (Exception e)
                {
                    throw new ModelProgramUserException("Cannot create test suite: " + e.Message);
                }
            }
            #endregion

            #region load the fsms if any
            Dictionary<string, FSM> fsms = new Dictionary<string, FSM>();
            if (settings.fsm != null && settings.fsm.Length > 0)
            {
                try
                {
                    foreach (string fsmFile in settings.fsm)
                    {
                        System.IO.StreamReader fsmReader = new System.IO.StreamReader(fsmFile);
                        string fsmAsString = fsmReader.ReadToEnd();
                        fsmReader.Close();
                        fsms[fsmFile] = FSM.FromTerm(CompoundTerm.Parse(fsmAsString));
                    }
                }
                catch (Exception e)
                {
                    throw new ModelProgramUserException("Cannot create fsm: " + e.Message);
                }
            }
            #endregion

            if (mp == null && testcases.IsEmpty && fsms.Count == 0)
            {
                throw new ModelProgramUserException("No model, fsm, or test suite was given.");
            }

            if (!testcases.IsEmpty)
            {
                FSM fa = FsmTraversals.GenerateTestSequenceAutomaton(
                    settings.startTestAction, testcases, GetActionSymbols(testcases));
                ModelProgram famp = new FsmModelProgram(fa, settings.testSuite);
                if (mp == null)
                    mp = famp;
                else
                    mp = new ProductModelProgram(mp, famp);
            }

            if (fsms.Count > 0)
            {
                foreach (string fsmName in fsms.Keys)
                {
                    ModelProgram fsmmp = new FsmModelProgram(fsms[fsmName], fsmName);
                    if (mp == null)
                        mp = fsmmp;
                    else
                        mp = new ProductModelProgram(mp, fsmmp);
                }
            }

            // must replace form.View with something that doesn't depend on Windows.Forms or GLEE
            //ModelProgramGraphViewForm form = new ModelProgramGraphViewForm("Model Program Viewer");
            DummyGraphViewForm form = new DummyGraphViewForm("mp2dot");
            //configure the settings of the viewer
            form.View.AcceptingStatesMarked = settings.acceptingStatesMarked;
            form.View.TransitionLabels = settings.transitionLabels;
            form.View.CombineActions = settings.combineActions;
            form.View.Direction = settings.direction;
            form.View.UnsafeStateColor = Color.FromName(settings.unsafeStateColor);
            form.View.HoverColor = Color.FromName(settings.hoverColor);
            form.View.InitialStateColor = Color.FromName(settings.initialStateColor);
            form.View.LoopsVisible = settings.loopsVisible;
            form.View.MaxTransitions = settings.maxTransitions;
            form.View.NodeLabelsVisible = settings.nodeLabelsVisible;
            form.View.SelectionColor = Color.FromName(settings.selectionColor);
            form.View.MergeLabels = settings.mergeLabels;
            form.View.StateShape = settings.stateShape;
            form.View.DeadStateColor = Color.FromName(settings.deadStateColor);
            form.View.InitialTransitions = settings.initialTransitions;
            form.View.LivenessCheckIsOn = settings.livenessCheckIsOn;
            form.View.ExcludeIsomorphicStates = settings.excludeIsomorphicStates;
            form.View.SafetyCheckIsOn = settings.safetyCheckIsOn;
            form.View.DeadstatesVisible = settings.deadStatesVisible;

            form.View.SetModelProgram(mp);
            form.View.graphWorker();
            form.View.WriteExplorationStatistics();

            if (!String.IsNullOrEmpty(settings.dotFileName))
            {
                WriteDotFile(form, settings.dotFileName);
            }

            if (!String.IsNullOrEmpty(settings.machineFileName))
            {
                WriteFSMFile(form, settings.machineFileName);
            }
        }

        static void WriteDotFile(DummyGraphViewForm form, string dotFileName)
        {
            // Console.WriteLine("Preparing to write dot file {0}", dotFileName);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(dotFileName);
            string dot = form.View.ToDot();
            sw.Write(dot);
            sw.Close();
        }

        static void WriteFSMFile(DummyGraphViewForm form, string machineFileName)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(machineFileName);
            string fsmAsString = form.View.finiteAutomatonContext.fa.ToString();
            sw.Write(fsmAsString);
            sw.Close();
        }

        // Copied from ModelProgramViewerCommandLine.cs, do we even need it here?
        private static Set<Symbol> GetActionSymbols(Sequence<Sequence<CompoundTerm>> testcases)
        {
            Set<Symbol> symbs = Set<Symbol>.EmptySet;
            foreach (Sequence<CompoundTerm> testcase in testcases)
                foreach (CompoundTerm action in testcase)
                    symbs = symbs.Add(action.Symbol);
            return symbs;
        }
    }

    internal sealed class ProgramSettings
    {
        public ProgramSettings()
        {
            model = null;
            mp = null;
            reference = null;
            combineActions = false;
            livenessCheckIsOn = false;
            safetyCheckIsOn = false;
            testSuite = "";
            fsm = null;
            startTestAction = "Test";
            excludeIsomorphicStates = false;
            collapseExcludedIsomorphicStates = false;
            stateViewVisible = false;
            dotFileName = null;
            machineFileName = null;
        }

        [DefaultArgument(ArgumentType.MultipleUnique, HelpText = "Fully qualified names of factory methods returning an object that implements ModelProgram. Multiple model programs are composed into a product.")]
        public string[] model;

        [Argument(ArgumentType.MultipleUnique, ShortName = "", HelpText = "Model programs given on the form M or M[F1,...,Fn] where M is a model program name (namespace) and each Fi is a feature in M. Multiple model programs are composed into a product. No factory method is needed if this option is used.")]
        public string[] mp;

        [Argument(ArgumentType.MultipleUnique, ShortName = "r", HelpText = "Referenced assemblies.")]
        public string[] reference;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = -1, ShortName = "", HelpText = "Number of transitions that are explored initially up to maxTransitions. Negative value implies no bound.")]
        public int initialTransitions = -1;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = TransitionLabel.Action, ShortName = "", HelpText = "Determines what is shown as a transition label.")]
        public TransitionLabel transitionLabels = TransitionLabel.Action;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = true, ShortName = "", HelpText = "Visibility of node labels.")]
        public bool nodeLabelsVisible = true;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = "LightGray", ShortName = "", HelpText = "Background color of the initial state.")]
        public string initialStateColor = "LightGray";

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = "Lime", ShortName = "", HelpText = "Line and action label color to use when edges or nodes are hovered over.")]
        public string hoverColor = "Lime";

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = "Blue", ShortName = "", HelpText = "Background color to use when a node is selected.")]
        public string selectionColor = "Blue";

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = "Yellow", ShortName = "", HelpText = "Background color of dead states. Dead states are states from which no accepting state is reachable.")]
        public string deadStateColor = "Yellow";

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = true, ShortName = "", HelpText = "Visibility of dead states.")]
        public bool deadStatesVisible = true;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = "Red", ShortName = "", HelpText = "Background color of states that violate a safety condition (state invariant).")]
        public string unsafeStateColor = "Red";

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = 100, ShortName = "", HelpText = "Maximum number of transitions to draw in the graph.")]
        public int maxTransitions = 100;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = true, ShortName = "", HelpText = "Visibility of transitions whose start and end states are the same.")]
        public bool loopsVisible = true;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = true, ShortName = "", HelpText = "Multiple transitions between same start and end states are shown as one transition with a merged label.")]
        public bool mergeLabels = true;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = true, ShortName = "", HelpText = "Mark accepting states with a bold outline.")]
        public bool acceptingStatesMarked = true;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = StateShape.Ellipse, ShortName = "", HelpText = "State shape.")]
        public StateShape stateShape = StateShape.Ellipse;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = GraphDirection.TopToBottom, ShortName = "", HelpText = "Direction of graph layout.")]
        public GraphDirection direction = GraphDirection.TopToBottom;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = false, ShortName = "", HelpText = "Whether to view matching start and finish actions by a single label.")]
        public bool combineActions;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = false, ShortName = "", HelpText = "Mark states from which no accepting state is reachable in the current view.")]
        internal bool livenessCheckIsOn;

        [Argument(ArgumentType.LastOccurenceWins, DefaultValue = false, ShortName = "", HelpText = "Mark states that violate a safety condition (state invariant).")]
        internal bool safetyCheckIsOn;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", HelpText = "File name of a file containing a sequence of actions sequences (test cases) to be viewed.")]
        public string testSuite;

        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "", HelpText = "Exclude isomorphic states from state space (symmetry reduction)")]
        internal bool excludeIsomorphicStates;

        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "", HelpText = "Group excluded isomorphic states together (symmetry reduction)")]
        internal bool collapseExcludedIsomorphicStates;

        [Argument(ArgumentType.MultipleUnique, ShortName = "", HelpText = "File name of a file containing the term representation fsm.ToTerm() of an fsm (object of type FSM). Multiple fsms are composed into a product.")]
        public string[] fsm;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = "Test", HelpText = "Name of start action of a test case. This value is used only if a testSuite is provided.")]
        public string startTestAction;

        [Argument(ArgumentType.AtMostOnce, DefaultValue = false, ShortName = "", HelpText = "Whether the State View is visible.")]
        public bool stateViewVisible;

        [Argument(ArgumentType.AtMostOnce, ShortName = "dot", HelpText = "The name of the file where Dot goes")]
        public string dotFileName;

        [Argument(ArgumentType.AtMostOnce, ShortName = "machine", HelpText = "The name of the file where FSM goes")]
        public string machineFileName;
    }
}
