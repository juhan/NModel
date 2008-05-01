//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NModel.Algorithms;
using NModel.Terms;
using NModel.Internals;

namespace NModel.Execution
{
    /// <summary>
    /// A field of a .Net class that has corresponds to a state variable. May be a static field or an instance field.
    /// </summary>
    internal class Field
    {
        internal StateVariable stateVariable;

        internal FieldInfo field;

        public Field(FieldInfo field)
        {
            // to do: ensure that context is fresh in LabeledInstanceBase, or implement some other type sort
            // mechanism. The problem is that sort is defined only within the context of a particular model program.
            Type t = field.GetType();
            Symbol sort = LabeledInstance.TypeSort(t);
            this.stateVariable = new StateVariable(GetStateVariableName(field), sort);
            this.field = field;
        }

        public IComparable/*?*/ GetValue(InterpretationContext context)
        {
            if (!field.IsStatic)
            {
                // TO DO: Implement instance field maps
                Map<LabeledInstance, IComparable/*?*/> result = Map<LabeledInstance, IComparable/*?*/>.EmptyMap;

                Symbol sort = AbstractValue.TypeSort(field.DeclaringType);
                foreach (LabeledInstance instance in context.InstancePoolValues(sort)) 
                {
                    object obj = field.GetValue(instance);
                    if (obj == null)
                        result = result.Add(instance, null);
                    else
                    {
                        IComparable comparable = obj as IComparable;
                        if ((object)comparable == null)
                            throw new ArgumentException(MessageStrings.LocalizedFormat(MessageStrings.RuntimeTypeError, typeof(IComparable).ToString(), obj.GetType().ToString()));
                        result = result.Add(instance, comparable);
                    }
                }
                return result;
            }
            else
            {
                object obj = field.GetValue(null);
                if (obj == null) return null;

                IComparable comparable = obj as IComparable;
                if ((object)comparable == null)
                    throw new ArgumentException(MessageStrings.LocalizedFormat(MessageStrings.RuntimeTypeError, typeof(IComparable).ToString(), obj.GetType().ToString()));
                return comparable;
            }
        }

        public void SetValue(IComparable/*?*/ value)
        {
            if (field.IsStatic)
            {
                field.SetValue(null, value);
            }
            else
            {
                Map<LabeledInstance, IComparable/*?*/> fieldMap = value as Map<LabeledInstance, IComparable/*?*/>;

                if ((object)fieldMap == null)
                    throw new InvalidOperationException();
                foreach (Pair<LabeledInstance, IComparable/*?*/> fieldBinding in fieldMap)
                {
                    field.SetValue(fieldBinding.First, fieldBinding.Second);
                }
            }
        }

        static string GetStateVariableName(FieldInfo field)
        {
            return field.Name;
        }
    }   
}
