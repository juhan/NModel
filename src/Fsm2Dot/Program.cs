//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using NModel.Utilities.Graph;

namespace Fsm2Dot
{
    class Program
    {
        static void Main(string[] args)
        {
            DotWriter.RunWithCommandLineArguments(args);
        }
    }
}
