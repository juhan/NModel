//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

namespace NModel.Attributes
{
    /// <summary>
    /// The [Sort] attribute indicates the "sort" (abstract type) of a type. It may be applied
    /// to a class or an enum declaration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, AllowMultiple=false)]
    public sealed class SortAttribute : Attribute
    {
        string name;

        /// <summary>
        /// Constructor of the [Sort] attribute.
        /// </summary>
        /// <param name="name">The name of the sort.</param>
        public SortAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// The name of the sort.
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

