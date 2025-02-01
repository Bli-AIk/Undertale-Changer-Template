using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class EditorService
    {
         public static void CopyProperty(SerializedProperty source, SerializedProperty target, string propertyName)
        {
            var sourceProperty = source.FindPropertyRelative(propertyName);
            var targetProperty = target.FindPropertyRelative(propertyName);

            switch (sourceProperty.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    targetProperty.boolValue = sourceProperty.boolValue;
                    break;
                case SerializedPropertyType.Integer:
                    targetProperty.intValue = sourceProperty.intValue;
                    break;
                case SerializedPropertyType.String:
                    targetProperty.stringValue = sourceProperty.stringValue;
                    break;
                case SerializedPropertyType.Enum:
                    targetProperty.enumValueIndex = sourceProperty.enumValueIndex;
                    break;
                case SerializedPropertyType.ObjectReference:
                    targetProperty.objectReferenceValue = sourceProperty.objectReferenceValue;
                    break;
                default:
                    UCT.Other.Debug.LogWarning($"Unsupported property type: {sourceProperty.propertyType}");
                    break;
            }
        }

        public static void ResetProperty<T>(SerializedProperty property, string propertyName, T defaultValue) 
        {
            var targetProperty = property.FindPropertyRelative(propertyName);

            switch (targetProperty.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    if (defaultValue is bool boolValue)
                        targetProperty.boolValue = boolValue;
                    break;
                case SerializedPropertyType.Integer:
                    if (defaultValue is int intValue)
                        targetProperty.intValue = intValue;
                    break;
                case SerializedPropertyType.String:
                    if (defaultValue is string stringValue)
                        targetProperty.stringValue = stringValue;
                    break;
                case SerializedPropertyType.Enum:
                    if (defaultValue is int enumValueIndex)
                        targetProperty.enumValueIndex = enumValueIndex;
                    break;
                case SerializedPropertyType.ObjectReference:
                    if (defaultValue == null)
                        targetProperty.objectReferenceValue = null;
                    break;
                default:
                    UCT.Other.Debug.LogWarning($"Unsupported property type: {targetProperty.propertyType}");
                    break;
            }
        }

        public static void DrawHorizontalLine(Rect rect, float width)
        {
            var lineRect = new Rect(rect.x, rect.y, width, 1);
            GUI.DrawTexture(lineRect, Texture2D.grayTexture);
        }

        public static void DrawVerticalLine(Rect rect, float height)
        {
            var lineRect = new Rect(rect.x, rect.y, 1, height);
            GUI.DrawTexture(lineRect, Texture2D.grayTexture);
        }

        /// <summary>
        ///     显示一个自定义 Popup 并修改 SerializedProperty 的值（使用字典映射）
        /// </summary>
        /// <param name="rect">Popup 显示的区域</param>
        /// <param name="property">要修改的 SerializedProperty</param>
        /// <param name="enumMap">枚举值和显示内容的字典</param>
        /// <param name="guiStyle"></param>
        public static void EnumPopup<TEnum>(Rect rect, SerializedProperty property,
            Dictionary<TEnum, string> enumMap, GUIStyle guiStyle) where TEnum : Enum
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.LabelField(rect, "Property must be an enum!");
                return;
            }

            var currentEnumValue = (TEnum)Enum.ToObject(typeof(TEnum), property.enumValueIndex);

            if (!enumMap.ContainsKey(currentEnumValue) && enumMap.Count > 0)
            {
                var firstKey = enumMap.Keys.First();
                property.enumValueIndex = Convert.ToInt32(firstKey);
                property.serializedObject.ApplyModifiedProperties();
                currentEnumValue = firstKey;
            }

            var keys = new List<TEnum>(enumMap.Keys);
            var displayOptions = new List<string>(enumMap.Values);
            var currentIndex = keys.IndexOf(currentEnumValue);
            var selectedIndex = EditorGUI.Popup(rect, currentIndex, displayOptions.ToArray(), guiStyle);

            if (selectedIndex == currentIndex || selectedIndex < 0 || selectedIndex >= keys.Count) return;
            property.enumValueIndex = Convert.ToInt32(keys[selectedIndex]);
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}