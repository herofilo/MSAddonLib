using System;
using System.IO;
using System.Text;
using MSAddonLib.Persistence;

namespace MSAddonLib.Domain
{
    public class DiskEntitySketchup : DiskEntityBase, IDiskEntity
    {
        public DiskEntitySketchup(string pEntityPath, bool pInsideArchive, IReportWriter pReportWriter) : base(pEntityPath, pInsideArchive, pReportWriter)
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        public bool CheckEntity(ProcessingFlags pProcessingFlags, string pNamePrinted = null)
        {
            string report;
            bool checkOk = CheckEntity(pProcessingFlags, out report);

            if (checkOk && pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues))
                return false;

            ReportWriter.WriteReportLineFeed($"{Name} : {report}");
            return checkOk;
        }


        private bool CheckEntity(ProcessingFlags pProcessingFlags, out string pReport)
        {
            bool reportOnlyIssues = pProcessingFlags.HasFlag(ProcessingFlags.JustReportIssues);
            // bool showAddonContents = pProcessingFlags.HasFlag(ProcessingFlags.ShowAddonContents);
            string versionString = null;
            try
            {
                byte[] headerBytes = File.ReadAllBytes(AbsolutePath);

                versionString = GetVersionString(headerBytes);
                if (versionString == null)
                {
                    pReport = $"{ErrorTokenString} Invalid file/unknown format";
                    return false;
                }

                if (!versionString.StartsWith("6."))
                {
                    pReport = $"{ErrorTokenString} Format not importable [{versionString}]";
                    return false;
                }
            }
            catch
            {
                
            }

            
            pReport = $"OK [{versionString}]";

            return true;
        }



        private string GetVersionString(byte[] pBytes)
        {
            const int startOffset = 0x24;

            StringBuilder versionStringBuilder = new StringBuilder();

            for (int index = startOffset; ;)
            {
                int value0 = pBytes[index++];

                int value1 = pBytes[index++];
                if (value1 != 0)
                    return null;

                char charValue = Convert.ToChar(value0);
                versionStringBuilder.Append(charValue);

                if (charValue == '}')
                    break;
            }

            string versionString = versionStringBuilder.ToString();

            return 
            (versionString.StartsWith("{") && versionString.EndsWith("}"))
            ? versionString.Remove(versionString.Length - 1).Remove(0, 1)
            : null;
        }



    }
}
