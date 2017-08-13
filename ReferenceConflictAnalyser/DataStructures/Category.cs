using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser.DataStructures
{
    public enum Category
    {
        [Description("Entry point for analysis")]
        EntryPoint,

        [Description("Normal reference")]
        Normal,

        [Description("Versions conflict")]
        VersionsConflicted,

        [Description("Other conflict")]
        OtherConflict,

        [Description("Versions conflict is resolved by config file")]
        VersionsConflictResolved,

        [Description("Assembly is missed or failed to load")]
        Missed,

        [Description("Detailed information")]
        Comment,

        [Description("Unused assemblies")]
        UnusedAssembly
    }
}
