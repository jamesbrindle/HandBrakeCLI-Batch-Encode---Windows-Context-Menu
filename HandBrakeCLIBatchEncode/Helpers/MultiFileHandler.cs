using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HandBrakeCLIBatchEncode
{
    public class MultiFileHandler
    {
        private static readonly string _tempRoot = Path.Combine(Path.GetTempPath(), ".hbcbe_temp");
        private static readonly string _busyFile = Path.Combine(_tempRoot,  "busy");

        public static void SetBusyFlag()
        {
            File.Create(_busyFile).Dispose();
        }

        public static bool IsBusy()
        {
            return File.Exists(_busyFile);
        }

        public static void AddFile(string path)
        {
            if (!Directory.Exists(_tempRoot))
                Directory.CreateDirectory(_tempRoot);

            File.WriteAllText(_tempRoot + "\\" + Guid.NewGuid().ToString(), path);
        }

        public static List<string> GetFilesInSession()
        {
            var inputFiles = new List<string>();

            string[] files = Directory.GetFiles(_tempRoot).Where(f => new FileInfo(f).CreationTime > DateTime.Now.AddSeconds(-3.2) && new FileInfo(f).Name != "busy").ToArray();

            foreach (string file in files)
                inputFiles.Add(File.ReadAllText(file).Replace("\n", "").Replace("\r", "").Trim());

            return inputFiles;
        }

        public static void ResetHandler()
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
