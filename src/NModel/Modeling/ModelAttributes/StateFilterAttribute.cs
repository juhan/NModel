using System;
using System.Collections.Generic;
using System.Text;

namespace NModel.Attributes
{
    /// <summary>
    /// The [StateFilter] attribute can be applied to a method or a property. 
    /// It indicates that its target method or property is a state filter.
    /// The value true indicates that the state will be included during exploration;
    /// the value false means that the state will be excluded from exploration.
    /// State filters can be used in conjunction with a [Feature] attribute 
    /// so that they can be selectively applied.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class StateFilterAttribute : Attribute
    {
        /// <summary>
        /// Constructor of the state filter attribute. 
        /// </summary>
        public StateFilterAttribute()
        {
        }
    }
}
