using Microsoft.Web.Administration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;

namespace AspNetCoreModule.Test.Utility
{
    public class UseIISServer : IDisposable
    {
        public static UseIISServer IIS;

        public UseIISServer()
        {
            BackupAppHostConfig();
            Initialize();
            IIS = this;       
        }
        public void Dispose()
        {
            RestoreAppHostConfig();
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

        public void Initialize()
        {
            TestUtility.LogMessage("Instanciate applications with accessing Name property");
            TestUtility.LogMessage(Websocketecho.Name);
            TestUtility.LogMessage(WebsocketechoChild.Name);
            TestUtility.LogMessage(Parent.Name);
        }

        private AppContext _websocketecho = null;
        public AppContext Websocketecho
        {
            get
            {
                if (_websocketecho == null)
                {
                    _websocketecho = new AppContext("websocketecho", @"%AspNetCoreModuleTest%\AspnetCoreApp_WebSocketEcho", "/websocketecho");
                }
                return _websocketecho;
            }
        }

        private AppContext _websocketechoChild = null;
        public AppContext WebsocketechoChild
        {
            get
            {
                if (_websocketechoChild == null)
                {
                    _websocketechoChild = new AppContext("websocketechoChild", @"%AspNetCoreModuleTest%\AspnetCoreApp_WebSocketEcho", "/parent/websocketechoChild");
                }
                return _websocketechoChild;
            }
        }

        private AppContext _parent = null;
        public AppContext Parent
        {
            get
            {
                if (_parent == null)
                {
                    _parent = new AppContext("parent", @"%AspNetCoreModuleTest%\parent", "/parent");
                }
                return _parent;
            }
        }
        public ArrayList Applications = new ArrayList();

        public enum AppPoolSetting
        {
            enable32BitAppOnWin64,
            none
        }

        public enum AppPoolSettings
        {
            enable32BitAppOnWin64
        }

        public void SetAppPoolSetting(string name, AppPoolSettings attribute, object value)
        {
            using (ServerManager serverManager = new ServerManager())
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
            string fromfile = Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), "system32", "inetsrv", "config", "applicationHost.config");
            string tofile = Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), "system32", "inetsrv", "config", "applicationHost.config.bak");
            if (File.Exists(fromfile))
            {
                TestUtility.FileCopy(fromfile, tofile, overWrite: false);
            }
        }

        public void RestoreAppHostConfig()
        {
            string fromfile = Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), "system32", "inetsrv", "config", "applicationHost.config.bak");
            string tofile = Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), "system32", "inetsrv", "config", "applicationHost.config");
            if (File.Exists(fromfile))
            {
                TestUtility.FileCopy(fromfile, tofile);
            }
        }        
    }
}