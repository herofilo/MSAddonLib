using System;

namespace MSAddonLib.Domain
{
    [Flags]
    public enum  ProcessingFlags
    {
        None = 0,
        JustReportIssues = 0x0001,
        ShowAddonContents = 0x0002,
        ListAllAnimationFiles = 0x0004,
        ListGestureGaitsAnimations = 0x0008,
        ListWeirdGestureGaitsVerbs = 0x0010
    }
}
