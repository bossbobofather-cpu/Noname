using UnityEditor;
using UnityEngine;

namespace Noname.GameAbilitySystem.Editor
{
    /// <summary>
    /// AttributeModifier를 인스펙터에서 보기 좋게 그리는 드로어입니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(AttributeModifier))]
    public sealed class AttributeModifierDrawer : PropertyDrawer
    {
        /// <summary>
        /// ValueMode에 따라 노출 필드를 조절합니다.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 기본 레이아웃 계산
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var rect = new Rect(position.x, position.y, position.width, lineHeight);

            // 하위 프로퍼티 참조
            var attributeProp = property.FindPropertyRelative("Attribute");
            var modeProp = property.FindPropertyRelative("ValueMode");
            var operationProp = property.FindPropertyRelative("Operation");
            var magnitudeProp = property.FindPropertyRelative("Magnitude");
            var calculatorProp = property.FindPropertyRelative("Calculator");

            // 공통 필드 표시
            EditorGUI.PropertyField(rect, attributeProp);
            rect.y += lineHeight + spacing;
            EditorGUI.PropertyField(rect, modeProp);
            rect.y += lineHeight + spacing;
            EditorGUI.PropertyField(rect, operationProp);

            // 모드에 따라 추가 필드 표시
            var mode = (AttributeModifierValueMode)modeProp.enumValueIndex;
            if (mode == AttributeModifierValueMode.Static || mode == AttributeModifierValueMode.StaticPlusCalculated)
            {
                rect.y += lineHeight + spacing;
                EditorGUI.PropertyField(rect, magnitudeProp);
            }

            if (mode == AttributeModifierValueMode.Calculated || mode == AttributeModifierValueMode.StaticPlusCalculated)
            {
                rect.y += lineHeight + spacing;
                EditorGUI.PropertyField(rect, calculatorProp);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 표시 줄 수를 모드에 맞춰 계산
            var modeProp = property.FindPropertyRelative("ValueMode");
            var mode = modeProp != null
                ? (AttributeModifierValueMode)modeProp.enumValueIndex
                : AttributeModifierValueMode.Static;

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var lines = 3;

            if (mode == AttributeModifierValueMode.StaticPlusCalculated)
            {
                lines += 2;
            }
            else if (mode == AttributeModifierValueMode.Calculated || mode == AttributeModifierValueMode.Static)
            {
                lines += 1;
            }

            return lines * lineHeight + (lines - 1) * spacing;
        }
    }
}
