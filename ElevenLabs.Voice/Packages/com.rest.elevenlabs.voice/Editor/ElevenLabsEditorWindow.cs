// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.History;
using ElevenLabs.User;
using ElevenLabs.Voices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ElevenLabs.Editor
{
    public sealed class ElevenLabsEditorWindow : EditorWindow
    {
        #region UX Content

        private const float SettingsLabelWidth = 1.56f;

        private const float InnerLabelWidth = 1.9f;

        private const int InnerLabelIndentLevel = 13;

        private static readonly GUIContent guiTitleContent = new GUIContent("Eleven Labs Dashboard");

        private static readonly GUIContent voiceModelContent = new GUIContent("Voice Model");

        private static readonly GUIContent stabilityContent = new GUIContent("Stability");

        private static readonly GUIContent moreVariableContent = new GUIContent("More Variable", "Increasing variability can make speech more expressive with output varying between re-generations. It can also lead to instabilities.");

        private static readonly GUIContent moreStableContent = new GUIContent("More Stable", "Increasing stability will make the voice more consistent between re-generations, but it can also make it sounds a bit monotone. On longer text fragments we recommend lowering this value.");

        private static readonly GUIContent clarityContent = new GUIContent("Clarity + Similarity Enhancement");

        private static readonly GUIContent lowClarityContent = new GUIContent("Low", "Low values are recommended if background artifacts are present in generated speech.");

        private static readonly GUIContent highClarityContent = new GUIContent("High", "Recommended. High enhancement boosts overall voice clarity and target speaker similarity. Very high values can cause artifacts, so adjusting this setting to find the optimal value is encouraged.");

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

        #endregion UX Content

        private static ElevenLabsClient api;

        private static bool hasFetchedUserInfo;

        private static bool isFetchingUserInfo;

        private static SubscriptionInfo userInfo;

        private static bool hasFetchedVoices;

        private static bool isFetchingVoices;

        private static IReadOnlyList<Voice> voices;

        private static GUIContent[] voiceOptions = Array.Empty<GUIContent>();

        private static Voice currentVoiceOption;

        private static VoiceSettings currentVoiceSettings;

        private static readonly Dictionary<string, Dictionary<string, string>> voiceLabels = new Dictionary<string, Dictionary<string, string>>();

        private static bool hasFetchedHistory;

        private static bool isFetchingHistory;

        private static IReadOnlyList<HistoryItem> history;

        [SerializeField]
        private int tab;

        [SerializeField]
        private string currentVoiceId;

        [SerializeField]
        private Vector2 voiceSettingsSliderValues = Vector2.zero;

        private Vector2 scrollPosition = Vector2.zero;

        private string speechSynthesisTextInput = string.Empty;

        private string editorDownloadDirectory = string.Empty;

        private AudioClip newSampleClip;

        private string tempLabelKey;

        private string tempLabelValue;

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
            minSize = new Vector2(640, 256);
        }

        private void OnFocus()
        {
            api ??= new ElevenLabsClient();

            if (!hasFetchedUserInfo ||
                userInfo == null)
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

            if (string.IsNullOrWhiteSpace(editorDownloadDirectory))
            {
                editorDownloadDirectory = EditorPrefs.GetString($"{Application.productName}_ElevenLabs_EditorDownloadDirectory", $"{Application.streamingAssetsPath}/{nameof(ElevenLabs)}");
            }

            EditorGUILayout.TextField(editorDownloadDirectory);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(20);

                if (GUILayout.Button("Change Save Directory", GUILayout.ExpandWidth(true)))
                {
                    EditorApplication.delayCall += () =>
                    {
                        var result = EditorUtility.OpenFolderPanel("Save Directory", editorDownloadDirectory, string.Empty);

                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            editorDownloadDirectory = result;
                            EditorPrefs.SetString($"{Application.productName}_ElevenLabs_EditorDownloadDirectory", editorDownloadDirectory);
                        }
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

        private static async void FetchUserInfo()
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
                voiceLabels.Clear();
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

                GUI.enabled = !isFetchingVoices || !isFetchingUserInfo;

                if (GUILayout.Button("Refresh"))
                {
                    EditorApplication.delayCall += () =>
                    {
                        FetchVoices();
                        FetchUserInfo();
                    };
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
                EditorGUIUtility.labelWidth = 128 * SettingsLabelWidth;

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

                        if (currentVoiceSettings == null ||
                            voiceSettingsSliderValues == Vector2.zero)
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
                    StartIndent(InnerLabelIndentLevel);
                    EditorGUIUtility.labelWidth = 128 * InnerLabelWidth;
                    EditorGUILayout.LabelField(moreVariableContent, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(moreStableContent, RightMiddleAlignedLabel, GUILayout.ExpandWidth(true));
                    EditorGUIUtility.labelWidth = 128 * SettingsLabelWidth;
                    EndIndent(InnerLabelIndentLevel);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
                voiceSettingsSliderValues.y = EditorGUILayout.Slider(clarityContent, voiceSettingsSliderValues.y, 0f, 1f);
                EditorGUILayout.BeginHorizontal();
                {
                    StartIndent(InnerLabelIndentLevel);
                    EditorGUIUtility.labelWidth = 128 * InnerLabelWidth;
                    EditorGUILayout.LabelField(lowClarityContent, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(highClarityContent, RightMiddleAlignedLabel, GUILayout.ExpandWidth(true));
                    EditorGUIUtility.labelWidth = 128 * SettingsLabelWidth;
                    EndIndent(InnerLabelIndentLevel);
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

        #region Voice Lab

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
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (voices == null) { return; }

            foreach (var voice in voices)
            {
                if (voice.Category.Contains("premade")) { continue; }

                var isCloned = voice.Category.Contains("cloned");

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField($"{voice.Category}/{voice.Name} | {voice.Id}", GUILayout.ExpandWidth(true));

                    GUI.enabled = !isFetchingVoices;

                    if (GUILayout.Button("Delete"))
                    {
                        EditorApplication.delayCall += () => DeleteVoice(voice);
                    }

                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;

                if (!voiceLabels.TryGetValue(voice.Id, out var cachedLabels))
                {
                    cachedLabels = new Dictionary<string, string>();

                    foreach (var (key, value) in voice.Labels.OrderBy(pair => pair.Key))
                    {
                        cachedLabels.TryAdd(key, value);
                    }

                    voiceLabels.TryAdd(voice.Id, cachedLabels);
                }

                EditorGUILayout.LabelField($"Labels {cachedLabels?.Count ?? 0}/5", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                const string labelControlName = "labelKeyTextField";

                if (cachedLabels is { Count: > 0 })
                {
                    var labelIndex = 0;

                    foreach (var cachedLabel in cachedLabels)
                    {
                        labelIndex++;
                        EditorGUILayout.BeginHorizontal();
                        {
                            var (key, value) = cachedLabel;
                            EditorGUI.BeginChangeCheck();
                            var prevLabelWidth = EditorGUIUtility.labelWidth;
                            EditorGUIUtility.labelWidth = 96f;
                            GUI.SetNextControlName($"{labelControlName}{labelIndex}");

                            EditorGUI.BeginChangeCheck();
                            var newKey = EditorGUILayout.TextField("Key", key);

                            if (EditorGUI.EndChangeCheck())
                            {
                                tempLabelKey = newKey;
                            }

                            if (Event.current.isKey && Event.current.keyCode == KeyCode.Tab &&
                                GUI.GetNameOfFocusedControl() == $"{labelControlName}{labelIndex}" &&
                                !string.IsNullOrWhiteSpace(tempLabelKey))
                            {
                                if (key != tempLabelKey)
                                {
                                    EditorApplication.delayCall += () =>
                                    {
                                        if (cachedLabels.TryGetValue(key, out _))
                                        {
                                            cachedLabels.Remove(key);
                                        }

                                        if (!cachedLabels.TryAdd(tempLabelKey, string.Empty))
                                        {
                                            Debug.LogError($"failed to add label {tempLabelKey}");
                                        }

                                        tempLabelKey = string.Empty;
                                    };
                                }
                            }

                            EditorGUI.BeginChangeCheck();
                            var newValue = EditorGUILayout.TextField("Value", value);
                            EditorGUIUtility.labelWidth = prevLabelWidth;

                            if (EditorGUI.EndChangeCheck())
                            {
                                tempLabelValue = newValue;

                                EditorApplication.delayCall += () =>
                                {
                                    if (!string.IsNullOrWhiteSpace(tempLabelValue))
                                    {
                                        cachedLabels[key] = tempLabelValue;
                                    }

                                    tempLabelValue = string.Empty;
                                };
                            }

                            GUI.enabled = !isFetchingVoices;

                            if (GUILayout.Button("Delete", GUILayout.Width(96)))
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    cachedLabels.Remove(key);
                                };
                            }

                            GUI.enabled = true;
                        }

                        GUILayout.Space(10);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.Space();
                GUI.enabled = !isFetchingVoices;

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.Space(40);

                    if (cachedLabels is { Count: < 5 } &&
                        GUILayout.Button("Add new Label", GUILayout.Width(96)))
                    {
                        tempLabelKey = string.Empty;

                        EditorApplication.delayCall += () =>
                        {
                            cachedLabels.TryAdd("New Label", string.Empty);
                            EditorGUI.FocusTextInControl($"{labelControlName}{cachedLabels.Count}");
                        };
                    }

                    GUILayout.FlexibleSpace();

                    bool IsLabelsDirty()
                    {
                        var isDirty = false;

                        foreach (var (key, cachedValue) in cachedLabels)
                        {
                            if (voice.Labels.TryGetValue(key, out var realValue))
                            {
                                isDirty |= cachedValue != realValue;
                            }
                            else
                            {
                                isDirty = true;
                            }
                        }

                        foreach (var (key, realValue) in voice.Labels)
                        {
                            if (cachedLabels.TryGetValue(key, out var cachedValue))
                            {
                                isDirty |= cachedValue != realValue;
                            }
                            else
                            {
                                isDirty = true;
                            }
                        }

                        return isDirty;
                    }

                    GUI.enabled = !isFetchingVoices && IsLabelsDirty();

                    if (GUILayout.Button("Update Labels", GUILayout.Width(96)))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            EditVoice(voice, null, cachedLabels);
                        };
                    }

                    GUILayout.Space(10);
                }

                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;

                if (isCloned)
                {

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"Samples {voice.Samples?.Count ?? 0}/25", EditorStyles.boldLabel);
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel++;

                    if ((voice.Samples?.Count ?? 0) < 25)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            newSampleClip = EditorGUILayout.ObjectField("New Sample", newSampleClip, typeof(AudioClip), false, GUILayout.ExpandWidth(true)) as AudioClip;

                            GUI.enabled = newSampleClip != null && !isFetchingVoices;

                            if (GUILayout.Button("Add Sample", GUILayout.Width(96)))
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    EditVoice(voice, newSampleClip);
                                    newSampleClip = null;
                                };
                            }

                            GUI.enabled = true;
                        }
                        GUILayout.Space(10);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (voice.Samples != null)
                {
                    foreach (var voiceSample in voice.Samples)
                    {
                        EditorGUILayout.LabelField($"{voiceSample.Id} | {voiceSample.FileName} | {voiceSample.MimeType} | {voiceSample.SizeBytes}");
                        EditorGUILayout.BeginHorizontal();
                        var fileName = Path.GetFileNameWithoutExtension(voiceSample.FileName);
                        var files = AssetDatabase.FindAssets($"t:{nameof(AudioClip)} {fileName}").ToList();
                        files.AddRange(AssetDatabase.FindAssets($"t:{nameof(AudioClip)} {voiceSample.Id}"));

                        switch (files)
                        {
                            case { Count: 0 }:
                                GUI.enabled = !isFetchingVoices;

                                if (GUILayout.Button("Download"))
                                {
                                    EditorApplication.delayCall += () => DownloadVoiceSample(voice, voiceSample);
                                }

                                GUI.enabled = true;
                                break;
                            case { Count: 1 }:
                                var clipPath = AssetDatabase.GUIDToAssetPath(files[0]);
                                var sampleClip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
                                EditorGUILayout.ObjectField(GUIContent.none, sampleClip, typeof(AudioClip), false);
                                break;
                            default:
                                EditorGUILayout.LabelField($"Found multiple matches for {fileName}");
                                break;
                        }

                        GUI.enabled = !isFetchingVoices;

                        if (GUILayout.Button("Delete", GUILayout.Width(96)))
                        {
                            EditorApplication.delayCall += () => DeleteVoiceSample(voice, voiceSample);
                        }

                        GUI.enabled = true;
                        GUILayout.Space(10);
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel--;
        }

        private static async void DeleteVoice(Voice voice)
        {
            if (!EditorUtility.DisplayDialog(
                    "Alert!",
                    $"Are you sure you want to delete voice {voice.Id} \"{voice.Name}\"?", "Yes",
                    "No"))
            {
                return;
            }

            try
            {
                var result = await api.VoicesEndpoint.DeleteVoiceAsync(voice);

                if (!result)
                {
                    Debug.LogError($"Failed to delete voice: {voice.Name}!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                FetchVoices();
            }
        }

        private static async void EditVoice(Voice voice, AudioClip audioClip = null, Dictionary<string, string> labels = null)
        {
            try
            {
                var audioClipPaths = new List<string>();

                if (audioClip != null)
                {
                    EditorUtility.DisplayProgressBar("Uploading voice sample...", $"Uploading {audioClip.name}", -1);
                    var audioClipPath = Path.GetFullPath(AssetDatabase.GetAssetPath(audioClip));

                    if (string.IsNullOrWhiteSpace(audioClipPath))
                    {
                        throw new ArgumentNullException(nameof(audioClipPath), $"AssetDatabase failed to locate {audioClip.name}!");
                    }

                    if (!File.Exists(audioClipPath))
                    {
                        throw new ArgumentNullException(nameof(audioClipPath), $"Failed to find valid path to {audioClip.name}: \"{audioClipPath}\"");
                    }

                    audioClipPaths.Add(audioClipPath);
                }
                else
                {
                    EditorUtility.DisplayProgressBar("Updating voice labels...", $"Updating voice labels for {voice.Id}", -1);

                    if (labels == null)
                    {
                        throw new ArgumentNullException(nameof(labels));
                    }
                }

                var result = await api.VoicesEndpoint.EditVoiceAsync(voice, audioClipPaths, labels);

                if (!result)
                {
                    Debug.LogWarning("Failed to update voice!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                FetchVoices();
            }
        }

        private static async void DeleteVoiceSample(Voice voice, Sample voiceSample)
        {
            if (!EditorUtility.DisplayDialog(
                    "Alert!",
                    $"Are you sure you want to delete sample {voiceSample.Id} from {voice.Name}?", "Yes",
                    "No"))
            {
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("Deleting voice sample...", $"Deleting {voiceSample.Id}", -1);
                await api.VoicesEndpoint.DeleteVoiceSampleAsync(voice, voiceSample);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                FetchVoices();
            }
        }

        private async void DownloadVoiceSample(Voice sample, Sample voiceSample)
        {
            try
            {
                await api.VoicesEndpoint.GetVoiceSampleAsync(sample, voiceSample, editorDownloadDirectory);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }

        #endregion Voice Lab

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
