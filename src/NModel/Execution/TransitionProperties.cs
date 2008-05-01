//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel;
using NModel.Terms;

namespace NModel.Execution
{
    /// <summary>
    /// Set of named properties used to report meta-information such as coverage and state filtering predicates.
    /// </summary>
    /// <seealso cref="ModelProgram.GetTargetState"/>
    public class TransitionProperties
    {
        Map<string, Bag<Term>> properties;

        /// <summary>
        /// Map of property names to property values. Each property value consists of a 
        /// multiset of terms. For example, the property value might be the value of a Boolean function
        /// that controls state filtering. Or, it might correspond to the "coverage" of the model that 
        /// results from a step of a <see cref="ModelProgram"/>. In this case, the value might denote the 
        /// line numbers or blocks of the model program that were exercised in this step, or a projection of the state 
        /// space or a reference to section numbers of a requirements document to indicate
        /// that the functionality defined by that section was exercised.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Map<string, Bag<Term>> Properties { get { return this.properties; } }

        /// <summary>
        /// Retrieve a property by name
        /// </summary>
        /// <param name="propertyName">The name of the property to be retrieved</param>
        /// <param name="propertyValue">Output parameter that yields the value corresponding to <paramref name="propertyName"/>
        /// or the empty bag if not found</param>
        /// <returns>True if <paramref name="propertyName"/> was found; false otherwise.</returns>
        public bool TryGetPropertyValue(string propertyName, out Bag<Term> propertyValue)
        {
            return this.properties.TryGetValue(propertyName, out propertyValue);
        }

        /// <summary>
        /// TransitionProperties constructor
        /// </summary>
        public TransitionProperties()
        {
            this.properties = Map<string, Bag<Term>>.EmptyMap;
        }

        TransitionProperties(Map<string, Bag<Term>> properties)
        {
            this.properties = properties;
        }

        /// <summary>
        /// Merges two property sets by multiset-union of corresponding entries.
        /// </summary>
        /// <param name="other">The property set to be merged</param>
        /// <returns>The union of this property set and <paramref name="other"/></returns>
        public TransitionProperties Union(TransitionProperties other)
        {
            if (null == other)
                throw new ArgumentNullException("other");

            Map<string, Bag<Term>> result = this.properties.Count > other.properties.Count ? this.properties : other.properties;
            Map<string, Bag<Term>> smaller = this.properties.Count > other.properties.Count ? other.properties : this.properties;
            foreach (Pair<string, Bag<Term>> kv in smaller)
            {
                Bag<Term> propVal;
                if (result.TryGetValue(kv.First, out propVal))
                    result = result.Override(kv.First, propVal.Union(kv.Second));
                else
                    result = result.Add(kv);
            }
            return new TransitionProperties(result);
        }

        /// <summary>
        /// Updates the properties by adding a new value. If <paramref name="propertyName"/>
        /// already exists, then the existing value will be merged with <paramref name="propertyValue"/>
        /// via multiset union.
        /// </summary>
        /// <param name="propertyName">The name of the property to be added</param>
        /// <param name="propertyValue">The value to be added</param>
        public void AddProperty(string propertyName, Bag<Term> propertyValue)
        {
            Bag<Term> existingValue;
            if (this.properties.TryGetValue(propertyName, out existingValue))
                this.properties = this.properties.Override(propertyName, existingValue.Union(propertyValue));
            else
                this.properties = this.properties.Add(propertyName, propertyValue);
        }
    }
}
