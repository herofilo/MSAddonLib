using System;
using System.IO;

namespace MSAddonLib.Domain.Addon
{
    public class AddonPropertiesInfo
    {
        public string Name { get; set; }
        public string Blurb { get; set; }
        public string Url { get; set; }



        public static AddonPropertiesInfo Load(string pFilename, out string pErrorText)
        {
            pErrorText = null;
            if (!File.Exists(pFilename))
            {
                pErrorText = "AddonPropertiesInfo.Load(): File not found";
                return null;
            }

            AddonPropertiesInfo addonPropertiesInfo = new AddonPropertiesInfo();

            try
            {
                using (TextReader reader = File.OpenText(pFilename))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] splitStrings = line.Split('=');
                        if (splitStrings.Length > 1)
                        {
                            switch (splitStrings[0].ToLower())
                            {
                                case "name": addonPropertiesInfo.Name = splitStrings[1]; break;
                                case "blurb": addonPropertiesInfo.Blurb = splitStrings[1].Replace("\\n", " - "); break;
                                case "url": addonPropertiesInfo.Url = splitStrings[1].Replace("\\", ""); break;
                            }
                        }
                    }

                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing AddonPropertiesInfo: {exception.Message}";
                addonPropertiesInfo = null;
            }

            return addonPropertiesInfo;
        }


        public static AddonPropertiesInfo LoadFromString(string pText, out string pErrorText)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pText = pText?.Trim()))
            {
                pErrorText = "AddonPropertiesInfo.LoadFromString(): No input text";
                return null;
            }

            AddonPropertiesInfo addonPropertiesInfo = new AddonPropertiesInfo();

            foreach (string line in pText.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                string[] splitStrings = line.Trim().Split('=');
                if (splitStrings.Length > 1)
                {
                    switch (splitStrings[0].ToLower())
                    {
                        case "name": addonPropertiesInfo.Name = splitStrings[1]; break;
                        case "blurb": addonPropertiesInfo.Blurb = splitStrings[1].Replace("\\n", " - "); break;
                        case "url": addonPropertiesInfo.Url = splitStrings[1].Replace("\\", ""); break;
                    }
                }
            }
            return addonPropertiesInfo;
        }

    }
}