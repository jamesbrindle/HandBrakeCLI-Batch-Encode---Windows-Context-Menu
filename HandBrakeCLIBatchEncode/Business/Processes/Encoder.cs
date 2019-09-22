using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace HandBrakeCLIBatchEncode
{
    public class Encoder : BatchEncoder
    {
        public void EncodeVideos(string rootFileOrCombined, string presetPath, string presetName, string audioByteRate)
        {
            if (!PresetValidator.ValidatePreset(presetPath, presetName, out string msg))
            {
                WriteLineAndRecord(msg);

                Console.WriteLine("\n\n\n Exiting...");
                Thread.Sleep(2500);
            }
            else
            {
                List<string> acceptedFileList = GenericHelper.GetCompatibleFiles(rootFileOrCombined);

                WriteAndRecord("\n\n Encoding videos (");
                Console.ForegroundColor = ConsoleColor.Yellow;
                WriteAndRecord(presetName);
                Console.ResetColor();
                WriteAndRecord("): " + acceptedFileList.Count + " found...\n\n") ;

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
