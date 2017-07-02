using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser.DataStructures
{
    public class ReferenceList 
    {
        public void AddEntryPoint(ReferencedAssembly referencedAssembly)
        {
            _assemblies.Add(referencedAssembly);
        }

        public bool AddReference(ReferencedAssembly assembly, ReferencedAssembly referencedAssembly)
        {
            _assemblies.Add(referencedAssembly);

            var reference = new Reference(assembly, referencedAssembly);
            return _references.Add(reference);
        }

        public HashSet<Reference> References => _references;
        public HashSet<ReferencedAssembly> Assemblies => _assemblies;

        #region private members

        private readonly HashSet<Reference> _references = new HashSet<Reference>();
        private readonly HashSet<ReferencedAssembly> _assemblies = new HashSet<ReferencedAssembly>();

        #endregion
    }
}
