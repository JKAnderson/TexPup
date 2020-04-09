using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TexPup
{
    public class GameConfig
    {
        [XmlAttribute]
        public string Name { get; set; }

        public string DictionaryPath { get; set; }

        public BHD5.Game BHD5Game { get; set; }

        [XmlArrayItem(ElementName = "Filter")]
        public UnpackFilter[] Filters { get; set; }

        [XmlArrayItem(ElementName = "Filter")]
        public UnpackFilter[] MapFilters { get; set; }

        [XmlArrayItem(ElementName = "Ebl")]
        public UnpackEbl[] Ebls { get; set; }

        public static GameConfig XmlDeserialize(string text)
        {
            var serializer = new XmlSerializer(typeof(GameConfig));
            using (var sw = new StringReader(text))
                return (GameConfig)serializer.Deserialize(sw);
        }
    }
}
