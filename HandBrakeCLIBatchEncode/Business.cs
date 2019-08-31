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

        private static string RecordedOutput = string.Empty;

        public static string[] AcceptedFileTypes
        {
            get
            {
                return new string[] { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".mpv", ".mpeg", "mpg", ".m4v", ".3gp", ".3g2", "ts", "mts", "m2ts", " 4xm", "mtv", "roq", "avm2", "avm2", "flv", "flv", "mj2", "mj2" };
            }
        }

        #region Encoding

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

            WriteLineAndRecord("\nEncoding videos: " + acceptedFileList.Count + " found...\n");

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
                    WriteLineAndRecord("\nFolder: " + info.Directory.FullName + "\n");
                    Console.ResetColor();
                }

                lastDir = info.Directory.FullName;

                Console.ForegroundColor = ConsoleColor.Green;
                WriteAndRecord(string.Format("[{0}/{1}]: ", i, acceptedFileList.Count));
                Console.ResetColor();
                WriteAndRecord(info.Name);

                EncodeVideo(tempFileName, newFileName, presetPath, audioByteRate);

                i++;
            }

            WriteOutputToFileOption(WriteToFileType.Encoder);
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
                process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(ProcessEncoder_OutputDataReceived);
                process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(ProcessEncoder_ErrorDataReceived);
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

        private static void ProcessEncoder_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            WriteEncoderOutput(e.Data);
        }

        private static void ProcessEncoder_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            WriteEncoderOutput(e.Data);
        }

        private static string _lastOutput = string.Empty;

        private static void WriteEncoderOutput(string output)
        {
            try
            {
                MatchCollection pMc = Regex.Matches(output, PercentageRegEx);
                MatchCollection fpsMc = Regex.Matches(output, FPSRegEx);

                if (pMc.Count > 0)
                {
                    string textP = " " + pMc[pMc.Count - 1].Value.ToString().Replace(" ", "").Replace("%", "") + "%";
                    string textFPS = (fpsMc.Count > 0 && fpsMc[fpsMc.Count - 1].Value.ToString().Length > 3 ? " (" + fpsMc[fpsMc.Count - 1].Value.ToString() + ")" : "") + "                  ";

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

        #endregion

        #region Integrity Checking

        public static void IntegrityCheckVideos(string root)
        {
            string[] filesList = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);
            List<string> acceptedFileList = new List<string>();

            foreach (string file in filesList)
            {
                FileInfo info = new FileInfo(file);

                if (info.Extension.ToLower().In(AcceptedFileTypes))
                    acceptedFileList.Add(file);
            }

            WriteLineAndRecord("\nIntegrity checking videos: " + acceptedFileList.Count + " found...\n");

            int i = 1;

            string lastDir = string.Empty;

            foreach (string file in acceptedFileList)
            {
                FileInfo info = new FileInfo(file);

                if (info.Directory.FullName != lastDir)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    WriteLineAndRecord("\nFolder: " + info.Directory.FullName + "\n");
                    Console.ResetColor();

                    lastDir = info.Directory.FullName;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                WriteAndRecord(string.Format("[{0}/{1}]: ", i, acceptedFileList.Count));
                Console.ResetColor();
                WriteAndRecord(info.Name + "... ");

                IntegrityCheckVideo(file);

                i++;
            }

            WriteOutputToFileOption(WriteToFileType.IntegrityCheck);
        }

        private static void IntegrityCheckVideo(string file)
        {
            string arguments = @"-i """ + file + @"""" + " - hide_banner";

            errorAlreadyBeenOutput = false;

            using (Process process = new Process())
            {
                process.StartInfo.FileName = Global.FfmpegPath;
                process.StartInfo.Arguments = arguments;

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.EnableRaisingEvents = true;
                process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(ProcessIntegrityCheck_OutputDataReceived);
                process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(ProcessIntegrityCheck_ErrorDataReceived);
                process.Exited += new System.EventHandler(Process_Exited);

                process.Start();

                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                process.WaitForExit();

                if (!errorAlreadyBeenOutput)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    WriteAndRecord("OK\n");
                    Console.ResetColor();
                }
            }            
        }

        private static void ProcessIntegrityCheck_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            WriteIntegrityCheckOutout(e.Data);
        }

        private static void ProcessIntegrityCheck_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            WriteIntegrityCheckOutout(e.Data);
        }

        private static bool errorAlreadyBeenOutput = false;

        private static void WriteIntegrityCheckOutout(string output)
        {
            if (!errorAlreadyBeenOutput)
            {
                if (!string.IsNullOrEmpty(output))
                {
                    if (output.Contains("missing mandatory atoms") ||
                        output.Contains("unspecified pixel format") ||
                        output.Contains("Could not find codec"))
                    {
                        errorAlreadyBeenOutput = true;

                        Console.ForegroundColor = ConsoleColor.Red;
                        WriteAndRecord("FAIL\n");
                        Console.ResetColor();
                    }
                }
            }
        }

        #endregion

        private static void Process_Exited(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_lastOutput))
                {
                    Console.SetCursorPosition(Console.CursorLeft - _lastOutput.Length, Console.CursorTop);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    WriteAndRecord(" 100%                         \n");
                }
            }
            catch { }

            _lastOutput = string.Empty;
        }

        public enum WriteToFileType
        {
            Encoder,
            IntegrityCheck
        }

        private static void WriteOutputToFileOption(WriteToFileType writeToFileTypeEnum)
        {
            Console.ResetColor();

            string typeTitle = "Operation";
            string fileTitle = "HandBrakeCLI Batch Encode Output.txt";

            if (writeToFileTypeEnum == WriteToFileType.Encoder)
            {
                typeTitle = "Encoding";
                fileTitle = "HandBrakeCLI Batch Encode Encoder Results.txt";
            }
            else if (writeToFileTypeEnum == WriteToFileType.IntegrityCheck)
            {
                typeTitle = "Integrity Check";
                fileTitle = "HandBrakeCLI Integrity Check Results.txt";
            }

            Console.WriteLine("\n\n" + typeTitle + " Complete... Would you like to output the result? (Y/N):\n");
            string s = Console.In.ReadLine();

            while (s.ToLower() != "n" && s.ToLower() != "no" && s.ToLower() != "y" && s.ToLower() != "yes")
            {
                Console.WriteLine("\nUnregognised character command. Please type 'Y' or 'No' for yes or no respectively: \n");
                s = Console.In.ReadLine();
            }
            if (s.ToLower() == "y" || s.ToLower() == "yes")
            {
                Console.WriteLine("\nWriting output to: C:\\Temp\\" + fileTitle + "...");
               
                try
                {
                    if (!Directory.Exists(@"C:\Temp"))
                        Directory.CreateDirectory(@"C:\Temp");

                    RecordedOutput += "\n\n\nComplete";
                    File.WriteAllText(@"C:\Temp\" + fileTitle, RecordedOutput);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error writing output file:" + e.Message);                    
                }

                Console.WriteLine("\nExiting...");
            }
            else
                Console.WriteLine("\nExiting...");

            Thread.Sleep(5000);
        }

        public static void WriteAndRecord(string output)
        {
            Console.Out.Write(output);
            RecordedOutput += output;
        }

        public static void WriteLineAndRecord(string output)
        {
            Console.Out.WriteLine(output);
            RecordedOutput += "\n" + output + "\n";
        }
    }
}
