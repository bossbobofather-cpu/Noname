using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Noname.GameAbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(FGameplayTag))]
    public sealed class GameplayTagDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 52f;
        private const float HelpBoxHeight = 32f;
        private static readonly GUIContent TagsButtonContent = new("Tags");
        private static readonly GUIContent NoneContent = new("<None>");
        private static GameplayTagRegistry _cachedRegistry;
        private static double _lastRegistryCheck;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("_value");
            if (valueProp == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            lineRect = EditorGUI.IndentedRect(lineRect);
            var contentRect = EditorGUI.PrefixLabel(lineRect, label);

            var registry = GetRegistry();
            if (registry != null)
            {
                var buttonRect = new Rect(contentRect.xMax - ButtonWidth, contentRect.y, ButtonWidth, contentRect.height);
                var fieldRect = contentRect;
                fieldRect.width -= ButtonWidth + 4f;

                var newValue = EditorGUI.DelayedTextField(fieldRect, GUIContent.none, valueProp.stringValue);
                if (newValue != valueProp.stringValue)
                {
                    valueProp.stringValue = newValue;
                }

                if (EditorGUI.DropdownButton(buttonRect, TagsButtonContent, FocusType.Passive))
                {
                    ShowTagMenu(valueProp, registry);
                }
            }
            else
            {
                var newValue = EditorGUI.DelayedTextField(contentRect, GUIContent.none, valueProp.stringValue);
                if (newValue != valueProp.stringValue)
                {
                    valueProp.stringValue = newValue;
                }
            }

            var message = GetValidationMessage(valueProp.stringValue, registry);
            if (!string.IsNullOrEmpty(message))
            {
                var helpRect = new Rect(position.x, lineRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                    position.width, HelpBoxHeight);
                helpRect = EditorGUI.IndentedRect(helpRect);
                EditorGUI.HelpBox(helpRect, message, MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("_value");
            var registry = GetRegistry();
            var message = valueProp == null ? string.Empty : GetValidationMessage(valueProp.stringValue, registry);
            if (string.IsNullOrEmpty(message))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + HelpBoxHeight;
        }

        private static void ShowTagMenu(SerializedProperty valueProp, GameplayTagRegistry registry)
        {
            var menu = new GenericMenu();
            menu.AddItem(NoneContent, string.IsNullOrEmpty(valueProp.stringValue), () =>
            {
                valueProp.stringValue = string.Empty;
                valueProp.serializedObject.ApplyModifiedProperties();
            });

            var tags = registry.GetAllTags(includeParents: true);
            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                menu.AddItem(new GUIContent(tag),
                    string.Equals(valueProp.stringValue, tag, StringComparison.Ordinal),
                    () =>
                    {
                        valueProp.stringValue = tag;
                        valueProp.serializedObject.ApplyModifiedProperties();
                    });
            }

            menu.ShowAsContext();
        }

        private static GameplayTagRegistry GetRegistry()
        {
            if (_cachedRegistry != null)
            {
                return _cachedRegistry;
            }

            if (EditorApplication.timeSinceStartup - _lastRegistryCheck < 1.0f)
            {
                return _cachedRegistry;
            }

            _lastRegistryCheck = EditorApplication.timeSinceStartup;
            var guids = AssetDatabase.FindAssets("t:GameplayTagRegistry");
            if (guids.Length == 0)
            {
                _cachedRegistry = null;
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedRegistry = AssetDatabase.LoadAssetAtPath<GameplayTagRegistry>(path);
            return _cachedRegistry;
        }

        private static string GetValidationMessage(string value, GameplayTagRegistry registry)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (!GameplayTagUtility.IsValidTagString(value))
            {
                return "Invalid tag format. Use A.B.C with letters, digits, or underscore.";
            }

            if (registry != null && !registry.IsTagDefined(value, includeParents: true))
            {
                return "Tag not found in GameplayTagRegistry.";
            }

            return string.Empty;
        }
    }
}
