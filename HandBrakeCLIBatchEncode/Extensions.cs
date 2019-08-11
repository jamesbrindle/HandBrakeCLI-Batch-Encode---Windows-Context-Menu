using System.Linq;

namespace System
{
    public static class SystemExtensions
    {
        public static bool In<T>(this T needle, params T[] haystack)
        {
            return haystack.Contains(needle);
        }
    }
}
