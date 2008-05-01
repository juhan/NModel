//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;


namespace NModel.Attributes
{
    /// <summary>
    /// The [Abstract] attribute may be applied to definitions of enums and classes. It indicates
    /// that the objects of the attributed enum or class will be considered abstract, i.e. they will not have an ordering.
    /// It is useful when using the state isomorphism based state space reduction.
    /// </summary>
    [AttributeUsage((AttributeTargets.Enum | AttributeTargets.Class), AllowMultiple = false)]
    public sealed class AbstractAttribute : Attribute 
    {
        /// <summary>
        /// Constructor of the [Abstract] attribute.        
        /// </summary>
        public AbstractAttribute() { }
    }
}
