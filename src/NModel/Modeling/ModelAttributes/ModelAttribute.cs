//using System;

//namespace NModel.Attributes
//{
//    /// <summary>
//    /// The [Model] attribute may be applied to a class to define state variables and actions. The [Model] attribute
//    /// includes the name of the model. If the state of the model consists of fields of more than one class, then
//    /// the attribute (with the same model name) may appear on multiple classes.
//    /// </summary>
//    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
//    public sealed class ModelAttribute : Attribute
//    {
//        string name;

//        /// <summary>
//        /// Constructor of the [Model] attribute. The name of the model defaults to the name of the attributed class.
//        /// </summary>
//        public ModelAttribute()
//        {
//            name = "";
//        }

//        /// <summary>
//        /// Constructor of the [Model] attribute.
//        /// </summary>
//        /// <param name="name">The model name.</param>
//        public ModelAttribute(string name)
//        {
//            this.name = name;
//        }

//        /// <summary>
//        /// The name of the model.
//        /// </summary>
//        public string Name
//        {
//            get
//            {
//                return name;
//            }
//        }
//    }
//}
