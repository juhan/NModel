//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace NModel.Internals
{
    /// <summary>
    /// Abstract implementation helper. Use <see cref="CollectionValue&lt;T&gt;" /> instead.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    abstract public class CachedHashCollectionValue<T> : CollectionValue<T> 
    {
        int? hashCode = null;

        /// <summary>
        /// Structural equality 
        /// </summary>
        /// <param name="obj">The value to be compared with this value.</param>
        /// <returns>True if structurally equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (this.hashCode != null)
            {
                CachedHashCollectionValue<T> other = obj as CachedHashCollectionValue<T>;
                return ((object)other != null && other.hashCode != null && other.hashCode != this.hashCode) ? false : base.Equals(obj);
            }
            else
                return base.Equals(obj);
        }

        /// <summary>
        /// The hash code
        /// </summary>
        /// <returns>The hash code (either from the cache or calculated)</returns>
        public override int GetHashCode()
        {
            return (this.hashCode != null ? (int) this.hashCode : CalculateHashCode());
        }

        /// <summary>
        /// Calculates the hash code to be cached.
        /// </summary>
        /// <returns>The hash code</returns>
        protected virtual int CalculateHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Required to be invoked when any internal state could invalidate the cached hash code.
        /// </summary>
        protected virtual void InvalidateCache()
        {
            this.hashCode = null;
        }
    }
}
