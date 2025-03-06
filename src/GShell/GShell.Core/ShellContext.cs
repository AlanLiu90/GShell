using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;

namespace GShell.Core
{
    public class ShellContext
    {
        internal static CSharpParseOptions ParseOptions => mParseOptions;

        public string SessionId => mSessionId;
        public int SubmissionId => mSubmissionId;

        private static readonly CSharpParseOptions mParseOptions = new CSharpParseOptions(kind: SourceCodeKind.Script);
        private static readonly string[] mSuppressedDiagnosticIds = new[] { "CS1701", "CS1702", "CS1705" };

        private readonly MetadataReference[] mMetadataReferences;
        private readonly string mSessionId;
        private readonly string mAssemblyNamePrefix;
        private readonly string mScriptClassNamePrefix;
        private readonly AdditionalAttributeType mAdditionalAttributeType;
        private readonly CSharpCompilationOptions mCompilationOptions;

        private int mSubmissionId = 1;
        private CSharpCompilation mPreviousCompilation;

        public ShellContext(IEnumerable<string> searchPaths,
            IEnumerable<string> references,
            IEnumerable<string> usings,
            string scriptClassName = "Script",
            AdditionalAttributeType additionalAttributeType = AdditionalAttributeType.None)
        {
            var metadataReferenceResolver = ScriptMetadataResolver.Default
                .WithSearchPaths(searchPaths)
                .WithBaseDirectory(Directory.GetCurrentDirectory());

            mMetadataReferences = references
                .Select(x =>
                {
                    var pes = metadataReferenceResolver.ResolveReference(x, null, default);
                    if (pes.Length == 0)
                        throw new ArgumentException("Cannot find reference: " + x);

                    return pes[0].FilePath;
                })
                .Select(x => MetadataReference.CreateFromFile(x))
                .ToArray();

            mCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                usings: usings,
                sourceReferenceResolver: new SourceFileResolver(ImmutableArray<string>.Empty, Directory.GetCurrentDirectory()),
                metadataReferenceResolver: metadataReferenceResolver,
                metadataImportOptions: MetadataImportOptions.All);

            var topLevelBinderFlagsProperty = typeof(CSharpCompilationOptions).GetProperty("TopLevelBinderFlags", BindingFlags.Instance | BindingFlags.NonPublic);
            topLevelBinderFlagsProperty.SetValue(mCompilationOptions, (uint)1 << 22); // Microsoft.CodeAnalysis.CSharp.BinderFlags.IgnoreAccessibility

            mSessionId = Guid.NewGuid().ToString("N");
            mAssemblyNamePrefix = $"Dynamic_{mSessionId}";
            mScriptClassNamePrefix = scriptClassName;
            mAdditionalAttributeType = additionalAttributeType;
        }

        public (byte[], string) Compile(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code, mParseOptions);

            GetSubmissionInfo(out var assemblyName, out var scriptClassName);

            var compilationOptions = mCompilationOptions.WithScriptClassName(scriptClassName);

            try
            {
                var compilation = CSharpCompilation.CreateScriptCompilation(assemblyName, tree, mMetadataReferences, compilationOptions, mPreviousCompilation, typeof(object));

                using var ms = new MemoryStream();

                var cr = compilation.Emit(ms);

                var diagnostics = cr.Diagnostics.Where(d => !mSuppressedDiagnosticIds.Contains(d.Id));
                foreach (var diagnostic in diagnostics)
                    Console.WriteLine(diagnostic);

                if (cr.Success)
                {
                    mSubmissionId++;
                    mPreviousCompilation = compilation;

                    ms.Seek(0, SeekOrigin.Begin);

                    var rawAssembly = PostProcess(ms.ToArray());
                    return (rawAssembly, scriptClassName);
                }
                else if (cr.Diagnostics.Length == 0)
                {
                    // 对于 using UnityEngine; 这样的输入，不会生成Assembly，但是需要记录 using 本身

                    mPreviousCompilation = compilation;
                    return default;
                }
                else
                {
                    return default;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
        }

        private void GetSubmissionInfo(out string assemblyName, out string scriptClassName)
        {
            assemblyName = $"{mAssemblyNamePrefix}_{mSubmissionId}";
            scriptClassName = $"{mScriptClassNamePrefix}_{mSubmissionId}";
        }

        private byte[] PostProcess(byte[] bytes)
        {
            if (mAdditionalAttributeType == AdditionalAttributeType.SecurityPermission)
            {
                // 添加属性: [assembly: System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum, SkipVerification = true)]

                using var module = ModuleDefMD.Load(bytes);

                var namedArg = new CANamedArgument(
                    false,
                    module.CorLibTypes.Boolean,
                    "SkipVerification",
                    new CAArgument(module.CorLibTypes.Boolean, true));

                var attrType = module.Import(typeof(System.Security.Permissions.SecurityPermissionAttribute));
                var secDecl = new DeclSecurityUser(SecurityAction.RequestMinimum, new[] { 
                    new SecurityAttribute(attrType, new[] { namedArg }) 
                });

                module.Assembly.DeclSecurities.Add(secDecl);

                using var ms = new MemoryStream();
                module.Write(ms);

                return ms.ToArray();
            }
            else
            {
                return bytes;
            }
        }
    }
}
