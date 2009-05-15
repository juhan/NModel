//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
//using NModel.Visualization;
using NModel.Execution;
using NModel.Utilities;
using NModel.Conformance;
using NModel.Terms;

namespace NModel.Conformance
{
    //This part inludes the commandline utility

    partial class ConformanceTester
    {
        /// <summary>
        /// Provides programmatic access to the conformance tester commandline utility 'ct.exe'.
        /// </summary>
        /// <param name="args">command line arguments: model program(s), implementation stepper, optional settings for conformance tester</param>
        /// <remarks>The settings are displayed when 'ct.exe /?' is executed from the command line.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static void RunWithCommandLineArguments(string[] args)
        {
            //System.Diagnostics.Debugger.Break();
            ConformanceTester confTester = null;
            try
            {

                ConfTesterCommandLineSettings settings = new ConfTesterCommandLineSettings();
                if (!Parser.ParseArgumentsWithUsage(args, settings))
                {
                    //Console.ReadLine();
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

                #region create the implementation stepper using the factory method
                string implStepperMethodName;
                string implStepperClassName;
                ReflectionHelper.SplitFullMethodName(settings.iut, out implStepperClassName, out implStepperMethodName);
                Type implStepperType = ReflectionHelper.FindType(libs, implStepperClassName);
                MethodInfo implStepperMethod = ReflectionHelper.FindMethod(implStepperType, implStepperMethodName, Type.EmptyTypes, typeof(IStepper));
                IStepper implStepper = null;
                try
                {
                    implStepper = (IStepper)implStepperMethod.Invoke(null, null);
                }
                catch (Exception e)
                {
                    throw new ModelProgramUserException("Invocation of '" + settings.iut + "' failed: " + e.ToString());
                }
                #endregion

                #region create a model program for each model using the factory method and compose into product
                string mpMethodName;
                string mpClassName;
                ModelProgram mp = null;
                if (settings.model != null && settings.model.Length > 0)
                {
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
                        CompoundTerm testSuite = (CompoundTerm)Term.Parse(testSuiteAsString);
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


                // Requirements metrics

                #region load all the requirements from an external file - if any
                if (!String.IsNullOrEmpty(settings.RequirementsFile))
                {
                    try
                    {
                        System.IO.StreamReader reqsReader =
                            new System.IO.StreamReader(settings.RequirementsFile);
                        string line;
                        char[] splitchars = { '|' };
                        string[] splitedLine;
                        while ((line = reqsReader.ReadLine()) != null)
                        {
                            if (line.Length > 5)
                            {
                                splitedLine = line.Split(splitchars);
                                // The format of a requirement line is:
                                // action (ignore by the parser) | id | description
                                AllRequirements.Add(new KeyValuePair<string, string>(splitedLine[1].Trim().ToLower(), splitedLine[2].Trim().ToLower()));
                            }
                        }
                        reqsReader.Close();
                    }
                    catch (Exception e)
                    {
                        throw new ModelProgramUserException("Cannot create all-requirements list: " + e.Message);
                    }
                }
                #endregion


                if (mp == null && testcases.IsEmpty && fsms.Count == 0)
                {
                    throw new ModelProgramUserException("No model, fsm, or test suite was given.");
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

                #region create the model stepper
                IStrategy ms;
                Set<Symbol> obs = new Set<string>(settings.observableAction).Convert<Symbol>(delegate(string s) { return Symbol.Parse(s); });
                if (!testcases.IsEmpty)
                {
                    ms = new TestSuiteStepper(settings.startTestAction, testcases, mp);
                }
                else
                {
                    ms = CreateModelStepper(libs, mp, settings.modelStepper, settings.coverage, obs);
                }

                #endregion

                confTester = new ConformanceTester(ms, implStepper);

                #region configure conformance tester settings

                confTester.ContinueOnFailure = settings.continueOnFailure;
                confTester.StepsCnt = (testcases.IsEmpty ? settings.steps : 0);
                confTester.MaxStepsCnt = (testcases.IsEmpty ? settings.maxSteps : 0);
                confTester.RunsCnt = (testcases.IsEmpty ? settings.runs : testcases.Count);
                confTester.WaitAction = settings.waitAction;
                confTester.TimeoutAction = settings.timeoutAction;
                confTester.ShowTestCaseCoveredRequirements = settings.showTestCaseCoveredRequirements;
                confTester.showMetrics = settings.showMetrics;

                Symbol waitActionSymbol = confTester.waitActionSet.Choose();
                Symbol timeoutActionSymbol = confTester.timeoutAction.Symbol;


                confTester.ObservableActionSymbols = obs;

                Set<Symbol> cleanup = new Set<string>(settings.cleanupAction).Convert<Symbol>(delegate(string s) { return Symbol.Parse(s); });
                confTester.CleanupActionSymbols = cleanup;

                if (confTester.IsAsync)
                {
                    //remove the wait and timeout action symbol from tester action symbols
                    if (confTester.testerActionSymbols.Contains(waitActionSymbol) ||
                        confTester.testerActionSymbols.Contains(timeoutActionSymbol))
                        confTester.testerActionSymbols =
                            confTester.testerActionSymbols.Remove(waitActionSymbol).Remove(timeoutActionSymbol);
                }

                Set<Symbol> internals = new Set<string>(settings.internalAction).Convert<Symbol>(delegate(string s) { return Symbol.Parse(s); });

                confTester.InternalActionSymbols =
                    (testcases.IsEmpty || settings.startTestAction != "Test" ?
                     internals :
                     internals.Add(Symbol.Parse("Test")));

                TimeSpan timeout = new TimeSpan(0, 0, 0, 0, settings.timeout);
                confTester.TesterActionTimeout = delegate(IState s, CompoundTerm a) { return timeout; };
                confTester.Logfile = settings.logfile;
                confTester.OverwriteLog = settings.overwriteLog;
                if (settings.randomSeed != 0)
                {
                    confTester.RandomSeed = settings.randomSeed;
                }
                #endregion

                //finally, run the application
                confTester.Run();
            }
            catch (ModelProgramUserException)
            {
                throw;
            }
            catch (ConformanceTesterException e)
            {
                throw new ModelProgramUserException(e.Message);
            }
            finally
            {
                if (confTester != null)
                    confTester.Dispose();
            }
        }

        private static IStrategy CreateModelStepper(List<Assembly> libs, ModelProgram mp,
            string/*?*/ msName, string[]/*?*/ coverage, Set<Symbol> obs)
        {
            //if no model stepper name is provided, use the default one and ignore coverage
            if (msName == null)
                return new Strategy(mp);

            //check if one of the supported model steppers is used
            if (msName.Equals(typeof(StrategyWithCoverage).FullName + ".CreateWithMaximumReward"))
                return StrategyWithCoverage.CreateWithMaximumReward(mp, coverage);
            //check if one of the supported model steppers is used
            if (msName.Equals(typeof(StrategyWithCoverage).FullName + ".CreateWithProbableReward"))
                return StrategyWithCoverage.CreateWithProbableReward(mp, coverage);

            string msMethodName;
            string msClassName;
            ReflectionHelper.SplitFullMethodName(msName, out msClassName, out msMethodName);
            Type msType = ReflectionHelper.FindType(libs, msClassName);
            MethodInfo msMethod = ReflectionHelper.FindMethod(msType, msMethodName, new Type[] { typeof(ModelProgram), typeof(string[]) } , typeof(IStrategy));
            IStrategy ms = null;
            try
            {
                ms = (IStrategy)msMethod.Invoke(null, new object[] { mp, coverage });
                ms.ObservableActionSymbols = obs;
            }
            catch (Exception e)
            {
                throw new ModelProgramUserException("Invocation of '" + msName + "' failed: " + e.ToString());
            }
            return ms;
        }
    }

    internal sealed class ConfTesterCommandLineSettings
    {
        /// <summary>
        /// Create an instance and initializes some of the fields explicitly to avoid warnings
        /// </summary>
        public ConfTesterCommandLineSettings()
        {
            iut = null;
            model = null;
            mp = null;
            modelStepper = null;
            reference = null;
            coverage = null;
            steps = 0;
            runs = 0;
            observableAction = null;
            cleanupAction = null;
            internalAction = null;
            logfile = null;
            overwriteLog = true;
            randomSeed = 0;
            maxSteps = 0;
            testSuite = null;
            fsm = null;
            startTestAction = "Test";
            waitAction = "Wait";
            timeoutAction = "Timeout";
            RequirementsFile = null; // Requirements metrics
            showTestCaseCoveredRequirements = false; // Show requirements coverage per test-case?
            showMetrics = false; // Show metrics in the ct log?
        }

        [Argument(ArgumentType.Required, ShortName = "", HelpText = "Implementation under test, a fully qualified name of a factory method that returns an object that implements IStepper.")]
        public string iut;

        [DefaultArgument(ArgumentType.MultipleUnique, HelpText = "Fully qualified names of factory methods returning an object that implements ModelProgram. Multiple models are composed into a product.")]
        public string[] model;

        [Argument(ArgumentType.MultipleUnique, ShortName = "", HelpText = "Model programs given on the form M or M[F1,...,Fn] where M is a model program name (namespace) and each Fi is a feature in M. Multiple model programs are composed into a product. No factory method is needed if this option is used.")]
        public string[] mp;

        [Argument(ArgumentType.AtMostOnce, ShortName = "", HelpText = "A fully qualified name of creator method that takes arguments (ModelProgram modelProgram, string[] coverage) and returns an object that implements IModelStepper. If left unspecified the default model stepper is used that ignores coverage point names (if any). (If a testSuite is provided, this option is ignored.)")]
        public string modelStepper;

        [Argument(ArgumentType.AtLeastOnce | ArgumentType.MultipleUnique, ShortName = "r", HelpText = "Referenced assemblies.")]
        public string[] reference;

        [Argument(ArgumentType.MultipleUnique, ShortName = "", HelpText = "Coverage point names used by model stepper. (If a testSuite is provided, this option is ignored.)")]
        public string[] coverage;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = 0, HelpText = "The desired number of steps that a single test run should have. After the number is reached, only cleanup tester actions are used and the test run continues until an accepting state is reached or the number of steps is MaxSteps (whichever occurs first). 0 implies no bound and a test case is executed until either a conformance failure occurs or no more actions are enabled. (If a testSuite is provided, this value is set to 0.)")]
        public int steps;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = 0, HelpText = "The maximum number of steps that a single test run can have. This value must be either 0, which means that there is no bound, or greater than or equal to steps. Steps is assigned the value of maxsteps if not explicitly set to a lower value.")]
        public int maxSteps;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = 0, HelpText = "The desired number of test runs. Testing stops when this number has been reached. Negative value or 0 implies no bound. (If a testSuite is provided, this value is set to the number of test cases in the test suite.)")]
        public int runs;

        [Argument(ArgumentType.MultipleUnique, ShortName = "o", HelpText = "Action symbols of actions controlled by the implementation. Other actions are controlled by the tester.")]
        public string[] observableAction;

        [Argument(ArgumentType.MultipleUnique, ShortName = "c", HelpText = "Action symbols of actions that are used to end a test run during a cleanup phase. Other actions are omitted during a cleanup phase.")]
        public string[] cleanupAction;

        [Argument(ArgumentType.MultipleUnique, ShortName = "i", HelpText = "Action symbols of tester actions that are not shared with the implementation and are not used for conformance evaluation. Other tester actions are passed to the implementation stepper.")]
        public string[] internalAction;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = 10000, HelpText = "The amount of time in milliseconds within which a tester action must return when passed to the implementation stepper.")]
        public int timeout = 10000;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = true, HelpText = "Continue testing when a conformance failure occurs.")]
        public bool continueOnFailure = true;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "log", HelpText = "Filename where test results are logged. The console is used if no logfile is provided.")]
        public string logfile;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "seed", DefaultValue = 0, HelpText = "A number used to calculate the starting value for the pseudo-random number sequence that is used by the global choice controller to select tester actions. If a negative number is specified, the absolute value is used. If left unspecified or if 0 is provided a random number is generated as the seed.")]
        public int randomSeed;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = true, HelpText = "If true the log file is overwritten, otherwise the testresults are appended to the logfile")]
        public bool overwriteLog;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", HelpText = "File name of a file containing a sequence of actions sequences to be used as the test suite.")]
        public string testSuite;

        [Argument(ArgumentType.MultipleUnique, ShortName = "", HelpText = "File name of a file containing the term representation fsm.ToTerm() of an fsm (object of type FSM). Multiple fsms are composed into a product.")]
        public string[] fsm;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = "Test", HelpText = "Name of start action of a test case. This value is used only if a testSuite is provided. The default 'Test' action sybmol is considered as an internal test action symbol. If another action symbol is provided it is not considered as being internal by default.")]
        public string startTestAction;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = "Wait", HelpText = "A name of an action that is used to wait for observable actions in a state where no controllable actions are enabled. A wait action is controllable and internal and must take one integer argument that determines the time to wait in milliseconds during which an observable action is expected. Only used with IAsyncStepper.")]
        public string waitAction;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "", DefaultValue = "Timeout", HelpText = "A name of an action that happens when a wait action has been executed and no obsevable action occurred within the time limit provided in the wait action. A timeout action is observable and takes no arguments. Only used with IAsyncStepper.")]
        public string timeoutAction;

        // Metrics
        [Argument(ArgumentType.LastOccurenceWins, ShortName = "metrics", DefaultValue = false, HelpText = "Show test-suite metrics at the end of the ct log?")]
        public bool showMetrics = false;
        
        // Requirements metrics

        // The format of a requirement line is:
        // action (ignore by the parser) | id | description
        [Argument(ArgumentType.AtMostOnce, ShortName = "req", HelpText = "File containing the requirements for checking execution coverage.")]
        public string RequirementsFile;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "tcreq", DefaultValue = false, HelpText = "Show executed requirements by each test-case?")]
        public bool showTestCaseCoveredRequirements = false;
    }
}
