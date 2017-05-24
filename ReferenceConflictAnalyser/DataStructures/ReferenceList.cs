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
        public void AddEntryPoint(AssemblyName assembly)
        {
            AddAssembly(assembly, Category.EntryPoint);
        }

        public bool AddReference(AssemblyName assembly, AssemblyName referencedAssembly, Category category)
        {
            var reference = new Reference(assembly, referencedAssembly);

            AddAssembly(referencedAssembly, category);

            return _references.Add(reference);
        }

        public void AddConflict(AssemblyName referencedAssembly)
        {
            var conflicts = _assemblies.Where(x => x.Key.Name == referencedAssembly.Name && x.Value == Category.Normal).ToArray();
            foreach(var conflict in conflicts)
                _assemblies[conflict.Key] = Category.Conflicted;
        }

        public void MarkConflictAsResolved(ReferencedAssembly referencedAssembly)
        {
            _assemblies[referencedAssembly] = Category.ConflictResolved;
        }


        public HashSet<Reference> References => _references;
        public Dictionary<ReferencedAssembly, Category> Assemblies => _assemblies;

        #region private members

        private readonly HashSet<Reference> _references = new HashSet<Reference>();
        private readonly Dictionary<ReferencedAssembly, Category> _assemblies = new Dictionary<ReferencedAssembly, Category>();

        private void AddAssembly(AssemblyName assemblyName, Category category)
        {
            var data = new ReferencedAssembly(assemblyName);
            if (!_assemblies.ContainsKey(data))
                _assemblies.Add(data, category);
        }


        #endregion
    }
}
