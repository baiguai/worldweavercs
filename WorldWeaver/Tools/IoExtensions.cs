using System;
namespace WorldWeaver.Tools
{
    public static class IoExtensions
    {
        public static string FileSafe(this string str)
        {
            if (str == null) return null;
            var output = str;

            output.Replace(' ', '_');
            output.Replace('*', '^');
            output.Replace('\\', '|');
            output.Replace('/', '|');

            return output;
        }
    }
}
