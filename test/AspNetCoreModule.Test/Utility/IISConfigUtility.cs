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

        public string _serverName = null;
        public string ServerName
        {
            get
            {
                if (_serverName == null)
                {
                    _serverName = "localhost";
                }
                return _serverName;
            }
            set
            {
                _serverName = value;
            }
        }

        public string _appPoolName = null;
        public string AppPoolName
        {
            get
            {
                if (_appPoolName == null)
                {
                    _appPoolName = "DefaultAppPool";
                }
                return _appPoolName;
            }
            set
            {
                _appPoolName = value;
            }
        }

        public string _siteName = null;
        public string SiteName
        {
            get
            {
                return _siteName;
            }
            set
            {
                _siteName = value;
            }
        }

        public int _tcpPort = 8080;
        public int TcpPort
        {
            get
            {
                return _tcpPort;
            }
            set
            {
                _tcpPort = value;
            }
        }

        public IISConfigUtility(ServerType type)
        {
            this.ServerType = type;
            BackupAppHostConfig();
        }
        public void Dispose()
        {
            RestoreAppHostConfig();
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

        public void SetAppPoolSetting(AppPoolSettings attribute, object value)
        {
            SetAppPoolSetting(AppPoolName, attribute, value);
        }

        public void SetAppPoolSetting(string name, AppPoolSettings attribute, object value)
        {
            using (ServerManager serverManager = GetServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();
                ConfigurationSection applicationPoolsSection = config.GetSection("system.applicationHost/applicationPools");
                ConfigurationElementCollection applicationPoolsCollection = applicationPoolsSection.GetCollection();
                ConfigurationElement addElement = FindElement(applicationPoolsCollection, "add", "name", name);
                if (addElement == null) throw new InvalidOperationException("Element not found!");
                var attributeName = attribute.ToString();
                addElement[attributeName] = value;
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
            string tofile = Strings.AppHostConfigPath + ".bak";
            if (File.Exists(fromfile))
            {
                TestUtility.FileCopy(fromfile, tofile, overWrite: false);
            }
        }

        public void RestoreAppHostConfig()
        {
            string fromfile = Strings.AppHostConfigPath + ".bak";
            string tofile = Strings.AppHostConfigPath;
            if (File.Exists(fromfile))
            {
                TestUtility.FileCopy(fromfile, tofile);
            }
        }        
    }
}