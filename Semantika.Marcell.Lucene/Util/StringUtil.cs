using System;

namespace Semantika.Marcell.LuceneStore.Util
{
    /// <summary>
    /// Extension methods for standard string operations
    /// </summary>
    public static class StringUtil
    {
        /// <summary>
        /// This methos truncates the string to the specified length and adds and elypsis in case the string is actually truncated.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxChars"></param>
        /// <returns></returns>
        public static string Truncate(this string value, int maxChars)
        {
            if (maxChars < 4)
            {
                throw new ArgumentException("The method can only truncate strings to at leas 4 characters.");
            }
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "...";
        }
    }
}