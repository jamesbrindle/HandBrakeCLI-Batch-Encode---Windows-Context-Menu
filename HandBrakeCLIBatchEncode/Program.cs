using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace BatchEncode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Out.WriteLine("");
            Console.Out.WriteLine("       ____.__________ ");
            Console.Out.WriteLine("      |    |\\______   \\");
            Console.Out.WriteLine("      |    | |    | _ /");
            Console.Out.WriteLine(" /\\__ |    | |    |   \\");
            Console.Out.WriteLine(" \\________ | | ______ /");
            Console.Out.WriteLine("                    \\/");
            Console.Out.WriteLine("");

            EncodeVideos(args[0], args[1], args[2]);
        }


        public static void EncodeVideos(string root, string presetPath, string audioByteRate)
        {
            string[] filesList = Directory.GetFiles(root);

            foreach (string file in filesList)
            {
                FileInfo info = new FileInfo(file);

                if (info.Extension.ToLower().In(".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv", ".mpv", ".mpeg", ".m4v", ".3gp", ".3g2", ".f4v", ".f4a", ".f4b"))
                {
                    string newFileName = info.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(file) + "_" + info.Extension;

                    File.Move(file, newFileName);
                    EncodeVideo(newFileName, file, presetPath, audioByteRate);
                }
            }
        }

        private static bool EncodeVideo(string inputFile, string outputFile, string presetPath, string audioByteRate)
        {
            string arguments = @"-i """ + inputFile + @""" -o """ + outputFile + @""" --preset-import-file """ + presetPath  + @""" ""TV Shows - Kids"" -B " + audioByteRate;

            using (Process proc = new Process())
            {
                Console.Out.Write("Encoding video: " + outputFile);

                proc.StartInfo.FileName = @"C:\Utilities\HandBrakeCLI\HandBrakeCLI.exe";
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.RedirectStandardOutput = false;
                proc.Start();
                proc.WaitForExit();

                Console.Out.Write(".. Done\n");
            }              

            try
            {
                File.Delete(inputFile);
            }
            catch
            {
                Thread.Sleep(1000);

                try
                {
                    File.Delete(inputFile);
                }
                catch
                {

                }
            }

            return true;
        }
    }

    public static class SystemExtensions
    {
        public static bool In<T>(this T needle, params T[] haystack)
        {
            return haystack.Contains(needle);
        }
    }
}