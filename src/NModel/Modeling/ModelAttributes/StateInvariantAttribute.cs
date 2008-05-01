//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

namespace NModel.Attributes
{
    /// <summary>
    /// The [StateInvariant] attribute can be applied to a method or a property. 
    /// It specifies a property that must hold in every model state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class StateInvariantAttribute : Attribute
    {
        /// <summary>
        /// Constructor of the accepting state condition attribute. 
        /// </summary>
        public StateInvariantAttribute()
        {
        }
    }


}

