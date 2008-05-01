//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using NModel.Algorithms;
using NModel.Execution;

namespace NModel.Tools
{
    /// <summary>
    /// Main program for the OfflineTestGen Utility.
    /// </summary>
    static class OTG
    {
        static int Main(string[] args)
        {
            try
            {
                OfflineTestGenerator.RunWithCommandLineArguments(args);
                return 0;
            }
            catch (ModelProgramUserException e)
            {
                // Redirect the exception message to the console and quit.
                Console.Error.WriteLine(e.Message);
                return -1;
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                if (e.InnerException != null && e.InnerException is ModelProgramUserException)
                {
                    // Redirect the exception message to the console and quit.
                    Console.Error.WriteLine(e.InnerException.Message);
                    return -1;
                }
                else
                {
                    Console.Error.WriteLine("Unexpected error occurred.");
                    throw;
                }
            }
            catch
            {
                Console.Error.WriteLine("Unexpected error occurred.");
                throw;
            }
        }
    }
}
