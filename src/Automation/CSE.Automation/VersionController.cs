// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace CSE.Automation
{
  [System.AttributeUsage(System.AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
  sealed class VersionMajorMinorAttribute : System.Attribute
  {
    public string Value { get; }
    public VersionMajorMinorAttribute(string VersionMajorMinor)
    {
        this.Value = VersionMajorMinor;
    }
  }
  sealed class VersionRunIdAttribute : System.Attribute
  {
    public string Value { get; }
    public VersionRunIdAttribute(string VersionRunId)
    {
        this.Value = VersionRunId;
    }
  }
  sealed class VersionGitHashAttribute : System.Attribute
  {
    public string Value { get; }
    public VersionGitHashAttribute(string VersionGitHash)
    {
        this.Value = VersionGitHash;
    }
  }
  sealed class VersionQualityTagAttribute : System.Attribute
  {
    public string Value { get; }
    public VersionQualityTagAttribute(string VersionQualityTag)
    {
        this.Value = VersionQualityTag;
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
      string majorMinor = this.GetType().Assembly.GetCustomAttribute<VersionMajorMinorAttribute>().Value;

      // These properties get injected by Dockerfile
      string runId = this.GetType().Assembly.GetCustomAttribute<VersionRunIdAttribute>().Value;
      string gitHash = this.GetType().Assembly.GetCustomAttribute<VersionGitHashAttribute>().Value;
      string qualityTag = this.GetType().Assembly.GetCustomAttribute<VersionQualityTagAttribute>().Value;

      this.BuildTs = this.GetType().Assembly.GetCustomAttribute<BuildTimestampAttribute>().Value;

      if (String.IsNullOrEmpty(runId))
      {
        // version if solution not being run by docker (e.g. local development)
        this.Version = $"{majorMinor}-alpha";
      }
      else
      {
        // version if being run by docker (e.g. production)
        this.Version = $"{majorMinor}.{runId}-{qualityTag}+{gitHash}";
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
