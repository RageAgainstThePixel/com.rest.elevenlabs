// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEditor;
using UnityEngine;
using Utilities.Rest.Editor;

namespace ElevenLabs.Editor
{
    [CustomEditor(typeof(ElevenLabsConfiguration))]
    internal class ElevenLabsConfigurationInspector : BaseConfigurationInspector<ElevenLabsConfiguration>
    {
        private static bool triggerReload;

        private SerializedProperty apiKey;
        private SerializedProperty proxyDomain;
        private SerializedProperty apiVersion;

        #region Project Settings Window

        [SettingsProvider]
        private static SettingsProvider Preferences() => GetSettingsProvider(nameof(ElevenLabs), CheckReload);

        #endregion Project Settings Window

        #region Inspector Window

        private void OnEnable()
        {
            GetOrCreateInstance(target);

            try
            {
                apiKey = serializedObject.FindProperty(nameof(apiKey));
                proxyDomain = serializedObject.FindProperty(nameof(proxyDomain));
                apiVersion = serializedObject.FindProperty(nameof(apiVersion));
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        private void OnDisable() => CheckReload();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(apiKey);
            EditorGUILayout.PropertyField(proxyDomain);

            GUI.enabled = false;

            if (string.IsNullOrWhiteSpace(apiVersion.stringValue) ||
                apiVersion.stringValue != ElevenLabsSettingsInfo.DefaultApiVersion)
            {
                apiVersion.stringValue = ElevenLabsSettingsInfo.DefaultApiVersion;
            }

            EditorGUILayout.PropertyField(apiVersion);
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                triggerReload = true;
            }

            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }

        #endregion Inspector Window
        private static void CheckReload()
        {
            if (triggerReload)
            {
                triggerReload = false;
                EditorUtility.RequestScriptReload();
            }
        }
    }
}
