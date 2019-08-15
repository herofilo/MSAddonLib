using System;
using System.IO;
using System.Xml.Serialization;

namespace MSAddonLib.Domain.Addon
{

    /// <summary>
    /// Version information (in the addon version file)
    /// </summary>
    [XmlRoot("patch")]
    public class AddonVersionInfo
    {
        [XmlElement("component")]
        public string Component { get; set; }

        [XmlElement("revision")]
        public string Revision { get; set; }



        public static AddonVersionInfo Load(string pFilename, out string pErrorText)
        {
            pErrorText = null;
            if (!File.Exists(pFilename))
            {
                pErrorText = "AddonVersionInfo.Load(): File not found";
                return null;
            }

            AddonVersionInfo addonVersionInfo = new AddonVersionInfo();

            try
            {
                XmlSerializer serializer = new XmlSerializer(addonVersionInfo.GetType());
                using (StreamReader reader = new StreamReader(pFilename))
                {
                    addonVersionInfo = (AddonVersionInfo)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing AddonVersionInfo: {exception.Message} - {exception.InnerException?.Message}";
                addonVersionInfo = null;
            }

            return addonVersionInfo;
        }


        public static AddonVersionInfo LoadFromString(string pText, out string pErrorText)
        {

            pErrorText = null;
            if (string.IsNullOrEmpty(pText = pText?.Trim()))
            {
                pErrorText = "AddonVersionInfo.LoadFromString(): No input text";
                return null;
            }

            AddonVersionInfo addonVersionInfo = new AddonVersionInfo();

            try
            {
                XmlSerializer serializer = new XmlSerializer(addonVersionInfo.GetType());
                using (TextReader reader = new StringReader(pText))
                {
                    addonVersionInfo = (AddonVersionInfo)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing AddonVersionInfo: {exception.Message} - {exception.InnerException?.Message}";
                addonVersionInfo = null;
            }

            return addonVersionInfo;
        }



    }
}