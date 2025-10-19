using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace GShell
{
    public sealed class ShellExecutor
    {
        public int MaximumOutputLength { get; private set; }

        public ShellExecutor(int maximumOutputLength = 8 * 1024)
        {
            if (maximumOutputLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumOutputLength));

            MaximumOutputLength = maximumOutputLength;
        }

        private class SessionData
        {
            public int SubmissionCount;
            public object[] SubmissionArray = new object[8];
        }

        private readonly Dictionary<string, SessionData> mSessions = new Dictionary<string, SessionData>();
        private ObjectFormatter mObjectFormatter;

        public void EnsureObjectFormatterCreated(Dictionary<string, string> extraEncodedAssemblies)
        {
            if (mObjectFormatter == null)
                mObjectFormatter = ObjectFormatterProvider.Instance.CreateFormatter(extraEncodedAssemblies, MaximumOutputLength);
        }

        public void EnsureObjectFormatterCreated(Dictionary<string, byte[]> extraAssemblies)
        {
            if (mObjectFormatter == null)
                mObjectFormatter = ObjectFormatterProvider.Instance.CreateFormatter(extraAssemblies, MaximumOutputLength);
        }

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
                mSessions.Remove(sessionId);

                Debug.LogException(ex);
                return (TryFormatObject(ex), false);
            }

            try
            {
                var obj = await (Task<object>)factoryMethod.Invoke(null, new object[] { submissionArray });
                return (TryFormatObject(obj), true);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return (TryFormatObject(ex), true);
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

        private object TryFormatObject(object obj)
        {
            if (mObjectFormatter != null)
                return mObjectFormatter.FormatObject(obj);
            else
                return obj;
        }
    }
}
