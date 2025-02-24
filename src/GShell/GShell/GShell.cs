using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GShell.Core;
using Newtonsoft.Json;

namespace GShell
{
    internal class GShell : ShellBase
    {
        private class Response
        {
#pragma warning disable CS0649
            public string Result;
            public bool Success;
#pragma warning restore CS0649
        }

        private readonly string mURL;
        private readonly Dictionary<string, string> mExtraData;
        private readonly HttpClient mHttpClient;

        public GShell(ShellContext context, string url, Dictionary<string, string> extraData) : base(context)
        {
            mURL = url;
            mExtraData = extraData;
            mHttpClient = new HttpClient();
        }

        protected override async Task<bool> Process(byte[] rawAssembly, string scriptClassName)
        {
            var obj = new {
                SessionId = mContext.SessionId,
                SubmissionId = mContext.SubmissionId - 1,
                RawAssembly = Convert.ToBase64String(rawAssembly),
                ScriptClassName = scriptClassName,
                ExtraData = mExtraData,
            };

            string json = JsonConvert.SerializeObject(obj);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await mHttpClient.PostAsync(mURL, content);
            response.EnsureSuccessStatusCode();

            json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine("The response is empty");
                return false;
            }

            var resp = JsonConvert.DeserializeObject<Response>(json);

            if (!resp.Success)
            {
                Console.WriteLine("Failed to execute script: {0}", resp.Result);
                return false;
            }

            if (!string.IsNullOrEmpty(resp.Result))
                Console.WriteLine(resp.Result);

            return true;
        }
    }
}
