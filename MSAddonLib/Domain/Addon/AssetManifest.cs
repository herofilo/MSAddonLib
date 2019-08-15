using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MSAddonLib.Domain.Addon
{
    public class AssetManifest
    {
        [XmlArray("propModels")]
        [XmlArrayItem("Prop")]
        public List<PropModelItem> PropModels { get; set; } = new List<PropModelItem>();

        [XmlArray("bodyModels")]
        [XmlArrayItem("Body")]
        public List<BodyModelItem> BodyModels { get; set; } = new List<BodyModelItem>();



        public AssetManifest()
        {


        }


        public static AssetManifest Load(string pFilename, out string pErrorText)
        {
            pErrorText = null;
            if (!File.Exists(pFilename))
            {
                pErrorText = "AssetManifest.Load(): File not found";
                return null;
            }

            AssetManifest assetManifest = new AssetManifest();

            try
            {
                XmlSerializer serializer = new XmlSerializer(assetManifest.GetType());
                using (StreamReader reader = new StreamReader(pFilename))
                {
                    assetManifest = (AssetManifest)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing AssetManifest: {exception.Message} - {exception.InnerException?.Message}";
                assetManifest = null;
            }

            return assetManifest;
        }



        public static AssetManifest LoadFromString(string pText, out string pErrorText)
        {
            pErrorText = null;
            if (string.IsNullOrEmpty(pText = pText?.Trim()))
            {
                pErrorText = "AssetManifest.LoadFromString(): No input text";
                return null;
            }

            AssetManifest assetManifest = new AssetManifest();

            try
            {
                XmlSerializer serializer = new XmlSerializer(assetManifest.GetType());
                using (TextReader reader = new StringReader(pText))
                {
                    assetManifest = (AssetManifest)serializer.Deserialize(reader);
                    reader.Close();
                }
            }
            catch (Exception exception)
            {
                pErrorText = $"EXCEPTION while deserializing AssetManifest: {exception.Message} - {exception.InnerException?.Message}";
                assetManifest = null;
            }

            return assetManifest;
        }


        // ------------------------------------------------------------------------------------------------------------------------------------------------
    }
}