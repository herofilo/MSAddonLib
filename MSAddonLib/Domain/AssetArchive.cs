using System.Collections.Generic;
using System.IO;
using MSAddonLib.Persistence;
using MSAddonLib.Util;
using MSAddonLib.Util.Persistence;
using SevenZip;

namespace MSAddonLib.Domain
{
    public class AssetArchive : AssetBase, IAsset
    {
        public AssetArchive(string pAssetPath, IReportWriter pReportWriter) : base(pAssetPath, pReportWriter)
        {
        }

        // -------------------------------------------------------------------------------------------

        // TODO - Print report
        public bool CheckAsset(ProcessingFlags pProcessingFlags, string pNamePrinted = null)
        {

            ReportWriter.WriteReportLineFeed($"+{Name} : ");
            ReportWriter.IncreaseReportLevel();

            string report;
            bool checkOk = CheckAsset(pProcessingFlags, out report);

            if (!string.IsNullOrEmpty(report))
            {
                ReportWriter.WriteReportLineFeed(report);
                ReportWriter.DecreaseReportLevel();
                return checkOk;
            }
            ReportWriter.DecreaseReportLevel();
            ReportWriter.WriteReportLineFeed("");
            return checkOk;
        }




        private bool CheckAsset(ProcessingFlags pProcessingFlags, out string pReport)
        {
            // bool reportOnlyIssues = pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues);
            // bool showAddonContents = pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents);
            pReport = null;

            SevenZipArchiver archiver = new SevenZipArchiver(AbsolutePath);
            List<string> fileList = GetFileList(archiver);

            if ((fileList?.Count ?? -1) <= 0)
                return false;

            return CheckFiles(pProcessingFlags, archiver, fileList, out pReport);
        }



        private List<string> GetFileList(SevenZipArchiver pArchiver)
        {
            List<ArchiveFileInfo> entryList;

            pArchiver.ArchivedFileList(out entryList);
            if ((entryList?.Count ?? -1) <= 0)
                return null;

            List<string> fileList = new List<string>();
            foreach (ArchiveFileInfo entry in entryList)
            {
                if (!entry.IsDirectory && 
                    (entry.FileName.ToLower().EndsWith(".addon") || entry.FileName.ToLower().EndsWith(".skp")))
                {
                    fileList.Add(entry.FileName);
                }
            }

            fileList.Sort();
            return fileList;
        }


        private bool CheckFiles(ProcessingFlags pProcessingFlags, SevenZipArchiver pArchiver, List<string> pFileList, out string pReport)
        {
            bool reportOnlyIssues = pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues);
            // bool showAddonContents = pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents);


            string rootTempPath = Utils.GetTempDirectory();
            pArchiver.ArchivedFilesExtract(rootTempPath, pFileList);
            string currentPath = Utils.GetExecutableDirectory();

            Directory.SetCurrentDirectory(rootTempPath);
            pReport = null;
            try
            {
                foreach (string addonFile in pFileList)
                {
                    string extension =
                        Path.GetExtension(addonFile)?.Trim().ToLower();

                    bool isAddonFile = false;
                    if (extension == ".addon")
                    {
                        if (addonFile.ToLower() == ".addon")
                        {
                            pReport = $"   {ErrorTokenString} Possibly an Addon file disguised as a ZIP Archive";
                            return false;
                        }
                        if (addonFile.ToLower().EndsWith(@"\.addon"))
                        {
                            pReport = $"   {ErrorTokenString} Possibly an Addon file disguised as a ZIP Archive, with a root directory";
                            return false;
                        }

                        isAddonFile = true;
                    }

                    IAsset asset =
                        isAddonFile
                        ? new AssetAddon(addonFile, ReportWriter)
                        : (IAsset) new AssetSketchup(addonFile, ReportWriter);

                    asset.CheckAsset(pProcessingFlags);

                    File.Delete(addonFile);
                }
            }
            catch
            {

            }
            finally
            {
                Directory.SetCurrentDirectory(currentPath);
            }

            return true;
        }



    }
}
