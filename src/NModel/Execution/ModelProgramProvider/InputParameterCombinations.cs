//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using NModel.Terms;
//using NModel.Internals;

//namespace NModel.Execution
//{

//    public interface IParameterGenerator 
//    {
//        IEnumerable<Sequence<Term>> GetParameters(IState state);
//    }


//    /// <summary>
//    /// Provides sequences of input parameter combinations for a given action method
//    /// </summary>
//    internal class InputParameterCombinations
//    {
//        InputParameterDomain[] domains;
//        List<Sequence<Term>>/*?*/ precomputedParameterCombinations;
//        // EnablingCondition stateIndependentCondition;
//        IParameterGenerator/*?*/ userProvidedParameterGenerator;

//        internal InputParameterCombinations(IParameterGenerator/*?*/ userProvidedParameterGenerator, InputParameterDomain[] domains, EnablingCondition stateIndependentCondition)
//        {
//            this.userProvidedParameterGenerator = userProvidedParameterGenerator;
//            this.domains = domains;
//            this.stateIndependentCondition = stateIndependentCondition;
//        }

//        internal IEnumerable<Sequence<Term>> GetParameters(InterpretationContext c, IState state)
//        {
//            //if (userProvidedParameterGenerator == null)
//            //{
//            //    if (precomputedParameterCombinations == null)
//            //        PrecomputeParameterCombinations(c);
//            //    //BOOGIE ISSUE: PrecomputeParameterCombinations() ensures that precomputedParameterCombinations != null
//            //    foreach (Sequence<Term> args in precomputedParameterCombinations)
//            //        yield return args;
//            //}
//            //else
//            //{
//            //    foreach (Sequence<Term> args in userProvidedParameterGenerator.GetParameters(state))
//            //    {
//            //        if (stateIndependentCondition.Holds(c, args))
//            //            yield return args;
//            //    }
//            //}

//            foreach (Sequence<Term> args in userProvidedParameterGenerator.GetParameters(state))
//            {
//               yield return args;
//            }
//        }


//        static bool AreEqual(Sequence<Term> args1, Sequence<Term> args2)
//        {
//            if (args1 == args2) return true;
//            if (args1 == null || args2 == null) return false;
//            if (args1.Count != args2.Count) return false;
//            for (int i = 0; i < args1.Count; i++)
//                if (!EQ(args1[i],args2[i])) return false;
//            return true;
//        }

//        static bool EQ(object o1, object o2)
//        {
//            if (o1 == o2) return true;
//            if (o1 == null) return false;
//            return o1.Equals(o2);
//        }

//        static internal bool TryMatch(Sequence<Term> args1, Sequence<Term> args2, out Sequence<Term> args)
//            //^requires args1.Length == args2.Length;
//        {
//            if (AreEqual(args1, args2))
//            {
//                args = args1;
//                return true;
//            }
//            //oterwise build an new argumentlist from scratch
//            LinkedList<Term> res = new LinkedList<Term>();
            
//            for (int i = 0; i < args1.Count; i++)
//            {
//                if (Object.Equals(args1[i], Any.Value))
//                {
//                    res.AddLast(args2[i]);
//                }
//                else if (Object.Equals(args2[i],  Any.Value) || Object.Equals(args1[i], args2[i]))
//                {
//                    res.AddLast(args1[i]);
//                }
//                else 
//                {
//                    args = null;
//                    return false;
//                }
//            }
//            args = new Sequence<Term>(res);
//            return true;
//        }

//        internal IEnumerable<Sequence<Term>> GetMatchingParameters(InterpretationContext c, Sequence<Term> args1, Symbol sym, IState state)
//        {
//            //if (userProvidedParameterGenerator == null)
//            //{
//            //    if (precomputedParameterCombinations == null)
//            //        PrecomputeParameterCombinations(c);
//            //    //BOOGIE ISSUE: PrecomputeParameterCombinations() ensures that precomputedParameterCombinations != null
//            //    foreach (Sequence<Term> args2 in precomputedParameterCombinations)
//            //    {
//            //        Sequence<Term> args;
//            //        if (TryMatch(args1, args2, out args))
//            //        {
//            //            yield return args;
//            //        }
//            //    }
//            //}
//            //else
//            //{
//            //    foreach (Sequence<Term> args2 in userProvidedParameterGenerator.GetParameters(state))
//            //    {
//            //        if (stateIndependentCondition.Holds(c, args2))
//            //        {
//            //            Sequence<Term> args;
//            //            if (TryMatch(args1, args2, out args))
//            //            {
//            //                yield return args;
//            //            }
//            //        }
//            //    }
//            //}

//            foreach (Sequence<Term> args2 in userProvidedParameterGenerator.GetParameters(state))
//            {
//                Sequence<Term> args;
//                    if (TryMatch(args1, args2, out args))
//                    {
//                        yield return args;
//                    }
                
//            }
//        }

//        private void PrecomputeParameterCombinations(InterpretationContext c)
//        //^ ensures this.precomputedParameterCombinations != null;
//        {
//            int k = MaxNumberOfCombinations();
//            precomputedParameterCombinations = new List<Sequence<Term>>(k);
//            if (k > 0)
//            {
//                int[] elemIDs = new int[domains.Length];
//                do
//                {
//                    LinkedList<Term> theArgs = new LinkedList<Term>();
//                    for (int j = 0; j < domains.Length; j++)
//                        //BOOGIE: here 
//                        theArgs.AddLast(AbstractValue.GetTerm(domains[j].Values[elemIDs[j]]));

//                    Sequence<Term> args = new Sequence<Term>(theArgs);
//                    if (stateIndependentCondition.Holds(c, args))
//                        precomputedParameterCombinations.Add(args);
//                }
//                while (IncrElemIDs(elemIDs));
//            }
//        }

//        /// <summary>
//        /// Increment in lexicographic order.
//        /// Suppose there are 3 domains with sizes 3 x 2 x 3
//        /// the generated elemIDs occur in the following order
//        /// [0,0,0] -> [0,0,1] -> [0,0,2] -> 
//        /// [0,1,0] -> [0,1,1] -> [0,1,2] -> 
//        /// [1,0,0] -> [1,0,1] -> ...
//        /// </summary>
//        /// <param name="elemIDs">given sequence of element ids</param>
//        /// <returns>true iff the sequence could be incremented</returns>
//        private bool IncrElemIDs(int[] elemIDs)
//        //^ requires elemIDs.Length == domains.Length;
//        //^ requires elemIDs.Length > 0;
//        {
//            if (domains.Length == 0) return false;
//            int i = elemIDs.Length - 1;
//            while (elemIDs[i] == domains[i].Values.Length - 1)
//            {
//                elemIDs[i] = 0;
//                i -= 1;
//                if (i < 0) return false; //reached maximum
//            }
//            elemIDs[i] += 1;
//            return true;
//        }

//        /// <summary>
//        /// Maximum number of elements in the cartesian product of all domains.
//        /// Returns 0 if one of the domains is empty.
//        /// </summary>
//        private int MaxNumberOfCombinations()
//        //^ ensures result >= 0;
//        {
//            int res = 1;
//            for (int i = 0; i < domains.Length; i++)
//            {
//                res = res * domains[i].Values.Length;
//            }
//            return res;
//        }
//    }
//}
