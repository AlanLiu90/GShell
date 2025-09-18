using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace GShell.Core
{
    public interface IReferenceResolver
    {
        public ImmutableArray<PortableExecutableReference> Resolve(string reference, string? baseFilePath, MetadataReferenceProperties properties);
    }
}
