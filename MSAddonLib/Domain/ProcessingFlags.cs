using System;

namespace MSAddonLib.Domain
{
    [Flags]
    public enum  ProcessingFlags
    {
        None = 0,
        JustReportIssues = 1,
        ShowAddonContents = 2,
        ListAllAnimationFiles = 4,
        ListGestureGaitsAnimations = 8
    }
}
