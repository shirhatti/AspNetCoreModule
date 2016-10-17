using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Web.Administration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using static AspNetCoreModule.Test.Utility.IISServer;

namespace AspNetCoreModule.Test.Utility
{
    public class IISConfigUtility : IDisposable
    {
        public class Strings
        {
            public static string AppHostConfigPath = Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), "system32", "inetsrv", "config", "applicationHost.config");
        }
        
        public enum AppPoolSettings
        {
            enable32BitAppOnWin64,
            none
        }

        public void Dispose()
        {
            RestoreAppHostConfig();
        }

        public IISConfigUtility(ServerType type)
        {
            this.ServerType = type;
            BackupAppHostConfig();
        }
        public ServerType ServerType = ServerType.IIS;
         
        public ServerManager GetServerManager()
        {
            if (ServerType == ServerType.IISExpress)
            {
                return new ServerManager();
            }
            else
            {
                return new ServerManager(
                    false,                         // readOnly 
                    Strings.AppHostConfigPath      // applicationhost.config path
                );
            }
        }
        
        public void SetAppPoolSetting(string appPoolName, AppPoolSettings attribute, object value)
        {
            using (ServerManager serverManager = GetServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();
                ConfigurationSection applicationPoolsSection = config.GetSection("system.applicationHost/applicationPools");
                ConfigurationElementCollection applicationPoolsCollection = applicationPoolsSection.GetCollection();
                ConfigurationElement addElement = FindElement(applicationPoolsCollection, "add", "name", appPoolName);
                if (addElement == null) throw new InvalidOperationException("Element not found!");
                var attributeName = attribute.ToString();
                addElement[attributeName] = value;
                serverManager.CommitChanges();
            }
        }

        public void RecycleAppPool(string appPoolName)
        {
            using (ServerManager serverManager = GetServerManager())
            {
                serverManager.ApplicationPools[appPoolName].Recycle();
            }
        }

        public void CreateSite(string siteName, string siteRootPath, int siteId, int tcpPort, string appPoolName = "DefaultAppPool")
        {
            using (ServerManager serverManager = GetServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();
                ConfigurationSection sitesSection = config.GetSection("system.applicationHost/sites");
                ConfigurationElementCollection sitesCollection = sitesSection.GetCollection();
                ConfigurationElement siteElement = sitesCollection.CreateElement("site");
                siteElement["id"] = siteId;
                siteElement["name"] = siteName;
                ConfigurationElementCollection bindingsCollection = siteElement.GetCollection("bindings");

                ConfigurationElement bindingElement = bindingsCollection.CreateElement("binding");
                bindingElement["protocol"] = @"http";
                bindingElement["bindingInformation"] = "*:" + tcpPort + ":";
                bindingsCollection.Add(bindingElement);

                ConfigurationElementCollection siteCollection = siteElement.GetCollection();
                ConfigurationElement applicationElement = siteCollection.CreateElement("application");
                applicationElement["path"] = @"/";
                applicationElement["applicationPool"] = appPoolName;

                ConfigurationElementCollection applicationCollection = applicationElement.GetCollection();
                ConfigurationElement virtualDirectoryElement = applicationCollection.CreateElement("virtualDirectory");
                virtualDirectoryElement["path"] = @"/";
                virtualDirectoryElement["physicalPath"] = siteRootPath;
                applicationCollection.Add(virtualDirectoryElement);
                siteCollection.Add(applicationElement);
                sitesCollection.Add(siteElement);

                serverManager.CommitChanges();
            }
        }

        private static ConfigurationElement FindElement(ConfigurationElementCollection collection, string elementTagName, params string[] keyValues)
        {
            foreach (ConfigurationElement element in collection)
            {
                if (String.Equals(element.ElementTagName, elementTagName, StringComparison.OrdinalIgnoreCase))
                {
                    bool matches = true;

                    for (int i = 0; i < keyValues.Length; i += 2)
                    {
                        object o = element.GetAttributeValue(keyValues[i]);
                        string value = null;
                        if (o != null)
                        {
                            value = o.ToString();
                        }

                        if (!String.Equals(value, keyValues[i + 1], StringComparison.OrdinalIgnoreCase))
                        {
                            matches = false;
                            break;
                        }
                    }
                    if (matches)
                    {
                        return element;
                    }
                }
            }
            return null;
        }

        public void BackupAppHostConfig()
        {
            string fromfile = Strings.AppHostConfigPath;
            string tofile = Strings.AppHostConfigPath + ".ancmtest.bak";
            if (File.Exists(fromfile))
            {
                TestUtility.FileCopy(fromfile, tofile, overWrite: false);
            }
        }

        public void RestoreAppHostConfig()
        {
            string fromfile = Strings.AppHostConfigPath + ".ancmtest.bak";
            string tofile = Strings.AppHostConfigPath;
            if (File.Exists(fromfile))
            {
                TestUtility.FileCopy(fromfile, tofile);
            }
        }        
    }
}