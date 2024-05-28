namespace WorldWeaver.Tools
{
    public static class StringExtensions
    {
        public static string SubstringByIndexes(this string value, int startIndex, int endIndex)
        {
            return value.Substring(startIndex, endIndex - startIndex + 1);
        }
    }
}