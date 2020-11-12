// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Automation.Model;
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
  internal class VersionService
  {
    private VersionMetadata versionMetadata;
    public VersionService(VersionMetadata versionMetaData)
    {
      this.versionMetadata = versionMetaData;
    }

    [FunctionName("Version")]
    public IActionResult Version([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] ILogger log)
    {
      log.LogInformation(this.versionMetadata.ProductVersion);
      return new JsonResult(this.versionMetadata);
    }
  }
}
