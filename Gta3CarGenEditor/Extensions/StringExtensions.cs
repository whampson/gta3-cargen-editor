namespace WHampson.Gta3CarGenEditor.Extensions
{
    /// <summary>
    /// Extensions to the <see cref="string"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Removes all NUL characters from a string.
        /// </summary>
        /// <param name="s">The string to strip of NUL characters.</param>
        /// <returns>The stripped string.</returns>
        public static string StripNull(this string s)
        {
            return s.Replace("\0", "");
        }
    }
}
