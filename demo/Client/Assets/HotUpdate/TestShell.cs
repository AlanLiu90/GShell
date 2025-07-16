using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GShell;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

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
    public static readonly string HttpServerAddress = "http://localhost:12345/";
    public static readonly int PlayerId = 100;

    private void Start()
    {
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        var executor = new ShellExecutor();

        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            var request = UnityWebRequest.Get(HttpServerAddress + $"query?PlayerId={PlayerId}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                continue;

            var text = request.downloadHandler.text;
            if (string.IsNullOrEmpty(text))
                continue;

            var tcs = new TaskCompletionSource<string>();
            Execute(executor, text, tcs);
            yield return new WaitUntil(() => tcs.Task.IsCompleted);

            var result = tcs.Task.Result;
            request = UnityWebRequest.Put(HttpServerAddress + $"report?PlayerId={PlayerId}", result);
            request.method = UnityWebRequest.kHttpVerbPOST;
            yield return request.SendWebRequest();
        }
    }

    private async void Execute(ShellExecutor executor, string text, TaskCompletionSource<string> tcs)
    {
        var shellData = JsonConvert.DeserializeObject<ShellPostData>(text);

        var (result, success) = await executor.Execute(shellData.SessionId, shellData.SubmissionId, shellData.RawAssembly, shellData.ScriptClassName);

        string str = FormatObject(result);

        var shellResponse = new ShellResponse() { Result = str, Success = success };

        var json = JsonConvert.SerializeObject(shellResponse);

        tcs.SetResult(json);
    }

    private string FormatObject(object obj)
    {
        if (obj == null)
            return string.Empty;

        if (obj is Object uobj && uobj == null)
            return $"null ({obj.GetType()}) (Destroyed)";

        var str = obj.ToString();
        if (str.Length > 1024)
            str = str.Substring(0, 1024);

        return str;
    }
}
