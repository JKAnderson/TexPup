using SoulsFormats;
using System.Collections.Generic;
using System.IO;
using TeximpNet.DDS;

namespace TexPup
{
    class UnpackReportFile
    {
        public string Name { get; set; }

        public DXGIFormat Format { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public UnpackReportFile(TPF.Texture tex)
        {
            using (var ms = new MemoryStream(tex.Bytes))
            using (var dds = DDSFile.Read(ms))
            {
                Name = tex.Name;
                Format = dds?.Format ?? DXGIFormat.Unknown;
                Width = dds?.MipChains[0][0].Width ?? -1;
                Height = dds?.MipChains[0][0].Height ?? -1;
            }
        }

        public string Write()
        {
            if (Width != -1 && Height != -1)
                return $"File:   {Name}.dds\r\nFormat: {PrintDXGIFormat(Format)}\r\nSize:   {Width}x{Height}";
            else
                return $"File:   {Name}.dds\r\nFormat: {PrintDXGIFormat(Format)}\r\nSize:   Unknown";
        }

        private static Dictionary<DXGIFormat, string> DXGIFormatOverride = new Dictionary<DXGIFormat, string>()
        {
            [DXGIFormat.BC1_UNorm] = "DXT1",
            [DXGIFormat.BC2_UNorm] = "DXT3",
            [DXGIFormat.BC3_UNorm] = "DXT5",
            [DXGIFormat.Opaque_420] = "420_OPAQUE",
        };

        public static string PrintDXGIFormat(DXGIFormat format)
        {
            if (DXGIFormatOverride.ContainsKey(format))
                return DXGIFormatOverride[format];
            else
                return format.ToString().ToUpper();
        }
    }
}
