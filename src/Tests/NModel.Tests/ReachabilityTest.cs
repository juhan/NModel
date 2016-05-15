using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

using NModel.Algorithms;
using NModel.Execution;


namespace NModel.Tests
{
    [TestFixture]
    public class ReachabilityTest
    {
        [Test]
        public void SwitchTest()
        {
            Reachability mc = new Reachability();
            //mc.ExcludeIsomorphicStates = false;
            mc.ModelProgram = LibraryModelProgram.Create(typeof(SampleModels.PowerSwitch.Contract));
            ReachabilityResult result=mc.CheckReachability();
            Assert.AreEqual(result.StateCount, 2, "The state count returned differs from actual...");
            Assert.AreEqual(result.TransitionCount, 2, "The transition count returned differs from actual...");

        }

        [Test]
        public void FSMReachTest()
        {
            FSM fsm = FSM.FromString("FSM(0,AcceptingStates(1),Transitions(t(0, a(), 1)),Vocabulary(\"a\"))");
            FsmModelProgram fsmmp = new FsmModelProgram(fsm, "Scenario");
            ReachabilityResult result = NModel.Algorithms.Reachability.Check(fsmmp, "Scenario(1)");
            Assert.AreEqual(result.GoalReached, true);
        }
    }
}
