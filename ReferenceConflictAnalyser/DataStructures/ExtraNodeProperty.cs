using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser.DataStructures
{
    public enum ExtraNodeProperty
    {
        [Description("Source Node Details")]
        SourceNodeDetails,

        [Description("Target Node Details")]
        TargetNodeDetails,

        [Description("Platform Target (Processor Architecture)")]
        ProcessorArchitecture
    }
}
