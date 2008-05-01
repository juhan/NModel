//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace NModel.Attributes
{
    /// <summary>
    /// Indicates that an action method is never enabled for top-level execution. Instead, it may only 
    /// be invoked as a subaction using the <seealso cref="Execute"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ScenarioAttribute : Attribute
    {
    }
}
