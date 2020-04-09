using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TexPup
{
    class UnpackReport
    {
        public List<UnpackReportFile> Files { get; set; }

        public UnpackReport()
        {
            Files = new List<UnpackReportFile>();
        }

        public string Write()
        {
            var sb = new StringBuilder();
            foreach (UnpackReportFile file in Files.OrderBy(f => f.Name).GroupBy(f => f.Name).Select(g => g.First()))
            {
                if (sb.Length > 0)
                    sb.AppendLine();
                sb.AppendLine(file.Write());
            }
            return sb.ToString();
        }
    }
}
