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

        [Description("Conflicted reference")]
        Conflicted,

        [Description("Conflicted but resolved by means of config file")]
        ConflictResolved
    }
}
