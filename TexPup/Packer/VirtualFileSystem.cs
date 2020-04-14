using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TexPup
{
    class VirtualFileSystem : IDisposable
    {
        public Dictionary<string, VirtualFile> Files { get; }

        private Queue<Stream> DataStreams { get; }

        public VirtualFileSystem()
        {
            Files = new Dictionary<string, VirtualFile>();
            DataStreams = new Queue<Stream>();
        }

        public void LoadFiles(string directory)
        {
            directory = directory.TrimEnd('\\');
            if (Directory.Exists(directory))
            {
                foreach (string realPath in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
                {
                    string path = realPath.Substring(directory.Length + 1);
                    var file = new DiskFile(path, realPath);
                    Files[path.ToLower()] = file;
                }
            }
        }

        public void LoadEbls(UnpackGame game)
        {
            var archiveDict = new EblDictionary(File.ReadAllText($@"res\{game.Config.DictionaryPath}"));
            foreach (UnpackEbl ebl in game.Config.Ebls)
            {
                LoadEbl(ebl, game.Settings.GameDirectory, archiveDict, game.Config.BHD5Game);
            }
        }

        private void LoadEbl(UnpackEbl ebl, string gameDirectory, EblDictionary eblDict, BHD5.Game game)
        {
            gameDirectory = gameDirectory.TrimEnd('\\');
            string bhdPath = $@"{gameDirectory}\{ebl.Name}.bhd";
            string bdtPath = $@"{gameDirectory}\{ebl.Name}.bdt";

            if (!ebl.Optional)
            {
                if (!File.Exists(bhdPath))
                    throw new FileNotFoundException($"Mandatory header file not found:\n{bhdPath}");
                else if (!File.Exists(bdtPath))
                    throw new FileNotFoundException($"Mandatory data file not found:\n{bdtPath}");
            }
            else if (!File.Exists(bhdPath) || !File.Exists(bdtPath))
            {
                return;
            }

            BHD5 bhd = ReadBHD5(bhdPath, ebl.Key, game);
            FileStream bdtStream = File.OpenRead(bdtPath);
            DataStreams.Enqueue(bdtStream);

            foreach (BHD5.Bucket bucket in bhd.Buckets)
            {
                foreach (BHD5.FileHeader header in bucket)
                {
                    if (eblDict.GetPath(header.FileNameHash, out string path))
                    {
                        path = path.Replace('/', '\\').TrimStart('\\');
                        var file = new EblFile(path, header, bdtStream);
                        Files[path.ToLower()] = file;
                    }
                }
            }
        }

        private static BHD5 ReadBHD5(string path, string key, BHD5.Game game)
        {
            using (FileStream fs = File.OpenRead(path))
            {
                var magic = new byte[4];
                fs.Read(magic, 0, 4);
                fs.Seek(0, SeekOrigin.Begin);
                bool encrypted = Encoding.ASCII.GetString(magic) != "BHD5";

                if (encrypted)
                {
                    using (var ms = TPUtil.DecryptRsa(fs, key))
                    {
                        return BHD5.Read(ms, game);
                    }
                }
                else
                {
                    return BHD5.Read(fs, game);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    while (DataStreams.Count > 0)
                    {
                        DataStreams.Dequeue().Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~VirtualFileSystem()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
