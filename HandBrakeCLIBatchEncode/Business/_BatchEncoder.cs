using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace HandBrakeCLIBatchEncode
{
    public abstract class BatchEncoder : IBatchEncoder
    {
        #region Properties

        public virtual string PercentageRegEx { get; set; } = @"(\d+)(\.\d{1,2})? %";

        public virtual string FPSRegEx { get; set; } = @"(\d+)(\.\d{1,2})? fps";

        internal static string RecordedOutput { get; set; } = string.Empty;

        internal static bool ErrorOutputFlag { get; set; } = false;

        #endregion

        internal string _lastOutput = string.Empty;        
        internal static int _originalX = -1;
        internal static int _originalY = -1;

        private bool _readingSuccessful = false;

        public virtual void ProcessBatch_OutputDataReceived<T>(object sender, DataReceivedEventArgs e)
        {
            WriteBatchOutput<T>(e.Data);
        }

        public virtual void ProcessBatch_ErrorDataReceived<T>(object sender, DataReceivedEventArgs e)
        {
            WriteBatchOutput<T>(e.Data);
        }

        public virtual void Process_Exited(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_lastOutput))
            {
                _readingSuccessful = true;
                Console.SetCursorPosition(_originalX, _originalY);

                Console.ForegroundColor = ConsoleColor.Yellow;

                string pad = "";
                for (int i = 0; i < _lastOutput.Length - 5; i++)
                    pad += " ";

                Thread.Sleep(1000);

                if (!Encoder.StartEncodeSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Out.Write("\n          -- Incompatible encoder. Trying another... ");
                    Console.ResetColor();

                    Encoder.TryAnotherEncoder = true;
                }
                else
                {
                    WriteAndRecord(" 100%" + pad + "\n");
                }

                _originalY = -1;
                _originalX = -1;
            }
            else
            {
                if (!_readingSuccessful && Console.CursorLeft > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    WriteAndRecord("\n FAIL\n");
                    Console.ResetColor();
                }

                _readingSuccessful = false;
            }

            _lastOutput = string.Empty;
        }

        public virtual void WriteBatchOutput<T>(string output)
        {
            Type batchType = typeof(T);

            if (batchType == typeof(Encoder))
            {
                try
                {
                    if (output != null)
                    {
                        MatchCollection pMc = Regex.Matches(output, PercentageRegEx);
                        MatchCollection fpsMc = Regex.Matches(output, FPSRegEx);

                        if (pMc != null && fpsMc != null)
                        {
                            if (pMc.Count > 0)
                            {
                                if (_originalX == -1)
                                {
                                    _originalX = Console.CursorLeft;
                                    _originalY = Console.CursorTop;
                                }

                                string pad = "";
                                for (int i = 0; i < _lastOutput.Length - 5; i++)
                                    pad += " ";

                                string textP = pMc[pMc.Count - 1].Value.ToString().Replace(" ", "").Replace("%", "");

                                try
                                {
                                    if (!Encoder.StartEncodeSuccess)
                                    {
                                        if (double.TryParse(textP, out double percent))
                                        {
                                            if (percent > 0)
                                            {
                                                if (!output.Contains("Scanning"))
                                                {
                                                    Encoder.StartEncodeSuccess = true;

                                                    Encoder.TryAnotherEncoder = false;
                                                    Encoder.DontDeleteTempFile = false;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch { }

                                textP = " " + textP + "%";
                                string textFPS = (fpsMc.Count > 0 && fpsMc[fpsMc.Count - 1].Value.ToString().Length > 3 ? " (" + fpsMc[fpsMc.Count - 1].Value.ToString() + ")" : "" + pad);

                                if (!string.IsNullOrEmpty(_lastOutput))
                                    Console.SetCursorPosition(_originalX, _originalY);

                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write(textP);
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write(textFPS);

                                Console.ResetColor();

                                _lastOutput = textP + textFPS;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Console.Out.WriteLine(e);
                    // nevermind
                }
            }

            if (batchType == typeof(IntegrityChecker))
            {
                _readingSuccessful = true;

                if (!ErrorOutputFlag)
                {
                    if (!string.IsNullOrEmpty(output))
                    {
                        if (output.Contains("missing mandatory atoms") ||
                            output.Contains("unspecified pixel format") ||
                            output.Contains("Could not find codec"))
                        {
                            ErrorOutputFlag = true;
                            _readingSuccessful = true;

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

            Program.DeleteMenu(Program.GetSystemMenu(Program.GetConsoleWindow(), true), Program.SC_CLOSE, Program.MF_BYCOMMAND);

            Console.Write("\n\n\n " + typeTitle + " Complete... Would you like to output the result? (Y/N): ");
            char c = Console.ReadKey().KeyChar;

            while (c != 'n' && c != 'N' && c != 'y' && c != 'Y')
            {
                Console.Write("\n\n Unrecognised character command. Please type 'Y' or 'N' for yes or no respectively: ");
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
    }
}
