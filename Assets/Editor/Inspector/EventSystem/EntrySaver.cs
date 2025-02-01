using System;
using System.Linq;
using UCT.EventSystem;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspector.EventSystem
{
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
        
        public static void EventEntryField(Rect rect, SerializedProperty property)
        {
            events ??= GetAllEventEntry();
            if (events.Length <= 0)
            {
                GUI.Label(rect, "No events!",
                    new GUIStyle { normal = { textColor = Color.red } });
                return;
            }

            var oldEventName = property.FindPropertyRelative("name").stringValue;

            var eventNames = events.Select(nEvent => nEvent.name).ToArray();

            var popup = Array.IndexOf(eventNames, oldEventName);

            popup = popup >= 0 ? popup : 0;

            popup = EditorGUI.Popup(rect, popup, eventNames);

            property.FindPropertyRelative("name").stringValue = events[popup].name;
            property.FindPropertyRelative("isTriggering").boolValue = events[popup].isTriggering;
        }

        public static void FactEntryField(Rect rect, SerializedProperty fact, bool isDisplayValue = true)
        {
            facts ??= GetAllFactEntry();
            if (facts.Length <= 0)
            {
                GUI.Label(rect, "No facts!",
                    new GUIStyle { normal = { textColor = Color.red } });
                return;
            }

            var oldFactName = fact.FindPropertyRelative("name").stringValue;

            var factNames = facts.Select(nFact => nFact.name).ToArray();

            var popup = Array.IndexOf(factNames, oldFactName);

            popup = popup >= 0 ? popup : 0;

            popup = EditorGUI.Popup(rect, popup, factNames);

            fact.FindPropertyRelative("name").stringValue = facts[popup].name;
            fact.FindPropertyRelative("scope").enumValueIndex = (int)facts[popup].scope;
            fact.FindPropertyRelative("area").enumValueIndex = (int)facts[popup].area;
            fact.FindPropertyRelative("scene").stringValue = facts[popup].scene;

            if (!isDisplayValue) return;

            var factLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight
            };
            var factLabelRect = rect;
            factLabelRect.x -= 15;
            factLabelRect.y -= 1;
            GUI.Label(factLabelRect, $"({fact.FindPropertyRelative("value").intValue})", factLabelStyle);
        }
    }
}