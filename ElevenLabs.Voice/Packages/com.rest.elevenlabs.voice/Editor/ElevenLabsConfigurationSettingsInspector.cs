// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ElevenLabs.Editor
{
    [CustomEditor(typeof(ElevenLabsConfigurationSettings))]
    internal class ElevenLabsConfigurationSettingsInspector : UnityEditor.Editor
    {
        private SerializedProperty authInfo;
        private SerializedProperty apiKey;

        #region Project Settings Window

        [SettingsProvider]
        private static SettingsProvider Preferences()
            => new SettingsProvider("Project/ElevenLabs", SettingsScope.Project, new[] { "ElevenLabs" })
            {
                label = "ElevenLabs",
                guiHandler = OnPreferencesGui,
                keywords = new[] { "ElevenLabs" }
            };

        private static void OnPreferencesGui(string searchContext)
        {
            if (EditorApplication.isPlaying ||
                EditorApplication.isCompiling)
            {
                return;
            }

            var instance = GetOrCreateInstance();
            var instanceEditor = CreateEditor(instance);
            instanceEditor.OnInspectorGUI();
        }

        #endregion Project Settings Window

        #region Inspector Window

        private void OnEnable()
        {
            GetOrCreateInstance(target);

            try
            {
                authInfo = serializedObject.FindProperty(nameof(authInfo));
                apiKey = authInfo.FindPropertyRelative(nameof(apiKey));
            }
            catch (Exception)
            {
                // throw away
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;

            var apiKeyContent = new GUIContent(apiKey.displayName, apiKey.tooltip);
            apiKey.stringValue = EditorGUILayout.TextField(apiKeyContent, apiKey.stringValue);

            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }

        #endregion Inspector Window

        private static ElevenLabsConfigurationSettings GetOrCreateInstance(Object target = null)
        {
            var update = false;
            ElevenLabsConfigurationSettings instance;

            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
                update = true;
            }

            if (target != null)
            {
                instance = target as ElevenLabsConfigurationSettings;

                var currentPath = AssetDatabase.GetAssetPath(instance);

                if (string.IsNullOrWhiteSpace(currentPath))
                {
                    return instance;
                }

                if (!currentPath.Contains("Resources"))
                {
                    var newPath = $"Assets/Resources/{instance!.name}.asset";

                    if (!File.Exists(newPath))
                    {
                        File.Move(Path.GetFullPath(currentPath), Path.GetFullPath(newPath));
                        File.Move(Path.GetFullPath($"{currentPath}.meta"), Path.GetFullPath($"{newPath}.meta"));
                    }
                    else
                    {
                        AssetDatabase.DeleteAsset(currentPath);
                        var instances = AssetDatabase.FindAssets($"t:{nameof(ElevenLabsConfigurationSettings)}");
                        var path = AssetDatabase.GUIDToAssetPath(instances[0]);
                        instance = AssetDatabase.LoadAssetAtPath<ElevenLabsConfigurationSettings>(path);
                    }

                    update = true;
                }
            }
            else
            {
                var instances = AssetDatabase.FindAssets($"t:{nameof(ElevenLabsConfigurationSettings)}");

                if (instances.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(instances[0]);
                    instance = AssetDatabase.LoadAssetAtPath<ElevenLabsConfigurationSettings>(path);
                }
                else
                {
                    instance = CreateInstance<ElevenLabsConfigurationSettings>();
                    AssetDatabase.CreateAsset(instance, $"Assets/Resources/{nameof(ElevenLabsConfigurationSettings)}.asset");
                    update = true;
                }
            }

            if (update)
            {
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    EditorGUIUtility.PingObject(instance);
                };
            }

            return instance;
        }
    }
}
