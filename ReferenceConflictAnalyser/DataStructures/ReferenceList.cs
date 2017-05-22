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

        public bool AddReference(AssemblyName assembly, AssemblyName referencedAssembly)
        {
            var reference = new Reference(assembly, referencedAssembly);

            AddAssembly(referencedAssembly, Category.Normal);

            return _references.Add(reference);
        }

        public void AddError(AssemblyName assembly)
        {
            _errors.Add(assembly);
        }

        public void AddConflict(AssemblyName referencedAssembly)
        {
            var conflicts = _assemblies.Keys.Where(x => x.Name == referencedAssembly.Name).ToArray();
            foreach(var conflict in conflicts)
              _assemblies[conflict] = Category.Conflicted;
        }

        public void MarkConflictAsResolved(ReferencedAssembly referencedAssembly)
        {
            _assemblies[referencedAssembly] = Category.ConflictResolved;
        }


        public HashSet<Reference> References => _references;
        public HashSet<AssemblyName> LoadingErrors => _errors;
        public Dictionary<ReferencedAssembly, Category> Assemblies => _assemblies;

        #region private members

        private readonly HashSet<Reference> _references = new HashSet<Reference>();
        private readonly HashSet<AssemblyName> _errors = new HashSet<AssemblyName>();
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
