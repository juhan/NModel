using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using NModel;
using NModel.Terms;
using NModel.Utilities.Graph;
using NModel.Execution;
using NUnit.Framework;

namespace NModel.Tests
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
            LibraryModelProgram mp = LibraryModelProgram.Create(typeof(SampleModels.PowerSwitch.Contract));
            FSMBuilder fabuilder = new FSMBuilder(mp);
            FSM fsm = fabuilder.Explore();
            Assert.AreEqual(2, fsm.AcceptingStates.Count, "Unexpected number of accepting states.");
            Assert.AreEqual(2, fsm.States.Count, "Unexpected number of states.");
            Assert.AreEqual(2, fsm.Transitions.Count, "Unexpected number of transitions.");
            Assert.IsTrue(fsm.IsDeterministic, "FSM expected to be deterministic.");
            Set<Symbol> voc = new Set<Symbol>(Symbol.Parse("PowerOn"), Symbol.Parse("PowerOff")); 
            Assert.AreEqual(voc, fsm.Vocabulary, "Unexpected vocabulary.");
        }

        /// <summary>
        /// Generate the FSM from the Fan model program and check the resulting FSM.
        /// </summary>
        [Test]
        public void FanTest1()
        {
            LibraryModelProgram mp = LibraryModelProgram.Create(typeof(SampleModels.Fan.Control),
                                                                "Power", "Control");
            FSMBuilder fabuilder = new FSMBuilder(mp);
            FSM fsm = fabuilder.Explore();
            Assert.AreEqual(4, fsm.AcceptingStates.Count, "Unexpected number of accepting states.");
            Assert.AreEqual(4, fsm.States.Count, "Unexpected number of states.");
            Assert.AreEqual(8, fsm.Transitions.Count, "Unexpected number of transitions.");
            Assert.IsTrue(fsm.IsDeterministic, "FSM expected to be deterministic.");
            Set<Symbol> voc = new Set<Symbol>(Symbol.Parse("PowerOn"),
                Symbol.Parse("PowerOff"), Symbol.Parse("ControlPower"), 
                Symbol.Parse("ControlSpeed"), Symbol.Parse("IncrementSpeed"));
            Assert.AreEqual(voc, fsm.Vocabulary, "Unexpected vocabulary.");
        }

        /// <summary>
        /// Generate the FSM from the Fan model program with a filter and check the resulting FSM.
        /// </summary>
        [Test]
        public void FanTest2()
        {
            LibraryModelProgram mp = LibraryModelProgram.Create(typeof(SampleModels.Fan.Control),
                                                                "Power", "Control", "Speed", "Filter1" );
            FSMBuilder fabuilder = new FSMBuilder(mp);
            FSM fsm = fabuilder.Explore();
            Assert.AreEqual(11, fsm.States.Count, "Unexpected number of states.");
            Assert.AreEqual(20, fsm.Transitions.Count, "Unexpected number of transitions.");
            Assert.IsTrue(fsm.IsDeterministic, "FSM expected to be deterministic.");
            Set<Symbol> voc = new Set<Symbol>(Symbol.Parse("PowerOn"),
                Symbol.Parse("PowerOff"), Symbol.Parse("ControlPower"),
                Symbol.Parse("ControlSpeed"), Symbol.Parse("IncrementSpeed"));
            Assert.AreEqual(voc, fsm.Vocabulary, "Unexpected vocabulary.");
        }
    }
}
