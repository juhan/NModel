//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

namespace NModel.Attributes
{
    /// <summary>
    /// The [Requirement] attribute may be applied to any .NET attributable element. It includes
    /// a reference to the informal requirement upon which the model element is based. Requirement 
    /// strings provide traceability back to the informal requirement documents. They are printed in
    /// error contexts such as conformance failures and state invariant violations. They can also be 
    /// used to check for coverage of requirements. More than one [Requirement] attribute may be
    /// provided for any entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class RequirementAttribute : Attribute
    {
        readonly string documentation;

        /// <summary>
        /// Constructor of the [Requirement] attribute.
        /// </summary>
        /// <param name="documentation">The model name.</param>
        public RequirementAttribute(string documentation)
        {
            this.documentation = documentation;
        }

        /// <summary>
        /// The documentation string for this requirement.
        /// </summary>
        public string Documentation
        {
            get
            {
                return documentation;
            }
        }
    }
}
