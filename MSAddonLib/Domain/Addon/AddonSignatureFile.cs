using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MSAddonLib.Domain.Addon
{

    /// <summary>
    /// Info in the Signature file of the addon (.Addon)
    /// </summary>
    [XmlRoot("addon")]
    public class AddonSignatureFile
    {
        /// <summary>
        /// DescriptionFilePath of the addon
        /// </summary>
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Description of the addon
        /// </summary>
        [XmlElement("description")]
        public string Description { get; set; }

        /// <summary>
        /// Flag indicating is a free addon
        /// </summary>
        [XmlElement("free")]
        public bool Free { get; set; } = false;

        /// <summary>
        /// List of files referred to in the signature file
        /// </summary>
        [XmlArray("files")]
        [XmlArrayItem("string")]
        public List<string> Files { get; set; } = new List<string>();

        /// <summary>
        /// DescriptionFilePath of the account of the publisher
        /// </summary>
        public string Publisher { get; set; }


        // .......................................................................................


        /// <summary>
        /// Loads the signature file
        /// </summary>
        /// <param name="pFileName">Path to the signature file</param>
        /// <returns>Information contained</returns>
        public static AddonSignatureFile Load(string pFileName)
        {
            if (!File.Exists(pFileName))
                throw new Exception("No Addon Signature file (.AddOn)");

            byte[] addOnFileContents = File.ReadAllBytes(pFileName);

            return Load(addOnFileContents);
        }


        /// <summary>
        /// Loads the signature file
        /// </summary>
        /// <param name="pContent">Binary contents of the file</param>
        /// <returns>Information contained</returns>
        public static AddonSignatureFile Load(byte[] pContent)
        {
            byte[] lengthBytes = pContent.Take(4).ToArray();
            int length = BitConverter.ToInt32(lengthBytes.Reverse().ToArray(), 0);

            string developer = Encoding.Default.GetString(pContent, 4, length);

            // BitConverter.T .ToString(addOnFileContents, 4, length);
            byte[] startBytes = Encoding.ASCII.GetBytes("<addon>");

            int position = -1;
            for (int idx = 4 + length; idx < (pContent.Length - startBytes.Length); ++idx)
            {
                if (pContent[idx] == '<')
                {
                    position = idx;
                    for (int srcIdx = idx, destIdx = 0, count = 0; count < startBytes.Length; ++srcIdx, ++destIdx, ++count)
                        if (pContent[srcIdx] != startBytes[destIdx])
                        {
                            position = -1;
                            break;
                        }
                    if (position > 0)
                        break;
                }
            }

            if (position < 0)
            {
                // throw new Exception("Invalid format .AddOn file");
                return null;
            }


            string addonXmlText = Encoding.Default.GetString(pContent, position,
                pContent.Length - position);

            AddonSignatureFile addonSignatureFile;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AddonSignatureFile));
                using (StringReader reader = new StringReader(addonXmlText))
                {
                    addonSignatureFile = (AddonSignatureFile)serializer.Deserialize(reader);
                    reader.Close();
                }
                addonSignatureFile.Publisher = developer;
            }
            catch (Exception exception)
            {
                addonSignatureFile = null;
            }

            return addonSignatureFile;
        }




    }
}
