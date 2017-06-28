using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using ReferenceConflictAnalyser.DataStructures;
using System.Configuration;

namespace ReferenceConflictAnalyser
{

    public class ReferenceReader
    {
        public ReferenceList Read(string entryAssemblyFilePath, string configFilePath, bool skipSystemAssemblies = true)
        {
            if (!File.Exists(entryAssemblyFilePath))
                throw new ArgumentException(string.Format("File does not exist: {0}", entryAssemblyFilePath));

            _skipSystemAssemblies = skipSystemAssemblies;
            _result = new ReferenceList();
            _cache = new Dictionary<string, Assembly>();

            _workingDirectory = Path.GetDirectoryName(entryAssemblyFilePath);

            var assembly = Assembly.ReflectionOnlyLoadFrom(entryAssemblyFilePath);
            _result.AddEntryPoint(assembly.GetName());
            ReadReferences(assembly);
            FindConflicts();
            FindResolvedConflicts(configFilePath);

            return _result;
        }

        #region private members

        private bool _skipSystemAssemblies;
        private ReferenceList _result;
        private string _workingDirectory;
        private Dictionary<string, Assembly> _cache;

        private void ReadReferences(Assembly assembly)
        {
            var references = assembly.GetReferencedAssemblies();
            foreach (var reference in references)
            {
                if (_skipSystemAssemblies
                    &&
                        (reference.Name == "mscorlib"
                        || reference.Name == "System"
                        || reference.Name.StartsWith("System."))
                    )
                    continue;

                LoadingError error;
                var referencedAssembly = LoadReferencedAssembly(reference, out error);
                if (referencedAssembly != null)
                {
                    var isNewReference = _result.AddReference(assembly.GetName(), reference, Category.Normal);
                    if (isNewReference)
                        ReadReferences(referencedAssembly);
                }
                else
                {
                    _result.AddReference(assembly.GetName(), reference, Category.Missed, error);
                }
            }

        }

        private Assembly LoadReferencedAssembly(AssemblyName reference, out LoadingError error)
        {
            error = null;

            if (_cache.ContainsKey(reference.FullName))
                return _cache[reference.FullName];

            Assembly assembly = null;
            try
            {
                //assembly = Assembly.ReflectionOnlyLoad(reference.FullName);
                var files = Directory.GetFiles(_workingDirectory, reference.Name + ".???", SearchOption.TopDirectoryOnly);
                var file = files.FirstOrDefault(x => x.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
                if (file != null)
                {
                    assembly = Assembly.ReflectionOnlyLoadFrom(file);
                }
                else
                {
                    assembly = Assembly.ReflectionOnlyLoad(reference.FullName);
                }
            }
            catch (Exception e)
            {
                error = new LoadingError(e);
            }

            _cache.Add(reference.FullName, assembly);
            return assembly;
        }


        private void FindConflicts()
        {
            var referencedVersions = new Dictionary<string, Version>();
      
            foreach (var reference in _result.References)
            {
                if (referencedVersions.ContainsKey(reference.ReferencedAssembly.Name))
                {
                    if (!AreVersionCompatible(referencedVersions[reference.ReferencedAssembly.Name], reference.ReferencedAssembly.Version))
                    {
                        _result.AddConflict(reference.ReferencedAssembly);
                    }
                }
                else
                {
                    referencedVersions.Add(reference.ReferencedAssembly.Name, reference.ReferencedAssembly.Version);
                }
            }
        }

        private void FindResolvedConflicts(string configFilePath)
        {
            if (string.IsNullOrEmpty(configFilePath))
                return;

            if (!File.Exists(configFilePath))
                return;

            var bindingRedirects = ConfigurationHelper.GetBindingRedirects(configFilePath);
            if (!bindingRedirects.Any())
                return;

            var conflicts = _result.Assemblies.Where(x => x.Value == Category.Conflicted).Select(x=>x.Key).ToArray();
            foreach (var conflict in conflicts)
            {
                var bindingRedirect = bindingRedirects.FirstOrDefault(x => x.AssemblyName == conflict.Name);
                if (bindingRedirect == null)
                    continue;

                var mainVersion = new Version(conflict.Version.Major, conflict.Version.Minor);

                if (mainVersion >= bindingRedirect.OldVersionLowerBound
                   && mainVersion <= bindingRedirect.OldVersionUpperBound)
                {
                    _result.MarkConflictAsResolved(conflict);
                }
            }
        }

        private bool AreVersionCompatible(Version version1, Version version2)
        {
            //versions are considered compatible if they differ in build or revision only

            return version1.Major == version2.Major
                && version1.Minor == version2.Minor;
        }

        #endregion
    }
}
