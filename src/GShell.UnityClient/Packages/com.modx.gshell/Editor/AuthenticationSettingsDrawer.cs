using System;
using UnityEditor;
using UnityEngine;

namespace GShell.Editor
{
    [CustomPropertyDrawer(typeof(AuthenticationSettings))]
    public class AuthenticationSettingsDrawer : PropertyDrawer
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
            public string Token;
        }

        private BasicData mBasicData;
        private JWTData mJWTData;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded,
                label,
                true
            );

            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;

            SerializedProperty typeProp = property.FindPropertyRelative("Type");
            SerializedProperty dataProp = property.FindPropertyRelative("Data");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect typeRect = new Rect(
                position.x,
                position.y + lineHeight + spacing,
                position.width,
                lineHeight
            );

            EditorGUI.PropertyField(typeRect, typeProp);

            switch ((AuthenticationType)typeProp.enumValueIndex)
            {
                case AuthenticationType.Basic:
                    ShowBasicGUI(position.x, typeRect.y, position.width, dataProp);
                    break;

                case AuthenticationType.JWT:
                    ShowJWTGUI(position.x, typeRect.y, position.width, dataProp);
                    break;

                case AuthenticationType.None:
                default:
                    dataProp.stringValue = string.Empty;
                    break;
            }

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty typeProp = property.FindPropertyRelative("Type");

                if ((AuthenticationType)typeProp.enumValueIndex == AuthenticationType.Basic)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                switch ((AuthenticationType)typeProp.enumValueIndex)
                {
                    case AuthenticationType.Basic:
                        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        break;

                    case AuthenticationType.JWT:
                        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        break;

                    case AuthenticationType.None:
                    default:
                        break;
                }
            }

            return height;
        }

        private void ShowBasicGUI(float x, float y, float width, SerializedProperty prop)
        {
            if (string.IsNullOrEmpty(prop.stringValue))
                prop.stringValue = JsonUtility.ToJson(new BasicData());

            if (mBasicData == null)
                mBasicData = JsonUtility.FromJson<BasicData>(prop.stringValue);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect userNameRect = new Rect(
                x,
                y + lineHeight + spacing,
                width,
                lineHeight
            );

            Rect passwordRect = new Rect(
               x,
               userNameRect.y + lineHeight + spacing,
               width,
               lineHeight
            );

            mBasicData.UserName = EditorGUI.TextField(userNameRect, new GUIContent("UserName"), mBasicData.UserName);
            mBasicData.Password = EditorGUI.TextField(passwordRect, new GUIContent("Password"), mBasicData.Password);

            prop.stringValue = JsonUtility.ToJson(mBasicData);
        }

        private void ShowJWTGUI(float x, float y, float width, SerializedProperty prop)
        {
            if (string.IsNullOrEmpty(prop.stringValue))
                prop.stringValue = JsonUtility.ToJson(new JWTData());

            if (mJWTData == null)
                mJWTData = JsonUtility.FromJson<JWTData>(prop.stringValue);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect tokenRect = new Rect(
                x,
                y + lineHeight + spacing,
                width,
                lineHeight
            );

            mJWTData.Token = EditorGUI.TextField(tokenRect, new GUIContent("Token"), mJWTData.Token);

            prop.stringValue = JsonUtility.ToJson(mJWTData);
        }
    }
}
