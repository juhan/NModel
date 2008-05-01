//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using NModel.Execution;

namespace NModel.Attributes
{
    /// <summary>
    /// The [TransitionProperty] attribute may be applied to methods, properties or fields. It indicates that the
    /// method, property or field should be evaluated at the end of a model step and reported as a named
    /// property. 
    /// </summary>
    /// <seealso cref="ModelProgram.GetTargetState"/>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class TransitionPropertyAttribute : Attribute
    {
        readonly string/*?*/ name;

        /// <summary>
        /// Constructor of the [TransitionProperty] attribute. The transition property name will be the 
        /// name of the attributed method, property or field.        
        /// </summary>
        public TransitionPropertyAttribute()
        {
        }

        /// <summary>
        /// Constructor of the [TransitionProperty] attribute.
        /// </summary>
        /// <param name="name">The name of the transition property.</param>
        public TransitionPropertyAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// The name of the transition property.
        /// </summary>
        public string/*?*/ Name
        {
            get
            {
                return name;
            }
        }
    }
}

