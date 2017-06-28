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

        [Description("Possible Cause")]
        LoadingErrorPossibleCause,

        [Description("Possible Cause #1")]
        LoadingErrorPossibleCause1,

        [Description("Possible Cause #2")]
        LoadingErrorPossibleCause2,

        [Description("Possible Cause #3")]
        LoadingErrorPossibleCause3
    }
}
