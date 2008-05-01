//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using NModel.Visualization;
using NModel.Execution;

namespace NModel.Tools
{
    /// <summary>
    /// Main program for the Model Program Viewer (mpv.exe) Utility.
    /// </summary>
    static class MPV
    {
        /// <summary>
        /// Model Program Viewer Utility
        /// </summary>
        /// <param name="args">models to be composed in the viewer and viewer settings</param>
        /// <returns>0 if application returns normally, returns -1 otherwise</returns>
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                CommandLineViewer.RunWithCommandLineArguments(args);
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
                Console.Error.WriteLine("Unexpected error occurred." );
                throw;
            }
        }
    }
}
