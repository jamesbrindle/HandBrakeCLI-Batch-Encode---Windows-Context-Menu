using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HandBrakeCLIBatchEncode
{
    public class Encoder : BatchEncoder
    {
        static ConsoleEventDelegate handler;

        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        internal static string TempFilePath { get; set; } = string.Empty;

        internal static string NewFilePath { get; set; } = string.Empty;

        internal static bool ClosingPrematurely { get; set; } = false;

        public void EncodeVideos(string rootFileOrCombined, string presetPath, string presetName, string audioByteRate)
        {
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

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
                WriteAndRecord("): " + acceptedFileList.Count + " found...\n\n");

                int i = 1;

                string lastDir = string.Empty;

                foreach (string file in acceptedFileList)
                {
                    if (ClosingPrematurely)
                        break;

                    FileInfo info = new FileInfo(file);

                    TempFilePath = info.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(file) + "_" + info.Extension;
                    NewFilePath = info.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(file) + Global.DefaultOutputExtension;

                    #region Rename file

                    try
                    {
                        File.Move(file, TempFilePath);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("already exists"))
                        {
                            try
                            {
                                File.Delete(TempFilePath);
                            }
                            catch
                            {
                                Thread.Sleep(1000);

                                try
                                {
                                    File.Delete(TempFilePath);
                                }
                                catch
                                {
                                    WriteAndRecord("... FAIL");
                                    continue;
                                }
                            }

                            try
                            {
                                File.Move(file, TempFilePath);
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

                    if (!ClosingPrematurely)
                        PerformVideoEncode(TempFilePath, NewFilePath, presetPath, presetName, audioByteRate);

                    i++;
                }

                new Thread((ThreadStart)delegate
                {
                    Thread.Sleep(500);

                    if (!ClosingPrematurely)
                        WriteOutputToFileOption<Encoder>();

                }).Start();
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

            // Need to delay so that if closing the application prematurely, run the 'ConsoleEventCallback' method first so that
            // We don't delete the original file

            new Thread((ThreadStart)delegate
            {
                Thread.Sleep(2000);

                if (!ClosingPrematurely)
                {
                    try
                    {
                        File.Delete(inputFile);
                    }
                    catch
                    {
                        Thread.Sleep(500);

                        try
                        {
                            File.Delete(inputFile);
                        }
                        catch
                        {
                            // Forget it
                        }
                    }
                }

            }).Start();

            #endregion

            return true;
        }

        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                ClosingPrematurely = true;

                if (File.Exists(TempFilePath) && File.Exists(NewFilePath))
                {
                    // closing prematurely

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n\n\n User closed prematurely. Cleaning up files...");

                    try
                    {
                        File.Delete(NewFilePath);
                        File.Move(TempFilePath, NewFilePath);

                        MultiFileHandler.ResetHandler();
                    }
                    catch { }

                    Thread.Sleep(5000);
                }
            }

            return false;
        }
    }
}
