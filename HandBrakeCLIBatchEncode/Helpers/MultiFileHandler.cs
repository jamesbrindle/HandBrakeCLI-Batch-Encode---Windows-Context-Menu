using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace HandBrakeCLIBatchEncode
{
    internal class MultiFileHandler
    {
        private static readonly string _tempRoot = Path.Combine(Path.GetTempPath(), ".hbcbe_temp");
        private static readonly string _busyFile = Path.Combine(_tempRoot, "busy");

        internal static bool IsBusy
        {
            get
            {
                if (!File.Exists(_busyFile))
                    return false;

                else
                {
                    try
                    {
                        return new FileInfo(_busyFile).LastWriteTime > DateTime.Now.AddSeconds(-5);
                    }
                    catch
                    {
                        Thread.Sleep(100);
                        return IsBusy;
                    }
                }
            }
        }

        internal static void SetBusyFlag()
        {
            try
            {
                File.Create(_busyFile).Dispose();
            }
            catch
            {
                Thread.Sleep(100);
                SetBusyFlag();
            }
        }

        internal static void AddFile(string path)
        {
            SetBusyFlag();

            if (!Directory.Exists(_tempRoot))
                Directory.CreateDirectory(_tempRoot);

            File.WriteAllText(_tempRoot + "\\" + Guid.NewGuid().ToString(), path);
        }

        internal static List<string> GetFilesInSession()
        {
            var inputFiles = new List<string>();

            string[] files = Directory.GetFiles(_tempRoot).Where(f => new FileInfo(f).Name != "busy").ToArray();

            foreach (string file in files)
                inputFiles.Add(File.ReadAllText(file).Replace("\n", "").Replace("\r", "").Trim());

            return inputFiles;
        }

        internal static void ResetHandler()
        {
            DirectoryInfo dir = new DirectoryInfo(_tempRoot);

            foreach (FileInfo fi in dir.GetFiles())
            {
                try
                {
                    fi.Delete();
                }
                catch { }
            }
        }
    }
}
