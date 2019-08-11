using System;

namespace BatchEncode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.Title = "HandBrakeCLI Batch Encoder";

            Console.Out.WriteLine("");
            Console.Out.WriteLine("       ____.__________ ");
            Console.Out.WriteLine("      |    |\\______   \\");
            Console.Out.WriteLine("      |    | |    | _ /");
            Console.Out.WriteLine(" /\\__ |    | |    |   \\");
            Console.Out.WriteLine(" \\________ | | ______ /");
            Console.Out.WriteLine("                    \\/");
            Console.Out.WriteLine("");

#if DEBUG
            Business.EncodeVideos(@"E:\Downloads\Test", @"C:\Utilities\HandBrakeCLI\presets\tv-kids-preset.json", "128");
#else
            Business.EncodeVideos(args[0], args[1], args[2]);
#endif

        }
    }
}