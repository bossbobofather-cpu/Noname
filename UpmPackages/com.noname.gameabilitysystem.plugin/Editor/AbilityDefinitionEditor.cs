using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Noname.GameAbilitySystem.Editor
{
    /// <summary>
    /// GameplayAbilityDefinition 인스펙터를 사용자 정의로 표시합니다.
    /// </summary>
    [CustomEditor(typeof(GameplayAbilityDefinition))]
    public sealed class GameplayAbilityDefinitionEditor : UnityEditor.Editor
    {
        private static GUIContent[] s_typeOptions;
        private static string[] s_typeNames;

        /// <summary>
        /// 어빌리티 타입 선택 UI와 구성 목록을 그립니다.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // 편집 대상의 프로퍼티 캐시 갱신
            serializedObject.Update();

            // 필요한 필드 참조
            var typeProp = serializedObject.FindProperty("_abilityTypeName");
            var configsProp = serializedObject.FindProperty("_configs");

            if (typeProp == null || configsProp == null)
            {
                // 필드가 없으면 기본 인스펙터로 폴백
                DrawDefaultInspector();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            // 타입 목록 캐시 준비
            EnsureTypeCache();

            var currentTypeName = typeProp.stringValue;
            var options = s_typeOptions;
            var typeNames = s_typeNames;
            var currentIndex = FindIndex(typeNames, currentTypeName);

            if (currentIndex < 0 && !string.IsNullOrEmpty(currentTypeName))
            {
                // 목록에 없는 타입은 Missing 항목으로 추가
                AppendMissingType(currentTypeName, ref options, ref typeNames, out currentIndex);
            }

            var newIndex = EditorGUILayout.Popup(new GUIContent("Ability Type"), currentIndex, options);
            if (newIndex != currentIndex && newIndex >= 0 && newIndex < typeNames.Length)
            {
                // 선택 결과를 프로퍼티에 반영
                typeProp.stringValue = typeNames[newIndex];
            }

            // Configs 목록 표시
            EditorGUILayout.PropertyField(configsProp, true);

            // 변경 사항 적용
            serializedObject.ApplyModifiedProperties();
        }

        private static void EnsureTypeCache()
        {
            // 이미 캐시되어 있으면 재생성하지 않음
            if (s_typeOptions != null && s_typeNames != null)
            {
                return;
            }

            // GameplayAbility 파생 타입을 수집
            var types = new List<Type>();
            foreach (var type in TypeCache.GetTypesDerivedFrom<GameplayAbility>())
            {
                if (type.IsAbstract || type.IsGenericTypeDefinition)
                {
                    continue;
                }

                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    continue;
                }

                types.Add(type);
            }

            types.Sort((a, b) => string.CompareOrdinal(a.FullName, b.FullName));

            // 드롭다운 항목용 배열 생성
            s_typeOptions = new GUIContent[types.Count + 1];
            s_typeNames = new string[types.Count + 1];

            s_typeOptions[0] = new GUIContent("<None>");
            s_typeNames[0] = string.Empty;

            // 풀네임 기준으로 표시
            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                var display = type.FullName ?? type.Name;
                s_typeOptions[i + 1] = new GUIContent(display);
                s_typeNames[i + 1] = type.AssemblyQualifiedName;
            }
        }

        private static int FindIndex(string[] typeNames, string typeName)
        {
            // 현재 타입명이 없으면 첫 항목(<None>)으로 처리
            if (typeNames == null || string.IsNullOrEmpty(typeName))
            {
                return 0;
            }

            for (var i = 0; i < typeNames.Length; i++)
            {
                if (string.Equals(typeNames[i], typeName, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static void AppendMissingType(string typeName, ref GUIContent[] options, ref string[] typeNames,
            out int index)
        {
            // 기존 배열을 늘려 Missing 항목 추가
            var newOptions = new GUIContent[options.Length + 1];
            var newTypeNames = new string[typeNames.Length + 1];
            Array.Copy(options, newOptions, options.Length);
            Array.Copy(typeNames, newTypeNames, typeNames.Length);

            var shortName = GetShortTypeName(typeName);
            newOptions[newOptions.Length - 1] = new GUIContent("Missing: " + shortName);
            newTypeNames[newTypeNames.Length - 1] = typeName;

            options = newOptions;
            typeNames = newTypeNames;
            index = newOptions.Length - 1;
        }

        private static string GetShortTypeName(string typeName)
        {
            // 어셈블리 정보 제외
            var comma = typeName.IndexOf(',');
            return comma >= 0 ? typeName.Substring(0, comma) : typeName;
        }
    }
}
