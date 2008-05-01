//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace NModel.Internals
{
        /// <summary>
        /// A container class for common error message strings.
        /// </summary>
        internal static class MessageStrings
        {
            /// <exclude />
            public const string ArgMustNotBeNegative = "The argument may not be less than zero." ;

            /// <exclude />
            public const string ArrayTooSmall = "The array is too small to hold all of the items.";

            /// <exclude />
            public const string ChooseInvalidArgument = "Choose called on empty collection value";

            /// <exclude />
            public const string MaximumInvalidArgument = "Minimum called on empty collection value";

            /// <exclude />
            public const string MinimumInvalidArgument = "Minimum called on empty collection value";

            /// <exclude />
            public const string MapAddInvalidArgument = "Map.Add: Duplicate key '{0}'";

            /// <exclude />
            public const string MapKeyNotFound = "Map.[]: key '{0}' not in Map";

            /// <exclude />
            public const string MergeInvalidArgument = "Map.Merge: Cannot merge maps having shared keys with inconsistent values.";

            /// <exclude />
            public const string MapIntersectInvalidArgument = "Map.Intersect: Cannot intersect maps having shared keys with inconsistent values.";

            /// <exclude />
            public const string MapRemoveInvalidArgument = "Map.Remove: Cannot remove key/value pair with inconsistent value; use RemoveKey to ignore inconsistency.";

            /// <exclude />
            public const string MapDifferenceInvalidArgument = "Map.Difference: Cannot perform difference on maps having shared keys with inconsistent values.";

            /// <exclude />
            public const string CantTakeHeadOfEmptySequence = "Seq.Head: Attempt to take head of empty sequence.";

            /// <exclude />
            public const string CantTakeTailOfEmptySequence = "Seq.Tail: Attempt to take tail of empty sequence.";

            /// <exclude />
            public const string MapDomainErrorOnConvert = "Map.Convert: Cannot map to duplicate domain values";
            
            /// <exclude />
            public const string ComparableTypeRequired = "Type '{0}' cannot be used where IComparable is expected";
            
            /// <exclude />
            public const string LabeledInstanceRequired = "Context requires type LabeledInstance, saw '{0}'";

            /// <exclude />
            public const string EnumeratedInstanceRequired = "Context requires type LabeledInstance, saw '{0}'";

            /// <exclude />
            public const string CompoundValueRequired = "Context requires type LabeledInstance, saw '{0}'";

            /// <exclude />
            public const string RuntimeTypeError = "Context requires type '{0}', saw type '{1}'";

            /// <exclude />
            public const string ComparableResultRequired = "Action '{0}' did not return a result of type IComparable, saw '{1}'";

            /// <exclude />
            public const string NonEmptySequenceRequired = "Sequence.Tail called on empty sequence; must be nonempty.";

            /// <exclude />
            public const string SequenceIndexOutOfRange = "Sequence.Item called with out-of-range index.";

            public const string FactorialArgumentOutOfRange = "NModel.Combinatorics.Factorial: out of range. Must be between 0 and 12.";
            public const string CoveragePointTypeError = "Coverage point value did not satisfy AbstractValue.IsAbstractValue() test";
            
            /// <summary>
            /// Invokes <see cref="String.Format(System.IFormatProvider, string, object[])">String.Format</see> using <see cref="System.Globalization.CultureInfo.CurrentCulture" /> as the formatting "culture."
            /// </summary>
            /// <param name="s">The format string</param>
            /// <param name="args">The format arguments</param>
            /// <returns>The formatted string</returns>
            internal static string LocalizedFormat(string s, params string[] args)
            {
                return String.Format(System.Globalization.CultureInfo.CurrentCulture, s, args);
            }
        
        }
}
