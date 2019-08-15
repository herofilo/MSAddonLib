namespace MSAddonLib.Persistence
{
    public class NullReportWriter : ReportWriterBase, IReportWriter
    {

        public NullReportWriter()
        {

        }


        public NullReportWriter(object[] pArguments)
        {

        }

        // ---------------------------------------------------------------------------------------------


        public override void ClearOutput()
        {
        }


        public override bool WriteReport(string pText)
        {
            return true;
        }

        public override bool WriteReportNoPrefix(string pText)
        {
            return true;
        }

        public override bool WriteReportLineFeed(string pText)
        {
            return true;
        }

        public override bool WriteReportNoPrefixLineFeed(string pText)
        {
            return true;
        }


    }
}
