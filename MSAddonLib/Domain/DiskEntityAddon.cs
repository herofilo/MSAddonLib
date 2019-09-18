using System.Collections.Generic;
using MSAddonLib.Domain.Addon;
using MSAddonLib.Persistence;
using MSAddonLib.Util.Persistence;
using MSAddonLib.Util;
using SevenZip;

namespace MSAddonLib.Domain
{
    public class DiskEntityAddon : DiskEntityBase, IDiskEntity
    {
        public DiskEntityAddon(string pEntityPath, string pArchivedPath, IReportWriter pReportWriter) : base(pEntityPath, pArchivedPath, pReportWriter)
        {
        }


        // ---------------------------------------------------------------------------

        // TODO - Print report
        public bool CheckEntity(ProcessingFlags pProcessingFlags, string pNamePrinted = null)
        {
            string report;
            bool checkOk = CheckEntity(pProcessingFlags, out report);
            
            pNamePrinted = string.IsNullOrEmpty(pNamePrinted) ? Name : Name + pNamePrinted;

            if (checkOk && pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues))
                return false;

            if (!checkOk || !pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents))
            {
                ReportWriter.WriteReportLineFeed($"*{pNamePrinted} : {report}");
                return checkOk;
            }

            ReportWriter.WriteReportLineFeed($"*{pNamePrinted} :");
            // ReportWriter.IncreaseReportLevel();
            ReportWriter.WriteReportLineFeed(report);
            // ReportWriter.DecreaseReportLevel();
            return true;
        }




        private bool CheckEntity(ProcessingFlags pProcessingFlags, out string pReport)
        {
            bool reportOnlyIssues = pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues);
            bool showAddonContents = pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents);
            bool appendToPackageSet = pProcessingFlags.HasFlag(ProcessingFlags.AppendToAddonPackageSet);
            List<ArchiveFileInfo> entryList;

            SevenZipArchiver archiver = new SevenZipArchiver(AbsolutePath);

            archiver.ArchivedFileList(out entryList);
            if ((entryList?.Count ?? -1) <= 0)
            {
                pReport = $"{ErrorTokenString} Invalid file or format";
                if (showAddonContents)
                    pReport += "\n";
                return false;
            }

            List<string> demoMovies, stockAssets;
            bool hasMeshes, hasData;
            AddonSignatureFile addonSignature;
            bool formatOk = CheckFormat(archiver, entryList, out hasMeshes, out hasData, out demoMovies, out stockAssets, out addonSignature);
            if (!formatOk)
            {
                pReport = $"{ErrorTokenString} Invalid/obsolete addon format";
                if (showAddonContents)
                    pReport += "\n";
                return false;
            }

            // Addon good format

            pReport = null;
            if (reportOnlyIssues)
                return true;

            if (!showAddonContents)
            {
                pReport = (hasMeshes ? "OK" : "OK, no meshes");
                if (demoMovies != null)
                    pReport += " (incl. Movies)";
                if (stockAssets != null)
                    pReport += " (incl. Stock assets)";
                string freeText = addonSignature.Free ? "" : "  NOT FREE!";
                pReport += $"   [{addonSignature.Publisher}{freeText}]";
                if(!appendToPackageSet)
                    return true;
            }

            string tempPath = Utils.GetTempDirectory();

            
            AddonPackage package = new AddonPackage(archiver, pProcessingFlags, tempPath, ArchivedPath);

            if (showAddonContents)
                pReport = package?.ToString();

            if (appendToPackageSet && (AddonPackageSet != null) && (package != null) && (!package.HasIssues))
            {
                if (AddonPackageSet.Append(package,
                    pProcessingFlags.HasFlag(ProcessingFlags.AppendToAddonPackageSetForceRefresh)))
                {
                    pReport += " >>> Inserted/updated into Database";
                    if (package.HasIssues)
                        pReport += " [Has Issues!]";
                }
            }


            return true;
        }
        



        private bool CheckFormat(SevenZipArchiver pArchiver,
            List<ArchiveFileInfo> pArchiveEntryList, out bool pHasMeshes, 
            out bool pHasData, out List<string> pDemoMovies, out List<string> pStockAssets,
            out AddonSignatureFile pAddonSignature)
        {
            List<string> fileNames = new List<string>();
            pHasMeshes = false;
            List<string> movies = new List<string>();
            List<string> stocks = new List<string>();
            pHasData = false;
            pAddonSignature = null;

            foreach (ArchiveFileInfo entry in pArchiveEntryList)
            {
                string lwrName; 
                fileNames.Add(lwrName = entry.FileName?.Trim().ToLower());
                if (!pHasData)
                {
                    if (lwrName?.StartsWith(@"data\") ?? false)
                        pHasData = true;
                }

                if (lwrName?.StartsWith(@"movies\") ?? false)
                {
                    string movieName = GetSubitemName(entry.FileName, "movies");
                    if (!string.IsNullOrEmpty(movieName) && !movies.Contains(movieName))
                    {
                        movies.Add(movieName);
                    }
                }

                if (lwrName?.StartsWith(@"stock\") ?? false)
                {
                    string stockName = GetSubitemName(entry.FileName, "stock");
                    if (!string.IsNullOrEmpty(stockName) && !stocks.Contains(stockName))
                    {
                        stocks.Add(stockName);
                    }
                }

            }

            pDemoMovies = movies.Count > 0 ? movies : null;
            pStockAssets = stocks.Count > 0 ? stocks : null;

            pHasMeshes = fileNames.Contains("meshdata.data") && fileNames.Contains("meshdata.index");

            bool formatOk = fileNames.Contains(".addon") && fileNames.Contains("assetdata.jar");

            byte[] addonContent = pArchiver.ExtractArchivedFileToByte(".addon");
            if (addonContent != null)
            {
                pAddonSignature = AddonSignatureFile.Load(addonContent);
            }

            return formatOk && (pAddonSignature != null);
        }



        private string GetSubitemName(string pFileName, string pFirstPart)
        {
            if (string.IsNullOrEmpty(pFileName = pFileName?.Trim()) || (string.IsNullOrEmpty(pFirstPart = pFirstPart?.ToLower().Trim())))
                return null;
            string[] parts = pFileName?.Split(@"\".ToCharArray());
            if (parts.Length < 2)
                return null;

            return (parts[0].ToLower() == pFirstPart) ? parts[1] : null;
        }
    }
}
