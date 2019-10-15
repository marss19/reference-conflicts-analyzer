using ReferenceConflictAnalyser.DataStructures;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ReferenceConflictAnalyser
{
    public class ConfigurationHelper
    {
        public static bool TrySuggestConfigFile(string entryAssemblyFilePath, out string configFilePath)
        {
            configFilePath = null;

            if (entryAssemblyFilePath == null)
                return false;

            var fileExtension = Path.GetExtension(entryAssemblyFilePath).ToLower();
            switch (fileExtension)
            {
                case ".exe":
                    {
                        var temp = entryAssemblyFilePath + ".config";
                        if (File.Exists(temp))
                            configFilePath = temp;
                    }
                    break;

                case ".dll":
                    {
                        var directory = Path.GetDirectoryName(entryAssemblyFilePath);
                        var re = new Regex(@"\\bin\\?$", RegexOptions.IgnoreCase);
                        directory = re.Replace(directory, "");
                        var temp = Path.Combine(directory, "web.config");
                        if (File.Exists(temp))
                            configFilePath = temp;
                    }
                    break;
            }

            return configFilePath != null;
        }

        public static BindingData GetBindingRedirects(string configFilePath)
        {
            if (string.IsNullOrEmpty(configFilePath) || !File.Exists(configFilePath))
                return new BindingData();

            var redirects = new List<BindingRedirectData>();
            string[] subFolders = null;
            try
            {
                var doc = new XmlDocument();
                doc.Load(configFilePath);

                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("bind", "urn:schemas-microsoft-com:asm.v1");

                var probingNode = doc.SelectSingleNode("//bind:probing", nsmgr);
                if (probingNode != null)
                {
                    var str = probingNode.Attributes["privatePath"]?.Value;
                    if (!string.IsNullOrEmpty(str))
                        subFolders = str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries); 
                }

                var nodes = doc.SelectNodes("//bind:dependentAssembly", nsmgr);
                foreach(XmlElement node in nodes)
                {
                    var assemblyIdentityElem = node.GetElementsByTagName("assemblyIdentity")[0];
                    var bindingRedirectElem = node.GetElementsByTagName("bindingRedirect")[0];
                    if (assemblyIdentityElem == null || bindingRedirectElem == null)
                        continue;

                    var data = new BindingRedirectData()
                    {
                        AssemblyName = assemblyIdentityElem.Attributes["name"].Value,
                        PublicKeyToken = assemblyIdentityElem.Attributes["publicKeyToken"]?.Value,
                        NewVersion = new Version(bindingRedirectElem.Attributes["newVersion"].Value)
                    };

                    var oldVersions = bindingRedirectElem.Attributes["oldVersion"].Value.Split('-');
                    data.OldVersionLowerBound = GetMainVersion(oldVersions[0]);
                    data.OldVersionUpperBound = oldVersions.Count() > 1 ? GetMainVersion(oldVersions[1]) : data.OldVersionLowerBound;

                    redirects.Add(data);
                }
            }
            catch (ConfigurationErrorsException)
            {
            }
            return new BindingData(redirects, subFolders);
        }

        private static Version GetMainVersion(string versionStr)
        {
            var temp = new Version(versionStr);
            return new Version(temp.Major, temp.Minor);
        }


    }
}
