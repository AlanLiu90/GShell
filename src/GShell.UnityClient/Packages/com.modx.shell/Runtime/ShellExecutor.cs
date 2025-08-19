using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace GShell
{
    public class ShellExecutor
    {
        private class SessionData
        {
            public int SubmissionCount;
            public object[] SubmissionArray = new object[8];
        }

        private readonly Dictionary<string, SessionData> mSessions = new Dictionary<string, SessionData>();

        public Task<(object, bool)> Execute(string sessionId, int submissionId, string encodedAssembly, string scriptClassName)
        {
            byte[] rawAssembly;

            try
            {
                rawAssembly = Convert.FromBase64String(encodedAssembly);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return Task.FromResult<(object, bool)>((ex, false));
            }

            return Execute(sessionId, submissionId, rawAssembly, scriptClassName);
        }

        public async Task<(object, bool)> Execute(string sessionId, int submissionId, byte[] rawAssembly, string scriptClassName)
        {
            MethodInfo factoryMethod;

            if (!TryGetSubmissionArray(sessionId, submissionId, out var submissionArray))
            {
                mSessions.Remove(sessionId);
                return ("Mismatched submissionId ", false);
            }

            try
            {
                var assembly = Assembly.Load(rawAssembly);
                var scriptType = assembly.GetType(scriptClassName);
                factoryMethod = scriptType.GetMethod("<Factory>", BindingFlags.Public | BindingFlags.Static);
            }
            catch (Exception ex)
            {
                // 运行到这里，表示这个会话出问题了，需要通知Shell结束会话

                mSessions.Remove(sessionId);

                Debug.LogException(ex);
                return (ex, false);
            }

            try
            {
                var obj = await (Task<object>)factoryMethod.Invoke(null, new object[] { submissionArray });
                return (obj, true);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return (ex, true);
            }
        }

        private bool TryGetSubmissionArray(string sessionId, int submissionId, out object[] submissionArray)
        {
            submissionArray = null;

            if (!mSessions.TryGetValue(sessionId, out var data))
            {
                if (submissionId != 1)
                    return false;

                data = new SessionData();
                mSessions.Add(sessionId, data);
            }

            data.SubmissionCount++;

            if (data.SubmissionCount != submissionId)
                return false;

            if (data.SubmissionArray.Length < data.SubmissionCount + 1)
                Array.Resize(ref data.SubmissionArray, data.SubmissionArray.Length * 2);

            submissionArray = data.SubmissionArray;
            return true;
        }
    }
}
