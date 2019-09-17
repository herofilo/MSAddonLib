using System.IO;
using System.Linq;
using MSAddonLib.Domain.Addon;
using MSAddonLib.Persistence;

namespace MSAddonLib.Domain
{
    public class DiskEntityFolder : DiskEntityBase, IDiskEntity
    {
        public DiskEntityFolder(string pEntityPath, string pArchivedPath, IReportWriter pReportWriter) : base(pEntityPath, pArchivedPath, pReportWriter)
        {
        }


        // ------------------------------------------------------------------------------------------


        public bool CheckEntity(ProcessingFlags pProcessingFlags, string pNamePrinted = null)
        {
            if (IsAddonFolder(EntityPath))
            {
                new DiskEntityAddonFolder(EntityPath, ArchivedPath, ReportWriter).CheckEntity(pProcessingFlags);
                return true;
            }

            ReportWriter.WriteReportLineFeed($"\n/{Name} :");
            ReportWriter.IncreaseReportLevel();

            string report;
            bool checkOk = CheckEntity(pProcessingFlags, out report);

            ReportWriter.DecreaseReportLevel();
            ReportWriter.WriteReportLineFeed("");
            return checkOk;
        }


        private bool CheckEntity(ProcessingFlags pProcessingFlags, out string pReport)
        {
            pReport = null;

            bool reportOnlyIssues = pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues);
            // bool showAddonContents = pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents);


            DirectoryInfo directoryInfo = new DirectoryInfo(EntityPath);

            FileInfo[] addonInfoList = directoryInfo.GetFiles("*.addon", SearchOption.TopDirectoryOnly);

            foreach (FileInfo item in addonInfoList)
            {
                new DiskEntityAddon(item.FullName, ArchivedPath, ReportWriter).CheckEntity(pProcessingFlags);
            }


            FileInfo[] sketchupInfoList = directoryInfo.GetFiles("*.skp", SearchOption.TopDirectoryOnly);

            foreach (FileInfo item in sketchupInfoList)
            {
                new DiskEntitySketchup(item.FullName, null, ReportWriter).CheckEntity(pProcessingFlags);
            }


            FileInfo[] archiveInfoList = directoryInfo.GetFiles("*.zip", SearchOption.TopDirectoryOnly);
            FileInfo[] rarInfoList = directoryInfo.GetFiles("*.rar", SearchOption.TopDirectoryOnly);
            archiveInfoList = archiveInfoList.Concat(rarInfoList).ToArray();
            FileInfo[] s7InfoList = directoryInfo.GetFiles("*.7z", SearchOption.TopDirectoryOnly);
            archiveInfoList = archiveInfoList.Concat(s7InfoList).ToArray();

            foreach (FileInfo item in archiveInfoList)
            {
                new DiskEntityArchive(item.FullName, ArchivedPath, ReportWriter).CheckEntity(pProcessingFlags);
            }


            if (!pProcessingFlags.HasFlag(ProcessingFlags.FolderTopOnlySearch))
            {
                DirectoryInfo[] subdirectories = directoryInfo.GetDirectories();
                if (subdirectories.Length > 0)
                {
                    foreach (DirectoryInfo subdirectoryInfo in subdirectories)
                    {
                        new DiskEntityFolder(subdirectoryInfo.FullName, null, ReportWriter).CheckEntity(pProcessingFlags);
                    }
                }
            }

            return true;
        }

    }
}
