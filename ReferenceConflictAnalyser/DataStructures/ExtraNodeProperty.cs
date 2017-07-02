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

        [Description("Loading Error")]
        LoadingErrorMessage,

        [Description("Error Type")]
        LoadingErrorType,

        [Description("Possible Failure Cause")]
        LoadingErrorPossibleCause,

        [Description("Possible Failure Cause #1")]
        LoadingErrorPossibleCause1,

        [Description("Possible Failure Cause #2")]
        LoadingErrorPossibleCause2,

        [Description("Possible Failure Cause #3")]
        LoadingErrorPossibleCause3,

        [Description("Possible Failure Cause #4")]
        LoadingErrorPossibleCause4,

        [Description("Platform Target (Processor Architecture)")]
        ProcessorArchitecture,

        [Description("Platform Target Mismatch")]
        ProcessorArchitectureMismatch,
    }
}
