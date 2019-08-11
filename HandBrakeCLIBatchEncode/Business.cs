using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BatchEncode
{
    public class Business
    {
        private static string PercentageRegEx = @"(\d+)(\.\d{1,2})? %";
        private static string FPSRegEx = @"(\d+)(\.\d{1,2})? fps";

        public static string[] AcceptedFileTypes
        {
            get
            {
                return new string[] { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".mpv", ".mpeg", "mpg", ".m4v", ".3gp", ".3g2", "ts", "mts", "m2ts", " 4xm", "mtv", "roq", "avm2", "avm2", "flv", "flv", "mj2", "mj2" };
            }
        }

        public static void EncodeVideos(string root, string presetPath, string audioByteRate)
        {
            string[] filesList = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);
            List<string> acceptedFileList = new List<string>();

            foreach (string file in filesList)
            {
                FileInfo info = new FileInfo(file);

                if (info.Extension.ToLower().In(AcceptedFileTypes))
                    acceptedFileList.Add(file);
            }

            Console.Out.WriteLine("\nEncoding videos: " + acceptedFileList.Count + " found...\n");

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
                                Console.Out.Write("... FAIL");
                                continue;
                            }
                        }

                        try
                        {
                            File.Move(file, tempFileName);
                        }
                        catch
                        {
                            Console.Out.Write("... FAIL");

                            continue;
                        }
                    }
                }

                #endregion

                if (info.Directory.FullName != lastDir)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Out.WriteLine("\nFolder: " + info.Directory.FullName + "\n");
                    Console.ResetColor();
                }

                lastDir = info.Directory.FullName;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.Write(string.Format("[{0}/{1}]: ", i, acceptedFileList.Count));
                Console.ResetColor();
                Console.Out.Write(info.Name);

                EncodeVideo(tempFileName, newFileName, presetPath, audioByteRate);

                i++;
            }

            Console.Out.WriteLine("\nEncoding Complete... Exiting\n");
            Thread.Sleep(5000);
        }

        private static bool EncodeVideo(string inputFile, string outputFile, string presetPath, string audioByteRate)
        {
            string arguments = @"-i """ + inputFile + @""" -o """ + outputFile + @""" --preset-import-file """ + presetPath + @""" -B " + audioByteRate;

            using (Process process = new Process())
            {
                process.StartInfo.FileName = Global.HandrakeCLIPath;
                process.StartInfo.Arguments = arguments;

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.EnableRaisingEvents = true;
                process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(Process_OutputDataReceived);
                process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(Process_ErrorDataReceived);
                process.Exited += new System.EventHandler(Process_Exited);

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

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            WriteOutput(e.Data);
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            WriteOutput(e.Data);
        }

        private static string _lastOutput = string.Empty;
        private static void WriteOutput(string output)
        {
            try
            {
                MatchCollection pMc = Regex.Matches(output, PercentageRegEx);
                MatchCollection fpsMc = Regex.Matches(output, FPSRegEx);

                if (pMc.Count > 0)
                {
                    string textP = " " + pMc[pMc.Count - 1].Value.ToString().Replace(" ", "").Replace("%", "") + "%";
                    string textFPS = (fpsMc.Count > 0 && fpsMc[fpsMc.Count - 1].Value.ToString().Length > 3 ? " (" + fpsMc[fpsMc.Count - 1].Value.ToString() + ")" : "") + "  ";

                    if (!string.IsNullOrEmpty(_lastOutput))
                        Console.SetCursorPosition(Console.CursorLeft - _lastOutput.Length, Console.CursorTop);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(textP);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(textFPS);

                    Console.ResetColor();

                    _lastOutput = textP + textFPS;
                }
            }
            catch
            {
                // nevermind
            }
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_lastOutput))
                {
                    Console.SetCursorPosition(Console.CursorLeft - _lastOutput.Length, Console.CursorTop);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(" 100%               \n");
                }
            }
            catch { }

            _lastOutput = string.Empty;

        }
    }
}
