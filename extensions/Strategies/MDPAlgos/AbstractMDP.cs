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
    #region binary decision tree
    public class BinaryDecisionTree
    {
        internal string property;
        internal  BinaryDecisionTree trueEdge; // subtree where the property is true
        internal  BinaryDecisionTree falseEdge; // subtree where the property is false 
        internal double maxValue;
        internal double minValue;
        internal Set<int> states;
        public BinaryDecisionTree(string prop, BinaryDecisionTree t, BinaryDecisionTree f, double maxv, double minv, Set<int> setStates)
        {
            this.property = prop;
            this.trueEdge = t;
            this.falseEdge = f;
            this.maxValue = maxv;
            this.minValue = minv;
            this.states = setStates;
        }
        public void PrintTree(int level)
        {
            System.Console.WriteLine("property "+ property + ", maxv " + maxValue + ", minv " + minValue+ ", "+ states.ToString());

            if (this.trueEdge != null)
            {
                for (int k = 0; k < level;k++)
                    System.Console.Write("  ");
                System.Console.Write("T: ");
                this.trueEdge.PrintTree(level+1);
            }
            if (this.falseEdge != null)
            {
                for (int k = 0; k < level; k++)
                    System.Console.Write("  ");
                System.Console.Write("F: ");
                this.falseEdge.PrintTree(level+1);
            }
            return;
        }
        public List<Set<int>> ReturnLeaves()
        {
            List<Set<int>> leaves = new List<Set<int>> ();
            if (trueEdge == null && falseEdge == null)
                leaves.Add(states);
            if (this.trueEdge != null && !this.trueEdge.states.IsEmpty)
                leaves.AddRange(this.trueEdge.ReturnLeaves());
            if (this.falseEdge != null && !this.falseEdge.states.IsEmpty)
                leaves.AddRange(this.falseEdge.ReturnLeaves());
            return leaves;
        }
        public List<double> ReturnValue(bool b)
        {
            List<double> Value = new List<double>();
            if (trueEdge == null && falseEdge == null)
            {
                if (b) Value.Add(maxValue);
                else Value.Add(minValue);
            }
            if (this.trueEdge != null && !this.trueEdge.states.IsEmpty)
                Value.AddRange(this.trueEdge.ReturnValue(b));
            if (this.falseEdge != null && !this.falseEdge.states.IsEmpty)
                Value.AddRange(this.falseEdge.ReturnValue(b));
            return Value;
        }
        public BinaryDecisionTree Refine(string prop, Set<int> enabledStates)
        {
            // dont refine already added property
            if (this.property.Equals(prop))
                return this;
            if (states.IsEmpty)
                return this;
            Set<int> s1 = states.Intersect(enabledStates);
            Set<int> s2 = states.Difference(enabledStates);
            if (this.trueEdge != null) this.trueEdge.Refine(prop,s1);
            if (this.falseEdge != null) this.falseEdge.Refine(prop,s2);
            
            if (this.trueEdge == null && this.falseEdge == null)
            {
                //if (!s1.IsEmpty)
                //{
                    this.trueEdge = new BinaryDecisionTree(prop, null, null,this.maxValue,this.minValue, s1);
                //}
                //else this.trueEdge = null;
                //if (!s2.IsEmpty)
                //{
                   this.falseEdge = new BinaryDecisionTree(prop, null, null,this.maxValue,this.minValue, s2);
                //}
                //else this.falseEdge = null;
            }
            return this;
        }
        public BinaryDecisionTree addState(Set<string> trueProps, int StateId)
        {
            if (states.Contains(StateId))
                return this;
            //if (this.trueEdge == null && this.falseEdge == null)
            //{
            states = this.states.Add(StateId);
            //}
            if (trueEdge!=null && trueProps.Contains(this.property))
            {
                this.trueEdge = this.trueEdge.addState(trueProps, StateId);
            }
            else if(falseEdge != null)
                this.falseEdge = this.falseEdge.addState(trueProps, StateId);

            return this;
        }
    #endregion


        internal BinaryDecisionTree UpdateMaxMin(List<double> maxV, List<double> minV, List<Set<int>> abstractMap)
        {
            if (this.trueEdge == null && this.falseEdge == null)
            {
                if (abstractMap.Contains(states))
                {
                    int index = abstractMap.IndexOf(states);
                    this.maxValue = maxV[index];
                    this.minValue = minV[index];
                }
                return this;
            }
            else
            {
                trueEdge = trueEdge.UpdateMaxMin(maxV, minV, abstractMap);
                falseEdge = falseEdge.UpdateMaxMin(maxV, minV, abstractMap);
                return this;
            }

        }   
    }

    public class MDPNewAbstractStrategy : NModel.Conformance.Strategy
    {
        //sequence of requirment properties 
        internal static Sequence<string> requirementProperties;
        //given a requirement the map provides the set of states where it satisfies
        internal static Map<string, Set<int>> requireEnabledStateMap;
        internal static Dictionary<int, Set<int>> activeEdges;
        internal static Dictionary<int, Bag<int>> passiveEdges;
        internal static BinaryDecisionTree bdt;
        internal static int i =0;

        public MDPNewAbstractStrategy(ModelProgram modelProgram):
            base(modelProgram)
        {
            requirementProperties = Sequence<string>.EmptySequence;
            requireEnabledStateMap = Map<string, Set<int>>.EmptyMap;
            activeEdges = new Dictionary<int, Set<int>>();
            passiveEdges = new Dictionary<int, Bag<int>>();
            bdt = new BinaryDecisionTree("root", null, null,1,1, new Set<int>(currState.GetHashCode()));
        }
        #region IModelStepper methods

        /// <summary>
        /// Update the current state to the target state of the action from the current state.
        /// Records coverage points of this transition.
        /// </summary>
        /// <param name="action">given action</param>
        public override void DoAction(Action action)
        {
            System.Console.WriteLine("Action " + action.ToString());
           
            int srcHash = this.currState.GetHashCode();
            base.DoAction(action);
            int targetHash = this.currState.GetHashCode();
           
            if (this.ObservableActionSymbols.Contains(action.FunctionSymbol1))
                AddPassiveEdge(srcHash, targetHash);
            else
                AddActiveEdge(srcHash, targetHash);
            doValueIteration(bdt);
            
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

        #region Abstract Value Iteration Algorithm

        private void doValueIteration(BinaryDecisionTree bdt)
        {
            List<double> maxV = bdt.ReturnValue(true);
            List<double> minV = bdt.ReturnValue(false);
            List<Set<int>> abstractMap = bdt.ReturnLeaves();
            double tmp; 
            int maxvcount = maxV.Count;
            
                    
            double diff = 1.0;
            double epsilon = 0.1;
            while (diff > epsilon)
            {
                diff = 0.0;
                for (int k = 0; k < maxvcount;k++)
                {
                    tmp = doLocalValueIteration(k, abstractMap, maxV, true);
                    diff = Math.Max(diff, Math.Abs(tmp - maxV[k]));
                    maxV[k] = tmp;
                    tmp = doLocalValueIteration(k, abstractMap, minV, false);
                    diff = Math.Max(diff, Math.Abs(tmp - minV[k]));
                    minV[k] = tmp;
                }
            }        
            
            for (int k = 0; k < maxvcount; k++)
            {
                System.Console.WriteLine("maxv "+ maxV[k]+ " minv "+ minV[k]);
            }
            // Need to update the maxValue as well as minValue in the tree
            bdt = bdt.UpdateMaxMin(maxV, minV, abstractMap);
        }
        // (max)value of an abstract state A = max_{conc state s \in A} max_{t \in succ(s)} maxvalue(B|t \in B)
        private double doLocalValueIteration(int k, List<Set<int>> abstractMap, List<double> absValue, bool isMax)
        {
            double diff = 1.0;
            double epsilon = 0.01;
            double alpha = 0.9;
            double newv;
            Dictionary<int,double> value = new Dictionary<int,double>();
            foreach (int srcStateId in abstractMap[k])
                value.Add(srcStateId,absValue[k]);
            Set<int> valueKeys = new Set<int>(value.Keys);
            while (diff > epsilon)
            {
                diff = 0.0;
                foreach (int stateHash in valueKeys)
                {                    
                    if (activeEdges.ContainsKey(stateHash))
                    {
                        newv = Math.Min(value[stateHash], alpha * findMax(activeEdges[stateHash],value,k,abstractMap,absValue));
                        diff = Math.Max(diff,Math.Abs(newv - value[stateHash]));
                        value[stateHash] = newv;
                    }
                    else if (passiveEdges.ContainsKey(stateHash))
                    {
                        newv = Math.Min(value[stateHash], alpha * findExpectedValue(passiveEdges[stateHash],value,k,abstractMap,absValue));
                        diff = Math.Max(diff,Math.Abs(newv - value[stateHash]));
                        value[stateHash] = newv;
                    }                    
                }
            }
            Set<double> vals = new Set<double>(value.Values);
            return (isMax? vals.Maximum():vals.Minimum());
        }
        private double findExpectedValue(Bag<int> bag , Dictionary<int,double> value, int k, List<Set<int>> abstractMap, List<double> absValue)
        {
            double sum = 0.0;
            int targetAbsId;
            foreach (int t in bag)
            {
                if(abstractMap[k].Contains(t))
                    sum = sum + value[t];
                else
                { 
                    targetAbsId = findAbstractId(t, abstractMap);
                    sum = sum + (targetAbsId==-1? 1:absValue[targetAbsId]);
                }
            }
            return (sum / (double)bag.Count);            
        }

        private double findMax(Set<int> set, Dictionary<int,double> value, int k, List<Set<int>> abstractMap, List<double> absValue)
        {
            double max = 0.0;
            int targetAbsId;
            foreach (int t in set)
            {
                if(abstractMap[k].Contains(t))
                    max = Math.Max(max,value[t]);
                else
                { 
                    targetAbsId = findAbstractId(t, abstractMap);
                    max = Math.Max(max,(targetAbsId==-1? 1:absValue[targetAbsId]));
                }               
            }
            return max;     
        }           

        private int findAbstractId(int targetStateId, List<Set<int>> abstractMap)
        {
            //System.Console.WriteLine(targetStateId + " "+ abstractMap.ToString());
            
            for (int k=0; k< abstractMap.Count;k++)
            {
                if (abstractMap[k].Contains(targetStateId))
                    return k;
            }
            return -1;
        }
       #endregion

              
        

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
            TransitionProperties tp;
            int targetId = -1;
            List<double> MaxV = bdt.ReturnValue(true);
            List<double> MinV = bdt.ReturnValue(false);
            List<Set<int>> abstractMap = bdt.ReturnLeaves();
            Sequence<Pair<int, Action>> cumulActSum = Sequence<Pair<int, Action>>.EmptySequence;
            double epsilon = 0.1;
            Dictionary<int, int> sumTarget = new Dictionary<int, int>();
            Action maxAct = null;
            int targetAbsId = -1;
            int sum = 0;
            UpdateRequirementMaps(actions, iState);
            Set<Action> newStateActs = new Set<Action>();
            Set<Action> oldActs = new Set<Action>(actions.Head);
            foreach (Action a in actions)
            {
                int tState = this.modelProgram.GetTargetState(iState, a, null, out tp).GetHashCode();
                targetAbsId = findAbstractId(tState, abstractMap);
                if (targetAbsId == -1)
                {
                    newStateActs = newStateActs.Add(a); 
                }
                else
                {
                    sum = sum + (int)(MaxV[targetAbsId] * Math.Pow(10.0, 9.0));
                    Pair<int, Action> np = new Pair<int, Action>(sum, a);
                    sumTarget.Add(sum, targetAbsId);
                    cumulActSum = cumulActSum.AddLast(np);
                }                    
            }
            if (!newStateActs.IsEmpty)
            {
                maxAct = newStateActs.Choose();
                System.Console.WriteLine("new action in new state " + maxAct.ToString());
                return maxAct;
            }
            else
            {
                
                Random rndNumbers = new Random();
                int rndNumber = rndNumbers.Next(sum);
                System.Console.WriteLine(sum + " " + rndNumber);
                foreach (Pair<int, Action> np in cumulActSum)
                {
                    System.Console.WriteLine(np.First + " " + np.Second.ToString());
                    if (rndNumber <= np.First)
                    {
                        maxAct = np.Second;
                        targetId = sumTarget[np.First];
                        break;
                    }
                    targetId = sumTarget[np.First];
                    maxAct = np.Second;
                }
                System.Console.WriteLine("old action in old state " + maxAct.ToString());
            }
            // Adaptive Refinement
            if (MaxV[targetId] - MinV[targetId] > epsilon)
            {
                if (i < requirementProperties.Count)
                {
                    string s1 = requirementProperties[i++];
                    bdt = bdt.Refine(s1, requireEnabledStateMap[s1]);
                    bdt.PrintTree(0);
                }
            }
            return maxAct;
        }

        private void UpdateRequirementMaps(Sequence<CompoundTerm> actions, IState iState)
        {
            Set<string> yesStrings = new Set<string>();
            foreach (Action a in actions)
            {
                System.Console.WriteLine(iState.GetHashCode() + a.ToString());
                foreach (string s in this.modelProgram.GetEnablingConditionDescriptions(iState, a, false))
                {
                    yesStrings = yesStrings.Add(s);
                    if (!requirementProperties.Contains(s))
                    {
                        requirementProperties = requirementProperties.AddLast(s);
                        requireEnabledStateMap = requireEnabledStateMap.Add(s, Set<int>.EmptySet);
                    }
                    requireEnabledStateMap = requireEnabledStateMap.Override(s, requireEnabledStateMap[s].Add(iState.GetHashCode()));
                }
            }
            bdt = bdt.addState(yesStrings, iState.GetHashCode());
            if (i == 0)
            {
                if (i < requirementProperties.Count)
                {
                    string s1 = requirementProperties[i++];
                    bdt = bdt.Refine(s1, requireEnabledStateMap[s1]);
                    bdt.PrintTree(0);
                }
            }            
        }

      

        #endregion

        /// <summary>
        /// Creates a model stepper that uses coverage points (when != null) to direct the selection of actions.
        /// If coverage == null, uses default coverage points that are (state.GetHashCode(), action.GetHashCode()) pairs.
        /// Uses RewardPolicy.MaximumReward for action selection.
        /// </summary>
        /// <param name="modelProgram">given model program</param>
        /// <param name="coverage">given coverage point names (may be null)</param>
        public static MDPNewAbstractStrategy CreateMDPNewAbstractStrategy(ModelProgram modelProgram, string[]/*?*/ coverage)
        {
            //return new MDPStrategy(modelProgram,new Set<string>(coverage));      
            return new MDPNewAbstractStrategy(modelProgram);
        }
    }
}

