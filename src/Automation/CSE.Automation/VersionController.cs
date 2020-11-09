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
  sealed class SemanticVersionAttribute : System.Attribute
  {
    public string Value { get; }
    public SemanticVersionAttribute(string SemanticVersion)
    {
        this.Value = SemanticVersion;
    }
  }
  sealed class BuildTimestampAttribute : System.Attribute
  {
    public string Value { get; }
    public BuildTimestampAttribute(string BuildTimestamp)
    {
        this.Value = BuildTimestamp;
    }
  }
  public class VersionMetaData
  {
    // Conforms to semver.org
    public string Version { get; }
    public string BuildTs { get; }
    public VersionMetaData()
    {
      this.Version = this.GetType().Assembly.GetCustomAttribute<SemanticVersionAttribute>().Value;
      this.BuildTs = this.GetType().Assembly.GetCustomAttribute<BuildTimestampAttribute>().Value;

      if (String.IsNullOrEmpty(this.Version)) // local build
      {
        this.Version = $"{File.ReadAllLines("version.config")[0]}-alpha";
      }
    }
  }
  internal class VersionController
  {
    [FunctionName("Version")]
    public IActionResult Version([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
    {
      return new JsonResult(new VersionMetaData());
    }
  }
}
