using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser.DataStructures
{
    public class ReferencedAssembly
    {
        public ReferencedAssembly(AssemblyName assemblyName)
            : this(assemblyName, null)
        {
        }

        public ReferencedAssembly(AssemblyName assemblyName, Exception loadingError)
        {
            AssemblyName = assemblyName;

            Name = assemblyName.Name;
            ProcessorArchitecture = assemblyName.ProcessorArchitecture;
            //PublicKeyToken = Encoding.UTF8.GetString(assemblyName.GetPublicKeyToken()).ToLowerInvariant();
            Version = assemblyName.Version;


            Category = loadingError == null ? Category.Normal : Category.Missed;
            LoadingError = loadingError;
            PossibleLoadingErrorCauses = new List<string>();

            GenerateHashCode();
        }


        public string Name { get; private set; }
        //public string PublicKeyToken { get; private set; }
        public Version Version { get; private set; }
        public AssemblyName AssemblyName { get; private set; }
        public Exception LoadingError { get; private set; }
        public List<string> PossibleLoadingErrorCauses { get; private set; }
        public ProcessorArchitecture ProcessorArchitecture { get; set; }
        public Category Category { get; set; }
        public bool ProcessorArchitectureMismatch { get; set; }

        private int _hashCode;
        
        private void GenerateHashCode()
        {
            //TODO: add token and culture
            _hashCode = new { Name, Version }.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ReferencedAssembly;
            if (other == null)
                return false;

            //TODO: add token and culture check
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) && Version == Version;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

    }
}
