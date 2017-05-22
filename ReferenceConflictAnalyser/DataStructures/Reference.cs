using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser.DataStructures
{
    public class Reference
    {
        public Reference(AssemblyName assembly, AssemblyName referencedAssembly)
        {
            Assembly = assembly;
            ReferencedAssembly = referencedAssembly;

            _stringPresentation = string.Concat(assembly.FullName, " -> ", referencedAssembly.FullName);
        }

        public AssemblyName Assembly { get; private set; }
        public AssemblyName ReferencedAssembly { get; private set; }

        private string _stringPresentation;

        public override string ToString()
        {
            return _stringPresentation;
        }

        public override bool Equals(object obj)
        {
            return _stringPresentation.Equals(obj.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return _stringPresentation.GetHashCode();
        }
    }
}
