using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using UCT.EventSystem;
using UCT.Overworld.FiniteStateMachine;
using UCT.Service;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Inspector.EventSystem
{
    [CustomEditor(typeof(OverworldEventTrigger))]
    public class OverworldEventTriggerEditor : UnityEditor.Editor
    {
        public List<float> elementHeight;
        private ReorderableList _eventList;
        private ReorderableList _simpleRulesList;
        private ReorderableList _tagList;
        private ReorderableList _triggerModeList;

        private void OnEnable()
        {
            InitializeReorderableList();
        }

        private static string GetSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        private void RenameDetection(SerializedProperty element)
        {
            var nameProperty = element.FindPropertyRelative("name");
            var nameP = nameProperty.stringValue;
            while (EntrySaver.GetRuleEntry(true, GetSceneName()).Any(bRule => bRule.name == nameP))
            {
                nameP += "_b";
            }

            nameProperty.stringValue = nameP;
            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeReorderableList()
        {
            var tagListProperty = serializedObject.FindProperty("tags");
            var eventListProperty = serializedObject.FindProperty("eventNames");
            var triggerModeListProperty = serializedObject.FindProperty("eventTriggerModes");
            var simpleRulesListProperty = serializedObject.FindProperty("simpleRules");

            _simpleRulesList = new ReorderableList(serializedObject, simpleRulesListProperty, true,
                true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    var rectX = rect.x + 15;
                    var fieldWidth = rect.width - 10;

                    EditorGUI.LabelField(new Rect(rectX, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                        "Rules");
                },

                drawElementCallback = (rect, index, _, _) =>
                {
                    var element = simpleRulesListProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    var originalCriteria =
                        element.FindPropertyRelative("ruleCriterion").FindPropertyRelative("criteria");
                    var isGlobalFactModifications = element.FindPropertyRelative("isGlobalFactModifications");
                    var factModifications = element.FindPropertyRelative("factModifications");
                    var isGlobalTriggeredBy = element.FindPropertyRelative("isGlobalTriggeredBy");
                    var triggeredBy = element.FindPropertyRelative("triggeredBy");
                    var isGlobalTriggers = element.FindPropertyRelative("isGlobalTriggers");
                    var triggers = element.FindPropertyRelative("triggers");
                    var methodNames = element.FindPropertyRelative("methodNames");
                    var firstStringParams = element.FindPropertyRelative("firstStringParams");
                    var secondStringParams = element.FindPropertyRelative("secondStringParams");
                    var thirdStringParams = element.FindPropertyRelative("thirdStringParams");
                    var useMethodEvents = element.FindPropertyRelative("useMethodEvents");
                    var isGlobalMethodEvents = element.FindPropertyRelative("isGlobalMethodEvents");
                    var methodEvents = element.FindPropertyRelative("methodEvents");

                    if (originalCriteria.arraySize == 0)
                    {
                        originalCriteria.arraySize = 1;
                    }

                    var calculateCriteriaSize = RuleTableInspector.CalculateCriteriaSize(originalCriteria);


                    int triggerSize;
                    if (triggeredBy.arraySize == 0 && triggers.arraySize == 0)
                    {
                        triggerSize = 1;
                    }
                    else
                    {
                        triggerSize = Math.Max(triggeredBy.arraySize, triggers.arraySize);
                    }

                    triggerSize++;

                    var factSize = factModifications.arraySize + 1;
                    if (factModifications.arraySize == 0)
                    {
                        factSize++;
                    }

                    var methodSize = methodNames.arraySize + 1;
                    if (methodNames.arraySize == 0)
                    {
                        methodSize++;
                    }

                    methodSize *= 2;
                    methodSize--;

                    var lineCount = 2 + triggerSize + calculateCriteriaSize + factSize + methodSize;


                    var rectHeight = rect.height / lineCount;
                    if (index < elementHeight.Count)
                    {
                        elementHeight.Add(0);
                    }

                    elementHeight[index] = (EditorGUIUtility.singleLineHeight + 2.5f) * lineCount;

                    var fieldWidth = rect.width / 4 - 10;

                    //  line 1

                    var nameLabelRect = new Rect(rect.x, rect.y, fieldWidth * 2, EditorGUIUtility.singleLineHeight);
                    var nameFieldRect =
                        new Rect(rect.x + fieldWidth * 2 + 10, rect.y, fieldWidth * 2,
                            EditorGUIUtility.singleLineHeight);

                    GUI.Label(nameLabelRect, "Name");
                    var nameProperty = element.FindPropertyRelative("name");
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(nameFieldRect, nameProperty, GUIContent.none);
                    if (string.IsNullOrEmpty(nameProperty.stringValue))
                    {
                        nameProperty.stringValue = "Rule" + (index + 1);
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        RenameDetection(nameProperty);
                    }

                    //  line 2
                    var triggeredByLabelRect =
                        new Rect(rect.x, rect.y + rectHeight, fieldWidth, EditorGUIUtility.singleLineHeight);

                    //  line 2 left
                    EditorService.DrawHorizontalLine(triggeredByLabelRect, rect.width);
                    var lineTwoRect = triggeredByLabelRect;
                    triggeredByLabelRect.y += 2.5f;

                    GUI.Label(triggeredByLabelRect, "Triggered By");
                    triggeredByLabelRect.width = fieldWidth / 4;
                    triggeredByLabelRect.x += fieldWidth * 1.5f;
                    triggeredByLabelRect.height -= 0.25f;

                    var triggeredByExistingNames = new HashSet<string>();

                    for (var i = 0; i < triggeredBy.arraySize; i++)
                    {
                        var item = triggeredBy.GetArrayElementAtIndex(i);
                        if (item != null)
                        {
                            triggeredByExistingNames.Add(item.stringValue);
                        }
                    }

                    GUI.enabled = !EntrySaver.GetEventEntry(true, GetSceneName()).Select(entry => entry.name)
                        .All(nameP => triggeredByExistingNames.Contains(nameP));
                    if (GUI.Button(triggeredByLabelRect, EditorGUIUtility.IconContent("d_Toolbar Plus")))
                    {
                        triggeredBy.arraySize++;
                    }

                    GUI.enabled = true;

                    triggeredByLabelRect.x += fieldWidth / 4 + 2.5f;

                    GUI.enabled = triggeredBy.arraySize > 0;
                    if (GUI.Button(triggeredByLabelRect, EditorGUIUtility.IconContent("d_Toolbar Minus")))
                    {
                        triggeredBy.arraySize--;
                    }

                    GUI.enabled = true;

                    triggeredByLabelRect.x += fieldWidth / 4 + 2.5f;
                    triggeredByLabelRect.y += rectHeight - 1f;
                    triggeredByLabelRect.height += 0.25f;

                    if (triggeredBy.arraySize == 0)
                    {
                        triggeredByLabelRect.x = rect.x;
                        GUI.Label(triggeredByLabelRect, "Triggered by nothing.",
                            new GUIStyle { normal = { textColor = Color.grey } });
                    }
                    else
                    {
                        for (var i = 0; i < triggeredBy.arraySize; i++)
                        {
                            triggeredByLabelRect.x = rect.x;
                            triggeredByLabelRect.width = fieldWidth * 2 + 2.5f;

                            if (i >= triggeredBy.arraySize)
                            {
                                triggeredBy.arraySize = i + 1;
                            }

                            var item = triggeredBy.GetArrayElementAtIndex(i);

                            isGlobalTriggeredBy.boolValue =
                                EntrySaver.EventEntryField(triggeredByLabelRect, item, isGlobalTriggeredBy.boolValue,
                                    GetSceneName());

                            triggeredByLabelRect.y += rectHeight;
                        }
                    }

                    //  line 2 right
                    var triggersRect = lineTwoRect;
                    triggersRect.x += fieldWidth * 2 + 10f;
                    EditorService.DrawVerticalLine(triggersRect, rectHeight * triggerSize + 1);
                    triggersRect.x += 2.5f;
                    lineTwoRect = triggersRect;
                    triggersRect.y += 2.5f;

                    GUI.Label(triggersRect, "Triggers");
                    triggersRect.width = fieldWidth / 4;
                    triggersRect.x = rect.width - (fieldWidth / 8 + 2.5f);
                    triggersRect.height -= 0.25f;

                    var triggersExistingNames = new HashSet<string>();

                    for (var i = 0; i < triggers.arraySize; i++)
                    {
                        var item = triggers.GetArrayElementAtIndex(i);
                        if (item != null)
                        {
                            triggersExistingNames.Add(item.stringValue);
                        }
                    }

                    GUI.enabled = !EntrySaver.GetEventEntry(true, GetSceneName()).Select(entry => entry.name)
                        .All(nameP => triggersExistingNames.Contains(nameP));
                    if (GUI.Button(triggersRect, EditorGUIUtility.IconContent("d_Toolbar Plus")))
                    {
                        triggers.arraySize++;
                    }

                    GUI.enabled = true;

                    triggersRect.x += fieldWidth / 4 + 2.5f;

                    GUI.enabled = triggers.arraySize > 0;
                    if (GUI.Button(triggersRect, EditorGUIUtility.IconContent("d_Toolbar Minus")))
                    {
                        triggers.arraySize--;
                    }

                    GUI.enabled = true;

                    triggersRect.x += fieldWidth / 4 + 2.5f;
                    triggersRect.y += rectHeight - 1f;

                    if (triggers.arraySize == 0)
                    {
                        triggersRect.x = lineTwoRect.x;
                        GUI.Label(triggersRect, "No Triggers.",
                            new GUIStyle { normal = { textColor = Color.grey } });
                    }
                    else
                    {
                        for (var i = 0; i < triggers.arraySize; i++)
                        {
                            triggersRect.x = lineTwoRect.x;
                            triggersRect.width = fieldWidth * 2 + 26f;
                            if (i >= triggers.arraySize)
                            {
                                triggers.arraySize = i + 1;
                            }

                            var item = triggers.GetArrayElementAtIndex(i);

                            isGlobalTriggers.boolValue =
                                EntrySaver.EventEntryField(triggersRect, item, isGlobalTriggers.boolValue,
                                    GetSceneName());
                        }
                    }

                    RuleTableInspector.EnsureUniqueTriggeredBy(triggeredBy, GetSceneName());
                    RuleTableInspector.EnsureUniqueTriggeredBy(triggers, GetSceneName());
                    //  line 3
                    var criteriaLabelRect = triggeredByLabelRect;

                    var originalRuleCriterion = element.FindPropertyRelative("ruleCriterion");
                    var useRuleCriterion = element.FindPropertyRelative("useRuleCriterion");

                    criteriaLabelRect.y = rect.y + rectHeight * (triggerSize + 1);
                    EditorService.DrawHorizontalLine(criteriaLabelRect, rect.width);

                    criteriaLabelRect.y += 1;
                    criteriaLabelRect.width = 15;
                    useRuleCriterion.boolValue =
                        GUI.Toggle(criteriaLabelRect, useRuleCriterion.boolValue, new GUIContent());
                    criteriaLabelRect.width = rect.width;
                    criteriaLabelRect.y -= 1;

                    var color = GUI.color;

                    GUI.enabled = useRuleCriterion.boolValue;
                    criteriaLabelRect.x += 15;

                    bool result;
                    if (!useRuleCriterion.boolValue)
                    {
                        result = true;
                    }
                    else
                    {
                        result = RuleTableInspector.GetRuleCriterionResult(originalRuleCriterion,
                            out _, out _, out _, out _, out _, out _);
                    }

                    GUI.Label(criteriaLabelRect,
                        $"Rule Criterion: <color={(result ? "green" : "red")}>{(result ? "True" : "False")}</color>",
                        new GUIStyle(GUI.skin.label) { richText = true });

                    //  line 4
                    var ruleCriteriaRectY = rect.y + (lineCount - 2) * rectHeight;
                    var originalCriteriaParentOperation = originalRuleCriterion.FindPropertyRelative("operation");
                    var factModificationsRect = RuleTableInspector.CreateRuleCriteria(rect,
                        rect.y + rectHeight * (triggerSize + 2),
                        originalRuleCriterion,
                        originalCriteriaParentOperation,
                        fieldWidth, rectHeight, 0, 0, ruleCriteriaRectY,
                        GetSceneName());
                    GUI.enabled = true;
                    GUI.color = color;

                    //  line 5
                    EditorService.DrawHorizontalLine(factModificationsRect, rect.width);
                    factModificationsRect.y += 2.5f;

                    GUI.Label(factModificationsRect, "Modifications");
                    factModificationsRect.width = fieldWidth / 4;
                    factModificationsRect.x = rect.width - (fieldWidth / 8 + 2.5f);
                    factModificationsRect.height -= 0.25f;

                    if (GUI.Button(factModificationsRect, EditorGUIUtility.IconContent("d_Toolbar Plus")))
                    {
                        factModifications.arraySize++;
                    }

                    factModificationsRect.x += fieldWidth / 4 + 2.5f;

                    GUI.enabled = factModifications.arraySize > 0;
                    if (GUI.Button(factModificationsRect, EditorGUIUtility.IconContent("d_Toolbar Minus")))
                    {
                        factModifications.arraySize--;
                    }

                    GUI.enabled = true;

                    factModificationsRect.x += fieldWidth / 4 + 2.5f;
                    factModificationsRect.y += rectHeight - 1f;
                    factModificationsRect.height += 0.25f;

                    if (factModifications.arraySize == 0)
                    {
                        factModificationsRect.x = rect.x;
                        GUI.Label(factModificationsRect, "No Modifications.",
                            new GUIStyle { normal = { textColor = Color.grey } });
                        factModificationsRect.y += rectHeight;
                    }
                    else
                    {
                        for (var i = 0; i < factModifications.arraySize; i++)
                        {
                            var width = rect.width / 3 - 7.5f;
                            factModificationsRect.x = rect.x;
                            factModificationsRect.width = width * 1.25f;

                            if (i >= factModifications.arraySize)
                            {
                                factModifications.arraySize = i + 1;
                            }

                            var item = factModifications.GetArrayElementAtIndex(i);
                            var fact = item.FindPropertyRelative("fact");

                            if (i >= isGlobalFactModifications.arraySize)
                            {
                                isGlobalFactModifications.arraySize = i + 1;
                            }

                            var isGlobalFactModification = isGlobalFactModifications.GetArrayElementAtIndex(i);

                            isGlobalFactModification.boolValue = EntrySaver.FactEntryField(factModificationsRect, fact,
                                isGlobalFactModification.boolValue, GetSceneName(), true, true);
                            factModificationsRect.x += factModificationsRect.width + 5f;

                            var operation = item.FindPropertyRelative("operation");
                            var operationDictionary = new Dictionary<FactModification.Operation, string>
                            {
                                { FactModification.Operation.Change, "=" },
                                { FactModification.Operation.Add, "+" },
                                { FactModification.Operation.Subtract, "-" },
                                { FactModification.Operation.Multiply, "×" },
                                { FactModification.Operation.Divide, "÷" }
                            };
                            factModificationsRect.width = width * 0.5f;
                            EditorService.EnumPopup(factModificationsRect, operation, operationDictionary,
                                new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter });
                            factModificationsRect.x += factModificationsRect.width + 5f;

                            var number = item.FindPropertyRelative("number");
                            factModificationsRect.width = width * 1.25f;
                            EditorGUI.PropertyField(factModificationsRect, number, new GUIContent());
                            factModificationsRect.y += rectHeight;
                        }
                    }

                    factModificationsRect.x = rect.x;
                    EditorService.DrawHorizontalLine(factModificationsRect, rect.width);
                    //  line 6
                    var methodNameRect = factModificationsRect;
                    methodNameRect.width = fieldWidth * 2;
                    GUI.Label(methodNameRect, "Trigger methods and events");
                    methodNameRect.width = fieldWidth / 4;
                    methodNameRect.x = rect.width - (fieldWidth / 8 + 2.5f);
                    methodNameRect.height -= 0.25f;
                    methodNameRect.y += 2;
                    if (GUI.Button(methodNameRect, EditorGUIUtility.IconContent("d_Toolbar Plus")))
                    {
                        methodNames.arraySize++;
                        firstStringParams.arraySize++;
                    }

                    methodNameRect.x += fieldWidth / 4 + 2.5f;

                    GUI.enabled = methodNames.arraySize > 0;
                    if (GUI.Button(methodNameRect, EditorGUIUtility.IconContent("d_Toolbar Minus")))
                    {
                        methodNames.arraySize--;
                        firstStringParams.arraySize--;
                    }

                    GUI.enabled = true;


                    methodNameRect.y -= 2;

                    methodNameRect.x += fieldWidth / 4 + 2.5f;
                    factModificationsRect.y += rectHeight;

                    if (methodNames.arraySize == 0)
                    {
                        methodNameRect.y += rectHeight;
                        methodNameRect.x = rect.x;
                        GUI.Label(methodNameRect, "No Method Names.",
                            new GUIStyle { normal = { textColor = Color.grey } });
                    }
                    else
                    {
                        var width = rect.width / 2 - 5f;
                        var originalKeys = EventController.MethodDictionary.Keys.ToArray();
                        var methodNameKeys = originalKeys.Select(t => t.MethodName).ToArray();
                        var methodTypeKeys = originalKeys.Select(t => t.MethodType).ToArray();
                        var methodKeys = methodNameKeys.Select(key => key.Split(':')[1]).ToArray();

                        for (var i = 0; i < methodNames.arraySize; i++)
                        {
                            color = GUI.color;
                            GUI.color = i % 2 == 0 ? new Color(0.75f, 0.85f, 1f) : new Color(0.85f, 0.75f, 1f);

                            methodNameRect.y += rectHeight;
                            methodNameRect.x = rect.x;
                            methodNameRect.width = width - 20;

                            if (i >= methodNames.arraySize)
                            {
                                methodNames.arraySize = i + 1;
                            }

                            var item = methodNames.GetArrayElementAtIndex(i);
                            var tag = "";
                            var methodName = "";
                            foreach (var t in item.stringValue)
                            {
                                if (t == ':')
                                {
                                    tag = methodName;
                                    methodName = "";
                                    continue;
                                }

                                methodName += t;
                            }

                            var popupKeys = new List<string>();
                            for (var j = 0; j < methodTypeKeys.Length; j++)
                            {
                                var popupKey = methodTypeKeys[j];
                                popupKey = !string.IsNullOrEmpty(popupKey)
                                    ? $"{methodTypeKeys[j]}/{methodKeys[j]}"
                                    : methodKeys[j];

                                popupKeys.Add(popupKey);
                            }

                            var methodKeysIndex = EditorGUI.Popup(methodNameRect,
                                Array.IndexOf(methodNameKeys, item.stringValue), popupKeys.ToArray());

                            if (methodKeysIndex >= methodNameKeys.Length)
                            {
                                methodKeysIndex = methodNameKeys.Length - 1;
                            }

                            if (methodKeysIndex < 0)
                            {
                                methodKeysIndex = 0;
                            }

                            item.stringValue = methodNameKeys[methodKeysIndex];

                            if (i >= useMethodEvents.arraySize)
                            {
                                useMethodEvents.arraySize = i + 1;
                            }

                            var itemBool = useMethodEvents.GetArrayElementAtIndex(i);

                            methodNameRect.x += methodNameRect.width + 8.5f;
                            methodNameRect.width = 15f;
                            itemBool.boolValue = GUI.Toggle(methodNameRect, itemBool.boolValue, new GUIContent());
                            methodNameRect.x += methodNameRect.width + 8.5f;
                            methodNameRect.width = width - 20;
                            if (i >= methodEvents.arraySize)
                            {
                                methodEvents.arraySize = i + 1;
                            }

                            var itemEvent = methodEvents.GetArrayElementAtIndex(i);
                            GUI.enabled = itemBool.boolValue;

                            if (i >= isGlobalMethodEvents.arraySize)
                            {
                                isGlobalMethodEvents.arraySize = i + 1;
                            }

                            var isGlobalMethodEvent = isGlobalMethodEvents.GetArrayElementAtIndex(i);
                            isGlobalMethodEvent.boolValue = EntrySaver.EventEntryField(methodNameRect, itemEvent,
                                isGlobalMethodEvent.boolValue, GetSceneName());

                            GUI.enabled = true;

                            methodNameRect.y += rectHeight;
                            methodNameRect.x = rect.x;
                            methodNameRect.width = rect.width - 12.5f;

                            if (i >= firstStringParams.arraySize)
                            {
                                firstStringParams.arraySize = i + 1;
                            }

                            var itemFirstString = firstStringParams.GetArrayElementAtIndex(i);

                            if (i >= secondStringParams.arraySize)
                            {
                                secondStringParams.arraySize = i + 1;
                            }

                            var itemSecondString = secondStringParams.GetArrayElementAtIndex(i);

                            if (i >= thirdStringParams.arraySize)
                            {
                                thirdStringParams.arraySize = i + 1;
                            }

                            var itemThirdString = thirdStringParams.GetArrayElementAtIndex(i);


                            switch (tag)
                            {
                                case "Vector2Ease":
                                {
                                    var rectOriginal = methodNameRect;
                                    methodNameRect.width /= 3;

                                    var value = TextProcessingService
                                        .StringVector2ToRealVector2(itemFirstString.stringValue);
                                    itemFirstString.stringValue = TextProcessingService.RealVector2ToStringVector2
                                        (EditorGUI.Vector2Field(methodNameRect, new GUIContent(), value));

                                    methodNameRect.x += methodNameRect.width + 5f;

                                    methodNameRect.width /= 2;
                                    methodNameRect.width -= 10;

                                    GUI.Label(methodNameRect, "Duration");
                                    methodNameRect.x += methodNameRect.width + 5f;

                                    if (!float.TryParse(itemSecondString.stringValue, out var floatValue))
                                    {
                                        floatValue = 0;
                                    }

                                    itemSecondString.stringValue = EditorGUI.FloatField(methodNameRect, floatValue)
                                        .ToString(CultureInfo.CurrentCulture);

                                    methodNameRect.x += methodNameRect.width + 5f;
                                    methodNameRect.width += 10;
                                    methodNameRect.width *= 2;

                                    if (!int.TryParse(itemThirdString.stringValue, out var easeValue))
                                    {
                                        easeValue = 0;
                                    }

                                    itemThirdString.stringValue =
                                        ((int)(Ease)EditorGUI.EnumPopup(methodNameRect, (Ease)easeValue)).ToString();

                                    methodNameRect = rectOriginal;
                                    break;
                                }


                                case "string":
                                {
                                    itemFirstString.stringValue =
                                        EditorGUI.TextField(methodNameRect, itemFirstString.stringValue);
                                    break;
                                }

                                case "Vector2":
                                {
                                    var value = TextProcessingService
                                        .StringVector2ToRealVector2(itemFirstString.stringValue);
                                    itemFirstString.stringValue = TextProcessingService.RealVector2ToStringVector2
                                        (EditorGUI.Vector2Field(methodNameRect, new GUIContent(), value));
                                    break;
                                }

                                case "bool":
                                {
                                    if (!bool.TryParse(itemFirstString.stringValue, out var boolValue))
                                    {
                                        boolValue = false;
                                    }

                                    itemFirstString.stringValue =
                                        EditorGUI.Toggle(methodNameRect, "Bool Value", boolValue).ToString();
                                    break;
                                }

                                case "int":
                                {
                                    if (!int.TryParse(itemFirstString.stringValue, out var intValue))
                                    {
                                        intValue = 0;
                                    }

                                    itemFirstString.stringValue =
                                        EditorGUI.IntField(methodNameRect, intValue).ToString();
                                    break;
                                }

                                case "float":
                                {
                                    if (!float.TryParse(itemFirstString.stringValue, out var floatValue))
                                    {
                                        floatValue = 0f;
                                    }

                                    itemFirstString.stringValue = EditorGUI.FloatField(methodNameRect, floatValue)
                                        .ToString(CultureInfo.CurrentCulture);
                                    break;
                                }


                                case "null":
                                {
                                    GUI.Label(methodNameRect, "No parameters are required.",
                                        new GUIStyle(GUI.skin.label) { normal = { textColor = Color.gray } });
                                    break;
                                }

                                case "scene":
                                {
                                    methodNameRect.width /= 3;
                                    itemFirstString.stringValue =
                                        EditorGUI.TextField(methodNameRect, itemFirstString.stringValue);
                                    methodNameRect.x += methodNameRect.width + 2.5f;

                                    if (!bool.TryParse(itemSecondString.stringValue, out var boolValue))
                                    {
                                        boolValue = false;
                                    }

                                    itemSecondString.stringValue =
                                        EditorGUI.ToggleLeft(methodNameRect, "Is Mute BGM", boolValue).ToString();

                                    methodNameRect.x += methodNameRect.width + 2.5f;

                                    var value = TextProcessingService
                                        .StringVector2ToRealVector2(itemThirdString.stringValue);
                                    itemThirdString.stringValue = TextProcessingService.RealVector2ToStringVector2
                                        (EditorGUI.Vector2Field(methodNameRect, new GUIContent(), value));
                                    break;
                                }

                                default:
                                {
                                    if (tag != "  ")
                                    {
                                        UnityEngine.Debug.Log($"Case {tag} is not defined");
                                    }

                                    goto case "string";
                                }
                            }

                            methodNameRect.x += 3;
                            methodNameRect.y += 2;
                            if (string.IsNullOrEmpty(itemFirstString.stringValue))
                            {
                                GUI.Label(methodNameRect, "Parameter",
                                    new GUIStyle { normal = { textColor = Color.grey } });
                            }

                            methodNameRect.y -= 2;
                            methodNameRect.x -= 3;
                            GUI.color = color;
                        }
                    }

                    GUI.color = color;
                },

                onAddCallback = _ =>
                {
                    serializedObject.Update();
                    OnAddElement(simpleRulesListProperty);
                    serializedObject.ApplyModifiedProperties();
                },

                onRemoveCallback = list =>
                {
                    serializedObject.Update();
                    OnRemoveElement(simpleRulesListProperty, list.index);
                    serializedObject.ApplyModifiedProperties();
                },
                elementHeightCallback = index =>
                {
                    elementHeight ??= new List<float> { 0 };

                    if (index >= elementHeight.Count)
                    {
                        elementHeight.Add(0);
                    }

                    return elementHeight[index] == 0
                        ? EditorGUIUtility.singleLineHeight
                        : elementHeight[index] + 0.75f;
                }
            };
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
                        {
                            continue;
                        }

                        duplicates = true;
                        break;
                    }

                    if (duplicates)
                    {
                        var existingTags = new HashSet<string>();
                        for (var i = 0; i < tagListProperty.arraySize; i++)
                        {
                            if (i != index)
                            {
                                existingTags.Add(tagListProperty.GetArrayElementAtIndex(i).stringValue);
                            }
                        }

                        foreach (var tag in tags)
                        {
                            if (existingTags.Contains(tag))
                            {
                                continue;
                            }

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
                        if (existingTags.Contains(tag))
                        {
                            continue;
                        }

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
                            element.stringValue)
                        {
                            continue;
                        }

                        duplicates = true;
                        break;
                    }

                    var allEvents = EntrySaver.GetEventEntry(true, GetSceneName());
                    var allEventsName = allEvents.Select(item => item.name).ToArray();


                    if (duplicates)
                    {
                        var existingNames = new HashSet<string>();
                        for (var i = 0; i < eventListProperty.arraySize; i++)
                        {
                            if (i == index)
                            {
                                continue;
                            }

                            var existingName = eventListProperty.GetArrayElementAtIndex(i).stringValue;
                            existingNames.Add(existingName);
                        }

                        foreach (var evt in allEvents)
                        {
                            if (existingNames.Contains(evt.name))
                            {
                                continue;
                            }

                            element.stringValue = evt.name;
                            break;
                        }
                    }

                    var eventRect = new Rect(rect.x, rect.y, rect.width / 2 - 5, EditorGUIUtility.singleLineHeight);

                    var currentIndex = Array.IndexOf(allEventsName, element.stringValue);
                    currentIndex = EditorGUI.Popup(eventRect, currentIndex, allEventsName);

                    if (currentIndex >= allEventsName.Length || currentIndex < 0)
                    {
                        currentIndex = 0;
                    }

                    element.stringValue = allEventsName[currentIndex];

                    eventRect.x += (rect.width + 2.5f) / 2;
                    var labelStyle = new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleLeft };
                    GUI.enabled = false;
                    GUI.Label(eventRect, allEvents[index].closeTime.ToString(CultureInfo.CurrentCulture), labelStyle);
                    GUI.enabled = true;
                },

                onCanAddCallback = _ =>
                {
                    var existingNames = new HashSet<string>();
                    for (var i = 0; i < eventListProperty.arraySize; i++)
                    {
                        var eventName = eventListProperty.GetArrayElementAtIndex(i).stringValue;
                        existingNames.Add(eventName);
                    }

                    return existingNames.Count < EntrySaver.GetEventEntry(true, GetSceneName()).Length;
                },

                onAddCallback = _ =>
                {
                    serializedObject.Update();

                    var allEvents = EntrySaver.GetEventEntry(true, GetSceneName());
                    var existingNames = new HashSet<string>();
                    for (var i = 0; i < eventListProperty.arraySize; i++)
                    {
                        var eventName = eventListProperty.GetArrayElementAtIndex(i).stringValue;
                        existingNames.Add(eventName);
                    }

                    foreach (var evt in allEvents)
                    {
                        if (existingNames.Contains(evt.name))
                        {
                            continue;
                        }

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
                            element.enumValueIndex)
                        {
                            continue;
                        }

                        duplicates = true;
                        break;
                    }

                    if (duplicates)
                    {
                        var existingValues = new HashSet<int>();
                        for (var i = 0; i < triggerModeListProperty.arraySize; i++)
                        {
                            if (i != index)
                            {
                                existingValues.Add(triggerModeListProperty.GetArrayElementAtIndex(i).enumValueIndex);
                            }
                        }

                        foreach (EventTriggerMode mode in Enum.GetValues(typeof(EventTriggerMode)))
                        {
                            if (existingValues.Contains((int)mode))
                            {
                                continue;
                            }

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
                        if (existingValues.Contains(mode))
                        {
                            continue;
                        }

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
                {
                    EventTriggerMode
                        .Interact,
                    "调查型：按下调查键后触发。这只能用于玩家。"
                },
                {
                    EventTriggerMode
                        .ColliderEnter,
                    "碰撞器触发型：开始碰撞后触发。"
                },
                {
                    EventTriggerMode
                        .ColliderExit,
                    "碰撞器离开型：结束碰撞后触发。"
                },
                {
                    EventTriggerMode
                        .TriggerEnter,
                    "触发器触发型：进入触发器范围后触发。"
                },
                {
                    EventTriggerMode
                        .TriggerExit,
                    "触发器离开型：离开触发器范围后触发。"
                },
                {
                    EventTriggerMode
                        .LineOfSightEnter,
                    "视线触发型：进入视野（射线范围）时触发。"
                },
                {
                    EventTriggerMode
                        .LineOfSightExit,
                    "视线离开型：离开视野（射线范围）时触发。"
                }
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
            GUILayout.Space(5);
            EditorGUILayout.LabelField("=== Tags ===", centerBoldStyle);
            GUILayout.Space(5);
            _tagList.DoLayoutList();

            GUILayout.Space(5);

            EditorGUILayout.LabelField("=== Events ===", centerBoldStyle);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            trigger.useSimpleRules = GUILayout.Toggle(trigger.useSimpleRules, "Use simple rules");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            if (!trigger.useSimpleRules)
            {
                GUILayout.Space(5);
                _eventList.DoLayoutList();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                trigger.isExecuteAllRules = GUILayout.Toggle(trigger.isExecuteAllRules, "Is Execute All Rules");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                _simpleRulesList.DoLayoutList();
            }

            GUILayout.Space(5);
            EditorGUILayout.LabelField("=== Trigger Modes ===", centerBoldStyle);
            GUILayout.Space(5);
            _triggerModeList.DoLayoutList();

            var isDisplayClarity = trigger.eventTriggerModes.Any(mode =>
                mode is EventTriggerMode.LineOfSightEnter or EventTriggerMode.LineOfSightExit);

            if (isDisplayClarity)
            {
                EditorGUILayout.PrefixLabel("Sight Clarity");
                trigger.clarity = EditorGUILayout.Slider(trigger.clarity, 0, 1);
            }


            serializedObject.ApplyModifiedProperties();
        }

        private void OnAddElement(SerializedProperty listProperty)
        {
            listProperty.arraySize++;
            var element = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
            var nameProperty = element.FindPropertyRelative("name");
            nameProperty.stringValue = "Rule" + listProperty.arraySize;
            RenameDetection(element);
        }

        private void OnRemoveElement(SerializedProperty listProperty, int index)
        {
            listProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }
    }
}