using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using MSAddonLib.Domain.Addon;
using MSAddonLib.Persistence;
using MSAddonLib.Util;
using MSAddonLib.Util.Persistence;
using SevenZip;

namespace MSAddonLib.Domain
{
    public sealed class DiskEntityAddonFolder : DiskEntityBase, IDiskEntity
    {
        public DiskEntityAddonFolder(string pEntityPath, string pArchivedPath, IReportWriter pReportWriter) : base(pEntityPath, pArchivedPath, pReportWriter)
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
            bool showDetailedContents = pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents);
            bool appendToPackageSet = pProcessingFlags.HasFlag(ProcessingFlags.AppendToAddonPackageSet);

            if (!showDetailedContents)
            {
                pReport = BriefReport();

                if(!appendToPackageSet)
                    return true;
            }

            string tempPath = Utils.GetTempDirectory();

            AddonPackage package = new AddonPackage(AbsolutePath, pProcessingFlags, tempPath);

            if (showDetailedContents)
                pReport = package?.ToString();

            if (appendToPackageSet && (AddonPackageSet != null) && (package != null) && (!package.HasIssues))
            {
                if (AddonPackageSet.Append(package,
                    pProcessingFlags.HasFlag(ProcessingFlags.AppendToAddonPackageSetForceRefresh)))
                    pReport += " >>> Inserted/updated into Database";
            }

            return true;
        }


        private string BriefReport()
        {
            string report;
            bool hasDemoMovies, hasStockAssets;
            bool hasMeshes;
            AddonSignatureFile addonSignature; 
            CheckContents(out hasMeshes, out hasDemoMovies, out hasStockAssets, out addonSignature);

            report = (hasMeshes ? "OK" : "OK, no meshes");
            if (hasDemoMovies)
                report += " (incl. Movies)";
            if (hasStockAssets)
                report += " (incl. Stock assets)";
            string freeText = addonSignature.Free ? "" : "  NOT FREE!";
            report += $"   [{addonSignature.Publisher}{freeText}]";

            return report;
        }



        private void CheckContents(out bool pHasMeshes, out bool pHasDemoMovies, out bool pHasStockAssets, out AddonSignatureFile pAddonSignature)
        {
            pAddonSignature = null;
            byte[] addonContent = File.ReadAllBytes(Path.Combine(AbsolutePath, ".addon"));
            if (addonContent != null)
            {
                pAddonSignature = AddonSignatureFile.Load(addonContent);
            }

            pHasMeshes = File.Exists(Path.Combine(AbsolutePath, "meshdata.data")) &&
                         File.Exists(Path.Combine(AbsolutePath, "meshdata.index"));

            pHasDemoMovies = Directory.Exists(Path.Combine(AbsolutePath, "movies"));
            pHasStockAssets = Directory.Exists(Path.Combine(AbsolutePath, "stock"));
        }
    }
}
