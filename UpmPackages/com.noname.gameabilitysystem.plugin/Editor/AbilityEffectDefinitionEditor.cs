using Noname.GameAbilitySystem.Domain;
using Noname.GameAbilitySystem.Presentation;
using UnityEditor;

namespace Noname.GameAbilitySystem.Editor
{
    /// <summary>
    /// GameplayEffectConfig 인스펙터를 사용자 정의로 표시합니다.
    /// </summary>
    [CustomEditor(typeof(GameplayEffectConfig))]
    public sealed class GameplayEffectConfigEditor : UnityEditor.Editor
    {
        /// <summary>
        /// 지속 타입에 따라 노출 필드를 조절하며 그립니다.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // 편집 대상의 프로퍼티 캐시 갱신
            serializedObject.Update();

            // 필요한 필드 참조
            var durationTypeProp = serializedObject.FindProperty("_durationType");
            var durationProp = serializedObject.FindProperty("_duration");
            var periodProp = serializedObject.FindProperty("_period");
            var grantedTagsProp = serializedObject.FindProperty("_grantedTags");
            var requiredTagsProp = serializedObject.FindProperty("_activationRequiredTags");
            var blockedTagsProp = serializedObject.FindProperty("_activationBlockedTags");
            var modifiersProp = serializedObject.FindProperty("_modifiers");

            if (durationTypeProp == null
                || durationProp == null
                || periodProp == null
                || grantedTagsProp == null
                || requiredTagsProp == null
                || blockedTagsProp == null
                || modifiersProp == null)
            {
                // 필드가 없으면 기본 인스펙터로 폴백
                DrawDefaultInspector();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            // 지속 타입 먼저 선택
            EditorGUILayout.PropertyField(durationTypeProp);

            var durationType = (EffectDurationType)durationTypeProp.enumValueIndex;
            if (durationType == EffectDurationType.HasDuration)
            {
                // 시간 기반 효과만 Duration 노출
                EditorGUILayout.PropertyField(durationProp);
                EditorGUILayout.PropertyField(periodProp);
            }
            else if (durationType == EffectDurationType.Infinite)
            {
                // 무한 지속은 Period만 노출
                EditorGUILayout.PropertyField(periodProp);
            }

            // 태그 및 수정자 표시
            EditorGUILayout.PropertyField(grantedTagsProp, true);
            EditorGUILayout.PropertyField(requiredTagsProp, true);
            EditorGUILayout.PropertyField(blockedTagsProp, true);
            EditorGUILayout.PropertyField(modifiersProp, true);

            // 변경 사항 적용
            serializedObject.ApplyModifiedProperties();
        }
    }
}
