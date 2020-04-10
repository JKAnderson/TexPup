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
            foreach (UnpackReportFile file in Files.OrderBy(f => f.Name.ToLower()).GroupBy(f => f.Name.ToLower()).Select(g => g.Last()))
            {
                if (sb.Length > 0)
                    sb.AppendLine();
                sb.AppendLine(file.Write());
            }
            return sb.ToString();
        }
    }
}
