//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

namespace NModel.Attributes
{
    /// <summary>
    /// The [AcceptingStateCondition] attribute can be applied to a method or a property. 
    /// The method or property to which the attribute is applied must be Boolean and static.
    /// The conjunction of all the accepting state conditions, when evaluated in a given state,
    /// determines if the state is an accepting state or not.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method , AllowMultiple = false)]
    public sealed class AcceptingStateConditionAttribute : Attribute
    {
        /// <summary>
        /// Constructor of the accepting state condition attribute. 
        /// </summary>
        public AcceptingStateConditionAttribute()
        {
        }
    }
}
