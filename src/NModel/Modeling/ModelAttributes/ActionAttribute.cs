//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;

namespace NModel.Attributes
{
    /// <summary>
    /// The [Action] attribute may be applied to methods. It indicates that the method is a model action.
    /// The containing class must have a [Model] attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public sealed class ActionAttribute : Attribute
    {
        readonly string/*?*/ name;
        string start;
        string finish;
        int weight;


        /// <summary>
        /// Constructor of the [Action] attribute. The action name will be the name of the attributed method.        
        /// </summary>
        public ActionAttribute()
        {
        }

        /// <summary>
        /// Constructor of the [Action] attribute.
        /// </summary>
        /// <param name="name">The name of the action. This does not have to be the same as the
        /// name of the method being attributed by this attribute.</param>
        public ActionAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// The name of the action.
        /// </summary>
        public string/*?*/ Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// The name of the Start action
        /// </summary>
        public string/*?*/ Start
        {
            get { return start; }
            set { start = value; }
        }

        /// <summary>
        /// The name of the Finish action
        /// </summary>
        public string/*?*/ Finish
        {
            get { return finish; }
            set { finish = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Weight
        {
            get
            {
                return weight;
            }
            set
            {
                weight = value;
            }
        }
    }
}

