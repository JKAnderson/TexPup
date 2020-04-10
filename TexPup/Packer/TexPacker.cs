using SoulsFormats;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TexPup
{
    class TexPacker : IDisposable
    {
        private UnpackGame Game { get; }

        private string OutputDirectory { get; }

        private VirtualFileSystem VirtualFS { get; }

        private long BaseMemoryCommitted;

        private int FilesCompleted;

        public TexPacker(UnpackGame game)
        {
            Game = game;
            VirtualFS = new VirtualFileSystem();
            BaseMemoryCommitted = 0;
            FilesCompleted = 0;

            if (game.Settings.PackMode == PackMode.ModEngine)
                OutputDirectory = TPUtil.ReadModEngineDirectory(Game.Settings.GameDirectory);
            else if (game.Settings.PackMode == PackMode.UXM)
                OutputDirectory = Game.Settings.GameDirectory;
            else
                throw new NotImplementedException($"Unknown pack mode {Game.Settings.PackMode}");
            OutputDirectory = OutputDirectory.TrimEnd('\\');
        }

        public void Pack(IProgress<ProgressReport> progress, IProgress<string> errors, CancellationToken cancelToken)
        {
            TPUtil.CopyOodle(Game.Type, Game.Settings.GameDirectory);

            progress.Report(new ProgressReport(0, "Scanning files..."));
            VirtualFS.LoadEbls(Game);
            if (Game.Settings.PackMode == PackMode.ModEngine)
                VirtualFS.LoadFiles(TPUtil.ReadModEngineDirectory(Game.Settings.GameDirectory));
            else if (Game.Settings.PackMode == PackMode.UXM)
                VirtualFS.LoadFiles(Game.Settings.GameDirectory);

            string[] paths = VirtualFS.Files.Keys.Where(k =>
                {
                    string dir = $@"{Game.Settings.PackDirectory.TrimEnd('\\')}\{TPUtil.GetRelativeOutputDir(k)}";
                    return TPUtil.HasValidExtension(k) && Directory.Exists(dir) && Directory.GetFiles(dir, "*.dds").Length > 0;
                })
                .OrderBy(k => k).ToArray();

            progress.Report(new ProgressReport(0, $"Checking files... (0/{paths.Length})"));
            var tasks = new Task<int>[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                VirtualFile vf = VirtualFS.Files[paths[i]];
                tasks[i] = Task.Run(() => PackVirtualFile(vf, cancelToken));
                tasks[i].ContinueWith(task =>
                {
                    Interlocked.Increment(ref FilesCompleted);
                    progress.Report(new ProgressReport((float)FilesCompleted / paths.Length,
                        $"Checking files... ({FilesCompleted}/{paths.Length})"));

                    if (task.IsFaulted)
                        errors.Report($"Error in file \"{vf.Path}\"\n{task.Exception}");
                });
            }

            Task.WaitAll(tasks);
            int textureCount = tasks.Where(t => t.IsCompleted).Sum(t => t.Result);
            if (cancelToken.IsCancellationRequested)
                progress.Report(new ProgressReport(1, "Packing aborted successfully."));
            else
                progress.Report(new ProgressReport(1, $"Finished packing {textureCount} textures in {paths.Length} files!"));
        }

        private int PackVirtualFile(VirtualFile vf, CancellationToken cancelToken)
        {
            byte[] bytes;
            long baseMemory;
            lock (this)
            {
                while (BaseMemoryCommitted > TPUtil.MAX_BASE_MEMORY)
                    Thread.Sleep(10);

                if (cancelToken.IsCancellationRequested)
                    return 0;

                bytes = vf.Load();
                baseMemory = bytes.Length;
                Interlocked.Add(ref BaseMemoryCommitted, baseMemory);
            }

            try
            {
                string relOutputDir = TPUtil.GetRelativeOutputDir(vf.Path);
                DCX.Type dcxType = DCX.Type.None;
                if (DCX.Is(bytes))
                    bytes = DCX.Decompress(bytes, out dcxType);

                int textureCount;
                if (TPF.IsRead(bytes, out TPF tpf))
                {
                    textureCount = PackTPF(tpf, relOutputDir);
                    if (textureCount > 0)
                        tpf.Write($@"{OutputDirectory}\{vf.Path}", dcxType);
                }
                else if (BND4.IsRead(bytes, out BND4 bnd))
                {
                    textureCount = PackBinder(bnd, relOutputDir, cancelToken);
                    if (textureCount > 0)
                        bnd.Write($@"{OutputDirectory}\{vf.Path}", dcxType);
                }
                else if (BXF4.IsBDT(bytes))
                {
                    string ext = Path.GetExtension(vf.Path).Replace("bdt", "bhd");
                    string bhdPath = Path.ChangeExtension(vf.Path, ext);
                    VirtualFile vfHeader = VirtualFS.Files[bhdPath];
                    byte[] bhdBytes = vfHeader.Load();

                    var bxf = BXF4.Read(bhdBytes, bytes);
                    textureCount = PackBinder(bxf, relOutputDir, cancelToken);
                    if (textureCount > 0)
                        bxf.Write($@"{OutputDirectory}\{vfHeader.Path}", $@"{OutputDirectory}\{vf.Path}");
                }
                else
                {
                    throw new NotSupportedException("Unknown file type.");
                }
                return textureCount;
            }
            finally
            {
                Interlocked.Add(ref BaseMemoryCommitted, -baseMemory);
            }
        }

        private int PackTPF(TPF tpf, string relOutputDir)
        {
            if (tpf.Platform != TPF.TPFPlatform.PC)
                return 0;

            int textureCount = 0;
            foreach (TPF.Texture texture in tpf)
            {
                try
                {
                    string overridePath = $@"{Game.Settings.PackDirectory.TrimEnd('\\')}\{relOutputDir}\{texture.Name}.dds";
                    if (File.Exists(overridePath))
                    {
                        texture.Bytes = File.ReadAllBytes(overridePath);
                        textureCount++;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error in texture \"{texture.Name}\"", ex);
                }
            }
            return textureCount;
        }

        private int PackBinder(IBinder binder, string relOutputDir, CancellationToken cancelToken)
        {
            int textureCount = 0;
            foreach (BinderFile file in binder.Files)
            {
                if (cancelToken.IsCancellationRequested)
                    return textureCount;

                if (TPUtil.HasValidExtension(file.Name))
                {
                    try
                    {
                        byte[] bytes = file.Bytes;
                        DCX.Type dcxType = DCX.Type.None;
                        if (DCX.Is(bytes))
                            bytes = DCX.Decompress(bytes, out dcxType);

                        if (TPF.IsRead(bytes, out TPF tpf))
                        {
                            int thisTextureCount = PackTPF(tpf, relOutputDir);
                            if (thisTextureCount > 0)
                            {
                                file.Bytes = tpf.Write(dcxType);
                            }
                            textureCount += thisTextureCount;
                        }
                        else if (BND4.IsRead(bytes, out BND4 bnd))
                        {
                            int thisTextureCount = PackBinder(bnd, relOutputDir, cancelToken);
                            if (thisTextureCount > 0)
                            {
                                file.Bytes = bnd.Write(dcxType);
                            }
                            textureCount += thisTextureCount;
                        }
                        else
                            throw new NotSupportedException("Unknown file type.");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error in binder file \"{file.Name}\"", ex);
                    }
                }
            }
            return textureCount;
        }

        public void Dispose()
        {
            ((IDisposable)VirtualFS).Dispose();
        }
    }
}
