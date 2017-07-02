using ReferenceConflictAnalyser.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser
{
    public class ReferenceAnalyser
    {
        public ReferenceList AnalyzeReferences(ReferenceList list, IEnumerable<BindingRedirectData> bindingRedirects)
        {
            _referenceList = list;

            FindConflicts();
            FindResolvedConflicts(bindingRedirects);
            AddExplanationToLoadingErrors();
            FindProcessorArchitectureMismatch();

            return _referenceList;
        }

        #region private

        private ReferenceList _referenceList { get; set; }

        private void FindConflicts()
        {
            var referencedVersions = new Dictionary<string, Version>();

            foreach (var reference in _referenceList.References)
            {
                if (referencedVersions.ContainsKey(reference.ReferencedAssembly.Name))
                {
                    if (!AreVersionCompatible(referencedVersions[reference.ReferencedAssembly.Name], reference.ReferencedAssembly.Version))
                    {
                        var conflicts = _referenceList.Assemblies.Where(x => x.Name == reference.ReferencedAssembly.Name && x.Category == Category.Normal).ToArray();
                        foreach (var conflict in conflicts)
                            conflict.Category = Category.Conflicted;
                    }
                }
                else
                {
                    referencedVersions.Add(reference.ReferencedAssembly.Name, reference.ReferencedAssembly.Version);
                }
            }
        }

        private void FindResolvedConflicts(IEnumerable<BindingRedirectData> bindingRedirects)
        {
            if (bindingRedirects == null || !bindingRedirects.Any())
                return;

            var conflicts = _referenceList.Assemblies.Where(x => x.Category == Category.Conflicted).ToArray();
            foreach (var conflict in conflicts)
            {
                var bindingRedirect = bindingRedirects.FirstOrDefault(x => x.AssemblyName == conflict.Name);
                if (bindingRedirect == null)
                    continue;

                var mainVersion = new Version(conflict.Version.Major, conflict.Version.Minor);

                if (mainVersion >= bindingRedirect.OldVersionLowerBound
                   && mainVersion <= bindingRedirect.OldVersionUpperBound)
                {
                    conflict.Category = Category.ConflictResolved;
                }
            }
        }

        private bool AreVersionCompatible(Version version1, Version version2)
        {
            //versions are considered compatible if they differ in build or revision only
            return version1.Major == version2.Major
                && version1.Minor == version2.Minor;
        }

        private void AddExplanationToLoadingErrors()
        {
            var failedToLoad = _referenceList.Assemblies.Where(x => x.LoadingError != null);
            foreach (var referencedAssembly in failedToLoad)
            {
                if (referencedAssembly.LoadingError is FileNotFoundException)
                {
                    referencedAssembly.PossibleLoadingErrorCauses.Add("The assembly is missed.");
                }
                else if (referencedAssembly.LoadingError is FileLoadException)
                {
                    referencedAssembly.PossibleLoadingErrorCauses.Add("The assembly file is found but cannot be loaded.");
                }
                else if (referencedAssembly.LoadingError is BadImageFormatException)
                {
                    referencedAssembly.PossibleLoadingErrorCauses.AddRange(new[]
                    {
                        "The assembly was developed with a later version of the .NET Framework then one which is used to load the assembly.",
                        "Attempt to load an unmanaged dynamic link library or executable (such as a Windows system DLL) as if it were a .NET Framework assembly.",
                        "The assembly built as a 32-bit assembly is loaded as a 64-bit assembly, and vice versa."
                    });
                }
            }
        }

        private void FindProcessorArchitectureMismatch()
        {
            var processorArchitecture = _referenceList.Assemblies.First(x => x.Category == Category.EntryPoint).ProcessorArchitecture;
            if (processorArchitecture == ProcessorArchitecture.None)
                return;

            var mismatched = Enumerable.Empty<ReferencedAssembly>();
            switch (processorArchitecture)
            {
                case ProcessorArchitecture.MSIL:
                    mismatched = _referenceList.Assemblies
                        .Where(x => x.ProcessorArchitecture != ProcessorArchitecture.None && x.ProcessorArchitecture != ProcessorArchitecture.MSIL);
                    break;

                case ProcessorArchitecture.Amd64:
                case ProcessorArchitecture.Arm:
                case ProcessorArchitecture.IA64:
                case ProcessorArchitecture.X86:
                    mismatched = _referenceList.Assemblies
                        .Where(x => x.ProcessorArchitecture != ProcessorArchitecture.None && x.ProcessorArchitecture != ProcessorArchitecture.MSIL && x.ProcessorArchitecture != processorArchitecture);
                    break;
            }

            foreach (var referencedAssembly in mismatched)
            {
                referencedAssembly.ProcessorArchitectureMismatch = true;
                referencedAssembly.PossibleLoadingErrorCauses.Add("The platform target (processor architecture) of the assembly differs from the entry point's platform target.");
            }
        }


        #endregion
    }
}
