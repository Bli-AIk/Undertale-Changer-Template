using System;
using System.Collections.Generic;
using System.Linq;
using UCT.EventSystem;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspector.EventSystem
{
    public static class EntrySaver
    {
        public static FactEntry[] GetFactEntry(bool isGlobal, string sceneName)
        {
            var path = "Tables/FactTable";
            if (!isGlobal && string.IsNullOrEmpty(sceneName)) isGlobal = true;
            if (!isGlobal)
                path = $"Tables/{sceneName}/FactTable";

            return Resources.Load<FactTable>(path).facts.ToArray();
        }

        public static EventEntry[] GetEventEntry(bool isGlobal, string sceneName)
        {
            var path = "Tables/EventTable";
            if (!isGlobal && string.IsNullOrEmpty(sceneName)) isGlobal = true;
            if (!isGlobal)
                path = $"Tables/{sceneName}/EventTable";

            return Resources.Load<EventTable>(path).events.ToArray();
        }

        public static RuleEntry[] GetRuleEntry(bool isGlobal, string sceneName)
        {
            var path = "Tables/RuleTable";
            if (string.IsNullOrEmpty(sceneName)) isGlobal = false;
            if (isGlobal)
                path = $"Tables/{sceneName}/RuleTable";

            return Resources.Load<RuleTable>(path).rules.ToArray();
        }

        public static bool EventEntryField(Rect rect, SerializedProperty property, bool isGlobal, string sceneName)
        {
            rect = RegionToggle(rect, isGlobal, out var changedIsGlobal);
            var entryIndex = 0;
            var allEventEntry = GetEventEntry(changedIsGlobal, sceneName);

            if (allEventEntry.Length <= 0)
            {
                GUI.Label(rect, "No events!",
                    new GUIStyle { normal = { textColor = Color.red } });
                return changedIsGlobal;
            }

            var allEventEntryName = new List<string>();
            for (var j = 0; j < allEventEntry.Length; j++)
            {
                allEventEntryName.Add(allEventEntry[j].name);
                if (allEventEntry[j].name == property.stringValue) entryIndex = j;
            }

            entryIndex = EditorGUI.Popup(rect, entryIndex, allEventEntryName.ToArray());
            property.stringValue = allEventEntryName[entryIndex];
            return changedIsGlobal;
        }

        public static bool FactEntryField(Rect rect, SerializedProperty fact, bool isGlobal, string sceneName,
            bool isDisplayValue = true,
            bool setValue = false)
        {
            rect = RegionToggle(rect, isGlobal, out var changedIsGlobal);

            var facts = GetFactEntry(changedIsGlobal, sceneName);

            if (facts.Length <= 0)
            {
                GUI.Label(rect, "No facts!",
                    new GUIStyle { normal = { textColor = Color.red } });
                return changedIsGlobal;
            }

            var oldFactName = fact.FindPropertyRelative("name").stringValue;

            var factNames = facts.Select(nFact => nFact.name).ToArray();

            var popup = Array.IndexOf(factNames, oldFactName);

            popup = popup >= 0 ? popup : 0;

            popup = EditorGUI.Popup(rect, popup, factNames);

            fact.FindPropertyRelative("name").stringValue = facts[popup].name;
            if (setValue) fact.FindPropertyRelative("value").intValue = facts[popup].value;
            fact.FindPropertyRelative("scope").enumValueIndex = (int)facts[popup].scope;
            fact.FindPropertyRelative("area").enumValueIndex = (int)facts[popup].area;
            fact.FindPropertyRelative("scene").stringValue = facts[popup].scene;

            if (!isDisplayValue) return changedIsGlobal;

            var factLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight
            };
            var factLabelRect = rect;
            factLabelRect.x -= 15;
            factLabelRect.y -= 1;
            GUI.Label(factLabelRect, $"({facts[popup].value})", factLabelStyle);
            return changedIsGlobal;
        }

        public static Rect RegionToggle(Rect rect, bool isGlobal, out bool changedIsGlobal)
        {
            var toggleRect = rect;
            toggleRect.width = rect.width / 5;
            var toggleStyle = new GUIStyle(GUI.skin.button);

            changedIsGlobal = GUI.Toggle(toggleRect, isGlobal, new GUIContent(), toggleStyle);
            const float shrinkAmount = 2.5f;
            var shrunkRect = new Rect(
                toggleRect.x + shrinkAmount,
                toggleRect.y + shrinkAmount,
                toggleRect.width - shrinkAmount * 2,
                toggleRect.height - shrinkAmount * 2
            );
            GUI.DrawTexture(shrunkRect,
                (Texture)EditorGUIUtility.Load($"Icons/EventSystem/{(changedIsGlobal ? "Public" : "Location")}.png"),
                ScaleMode.ScaleToFit);

            rect.x += toggleRect.width + 2.5f;
            rect.width -= toggleRect.width + 2.5f;

            return rect;
        }
    }
}