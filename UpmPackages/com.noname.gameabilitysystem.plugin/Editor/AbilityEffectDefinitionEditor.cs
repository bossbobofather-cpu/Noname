using UnityEditor;
using UnityEngine;

namespace Noname.GameAbilitySystem.Editor
{
    [CustomEditor(typeof(GameplayEffectConfig))]
    public sealed class GameplayEffectConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var durationTypeProp = serializedObject.FindProperty("_durationType");
            var durationProp = serializedObject.FindProperty("_duration");
            var periodProp = serializedObject.FindProperty("_period");
            var grantedTagsProp = serializedObject.FindProperty("_grantedTags");

            if (durationTypeProp == null || durationProp == null || periodProp == null || grantedTagsProp == null)
            {
                DrawDefaultInspector();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.PropertyField(durationTypeProp);

            var durationType = (EGameplayEffectDurationType)durationTypeProp.enumValueIndex;
            if (durationType != EGameplayEffectDurationType.Instant)
            {
                EditorGUILayout.PropertyField(durationProp);
                EditorGUILayout.PropertyField(periodProp);
            }

            EditorGUILayout.PropertyField(grantedTagsProp, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
