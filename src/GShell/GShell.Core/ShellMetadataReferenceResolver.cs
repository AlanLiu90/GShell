using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;

namespace GShell.Core
{
    internal sealed class ShellMetadataReferenceResolver : MetadataReferenceResolver
    {
        private readonly ScriptMetadataResolver mResolver;
        private readonly HashSet<MetadataReference> mFrameworkReferences;
        private readonly string mAssemblyDirectory;

        public ShellMetadataReferenceResolver(ScriptMetadataResolver resolver, ImmutableArray<PortableExecutableReference> frameworkReferences)
        {
            mResolver = resolver;
            mFrameworkReferences = new HashSet<MetadataReference>(frameworkReferences);
            mAssemblyDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
        }

        public override bool ResolveMissingAssemblies => mResolver.ResolveMissingAssemblies;

        public override PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
        {
            if (definition is PortableExecutableReference per)
            {
                if (mFrameworkReferences.Contains(per))
                {
                    // 对于通过 Basic.Reference.Assemblies 加载的dll，直接返回，否则执行到 ScriptMetadataResolver.ResolveMissingAssembly 内部会抛异常
                    return per;
                }

                if (per.FilePath.StartsWith(mAssemblyDirectory))
                {
                    // 使用 #r 加载Unity工程的dll时，roslyn会尝试加载本工程依赖的dll，但实际上不应该加载，这里直接返回空
                    return null;
                }
            }

            return mResolver.ResolveMissingAssembly(definition, referenceIdentity);
        }

        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
        {
            return mResolver.ResolveReference(reference, baseFilePath, properties);
        }

        public bool Equals(ScriptMetadataResolver other)
        {
            return mResolver.Equals(other);
        }

        public override bool Equals(object other)
        {
            return Equals(other as ScriptMetadataResolver);
        }

        public override int GetHashCode()
        {
            return mResolver.GetHashCode();
        }
    }
}
