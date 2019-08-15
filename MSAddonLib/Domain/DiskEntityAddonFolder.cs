using System.IO;
using MSAddonLib.Persistence;
using MSAddonLib.Util;
using MSAddonLib.Util.Persistence;
using SevenZip;

namespace MSAddonLib.Domain
{
    // TODO : Rewrite for direct support
    public sealed class DiskEntityAddonFolder : DiskEntityBase, IDiskEntity
    {
        public DiskEntityAddonFolder(string pEntityPath, IReportWriter pReportWriter) : base(pEntityPath, pReportWriter)
        {
        }


        public bool CheckEntity(ProcessingFlags pProcessingFlags, string pNamePrinted = null)
        {
            string report;
            bool checkOk = CheckEntity(pProcessingFlags, out report);

            string namePrinted = Name + " (installed)";

            if (checkOk && pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues))
                return false;

            if (!checkOk || !pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents))
            {
                ReportWriter.WriteReportLineFeed($"*{namePrinted} : {report}");
                return checkOk;
            }

            ReportWriter.WriteReportLineFeed($"*{namePrinted} :");
            // ReportWriter.IncreaseReportLevel();
            ReportWriter.WriteReportLineFeed(report);
            // ReportWriter.DecreaseReportLevel();
            return true;
        }


        private bool CheckEntity(ProcessingFlags pProcessingFlags, out string pReport)
        {
            pReport = null;

            new DiskEntityAddon(EntityPath, ReportWriter).CheckEntity(pProcessingFlags, " (Installed)");


            return true;
        }



    }
}
