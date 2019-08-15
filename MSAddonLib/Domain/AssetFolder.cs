using System.IO;
using System.Linq;
using MSAddonLib.Domain.Addon;
using MSAddonLib.Persistence;

namespace MSAddonLib.Domain
{
    public class AssetFolder : AssetBase, IAsset
    {
        public AssetFolder(string pAssetPath, IReportWriter pReportWriter) : base(pAssetPath, pReportWriter)
        {
        }


        // ------------------------------------------------------------------------------------------


        public bool CheckAsset(ProcessingFlags pProcessingFlags, string pNamePrinted = null)
        {
            if (IsAddonFolder())
            {
                new AssetAddonFolder(AssetPath, ReportWriter).CheckAsset(pProcessingFlags);
                return true;
            }

            ReportWriter.WriteReportLineFeed($"\n/{Name} :");
            ReportWriter.IncreaseReportLevel();

            string report;
            bool checkOk = CheckAsset(pProcessingFlags, out report);

            ReportWriter.DecreaseReportLevel();
            ReportWriter.WriteReportLineFeed("");
            return checkOk;
        }


        private bool CheckAsset(ProcessingFlags pProcessingFlags, out string pReport)
        {
            pReport = null;

            bool reportOnlyIssues = pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues);
            // bool showAddonContents = pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents);


            DirectoryInfo directoryInfo = new DirectoryInfo(AssetPath);

            FileInfo[] addonInfoList = directoryInfo.GetFiles("*.addon", SearchOption.TopDirectoryOnly);
            

            foreach (FileInfo item in addonInfoList)
            {
                new AssetAddon(item.FullName, ReportWriter).CheckAsset(pProcessingFlags);
            }


            FileInfo[] sketchupInfoList = directoryInfo.GetFiles("*.skp", SearchOption.TopDirectoryOnly);

            foreach (FileInfo item in sketchupInfoList)
            {
                new AssetSketchup(item.FullName, ReportWriter).CheckAsset(pProcessingFlags);
            }


            FileInfo[] archiveInfoList = directoryInfo.GetFiles("*.zip", SearchOption.TopDirectoryOnly);
            FileInfo[] rarInfoList = directoryInfo.GetFiles("*.rar", SearchOption.TopDirectoryOnly);
            archiveInfoList = archiveInfoList.Concat(rarInfoList).ToArray();
            FileInfo[] s7InfoList = directoryInfo.GetFiles("*.7z", SearchOption.TopDirectoryOnly);
            archiveInfoList = archiveInfoList.Concat(s7InfoList).ToArray();

            foreach (FileInfo item in archiveInfoList)
            {
                new AssetArchive(item.FullName, ReportWriter).CheckAsset(pProcessingFlags);
            }


            DirectoryInfo[] subdirectories = directoryInfo.GetDirectories();
            if (subdirectories.Length > 0)
            {
                foreach (DirectoryInfo subdirectoryInfo in subdirectories)
                {
                    new AssetFolder(subdirectoryInfo.FullName, ReportWriter).CheckAsset(pProcessingFlags);
                }
            }



            return true;
        }

        private bool IsAddonFolder()
        {
            return File.Exists(Path.Combine(AssetPath, AddonPackage.SignatureFilename)) &&
                   File.Exists(Path.Combine(AssetPath, AddonPackage.AssetDataFilename)) &&
                   Directory.Exists(Path.Combine(AssetPath, "Data"));


        }
    }
}
