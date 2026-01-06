using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Noname.GameAbilitySystem.Editor
{
    [CustomEditor(typeof(GameplayAbilityDefinition))]
    public sealed class GameplayAbilityDefinitionEditor : UnityEditor.Editor
    {
        private static GUIContent[] s_typeOptions;
        private static string[] s_typeNames;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var typeProp = serializedObject.FindProperty("_abilityTypeName");
            var configsProp = serializedObject.FindProperty("_configs");

            if (typeProp == null || configsProp == null)
            {
                DrawDefaultInspector();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EnsureTypeCache();

            var currentTypeName = typeProp.stringValue;
            var options = s_typeOptions;
            var typeNames = s_typeNames;
            var currentIndex = FindIndex(typeNames, currentTypeName);

            if (currentIndex < 0 && !string.IsNullOrEmpty(currentTypeName))
            {
                AppendMissingType(currentTypeName, ref options, ref typeNames, out currentIndex);
            }

            var newIndex = EditorGUILayout.Popup(new GUIContent("Ability Type"), currentIndex, options);
            if (newIndex != currentIndex && newIndex >= 0 && newIndex < typeNames.Length)
            {
                typeProp.stringValue = typeNames[newIndex];
            }

            EditorGUILayout.PropertyField(configsProp, true);

            serializedObject.ApplyModifiedProperties();
        }

        private static void EnsureTypeCache()
        {
            if (s_typeOptions != null && s_typeNames != null)
            {
                return;
            }

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

            s_typeOptions = new GUIContent[types.Count + 1];
            s_typeNames = new string[types.Count + 1];

            s_typeOptions[0] = new GUIContent("<None>");
            s_typeNames[0] = string.Empty;

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
            var comma = typeName.IndexOf(',');
            return comma >= 0 ? typeName.Substring(0, comma) : typeName;
        }
    }
}
