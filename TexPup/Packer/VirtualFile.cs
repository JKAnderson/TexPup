using System;
using System.Collections.Generic;
using System.Text;

namespace TexPup
{
    abstract class VirtualFile
    {
        public string Path { get; }

        public VirtualFile(string path)
        {
            Path = path;
        }

        public abstract byte[] Load();
    }
}
