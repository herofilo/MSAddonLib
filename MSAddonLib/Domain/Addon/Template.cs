using System.Xml.Serialization;

namespace MSAddonLib.Domain.Addon
{
    public sealed class Template
    {
        [XmlElement("name")]
        public string Name { get; set; }
    }
}