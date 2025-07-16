using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GShell.Core;

namespace GShell
{
    internal class GShell : ShellBase
    {
        private sealed class Response
        {
#pragma warning disable CS0649
            public string Result { get; set; }
            public bool Success { get; set; }
#pragma warning restore CS0649
        }

        private readonly string mURL;
        private readonly Dictionary<string, string> mExtraData;
        private readonly AuthenticationData mAuthenticationData;
        private readonly HttpClient mHttpClient;

        public GShell(
            ShellContext context,
            string url,
            Dictionary<string, string> extraData,
            AuthenticationData authenticationData
        )
            : base(context)
        {
            mURL = url;
            mExtraData = extraData;
            mAuthenticationData = authenticationData;
            mHttpClient = new HttpClient();

            SetRequestHeaders();
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

            string json = JsonSerializer.Serialize(obj);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await mHttpClient.PostAsync(mURL, content);
            response.EnsureSuccessStatusCode();

            json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine("The response is empty");
                return false;
            }

            var resp = JsonSerializer.Deserialize<Response>(json);

            if (!resp.Success)
            {
                Console.WriteLine("Failed to execute script: {0}", resp.Result);
                return false;
            }

            if (!string.IsNullOrEmpty(resp.Result))
                Console.WriteLine(resp.Result);

            return true;
        }

        private void SetRequestHeaders()
        {
            if (mAuthenticationData == null)
                return;

            switch (mAuthenticationData.Type)
            {
                case AuthenticationType.Basic:
                    {
                        if (string.IsNullOrEmpty(mAuthenticationData.UserName))
                            throw new Exception("UserName is empty");

                        if (mAuthenticationData.UserName.Contains(':'))
                            throw new Exception("UserName contains ':'");

                        if (string.IsNullOrEmpty(mAuthenticationData.Password))
                            throw new Exception("Password is empty");

                        var authenticationString = $"{mAuthenticationData.UserName}:{mAuthenticationData.Password}";
                        var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

                        mHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

                        break;
                    }

                case AuthenticationType.JWT:
                    {
                        if (string.IsNullOrEmpty(mAuthenticationData.Token))
                            throw new Exception("Token is empty");

                        mHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mAuthenticationData.Token);

                        break;
                    }
            }
        }
    }
}
