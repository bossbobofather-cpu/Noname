using UnityEditor;
using UnityEngine;

namespace Noname.GameAbilitySystem.Editor
{
    [CustomPropertyDrawer(typeof(AttributeModifier))]
    public sealed class AttributeModifierDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var rect = new Rect(position.x, position.y, position.width, lineHeight);

            var attributeProp = property.FindPropertyRelative("Attribute");
            var modeProp = property.FindPropertyRelative("ValueMode");
            var operationProp = property.FindPropertyRelative("Operation");
            var magnitudeProp = property.FindPropertyRelative("Magnitude");
            var calculatorProp = property.FindPropertyRelative("Calculator");

            EditorGUI.PropertyField(rect, attributeProp);
            rect.y += lineHeight + spacing;
            EditorGUI.PropertyField(rect, modeProp);
            rect.y += lineHeight + spacing;
            EditorGUI.PropertyField(rect, operationProp);

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
