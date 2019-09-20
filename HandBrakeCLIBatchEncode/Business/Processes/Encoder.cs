﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace HandBrakeCLIBatchEncode
{
    public class Encoder : BatchEncoder
    {
        public void EncodeVideos(string rootFileOrCombined, string presetPath, string presetName, string audioByteRate)
        {
            List<string> acceptedFileList = new List<string>();

            if (rootFileOrCombined.Contains(";"))
                acceptedFileList.AddRange(rootFileOrCombined.Split(';').Where(f => new FileInfo(f).Extension.In(Global.CompatibleExtensions)));

            else
            {
                if (rootFileOrCombined.IsFile())
                {
                    FileInfo info = new FileInfo(rootFileOrCombined);
                    if (info.Extension.ToLower().In(Global.CompatibleExtensions))
                        acceptedFileList.Add(rootFileOrCombined);
                }
                else
                {
                    string[] filesList = Directory.GetFiles(rootFileOrCombined, "*.*", SearchOption.AllDirectories);
                    foreach (string file in filesList)
                    {
                        FileInfo info = new FileInfo(file);
                        if (info.Extension.ToLower().In(Global.CompatibleExtensions))
                            acceptedFileList.Add(file);
                    }
                }
            }

            WriteLineAndRecord("\n Encoding videos: " + acceptedFileList.Count + " found...\n");

            int i = 1;

            string lastDir = string.Empty;

            foreach (string file in acceptedFileList)
            {
                FileInfo info = new FileInfo(file);

                string tempFileName = info.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(file) + "_" + info.Extension;
                string newFileName = info.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(file) + Global.DefaultOutputExtension;

                #region Rename file

                try
                {
                    File.Move(file, tempFileName);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("already exists"))
                    {
                        try
                        {
                            File.Delete(tempFileName);
                        }
                        catch
                        {
                            Thread.Sleep(1000);

                            try
                            {
                                File.Delete(tempFileName);
                            }
                            catch
                            {
                                WriteAndRecord("... FAIL");
                                continue;
                            }
                        }

                        try
                        {
                            File.Move(file, tempFileName);
                        }
                        catch
                        {
                            WriteAndRecord("... FAIL");

                            continue;
                        }
                    }
                }

                #endregion

                if (info.Directory.FullName != lastDir)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    WriteLineAndRecord("\n Folder: " + info.Directory.FullName + "\n");
                    Console.ResetColor();
                }

                lastDir = info.Directory.FullName;

                Console.ForegroundColor = ConsoleColor.Green;
                WriteAndRecord(string.Format(" [{0}/{1}]: ", i, acceptedFileList.Count));
                Console.ResetColor();
                WriteAndRecord(info.Name);

                PerformVideoEncode(tempFileName, newFileName, presetPath, presetName, audioByteRate);

                i++;
            }

            WriteOutputToFileOption<Encoder>();
        }

        private bool PerformVideoEncode(string inputFile, string outputFile, string presetPath, string presetName, string audioByteRate)
        {
            string arguments = @"-i """ + inputFile + @""" -o """ + outputFile + @""" --preset-import-file """ + presetPath + @""" -Z """ + presetName + @""" - B " + audioByteRate;

            using (Process process = new Process())
            {
                process.StartInfo.FileName = Global.HandrakeCLIPath;
                process.StartInfo.Arguments = arguments;

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.EnableRaisingEvents = true;
                process.OutputDataReceived += new DataReceivedEventHandler(ProcessBatch_OutputDataReceived<Encoder>);
                process.ErrorDataReceived += new DataReceivedEventHandler(ProcessBatch_ErrorDataReceived<Encoder>);
                process.Exited += new EventHandler(Process_Exited);

                process.Start();

                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                process.WaitForExit();
            }

            #region Delete temp file

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
                    // Forget it
                }
            }

            #endregion

            return true;
        }
    }
}