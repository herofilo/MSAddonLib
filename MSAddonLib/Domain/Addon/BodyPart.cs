using System.Collections.Generic;
using System.Xml.Serialization;

namespace MSAddonLib.Domain.Addon
{
    public sealed class BodyPart
    {
        [XmlElement("partsCovered")]
        public string PartsCovered { get; set; }

        [XmlElement("instanceClass")]
        public string InstanceClass { get; set; }

        [XmlArray("tags")]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Path to the bodypart file
        /// </summary>
        [XmlElement("name")]
        public string DescriptionFilePath { get; set; }

    }
}