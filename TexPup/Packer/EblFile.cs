using SoulsFormats;
using System.IO;

namespace TexPup
{
    class EblFile : VirtualFile
    {
        private BHD5.FileHeader FileHeader { get; }

        private FileStream DataStream { get; }

        public EblFile(string path, BHD5.FileHeader header, FileStream data) : base(path)
        {
            FileHeader = header;
            DataStream = data;
        }

        public override byte[] Load()
        {
            lock (DataStream)
            {
                return FileHeader.ReadFile(DataStream);
            }
        }
    }
}
