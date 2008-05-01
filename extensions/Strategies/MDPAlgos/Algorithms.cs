using System;
using System.Collections.Generic;
using System.Text;
using NModel.Terms;
using NModel.Execution;
using NModel.Internals;
using CoveragePoint = NModel.Terms.Term;
using Action = NModel.Terms.CompoundTerm;
using NModel;
using System.Collections;

namespace MDPAlgos
{
    
    public class MDPStrategy:NModel.Conformance.Strategy
    {
        internal static Dictionary<IState, double> cov;
        internal static Dictionary<IState, double> v;
        internal static Dictionary<int,Set<int>> cps;
        internal double alpha = 0.9;

        public MDPStrategy(ModelProgram modelProgram)
            :
            base(modelProgram)
        {
            cov = new Dictionary<IState, double>();
            v = new Dictionary<IState, double>();
            cps = new Dictionary<int, Set<int>>();
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

        private void Update(IState iState, CompoundTerm action)
        {
            int hash = iState.GetHashCode();
            //System.Console.WriteLine("action taken "+ action.ToString());
            if (!cps.ContainsKey(hash))
                cps[hash] = new Set<int>();
            cps[hash] = cps[hash].Add(action.GetHashCode());
            /*
            foreach (Symbol s in this.ObservableActionSymbols)
            {
                System.Console.WriteLine(s);
            }*/
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
            //System.Console.WriteLine("Chosen Action " + a.ToString());
            return a;
        }

        private CompoundTerm ChooseAction(Sequence<Action> actions, IState iState)
        {
            Set<int> coveredActs;
            double minCov = 1.0;
            double maxv = 0.0;
            IState tState;
            Action maxAct = actions.Head;
            TransitionProperties tp;
            if (!cov.ContainsKey(iState))
            {
                
                maxAct = actions.Choose();
                cov[iState] = (1.0/actions.Count);
                v[iState] = 1.0;
               // System.Console.WriteLine("inside if " + iState.GetHashCode() + " cov"+ cov[iState]);
                return maxAct;
            }
            else if (cov.ContainsKey(iState) && cov[iState] < 1.0)
            {                             
                coveredActs = new Set<int>(this.findCoveredActs(iState));
                Set<int> acts = new Set<int>();
                foreach (Action a in actions)
                {
                    if (!coveredActs.Contains(a.GetHashCode()))
                    {
                        System.Console.WriteLine(a.GetHashCode());
                        tState = modelProgram.GetTargetState(iState, a, null, out tp);
                        if (!cov.ContainsKey(tState))
                        {
                            minCov = 0;
                            maxAct = a;
                        }
                        else if (cov[tState] <= minCov)
                        {
                            minCov = cov[tState];
                            maxAct = a;
                        }
                        
                    }
                    else
                    {
                        acts.Add(a.GetHashCode());
                        tState = modelProgram.GetTargetState(iState, a, null, out tp);
                        if (v.ContainsKey(tState))
                            maxv = Math.Max(maxv, v[tState]);                        
                    }
                }    
                acts = acts.Add(maxAct.GetHashCode());
                cov[iState] = ( acts.Count/ actions.Count);
                if (v.ContainsKey(iState))
                    v[iState] = Math.Min(v[iState], (alpha * maxv));
                else
                    v[iState] = alpha * maxv;                
                //System.Console.WriteLine("inside <1.0 " + iState.GetHashCode() + " cov " + cov[iState]);
                return maxAct;                
            }
            else if (cov.ContainsKey(iState)  && cov[iState] == 1.0)
            {               
                foreach (Action a in actions)
                {
                    tState = modelProgram.GetTargetState(iState, a, null, out tp);
                    if (!cov.ContainsKey(tState))
                    {
                        maxv = 1.0;
                        maxAct = a;
                    }
                    else if (cov.ContainsKey(tState) && maxv <= v[tState])
                    {
                        maxv = v[tState];
                        maxAct = a;
                    }
                }
                if (v.ContainsKey(iState))
                    v[iState] = Math.Min(v[iState], (alpha * maxv));
                else
                    v[iState] = alpha * maxv;
                //System.Console.WriteLine("inside else " + iState.GetHashCode() + " " + v[iState]);
                return maxAct;
            }
            else
            {

                //System.Console.WriteLine("Should not be reachable!! cov" + cov[iState]);
                return maxAct;
            }
        }

        private Set<int> findCoveredActs(IState iState)
        {
            int hash = iState.GetHashCode();
            if (cps.ContainsKey(hash))
                return cps[hash];
            else
                return new Set<int> ();
        }

        #endregion

        /// <summary>
        /// Creates a model stepper that uses coverage points (when != null) to direct the selection of actions.
        /// If coverage == null, uses default coverage points that are (state.GetHashCode(), action.GetHashCode()) pairs.
        /// Uses RewardPolicy.MaximumReward for action selection.
        /// </summary>
        /// <param name="modelProgram">given model program</param>
        /// <param name="coverage">given coverage point names (may be null)</param>
        public static MDPStrategy CreateMDPStrategy(ModelProgram modelProgram, string[]/*?*/ coverage)
        {
            //return new MDPStrategy(modelProgram,new Set<string>(coverage));      
            return new MDPStrategy(modelProgram);
        }
    }
}
