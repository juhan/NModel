//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NModel;
using NModel.Terms;
using NModel.Execution;
using NModel.Utilities;

namespace NModel.Algorithms
{
    /// <summary>
    /// Provides a utility to generate an offline test suite from a model program 
    /// and to save the generated test suite in a file.
    /// </summary>
    public class OfflineTestGenerator
    {
        ModelProgram model;
        string fileName;
        //string startTest;
        bool append;

        #region configuration settings
        /// <summary>
        /// Gets or sets the name of the file where the generated test suite is saved.
        /// If null or empty, the test suite is written to the console.
        /// </summary>
        public String FileName
        {
            get 
            {
                return this.fileName;
            }
            
            set
            { 
                this.fileName = value;
            }
        }

        ///// <summary>
        ///// Gets or sets the name of a tester action that starts a test case. 
        ///// In the generated test suite (t_0,...,t_k), testcase t_i will have 
        ///// StartTest(i) as the first action. This action name should not occur 
        ///// in the model program and is only used to uniquely identify the 
        ///// different test cases. During conformance testing this action should 
        ///// be marked as an internal action.
        ///// </summary>
        //public String StartTest
        //{
        //    get
        //    {
        //        return this.startTest;
        //    }

        //    set
        //    {
        //        if (String.IsNullOrEmpty(value))
        //            throw new ModelProgramUserException("Start test action name must be a nonempty string");
        //        this.startTest = value;
        //    }
        //}

        /// <summary>
        /// If true, the generated test suite is appended at the end of the file,
        /// otherwise the content of the file is overwritten.
        /// Default is false.
        /// </summary>
        public bool Append
        {
            get
            {
                return this.append;
            }

            set
            {
                this.append = value;
            }
        }
        #endregion

        /// <summary>
        /// Constructs an instance of an offline test suite generator from a model program.
        /// </summary>
        /// <param name="model">given model program</param>
        public OfflineTestGenerator(ModelProgram model)
        {
            this.model = model;
            //this.startTest = "Test";
        }

        /// <summary>
        /// Generates a testsuite that covers the transitions of the state machine generated from the 
        /// model program. The state machine generated from the model program
        /// is assumed to be finite. If a nonempty filename is provided, 
        /// writes the generated test suite in the file, otherwise writes the
        /// testsuite to the console.
        /// </summary>
        public void GenerateTestSuite()
        {
            FSM fa = new FSMBuilder(model).Explore();
            Sequence<Sequence<CompoundTerm>>  testsuite = 
                FsmTraversals.GenerateTestSequences(fa);
            using (StreamWriter sw = GetTestSuiteWriter())
            {
                WriteLine(sw, "TestSuite(");
                int testNr = 0;
                foreach (Sequence<CompoundTerm> testcase in testsuite)
                {
                    WriteLine(sw, "    TestCase(");
                    //WriteLine(sw, "        " + this.startTest + "(" + testNr + "),");
                    int nrofactions = testcase.Count;
                    foreach (CompoundTerm action in testcase)
                    {
                        nrofactions -= 1;
                        WriteLine(sw, "        " + action.ToString() + 
                                 (nrofactions >0 ? "," : ""));
                    }
                    testNr += 1;
                    WriteLine(sw, "    )" + (testNr < testsuite.Count ? "," : ""));
                }
                WriteLine(sw, ")");
            }
        }

        StreamWriter GetTestSuiteWriter()
        {
            if (String.IsNullOrEmpty(fileName))
                return null;
            else
            {
                StreamWriter sw = new StreamWriter(fileName, append);
                return sw;
            }
        }

        static void WriteLine(StreamWriter sw, object value)
        {
            if (sw == null)
                Console.WriteLine(value);
            else
                sw.WriteLine(value);
        }


        /// <summary>
        /// Provides programmatic access to the offline test generator 'otg.exe'.
        /// </summary>
        /// <param name="args">command line arguments: references (required), model program(s) (required), name of test suite file and other settings (optional)</param>
        /// <remarks>The settings are displayed when 'otg.exe /?' is executed from the command line.</remarks>
        public static void RunWithCommandLineArguments(string[] args)
        {
            OfflineTestGenCommandLineSettings settings = new OfflineTestGenCommandLineSettings();
            if (!Parser.ParseArgumentsWithUsage(args, settings))
            {
                //Console.ReadLine();
                return;
            }

            if ((settings.reference == null || settings.reference.Length == 0) &&
                settings.model != null && settings.model.Length > 0)
                throw new ModelProgramUserException("Reference missing for model programs.");

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
            if (settings.model.Length > 0)
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

            if (mp == null && fsms.Count == 0)
            {
                throw new ModelProgramUserException("No model program or fsm was given.");
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

            OfflineTestGenerator gen = new OfflineTestGenerator(mp);

            #region configure offline test generator settings

            gen.FileName = settings.file;
            gen.Append = settings.append;
            //gen.StartTest = settings.startTest;
 
            #endregion

            //finally, run the offline test generator
            gen.GenerateTestSuite();
        }
    }

    internal sealed class OfflineTestGenCommandLineSettings
    {
        /// <summary>
        /// Create an instance and initializes some of the fields
        /// </summary>
        public OfflineTestGenCommandLineSettings()
        {
            model = null;
            mp = null;
            reference = null;
            file = null;
            append = false;
            fsm = null;
            //startTest = "Test";
        }

        [DefaultArgument(ArgumentType.MultipleUnique, HelpText = "Fully qualified names of factory methods returning an object that implements ModelProgram. Multiple models are composed into a product.")]
        public string[] model;

        [Argument(ArgumentType.MultipleUnique, ShortName = "", HelpText = "Model programs given on the form M or M[F1,...,Fn] where M is a model program name (namespace) and each Fi is a feature in M. Multiple model programs are composed into a product. No factory method is needed if this option is used.")]
        public string[] mp;

        [Argument(ArgumentType.MultipleUnique, ShortName = "r", HelpText = "Referenced assemblies.")]
        public string[] reference;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "f", HelpText = "File where test suite is saved. The console is used if no file is provided.")]
        public string file;

        [Argument(ArgumentType.LastOccurenceWins, ShortName = "a", DefaultValue = false, HelpText = "If false the file is overwritten, otherwise the generated test suite is appended at the end of the file.")]
        public bool append;

        [Argument(ArgumentType.MultipleUnique, ShortName = "", HelpText = "File name of a file containing the term representation fsm.ToTerm() of an fsm (object of type FSM). Multiple fsms are composed into a product.")]
        public string[] fsm;

        //[Argument(ArgumentType.LastOccurenceWins, ShortName = "s", DefaultValue = "Test", HelpText = "Name of a tester action that starts a test case. In the generated test suite (t_0,...,t_k), testcase t_i will have <startTest>(i) as the first action. This action name should not occur in the model programs and is only used to uniquely identify the different test cases. During conformance testing this action should be marked as an internal action.")]
        //public string startTest;
    }
}
