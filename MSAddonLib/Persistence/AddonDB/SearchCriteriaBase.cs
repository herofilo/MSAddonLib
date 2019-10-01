using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MSAddonLib.Persistence.AddonDB
{
    public class SearchCriteriaBase
    {


        protected Regex CreateMultiValuedRegex(string pStrings, bool pCleaned = false)
        {
            if (string.IsNullOrEmpty(pStrings = pStrings?.Trim().ToLower()))
                return null;

            string[] values = pStrings.Trim().ToLower()
                .Split(" ,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if ((values == null) || (values.Length == 0))
                return null;

            string regexString;
            if (values.Length == 1)
                regexString = values[0];
            else
            {
                StringBuilder regexBuilder = new StringBuilder();
                foreach (string item in values)
                    regexBuilder.Append($"{item}|");
                regexString = regexBuilder.ToString();
                regexString = regexString.Substring(0, regexString.Length - 1);
                regexString = $"({regexString})";
            }

            Regex regex = null;
            try
            {
                regex = new Regex(regexString, RegexOptions.IgnoreCase);
            }
            catch
            {
                if (!pCleaned)
                {
                    pStrings = CleanRegexStrings(pStrings);
                    regex = CreateMultiValuedRegex(pStrings, true);
                }
            }

            return regex;
        }


        private string CleanRegexStrings(string pStrings)
        {
            while (!string.IsNullOrEmpty(pStrings))
            {
                int index = pStrings.IndexOfAny("{}().,|[]".ToCharArray());
                if (index < 0)
                    break;
                pStrings = pStrings.Remove(index, 1);
            }

            return pStrings;
        }


    }
}
