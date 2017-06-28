using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser.DataStructures
{
    public class LoadingError
    {
        public LoadingError(Exception e)
        {
            Exception = e;

            TryExplain(e);
        }

        private void TryExplain(Exception e)
        {
            if (e is FileNotFoundException)
            {
                PossibleCauses = new[] 
                {
                    "The assembly is missed."
                };
            }
            else if (e is FileLoadException)
            {
                PossibleCauses = new[] 
                {
                    "The assembly file is found but cannot be loaded.",
                    "Attempt to load a 64-bit assembly by a 32-bit assembly, and vice versa."
                };
            }
            else if (e is BadImageFormatException)
            {
                PossibleCauses = new[]
                {
                    "The assembly was developed with a later version of the .NET Framework then one which is used to load the assembly.",
                    "Attempt to load an unmanaged dynamic link library or executable (such as a Windows system DLL) as if it were a .NET Framework assembly.",
                    "The assembly built as a 32-bit assembly is loaded as a 64-bit assembly, and vice versa."
                };
            }
        }

        public Exception Exception { get; private set; }
        public string[] PossibleCauses { get; private set; }
    }
}
