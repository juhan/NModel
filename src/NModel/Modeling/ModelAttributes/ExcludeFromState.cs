//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

namespace NModel.Attributes
{
    /// <summary>
    /// Indicates that a field is not a model variable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ExcludeFromStateAttribute : Attribute
    {
    }
}
