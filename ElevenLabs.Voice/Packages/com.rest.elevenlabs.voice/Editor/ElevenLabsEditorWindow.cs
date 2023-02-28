// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using ElevenLabs.History;
using ElevenLabs.User;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Serialization;
using Task = System.Threading.Tasks.Task;

namespace ElevenLabs.Editor
{
    public sealed class ElevenLabsEditorWindow : EditorWindow
    {
        private static readonly GUIContent guiTitleContent = new GUIContent("Eleven Labs Dashboard");

        private static readonly GUIContent voiceModelContent = new GUIContent("Voice Model");

        private static readonly GUIContent stabilityContent = new GUIContent("Stability");

        private static readonly GUIContent moreVariableContent = new GUIContent("More Variable", "Increasing variability can make speech more expressive with output varying between re-generations. It can also lead to instabilities.");

        private static readonly GUIContent moreStableContent = new GUIContent("More Stable", "Increasing stability will make the voice more consistent between re-generations, but it can also make it sounds a bit monotone. On longer text fragments we recommend lowering this value.");

        private static readonly GUIContent clarityContent = new GUIContent("Clarity + Similarity Enhancement");

        private static readonly string[] tabTitles = { "Speech Synthesis", "Voice Lab", "History" };

        private static GUIStyle boldCenteredHeaderStyle;

        private static GUIStyle BoldCenteredHeaderStyle
        {
            get
            {
                if (boldCenteredHeaderStyle == null)
                {
                    var editorStyle = EditorGUIUtility.isProSkin ? EditorStyles.whiteLargeLabel : EditorStyles.largeLabel;

                    if (editorStyle != null)
                    {
                        boldCenteredHeaderStyle = new GUIStyle(editorStyle)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 18,
                            padding = new RectOffset(0, 0, -8, -8)
                        };
                    }
                }

                return boldCenteredHeaderStyle;
            }
        }

        private static GUIStyle rightMiddleAlignedLabel;

        private static GUIStyle RightMiddleAlignedLabel => rightMiddleAlignedLabel ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleRight,
        };

        private float settingsLabelWidth = 1.56f;
        private float innerLabelWidth = 1.9f;
        private int indentLevel = 6;

        private static Vector2 scrollPosition = Vector2.zero;

        private static ElevenLabsClient api;

        private static bool hasFetchedUserInfo;

        private static bool isFetchingUserInfo;

        private SubscriptionInfo userInfo;

        private static bool hasFetchedVoices;

        private static bool isFetchingVoices;

        private static IReadOnlyList<Voice> voices;

        private static GUIContent[] voiceOptions = Array.Empty<GUIContent>();

        private static Voice currentVoiceOption;

        private static VoiceSettings currentVoiceSettings;

        private static bool hasFetchedHistory;

        private static bool isFetchingHistory;

        private static IReadOnlyList<HistoryItem> history;

        [SerializeField]
        private int tab;

        private string editorDownloadDirectory = $"{Application.streamingAssetsPath}/{nameof(ElevenLabs)}";

        [SerializeField]
        private string currentVoiceId;

        [SerializeField]
        private Vector2 voiceSettingsSliderValues = Vector2.zero;

        private string speechSynthesisTextInput;

        [MenuItem("ElevenLabs/Dashboard")]
        private static void OpenWindow()
        {
            // Dock it next to the Scene View.
            var instance = GetWindow<ElevenLabsEditorWindow>(typeof(SceneView));
            instance.Show();
            instance.titleContent = guiTitleContent;
        }

        private void OnEnable()
        {
            titleContent = guiTitleContent;
            minSize = new Vector2(512, 256);
        }

        private void OnFocus()
        {
            api ??= new ElevenLabsClient();

            if (!hasFetchedUserInfo)
            {
                hasFetchedUserInfo = true;
                FetchUserInfo();
            }

            if (!hasFetchedVoices)
            {
                hasFetchedVoices = true;
                FetchVoices();
            }

            if (!hasFetchedHistory)
            {
                hasFetchedHistory = true;
                FetchHistory();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Eleven Labs Dashboard", BoldCenteredHeaderStyle);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (api == null ||
                string.IsNullOrWhiteSpace(api.ElevenLabsAuthentication.ApiKey))
            {
                EditorGUILayout.HelpBox($"No valid {nameof(ElevenLabsConfigurationSettings)} was found. This tool requires that you set your API key.", MessageType.Error);
                return;
            }

            tab = GUILayout.Toolbar(tab, tabTitles);

            EditorGUILayout.Space();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(new GUIContent("Save Directory"));
            EditorGUILayout.TextField(editorDownloadDirectory);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(20);

                if (GUILayout.Button("Change Save Directory", GUILayout.ExpandWidth(true)))
                {
                    EditorApplication.delayCall += () =>
                    {
                        editorDownloadDirectory = EditorUtility.OpenFolderPanel("Save Directory", editorDownloadDirectory, string.Empty);
                    };
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            switch (tab)
            {
                case 0:
                    RenderSpeechSynthesis();
                    break;
                case 1:
                    RenderVoiceLab();
                    break;
                case 2:
                    RenderHistory();
                    break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private async void FetchUserInfo()
        {
            if (isFetchingUserInfo) { return; }
            isFetchingUserInfo = true;

            try
            {
                userInfo = await api.UserEndpoint.GetSubscriptionInfoAsync();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isFetchingUserInfo = false;
            }
        }

        private static async void FetchVoices()
        {
            if (isFetchingVoices) { return; }
            isFetchingVoices = true;

            try
            {
                voices = await api.VoicesEndpoint.GetAllVoicesAsync();
                voiceOptions = voices.OrderBy(voice => voice.Name).Select(voice => new GUIContent($"{voice.Category}/{voice.Name}")).ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isFetchingVoices = false;
            }
        }

        private static async void FetchHistory()
        {
            if (isFetchingHistory) { return; }
            isFetchingHistory = true;

            try
            {
                history = await api.HistoryEndpoint.GetHistoryAsync();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isFetchingHistory = false;
            }
        }

        #region Voice Synthesis

        private void RenderSpeechSynthesis()
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            { //Header
                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                GUI.enabled = !isFetchingVoices;

                if (GUILayout.Button("Refresh"))
                {
                    EditorApplication.delayCall += FetchVoices;
                }

                EditorGUILayout.Space(10);
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            { // Body
                var prevLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 128 * settingsLabelWidth;

                EditorGUILayout.BeginHorizontal();
                { // voice dropdown
                    var voiceIndex = -1;

                    currentVoiceOption ??= !string.IsNullOrWhiteSpace(currentVoiceId)
                        ? voices?.FirstOrDefault(voice => voice.Id == currentVoiceId)
                        : voices?.FirstOrDefault();

                    if (currentVoiceOption != null)
                    {
                        for (var i = 0; i < voiceOptions.Length; i++)
                        {
                            if (voiceOptions[i].text.Contains(currentVoiceOption.Name))
                            {
                                voiceIndex = i;
                                break;
                            }
                        }

                        if (currentVoiceSettings == null)
                        {
                            GetDefaultVoiceSettings(currentVoiceOption);
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    voiceIndex = EditorGUILayout.Popup(voiceModelContent, voiceIndex, voiceOptions);

                    if (EditorGUI.EndChangeCheck())
                    {
                        currentVoiceOption = voices?.FirstOrDefault(voice => voiceOptions[voiceIndex].text.Contains(voice.Name));
                        currentVoiceId = currentVoiceOption!.Id;
                        EditorApplication.delayCall += () => GetDefaultVoiceSettings(currentVoiceOption);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

                GUI.enabled = currentVoiceSettings != null && !isGettingDefaultVoiceSettings;
                EditorGUI.BeginChangeCheck();

                voiceSettingsSliderValues.x = EditorGUILayout.Slider(stabilityContent, voiceSettingsSliderValues.x, 0f, 1f);
                EditorGUILayout.BeginHorizontal();
                {
                    StartIndent(indentLevel);
                    EditorGUIUtility.labelWidth = 128 * innerLabelWidth;
                    EditorGUILayout.LabelField(moreVariableContent, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(moreStableContent, RightMiddleAlignedLabel, GUILayout.ExpandWidth(true));
                    EditorGUIUtility.labelWidth = 128 * settingsLabelWidth;
                    EndIndent(indentLevel);
                }
                EditorGUILayout.EndHorizontal();

                voiceSettingsSliderValues.y = EditorGUILayout.Slider(clarityContent, voiceSettingsSliderValues.y, 0f, 1f);
                EditorGUILayout.BeginHorizontal();
                {
                    StartIndent(indentLevel);
                    EditorGUIUtility.labelWidth = 128 * innerLabelWidth;
                    EditorGUILayout.LabelField(moreVariableContent, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(moreStableContent, RightMiddleAlignedLabel, GUILayout.ExpandWidth(true));
                    EditorGUIUtility.labelWidth = 128 * settingsLabelWidth;
                    EndIndent(indentLevel);
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                { // Text Area Header
                    EditorGUILayout.LabelField("Text", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("To Default", GUILayout.Width(128)))
                    {
                        EditorApplication.delayCall += () => GetDefaultVoiceSettings(currentVoiceOption);
                    }
                    else
                    {
                        if (EditorGUI.EndChangeCheck())
                        {
                            currentVoiceSettings = new VoiceSettings(voiceSettingsSliderValues.x, voiceSettingsSliderValues.y);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                GUI.enabled = true;
                EditorGUIUtility.labelWidth = prevLabelWidth;
                GUILayout.Space(10);

                EditorGUI.BeginChangeCheck();
                EditorStyles.textField.wordWrap = true;
                speechSynthesisTextInput = EditorGUILayout.TextArea(speechSynthesisTextInput, GUILayout.ExpandHeight(true));

                if (EditorGUI.EndChangeCheck())
                {
                    if (speechSynthesisTextInput.Length > 5000)
                    {
                        speechSynthesisTextInput = speechSynthesisTextInput[..5000];
                    }
                }

                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                { // Text area footer
                    EditorGUILayout.LabelField(new GUIContent($"{speechSynthesisTextInput.Length} / 5000"), GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    var remainingCharacters = 0;

                    if (userInfo != null)
                    {
                        remainingCharacters = userInfo.CharacterLimit - userInfo.CharacterCount;
                    }

                    EditorGUILayout.LabelField(new GUIContent($"Total quota remaining: {remainingCharacters}"), RightMiddleAlignedLabel, GUILayout.ExpandWidth(true));
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                {
                    GUI.enabled = !isSynthesisRunning;
                    GUILayout.Space(20);
                    if (GUILayout.Button("Generate", GUILayout.ExpandWidth(true)))
                    {
                        EditorApplication.delayCall += GenerateSynthesizedText;
                    }
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        private static bool isSynthesisRunning;

        private async void GenerateSynthesizedText()
        {
            if (isSynthesisRunning) { return; }
            isSynthesisRunning = true;

            try
            {
                if (string.IsNullOrWhiteSpace(speechSynthesisTextInput))
                {
                    throw new ArgumentNullException(nameof(speechSynthesisTextInput));
                }

                if (currentVoiceOption == null)
                {
                    throw new ArgumentNullException(nameof(currentVoiceOption));
                }

                if (currentVoiceSettings == null)
                {
                    throw new ArgumentNullException(nameof(currentVoiceSettings));
                }

                var (clipPath, audioClip) = await api.TextToSpeechEndpoint.TextToSpeechAsync(speechSynthesisTextInput, currentVoiceOption, currentVoiceSettings, editorDownloadDirectory);

                if (clipPath.Contains(Application.dataPath))
                {
                    var importPath = clipPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                    AssetDatabase.ImportAsset(importPath);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    EditorGUIUtility.PingObject(audioClip);
                    Selection.activeObject = audioClip;
                }

                FetchUserInfo();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isSynthesisRunning = false;
                speechSynthesisTextInput = string.Empty;
            }
        }

        private static bool isGettingDefaultVoiceSettings;

        private async void GetDefaultVoiceSettings(Voice voice)
        {
            if (isGettingDefaultVoiceSettings) { return; }
            isGettingDefaultVoiceSettings = true;

            try
            {
                currentVoiceSettings = await api.VoicesEndpoint.GetVoiceSettingsAsync(voice);
                voiceSettingsSliderValues = new Vector2(currentVoiceSettings.Stability, currentVoiceSettings.SimilarityBoost);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isGettingDefaultVoiceSettings = false;
            }
        }

        #endregion Voice Synthesis

        private void RenderVoiceLab()
        {
            EditorGUI.indentLevel++;

            var voiceCount = string.Empty;

            if (voices != null && userInfo != null)
            {
                var generatedCount = voices.Count(voice => voice.Category is "generated" or "cloned");
                voiceCount = $"{generatedCount}/{userInfo.VoiceLimit}";
            }

            EditorGUILayout.BeginHorizontal();
            { //Header
                EditorGUILayout.LabelField($"Voices {voiceCount}", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                GUI.enabled = !isFetchingVoices;

                if (GUILayout.Button("Refresh"))
                {
                    EditorApplication.delayCall += FetchVoices;
                }

                EditorGUILayout.Space(10);
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            if (voices == null) { return; }

            foreach (var voice in voices)
            {
                if (voice.Category.Contains("premade")) { continue; }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{voice.Category}/{voice.Name} | {voice.Id}");
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;

                var labels = voice.Labels.Select(label => $"{label.Key}: {label.Value}").ToList();

                if (labels is { Count: > 0 })
                {
                    EditorGUILayout.LabelField($"({string.Join(" | ", labels)})");
                }

                if (voice.Samples != null)
                {
                    foreach (var voiceSample in voice.Samples)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{voiceSample.FileName} | {voiceSample.MimeType} | {voiceSample.SizeBytes}");
                        var fileName = Path.GetFileNameWithoutExtension(voiceSample.FileName);
                        var files = AssetDatabase.FindAssets($"t:{nameof(AudioClip)} {fileName}");

                        switch (files)
                        {
                            case { Length: 0 }:
                                if (GUILayout.Button("Download"))
                                {
                                    EditorApplication.delayCall += () => DownloadVoiceSample(voice, voiceSample);
                                }

                                break;
                            case { Length: 1 }:
                                var clipPath = AssetDatabase.GUIDToAssetPath(files[0]);
                                var sampleClip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
                                EditorGUILayout.ObjectField(GUIContent.none, sampleClip, typeof(AudioClip), false);

                                break;
                            default:
                                EditorGUILayout.LabelField($"Found multiple matches for {fileName}");

                                break;
                        }

                        if (GUILayout.Button("Delete"))
                        {

                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            EditorGUI.indentLevel--;
        }

        private async void DownloadVoiceSample(Voice sample, Sample voiceSample)
        {
            try
            {
                var sampleClip = await api.VoicesEndpoint.GetVoiceSampleAsync(sample, voiceSample);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private static void RenderHistory()
        {
        }

        #region GUI Utilities

        private static void StartIndent(int levels)
        {
            for (var i = 0; i < levels; i++)
            {
                EditorGUI.indentLevel++;
            }
        }

        private static void EndIndent(int levels)
        {
            for (var i = 0; i < levels; i++)
            {
                EditorGUI.indentLevel--;
            }
        }

        #endregion GUI Utilities
    }
}
