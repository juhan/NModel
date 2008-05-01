//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

namespace NModel.Attributes
{
    /// <summary>
    /// <para>The [Split] attribute indicates that an action method will be split into "start" and "finish"
    /// transitions. In this case the start transition contains the method's input parameters (including byref
    /// parameters) as arguments. The finish transition contains the method's outputs (return value, 
    /// byref arguments and output arguments) as parameters.</para>
    /// <para>If the [Split] attribute is not provided, then methods with no outputs (void return value and 
    /// no byref or out parameters) will not be split. Methods with outputs are always split.
    /// </para>
    /// <para>An error occurs if the [Split] attribute is used for an action method that has a non-void return
    /// value or has byref or out arguments.</para>
    /// <para>An error occurs if the [Action] attribute is not present whenever the [Split] attribute is used.</para>
    /// </summary>
    /// <note>The typical use case for this attribute is to mark actions that have no outputs (for example, in 
    /// scenario model programs) as split so that they will combine with actions of another model program 
    /// that may contain output parameters.</note>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SplitAttribute : Attribute
    {
    }
}
