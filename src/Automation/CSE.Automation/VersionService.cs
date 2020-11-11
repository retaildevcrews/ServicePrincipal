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

namespace CSE.Automation
{
  sealed class BuildTimestampAttribute : System.Attribute
  {
    public string Value { get; }
    public BuildTimestampAttribute(string BuildTimestamp)
    {
        this.Value = BuildTimestamp;
    }
  }
  internal class VersionMetadata
  {
    public string AssemblyVersion { get; }
    public string AssemblyFileVersion { get; }
    public string ProductVersion { get; }
    public string BuildTs { get; }

    public VersionMetadata(Assembly assembly)
    {
      this.AssemblyVersion = assembly.GetName().Version.ToString();
      var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
      this.AssemblyFileVersion = versionInfo.FileVersion;
      this.ProductVersion = versionInfo.ProductVersion;
      this.BuildTs = this.GetType().Assembly.GetCustomAttribute<BuildTimestampAttribute>().Value;
    }
  }
  internal class VersionService
  {
    VersionMetadata _versionMetadata;
    public VersionService(VersionMetadata versionMetaData)
    {
      this._versionMetadata = versionMetaData;
    }
    [FunctionName("Version")]
    public IActionResult Version([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
    {
      return new JsonResult(_versionMetadata);
    }
  }
}
