//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace NModel.Execution
{
    /// <summary>
    /// Exception thrown on invalid user input to the ModelProgram library
    /// </summary>
    [Serializable]
    public class ModelProgramUserException : Exception
    {
 
       /// <summary>
        /// Constructor of <c>ModelProgramUserException</c>
       /// </summary>
       /// <param name="msg">The error message</param>
       public ModelProgramUserException(string msg) : base(msg)
       {
       }

       /// <summary>
       /// Constructor of <c>ModelProgramUserException</c>
       /// </summary>
       /// <param name="msg">The error message</param>
       /// <param name="e">inner exception</param>
       public ModelProgramUserException(string msg, Exception e)
           : base(msg,e)
       {
       }

       /// <summary>
       /// Empty constructor of <c>ModelProgramUserException</c>
       /// </summary>
       public ModelProgramUserException(): base()
       {
       }

       /// <summary>
       /// Constructor of <c>ModelProgramUserException</c>
       /// </summary>
        protected ModelProgramUserException(System.Runtime.Serialization.SerializationInfo si, System.Runtime.Serialization.StreamingContext sc)
            : base(si,sc)
        {
        }
    }
}
