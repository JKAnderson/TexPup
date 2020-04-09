using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TexPup
{
    public class UnpackFilter
    {
        [XmlIgnore]
        public bool Enabled { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlText]
        public string Pattern { get; set; }

        public UnpackFilter()
        {
            Enabled = true;
        }

        public static bool RunFilters(string path, IEnumerable<UnpackFilter> filters)
        {
            foreach (UnpackFilter filter in filters)
            {
                if (Regex.IsMatch(path, filter.Pattern))
                    return filter.Enabled;
            }
            return false;
        }
    }
}
