using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser.DataStructures
{
    public class BindingData
    {
        public BindingData(IEnumerable<BindingRedirectData> bindingRedirects = null, IEnumerable<string> subFolders = null)
        {
            BindingRedirects = bindingRedirects ?? Enumerable.Empty<BindingRedirectData>();
            SubFolders = subFolders ?? Enumerable.Empty<string>(); ;
        }

        public IEnumerable<string> SubFolders { get; private set; }

        public IEnumerable<BindingRedirectData> BindingRedirects { get; private set; }
    }
}
