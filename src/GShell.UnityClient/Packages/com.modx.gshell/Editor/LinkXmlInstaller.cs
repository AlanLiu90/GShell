using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;

namespace GShell
{
    public sealed class LinkXmlInstaller : IUnityLinkerProcessor
    {
        public int callbackOrder => 0;

        private static readonly string mLinkXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<linker>
  <assembly fullname=""mscorlib"">
    <type fullname=""System.Activator"" preserve=""all"" />
    <type fullname=""System.ArgumentException"" preserve=""all"" />
    <type fullname=""System.ArgumentNullException"" preserve=""all"" />
    <type fullname=""System.ArgumentOutOfRangeException"" preserve=""all"" />
    <type fullname=""System.Array"" preserve=""all"" />
    <type fullname=""System.AsyncCallback"" preserve=""all"" />
    <type fullname=""System.Attribute"" preserve=""all"" />
    <type fullname=""System.AttributeTargets"" preserve=""all"" />
    <type fullname=""System.AttributeUsageAttribute"" preserve=""all"" />
    <type fullname=""System.Boolean"" preserve=""all"" />
    <type fullname=""System.Byte"" preserve=""all"" />
    <type fullname=""System.Char"" preserve=""all"" />
    <type fullname=""System.Collections.DictionaryEntry"" preserve=""all"" />
    <type fullname=""System.Collections.Generic.Comparer`1"" preserve=""all"" />
    <type fullname=""System.Collections.Generic.HashSet`1"" preserve=""all"" />
    <type fullname=""System.Collections.Generic.ICollection`1"" preserve=""all"" />
    <type fullname=""System.Collections.Generic.IComparer`1"" preserve=""all"" />
    <type fullname=""System.Collections.Generic.IEnumerable`1"" preserve=""all"" />
    <type fullname=""System.Collections.Generic.IEqualityComparer`1"" preserve=""all"" />
    <type fullname=""System.Collections.Generic.IReadOnlyCollection`1"" preserve=""all"" />
    <type fullname=""System.Collections.Generic.IReadOnlyList`1"" preserve=""all"" />
    <type fullname=""System.Collections.Generic.KeyValuePair`2"" preserve=""all"" />
    <type fullname=""System.Collections.Generic.List`1/Enumerator"" preserve=""all"" />
    <type fullname=""System.Collections.ICollection"" preserve=""all"" />
    <type fullname=""System.Collections.IDictionary"" preserve=""all"" />
    <type fullname=""System.Collections.IDictionaryEnumerator"" preserve=""all"" />
    <type fullname=""System.Collections.IEnumerable"" preserve=""all"" />
    <type fullname=""System.Comparison`1"" preserve=""all"" />
    <type fullname=""System.Convert"" preserve=""all"" />
    <type fullname=""System.DateTime"" preserve=""all"" />
    <type fullname=""System.Decimal"" preserve=""all"" />
    <type fullname=""System.Diagnostics.ConditionalAttribute"" preserve=""all"" />
    <type fullname=""System.Diagnostics.DebuggerBrowsableAttribute"" preserve=""all"" />
    <type fullname=""System.Diagnostics.DebuggerBrowsableState"" preserve=""all"" />
    <type fullname=""System.Diagnostics.DebuggerDisplayAttribute"" preserve=""all"" />
    <type fullname=""System.Diagnostics.DebuggerTypeProxyAttribute"" preserve=""all"" />
    <type fullname=""System.Diagnostics.StackFrame"" preserve=""all"" />
    <type fullname=""System.Diagnostics.StackTrace"" preserve=""all"" />
    <type fullname=""System.Double"" preserve=""all"" />
    <type fullname=""System.Enum"" preserve=""all"" />
    <type fullname=""System.Environment"" preserve=""all"" />
    <type fullname=""System.FlagsAttribute"" preserve=""all"" />
    <type fullname=""System.Func`2"" preserve=""all"" />
    <type fullname=""System.Func`3"" preserve=""all"" />
    <type fullname=""System.Globalization.CharUnicodeInfo"" preserve=""all"" />
    <type fullname=""System.Globalization.CultureInfo"" preserve=""all"" />
    <type fullname=""System.Globalization.NumberStyles"" preserve=""all"" />
    <type fullname=""System.Globalization.UnicodeCategory"" preserve=""all"" />
    <type fullname=""System.IAsyncResult"" preserve=""all"" />
    <type fullname=""System.IEquatable`1"" preserve=""all"" />
    <type fullname=""System.IFormatProvider"" preserve=""all"" />
    <type fullname=""System.IndexOutOfRangeException"" preserve=""all"" />
    <type fullname=""System.InsufficientExecutionStackException"" preserve=""all"" />
    <type fullname=""System.Int16"" preserve=""all"" />
    <type fullname=""System.Int64"" preserve=""all"" />
    <type fullname=""System.InvalidOperationException"" preserve=""all"" />
    <type fullname=""System.Linq.Enumerable"" preserve=""all"" />
    <type fullname=""System.Math"" preserve=""all"" />
    <type fullname=""System.MulticastDelegate"" preserve=""all"" />
    <type fullname=""System.Nullable`1"" preserve=""all"" />
    <type fullname=""System.ObsoleteAttribute"" preserve=""all"" />
    <type fullname=""System.OperationCanceledException"" preserve=""all"" />
    <type fullname=""System.ParamArrayAttribute"" preserve=""all"" />
    <type fullname=""System.Predicate`1"" preserve=""all"" />
    <type fullname=""System.Reflection.Assembly"" preserve=""all"" />
    <type fullname=""System.Reflection.AssemblyCompanyAttribute"" preserve=""all"" />
    <type fullname=""System.Reflection.AssemblyConfigurationAttribute"" preserve=""all"" />
    <type fullname=""System.Reflection.AssemblyFileVersionAttribute"" preserve=""all"" />
    <type fullname=""System.Reflection.AssemblyInformationalVersionAttribute"" preserve=""all"" />
    <type fullname=""System.Reflection.AssemblyProductAttribute"" preserve=""all"" />
    <type fullname=""System.Reflection.AssemblyTitleAttribute"" preserve=""all"" />
    <type fullname=""System.Reflection.CustomAttributeExtensions"" preserve=""all"" />
    <type fullname=""System.Reflection.DefaultMemberAttribute"" preserve=""all"" />
    <type fullname=""System.Reflection.FieldInfo"" preserve=""all"" />
    <type fullname=""System.Reflection.IntrospectionExtensions"" preserve=""all"" />
    <type fullname=""System.Reflection.MemberInfo"" preserve=""all"" />
    <type fullname=""System.Reflection.MethodBase"" preserve=""all"" />
    <type fullname=""System.Reflection.MethodInfo"" preserve=""all"" />
    <type fullname=""System.Reflection.ParameterInfo"" preserve=""all"" />
    <type fullname=""System.Reflection.PropertyInfo"" preserve=""all"" />
    <type fullname=""System.Reflection.TargetInvocationException"" preserve=""all"" />
    <type fullname=""System.Reflection.TypeInfo"" preserve=""all"" />
    <type fullname=""System.Runtime.CompilerServices.CallerFilePathAttribute"" preserve=""all"" />
    <type fullname=""System.Runtime.CompilerServices.CallerLineNumberAttribute"" preserve=""all"" />
    <type fullname=""System.Runtime.CompilerServices.ConfiguredTaskAwaitable"" preserve=""all"" />
    <type fullname=""System.Runtime.CompilerServices.ConfiguredTaskAwaitable`1"" preserve=""all"" />
    <type fullname=""System.Runtime.CompilerServices.ExtensionAttribute"" preserve=""all"" />
    <type fullname=""System.Runtime.CompilerServices.RuntimeHelpers"" preserve=""all"" />
    <type fullname=""System.Runtime.CompilerServices.TaskAwaiter"" preserve=""all"" />
    <type fullname=""System.Runtime.ExceptionServices.ExceptionDispatchInfo"" preserve=""all"" />
    <type fullname=""System.Runtime.Versioning.TargetFrameworkAttribute"" preserve=""all"" />
    <type fullname=""System.RuntimeTypeHandle"" preserve=""all"" />
    <type fullname=""System.SByte"" preserve=""all"" />
    <type fullname=""System.Single"" preserve=""all"" />
    <type fullname=""System.StringComparer"" preserve=""all"" />
    <type fullname=""System.StringComparison"" preserve=""all"" />
    <type fullname=""System.Text.RegularExpressions.Capture"" preserve=""all"" />
    <type fullname=""System.Text.RegularExpressions.Group"" preserve=""all"" />
    <type fullname=""System.Text.RegularExpressions.GroupCollection"" preserve=""all"" />
    <type fullname=""System.Text.RegularExpressions.Match"" preserve=""all"" />
    <type fullname=""System.Text.RegularExpressions.Regex"" preserve=""all"" />
    <type fullname=""System.Text.RegularExpressions.RegexOptions"" preserve=""all"" />
    <type fullname=""System.Text.StringBuilder"" preserve=""all"" />
    <type fullname=""System.Threading.CancellationToken"" preserve=""all"" />
    <type fullname=""System.Threading.Interlocked"" preserve=""all"" />
    <type fullname=""System.Threading.Volatile"" preserve=""all"" />
    <type fullname=""System.UInt16"" preserve=""all"" />
    <type fullname=""System.UInt32"" preserve=""all"" />
    <type fullname=""System.UInt64"" preserve=""all"" />
    <type fullname=""System.ValueTuple`3"" preserve=""all"" />
    <type fullname=""System.Void"" preserve=""all"" />
  </assembly>
</linker>
";

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            var path = FileUtil.GetUniqueTempPathInProject();
            File.WriteAllText(path, mLinkXml);
            return Path.GetFullPath(path);
        }

#if !UNITY_2021_2_OR_NEWER
        public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }

        public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }
#endif
    }
}
