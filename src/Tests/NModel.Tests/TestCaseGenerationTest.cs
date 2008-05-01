using System;
using System.Collections.Generic;
using NModel.Algorithms;
using NModel;
using NModel.Terms;
using NModel.Execution;
using NUnit.Framework;

using Transition = NModel.Triple<NModel.Terms.Term, NModel.Terms.CompoundTerm, NModel.Terms.Term>;

namespace NModel.Tests
{
    /// <summary>
    /// Summary description for TestCaseGeneration
    /// </summary>
    [TestFixture]
    public class TestCaseGenerationTest
    {
        [Test]
        public void GenerateTestSequencesTest1()
        {
            FSM fa = CreateSampleFA();

            Sequence<Sequence<CompoundTerm>> testCases = 
                FsmTraversals.GenerateTestSequences(fa);

            Bag<Sequence<CompoundTerm>> expectedTestCases =
                new Bag<Sequence<CompoundTerm>>(
                    new Sequence<CompoundTerm>(new CompoundTerm(new Symbol("a")), new CompoundTerm(new Symbol("b"))),
                    new Sequence<CompoundTerm>(new CompoundTerm(new Symbol("a")), new CompoundTerm(new Symbol("c"))));

            Assert.AreEqual(expectedTestCases, new Bag<Sequence<CompoundTerm>>(testCases));
        }

        [Test]
        public void GetDeadStatesTest1()
        {
            FSM fa = CreateSampleFA();
            Set<Term> deadStates = FsmTraversals.GetDeadStates(fa);
            Set<Term> expectedDeadStates = new Set<Term>(new Literal(4));
            Assert.AreEqual(expectedDeadStates, deadStates);
        }

        FSM CreateSampleFA()
        {
            Literal[] states = new Literal[5];
            for (int i = 0; i < 5; i++)
                states[i] = new Literal(i);
            CompoundTerm a = new CompoundTerm(new Symbol("a"));
            CompoundTerm b = new CompoundTerm(new Symbol("b"));
            CompoundTerm c = new CompoundTerm(new Symbol("c"));

            Set<Term> faStates = new Set<Term>(states);
            Set<Term> accStates = new Set<Term>(states[2], states[3]);
            Set<Symbol> vocab = new Set<Symbol>(a.Symbol, b.Symbol, c.Symbol);

            Set<Transition> transitions =
                new Set<Transition>(
                    new Transition(states[0], a, states[1]),
                    new Transition(states[1], b, states[2]),
                    new Transition(states[1], c, states[3]),
                    new Transition(states[3], a, states[4]));

            return new FSM(states[0], faStates, transitions, accStates, vocab);
        }
    }
}
