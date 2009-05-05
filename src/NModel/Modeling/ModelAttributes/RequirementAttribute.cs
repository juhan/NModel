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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RequirementAttribute : Attribute
    {
        readonly string id;
        private static Set<Pair<string, string>> _allRequirementsInModels = Set<Pair<string, string>>.EmptySet;

        /// <summary>
        /// Returns a Set of all the requirements in the models
        /// </summary>
        public static Set<Pair<string, string>> AllRequirementsInModels
        {
            get { return RequirementAttribute._allRequirementsInModels; }
        }

        readonly string documentation;

        /// <summary>
        /// Constructor of the [Requirement] attribute.
        /// </summary>        
        /// <param name="id">The requirement id.</param>  
        /// <param name="documentation">The requirement description.</param>
        public RequirementAttribute(string id, string documentation)
        {
            this.id = id.Trim().ToLower();
            this.documentation = documentation.Trim().ToLower();
            _allRequirementsInModels = _allRequirementsInModels.Add(
                new Pair<string, string>(this.id, this.documentation));
        }

        /// <summary>
        /// The id string for this requirement.
        /// </summary>
        public string Id
        {
            get { return id; }
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
