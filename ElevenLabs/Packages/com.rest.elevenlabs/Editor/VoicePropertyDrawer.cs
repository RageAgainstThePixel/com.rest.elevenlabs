// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using UnityEditor;
using UnityEngine;

namespace ElevenLabs.Editor
{
    [CustomPropertyDrawer(typeof(Voice))]
    public class VoicePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var voiceName = property.FindPropertyRelative("name");
            var id = property.FindPropertyRelative("id");
            EditorGUI.LabelField(position, new GUIContent("Voice"), new GUIContent(voiceName.stringValue, id.stringValue));
        }
    }
}
