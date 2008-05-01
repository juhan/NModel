//using System;

//namespace NModel.Attributes
//{
//    /// <summary>
//    /// Specifies the name of a Boolean-valued method that must return true in order for attributed action method 
//    /// to be explored in a given state. The method must be defined in the current class but may be 
//    /// static or instance-based.
//    /// </summary>
//    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
//    public sealed class EnablingConditionAttribute : Attribute
//    {
//        string methodName;
//        string requirementString;

//        /// <summary>
//        /// Constructor of [EnablingCondition] attribute.
//        /// </summary>
//        /// <param name="methodName">A Boolean-valued static or instance-based method
//        /// that defines an enabling condition for the attributed action method.</param>
//        public EnablingConditionAttribute(string methodName)
//            //^ requires methodName.Length > 0;
//        {
//            if (string.IsNullOrEmpty(methodName))
//                throw new ArgumentNullException("methodName");

//            this.methodName = methodName;
//            this.requirementString = "";
//        }

//        /// <summary>
//        /// Constructor of [EnablingCondition] attribute.
//        /// </summary>
//        /// <param name="methodName">A Boolean-valued static or instance-based method
//        /// that defines an enabling condition for the attributed action method.</param>
//        /// <param name="requirement">A string that provides descriptive information
//        /// linking this enabling condition to external requirement documents. This string
//        /// will be printed in error contexts, for example, when conformance failures occur.</param>
//        public EnablingConditionAttribute(string methodName, string requirement)
//        //^ requires methodName.Length > 0;
//        {
//            if (string.IsNullOrEmpty(methodName))
//                throw new ArgumentNullException("methodName");

//            this.methodName = methodName;
//            this.requirementString = requirement;
//        }

//        /// <summary>
//        /// The string name of the method given by this [EnablingCondition] attribute.
//        /// </summary>
//        public string MethodName
//        {
//            get
//            {
//                return this.methodName;
//            }
//        }

//        /// <summary>
//        /// A string that provides descriptive information
//        /// linking this enabling condition to external requirement documents. This string
//        /// will be printed in error contexts, for example, when conformance failures occur.
//        /// </summary>
//        public string RequirementString
//        {
//            get
//            {
//                return this.requirementString;
//            }
//        }


//    }
//}
