using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace HandBrakeCLIBatchEncode
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (new Mutex(true, "BatchEncode", out bool createdNew))
            {
                if (createdNew)
                    CreateNew(args);
                else
                {
                    if (MultiFileHandler.IsBusy)
                        MultiFileHandler.AddFile(args[1]);
                    else
                        CreateNew(args);
                }
            }
        }

        private static void CreateNew(string[] args)
        {
            Console.CursorVisible = false;
            Console.Title = "HandBrakeCLI Batch Encoder";

            BatchEncoder.WriteLineAndRecord("");
            BatchEncoder.WriteAndRecord("       ____.__________ \n");
            BatchEncoder.WriteAndRecord("      |    |\\______   \\\n");
            BatchEncoder.WriteAndRecord("      |    | |    | _ /\n");
            BatchEncoder.WriteAndRecord(" /\\__ |    | |    |   \\\n");
            BatchEncoder.WriteAndRecord(" \\________ | | ______ /\n");
            BatchEncoder.WriteAndRecord("                    \\/\n");
            BatchEncoder.WriteLineAndRecord("");
#if DEBUG
            new Encoder().EncodeVideos(@"C:\Temp", @"C:\Utilities\HandBrakeCLI\presets\quality.json", "Quality", "128");
            //new IntegrityChecker().IntegrityCheckVideos(@"C:\Temp\control.mp4");
#else
            Console.Out.Write("\n\n\n Waiting for other files to be added...  ");

            ConsoleSpinner.ShowSpinner();
            MultiFileHandler.SetBusyFlag();            

            while(MultiFileHandler.IsBusy)
                Thread.Sleep(100);

            ConsoleSpinner.StopSpinner();
            Console.Out.WriteLine("\n");
            
            List<string> otherFiles = MultiFileHandler.GetFilesInSession();

            Thread.Sleep(100); // allow clearing of locks

            MultiFileHandler.ResetHandler();

            if (otherFiles.Count > 0)
            {
                otherFiles.Insert(0, args[1]); // still need to add current on
                args[1] = string.Join(";", otherFiles.ToArray());
            }

            if (args[0] == "i")
                new IntegrityChecker().IntegrityCheckVideos(args[1]);
            else
                new Encoder().EncodeVideos(args[1], args[2], args[3], args[4]);
#endif
        }
    }
}