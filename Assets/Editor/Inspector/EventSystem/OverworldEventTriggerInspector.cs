using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UCT.EventSystem;
using UCT.Overworld.FiniteStateMachine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.Inspector.EventSystem
{
    [CustomEditor(typeof(OverworldEventTrigger))]
    public class OverworldEventTriggerEditor : UnityEditor.Editor
    {
        private ReorderableList _eventList;
        private ReorderableList _tagList;
        private ReorderableList _triggerModeList;

        private void OnEnable()
        {
            InitializeReorderableList();
        }

        private void InitializeReorderableList()
        {
            var eventListProperty = serializedObject.FindProperty("eventNames");
            var triggerModeListProperty = serializedObject.FindProperty("eventTriggerModes");
            var tagListProperty = serializedObject.FindProperty("tags");

            _tagList = new ReorderableList(serializedObject, tagListProperty, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Tags"),

                drawElementCallback = (rect, index, _, _) =>
                {
                    var element = tagListProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;

                    var tags = InternalEditorUtility.tags;

                    var duplicates = false;
                    for (var i = 0; i < tagListProperty.arraySize; i++)
                    {
                        if (i == index || tagListProperty.GetArrayElementAtIndex(i).stringValue != element.stringValue)
                            continue;
                        duplicates = true;
                        break;
                    }

                    if (duplicates)
                    {
                        var existingTags = new HashSet<string>();
                        for (var i = 0; i < tagListProperty.arraySize; i++)
                            if (i != index)
                                existingTags.Add(tagListProperty.GetArrayElementAtIndex(i).stringValue);

                        foreach (var tag in tags)
                        {
                            if (existingTags.Contains(tag)) continue;
                            element.stringValue = tag;
                            break;
                        }
                    }

                    var currentIndex = Array.IndexOf(tags, element.stringValue);
                    currentIndex = EditorGUI.Popup(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        currentIndex,
                        tags
                    );
                    element.stringValue = currentIndex >= 0 && currentIndex < tags.Length ? tags[currentIndex] : "";
                },

                onCanAddCallback = _ =>
                {
                    var existingTags = new HashSet<string>();
                    for (var i = 0; i < tagListProperty.arraySize; i++)
                    {
                        var element = tagListProperty.GetArrayElementAtIndex(i);
                        existingTags.Add(element.stringValue);
                    }

                    return existingTags.Count < InternalEditorUtility.tags.Length;
                },

                onAddCallback = _ =>
                {
                    serializedObject.Update();

                    var existingTags = new HashSet<string>();
                    for (var i = 0; i < tagListProperty.arraySize; i++)
                    {
                        var element = tagListProperty.GetArrayElementAtIndex(i);
                        existingTags.Add(element.stringValue);
                    }

                    foreach (var tag in InternalEditorUtility.tags)
                    {
                        if (existingTags.Contains(tag)) continue;
                        tagListProperty.arraySize++;
                        var newElement = tagListProperty.GetArrayElementAtIndex(tagListProperty.arraySize - 1);
                        newElement.stringValue = tag;
                        break;
                    }

                    serializedObject.ApplyModifiedProperties();
                },

                onRemoveCallback = list =>
                {
                    serializedObject.Update();
                    tagListProperty.DeleteArrayElementAtIndex(list.index);
                    serializedObject.ApplyModifiedProperties();
                },

                elementHeightCallback = _ => EditorGUIUtility.singleLineHeight + 2
            };

            _eventList = new ReorderableList(serializedObject, eventListProperty, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, nameof(OverworldEventTrigger)),

                drawElementCallback = (rect, index, _, _) =>
                {
                    var element = eventListProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;

                    var duplicates = false;
                    for (var i = 0; i < eventListProperty.arraySize; i++)
                    {
                        if (i == index || eventListProperty.GetArrayElementAtIndex(i).stringValue !=
                            element.stringValue) continue;
                        duplicates = true;
                        break;
                    }

                    var allEvents = EntrySaver.GetAllEventEntry();
                    var allEventsName = allEvents.Select(item => item.name).ToArray();
                


                    if (duplicates)
                    {
                        var existingNames = new HashSet<string>();
                        for (var i = 0; i < eventListProperty.arraySize; i++)
                        {
                            if (i == index) continue;
                            var existingName = eventListProperty.GetArrayElementAtIndex(i).stringValue;
                            existingNames.Add(existingName);
                        }

                        foreach (var evt in allEvents)
                        {
                            if (existingNames.Contains(evt.name)) continue;
                            element.stringValue = evt.name;
                            break;
                        }
                    }
                    var eventRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

                    var currentIndex = Array.IndexOf(allEventsName, element.stringValue);
                    currentIndex = EditorGUI.Popup(eventRect, currentIndex, allEventsName);
                        element.stringValue = allEventsName[currentIndex];
                },

                onCanAddCallback = _ =>
                {
                    var existingNames = new HashSet<string>();
                    for (var i = 0; i < eventListProperty.arraySize; i++)
                    {
                        var eventName = eventListProperty.GetArrayElementAtIndex(i).stringValue;
                        existingNames.Add(eventName);
                    }

                    return existingNames.Count < EntrySaver.GetAllEventEntry().Length;
                },

                onAddCallback = _ =>
                {
                    serializedObject.Update();

                    var allEvents = EntrySaver.GetAllEventEntry();
                    var existingNames = new HashSet<string>();
                    for (var i = 0; i < eventListProperty.arraySize; i++)
                    {
                        var eventName = eventListProperty.GetArrayElementAtIndex(i).stringValue;
                        existingNames.Add(eventName);
                    }

                    foreach (var evt in allEvents)
                    {
                        if (existingNames.Contains(evt.name)) continue;

                        eventListProperty.arraySize++;
                        var newElement = eventListProperty.GetArrayElementAtIndex(eventListProperty.arraySize - 1);
                        newElement.stringValue = evt.name;
                        break;
                    }

                    serializedObject.ApplyModifiedProperties();
                },

                onRemoveCallback = list =>
                {
                    serializedObject.Update();
                    eventListProperty.DeleteArrayElementAtIndex(list.index);
                    serializedObject.ApplyModifiedProperties();
                },

                elementHeightCallback = _ => EditorGUIUtility.singleLineHeight + 2
            };

            _triggerModeList = new ReorderableList(serializedObject, triggerModeListProperty, true,
                true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Event Trigger Modes"),

                drawElementCallback = (rect, index, _, _) =>
                {
                    var element = triggerModeListProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;

                    var duplicates = false;
                    for (var i = 0; i < triggerModeListProperty.arraySize; i++)
                    {
                        if (i == index || triggerModeListProperty.GetArrayElementAtIndex(i).enumValueIndex !=
                            element.enumValueIndex) continue;
                        duplicates = true;
                        break;
                    }

                    if (duplicates)
                    {
                        var existingValues = new HashSet<int>();
                        for (var i = 0; i < triggerModeListProperty.arraySize; i++)
                            if (i != index)
                                existingValues.Add(triggerModeListProperty.GetArrayElementAtIndex(i).enumValueIndex);

                        foreach (EventTriggerMode mode in Enum.GetValues(typeof(EventTriggerMode)))
                        {
                            if (existingValues.Contains((int)mode)) continue;
                            element.enumValueIndex = (int)mode;
                            break;
                        }
                    }

                    var comment = GetEnumSummary((EventTriggerMode)element.enumValueIndex);

                    var labelRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(labelRect, comment, EditorStyles.wordWrappedLabel);
                    rect.y += EditorGUIUtility.singleLineHeight;

                    var triggerModeRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(triggerModeRect, element, GUIContent.none);
                },

                onCanAddCallback = _ =>
                {
                    var existingValues = new HashSet<EventTriggerMode>();
                    for (var i = 0; i < triggerModeListProperty.arraySize; i++)
                    {
                        var element = triggerModeListProperty.GetArrayElementAtIndex(i);
                        existingValues.Add((EventTriggerMode)element.enumValueIndex);
                    }

                    return existingValues.Count < Enum.GetValues(typeof(EventTriggerMode)).Length;
                },

                onAddCallback = _ =>
                {
                    serializedObject.Update();

                    var existingValues = new HashSet<EventTriggerMode>();
                    for (var i = 0; i < triggerModeListProperty.arraySize; i++)
                    {
                        var element = triggerModeListProperty.GetArrayElementAtIndex(i);
                        existingValues.Add((EventTriggerMode)element.enumValueIndex);
                    }

                    foreach (EventTriggerMode mode in Enum.GetValues(typeof(EventTriggerMode)))
                    {
                        if (existingValues.Contains(mode)) continue;
                        triggerModeListProperty.arraySize++;
                        var newElement =
                            triggerModeListProperty.GetArrayElementAtIndex(triggerModeListProperty.arraySize - 1);
                        newElement.enumValueIndex = (int)mode;
                        break;
                    }

                    serializedObject.ApplyModifiedProperties();
                },

                onRemoveCallback = list =>
                {
                    serializedObject.Update();
                    triggerModeListProperty.DeleteArrayElementAtIndex(list.index);
                    serializedObject.ApplyModifiedProperties();
                },
                elementHeightCallback = _ => EditorGUIUtility.singleLineHeight * 2
            };
        }

        /// <summary>
        ///     根据枚举值获取其注释
        /// </summary>
        private static string GetEnumSummary(EventTriggerMode value)
        {
            var enumComments = new Dictionary<EventTriggerMode, string>
            {
                { EventTriggerMode.Interact, "调查型：按下调查键后触发。这只能用于玩家。" },
                { EventTriggerMode.ColliderEnter, "碰撞器触发型：进入碰撞范围后触发。" },
                { EventTriggerMode.ColliderExit, "碰撞器离开型：离开碰撞范围后触发。" },
                { EventTriggerMode.TriggerEnter, "触发器触发型：进入触发器范围后触发。" },
                { EventTriggerMode.TriggerExit, "触发器离开型：离开触发器范围后触发。" },
                { EventTriggerMode.LineOfSightEnter, "视线触发型：进入视野（射线范围）时触发。" },
                { EventTriggerMode.LineOfSightExit, "视线离开型：离开视野（射线范围）时触发。" }
                // { EventTriggerMode.Timer, "定时型：到达指定时间后触发。" }
            };

            return enumComments.TryGetValue(value, out var comment) ? comment : string.Empty;
        }

        public override void OnInspectorGUI()
        {
            var centerBoldStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            var trigger = (OverworldEventTrigger)target;

            GUI.enabled = false;
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target),
                    typeof(MonoScript), false);
            }
            GUI.enabled = true;

            EditorGUILayout.PrefixLabel("Clarity");
            trigger.clarity = EditorGUILayout.Slider(trigger.clarity, 0, 1);

            GUILayout.Space(5);
            EditorGUILayout.LabelField("=== FSM Trigger ===", centerBoldStyle);
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("FSM Object Name");
                trigger.fsmObjectName = EditorGUILayout.TextField(trigger.fsmObjectName);
                if (string.IsNullOrEmpty(trigger.fsmObjectName))
                {
                    var lastRect = GUILayoutUtility.GetLastRect();
                    lastRect.x += 1;
                    lastRect.y += 0.5f;
                    GUI.Label(lastRect, "Player",
                        new GUIStyle(GUI.skin.label) { normal = { textColor = Color.gray } });
                }
            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = false;
            {
                trigger.fsmObject = (FiniteStateMachine)EditorGUILayout.ObjectField("FSM Object",
                    trigger.fsmObject, typeof(FiniteStateMachine), true);
            }
            GUI.enabled = true;

            GUILayout.Space(5);
            EditorGUILayout.LabelField("=== Trigger Conditions ===", centerBoldStyle);
            GUILayout.Space(5);

            serializedObject.Update();
            _tagList.DoLayoutList();
            _eventList.DoLayoutList();
            _triggerModeList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}