using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using Microsoft.CodeAnalysis;

namespace GShell.Core
{
    public sealed class HTTPReferenceResolver : IReferenceResolver
    {
        private static readonly Lazy<HttpClient> mDefaultHttpClient = new Lazy<HttpClient>(() => new HttpClient());

        private readonly ImmutableArray<string> mSearchURLs;
        private readonly Func<HttpClient>? mHttpClientFactory;

        public HTTPReferenceResolver(IEnumerable<string> searchURLs, Func<HttpClient>? httpClientFactory = null)
        {
            mSearchURLs = searchURLs.ToImmutableArray();
            mHttpClientFactory = httpClientFactory;
        }

        public ImmutableArray<PortableExecutableReference> Resolve(string reference, string? baseFilePath, MetadataReferenceProperties properties)
        {
            foreach (var url in mSearchURLs)
            {
                try
                {
                    var httpClient = mHttpClientFactory != null ? mHttpClientFactory() : mDefaultHttpClient.Value;
                    var fullURL = url + reference;

                    using HttpResponseMessage response = httpClient.GetAsync(fullURL).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();

                    var bytes = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    return ImmutableArray.Create(MetadataReference.CreateFromStream(new MemoryStream(bytes), properties));
                }
                catch
                {
                    // Ignore
                }
            }

            return default;
        }
    }
}
