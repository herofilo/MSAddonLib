using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace MSAddonLib.Domain.AssetFiles
{
    [System.Xml.Serialization.XmlRootAttribute("mscope.cuttingroom.assets.AssetInfo", Namespace = "", IsNullable = false)]
    public sealed class AssetInfo
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }


        [XmlElement("tagList")]
        public string TagList { get; set; }


        // --------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Creates an AssetInfo instance from an existent description file
        /// </summary>
        /// <param name="pFilename">Path to the AssetInfo file</param>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>AssetInfo instance, or null if error</returns>
        public static AssetInfo Load(string pFilename, out string pErrorText)
        {
            pErrorText = null;
            if (!File.Exists(pFilename))
            {
                pErrorText = "AssetInfo.Load(): File not found";
                return null;
            }

            AssetInfo assetInfo = new AssetInfo();

            try
            {
                XmlSerializer serializer = new XmlSerializer(assetInfo.GetType());
                using (StreamReader reader = new StreamReader(pFilename))
                {
                    assetInfo = (AssetInfo)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing AssetInfo: {exception.Message} - {exception.InnerException?.Message}";
                assetInfo = null;
            }

            return assetInfo;
        }




        /// <summary>
        /// Creates a AssetInfo instance from a XML string
        /// </summary>
        /// <param name="pText">XML string</param>
        /// <param name="pErrorText">Text of error, if any</param>
        /// <returns>AssetInfo instance, or null if error</returns>
        public static AssetInfo LoadFromString(string pText, out string pErrorText)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pText = pText?.Trim()))
            {
                pErrorText = "AssetInfo.LoadFromString(): No input text";
                return null;
            }

            AssetInfo assetInfo = new AssetInfo();

            try
            {
                XmlSerializer serializer = new XmlSerializer(assetInfo.GetType());
                using (TextReader reader = new StringReader(pText))
                {
                    assetInfo = (AssetInfo)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing AssetInfo: {exception.Message} - {exception.InnerException?.Message}";
                assetInfo = null;
            }

            return assetInfo;
        }


    }
}
