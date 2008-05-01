//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

//Tasks used by the build process
namespace NModel.Tasks
{
    /// <summary>
    /// Task used by msbuild to update the assembly version number to the build number.
    /// </summary>
  public sealed class CreateAssemblyVersion : Task
  {
    private ITaskItem versionFile;
      /// <summary>
      /// The task item is the version file
      /// </summary>
    [Required]
    public ITaskItem VersionFile
    {
      get { return this.versionFile; }
      set { this.versionFile = value; }
    }

    private string assemblyVersion;
      private Version version;
      /// <summary>
      /// The assembly version number
      /// </summary>
    [Output]
    public string AssemblyVersion
    {
      get { return this.assemblyVersion; }
        set { ComputeVersion(value); }
    }

      private void ComputeVersion(string val)
      {
          //"0" means that no value is provided
          if (val == "0")
          {
              DateTime today = System.DateTime.Today;
              this.version = new Version(1, 0, ((((today.Year - 2006) * 100) + today.Month) * 100) + today.Day, 0);
          }
          else
          {
              this.version = new Version(val);
          }
          this.assemblyVersion = this.version.ToString();
      }

      /// <summary>
      /// Execute the task
      /// </summary>
    public override bool Execute()
    {
      this.Log.LogMessage("creating version file at {0}", this.VersionFile);
      this.Log.LogMessage("AssemblyVersion: {0}", this.AssemblyVersion);
      this.Log.LogMessage("AssemblyFileVersion: {0}", this.AssemblyVersion);

      // force readonlyness
      FileInfo fi = new FileInfo(this.VersionFile.GetMetadata("FullPath"));
      if (fi.Exists && fi.IsReadOnly)
        fi.IsReadOnly = false;

      // write new version number
      using(StreamWriter writer = new StreamWriter(fi.FullName))
      {
        writer.WriteLine(
@"using System.Reflection;

[assembly: AssemblyCompany(""Microsoft"")]
[assembly: AssemblyProduct(""NModel"")]
[assembly: AssemblyCopyright(""Copyright © Microsoft 2007"")]
[assembly: AssemblyVersion(""{0}"")]
[assembly: AssemblyFileVersion(""{1}"")]",
          this.assemblyVersion,
          this.assemblyVersion
        );

        return true;
      }
    }
  }
}
