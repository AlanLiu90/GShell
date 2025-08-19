using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace GShell.ObjectFormatter
{
    public sealed class ObjectFormatter
    {
        private readonly PrintOptions mPrintOptions;

        public ObjectFormatter(int maximumOutputLength)
        {
            mPrintOptions = new PrintOptions();
            mPrintOptions.MaximumOutputLength = maximumOutputLength;
            mPrintOptions.MemberDisplayFormat = MemberDisplayFormat.SeparateLines;
        }

        public string FormatObject(object obj)
        {
            return CSharpObjectFormatter.Instance.FormatObject(obj, mPrintOptions);
        }
    }
}
