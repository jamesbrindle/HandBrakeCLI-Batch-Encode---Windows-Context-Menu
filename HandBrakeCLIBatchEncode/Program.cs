using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace HandBrakeCLIBatchEncode
{
    class Program
    {
        // P/Invoke declarations
        private struct RECT { public int left, top, right, bottom; }
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);

        internal const int MF_BYCOMMAND = 0x00000000;
        internal const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        internal static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

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
            MultiFileHandler.SetBusyFlag();

            CenterConsole();
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
            new Encoder().EncodeVideos(@"D:\Desktop\New folder", @"C:\Utilities\HandBrakeCLI\presets\generic-medium.json", "Generic - Medium", "128");
            //new IntegrityChecker().IntegrityCheckVideos(@"C:\Temp");
#else
            Console.Out.Write("\n\n\n Waiting for other files to be added...  ");

            ConsoleSpinner.ShowSpinner();            

            while (MultiFileHandler.IsBusy)
                Thread.Sleep(200);

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

        internal static void CenterConsole()
        {
            IntPtr hWin = GetConsoleWindow();
            GetWindowRect(hWin, out RECT rc);
            Screen scr = Screen.FromPoint(new Point(rc.left, rc.top));

            int x = scr.WorkingArea.Left + (scr.WorkingArea.Width - (rc.right - rc.left)) / 2;
            int y = scr.WorkingArea.Top + (scr.WorkingArea.Height - (rc.bottom - rc.top)) / 2;
            MoveWindow(hWin, x, y, rc.right - rc.left, rc.bottom - rc.top, false);
        }
    }
}