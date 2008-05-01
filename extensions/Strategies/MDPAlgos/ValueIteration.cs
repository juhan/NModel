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

    public class MDPNewStrategy : NModel.Conformance.Strategy
    {
        internal static Dictionary<int, double> cov;
        internal static Dictionary<int, double> v;
        internal static Dictionary<int, Set<int>> activeEdges;
        internal static Dictionary<int, Bag<int>> passiveEdges;
        internal static Dictionary<int, Set<int>> cps;
        internal double alpha = 0.9;
        internal double epsilon = 0.01;

        public MDPNewStrategy(ModelProgram modelProgram)
            :
            base(modelProgram)
        {
            cov = new Dictionary<int, double>();
            v = new Dictionary<int, double>();
            cps = new Dictionary<int, Set<int>>();
            activeEdges = new Dictionary<int, Set<int>>();
            passiveEdges = new Dictionary<int, Bag<int>>();
            v[this.currState.GetHashCode()] = 1.0;
            cov[this.currState.GetHashCode()] = 0.0;
        }
        #region IModelStepper methods

        /// <summary>
        /// Update the current state to the target state of the action from the current state.
        /// Records coverage points of this transition.
        /// </summary>
        /// <param name="action">given action</param>
        public override void DoAction(Action action)
        {
            UpdateCoveragePoints(action);
            int srcHash = this.currState.GetHashCode(); 

            base.DoAction(action);

            int targetHash = this.currState.GetHashCode();

            if (!v.ContainsKey(targetHash))
            {
                v[targetHash] = 1.0;
            }
            if (this.ObservableActionSymbols.Contains(action.FunctionSymbol1))
            {
                AddPassiveEdge(srcHash, targetHash);
                DoValueIteration(); // do value iteration when the probabilities change
            }
            else
            {       
                bool doiter = false;
                // do value iteration when a new active edge is added to the transition graph
                if (!activeEdges.ContainsKey(srcHash))
                    doiter = true;
                else if (!activeEdges[srcHash].Contains(targetHash))
                {
                    doiter = true;
                }
                AddActiveEdge(srcHash, targetHash);
                if (doiter)
                    DoValueIteration();
            }
            //foreach (int i in cov.Keys)
            //    if(cov[i]<1)
            //        System.Console.WriteLine("state : " + i + " cov " + cov[i]);
            
            
        }

        private void AddActiveEdge(int srcHash, int targetHash)
        {
            if (!activeEdges.ContainsKey(srcHash))
               activeEdges[srcHash] = new Set<int>();
            activeEdges[srcHash] = activeEdges[srcHash].Add(targetHash);
            
        }

        private void AddPassiveEdge(int srcHash, int targetHash)
        {
            if (!passiveEdges.ContainsKey(srcHash))
                passiveEdges[srcHash] = new Bag<int>();
            passiveEdges[srcHash] = passiveEdges[srcHash].Add(targetHash);            
        }
        
        private void UpdateCoveragePoints(CompoundTerm action)
        {
            int hash = this.currState.GetHashCode();
            //System.Console.WriteLine("action taken "+ action.ToString());
            if (!cps.ContainsKey(hash))
                cps[hash] = new Set<int>();
            cps[hash] = cps[hash].Add(action.GetHashCode());
        }

        private void DoValueIteration()
        {
            double newv;
            double maxDiff = 1.0;
            Set<int> vk = new Set<int> (v.Keys);
            while(maxDiff > epsilon){
                maxDiff = 0.0;
                foreach (int stateHash in vk)
                {
                    double diff = 0.0;
                    if (activeEdges.ContainsKey(stateHash))
                    {   
                        newv = Math.Min(v[stateHash],alpha * findMax(activeEdges[stateHash]));
                        diff = Math.Abs(newv - v[stateHash]);
                        v[stateHash] = newv;
                    }
                    else if (passiveEdges.ContainsKey(stateHash))
                    {
                        newv = Math.Min(v[stateHash],alpha * findExpectedValue(passiveEdges[stateHash]));
                        diff = Math.Abs(newv - v[stateHash]);
                        v[stateHash] = newv;
                    }
                    //System.Console.WriteLine(stateHash.GetHashCode() + " " + v[stateHash]);
                    maxDiff = Math.Max(maxDiff, diff);
                }
            }
        }

        private double findExpectedValue(Bag<int> bag)
        {
            double sum = 0.0;
            foreach (int t in bag)
            {
                sum = sum + v[t];
            }
            return (sum / (double)bag.Count);            
        }

        private double findMax(Set<int> set)
        {
            double max = 0.0;
            foreach (int t in set)
            {
                max = Math.Max(max,v[t]);
            }
            return max;     
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
            Action maxAct = actions.Head;
            int sState = iState.GetHashCode();
            int tState;
            IExtendedState iestate = (IExtendedState)iState;
            int c = iestate.LocationValuesCount;

            foreach (Action a in actions)
                foreach (string s in this.modelProgram.GetEnablingConditionDescriptions(iState, a, false))
                {
                    Term t = Term.Parse(s);
                    Sequence<Term> vars = (t.Arguments[0].Arguments);
                    Map<Variable, Term> subst = ConstructSubst(a,vars); 
                    System.Console.WriteLine(a.ToString() + sState + " enabled string " + t.Arguments[1].Substitute(subst));
                }
            /*
            for (int i = 0; i < c; i++)
            {
                System.Console.WriteLine("name: "+iestate.GetLocationName(i) + " value : "+
                    iestate.GetLocationValue(i) + " hash" + 
                    iestate.GetLocationValue(i).GetHashCode());
                CompoundValue t  = (CompoundValue)iestate.GetLocationValue(i);
                foreach (CompoundValue t1 in t.FieldValues())
                {
                    System.Console.WriteLine(" field " + t1.ToString());
                }
            }
            */

            TransitionProperties tp;
            int sum = 0;
            Sequence<Pair<int, Action>> cumulActSum = Sequence<Pair<int,Action>>.EmptySequence;

            Set<int> coveredActs = findCoveredActs(sState,actions);
             if(!cov.ContainsKey(sState))
                 cov[sState] = 0.0;
            Set<Action> newStateActs = new Set<Action>();
            Set<Action> newActs = new Set<Action>();
            Set<Action> oldActs = new Set<Action>(actions.Head);
            foreach (Action a in actions)
            {
                tState = this.modelProgram.GetTargetState(iState, a, null, out tp).GetHashCode();
                if (!v.ContainsKey(tState))
                {
                    newStateActs = newStateActs.Add(a);                   
                }
                else if (!coveredActs.Contains(a.GetHashCode()))
                {
                    newActs = newActs.Add(a);                                     
                }
                else
                {
                    // one greedy approach
                    /*
                    if (v.ContainsKey(tState) && v[tState] > maxv)
                    {
                        maxv = v[tState];
                        oldActs = new Set<Action>(a);
                    }
                    else if (v.ContainsKey(tState) && v[tState] == maxv)
                    {
                        oldActs = oldActs.Add(a);
                    }*/
                    
                    // probabilistic greedy approach
                    if (v.ContainsKey(tState))
                    {
                        sum = sum + (int)(v[tState]* Math.Pow(10.0,9.0));
                        Pair<int, Action> np = new Pair<int, Action>(sum, a);
                        cumulActSum = cumulActSum.AddLast(np);
                    }
                }
            }
            if (!newStateActs.IsEmpty)
            {
                maxAct = newStateActs.Choose();
                System.Console.WriteLine("new action in new state " + maxAct.ToString());
            }
            else if (!newActs.IsEmpty)
            {
                maxAct = newActs.Choose();
                System.Console.WriteLine("new action in old state " + maxAct.ToString());
            }
            else
            {
                //maxAct = oldActs.Choose();
                Random rndNumbers = new Random();
                int rndNumber = rndNumbers.Next(sum);
                System.Console.WriteLine(sum + " " + rndNumber);
                foreach (Pair<int, Action> np in cumulActSum)
                {
                    System.Console.WriteLine(np.First + " " + np.Second.ToString());
                    if (rndNumber <= np.First)
                    {
                        maxAct = np.Second;
                        break;
                    }
                    maxAct = np.Second;
                } 
                System.Console.WriteLine("old action in old state " + maxAct.ToString());
            }            
            coveredActs = coveredActs.Add(maxAct.GetHashCode());
            cov[sState] = (double)coveredActs.Count / (double)actions.Count;
            return maxAct;  
           
        }

        private Map<Variable, Term> ConstructSubst(CompoundTerm a, Sequence<Term> vars)
        {
            Map<Variable, Term> subst = Map<Variable, Term>.EmptyMap;
            for (int i = 0; i < vars.Count; i++)
            {
                subst = subst.Add((Variable)vars[i],a.Arguments[i]);
            }
            return subst;
            
        }

        private Set<int> findCoveredActs(int srcHash, Sequence<Action> actions)
        {
            Set<int> acts = new Set<int>(); 
            if (cps.ContainsKey(srcHash))
                foreach (Action a in actions)
                {
                    if (cps[srcHash].Contains(a.GetHashCode())){
                        acts = acts.Add(a.GetHashCode());
                    }
                }
            return acts;
        }

        #endregion

        /// <summary>
        /// Creates a model stepper that uses coverage points (when != null) to direct the selection of actions.
        /// If coverage == null, uses default coverage points that are (state.GetHashCode(), action.GetHashCode()) pairs.
        /// Uses RewardPolicy.MaximumReward for action selection.
        /// </summary>
        /// <param name="modelProgram">given model program</param>
        /// <param name="coverage">given coverage point names (may be null)</param>
        public static MDPNewStrategy CreateMDPNewStrategy(ModelProgram modelProgram, string[]/*?*/ coverage)
        {
            //return new MDPStrategy(modelProgram,new Set<string>(coverage));      
            return new MDPNewStrategy(modelProgram);
        }
    }
}
