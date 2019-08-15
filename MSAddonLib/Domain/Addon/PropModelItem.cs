using System.Collections.Generic;
using System.Xml.Serialization;

namespace MSAddonLib.Domain.Addon
{
    public class PropModelItem
    {
        // <tags/>
        [XmlArray("templates")]
        [XmlArrayItem("Entry")]
        public List<Template> Templates { get; set; } = new List<Template>();

        [XmlArray("parts")]
        public List<ModelPart> Parts { get; set; } = new List<ModelPart>();

        [XmlElement("skeleton")]
        public string Skeleton;

        [XmlArray("meshes")]
        [XmlArrayItem("string")]
        public List<string> Meshes { get; set; } = new List<string>();

        // <animations/>

        [XmlArray("animations")]
        [XmlArrayItem("string")]
        public List<string> Animations { get; set; } = new List<string>();

        [XmlElement("name")]
        public string Name;

    }
}