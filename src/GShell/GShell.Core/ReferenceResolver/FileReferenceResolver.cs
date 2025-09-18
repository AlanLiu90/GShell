using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;

namespace GShell.Core
{
    public sealed class FileReferenceResolver : IReferenceResolver
    {
        private readonly ImmutableArray<string> mSearchPaths;

        public FileReferenceResolver(IEnumerable<string> searchPaths)
        {
            mSearchPaths = searchPaths.ToImmutableArray();
        }

        public ImmutableArray<PortableExecutableReference> Resolve(string reference, string? baseFilePath, MetadataReferenceProperties properties)
        {
            foreach (var path in mSearchPaths)
            {
                var fullPath = Path.Combine(path, reference);
                if (File.Exists(fullPath))
                    return ImmutableArray.Create(MetadataReference.CreateFromFile(fullPath, properties));
            }

            return default;
        }
    }
}
