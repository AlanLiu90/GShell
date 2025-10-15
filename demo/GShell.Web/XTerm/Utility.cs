namespace GShell.Web.XTerm
{
    internal static class Utility
    {
        internal static int GetWidth(this IUnicodeVersionProvider unicodeVersionProvider, string s)
        {
            return unicodeVersionProvider.GetWidth(s, 0);
        }

        internal static int GetWidth(this IUnicodeVersionProvider unicodeVersionProvider, string s, int startIndex)
        {
            return unicodeVersionProvider.GetWidth(s, startIndex, s.Length - startIndex);
        }

        internal static int GetWidth(this IUnicodeVersionProvider unicodeVersionProvider, string s, int startIndex, int length)
        {
            int width = 0;

            for (int i = startIndex; i < startIndex + length; i++)
                width += unicodeVersionProvider.GetWidth(s[i]);

            return width;
        }
    }
}
