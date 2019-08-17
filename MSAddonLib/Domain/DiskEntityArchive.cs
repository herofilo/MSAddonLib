using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MSAddonLib.Persistence;
using MSAddonLib.Util;
using MSAddonLib.Util.Persistence;
using SevenZip;

namespace MSAddonLib.Domain
{
    public class DiskEntityArchive : DiskEntityBase, IDiskEntity
    {
        public DiskEntityArchive(string pEntityPath, bool pInsideArchive, IReportWriter pReportWriter) : base(pEntityPath, pInsideArchive, pReportWriter)
        {
        }

        // -------------------------------------------------------------------------------------------

        // TODO - Print report
        public bool CheckEntity(ProcessingFlags pProcessingFlags, string pNamePrinted = null)
        {

            ReportWriter.WriteReportLineFeed($"+{Name} : ");
            ReportWriter.IncreaseReportLevel();

            string report;
            bool checkOk = CheckEntity(pProcessingFlags, out report);

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




        private bool CheckEntity(ProcessingFlags pProcessingFlags, out string pReport)
        {
            // bool reportOnlyIssues = pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues);
            // bool showAddonContents = pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents);
            pReport = null;

            SevenZipArchiver archiver = new SevenZipArchiver(AbsolutePath);
            List<ArchiveFileInfo> archiveEntryList;
            List<string> fileList = GetFileList(archiver, out archiveEntryList);

            if ((fileList?.Count ?? -1) <= 0)
                return false;

            return CheckFiles(pProcessingFlags, archiver, archiveEntryList, fileList, out pReport);
        }



        private List<string> GetFileList(SevenZipArchiver pArchiver, out List<ArchiveFileInfo> pEntryList)
        {
            pArchiver.ArchivedFileList(out pEntryList);
            if ((pEntryList?.Count ?? -1) <= 0)
                return null;

            List<string> fileList = new List<string>();
            foreach (ArchiveFileInfo entry in pEntryList)
            {
                string entryLower = entry.FileName.ToLower();
                if (!entry.IsDirectory &&
                    (entryLower.EndsWith(".addon") || entryLower.EndsWith(".skp")))
                {
                    fileList.Add(entry.FileName);
                }
            }

            fileList.Sort();
            return fileList;
        }


        /// <summary>
        /// Checks files inside the archive
        /// </summary>
        /// <param name="pProcessingFlags">Processing flags</param>
        /// <param name="pArchiver">Archiver used to manage the archive contents</param>
        /// <param name="pArchiveEntryList">Full information about files in archive</param>
        /// <param name="pFileList">List of files inside the archive</param>
        /// <param name="pReport">Output text</param>
        /// <returns>Result of check</returns>
        private bool CheckFiles(ProcessingFlags pProcessingFlags, SevenZipArchiver pArchiver, List<ArchiveFileInfo> pArchiveEntryList, List<string> pFileList, out string pReport)
        {
            // bool reportOnlyIssues = pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues);
            // bool showAddonContents = pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents);


            string rootTempPath = Utils.GetTempDirectory();
            pArchiver.ArchivedFilesExtract(rootTempPath, pFileList);
            string currentPath = Utils.GetExecutableDirectory();

            Directory.SetCurrentDirectory(rootTempPath);
            pReport = null;
            try
            {
                foreach (string fileName in pFileList)
                {
                    string extension =
                        Path.GetExtension(fileName)?.Trim().ToLower();

                    bool isAddonFile = false;
                    if (extension == ".addon")
                    {
                        if (fileName.ToLower() == ".addon")
                        {
                            string rootFolder;
                            if (!InsideArchive && pProcessingFlags.HasFlag(ProcessingFlags.CorrectDisguisedFiles) &&
                                IsValidAddon(pArchiveEntryList, false, out rootFolder))
                            {
                                string errorText;
                                string newAddonFile = PersistenceUtils.RestoreAddonFile(pArchiver, pProcessingFlags.HasFlag(ProcessingFlags.CorrectDisguisedFilesDeleteSource), null, out errorText);
                                if (newAddonFile == null)
                                {
                                    pReport = $"   {ErrorTokenString} Possibly an Addon file disguised as an archive. Failed to restore: {errorText}";
                                    return false;
                                }
                                pReport = $"   Addon file disguised as an archive. Restored: {Path.GetFileName(newAddonFile)}";
                                return true;
                            }
                            else
                            {
                                pReport = $"   {ErrorTokenString} Possibly an Addon file disguised as an archive";
                                return false;
                            }
                        }
                        if (fileName.ToLower().EndsWith(@"\.addon"))
                        {
                            string rootFolder;
                            if (!InsideArchive && pProcessingFlags.HasFlag(ProcessingFlags.CorrectDisguisedFiles) &&
                                IsValidAddon(pArchiveEntryList, true, out rootFolder))
                            {
                                string errorText;
                                string newAddonFile = PersistenceUtils.RestoreAddonFile(pArchiver, pProcessingFlags.HasFlag(ProcessingFlags.CorrectDisguisedFilesDeleteSource), rootFolder, out errorText);
                                if (newAddonFile == null)
                                {
                                    pReport = $"   {ErrorTokenString} Possibly an Addon file disguised as an archive. Failed to restore: {errorText}";
                                    return false;
                                }
                                pReport = $"   Addon file disguised as an archive. Restored: {Path.GetFileName(newAddonFile)}";
                                return true;
                            }
                            else
                            {
                                pReport =
                                    $"   {ErrorTokenString} Possibly an Addon file disguised as an archive, with a root directory";
                                return false;
                            }
                        }

                        isAddonFile = true;
                    }

                    IDiskEntity diskEntity =
                        isAddonFile
                        ? new DiskEntityAddon(fileName, true, ReportWriter)
                        : (IDiskEntity)new DiskEntitySketchup(fileName, true, ReportWriter);

                    diskEntity.CheckEntity(pProcessingFlags);

                    File.Delete(fileName);
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

        private bool IsValidAddon(List<ArchiveFileInfo> pArchiveEntryList, bool pIsRooted, out string pRootFolder)
        {
            pRootFolder = null;
            bool hasSignatureFile = false;
            bool hasAssetDataFile = false;
            bool hasDataFolder = false;
            string root = null;
            foreach (ArchiveFileInfo item in pArchiveEntryList)
            {
                string fileLower = item.FileName.ToLower().Trim();
                if (pIsRooted)
                {
                    if (root == null)
                    {
                        string[] parts = item.FileName.Split("\\".ToCharArray());
                        if (parts.Length > 1)
                        {
                            pRootFolder = parts[0];
                            root = parts[0] + "\\";
                        }
                        else
                            return false;
                    }

                    fileLower = fileLower.Remove(0, root.Length);
                }
                if (fileLower == ".addon")
                {
                    hasSignatureFile = true;
                    continue;
                }
                if (fileLower == "assetdata.jar")
                {
                    hasAssetDataFile = true;
                    continue;
                }

                if (!hasDataFolder && fileLower.StartsWith("data\\"))
                    hasDataFolder = true;

                if (hasSignatureFile && hasAssetDataFile && hasDataFolder)
                    return true;
            }

            return false;
        }
    }
}
