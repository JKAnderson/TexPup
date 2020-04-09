using SoulsFormats;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TexPup
{
    class TexUnpacker : IDisposable
    {
        private UnpackGame Game { get; }

        private VirtualFileSystem VirtualFS { get; }

        private long BaseMemoryCommitted;

        private int FilesCompleted;

        public TexUnpacker(UnpackGame game)
        {
            Game = game;
            VirtualFS = new VirtualFileSystem();
            BaseMemoryCommitted = 0;
            FilesCompleted = 0;
        }

        public void Unpack(IProgress<ProgressReport> progress, IProgress<string> errors, CancellationToken cancelToken)
        {
            TPUtil.CopyOodle(Game.Type, Game.Settings.GameDirectory);

            progress.Report(new ProgressReport(0, "Scanning files..."));
            if (Game.Settings.UnpackSourceEbls)
                VirtualFS.LoadEbls(Game);
            if (Game.Settings.UnpackSourceUXM)
                VirtualFS.LoadFiles(Game.Settings.GameDirectory);
            if (Game.Settings.UnpackSourceModEngine)
                VirtualFS.LoadFiles(TPUtil.ReadModEngineDirectory(Game.Settings.GameDirectory));

            string[] paths = VirtualFS.Files.Keys
                .Where(k => TPUtil.HasValidExtension(k)
                    && UnpackFilter.RunFilters(k, Game.Config.MapFilters.Concat(Game.Config.Filters)))
                .OrderBy(k => k).ToArray();

            progress.Report(new ProgressReport(0, $"Unpacking files... (0/{paths.Length})"));
            var tasks = new Task<int>[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                VirtualFile vf = VirtualFS.Files[paths[i]];
                tasks[i] = Task.Run(() => UnpackVirtualFile(vf, cancelToken));
                tasks[i].ContinueWith(task =>
                {
                    Interlocked.Increment(ref FilesCompleted);
                    progress.Report(new ProgressReport((float)FilesCompleted / paths.Length,
                        $"Unpacking files... ({FilesCompleted}/{paths.Length})"));

                    if (task.IsFaulted)
                        errors.Report($"Error in file \"{vf.Path}\"\n{task.Exception}");
                });
            }

            Task.WaitAll(tasks);
            int textureCount = tasks.Where(t => t.IsCompleted).Sum(t => t.Result);
            if (cancelToken.IsCancellationRequested)
                progress.Report(new ProgressReport(1, "Unpacking aborted successfully."));
            else
                progress.Report(new ProgressReport(1, $"Finished unpacking {textureCount} textures from {paths.Length} files!"));
        }

        private int UnpackVirtualFile(VirtualFile vf, CancellationToken cancelToken)
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
                if (DCX.Is(bytes))
                    bytes = DCX.Decompress(bytes);

                int textureCount;
                var report = new UnpackReport();
                if (TPF.IsRead(bytes, out TPF tpf))
                {
                    textureCount = UnpackTPF(tpf, relOutputDir, report);
                }
                else if (BND4.IsRead(bytes, out BND4 bnd))
                {
                    textureCount = UnpackBinder(bnd, relOutputDir, report, cancelToken);
                }
                else if (BXF4.IsBDT(bytes))
                {
                    string ext = Path.GetExtension(vf.Path).Replace("bdt", "bhd");
                    string bhdPath = Path.ChangeExtension(vf.Path, ext);
                    VirtualFile vfHeader = VirtualFS.Files[bhdPath];
                    byte[] bhdBytes = vfHeader.Load();
                    var bxf = BXF4.Read(bhdBytes, bytes);
                    textureCount = UnpackBinder(bxf, relOutputDir, report, cancelToken);
                }
                else
                {
                    throw new NotSupportedException("Unknown file type.");
                }

                if (report.Files.Count > 0)
                {
                    File.WriteAllText($@"{Game.Settings.UnpackDirectory.TrimEnd('\\')}\{relOutputDir}\_report.txt", report.Write());
                }
                return textureCount;
            }
            finally
            {
                Interlocked.Add(ref BaseMemoryCommitted, -baseMemory);
            }
        }

        private int UnpackTPF(TPF tpf, string relOutputDir, UnpackReport report)
        {
            foreach (TPF.Texture texture in tpf)
            {
                try
                {
                    string dir = $@"{Game.Settings.UnpackDirectory.TrimEnd('\\')}\{relOutputDir}";
                    string path = $@"{dir}\{texture.Name}.dds";

                    Directory.CreateDirectory(dir);
                    File.WriteAllBytes(path, texture.Bytes);
                    report.Files.Add(new UnpackReportFile(texture));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error in texture \"{texture.Name}\"", ex);
                }
            }
            return tpf.Textures.Count;
        }

        private int UnpackBinder(IBinder binder, string relOutputDir, UnpackReport report, CancellationToken cancelToken)
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
                        if (DCX.Is(bytes))
                            bytes = DCX.Decompress(bytes);

                        if (TPF.IsRead(bytes, out TPF tpf))
                            textureCount += UnpackTPF(tpf, relOutputDir, report);
                        else if (BND4.IsRead(bytes, out BND4 bnd))
                            textureCount += UnpackBinder(bnd, relOutputDir, report, cancelToken);
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
