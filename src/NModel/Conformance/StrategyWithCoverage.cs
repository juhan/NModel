//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Terms;
using NModel.Execution;
using NModel.Internals;
using CoveragePoint = NModel.Terms.Term;
using Action = NModel.Terms.CompoundTerm;


namespace NModel.Conformance
{

    /// <summary>
    /// Action reward policy used by ModelStepperWithCoverage
    /// </summary>
    public enum RewardPolicy
    {
        /// <summary>
        /// Select an action that provides a maximum reward
        /// </summary>
        MaximumReward,
        /// <summary>
        /// Select an action with likelihood that is proportional to the reward
        /// </summary>
        ProbableReward,
    }

    /// <summary>
    /// Computes a bag of coverage points for the given action from the given source state.
    /// </summary>
    /// <param name="state">given state</param>
    /// <param name="action">given action</param>
    /// <returns>bag of coverage points</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public delegate Bag<CoveragePoint> CoveragePointDelegate(IState state, Action action);

    /// <summary>
    /// Provides a model stepper that uses coverage points to direct the selection of actions.
    /// </summary>
    public class StrategyWithCoverage: Strategy
    {     
        RewardPolicy policy;
        Bag<CoveragePoint> coveragePoints = Bag<CoveragePoint>.EmptyBag;  //total coverage points seen so far
        CoveragePointDelegate coveragePointProvider;

        /// <summary>
        /// Construct a model stepper that records coverage points.
        /// </summary>
        /// <param name="modelProgram">model program</param>
        /// <param name="policy">reward policy</param>
        /// <param name="coveragePointProvider">(optional) coverage point provider</param>
        public StrategyWithCoverage(ModelProgram modelProgram, RewardPolicy policy, CoveragePointDelegate coveragePointProvider)
            :
            base(modelProgram)
        {
            this.policy = policy;
            this.coveragePointProvider = coveragePointProvider;
        }

        /// <summary>
        /// Construct a model stepper that records coverage points.
        /// Defines the coverage point for a given action a and state s 
        /// as the bag containig the pair (s.GetHashCode(),a.GetHashCode())
        /// </summary>
        /// <param name="modelProgram">model program</param>
        /// <param name="policy">reward policy</param>
        public StrategyWithCoverage(ModelProgram modelProgram, RewardPolicy policy)
            :
            base(modelProgram)
        {
            this.policy = policy;
            this.coveragePointProvider = DefaultCoveragePointProvider;
        }

        /// <summary>
        /// Construct a model stepper that records coverage points.
        /// Coverage points of interest are provided by <paramref name="transitionPropertyNames"/>
        /// </summary>
        /// <param name="modelProgram">model program</param>
        /// <param name="policy">reward policy</param>
        /// <param name="transitionPropertyNames">(optional) coverage points of interest</param>
        public StrategyWithCoverage(ModelProgram modelProgram, RewardPolicy policy, Set<string> transitionPropertyNames)
            :
            base(modelProgram)
        {
            this.policy = policy;
            this.coveragePointProvider = new CoveragePointProvider(modelProgram, transitionPropertyNames).GetCoveragePoints;
        }

        /// <summary>
        /// Returns a bag containing a single coverage point that is a pair of integers 
        /// that are the hashcodes of the state and the action.
        /// </summary>
        static Bag<Term> DefaultCoveragePointProvider(IState state, CompoundTerm action)
        {
            return new Bag<Term>(new Pair<int, int>(state.GetHashCode(), action.GetHashCode()).AsTerm);
        }

        double GetReward(IState s, Action a)
        {
            Bag<CoveragePoint> cps = this.coveragePointProvider(s, a);
            double totreward = 0;
            foreach (CoveragePoint cp in cps)
            {
                int newCoverage = cps.CountItem(cp); //note this is >= 1
                int oldCoverage = coveragePoints.CountItem(cp);   //note this is >= 0
                double reward = (newCoverage / (newCoverage + (oldCoverage * oldCoverage)));
                totreward = totreward + reward;
            }
            return totreward;
        }

        /// <summary>
        /// Compute the weight of an action a in state s as 1000 * GetReward(s,a) as integer
        /// </summary>
        int GetWeight(IState s, Action a)
        {
            return Math.Max((int)GetReward(s, a) * 1000, 1);
        }

        /// <summary>
        /// Update the bag of coverage points seen so far, by adding 
        /// the coverage points provided by the transition labeled by action a from state s.
        /// </summary>
        internal void Update(IState s, Action a)
        {
            Bag<Term> cps = this.coveragePointProvider(s, a);
            this.coveragePoints = this.coveragePoints.Union(cps);
        }

        Set<Action> GetActionsWithMaximumReward(IEnumerable<Action> actions, IState s)
        {
            double bestRewardSoFar = 0;
            Set<Action> bestActionsSoFar = Set<Action>.EmptySet;
            foreach (Action a in actions)
            {
                double reward = GetReward(s, a);
                if (reward == bestRewardSoFar)
                    bestActionsSoFar = bestActionsSoFar.Add(a);
                else if (reward > bestRewardSoFar)
                {
                    bestRewardSoFar = reward;
                    bestActionsSoFar = new Set<Action>(a);
                }
            }
            Console.WriteLine(bestActionsSoFar.ToString());
            return bestActionsSoFar;
        }

        /// <summary>
        /// Choose the next action from the given set of actions in the given state.
        /// </summary>
        internal Action ChooseAction(Sequence<Action> actions, IState state)
        {
            switch (policy)
            {
                case RewardPolicy.MaximumReward:
                    {
                        return GetActionsWithMaximumReward(actions, state).Choose();
                    }
                default:
                    {
                        Converter<Action, int> weightOfAction =
                            delegate(Action a) { return GetWeight(state, a); };
                        Sequence<int> weights = actions.Convert<int>(weightOfAction);

                        //choose an action with probability proportional to its weight
                        int sum = weights.Tail.Reduce<int>(delegate(int x, int y) { return x + y; }, weights.Head);
                        int z = HashAlgorithms.GlobalChoiceController.Next(sum);
                        int tot = weights[0];
                        int i = 0;
                        while (z >= tot)
                            tot += weights[++i];
                        return actions[i];

                    }
            }
        }


        #region IModelStepper methods

        /// <summary>
        /// Update the current state to the target state of the action from the current state.
        /// Records coverage points of this transition.
        /// </summary>
        /// <param name="action">given action</param>
        public override void DoAction(Action action)
        {
            Update(this.CurrentState, action);
            base.DoAction(action);
        }

        /// <summary>
        /// Select an action that is enabled in the current state
        /// and whose action symbol is in the set <paramref name="actionSymbols"/>.
        /// Use coverage points and reward policy.
        /// </summary>
        /// <param name="actionSymbols">set of candidate action symbols</param>
        /// <returns>the chosen action or null if no choice is possible</returns>
        public override Action SelectAction(Set<Symbol> actionSymbols)
        {
            if (actionSymbols == null)
                throw new ArgumentNullException("actionSymbols");

            if (actionSymbols.IsEmpty)
                return null;

            Sequence<Action> actions = new Sequence<Action>(this.GetEnabledActions(actionSymbols));

            if (actions.IsEmpty)
                return null;

            Action a = ChooseAction(actions, this.CurrentState); //choose a tester action 
            return a;
        }

        #endregion


        /// <summary>
        /// Creates a model stepper that uses coverage points (when != null) to direct the selection of actions.
        /// If coverage == null, uses default coverage points that are (state.GetHashCode(), action.GetHashCode()) pairs.
        /// Uses RewardPolicy.MaximumReward for action selection.
        /// </summary>
        /// <param name="modelProgram">given model program</param>
        /// <param name="coverage">given coverage point names (may be null)</param>
        public static StrategyWithCoverage CreateWithMaximumReward(ModelProgram modelProgram, string[]/*?*/ coverage)
        {
            if (coverage == null)
                return new StrategyWithCoverage(modelProgram, RewardPolicy.MaximumReward);
            else
                return new StrategyWithCoverage(modelProgram, RewardPolicy.MaximumReward, new Set<string>(coverage));
        }

        /// <summary>
        /// Creates a model stepper that uses coverage points (when != null) to direct the selection of actions.
        /// If coverage == null, uses default coverage points that are (state.GetHashCode(), action.GetHashCode()) pairs.
        /// Uses RewardPolicy.ProbableReward for action selection.
        /// </summary>
        /// <param name="modelProgram">given model program</param>
        /// <param name="coverage">given coverage point names (may be null)</param>
        public static StrategyWithCoverage CreateWithProbableReward(ModelProgram modelProgram, string[]/*?*/ coverage)
        {
            if (coverage == null)
                return new StrategyWithCoverage(modelProgram, RewardPolicy.ProbableReward);
            else
                return new StrategyWithCoverage(modelProgram, RewardPolicy.ProbableReward, new Set<string>(coverage));
        }
    }
}
