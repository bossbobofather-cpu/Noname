using Noname.GameAbilitySystem.Presentation;
using UnityEditor;
using UnityEngine;

namespace Noname.GameAbilitySystem.Editor
{
    /// <summary>
    /// GameplayAbilityConfig 인스펙터를 사용자 정의로 표시합니다.
    /// </summary>
    [CustomEditor(typeof(GameplayAbilityConfig))]
    public sealed class GameplayAbilityDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty _abilityIdProp;
        private SerializedProperty _dpNameProp;
        private SerializedProperty _dpDescProp;
        private SerializedProperty _cooldownProp;
        private SerializedProperty _costEffectsProp;
        private SerializedProperty _appliedEffectsProp;
        private SerializedProperty _activationRequiredTagsProp;
        private SerializedProperty _activationBlockedTagsProp;

        private void OnEnable()
        {
            _abilityIdProp = serializedObject.FindProperty("_abilityId");
            _dpNameProp = serializedObject.FindProperty("_dpName");
            _dpDescProp = serializedObject.FindProperty("_dpDesc");
            _cooldownProp = serializedObject.FindProperty("_cooldown");
            _costEffectsProp = serializedObject.FindProperty("_costEffects");
            _appliedEffectsProp = serializedObject.FindProperty("_appliedEffects");
            _activationRequiredTagsProp = serializedObject.FindProperty("_activationRequiredTags");
            _activationBlockedTagsProp = serializedObject.FindProperty("_activationBlockedTags");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_abilityIdProp, new GUIContent("Ability ID"));
            EditorGUILayout.PropertyField(_dpNameProp, new GUIContent("Display Name"));
            EditorGUILayout.PropertyField(_dpDescProp, new GUIContent("Description"));
            EditorGUILayout.PropertyField(_cooldownProp, new GUIContent("Cooldown (초)"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("효과", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_costEffectsProp, new GUIContent("Cost Effects"), true);
            EditorGUILayout.PropertyField(_appliedEffectsProp, new GUIContent("Applied Effects"), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("태그 조건", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_activationRequiredTagsProp, new GUIContent("Required Tags"), true);
            EditorGUILayout.PropertyField(_activationBlockedTagsProp, new GUIContent("Blocked Tags"), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
