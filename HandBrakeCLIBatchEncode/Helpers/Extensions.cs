using System.IO;
using System.Linq;

namespace System
{
    public static class SystemExtensions
    {
        public static bool In<T>(this T needle, params T[] haystack)
        {
            return haystack.Contains(needle);
        }

        public static bool IsFile(this string path)
        {
            try
            {
                return !File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch { return false; }
        }

        public static bool IsDirectory(this string path)
        {
            try
            {
                return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch { return false; }
        }
    }
}