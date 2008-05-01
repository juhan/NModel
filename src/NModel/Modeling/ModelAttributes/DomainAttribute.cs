//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace NModel.Attributes
{
    /// <summary>
    /// <para>The [Domain] attribute may be applied to a parameter of an action method to indicate the possible
    /// values that may appear as arguments. The attribute gives the name of a Set-valued method or get-property 
    /// defined in the current class. The method may be instance-based or static.</para> 
    /// <para>The method that contains the attributed parameter must have the [Action] attribute, or an
    /// error will occur.</para>
    /// <para>Only one [Domain] or [New] attribute may be used for a parameter.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DomainAttribute : Attribute
    {
        readonly private string name;

        /// <summary>
        /// Constructor of the [Domain] attribute.
        /// </summary>
        /// <param name="name">The name of a method or property defined by the current class.</param>
        public DomainAttribute(string name)
        //^ requires methodName.Length > 0;
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty");

            this.name = name;
        }

        /// <summary>
        /// Name of the domain
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }
    }
}
