using System;
using System.Windows.Forms;

namespace MSAddonLib.Persistence
{
    public class FormReportWriter : ReportWriterBase, IReportWriter
    {

        private TextBox _outputTextBox = null;


        // ------------------------------------------------------------------------------------------------------

        public FormReportWriter()
        {

        }


        public FormReportWriter(object[] pArguments)
        {
            if ((pArguments == null) || (pArguments.Length == 0))
                return;
            object argument0 = pArguments[0];
            if (argument0.GetType() == typeof(TextBox))
                _outputTextBox = argument0 as TextBox;
        }


        // ------------------------------------------------------------------------------------------------------

        public override void ClearOutput()
        {
            if (_outputTextBox == null)
                return;
            _outputTextBox.Clear();
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
            if (_outputTextBox == null)
                return false;

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
                _outputTextBox.AppendText(prefixString + lines[index] + eol);
            }

            if(pAppendLineFeed)
                _outputTextBox.AppendText(Environment.NewLine);

            return true;
        }
    }
}
