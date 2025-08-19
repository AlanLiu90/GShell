using System;
using System.Collections.Generic;
using System.Reflection;

namespace GShell
{
    public sealed class ObjectFormatterProvider
    {
        public static readonly ObjectFormatterProvider Instance = new ObjectFormatterProvider();

        private const string mAssemblyName = "GShell.ObjectFormatter.dll";
        private const int mMinimumOutputLength = 1024;

        public ObjectFormatter CreateFormatter(Dictionary<string, string> encodedAssemblies, int maximumOutputLength)
        {
            byte[] rawAssembly;

            if (encodedAssemblies != null && encodedAssemblies.TryGetValue(mAssemblyName, out var data))
                rawAssembly = Convert.FromBase64String(data);
            else
                rawAssembly = null;

            return CreateFormatter(rawAssembly, maximumOutputLength);
        }

        public ObjectFormatter CreateFormatter(Dictionary<string, byte[]> assemblies, int maximumOutputLength)
        {
            byte[] rawAssembly;

            if (assemblies != null && assemblies.TryGetValue(mAssemblyName, out var data))
                rawAssembly = data;
            else
                rawAssembly = null;

            return CreateFormatter(rawAssembly, maximumOutputLength);
        }

        private ObjectFormatter CreateFormatter(byte[] rawAssembly, int maximumOutputLength)
        {
            maximumOutputLength = Math.Max(maximumOutputLength, mMinimumOutputLength);

            Func<object, string> func = null;
            if (rawAssembly != null)
            {
                var assembly = Assembly.Load(rawAssembly);
                var type = assembly.GetType("GShell.ObjectFormatter.ObjectFormatter");
                var formatter = Activator.CreateInstance(type, new object[] { maximumOutputLength });
                var method = type.GetMethod("FormatObject");

                func = (Func<object, string>)Delegate.CreateDelegate(typeof(Func<object, string>), formatter, method);
            }

            return new ObjectFormatter(maximumOutputLength, func);
        }
    }
}
