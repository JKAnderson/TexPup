using System.IO;

namespace TexPup
{
    class DiskFile : VirtualFile
    {
        private string RealPath { get; }

        public DiskFile(string path, string realPath) : base(path)
        {
            RealPath = realPath;
        }

        public override byte[] Load()
        {
            return File.ReadAllBytes(RealPath);
        }
    }
}
