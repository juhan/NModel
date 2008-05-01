using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using NModel;
using NModel.Terms;
using NModel.Utilities.Graph;
using NModel.Execution;
using NUnit.Framework;

namespace NModel.Visualization.Tests
{
    [TestFixture]
    public class FSMGenerationTest
    {
        /// <summary>
        /// Generate the FSM from the PowerSwitch model program and check the resulting FSM.
        /// </summary>
        [Test]
        public void PowerSwitchTest() 
        {
            NModel.Visualization.CommandLineViewer.RunWithCommandLineArguments(
                "/stateViewVisible+",
                "/r:NModel.Visualization.Tests.dll", "/mp:SampleModels.PowerSwitch");
        }

        /// <summary>
        /// Include two features
        /// </summary>
        [Test]
        public void FanTest1()
        {

            NModel.Visualization.CommandLineViewer.RunWithCommandLineArguments(
                "/stateViewVisible+",
                "/r:NModel.Visualization.Tests.dll", "/mp:SampleModels.Fan[Power,Control]");
        }

        /// <summary>
        /// Include all features and the filter.
        /// </summary>
        [Test]
        public void FanTest2()
        {

            NModel.Visualization.CommandLineViewer.RunWithCommandLineArguments(
                "/stateViewVisible+",
                "/r:NModel.Visualization.Tests.dll", 
                "/mp:SampleModels.Fan[Power,Speed,Control,Filter1]");
        }

        /// <summary>
        /// Include no features.
        /// </summary>
        [Test]
        public void FanTest3()
        {

            NModel.Visualization.CommandLineViewer.RunWithCommandLineArguments(
                "/stateViewVisible+",
                "/r:NModel.Visualization.Tests.dll",
                "/mp:SampleModels.Fan");
        }
    }
}
