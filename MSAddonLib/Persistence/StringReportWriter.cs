using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSAddonLib.Persistence
{
    public sealed class StringReportWriter : ReportWriterBase, IReportWriter
    {


        public new string Text => _stringBuilder.ToString();

        private StringBuilder _stringBuilder = new StringBuilder();


        // ------------------------------------------------------------------------------------------------------

        public override void ClearOutput()
        {
            _stringBuilder.Clear();
            ReportLevel = 0;
        }



        public override bool WriteReport(string pText)
        {
            return _writeOutput(pText, false, true);
        }

        public override bool WriteReportNoPrefix(string pText)
        {
            return _writeOutput(pText, false, false);
        }


        public override bool WriteReportLineFeed(string pText)
        {
            return _writeOutput(pText, true, true);
        }


        public override bool WriteReportNoPrefixLineFeed(string pText)
        {
            return _writeOutput(pText, true, false);
        }


        private bool _writeOutput(string pText, bool pAppendLineFeed, bool pWithPrefix)
        {
            if (pText.Length == 0)
                return true;

            string prefixString = ((ReportLevel == 0) || !pWithPrefix) ? "" : new string(' ', 4 * ReportLevel);

            string[] lines = pText.Split("\n".ToCharArray());
            if (lines.Length == 0)
                return true;

            int lastLineIndex = lines.Length - 1;
            for (int index = 0; index < lines.Length; ++index)
            {
                string eol = (index < lastLineIndex) ? Environment.NewLine : "";
                _stringBuilder.Append(prefixString + lines[index] + eol);
            }

            if (pAppendLineFeed)
                _stringBuilder.Append(Environment.NewLine);

            return true;
        }

    }
}
