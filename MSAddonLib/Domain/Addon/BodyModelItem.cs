using System.Collections.Generic;
using System.Xml.Serialization;

namespace MSAddonLib.Domain.Addon
{
    public class BodyModelItem
    {
        //   <templates/>

        [XmlArray("parts")]
        [XmlArrayItem("BodyPart")]
        public List<BodyPart> Parts { get; set; } = new List<BodyPart>();

        [XmlArray("meshes")]
        [XmlArrayItem("string")]
        public List<string> Meshes { get; set; } = new List<string>();

        [XmlArray("animations")]
        [XmlArrayItem("string")]
        public List<string> Animations { get; set; } = new List<string>();

        /// <summary>
        /// Name of the puppet/character/actor
        /// </summary>
        [XmlElement("name")]
        public string PuppetName { get; set; }

    }
}