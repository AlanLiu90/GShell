using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using GShell;
using System;

public class ShellPostData
{
    public string SessionId;
    public int SubmissionId;
    public string RawAssembly;
    public string ScriptClassName;
    public Dictionary<string, string> ExtraData;
}

public class ShellResponse
{
    public string Result;
    public bool Success;
}

public class TestShell : MonoBehaviour
{
    private HttpListener mHttpListener;

    private void Start()
    {
        string url = "http://localhost:12345/";

        mHttpListener = new HttpListener();
        mHttpListener.Prefixes.Add(url);
        mHttpListener.Start();

        Debug.Log("¼àÌý: " + url);

        RunHttpListener(mHttpListener);
    }

    private void OnDestroy()
    {
        mHttpListener.Abort();
    }

    private async void RunHttpListener(HttpListener listener)
    {
        try
        {
            var executor = new ShellExecutor();

            while (true)
            {
                var context = await listener.GetContextAsync();

                HttpListenerRequest request = context.Request;
                using HttpListenerResponse response = context.Response;

                if (request.HttpMethod != HttpMethod.Post.Method)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    continue;
                }

                if (request.Url.Segments.Length != 2 || request.Url.Segments[1] != "execute")
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    continue;
                }

                string postBody = string.Empty;

                if (request.HasEntityBody)
                {
                    using (Stream body = request.InputStream) // here we have data
                    {
                        using (var reader = new StreamReader(body, request.ContentEncoding))
                        {
                            postBody = await reader.ReadToEndAsync();
                        }
                    }
                }

                if (string.IsNullOrEmpty(postBody))
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    continue;
                }

                var shellData = JsonConvert.DeserializeObject<ShellPostData>(postBody);

                var (result, success) = await executor.Execute(shellData.SessionId, shellData.SubmissionId, shellData.RawAssembly, shellData.ScriptClassName);

                string str = FormatObject(result);

                var shellResponse = new ShellResponse() { Result = str, Success = success };

                string responseString = JsonConvert.SerializeObject(shellResponse);
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;

                using Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
            }
        }
        catch (ObjectDisposedException)
        {
            // ºöÂÔ
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private string FormatObject(object obj)
    {
        if (obj == null)
            return string.Empty;

        var str = obj.ToString();
        if (str.Length > 1024)
            str = str.Substring(0, 1024);

        return str;
    }
}
