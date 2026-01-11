using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace GShell.Core
{
    public static class ShellContextFactory
    {
        public static IShellContext CreateContext(
            ImmutableArray<PortableExecutableReference> targetFrameworkReferences,
            IReferenceResolver referenceResolver,
            IEnumerable<string> references,
            IEnumerable<string> usings,
            IEnumerable<string> sourceFileSearchPaths,
            string scriptClassName = "Script",
            AdditionalAttributeType additionalAttributeType = AdditionalAttributeType.None,
            ILogger? logger = null)
        {
            var context = new ShellContext(
                targetFrameworkReferences,
                referenceResolver,
                references,
                usings,
                sourceFileSearchPaths,
                scriptClassName,
                additionalAttributeType,
                logger);

            return context;
        }
    }
}
