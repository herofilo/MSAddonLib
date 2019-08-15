namespace MSAddonLib.Persistence
{
    public interface IReportWriter
    {
        int ReportLevel { get;  }

        void IncreaseReportLevel();

        void DecreaseReportLevel();


        void ClearOutput();

        bool WriteReport(string pText);

        bool WriteReport(int pNewReportLevel, string pText);

        bool WriteReportNoPrefix(string pText);


        bool WriteReportLineFeed(string pText);

        bool WriteReportLineFeed(int pNewReportLevel, string pText);

        bool WriteReportNoPrefixLineFeed(string pText);

    }
}
