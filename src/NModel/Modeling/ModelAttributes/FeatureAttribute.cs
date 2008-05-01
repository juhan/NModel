//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

namespace NModel.Attributes
{
    /// <summary>
    /// The [Feature] attribute may be applied to a class to group related state variables and actions. The [Feature] attribute
    /// includes the name of the Feature. A [Feature] attribute (with the same Feature name) may appear on multiple classes.
    /// This is a way to group related classes. When loading a LibraryModelProgram, the features to be included
    /// may be configured.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class FeatureAttribute : Attribute
    {
        string name;

        /// <summary>
        /// Constructor of the [Feature] attribute. The Feature name defaults to the name of the target class.
        /// </summary>
        public FeatureAttribute()
        {
            this.name = "";
        }

        /// <summary>
        /// Constructor of the [Feature] attribute.
        /// </summary>
        /// <param name="name">The Feature name.</param>
        public FeatureAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// The name of the Feature.
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

