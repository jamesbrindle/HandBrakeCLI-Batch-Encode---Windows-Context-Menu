namespace BatchEncode
{
    public static class Global
    {
        public static string DefaultOutputExtension { get; set; } = ".mp4";
        public static string HandrakeCLIPath { get; set; } = @"C:\Utilities\HandBrakeCLI\HandBrakeCLI.exe";

        public static string FfmpegPath { get; set; } = @"C:\Utilities\ffmpeg\ffmpeg.exe";
    }
}
