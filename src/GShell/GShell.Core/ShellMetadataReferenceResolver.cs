using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;

namespace GShell.Core
{
    internal sealed class ShellMetadataReferenceResolver : MetadataReferenceResolver
    {
        private readonly ImmutableArray<string> mSearchPaths;

        public ShellMetadataReferenceResolver(ImmutableArray<string> searchPaths)
        {
            mSearchPaths = searchPaths;
        }

        public override bool ResolveMissingAssemblies => true;

        public override PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
        {
            if (definition is PortableExecutableReference per)
                return per;

            return null;
        }

        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
        {
            foreach (var searchPath in mSearchPaths)
            {
                var path = Path.Combine(searchPath, reference);
                if (File.Exists(path))
                    return ImmutableArray.Create(MetadataReference.CreateFromFile(path, properties));
            }

            return default;
        }

        public override bool Equals(object other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
