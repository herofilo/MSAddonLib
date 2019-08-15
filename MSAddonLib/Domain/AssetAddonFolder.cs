using System.IO;
using MSAddonLib.Persistence;
using MSAddonLib.Util;
using MSAddonLib.Util.Persistence;
using SevenZip;

namespace MSAddonLib.Domain
{
    public sealed class AssetAddonFolder : AssetBase, IAsset
    {
        public AssetAddonFolder(string pAssetPath, IReportWriter pReportWriter) : base(pAssetPath, pReportWriter)
        {
        }


        public bool CheckAsset(ProcessingFlags pProcessingFlags, string pNamePrinted = null)
        {
            string report;
            return CheckAsset(pProcessingFlags, out report);

            /*
            if (checkOk && pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues))
                return false;

            if (!checkOk || !pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents))
            {
                // ReportWriter.WriteReportLineFeed($"{Name} : {report}");
                return checkOk;
            }
            */
            // ReportWriter.WriteReportLineFeed("(Installed Addon/Content Package):");
            // ReportWriter.IncreaseReportLevel();
            // ReportWriter.WriteReportLineFeed(report);
            // ReportWriter.DecreaseReportLevel();
            //return true;
        }


        private bool CheckAsset(ProcessingFlags pProcessingFlags, out string pReport)
        {
            pReport = null;

            string fileName = Path.GetFileNameWithoutExtension(AssetPath) + ".addon";
            string tempAddonArchive = Path.Combine(Utils.GetTempDirectory(), fileName);
            try
            {
                SevenZipArchiver archiver = new SevenZipArchiver(tempAddonArchive);
                archiver.CompressionLevel = CompressionLevel.Fast;
                if (!archiver.ArchiveFolder(AbsolutePath))
                {

                    return false;
                }
                new AssetAddon(tempAddonArchive, ReportWriter).CheckAsset(pProcessingFlags, " (Installed)");
            }
            catch
            {

            }
            finally
            {
                if(File.Exists(tempAddonArchive))
                    File.Delete(tempAddonArchive);
            }
            return true;
        }


    }
}
