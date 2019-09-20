using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace HandBrakeCLIBatchEncode
{
    public class IntegrityChecker : BatchEncoder
    {
        public void IntegrityCheckVideos(string rootFileOrCombined)
        {
            List<string> acceptedFileList = GenericHelper.GetCompatibleFiles(rootFileOrCombined);

            WriteLineAndRecord("\n Integrity checking videos: " + acceptedFileList.Count + " found...\n");

            int i = 1;

            string lastDir = string.Empty;

            foreach (string file in acceptedFileList)
            {
                FileInfo info = new FileInfo(file);

                if (info.Directory.FullName != lastDir)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    WriteLineAndRecord("\n Folder: " + info.Directory.FullName + "\n");
                    Console.ResetColor();

                    lastDir = info.Directory.FullName;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                WriteAndRecord(string.Format(" [{0}/{1}]: ", i, acceptedFileList.Count));
                Console.ResetColor();
                WriteAndRecord(info.Name + "... ");

                PerformVideoIntegrityCheck(file);

                i++;
            }

            WriteOutputToFileOption<IntegrityChecker>();
        }

        private void PerformVideoIntegrityCheck(string file)
        {
            string arguments = @"-i """ + file + @"""" + " - hide_banner";

            _errorOutputFlag = false;

            using (Process process = new Process())
            {
                process.StartInfo.FileName = Global.FfmpegPath;
                process.StartInfo.Arguments = arguments;

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.EnableRaisingEvents = true;
                process.OutputDataReceived += new DataReceivedEventHandler(ProcessBatch_OutputDataReceived<IntegrityChecker>);
                process.ErrorDataReceived += new DataReceivedEventHandler(ProcessBatch_ErrorDataReceived<IntegrityChecker>);
                process.Exited += new System.EventHandler(Process_Exited);

                process.Start();

                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                process.WaitForExit();

                if (!_errorOutputFlag)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    WriteAndRecord("OK\n");
                    Console.ResetColor();
                }
            }
        }
    }
}
