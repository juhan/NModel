//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using System.Reflection;
//using NModel.Algorithms;
//using NModel.Terms;
//using NModel.Internals;
//using NModel.Utilities;


//namespace NModel.Execution
//{
//    internal class ActionInvocationPoint : CompoundValue  // , IControlPoint
//    {
//        readonly LibraryModelProgram mpp;  //fixed for all actions and states
//        readonly CompoundTerm startAction;
//        readonly IState startState;
//        readonly ActionInfo actionInfo;     //fixed for given action
//        readonly Term/*?*/[] args;          //fixed for given action

//        internal ActionInvocationPoint(LibraryModelProgram mpp, CompoundTerm startAction, IState startState)
//        //^ requires startAction.ActionSymbol.Kind == ActionKind.Start;
//        {
//            ActionInfo actInfo = mpp.actionInfoMap[startAction.FunctionSymbol as Symbol];
//            Term[] argsTmp = new Term[actInfo.parameterInfos.Length];

//            Sequence<Term> inputArgs = startAction.Arguments;

//            if (inputArgs != null) //the action takes some input arguments
//            {
//                //j is the input argument counter for the start action
//                int j = 0;
//                //i is the method parameter counter
//                for (int i = 0; i < actInfo.parameterInfos.Length; i++)
//                    if (ReflectionHelper.IsInputParameter(actInfo.parameterInfos[i]))
//                        argsTmp[i] = (Object.Equals(inputArgs[j], Any.Value) ? actInfo.defaultInputParameters[j] : inputArgs[j++]);
//            }
//            //initialize the fields
//            this.mpp = mpp;
//            //this.startAction = startAction;
//            this.startState = startState;
//            this.actionInfo = actInfo;
//            this.args = argsTmp;
//            this.startAction = startAction;
//        }

//        //#region IControlPoint Members

//        //public bool IsAccepting
//        //{
//        //    get { return false; } //cannot be accepting in the middle of execution
//        //}

//        ////public ModelProgram ModelProgram
//        ////{
//        ////    get { return mpp; }
//        ////}

//        //public bool IsPotentiallyEnabled(IState state, Symbol actionSymbol)
//        //{
//        //    bool res = actionInfo.finishActionSymbol.Equals(actionSymbol);
//        //    return res;
//        //}

//        //public IEnumerable<Symbol> PotentiallyEnabledActionSymbols(IState state)
//        //{
//        //    yield return actionInfo.finishActionSymbol;
//        //}

//        //public bool IsEnabled(IState state, CompoundTerm action)
//        //{
//        //    if (!startState.Equals(state)) return false;
//        //    if (step == null) step = DoStep();
//        //    return step.Action.Equals(action);
//        //}

//        //public IEnumerable<CompoundTerm> GetActions(IState state, Symbol actionSymbol)
//        //{
//        //    if (!startState.Equals(state) || !actionSymbol.Equals(actionInfo.finishActionSymbol))
//        //        yield break;
//        //    else
//        //    {
//        //        if (step == null) step = DoStep();
//        //        //TBD: DoStep() ensures that step != null
//        //        yield return step.Action;
//        //    }
//        //}

//        //public IEnumerable<CompoundTerm> GetActionInstances(IState state, CompoundTerm action)
//        //{
//        //    if (!startState.Equals(state) || !action.FunctionSymbol.Equals(actionInfo.finishActionSymbol))
//        //        yield break;
//        //    else
//        //    {
//        //        if (step == null) step = DoStep();
//        //        //TBD: DoStep() ensures that step != null
//        //        Sequence<Term> args;
//        //        if (InputParameterCombinations.TryMatch(action.Arguments, step.Action.Arguments, out args))
//        //            yield return step.Action;
//        //    }
//        //}

//        //Step/*?*/ step;
//        //public IEnumerable<Step> GetSteps(IState state, params CompoundTerm[] actions)
//        //{
//        //    if (step == null) step = DoStep();
//        //    //if (Array.Exists<Action>(actions,
//        //    //    new Predicate<Action>(delegate(Action a) { return a.Equals(step.Action); })))
//        //    if (ContainsThisAction(actions))
//        //    {
//        //        yield return step;
//        //    }
//        //}

//        //bool ContainsThisAction(CompoundTerm[] actions)
//        //    //^ requires step != null;
//        //{
//        //    for (int i = 0; i < actions.Length; i++)
//        //        if (!(step.Action.Equals(actions[i]))) return false;
//        //    return true;
//        //}


//        //public IEnumerable<Step> GetAllSteps(IState state)
//        //{
//        //    if (!state.Equals(startState))
//        //        yield break;
//        //    if (step != null)
//        //        yield return step;
//        //    step = DoStep();
//        //    yield return step;
//        //}

//        //Step DoStep()
//        //{
//        //    //execute the method
//        //    mpp.SetState(startState);

//        //    // Result of invocation must be a value term (must support IComparable)
//        //    Term res;
//        //    object/*?*/ resultObj;
//        //    if (actionInfo.method.IsStatic)
//        //    {
//        //        resultObj = actionInfo.method.Invoke(null, args);
//        //    }
//        //    else
//        //    {
//        //        if (args.Length < 1)
//        //            throw new ArgumentException("arguments");
//        //        object obj = args[0];
//        //        object[] restArgs = new object[args.Length - 1];
//        //        for (int i = 1; i < args.Length; i += 1)
//        //            restArgs[i - 1] = args[i];
//        //        resultObj = actionInfo.method.Invoke(obj, restArgs);
//        //    }

//        //    if (resultObj == null)
//        //        res = null;
//        //    else
//        //    {
//        //       res = resultObj as Term;
//        //       if (res == null)
//        //           throw new InvalidOperationException(MessageStrings.LocalizedFormat(MessageStrings.ComparableResultRequired, actionInfo.ToString(), resultObj.GetType().ToString()));
//        //    }
            
//        //    IState endState = mpp.GetState(null);

//        //    //collect the output parameters
//        //    Term[] outargs = new Term[actionInfo.outArity];
//        //    int j = 0;
//        //    for (int i = 0; i < args.Length; i++)
//        //    {
//        //        if (ReflectionHelper.IsOutputParameter(actionInfo.parameterInfos[i]))
//        //        {
//        //            outargs[j] = args[i];
//        //            j += 1;
//        //        }
//        //    }
//        //    if (actionInfo.method.ReturnType != typeof(void))
//        //        outargs[j] = res;

//        //    CompoundTerm finishAction = new CompoundTerm(actionInfo.finishActionSymbol, outargs);
//        //    return new Step(finishAction, endState);
//        //}

//        //#endregion

//        public override bool Equals(object obj)
//        {
//            ActionInvocationPoint aip = obj as ActionInvocationPoint;
//            if (aip == null) return false;
//            return
//                this.startAction.Equals(aip.startAction) &&
//                this.startState.Equals(aip.startState);
//        }

//        public override int GetHashCode()
//        {
//            return TypedHash<ActionInvocationPoint>.ComputeHash(startAction, startState);
//        }
//    }
//}
