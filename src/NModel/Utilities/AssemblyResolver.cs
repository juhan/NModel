//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace NModel.Utilities
{
    internal static class AssemblyResolver
    {
        private static bool hooked = false;
        public static void Hook()
        {
            if (!hooked)
            {
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                hooked = true;
            }
        }

        static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = args.Name.Split(',')[0];

            if (name.StartsWith("GraphLayout"))
            {
                throw new NotImplementedException();
                // TODO: Fix
                //return Assembly.LoadFrom(
                //    Path.Combine(
                //        Path.Combine(
                //            InstallHelper.GetInstallationDirectory(),
                //            "bin", name + ".dll"
                //            )
                //        )
                //    );
            }
            else
                return null;
        }
    }
}
