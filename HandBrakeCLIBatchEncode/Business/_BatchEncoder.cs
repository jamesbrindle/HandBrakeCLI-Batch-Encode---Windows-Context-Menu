using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace HandBrakeCLIBatchEncode
{
    public abstract class BatchEncoder : IBatchEncoder
    {
        internal bool _errorOutputFlag = false;
        internal string _lastOutput = string.Empty;
        
        public virtual void ProcessBatch_OutputDataReceived<T>(object sender, DataReceivedEventArgs e)
        {
            WriteBatchOutput<T>(e.Data);
        }

        public virtual void ProcessBatch_ErrorDataReceived<T>(object sender, DataReceivedEventArgs e)
        {
            WriteBatchOutput<T>(e.Data);
        }

        public virtual void WriteBatchOutput<T>(string output)
        {
            Type batchType = typeof(T);

            if (batchType == typeof(Encoder))
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

            if (batchType == typeof(IntegrityChecker))
            {
                if (!_errorOutputFlag)
                {
                    if (!string.IsNullOrEmpty(output))
                    {
                        if (output.Contains("missing mandatory atoms") ||
                            output.Contains("unspecified pixel format") ||
                            output.Contains("Could not find codec"))
                        {
                            _errorOutputFlag = true;

                            Console.ForegroundColor = ConsoleColor.Red;
                            WriteAndRecord("FAIL\n");
                            Console.ResetColor();
                        }
                    }
                }
            }
        }

        public virtual void WriteOutputToFileOption<T>()
        {
            Console.ResetColor();

            string typeTitle = "Operation";
            string fileTitle = "HandBrakeCLI Batch Encode Output.txt";

            if (typeof(T) == typeof(Encoder))
            {
                typeTitle = "Encoding";
                fileTitle = "HandBrakeCLI Batch Encode Encoder Results.txt";
            }
            else if (typeof(T) == typeof(IntegrityChecker))
            {
                typeTitle = "Integrity Check";
                fileTitle = "HandBrakeCLI Integrity Check Results.txt";
            }

            Console.Write("\n\n\n " + typeTitle + " Complete... Would you like to output the result? (Y/N): ");
            char c = Console.ReadKey().KeyChar;

            while (c != 'n' && c != 'N'  && c != 'y' && c != 'Y')
            {
                Console.Write("\n\n Unregognised character command. Please type 'Y' or 'N' for yes or no respectively: ");
                c = Console.ReadKey().KeyChar;
            }
            if (c == 'y' || c == 'Y')
            {
                Console.WriteLine("\n\n Writing output to: C:\\Temp\\" + fileTitle + "...");

                try
                {
                    if (!Directory.Exists(@"C:\Temp"))
                        Directory.CreateDirectory(@"C:\Temp");

                    RecordedOutput += "\n\n\n Complete";
                    File.WriteAllText(@"C:\Temp\" + fileTitle, RecordedOutput);

                    try
                    {
                        Process.Start(@"C:\Temp\" + fileTitle);
                    }
                    catch { }
                }
                catch (Exception e)
                {
                    Console.WriteLine(" Error writing output file:" + e.Message);
                }

                Console.WriteLine("\n\n\n Exiting...");
            }
            else
                Console.WriteLine("\n\n\n Exiting...");

            Thread.Sleep(2500);
        }

        internal static void WriteAndRecord(string output)
        {
            Console.Out.Write(output);
            RecordedOutput += output;
        }

        internal static void WriteLineAndRecord(string output)
        {
            Console.Out.WriteLine(output);
            RecordedOutput += "\n" + output + "\n";
        }

        public virtual void Process_Exited(object sender, EventArgs e)
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
            catch {
                Console.ForegroundColor = ConsoleColor.Yellow;
                WriteAndRecord(" 100%                         \n");
            }

            _lastOutput = string.Empty;
        }

        #region Properties

        public virtual string PercentageRegEx { get; set; } = @"(\d+)(\.\d{1,2})? %";

        public virtual string FPSRegEx { get; set; } = @"(\d+)(\.\d{1,2})? fps";

        internal static string RecordedOutput { get; set; } = string.Empty;

        #endregion
    }
}
