//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Execution;
using NModel.Terms;
using Action = NModel.Terms.CompoundTerm;

namespace NModel.Conformance
{
    /// <summary>
    /// Provides functionality to define custom coverage point providers from a model program
    /// </summary>
    internal class CoveragePointProvider
    {
        ModelProgram mp;
        Set<string> transitionPropertyNames;

        /// <summary>
        /// Create a coverage point provider for a model program and a given set of property names
        /// </summary>
        /// <param name="mp">given model program</param>
        /// <param name="transitionPropertyNames">property names of interest</param>
        public CoveragePointProvider(ModelProgram mp, Set<string> transitionPropertyNames)
        {
            this.mp = mp;
            this.transitionPropertyNames = transitionPropertyNames;
        }

        /// <summary>
        /// Get coverage points from a given state and given action
        /// </summary>
        /// <param name="state">from given state</param>
        /// <param name="action">given action</param>
        /// <returns>coverage points</returns>
        public Bag<Term> GetCoveragePoints(IState state, Action action)
        {
            TransitionProperties tprops;
            mp.GetTargetState(state, action, transitionPropertyNames, out tprops);
            Bag<Term> res = Bag<Term>.EmptyBag;
            foreach (Bag<Term> props in tprops.Properties.Values)
                res = res.Union(props);
            return res;
        }
    }
}
