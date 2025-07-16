using System;
using System.Collections.Generic;
using GShell;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ConfigureAuthentication : EditorWindow
{
    [Serializable]
    private class BasicData
    {
        public string UserName;
        public string Password;
    }

    [Serializable]
    private class JWTData
    {
        public string UserName;
        public string PlayerId;
    }

    private AuthenticationType mAuthenticationType;
    private BasicData mBasicData = new BasicData();
    private JWTData mJWTData = new JWTData();

    [MenuItem("Demo/Configure Authentication")]
    public static void ShowWindow()
    {
        var window = GetWindow<ConfigureAuthentication>();
        window.titleContent = new GUIContent("Configure Authentication");
    }

    private void OnGUI()
    {
        var authType = (AuthenticationType)EditorGUILayout.EnumPopup(new GUIContent("Type"), mAuthenticationType);

        if (authType != mAuthenticationType)
        {
            mAuthenticationType = authType;
        }

        switch (mAuthenticationType)
        {
            case AuthenticationType.Basic:
                {
                    mBasicData.UserName = EditorGUILayout.TextField(new GUIContent("UserName"), mBasicData.UserName);
                    mBasicData.Password = EditorGUILayout.TextField(new GUIContent("Password"), mBasicData.Password);
                    break;
                }

            case AuthenticationType.JWT:
                {
                    mJWTData.UserName = EditorGUILayout.TextField(new GUIContent("UserName"), mJWTData.UserName);
                    mJWTData.PlayerId = EditorGUILayout.TextField(new GUIContent("PlayerId"), mJWTData.PlayerId);
                    break;
                }

            case AuthenticationType.None:
            default:
                break;
        }

        if (GUILayout.Button("Send to Server"))
        {
            var enable = false;
            var data = new Dictionary<string, string>();

            switch (mAuthenticationType)
            {
                case AuthenticationType.Basic:
                    {
                        enable = true;
                        data.Add("Type", mAuthenticationType.ToString());
                        data.Add("UserName", mBasicData.UserName);
                        data.Add("Password", mBasicData.Password);
                        break;
                    }

                case AuthenticationType.JWT:
                    {
                        enable = true;
                        data.Add("Type", mAuthenticationType.ToString());
                        data.Add("UserName", mJWTData.UserName);
                        data.Add("PlayerId", mJWTData.PlayerId);
                        break;
                    }

                case AuthenticationType.None:
                default:
                    {
                        enable = false;
                        break;
                    }
            }

            var json = JsonConvert.SerializeObject(data);
            var request = UnityWebRequest.Put(TestShell.HttpServerAddress + $"configure_authentication?Enable={enable}", json);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SendWebRequest().completed += op =>
            {
                Debug.Log(request.result);
            };
        }
    }
}
