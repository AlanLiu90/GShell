using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace GShell.Core
{
    internal sealed class ShellMetadataReferenceResolver : MetadataReferenceResolver
    {
        private readonly IReferenceResolver mReferenceResolver;

        public ShellMetadataReferenceResolver(IReferenceResolver referenceResolver)
        {
            mReferenceResolver = referenceResolver;
        }

        public override bool ResolveMissingAssemblies => true;

        public override PortableExecutableReference? ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
        {
            if (definition is PortableExecutableReference per)
                return per;

            return null;
        }

        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string? baseFilePath, MetadataReferenceProperties properties)
        {
            return mReferenceResolver.Resolve(reference, baseFilePath, properties);
        }

        public override bool Equals(object? other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
