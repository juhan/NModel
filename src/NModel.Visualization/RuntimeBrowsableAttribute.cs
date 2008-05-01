//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Permissions;

namespace NModel.Visualization
{
    /// <summary>
    /// Specifies whether a property or event should be displayed in a property browsing window at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class RuntimeBrowsableAttribute : Attribute
    {
        /// <summary>
        /// Specifies that a property or event can be modified at runtime. This <see langword='static '/> field is read-only.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly RuntimeBrowsableAttribute Yes = new RuntimeBrowsableAttribute(true);

        /// <summary>
        /// Specifies that a property or event cannot be modified at runtime. This <see langword='static '/> field is read-only.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly RuntimeBrowsableAttribute No = new RuntimeBrowsableAttribute(false);

        /// <summary>
        /// Specifies the default value for the <see cref='RuntimeBrowsableAttribute'/>,
        /// which is <see cref='RuntimeBrowsableAttribute.No'/>. This <see langword='static '/> field is read-only.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly RuntimeBrowsableAttribute Default = No;

        private bool browsable = true;

        /// <summary>
        /// Initializes a new instance of the <see cref='RuntimeBrowsableAttribute'/> class.
        /// </summary>
        public RuntimeBrowsableAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='RuntimeBrowsableAttribute'/> class.
        /// </summary>
        public RuntimeBrowsableAttribute(bool browsable)
        {
            this.browsable = browsable;
        }

        /// <summary>
        /// Gets a value indicating whether an object is browsable at runtime.
        /// </summary>
        public bool Browsable
        {
            get
            {
                return browsable;
            }
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        public override bool Equals(object/*?*/ obj)
        {
            if (obj == this)
            {
                return true;
            }

            RuntimeBrowsableAttribute other = obj as RuntimeBrowsableAttribute;
            
            return (other != null) && other.Browsable == browsable;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return browsable.GetHashCode();
        }

        /// <summary>
        /// Determines if this attribute is the default.
        /// </summary>
        public override bool IsDefaultAttribute()
        {
            return (this.Equals(Default));
        }
    }
}