// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace CSE.Automation.Model
{
  internal class VersionMetadata
  {
    public string AssemblyVersion { get; }
    public string AssemblyFileVersion { get; }
    public string ProductVersion { get; }
    public string BuildTimestamp { get; }

    public VersionMetadata(Assembly assembly)
    {
      this.AssemblyVersion = assembly.GetName().Version.ToString();
      var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
      this.AssemblyFileVersion = versionInfo.FileVersion;
      this.ProductVersion = versionInfo.ProductVersion;
      this.BuildTimestamp = this.GetType().Assembly.GetCustomAttribute<BuildTimestampAttribute>().Value;
    }
  }
}
