using System;

namespace GShell
{
    public sealed class ObjectFormatter
    {
        private readonly int mMaximumOutputLength;
        private readonly Func<object, string> mFormatObject;

        internal ObjectFormatter(int maximumOutputLength, Func<object, string> formatObject)
        {
            mMaximumOutputLength = maximumOutputLength;
            mFormatObject = formatObject;
        }

        public string FormatObject(object obj)
        {
            if (obj == null)
                return string.Empty;

            if (obj is UnityEngine.Object uobj && uobj == null)
                return $"null ({obj.GetType()}) (Destroyed)";

            if (mFormatObject != null)
            {
                try
                {
                    return mFormatObject.Invoke(obj);
                }
                catch (Exception e)
                {
                    return $"!<{e.GetType()}:{e.Message}>";
                }
            }

            try
            {
                var str = obj.ToString();
                if (str.Length > mMaximumOutputLength)
                    str = str.Substring(0, mMaximumOutputLength);

                return str;
            }
            catch (Exception e)
            {
                return $"!<{e.GetType()}:{e.Message}>";
            }
        }
    }
}
