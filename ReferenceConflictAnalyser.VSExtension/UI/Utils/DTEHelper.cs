using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser.VSExtension.UI.Utils
{
    public class DTEHelper
    {
        public static DTE CurrentDTE { get; set; }

        public static void OpenFile(string filePath)
        {
            CurrentDTE.ItemOperations.OpenFile(filePath);
        }
    }
}
