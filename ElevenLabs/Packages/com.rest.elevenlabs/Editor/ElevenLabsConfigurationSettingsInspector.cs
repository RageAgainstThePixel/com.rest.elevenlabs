// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ElevenLabs.Editor
{
    [CustomEditor(typeof(ElevenLabsConfiguration))]
    internal class ElevenLabsConfigurationSettingsInspector : UnityEditor.Editor
    {
        private SerializedProperty apiKey;
        private SerializedProperty proxyDomain;
        private SerializedProperty apiVersion;

        private static bool itemsUpdated;

        #region Project Settings Window

        [SettingsProvider]
        private static SettingsProvider Preferences()
            => new SettingsProvider($"Project/{nameof(ElevenLabs)}", SettingsScope.Project, new[] { nameof(ElevenLabs) })
            {
                label = nameof(ElevenLabs),
                guiHandler = OnPreferencesGui,
                keywords = new[] { nameof(ElevenLabs) },
                deactivateHandler = DeactivateHandler
            };

        private static void DeactivateHandler()
        {
            if (itemsUpdated)
            {
                itemsUpdated = false;
                EditorUtility.RequestScriptReload();
            }
        }

        private static void OnPreferencesGui(string searchContext)
        {
            if (EditorApplication.isPlaying ||
                EditorApplication.isCompiling)
            {
                return;
            }

            var instance = GetOrCreateInstance();

            if (Selection.activeObject != instance)
            {
                Selection.activeObject = instance;
            }

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
                apiKey = serializedObject.FindProperty(nameof(apiKey));
                proxyDomain = serializedObject.FindProperty(nameof(proxyDomain));
                apiVersion = serializedObject.FindProperty(nameof(apiVersion));
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        private void OnDisable()
        {
            if (itemsUpdated)
            {
                itemsUpdated = false;
                EditorUtility.RequestScriptReload();
            }
        }

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
                itemsUpdated = true;
            }

            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }

        #endregion Inspector Window

        private static ElevenLabsConfiguration GetOrCreateInstance(Object target = null)
        {
            var update = false;
            ElevenLabsConfiguration instance;

            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
                update = true;
            }

            if (target != null)
            {
                instance = target as ElevenLabsConfiguration;

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
                        var instances = AssetDatabase.FindAssets($"t:{nameof(ElevenLabsConfiguration)}");
                        var path = AssetDatabase.GUIDToAssetPath(instances[0]);
                        instance = AssetDatabase.LoadAssetAtPath<ElevenLabsConfiguration>(path);
                    }

                    update = true;
                }
            }
            else
            {
                var instances = AssetDatabase.FindAssets($"t:{nameof(ElevenLabsConfiguration)}");

                if (instances.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(instances[0]);
                    instance = AssetDatabase.LoadAssetAtPath<ElevenLabsConfiguration>(path);
                }
                else
                {
                    instance = CreateInstance<ElevenLabsConfiguration>();
                    AssetDatabase.CreateAsset(instance, $"Assets/Resources/{nameof(ElevenLabsConfiguration)}.asset");
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
