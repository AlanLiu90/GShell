using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JWT.Algorithms;
using JWT.Builder;

namespace HttpServer
{
    public class ShellPostData
    {
        public string SessionId { get; set; }
        public int SubmissionId { get; set; }
        public string RawAssembly { get; set; }
        public string ScriptClassName { get; set; }
        public Dictionary<string, string> ExtraData { get; set; }
    }

    public class PlayerData
    {
        public string Command;
        public TaskCompletionSource<string> TCS;
    }

    public class AuthenticationData
    {
        public string Type;
        public string UserName;
        public string Password;
    }

    public class JWTData
    {
        public string UserName { get; set; }
        public string PlayerId { get; set; }
    }

    internal class HttpServer
    {
        private const string mSecretKey = "a-string-secret-at-least-256-bits-long";

        private readonly string mAddress;
        private readonly Dictionary<int, PlayerData> mCommands = new Dictionary<int, PlayerData>();
        private readonly Dictionary<string, AuthenticationData> mAuthenticationData = new Dictionary<string, AuthenticationData>();
        private string mAuthenticationType;

        public HttpServer(string address) 
        {
            mAddress = address;
        }

        public async Task Run()
        {
            try
            {
                var httpListener = new HttpListener();
                httpListener.Prefixes.Add(mAddress);
                httpListener.Start();

                Console.WriteLine("开始监听: " + mAddress);

                while (true)
                {
                    var context = await httpListener.GetContextAsync();

                    Handle(context);
                }
            }
            catch (ObjectDisposedException)
            {
                // 忽略
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void Handle(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            using HttpListenerResponse response = context.Response;

            if (request.Url.Segments.Length != 2)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            switch (request.Url.Segments[1])
            {
                case "execute":
                    await Execute(request, response);
                    break;

                case "query":
                    Query(request, response);
                    break;

                case "report":
                    await Report(request, response);
                    break;

                case "configure_authentication":
                    await ConfigureAuthentication(request, response);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
            }
        }

        private async Task Execute(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.HttpMethod != HttpMethod.Post.Method)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (!Authenticate(request, out var userName, out var playerId))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            string postBody = await ReadPostBody(request);

            if (string.IsNullOrEmpty(postBody))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (!playerId.HasValue)
            {
                var data = JsonSerializer.Deserialize<ShellPostData>(postBody);

                if (!data.ExtraData.TryGetValue("PlayerId", out var s))
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                if (!int.TryParse(s, out var id))
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                playerId = id;
            }

            Console.WriteLine($"Execute command, userName={userName}, playerId={playerId}");

            var playerData = new PlayerData() { Command = postBody, TCS = new TaskCompletionSource<string>() };
            mCommands.Add(playerId.Value, playerData);

            var result = await playerData.TCS.Task;
            mCommands.Remove(playerId.Value);

            byte[] buffer = Encoding.UTF8.GetBytes(result);

            response.ContentLength64 = buffer.Length;

            using Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            response.StatusCode = (int)HttpStatusCode.OK;
        }

        private void Query(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.HttpMethod != HttpMethod.Get.Method)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var playerId = request.QueryString["PlayerId"];
            if (!int.TryParse(playerId, out var id))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            string text;

            if (mCommands.TryGetValue(id, out var cmd))
                text = cmd.Command;
            else
                text = string.Empty;

            byte[] buffer = Encoding.UTF8.GetBytes(text);

            response.ContentLength64 = buffer.Length;

            using Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            response.StatusCode = (int)HttpStatusCode.OK;
        }

        private async Task Report(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.HttpMethod != HttpMethod.Post.Method)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var playerId = request.QueryString["PlayerId"];
            if (!int.TryParse(playerId, out var id))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (!mCommands.TryGetValue(id, out var cmd))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            string postBody = await ReadPostBody(request);

            cmd.TCS.SetResult(postBody);

            response.StatusCode = (int)HttpStatusCode.OK;
        }

        private async Task ConfigureAuthentication(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.HttpMethod != HttpMethod.Post.Method)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var enable = request.QueryString["Enable"];
            if (!bool.TryParse(enable, out var flag))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (!flag)
            {
                mAuthenticationType = "";

                Console.WriteLine("Disable authentication");
                return;
            }

            Console.WriteLine("Enable authentication");

            string postBody = await ReadPostBody(request);

            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(postBody);

            var type = data["Type"];
            switch (type)
            {
                case "Basic":
                    {
                        var userName = data["UserName"];
                        var password = data["Password"];

                        var authData = new AuthenticationData() { Type = type, UserName = userName, Password = password };
                        mAuthenticationData[userName] = authData;
                        mAuthenticationType = type;

                        Console.WriteLine($"Configure authentication for {userName} (Type={type}, UserName={userName} Password={password})");

                        break;
                    }

                case "JWT":
                    {
                        var userName = data["UserName"];
                        var playerId = data["PlayerId"];

                        var token = JwtBuilder.Create()
                              .WithAlgorithm(new HMACSHA256Algorithm())
                              .WithSecret(mSecretKey)
                              .ExpirationTime(DateTimeOffset.UtcNow.AddMinutes(60).ToUnixTimeSeconds())
                              .AddClaim("UserName", userName)
                              .AddClaim("PlayerId", playerId)
                              .Encode();

                        mAuthenticationType = type;

                        Console.WriteLine($"Configure authentication for {userName} (Type={type}, UserName={userName}, PlayerId={playerId}, Token={token})");

                        break;
                    }

                default:
                    {
                        mAuthenticationType = null;
                        response.StatusCode= (int)HttpStatusCode.BadRequest;
                        break;
                    }
            }
        }

        private bool Authenticate(HttpListenerRequest request, out string userName, out int? playerId)
        {
            userName = "anonymous";
            playerId = null;

            if (!string.IsNullOrEmpty(mAuthenticationType))
            {
                try
                {
                    switch (mAuthenticationType)
                    {
                        case "Basic":
                            {
                                var basic = request.Headers["Authorization"];

                                if (string.IsNullOrEmpty(basic) || !basic.StartsWith("Basic ", StringComparison.Ordinal))
                                    return false;

                                var value = basic.Substring("Basic ".Length);
                                var authenticationString = Encoding.ASCII.GetString(Convert.FromBase64String(value));
                                var index = authenticationString.IndexOf(':');

                                if (index == -1)
                                    return false;

                                userName = authenticationString.Substring(0, index);
                                string password = authenticationString.Substring(index + 1);

                                if (!mAuthenticationData.TryGetValue(userName, out var authData))
                                    return false;

                                return authData.Password == password;
                            }

                        case "JWT":
                            {
                                var bearer = request.Headers["Authorization"];

                                if (string.IsNullOrEmpty(bearer) || !bearer.StartsWith("Bearer ", StringComparison.Ordinal))
                                    return false;

                                var value = bearer.Substring("Bearer ".Length);
                                var data = JwtBuilder.Create()
                                    .WithAlgorithm(new HMACSHA256Algorithm())
                                    .WithSecret(mSecretKey)
                                    .Decode<JWTData>(value);

                                if (string.IsNullOrEmpty(data.UserName))
                                    return false;

                                if (string.IsNullOrEmpty(data.PlayerId))
                                    return false;

                                userName = data.UserName;
                                playerId = int.Parse(data.PlayerId);

                                return true;
                            }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

            return true;
        }

        private async Task<string> ReadPostBody(HttpListenerRequest request)
        {
            if (request.HasEntityBody)
            {
                using (Stream body = request.InputStream)
                {
                    using (var reader = new StreamReader(body, request.ContentEncoding))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }

            return string.Empty;
        }
    }
}
