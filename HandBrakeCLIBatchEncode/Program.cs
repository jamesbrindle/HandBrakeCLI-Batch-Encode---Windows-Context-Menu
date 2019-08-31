using System;

namespace BatchEncode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.Title = "HandBrakeCLI Batch Encoder";

            Business.WriteLineAndRecord("");
            Business.WriteAndRecord("       ____.__________ \n");
            Business.WriteAndRecord("      |    |\\______   \\\n");
            Business.WriteAndRecord("      |    | |    | _ /\n");
            Business.WriteAndRecord(" /\\__ |    | |    |   \\\n");
            Business.WriteAndRecord(" \\________ | | ______ /\n");
            Business.WriteAndRecord("                    \\/\n");
            Business.WriteLineAndRecord("");

#if DEBUG
            // Business.EncodeVideos(@"E:\Downloads\Test", @"C:\Utilities\HandBrakeCLI\presets\tv-kids-preset.json", "128");
            Business.IntegrityCheckVideos(@"D:\Desktop\Realive (2016)");
#else
            if (args[0] == "i")
                Business.IntegrityCheckVideos(args[1]);
            else
                Business.EncodeVideos(args[1], args[2], args[3]);
#endif

        }
    }
}