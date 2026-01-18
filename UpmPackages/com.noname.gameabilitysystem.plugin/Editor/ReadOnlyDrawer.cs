using UnityEditor;
using UnityEngine;

namespace Noname.GameAbilitySystem.Editor
{
    /// <summary>
    /// ReadOnlyAttribute가 붙은 필드를 읽기 전용으로 표시합니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public sealed class ReadOnlyDrawer : PropertyDrawer
    {
        /// <summary>
        /// GUI 상태를 잠시 비활성화해 읽기 전용으로 그립니다.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 기존 GUI 상태 보존
            var wasEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = wasEnabled;
        }

        /// <summary>
        /// 기본 프로퍼티 높이를 그대로 사용합니다.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 기본 높이를 그대로 사용
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
