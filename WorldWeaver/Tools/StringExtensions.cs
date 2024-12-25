namespace WorldWeaver.Tools
{
    public static class StringExtensions
    {
        public static string SubstringByIndexes(this string value, int startIndex, int endIndex)
        {
            if (startIndex > endIndex)
            {
                return value;
            }
            var endDifference = endIndex - startIndex + 1;
            if ((startIndex + endDifference) > value.Length)
            {
                endDifference = value.Length - startIndex;
            }

            var newValue = value.Substring(startIndex, endDifference);
            return newValue;
        }

        public static string RemoveLeadingBreaks(this string value)
        {
            while (value.StartsWith("\n"))
            {
                value = value.Substring(1);
            }

            return value;
        }
    }
}
