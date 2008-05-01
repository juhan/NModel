//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel;

namespace NModel.Internals
{
    /// <summary>
    /// Provides functions for formatting values in human-readable form.
    /// </summary>
    public static class PrettyPrinter
    {            
        /// <summary>
        /// Formats values in human-readble form as strings.
        /// </summary>
        /// <param name="sb">The string builder instance that contains the context for pretty printing.</param>
        /// <param name="obj">The object to be pretty printed.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="sb"/> is null.</exception>
        public static void Format(StringBuilder sb, object obj)
        {
            if (sb == null)
                throw new ArgumentNullException("sb");

            if (obj == null)
            {
                sb.Append("null");
                return;
            }
            string s = obj as string;
            if (s != null)
            {
                sb.Append('"');
                sb.Append(s);
                sb.Append('"');
                return;
            }

            Type t = obj as Type;
            if (t != null)
            {
                FormatType(sb, t);
                return;
            }

            sb.Append(obj.ToString());
            return;
        }

        
        /// <summary>
        /// Formats type names in human-readable form. This removes special characters in generic type names
        /// and converts System.String to string, etc.
        /// </summary>
        /// <param name="sb">The string builder instance that contains the context for pretty printing.</param>
        /// <param name="t">The type to be pretty printed.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="sb"/> is null.</exception>
        public static void FormatTypeName(StringBuilder sb, Type t)
        {
            if (sb == null)
                throw new ArgumentNullException("sb");

            if (t.IsGenericType)
            {
                string name = t.Name;
                int tailPos = name.IndexOf('`');
                sb.Append(tailPos > 0 ? name.Substring(0, tailPos) : name);
            }
            else
            {
                // lookup for builtins like int and string, otherwise use type name
                string name;
                if (!AbstractValue.GetLiteralTypes().TryGetValue(t, out name))
                    name = t.Name;
                sb.Append(name);
            }
        }

        /// <summary>
        /// Formats type in human-readable form. Includes instantiated generic type arguments if present.
        /// </summary>
        /// <param name="sb">The string builder instance that contains the context for pretty printing.</param>
        /// <param name="t">The object to be pretty printed.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="sb"/> is null.</exception>
        public static void FormatType(StringBuilder sb, Type t)
        {
            if (sb == null)
                throw new ArgumentNullException("sb");
            if (t == null)
                throw new ArgumentNullException("t");

            FormatTypeName(sb, t);
            Type[] args = t.GetGenericArguments();
            if (args != null && args.Length > 0)
            {
                bool isFirst = true;
                sb.Append("<");
                foreach (Type arg in args)
                {
                    if (!isFirst) sb.Append(", ");
                    FormatType(sb, arg);
                    isFirst = false;
                }
                sb.Append(">");
            }
        }
    }
}
