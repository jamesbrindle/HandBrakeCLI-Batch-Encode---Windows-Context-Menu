using System.Collections.Generic;
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
            return !File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }
    }
}