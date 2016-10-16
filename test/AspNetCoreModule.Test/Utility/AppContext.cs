using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Xml;
using System.Management;

namespace AspNetCoreModule.Test.Utility
{
    public class AppContext : IDisposable
    {
        public AppContext(string name, string physicalPath, string url = null)
        {
            _name = name;
            string temp = physicalPath;
            if (physicalPath.Contains("%"))
            {
                temp = System.Environment.ExpandEnvironmentVariables(physicalPath);
            }
            _physicalPath = temp;

            if (url != null)
            {
                _url = url;
            }
            else
            {
                _url = "/" + name;
            }

            BackupFile("web.config");
        }

        public void Dispose()
        {
            DeleteFile("app_offline.htm");
            RestoreFile("web.config");
        }

        private string _name = null;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private string _physicalPath = null;
        public string PhysicalPath
        {
            get
            {
                return _physicalPath;
            }
            set
            {
                _physicalPath = value;
            }
        }

        private string _url = null;
        public string URL
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
            }
        }

        
        public string GetProcessFileName()
        {
            string filePath = Path.Combine(_physicalPath, "web.config");
            string result = null;

            // read web.config
            string fileContent = TestUtility.FileReadAllText(filePath);

            // get the value of processPath attribute of aspNetCore element
            if (fileContent != null)
            {
                result = TestUtility.XmlParser(fileContent, "aspNetCore", "processPath", null);
            }

            // split fileName from full path
            result = Path.GetFileName(result);

            // append .exe if it wasn't used
            if (!result.Contains(".exe"))
            {
                result = result + ".exe";
            }
            return result;
        }

        public void BackupFile(string from)
        {
            string fromfile = Path.Combine(_physicalPath, from);
            string tofile = Path.Combine(_physicalPath, fromfile + ".bak");
            TestUtility.FileCopy(fromfile, tofile, overWrite: false);
        }

        public void RestoreFile(string from)
        {
            string fromfile = Path.Combine(_physicalPath, from + ".bak");
            string tofile = Path.Combine(_physicalPath, from);
            TestUtility.FileCopy(fromfile, tofile);
        }

        public void MoveFile(string from, string to)
        {
            string fromfile = Path.Combine(_physicalPath, from);
            string tofile = Path.Combine(_physicalPath, to);
            TestUtility.FileMove(fromfile, tofile);
        }

        public void DeleteFile(string file = "app_offline.htm")
        {
            string filePath = Path.Combine(_physicalPath, file);
            TestUtility.DeleteFile(filePath);
        }

        public void CreateFile(string[] content, string file = "app_offline.htm")
        {
            string filePath = Path.Combine(_physicalPath, file);
            TestUtility.CreateFile(filePath, content);
        }
    }
}