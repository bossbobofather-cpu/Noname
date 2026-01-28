using System;
using System.Collections.Generic;
using Noname.GameAbilitySystem.Domain;
using Noname.GameAbilitySystem.Presentation;
using UnityEditor;
using UnityEngine;

namespace Noname.GameAbilitySystem.Editor
{
    /// <summary>
    /// FGameplayTag를 드롭다운으로 선택할 수 있게 하는 드로어입니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(FGameplayTag))]
    public sealed class GameplayTagDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 52f;
        private const float HelpBoxHeight = 32f;
        private static readonly GUIContent TagsButtonContent = new("Tags");
        private static readonly GUIContent NoneContent = new("<None>");
        private static GameplayTagRegistry _cachedRegistry;
        private static double _lastRegistryCheck;

        /// <summary>
        /// 태그 텍스트 필드와 선택 버튼을 함께 그립니다.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProp = property.FindPropertyRelative("_value");
            if (valueProp == null)
            {
                // 필드가 없으면 기본 표시
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // 프로퍼티 컨텍스트 시작
            EditorGUI.BeginProperty(position, label, property);

            var lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            lineRect = EditorGUI.IndentedRect(lineRect);
            var contentRect = EditorGUI.PrefixLabel(lineRect, label);

            // 레지스트리가 있으면 드롭다운 버튼을 함께 노출
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
                    // 등록된 태그 목록 표시
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

            // 입력값 검증 메시지 표시
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

        /// <summary>
        /// 경고 메시지 유무에 따라 높이를 계산합니다.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 메시지 출력 여부에 따라 높이 계산
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
            // 레지스트리 기준으로 태그 목록 생성
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
            // 캐시가 있으면 즉시 반환
            if (_cachedRegistry != null)
            {
                return _cachedRegistry;
            }

            // 조회 빈도 제한
            if (EditorApplication.timeSinceStartup - _lastRegistryCheck < 1.0f)
            {
                return _cachedRegistry;
            }

            // 첫 번째 GameplayTagRegistry 에셋 로드
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
            // 비어 있으면 경고 없음
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            // 형식 검사
            if (!GameplayTagUtility.IsValidTagString(value))
            {
                return "Invalid tag format. Use A.B.C with letters, digits, or underscore.";
            }

            // 레지스트리 존재 시 등록 여부 검사
            if (registry != null && !registry.IsTagDefined(value, includeParents: true))
            {
                return "Tag not found in GameplayTagRegistry.";
            }

            return string.Empty;
        }
    }
}
