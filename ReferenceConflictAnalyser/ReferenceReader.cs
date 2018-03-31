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
        public ReferenceList Read(string entryAssemblyFilePath, IEnumerable<string> subDirectories, bool skipSystemAssemblies)
        {
            if (!File.Exists(entryAssemblyFilePath))
                throw new ArgumentException(string.Format("File does not exist: {0}", entryAssemblyFilePath));

            _skipSystemAssemblies = skipSystemAssemblies;
            _result = new ReferenceList();
            _cache = new Dictionary<string, ReferencedAssembly>();

            var workingDirectory = Path.GetDirectoryName(entryAssemblyFilePath);
            _searchDirectories = new List<string>() { workingDirectory };
            try
            {
                if (subDirectories != null)
                    _searchDirectories.AddRange(subDirectories
                                                    .Select(x => Path.Combine(workingDirectory, x))
                                                    .Where(x => Directory.Exists(x))); 
            }
            catch (Exception e)
            {
                throw new Exception($"\"runtime/assemblyBinding/probing\" element in the config file contains invalid paths. {e.Message}");          
            }
 
            AssemblyName[] entryPointReferences;
            var entryPoint = LoadEntryPoint(entryAssemblyFilePath, out entryPointReferences);
            _result.AddEntryPoint(entryPoint);

            ReadReferencesRecursively(entryPoint, entryPointReferences);
            ReadUnusedAssemblies();

            return _result;
        }

        #region private members

        private bool _skipSystemAssemblies;
        private ReferenceList _result;
        private List<string> _searchDirectories;
        private Dictionary<string, ReferencedAssembly> _cache;

        private void ReadReferencesRecursively(ReferencedAssembly assembly, AssemblyName[] references)
        {
            foreach (var reference in references)
            {
                if (_skipSystemAssemblies
                    &&
                        (reference.Name == "mscorlib"
                        || reference.Name == "System"
                        || reference.Name.StartsWith("System."))
                    )
                    continue;

                if (_cache.ContainsKey(reference.FullName))
                {
                    _result.AddReference(assembly, _cache[reference.FullName]);
                    continue;
                }

                AssemblyName[] referencedAssemblyReferences;
                var referencedAssembly = LoadReferencedAssembly(reference, out referencedAssemblyReferences);
                if (referencedAssembly.Category != Category.Missed)
                {
                    var isNewReference = _result.AddReference(assembly, referencedAssembly);
                    if (isNewReference)
                        ReadReferencesRecursively(referencedAssembly, referencedAssemblyReferences);
                }
                else
                {
                    _result.AddReference(assembly, referencedAssembly);
                }
            }

        }

        private ReferencedAssembly LoadEntryPoint(string filePath, out AssemblyName[] assemblyReferences)
        {
            assemblyReferences = Assembly.ReflectionOnlyLoadFrom(filePath).GetReferencedAssemblies();
            var assembly = AssemblyName.GetAssemblyName(filePath);

            var referencedAssembly = new ReferencedAssembly(assembly)
            {
                Category = Category.EntryPoint,
            };

            _cache.Add(assembly.FullName, referencedAssembly);
            return referencedAssembly;
        }


        private ReferencedAssembly LoadReferencedAssembly(AssemblyName reference, out AssemblyName[] referencedAssemblyReferences)
        {
            referencedAssemblyReferences = null;
            ReferencedAssembly referencedAssembly;
            try
            {
                string file = null;
                foreach(var dir in _searchDirectories)
                {
                    var files = Directory.GetFiles(dir, reference.Name + ".???", SearchOption.TopDirectoryOnly);
                    file = files.FirstOrDefault(x => x.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
                    if (file != null)
                        break;
                }

                if (file != null)
                {
                    referencedAssemblyReferences = Assembly.ReflectionOnlyLoadFrom(file).GetReferencedAssemblies();
                    
                    //read additional information from file as the assembly name loaded by the ReflectionOnly load is not complete.
                    var temp = AssemblyName.GetAssemblyName(file);
                    if (temp.FullName == reference.FullName)
                        reference.ProcessorArchitecture = temp.ProcessorArchitecture;
                }
                else
                {
                    referencedAssemblyReferences = Assembly.ReflectionOnlyLoad(reference.FullName).GetReferencedAssemblies();
                }
                referencedAssembly = new ReferencedAssembly(reference);
            }
            catch (Exception e)
            {
                referencedAssembly = new ReferencedAssembly(reference, e);
            }

            if (!_cache.ContainsKey(reference.FullName))
                _cache.Add(reference.FullName, referencedAssembly);

            return referencedAssembly;
        }

        private void ReadUnusedAssemblies()
        {
            var loadedAssemblies = _result.Assemblies.Select(x => x.Name);

            var allFiles = new List<string>();
            foreach(var dir in _searchDirectories)
            {
                allFiles.AddRange(Directory
                    .GetFiles(dir, "*.exe", SearchOption.TopDirectoryOnly)
                    .Concat(Directory
                          .GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly)));
            }

            foreach(var filePath in allFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);

                if (!loadedAssemblies.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        var assemblyName = AssemblyName.GetAssemblyName(filePath);
                        _result.Assemblies.Add(new ReferencedAssembly(assemblyName)
                        {
                            Category = Category.UnusedAssembly
                        });
                    }
                    catch(Exception e)
                    {

                    }
                }
            }          
        }

        #endregion
    }
}
