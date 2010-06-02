using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Reflection;

using NModel.Utilities;

namespace NModel.Tools.GML
{
    /// <summary>
    /// Represents a commandline utility that starts up GML.
    /// </summary>
    public static class GML_CommandLine
    {
        /// <summary>
        /// Provides programmatic access to the commandline utility 'GML.exe'.
        /// </summary>
        /// <param name="args">GraphMl file or folder with several GraphMl files, path to folder where to save the generated class files</param>
        /// <remarks>The settings are displayed when 'GML.exe /?' is executed from the command line without arguments.</remarks>
        public static void RunWithCommandLineArguments(params string[] args)
        {
            ProgramSettings settings = new ProgramSettings();
            if (!Parser.ParseArgumentsWithUsage(args, settings))
            {
                return;
            }
            GraphmlCodeGen graphml = new GraphmlCodeGen(settings.GraphmlFileName, settings.InDirName, settings.OutDirName);
            graphml.GenerateClasses();
        }
    }

    internal sealed class ProgramSettings
    {
        public ProgramSettings()
        {
            GraphmlFileName = null;
            InDirName = null;
            OutDirName = null;
        }

        [Argument(ArgumentType.AtMostOnce, ShortName = "f", HelpText = "The name of the input Graphml file")]
        public string GraphmlFileName;

        [Argument(ArgumentType.AtMostOnce, ShortName = "id", HelpText = "The name of the directory where input GraphML files are")]
        public string InDirName;

        [Argument(ArgumentType.AtMostOnce, ShortName = "od", HelpText = "The name of the directory where output class files should be saved")]
        public string OutDirName;
    }
}
