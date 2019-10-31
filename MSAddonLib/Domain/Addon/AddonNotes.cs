using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSAddonLib.Domain.Addon
{
    /// <summary>
    /// Unofficial Addon notes 
    /// </summary>
    public sealed class AddonNotes
    {
        public string OriginalPublisher { get; set; }

        public string Comments { get; set; }


        // -----------------------------------------------------------------------------------------------------------------------

        public static AddonNotes LoadFromString(string pText, out string pErrorText)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pText = pText?.Trim()))
            {
                pErrorText = "AddonNotes.LoadFromString(): No input text";
                return null;
            }

            AddonNotes addonNotes = new AddonNotes();

            foreach (string line in pText.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                string[] splitStrings = line.Trim().Split('=');
                if (splitStrings.Length > 1)
                {
                    string itemName = splitStrings[0]?.Trim().ToLower();
                    if(string.IsNullOrEmpty(itemName))
                        continue;
                    string itemValue = splitStrings[1]?.Trim();
                    switch (itemName)
                    {
                        case "original publisher":
                        case "original_publisher":
                        case "publisher":
                            addonNotes.OriginalPublisher = itemValue; break;
                        case "comments": addonNotes.Comments = itemValue; break;
                    }
                }
            }
            return addonNotes;
        }
    }
}
