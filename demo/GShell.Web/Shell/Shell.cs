using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GShell.Core;

namespace GShell.Web.Shell
{
    internal sealed class Shell : ShellBase
    {
        private sealed class Response
        {
#pragma warning disable CS0649
            public string? Result { get; set; }
            public bool Success { get; set; }
#pragma warning restore CS0649
        }

        private readonly string mURL;
        private readonly Dictionary<string, string> mExtraData;
        private readonly Dictionary<string, string>? mExtraEncodedAssemblies;
        private readonly SemaphoreSlim mInputSemaphore;
        private readonly Channel<string> mInputChannel;
        private readonly Channel<string> mOutputChannel;
        private readonly HttpClient mHttpClient;

        public Shell(
            ShellContext context,
            string url,
            string[] extraAssemblies,
            Dictionary<string, string> extraData,
            SemaphoreSlim inputSemaphore,
            Channel<string> inputChannel,
            Channel<string> outputChannel
        )
            : base(context)
        {
            mURL = url;
            mExtraEncodedAssemblies = LoadExtraAssemblies(extraAssemblies);
            mExtraData = extraData;
            mInputSemaphore = inputSemaphore;
            mInputChannel = inputChannel;
            mOutputChannel = outputChannel;
            mHttpClient = new HttpClient();
        }

        protected override ValueTask<string> ReadLineAsync(CancellationToken cancellationToken = default)
        {
            mInputSemaphore!.Release();
            return mInputChannel.Reader.ReadAsync(cancellationToken);
        }

        protected override ValueTask WriteAsync(string s, CancellationToken cancellationToken = default)
        {
            mOutputChannel.Writer.WriteAsync(s, cancellationToken);
            return ValueTask.CompletedTask;
        }

        protected override async Task<bool> ProcessAsync(byte[] rawAssembly, string scriptClassName, CancellationToken cancellationToken = default)
        {
            int submissionId = mContext.SubmissionId - 1;

            var obj = new
            {
                SessionId = mContext.SessionId,
                SubmissionId = submissionId,
                EncodedAssembly = Convert.ToBase64String(rawAssembly),
                ScriptClassName = scriptClassName,
                ExtraEncodedAssemblies = submissionId == 1 ? mExtraEncodedAssemblies : null,
                ExtraData = mExtraData,
            };

            string json = JsonSerializer.Serialize(obj);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await mHttpClient.PostAsync(mURL, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            json = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrEmpty(json))
            {
                await WriteLineAsync("The response is empty", cancellationToken);
                return false;
            }

            var resp = JsonSerializer.Deserialize<Response>(json);

            if (resp == null || !resp.Success)
            {
                await WriteLineAsync($"Failed to execute script: {resp?.Result}", cancellationToken);
                return false;
            }

            if (!string.IsNullOrEmpty(resp.Result))
            {
                await WriteLineAsync(resp.Result, cancellationToken);
            }

            return true;
        }

        private Dictionary<string, string>? LoadExtraAssemblies(string[] extraAssemblies)
        {
            if (extraAssemblies == null || extraAssemblies.Length == 0)
                return null;

            var encodedAssemblies = new Dictionary<string, string>();

            var dir = Path.GetDirectoryName(GetType().Assembly.Location);

            foreach (var assembly in extraAssemblies)
            {
                string path;

                if (File.Exists(assembly))
                {
                    path = assembly;
                }
                else
                {
                    path = Path.Combine(dir!, assembly);
                    if (!File.Exists(path))
                        throw new FileNotFoundException("File not found", assembly);
                }

                var name = Path.GetFileName(path);
                if (encodedAssemblies.ContainsKey(name))
                    throw new ArgumentException($"Duplicate assembly: {name}");

                var bytes = File.ReadAllBytes(path);
                encodedAssemblies.Add(name, Convert.ToBase64String(bytes));
            }

            return encodedAssemblies;
        }
    }
}
