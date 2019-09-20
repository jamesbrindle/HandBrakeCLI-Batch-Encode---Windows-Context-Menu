using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace HandBrakeCLIBatchEncode
{
    class Program
    {
        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool ShowWindow([In] IntPtr hWnd, [In] int nCmdShow);

        [STAThread]
        static void Main(string[] args)
        {
            using (new Mutex(true, "BatchEncode", out bool createdNew))
            {
                if (createdNew)
                    CreateNew(args);
                else
                {
                    if (MultiFileHandler.IsBusy())
                    {
                        IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
                        ShowWindow(handle, 6);
                        MultiFileHandler.AddFile(args[1]);
                    }
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
            //new Encoder().EncodeVideos(@"C:\Temp", @"C:\Utilities\HandBrakeCLI\presets\tv-kids-preset.json", "128");
            new IntegrityChecker().IntegrityCheckVideos(@"C:\Temp");
#else
            Console.Out.WriteLine("\n\n Waiting for other files to be added...\n");

            MultiFileHandler.SetBusyFlag();
            Thread.Sleep(3000); // Give time for Windows to add mulitple files

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