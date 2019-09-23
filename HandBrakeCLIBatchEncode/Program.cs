﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace HandBrakeCLIBatchEncode
{
    class Program
    {
        internal const int MF_BYCOMMAND = 0x00000000;
        internal const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        internal static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetConsoleWindow();

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
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);

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
            //new Encoder().EncodeVideos(@"C:\Temp", @"C:\Utilities\HandBrakeCLI\presets\quality.json", "Quality", "128");
            new IntegrityChecker().IntegrityCheckVideos(@"C:\Temp\control.mp4");
#else
            Console.Out.Write("\n\n\n Waiting for other files to be added...  ");

            ConsoleSpinner.ShowSpinner();

            MultiFileHandler.SetBusyFlag();
            Thread.Sleep(6000); // Give time for Windows to add mulitple files

            ConsoleSpinner.StopSpinner();
            Console.Out.WriteLine("\n");
            
            List<string> otherFiles = MultiFileHandler.GetFilesInSession();

            Thread.Sleep(200); // allow clearing of locks

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