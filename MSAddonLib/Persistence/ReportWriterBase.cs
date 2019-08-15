using System;

namespace MSAddonLib.Persistence
{
    public class ReportWriterBase : IReportWriter
    {
        public int ReportLevel { get; protected set; } = 0;


        // ------------------------------------------------------------------------------------------------------

        public void IncreaseReportLevel()
        {
            ReportLevel++;
        }

        public void DecreaseReportLevel()
        {
            if (ReportLevel > 0)
                ReportLevel--;
        }

        public virtual void ClearOutput()
        {
        }


        public virtual bool WriteReport(string pText)
        {
            return true;
        }

        public bool WriteReport(int pNewReportLevel, string pText)
        {
            ReportLevel = (pNewReportLevel < 0) ? 0 : pNewReportLevel;
            return WriteReport(pText);
        }

        public virtual bool WriteReportNoPrefix(string pText)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteReportLineFeed(string pText)
        {
            throw new NotImplementedException();
        }


        public bool WriteReportLineFeed(int pNewReportLevel, string pText)
        {
            ReportLevel = (pNewReportLevel < 0) ? 0 : pNewReportLevel;
            return WriteReportLineFeed(pText);
        }

        public virtual bool WriteReportNoPrefixLineFeed(string pText)
        {
            throw new NotImplementedException();
        }
    }
}
