using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using SoulsFormats;
using System.Collections.Generic;
using System.IO;

namespace TexPup
{
    class TPUtil
    {
        // 128 MB
        public const long MAX_BASE_MEMORY = 128 * 1024 * 1024;

        public static string ReadModEngineDirectory(string gameDirectory)
        {
            gameDirectory = gameDirectory.TrimEnd('\\');
            string iniPath = $@"{gameDirectory}\modengine.ini";
            var parser = new IniParser.FileIniDataParser();
            IniParser.Model.IniData ini = parser.ReadFile(iniPath);
            string modDirectory = ini["files"]["modOverrideDirectory"].Trim('"').TrimStart('\\');
            return $@"{gameDirectory}\{modDirectory}";
        }

        public static void CopyOodle(GameType type, string gameDirectory)
        {
            if (type == GameType.Sekiro
                && File.Exists(Path.Combine(gameDirectory, "oo2core_6_win64.dll"))
                && !File.Exists(@"lib\oo2core_6_win64.dll"))
            {
                File.Copy(Path.Combine(gameDirectory, "oo2core_6_win64.dll"),
                    @"lib\oo2core_6_win64.dll");
            }
        }

        private static HashSet<string> ValidExtensions = new HashSet<string>
        {
            ".chrbnd",
            ".ffxbnd",
            ".fgbnd",
            ".objbnd",
            ".partsbnd",
            ".texbnd",
            ".tpf",
            ".tpfbdt",
            //".tpfbhd",
        };

        public static bool HasValidExtension(string path)
        {
            string ext = SFUtil.GetRealExtension(path);
            return ValidExtensions.Contains(ext.ToLower());
        }

        public static string GetRelativeOutputDir(string path)
        {
            return $@"{Path.GetDirectoryName(path)}\{SFUtil.GetRealFileName(path)}";
        }

        public static MemoryStream DecryptRsa(Stream inputStream, string pemKey)
        {
            AsymmetricKeyParameter keyParameter = ReadPem(pemKey);
            var engine = new RsaEngine();
            engine.Init(false, keyParameter);

            var outputStream = new MemoryStream();
            int inputBlockSize = engine.GetInputBlockSize();
            int outputBlockSize = engine.GetOutputBlockSize();
            byte[] inputBlock = new byte[inputBlockSize];
            while (inputStream.Read(inputBlock, 0, inputBlock.Length) > 0)
            {
                byte[] outputBlock = engine.ProcessBlock(inputBlock, 0, inputBlockSize);

                int requiredPadding = outputBlockSize - outputBlock.Length;
                if (requiredPadding > 0)
                {
                    byte[] paddedOutputBlock = new byte[outputBlockSize];
                    outputBlock.CopyTo(paddedOutputBlock, requiredPadding);
                    outputBlock = paddedOutputBlock;
                }

                outputStream.Write(outputBlock, 0, outputBlock.Length);
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }

        private static AsymmetricKeyParameter ReadPem(string pemKey)
        {
            using (var sr = new StringReader(pemKey))
            {
                var pemReader = new PemReader(sr);
                return (AsymmetricKeyParameter)pemReader.ReadObject();
            }
        }
    }
}
