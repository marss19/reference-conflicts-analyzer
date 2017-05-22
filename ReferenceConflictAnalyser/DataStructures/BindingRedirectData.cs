using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser.DataStructures
{
    public class BindingRedirectData
    {
        public string AssemblyName { get; set; }
        public string PublicKeyToken { get; set; }
        public Version OldVersionLowerBound { get; set; }
        public Version OldVersionUpperBound { get; set; }
        public Version NewVersion { get; set; }
    }
}
