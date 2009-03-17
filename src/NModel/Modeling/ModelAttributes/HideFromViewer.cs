//-------------------------------------
// Add the [HideFromViewer] to a field that you don't want to be shown in the viewer's tooltip  
//-------------------------------------
using System;

namespace NModel.Attributes
{
    /// <summary>
    /// Don't show this field in the FSM viewer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HideFromViewerAttribute : Attribute
    {
    }
}

