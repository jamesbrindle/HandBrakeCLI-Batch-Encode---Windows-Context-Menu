using System.Configuration;

namespace HandBrakeCLIBatchEncode
{
    public static class Global
    {
        public static string DefaultOutputExtension
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["DefaultOutputExtension"];
                }
                catch
                {
                    return @".mp4";
                }
            }
        }
        public static string HandrakeCLIPath
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["HandrakeCLIPath"];
                }
                catch
                {
                    return @"C:\Utilities\HandBrakeCLIBatchEncode\HandBrakeCLI\HandBrakeCLI.exe";
                }
            }
        }
        public static string FfmpegPath
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["FfmpegPath"];
                }
                catch
                {
                    return @"C:\Utilities\HandBrakeCLIBatchEncode\ffmpeg\ffmpeg.exe";
                }
            }
        }
        public static string[] CompatibleExtensions
        {
            get
            {
                try
                {
                    string[] extentions = ConfigurationManager.AppSettings["CompatibleExtensions"].Split(';');

                    for (int i = 0; i < extentions.Length; i++)
                        extentions[i] = "." + extentions[i];

                    return extentions;
                }
                catch
                {
                    return new string[] { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".mpv", ".mpeg", "mpg", ".m4v", ".3gp", ".3g2", ".ts", ".mts", ".m2ts", ".4xm", ".mtv", ".roq", ".avm2", ".flv", ".mj2" };
                }
            }
        }
    }
}
