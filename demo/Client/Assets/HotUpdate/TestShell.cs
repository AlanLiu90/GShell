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
    public string EncodedAssembly;
    public Dictionary<string, string> ExtraEncodedAssemblies;
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

    private ShellExecutor mExecutor;
    private ObjectFormatter mObjectFormatter;

    private void Start()
    {
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        mExecutor = new ShellExecutor();

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
            Execute(text, tcs);
            yield return new WaitUntil(() => tcs.Task.IsCompleted);

            var result = tcs.Task.Result;
            request = UnityWebRequest.Put(HttpServerAddress + $"report?PlayerId={PlayerId}", result);
            request.method = UnityWebRequest.kHttpVerbPOST;
            yield return request.SendWebRequest();
        }
    }

    private async void Execute(string text, TaskCompletionSource<string> tcs)
    {
        var shellData = JsonConvert.DeserializeObject<ShellPostData>(text);

        // The first message in each session contains extra assemblies, e.g. GShell.ObjectFormatter.dll
        if (mObjectFormatter == null)
            mObjectFormatter = ObjectFormatterProvider.Instance.CreateFormatter(shellData.ExtraEncodedAssemblies, 8 * 1024);

        var (result, success) = await mExecutor.Execute(shellData.SessionId, shellData.SubmissionId, shellData.EncodedAssembly, shellData.ScriptClassName);

        string str = mObjectFormatter.FormatObject(result);

        var shellResponse = new ShellResponse() { Result = str, Success = success };

        var json = JsonConvert.SerializeObject(shellResponse);

        tcs.SetResult(json);
    }
}
