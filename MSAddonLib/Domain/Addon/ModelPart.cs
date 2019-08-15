using System.Xml.Serialization;

namespace MSAddonLib.Domain.Addon
{
    public sealed class ModelPart
    {
        [XmlElement("slot")]
        public int Slot { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }
    }
}