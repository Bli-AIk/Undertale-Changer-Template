using System;
using System.Collections.Generic;
using System.Linq;
using UCT.EventSystem;
using UCT.Service;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Inspector.EventSystem
{
    public abstract class ReorderableListInspector<T> : UnityEditor.Editor where T : class, new()
    {
        private ReorderableList _reorderableList;

        protected abstract string PropertyName { get; }


        protected abstract string IconPath { get; }

        private void OnEnable()
        {
            InitializeReorderableList();
        }

        protected abstract void DrawElement(Rect rect, SerializedProperty element, int index);

        protected virtual void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, typeof(T).Name);
        }

        protected virtual void OnAddElement(SerializedProperty listProperty)
        {
            listProperty.arraySize++;
        }

        protected virtual void OnRemoveElement(SerializedProperty listProperty, int index)
        {
            listProperty.DeleteArrayElementAtIndex(index);
        }

        private void InitializeReorderableList()
        {
            var listProperty = serializedObject.FindProperty(PropertyName);

            _reorderableList = new ReorderableList(serializedObject, listProperty, true, 
                true, true, true)
            {
                drawHeaderCallback = DrawHeader,

                drawElementCallback = (rect, index, _, _) =>
                {
                    var element = listProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    DrawElement(rect, element, index);
                },

                onAddCallback = _ =>
                {
                    serializedObject.Update();
                    OnAddElement(listProperty);
                    serializedObject.ApplyModifiedProperties();
                },

                onRemoveCallback = list =>
                {
                    serializedObject.Update();
                    OnRemoveElement(listProperty, list.index);
                    serializedObject.ApplyModifiedProperties();
                }
            };
        }

        public override void OnInspectorGUI()
        {
            var listProperty = serializedObject.FindProperty(PropertyName);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(TextProcessingService.ToFirstLetterUpperCase(PropertyName), EditorStyles.boldLabel);

            var newCount = EditorGUILayout.DelayedIntField(listProperty.arraySize, GUILayout.Width(48));
            if (newCount != listProperty.arraySize)
            {
                serializedObject.Update();
                listProperty.arraySize = newCount;
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.Update();
            _reorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }


        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (string.IsNullOrEmpty(IconPath))
            {
                UCT.Other.Debug.Log($"{IconPath} is Empty!");
                return base.RenderStaticPreview(assetPath, subAssets, width, height);
            }

            var icon = EditorGUIUtility.Load(IconPath) as Texture2D;

            if (icon == null)
                throw new NullReferenceException();

            var previewIcon = new Texture2D(width, height);
            EditorUtility.CopySerialized(icon, previewIcon);
            return previewIcon;
        }
    }

    [CustomEditor(typeof(FactTable))]
    public class FactTableInspector : ReorderableListInspector<FactTable>
    {
        private readonly List<string> _kaomojis = new()
        {
            "( -_- )",
            "( I⃒‿I⃒ )",
            "( o_o )",
            "( ^_^ )",
            "( >_< )",
            "( T_T )",
            "( ^o^ )",
            "( ; _ ; )",
            "( >o< )",
            "( o_o')",
            "( T‿T )",
            "( •_• )",
            "( 'o' )"
        };

        protected override string PropertyName => "facts";
        protected override string IconPath => "Icons/EventSystem/Inventory.png";

        protected override void DrawElement(Rect rect, SerializedProperty element, int index)
        {
            var fieldWidth = rect.width / 4 - 10;

            var nameRect = new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            var scopeRect = new Rect(rect.x + fieldWidth + 5, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            var additionalRect = new Rect(rect.x + 2 * fieldWidth + 10, rect.y, fieldWidth,
                EditorGUIUtility.singleLineHeight);
            var valueRect = new Rect(rect.x + 3 * (fieldWidth + 5), rect.y, fieldWidth,
                EditorGUIUtility.singleLineHeight);

            var nameProperty = element.FindPropertyRelative("name");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
            if (string.IsNullOrEmpty(nameProperty.stringValue))
                nameProperty.stringValue = "Fact" + (index + 1);
            if (EditorGUI.EndChangeCheck())
                RenameDetection(element);
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(scopeRect, element.FindPropertyRelative("scope"), GUIContent.none);
            var scopeProperty = element.FindPropertyRelative("scope");
            var scopeIndex = scopeProperty.enumValueIndex;
            var scopeValue = (Scope)scopeIndex;
            switch (scopeValue)
            {
                case Scope.Global or Scope.Temp:
                    var style = new GUIStyle(GUI.skin.label);
                    if (scopeValue is Scope.Global)
                        style = new GUIStyle(GUI.skin.label)
                        {
                            fontStyle = FontStyle.Bold
                        };

                    var kaomojiIndex = (int)(rect.y / EditorGUIUtility.singleLineHeight) % _kaomojis.Count;
                    GUI.Label(additionalRect, _kaomojis[kaomojiIndex], style);
                    break;
                case Scope.Area:
                    EditorGUI.PropertyField(additionalRect, element.FindPropertyRelative("area"), GUIContent.none);
                    break;
                case Scope.Scene:
                    var scene = element.FindPropertyRelative("scene");
                    EditorGUI.BeginChangeCheck();
                    var stringValue = scene.stringValue;
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(stringValue);
                    sceneAsset =
                        (SceneAsset)EditorGUI.ObjectField(additionalRect, sceneAsset, typeof(SceneAsset), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (sceneAsset != null)
                        {
                            var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                            scene.stringValue = scenePath;
                        }
                        else
                        {
                            scene.stringValue = string.Empty;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (EditorGUI.EndChangeCheck())
                RenameDetection(element);
            
            EditorGUI.PropertyField(valueRect, element.FindPropertyRelative("value"), GUIContent.none);
        }

        protected override void DrawHeader(Rect rect)
        {
            var rectX = rect.x + 15;
            var fieldWidth = rect.width / 4 - 10;

            EditorGUI.LabelField(new Rect(rectX, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), "Name");
            EditorGUI.LabelField(new Rect(rectX + fieldWidth, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                "Scope");
            EditorGUI.LabelField(
                new Rect(rectX + 2 * fieldWidth, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                "Additional");
            EditorGUI.LabelField(
                new Rect(rectX + 3 * fieldWidth, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), "Value");
        }
        
        private void RenameDetection(SerializedProperty element)
        {
            var nameProperty = element.FindPropertyRelative("name");
            var nameP = nameProperty.stringValue;
            var currentScope = (Scope)element.FindPropertyRelative("scope").enumValueIndex;

            EntrySaver.facts = EntrySaver.GetAllFactEntry();
            while (EntrySaver.facts.Any(bFact => bFact.name == nameP && bFact.scope == currentScope))
                nameP += "_b";

            nameProperty.stringValue = nameP;
            SetAllFactEntry();
        }

        protected override void OnAddElement(SerializedProperty listProperty)
        {
            listProperty.arraySize++;
            var element = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
            element.FindPropertyRelative("name").stringValue = "Fact" + listProperty.arraySize;
            element.FindPropertyRelative("scope").enumValueIndex = 0; 
            RenameDetection(element);
        }
        
        protected override void OnRemoveElement(SerializedProperty listProperty, int index)
        {
            listProperty.DeleteArrayElementAtIndex(index);
            SetAllFactEntry();
        }

        private void SetAllFactEntry()
        {
            serializedObject.ApplyModifiedProperties();
            EntrySaver.facts = EntrySaver.GetAllFactEntry();
        }
        
    }


    [CustomEditor(typeof(EventTable))]
    public class EventTableInspector : ReorderableListInspector<EventTable>
    {
        protected override string PropertyName => "events";
        protected override string IconPath => "Icons/EventSystem/Bolt.png";

        protected override void DrawElement(Rect rect, SerializedProperty element, int index)
        {
            var fieldWidth = rect.width / 2 - 10;
            var nameRect = new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            var isTriggeringRect =
                new Rect(rect.x + fieldWidth + 5, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            
            var nameProperty = element.FindPropertyRelative("name");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
            if (string.IsNullOrEmpty(nameProperty.stringValue))
                nameProperty.stringValue = "Event" + (index + 1);
            if (EditorGUI.EndChangeCheck())
            {
                RenameDetection(nameProperty);
            }
            
            GUI.enabled = false;
            EditorGUI.PropertyField(isTriggeringRect, element.FindPropertyRelative("isTriggering"), GUIContent.none);
            GUI.enabled = true;
        }

        private void RenameDetection(SerializedProperty nameProperty)
        {
            var nameP = nameProperty.stringValue;
            EntrySaver.events = EntrySaver.GetAllEventEntry();
            while (EntrySaver.events.Any(bEvent => bEvent.name == nameP))
                nameP += "_b";
            nameProperty.stringValue = nameP;
            SetAllEventEntry();
        }

        protected override void DrawHeader(Rect rect)
        {
            var rectX = rect.x + 15;
            var fieldWidth = rect.width / 2 - 10;

            EditorGUI.LabelField(new Rect(rectX, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), "Name");
            EditorGUI.LabelField(new Rect(rectX + fieldWidth, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                "isTriggering");
        }
        
        
        protected override void OnAddElement(SerializedProperty listProperty)
        {
            listProperty.arraySize++;
            var element = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
            var nameProperty = element.FindPropertyRelative("name");
            nameProperty.stringValue = "Event" + listProperty.arraySize;
            RenameDetection(nameProperty);
        }
        
        protected override void OnRemoveElement(SerializedProperty listProperty, int index)
        {
            listProperty.DeleteArrayElementAtIndex(index);
            SetAllEventEntry();
        }

        private void SetAllEventEntry()
        {
            serializedObject.ApplyModifiedProperties();
            EntrySaver.events = EntrySaver.GetAllEventEntry();
        }
    }

    [CustomEditor(typeof(RuleTable))]
    public class RuleTableInspector : ReorderableListInspector<RuleTable>
    {
        protected override string PropertyName => "rules";
        protected override string IconPath => "Icons/EventSystem/Rule.png";

        protected override void DrawElement(Rect rect, SerializedProperty element, int index)
        {
            EntrySaver.events ??= EntrySaver.GetAllEventEntry();
            
            rect.height *= 2;
            
            var fieldWidth = rect.width / 3 - 10;
            var nameRect = new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            var triggeredByRect =
                new Rect(rect.x + fieldWidth + 5, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            var triggersRect =
                new Rect(rect.x + 2 * (fieldWidth + 5), rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);

            var nameProperty = element.FindPropertyRelative("name");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
            if (string.IsNullOrEmpty(nameProperty.stringValue))
                nameProperty.stringValue = "Rule" + (index + 1);
            if (EditorGUI.EndChangeCheck())
                RenameDetection(nameProperty);

            EventEntryField(element, triggeredByRect, "triggeredBy");
            EventEntryField(element, triggersRect, "triggers");
        }

        private void RenameDetection(SerializedProperty nameProperty)
        {
            var nameP = nameProperty.stringValue;
            EntrySaver.rules = EntrySaver.GetAllRuleEntry();
            while (EntrySaver.rules.Any(bRule => bRule.name == nameP))
                nameP += "_b";
            nameProperty.stringValue = nameP;
            SetAllRuleEntry();
        }
        
        private static void EventEntryField(SerializedProperty element, Rect triggeredByRect, string relativePropertyPath)
        {
            if (EntrySaver.events.Length <= 0)
            {
                GUI.Label(triggeredByRect, "There are no events...",
                    new GUIStyle { normal = { textColor = Color.gray } });
                return;
            }

            var triggeredByProperty = element.FindPropertyRelative(relativePropertyPath);
            var oldEventName = triggeredByProperty.FindPropertyRelative("name").stringValue;

            var eventNames = EntrySaver.events.Select(nEvent => nEvent.name).ToArray();

            var popup = Array.IndexOf(eventNames, oldEventName);

            popup = popup >= 0 ? popup : 0;

            popup = EditorGUI.Popup(triggeredByRect, popup, eventNames);

            triggeredByProperty.FindPropertyRelative("name").stringValue = EntrySaver.events[popup].name;
        }
        
        protected override void DrawHeader(Rect rect)
        {
            var rectX = rect.x + 15;
            var fieldWidth = rect.width / 2 - 10;

            EditorGUI.LabelField(new Rect(rectX, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), "Name");
            EditorGUI.LabelField(new Rect(rectX + fieldWidth, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                "TriggeredBy");
        }

        protected override void OnAddElement(SerializedProperty listProperty)
        {
            listProperty.arraySize++;
            var element = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
            var nameProperty = element.FindPropertyRelative("name");
            nameProperty.stringValue = "Rule" + listProperty.arraySize;
            RenameDetection(nameProperty);
        }
        
        protected override void OnRemoveElement(SerializedProperty listProperty, int index)
        {
            listProperty.DeleteArrayElementAtIndex(index);
            SetAllRuleEntry();
        }

        private void SetAllRuleEntry()
        {
            serializedObject.ApplyModifiedProperties();
            EntrySaver.rules = EntrySaver.GetAllRuleEntry();
        }
    }

    public static class EntrySaver
    {
        public static FactEntry[] facts;
        public static EventEntry[] events;
        public static RuleEntry[] rules;

        public static FactEntry[] GetAllFactEntry()
        {
            var guids = AssetDatabase.FindAssets("t:FactTable");
            return guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<FactTable>)
                .Where(eventTable => eventTable && eventTable.facts != null)
                .SelectMany(eventTable => eventTable.facts).ToArray();
        }

        public static EventEntry[] GetAllEventEntry()
        {
            var guids = AssetDatabase.FindAssets("t:EventTable");
            return guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<EventTable>)
                .Where(eventTable => eventTable && eventTable.events != null)
                .SelectMany(eventTable => eventTable.events).ToArray();
        }
        public static RuleEntry[] GetAllRuleEntry()
        {
            var guids = AssetDatabase.FindAssets("t:RuleTable");
            return guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<RuleTable>)
                .Where(ruleTable => ruleTable && ruleTable.rules != null)
                .SelectMany(ruleTable => ruleTable.rules).ToArray();
        }
    }
}