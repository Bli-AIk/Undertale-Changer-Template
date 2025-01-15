using UnityEditor;

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
    }
}