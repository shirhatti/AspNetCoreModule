﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.PlatformAbstractions;

namespace AspNetCoreModule.FunctionalTests
{
    public class Helpers
    {
        public static string GetApplicationPath(ApplicationType applicationType)
        {
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            string solutionPath = UseLatestAncm.GetSolutionDirectory();
            string applicationPath = string.Empty;
            if (applicationType == ApplicationType.Standalone)
            {
                applicationPath = Path.Combine(solutionPath, "test", "TestSites", "bin", "Debug", "netcoreapp1.0", "win7 - x64", "publish");
            }
            else
            {
                //applicationPath = Path.Combine(solutionPath, "test", "AspNetCoreModule.TestSites", "bin", "Debug", "netcoreapp1.0", "publish");
                applicationPath = Path.Combine(solutionPath, "test", "AspNetCoreModule.TestSites");
            }
            return applicationPath;
        }

        public static string GetConfigContent(ServerType serverType, string iisConfig)
        {
            string content = null;
            if (serverType == ServerType.IISExpress)
            {
                content = File.ReadAllText(iisConfig);
            }
            return content;
        }
    }
}