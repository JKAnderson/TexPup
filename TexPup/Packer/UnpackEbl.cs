using System.Xml.Serialization;

namespace TexPup
{
    public class UnpackEbl
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public bool Optional { get; set; }

        public string Key { get; set; }
    }
}
