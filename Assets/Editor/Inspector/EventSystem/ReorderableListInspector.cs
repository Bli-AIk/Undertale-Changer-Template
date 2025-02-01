using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DG.Tweening;
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
        public List<float> elementHeight;
        public float extraHeight;
        private ReorderableList _reorderableList;

        protected abstract string PropertyName { get; }


        protected abstract string IconPath { get; }

        private void OnEnable()
        {
            InitializeReorderableList();
        }

        protected abstract void DrawElement(Rect rect, SerializedProperty element, int index);

        protected abstract void RenameDetection(SerializedProperty element);

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
                },
                elementHeightCallback = index =>
                {
                    elementHeight ??= new List<float> { 0 };

                    if (index >= elementHeight.Count)
                        elementHeight.Add(0);

                    return elementHeight[index] == 0
                        ? EditorGUIUtility.singleLineHeight
                        : elementHeight[index] + extraHeight;
                }
            };
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            var listProperty = serializedObject.FindProperty(PropertyName);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(TextProcessingService.ToFirstLetterUpperCase(PropertyName), EditorStyles.boldLabel);

            var newCount = EditorGUILayout.DelayedIntField(listProperty.arraySize, GUILayout.Width(48));
            if (newCount != listProperty.arraySize)
            {
                var oldSize = listProperty.arraySize;
                serializedObject.Update();
                listProperty.arraySize = newCount;
                for (var i = oldSize; i < listProperty.arraySize; i++)
                {
                    var element = listProperty.GetArrayElementAtIndex(i);
                    RenameDetection(element);
                }

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
            var scopeProperty = element.FindPropertyRelative("scope");
            EditorGUI.PropertyField(scopeRect, scopeProperty, GUIContent.none);
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

        protected override void RenameDetection(SerializedProperty element)
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
            var fieldWidth = rect.width / 3 - 10;
            var nameRect = new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            var isTriggeringRect =
                new Rect(rect.x + fieldWidth + 5, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            var closeTimeRect =
                new Rect(rect.x + (fieldWidth + 5) * 2, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);

            var nameProperty = element.FindPropertyRelative("name");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
            if (string.IsNullOrEmpty(nameProperty.stringValue))
                nameProperty.stringValue = "Event" + (index + 1);
            if (EditorGUI.EndChangeCheck()) RenameDetection(element);

            var isTriggering = element.FindPropertyRelative("isTriggering").boolValue;

            var boxStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter
            };
            var color = GUI.color;

            string labelText;
            if (isTriggering)
            {
                GUI.color = Color.green;
                labelText = "True";
            }
            else
            {
                GUI.color = Color.red; // 文字颜色为红色
                labelText = "False";
            }

            EditorGUI.LabelField(isTriggeringRect, new GUIContent(labelText), boxStyle);
            GUI.color = color;

            var closeTimeProperty = element.FindPropertyRelative("closeTime");
            var currentValue = closeTimeProperty.floatValue;

            if (currentValue < 0f)
                closeTimeProperty.floatValue = 0f;
            else
                EditorGUI.PropertyField(closeTimeRect, closeTimeProperty, GUIContent.none);
            closeTimeRect.x += 12.5f;
            closeTimeRect.y += 2;
            if (Mathf.Approximately(currentValue, 0f))
                GUI.Label(closeTimeRect, "(0 == Null)", new GUIStyle { normal = { textColor = Color.grey } });
        }

        protected override void RenameDetection(SerializedProperty element)
        {
            var nameProperty = element.FindPropertyRelative("name");
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
            var fieldWidth = rect.width / 3 - 10;

            EditorGUI.LabelField(new Rect(rectX, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), "Name");
            EditorGUI.LabelField(new Rect(rectX + fieldWidth, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                "Is Triggering");
            EditorGUI.LabelField(
                new Rect(rectX + fieldWidth * 2, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                "Close Time");
        }


        protected override void OnAddElement(SerializedProperty listProperty)
        {
            listProperty.arraySize++;
            var element = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
            var nameProperty = element.FindPropertyRelative("name");
            nameProperty.stringValue = "Event" + listProperty.arraySize;
            RenameDetection(element);
            // var closeTimeProperty = element.FindPropertyRelative("closeTime");
            // closeTimeProperty.floatValue = 0.1f;
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
        private static float _ruleCriteriaRectY;

        private static readonly Dictionary<RuleLogicalOperation, string> OperationMapNoNext = new()
        {
            { RuleLogicalOperation.None, ";" }
        };

        private static readonly Dictionary<RuleLogicalOperation, string> OperationMapWithNext = new()
        {
            { RuleLogicalOperation.And, "&&" },
            { RuleLogicalOperation.Or, "||" }
        };

        private static readonly Dictionary<RuleLogicalOperation, string> OperationMap = new()
        {
            { RuleLogicalOperation.None, ";" },
            { RuleLogicalOperation.And, "&&" },
            { RuleLogicalOperation.Or, "||" }
        };

        private static readonly Dictionary<CriteriaCompare, string> CompareMap = new()
        {
            { CriteriaCompare.GreaterThan, ">" },
            { CriteriaCompare.GreaterThanOrEqual, ">=" },
            { CriteriaCompare.Equal, "==" },
            { CriteriaCompare.NotEqual, "!=" },
            { CriteriaCompare.LessThanOrEqual, "<=" },
            { CriteriaCompare.LessThan, "<" }
        };

        protected override string PropertyName => "rules";
        protected override string IconPath => "Icons/EventSystem/Rule.png";

        public static int CalculateCriteriaSize(SerializedProperty criteria, int maxDepth = 5)
        {
            if (criteria is not { isArray: true } || maxDepth <= 0)
                return 0;

            var totalSize = 0;


            var isGrouped = true;
            for (var i = 0; i < criteria.arraySize; i++)
            {
                var element = criteria.GetArrayElementAtIndex(i);

                var nestedCriteria = element.FindPropertyRelative("criteria");

                if (nestedCriteria is { isArray: true, arraySize: > 0 })
                {
                    totalSize += CalculateCriteriaSize(nestedCriteria, maxDepth - 1);
                    isGrouped = true;
                }
                else
                {
                    if (isGrouped)
                        totalSize += 1;

                    totalSize += 1;
                    isGrouped = false;
                }
            }

            return totalSize;
        }

        protected override void DrawElement(Rect rect, SerializedProperty element, int index)
        {
            extraHeight = 0.75f;
            var originalCriteria = element.FindPropertyRelative("ruleCriterion").FindPropertyRelative("criteria");
            var factModifications = element.FindPropertyRelative("factModifications");
            var triggeredBy = element.FindPropertyRelative("triggeredBy");
            var triggers = element.FindPropertyRelative("triggers");
            var methodNames = element.FindPropertyRelative("methodNames");
            var firstStringParams = element.FindPropertyRelative("firstStringParams");
            var secondStringParams = element.FindPropertyRelative("secondStringParams");
            var thirdStringParams = element.FindPropertyRelative("thirdStringParams");
            var useMethodEvents = element.FindPropertyRelative("useMethodEvents");
            var methodEvents = element.FindPropertyRelative("methodEvents");

            if (originalCriteria.arraySize == 0) originalCriteria.arraySize = 1;

            var calculateCriteriaSize = CalculateCriteriaSize(originalCriteria);

            int triggerSize;
            if (triggeredBy.arraySize == 0 && triggers.arraySize == 0)
                triggerSize = 1;
            else
                triggerSize = Math.Max(triggeredBy.arraySize, triggers.arraySize);
            triggerSize++;

            var factSize = factModifications.arraySize + 1;
            if (factModifications.arraySize == 0)
                factSize++;

            var methodSize = methodNames.arraySize + 1;
            if (methodNames.arraySize == 0)
                methodSize++;

            methodSize *= 2;
            methodSize--;

            var lineCount = 2 + triggerSize + calculateCriteriaSize + factSize + methodSize;

            var rectHeight = rect.height / lineCount;
            if (index < elementHeight.Count)
                elementHeight.Add(0);
            elementHeight[index] = (EditorGUIUtility.singleLineHeight + 2.5f) * lineCount;

            EntrySaver.events ??= EntrySaver.GetAllEventEntry();

            var fieldWidth = rect.width / 4 - 10;

            //  line 1
            var nameLabelRect = new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            var nameFieldRect =
                new Rect(rect.x + fieldWidth + 5, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);

            var priorityLabelRect = new Rect(rect.x + 2 * (fieldWidth + 5), rect.y, fieldWidth,
                EditorGUIUtility.singleLineHeight);
            var priorityFieldRect = new Rect(rect.x + 3 * (fieldWidth + 5), rect.y, fieldWidth,
                EditorGUIUtility.singleLineHeight);

            GUI.Label(nameLabelRect, "Name");
            var nameProperty = element.FindPropertyRelative("name");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(nameFieldRect, nameProperty, GUIContent.none);
            if (string.IsNullOrEmpty(nameProperty.stringValue))
                nameProperty.stringValue = "Rule" + (index + 1);
            if (EditorGUI.EndChangeCheck())
                RenameDetection(element);

            GUI.Label(priorityLabelRect, "Priority");
            var rulePriorityProperty = element.FindPropertyRelative("rulePriority");
            var rulePriority = (RulePriority)rulePriorityProperty.enumValueIndex;
            var defaultColor = GUI.color;

            GUI.color = rulePriority switch
            {
                RulePriority.Low => Color.green,
                RulePriority.Medium => Color.yellow,
                RulePriority.High => Color.red,
                _ => throw new ArgumentOutOfRangeException()
            };

            var rulePriorityStyle = new GUIStyle(EditorStyles.popup)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            var rulePriorityOptions = Enum.GetNames(typeof(RulePriority));

            rulePriorityProperty.enumValueIndex = EditorGUI.Popup(
                priorityFieldRect,
                rulePriorityProperty.enumValueIndex,
                rulePriorityOptions,
                rulePriorityStyle
            );

            GUI.color = defaultColor;

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
                if (item != null) triggeredByExistingNames.Add(item.stringValue);
            }

            GUI.enabled = !EntrySaver.events.Select(entry => entry.name)
                .All(nameP => triggeredByExistingNames.Contains(nameP));
            if (GUI.Button(triggeredByLabelRect, EditorGUIUtility.IconContent("d_Toolbar Plus")))
                triggeredBy.arraySize++;
            GUI.enabled = true;

            triggeredByLabelRect.x += fieldWidth / 4 + 2.5f;

            GUI.enabled = triggeredBy.arraySize > 0;
            if (GUI.Button(triggeredByLabelRect, EditorGUIUtility.IconContent("d_Toolbar Minus")))
                triggeredBy.arraySize--;
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
                    triggeredByLabelRect.width = fieldWidth;
                    if (i >= triggeredBy.arraySize)
                        triggeredBy.arraySize = i + 1;

                    var item = triggeredBy.GetArrayElementAtIndex(i);

                    var entryIndex = 0;
                    var allEventEntry = EntrySaver.GetAllEventEntry();
                    var allEventEntryName = new List<string>();
                    for (var j = 0; j < allEventEntry.Length; j++)
                    {
                        allEventEntryName.Add(allEventEntry[j].name);
                        if (allEventEntry[j].name == item.stringValue) entryIndex = j;
                    }

                    entryIndex = EditorGUI.Popup(triggeredByLabelRect, entryIndex, allEventEntryName.ToArray());
                    item.stringValue = allEventEntryName[entryIndex];

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
                if (item != null) triggersExistingNames.Add(item.stringValue);
            }

            GUI.enabled = !EntrySaver.events.Select(entry => entry.name)
                .All(nameP => triggersExistingNames.Contains(nameP));
            if (GUI.Button(triggersRect, EditorGUIUtility.IconContent("d_Toolbar Plus")))
                triggers.arraySize++;
            GUI.enabled = true;

            triggersRect.x += fieldWidth / 4 + 2.5f;

            GUI.enabled = triggers.arraySize > 0;
            if (GUI.Button(triggersRect, EditorGUIUtility.IconContent("d_Toolbar Minus")))
                triggers.arraySize--;
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
                    triggersRect.width = fieldWidth;
                    if (i >= triggers.arraySize)
                        triggers.arraySize = i + 1;

                    var item = triggers.GetArrayElementAtIndex(i);

                    var entryIndex = 0;
                    var allEventEntry = EntrySaver.GetAllEventEntry();
                    var allEventEntryName = new List<string>();
                    for (var j = 0; j < allEventEntry.Length; j++)
                    {
                        allEventEntryName.Add(allEventEntry[j].name);
                        if (allEventEntry[j].name == item.stringValue) entryIndex = j;
                    }

                    entryIndex = EditorGUI.Popup(triggersRect, entryIndex, allEventEntryName.ToArray());
                    item.stringValue = allEventEntryName[entryIndex];
                }
            }

            EnsureUniqueTriggeredBy(triggeredBy);
            EnsureUniqueTriggeredBy(triggers);
            //  line 3
            var criteriaLabelRect = triggeredByLabelRect;

            var originalRuleCriterion = element.FindPropertyRelative("ruleCriterion");

            criteriaLabelRect.width = rect.width;
            criteriaLabelRect.y = rect.y + rectHeight * (triggerSize + 1);
            EditorService.DrawHorizontalLine(criteriaLabelRect, rect.width);

            var useRuleCriterion = element.FindPropertyRelative("useRuleCriterion");
            criteriaLabelRect.y += 1;
            useRuleCriterion.boolValue =
                GUI.Toggle(criteriaLabelRect, useRuleCriterion.boolValue, new GUIContent());
            criteriaLabelRect.y -= 1;

            var color = GUI.color;

            GUI.enabled = useRuleCriterion.boolValue;
            criteriaLabelRect.x += 15;

            bool result;
            if (!useRuleCriterion.boolValue)
                result = true;
            else
                result = GetRuleCriterionResult(originalRuleCriterion,
                    out _, out _, out _, out _, out _);

            GUI.Label(criteriaLabelRect,
                $"Rule Criterion: <color={(result ? "green" : "red")}>{(result ? "True" : "False")}</color>",
                new GUIStyle(GUI.skin.label) { richText = true });

            //  line 4
            _ruleCriteriaRectY = rect.y + (lineCount - 2) * rectHeight;
            var originalCriteriaParentOperation = originalRuleCriterion.FindPropertyRelative("operation");
            var factModificationsRect = CreateRuleCriteria(rect,
                rect.y + rectHeight * (triggerSize + 2),
                originalRuleCriterion,
                originalCriteriaParentOperation,
                fieldWidth, rectHeight, 0, 0, _ruleCriteriaRectY);
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
                factModifications.arraySize++;

            factModificationsRect.x += fieldWidth / 4 + 2.5f;

            GUI.enabled = factModifications.arraySize > 0;
            if (GUI.Button(factModificationsRect, EditorGUIUtility.IconContent("d_Toolbar Minus")))
                factModifications.arraySize--;
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
                        factModifications.arraySize = i + 1;
                    var item = factModifications.GetArrayElementAtIndex(i);
                    var fact = item.FindPropertyRelative("fact");
                    EntrySaver.FactEntryField(factModificationsRect, fact);
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
                var keys = EventController.MethodDictionary.Keys.ToArray();
                var methodKeys = keys.Select(key => key.Split(':')[1]).ToArray();

                for (var i = 0; i < methodNames.arraySize; i++)
                {
                    color = GUI.color;
                    GUI.color = i % 2 == 0 ? new Color(0.75f, 0.85f, 1f) : new Color(0.85f, 0.75f, 1f);

                    methodNameRect.y += rectHeight;
                    methodNameRect.x = rect.x;
                    methodNameRect.width = width - 20;

                    if (i >= methodNames.arraySize)
                        methodNames.arraySize = i + 1;
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

                    var methodKeysIndex = EditorGUI.Popup(methodNameRect,
                        Array.IndexOf(keys, item.stringValue), methodKeys);

                    if (methodKeysIndex >= keys.Length) methodKeysIndex = keys.Length - 1;
                    if (methodKeysIndex < 0) methodKeysIndex = 0;
                    item.stringValue = keys[methodKeysIndex];

                    if (i >= useMethodEvents.arraySize)
                        useMethodEvents.arraySize = i + 1;
                    var itemBool = useMethodEvents.GetArrayElementAtIndex(i);

                    methodNameRect.x += methodNameRect.width + 8.5f;
                    methodNameRect.width = 15f;
                    itemBool.boolValue = GUI.Toggle(methodNameRect, itemBool.boolValue, new GUIContent());
                    methodNameRect.x += methodNameRect.width + 8.5f;
                    methodNameRect.width = width - 20;
                    if (i >= methodEvents.arraySize)
                        methodEvents.arraySize = i + 1;
                    var itemEvent = methodEvents.GetArrayElementAtIndex(i);


                    var allEventEntry = EntrySaver.GetAllEventEntry();
                    var allEventEntryName = allEventEntry.Select(t => t.name).ToArray();
                    var entryIndex = 0;
                    for (var eventEntryNameIndex = 0;
                         eventEntryNameIndex < allEventEntryName.Length;
                         eventEntryNameIndex++)
                    {
                        var eventName = allEventEntryName[eventEntryNameIndex];
                        if (itemEvent.stringValue != eventName) continue;
                        entryIndex = eventEntryNameIndex;
                        break;
                    }

                    GUI.enabled = itemBool.boolValue;
                    entryIndex = EditorGUI.Popup(methodNameRect, entryIndex, allEventEntryName);
                    GUI.enabled = true;
                    itemEvent.stringValue = allEventEntryName[entryIndex];

                    methodNameRect.y += rectHeight;
                    methodNameRect.x = rect.x;
                    methodNameRect.width = rect.width - 12.5f;

                    if (i >= firstStringParams.arraySize)
                        firstStringParams.arraySize = i + 1;
                    var itemFirstString = firstStringParams.GetArrayElementAtIndex(i);

                    if (i >= secondStringParams.arraySize)
                        secondStringParams.arraySize = i + 1;
                    var itemSecondString = secondStringParams.GetArrayElementAtIndex(i);

                    if (i >= thirdStringParams.arraySize)
                        thirdStringParams.arraySize = i + 1;
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
                                floatValue = 0;

                            itemSecondString.stringValue = EditorGUI.FloatField(methodNameRect, floatValue)
                                .ToString(CultureInfo.CurrentCulture);

                            methodNameRect.x += methodNameRect.width + 5f;
                            methodNameRect.width += 10;
                            methodNameRect.width *= 2;

                            if (!int.TryParse(itemThirdString.stringValue, out var easeValue))
                                easeValue = 0;

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

                        case "null":
                        {
                            GUI.Label(methodNameRect, "No parameters are required.",
                                new GUIStyle(GUI.skin.label) { normal = { textColor = Color.gray } });
                            break;
                        }

                        default:
                        {
                            if (tag != "  ")
                                UnityEngine.Debug.Log($"Case {tag} is not defined");
                            goto case "string";
                        }
                    }

                    methodNameRect.x += 3;
                    methodNameRect.y += 2;
                    if (string.IsNullOrEmpty(itemFirstString.stringValue))
                        GUI.Label(methodNameRect, "Parameter",
                            new GUIStyle { normal = { textColor = Color.grey } });
                    methodNameRect.y -= 2;
                    methodNameRect.x -= 3;
                    GUI.color = color;
                }
            }

            GUI.color = color;
        }

        public static void EnsureUniqueTriggeredBy(SerializedProperty triggeredByList)
        {
            if (triggeredByList == null || EntrySaver.events == null || EntrySaver.events.Length == 0)
            {
                UCT.Other.Debug.LogError("triggeredBy列表或EntrySaver.events为空，无法保证唯一性。");
                return;
            }

            var usedNames = new HashSet<string>();

            for (var i = 0; i < triggeredByList.arraySize; i++)
            {
                var triggeredByProperty = triggeredByList.GetArrayElementAtIndex(i);

                var originalName = triggeredByProperty.stringValue;

                if (usedNames.Contains(originalName))
                {
                    var newName = EntrySaver.events.Select(e => e.name)
                        .FirstOrDefault(itemName => !usedNames.Contains(itemName));

                    if (!string.IsNullOrEmpty(newName))
                    {
                        triggeredByProperty.stringValue = newName;
                    }
                    else
                    {
                        triggeredByList.arraySize--;
                        continue;
                    }
                }

                usedNames.Add(triggeredByProperty.stringValue);
            }
        }

        public static Rect CreateRuleCriteria(Rect rect, float y, SerializedProperty ruleCriterion,
            SerializedProperty criteriaParentOperation, float fieldWidth,
            float rectHeight, int nestedLevel, int index, float ruleCriteriaRectY)
        {
            var criteria = ruleCriterion.FindPropertyRelative("criteria");
            var inputColor = GetCriterionBoxColor(nestedLevel, index);
            var criteriaBoxRect =
                new Rect(rect.x, y, rect.width, EditorGUIUtility.singleLineHeight);
            for (var i = 0; i < criteria.arraySize; i++)
            {
                criteriaBoxRect.width = rect.width;
                var operationMap =
                    i == criteria.arraySize - 1 ? OperationMapNoNext : OperationMapWithNext;
                var sonRuleCriterion = criteria.GetArrayElementAtIndex(i);
                var sonCriteria = sonRuleCriterion.FindPropertyRelative("criteria");
                var sonCriteriaParentOperation = criteria.GetArrayElementAtIndex(i).FindPropertyRelative("operation");
                bool isGrouped;
                if (sonCriteria.arraySize == 0)
                {
                    criteriaBoxRect = CreateRuleCriterion(ruleCriterion, criteria, criteriaParentOperation, i,
                        criteriaBoxRect, fieldWidth, operationMap, inputColor, nestedLevel, rectHeight,
                        ruleCriteriaRectY);
                    isGrouped = false;
                }
                else
                {
                    criteriaBoxRect = CreateRuleCriteria(rect, criteriaBoxRect.y, sonRuleCriterion,
                        sonCriteriaParentOperation, fieldWidth, rectHeight, nestedLevel + 1, i, ruleCriteriaRectY);
                    isGrouped = true;
                }

                criteriaBoxRect.x = rect.x;
                if (!isGrouped)
                    criteriaBoxRect.y += rectHeight;
            }

            return criteriaBoxRect;
        }

        /// <summary>
        ///     根据嵌套层级和索引生成一个颜色，这个颜色会避开接近红色和绿色的颜色。
        /// </summary>
        /// <param name="nestedLevel">嵌套层级，决定颜色的亮度和饱和度。</param>
        /// <param name="index">索引，用于计算在当前色相范围内的具体颜色。</param>
        /// <returns>生成的颜色</returns>
        private static Color GetCriterionBoxColor(int nestedLevel, int index)
        {
            const int total = 5;
            nestedLevel = Mathf.Max(nestedLevel, 0);
            index = PingPongIndex(index, total);

            var hueRangeStart = nestedLevel % 2 == 0 ? 0.55f : 0.05f;
            var hueRangeEnd = nestedLevel % 2 == 0 ? 0.9f : 0.45f;

            var hue = Mathf.Lerp(hueRangeStart, hueRangeEnd, (float)index / (total - 1));
            var saturation = Mathf.Clamp01(1.0f - nestedLevel * 0.2f);
            var value = Mathf.Clamp01(1.0f - nestedLevel * 0.2f);

            var color = Color.HSVToRGB(hue, saturation, value);
            return color;
        }

        private static int PingPongIndex(int index, int total)
        {
            var cycleLength = 2 * (total - 1); // 往返一次的周期长度
            var modIndex = index % cycleLength; // 将index限制在一个周期内
            return modIndex < total ? modIndex : cycleLength - modIndex;
        }

        private static Rect CriteriaButtonGroup(Rect rect, float fieldWidth, SerializedProperty criteria, int index,
            int nestedLevel)
        {
            if (!GUI.enabled) return rect;

            var color = GUI.color;
            GUI.color = Color.white * 0.8f;
            rect.width = fieldWidth / 4;

            if (GUI.Button(rect, EditorGUIUtility.IconContent("d_Toolbar Plus")))
                AddCriterion(criteria, index);
            rect.x += fieldWidth / 4 + 2.5f;

            GUI.enabled = criteria.arraySize > 1;
            if (GUI.Button(rect, EditorGUIUtility.IconContent("d_Toolbar Minus")))
                RemoveCriterion(criteria, index);
            rect.x += fieldWidth / 4 + 2.5f;
            GUI.enabled = true;

            GUI.enabled = index > 0;
            if (GUI.Button(rect, (Texture)EditorGUIUtility.Load("Icons/EventSystem/Arrow_Upward.png")))
                MoveCriterionUp(criteria, index);
            rect.x += fieldWidth / 4 + 2.5f;
            GUI.enabled = true;

            GUI.enabled = index < criteria.arraySize - 1;
            if (GUI.Button(rect, (Texture)EditorGUIUtility.Load("Icons/EventSystem/Arrow_Downward.png")))
                MoveCriterionDown(criteria, index);
            rect.x += fieldWidth / 4 + 2.5f;
            GUI.enabled = true;

            GUI.enabled = nestedLevel < 3;
            if (GUI.Button(rect, (Texture)EditorGUIUtility.Load("Icons/EventSystem/Ad_Group.png")))
                GroupCriteria(criteria.GetArrayElementAtIndex(index));
            rect.x += fieldWidth / 4 + 2.5f;
            GUI.enabled = true;

            GUI.enabled = nestedLevel > 0;
            if (GUI.Button(rect, (Texture)EditorGUIUtility.Load("Icons/EventSystem/Ad_Group_Off.png")))
                UngroupCriteria(criteria);
            GUI.color = color;
            GUI.enabled = true;

            return rect;
        }

        private static void AddCriterion(SerializedProperty criterion, int index)
        {
            Undo.RecordObject(criterion.serializedObject.targetObject, "Add Criterion");
            criterion.InsertArrayElementAtIndex(index);
            criterion.serializedObject.ApplyModifiedProperties();
        }

        private static void RemoveCriterion(SerializedProperty criterion, int index)
        {
            Undo.RecordObject(criterion.serializedObject.targetObject, "Remove Criterion");
            criterion.DeleteArrayElementAtIndex(index);
            criterion.serializedObject.ApplyModifiedProperties();
        }

        private static void MoveCriterionUp(SerializedProperty criterion, int index)
        {
            if (index <= 0 || index >= criterion.arraySize) return;
            Undo.RecordObject(criterion.serializedObject.targetObject, "Move Criterion Up");
            criterion.MoveArrayElement(index, index - 1);
            criterion.serializedObject.ApplyModifiedProperties();
        }

        private static void MoveCriterionDown(SerializedProperty criterion, int index)
        {
            if (index < 0 || index >= criterion.arraySize - 1) return;
            Undo.RecordObject(criterion.serializedObject.targetObject, "Move Criterion Down");
            criterion.MoveArrayElement(index, index + 1);
            criterion.serializedObject.ApplyModifiedProperties();
        }

        private static void GroupCriteria(SerializedProperty criterion)
        {
            var group = criterion.FindPropertyRelative("criteria");
            group.arraySize = 1;

            var newCriteria = group.GetArrayElementAtIndex(0);

            var criteriaFact = criterion.FindPropertyRelative("fact");
            var newCriteriaFact = newCriteria.FindPropertyRelative("fact");

            EditorService.CopyProperty(criterion, newCriteria, "isResultReversed");
            EditorService.CopyProperty(criteriaFact, newCriteriaFact, "name");
            EditorService.CopyProperty(criteriaFact, newCriteriaFact, "scope");
            EditorService.CopyProperty(criteriaFact, newCriteriaFact, "value");
            EditorService.CopyProperty(criteriaFact, newCriteriaFact, "area");
            EditorService.CopyProperty(criteriaFact, newCriteriaFact, "scene");
            EditorService.CopyProperty(criterion, newCriteria, "compare");
            EditorService.CopyProperty(criterion, newCriteria, "detection");
            EditorService.CopyProperty(criterion, newCriteria, "operation");

            EditorService.ResetProperty(criterion, "isResultReversed", false);
            EditorService.ResetProperty(criteriaFact, "name", "");
            EditorService.ResetProperty(criteriaFact, "scope", 0);
            EditorService.ResetProperty(criteriaFact, "value", 0);
            EditorService.ResetProperty(criteriaFact, "area", 0);
            EditorService.ResetProperty(criteriaFact, "scene", "");
            EditorService.ResetProperty(criterion, "compare", 0);
            EditorService.ResetProperty(criterion, "detection", 0);
        }


        private static void UngroupCriteria(SerializedProperty criteria)
        {
            criteria.arraySize = 0;
        }


        private static Rect CreateRuleCriterion(SerializedProperty ruleCriterion, SerializedProperty criteria,
            SerializedProperty criteriaParentOperation,
            int index, Rect rect, float fieldWidth, Dictionary<RuleLogicalOperation, string> operationMap,
            Color inputColor, int nestedLevel, float rectHeight, float ruleCriteriaRectY)
        {
            var rectX = rect.x;
            var item = criteria.GetArrayElementAtIndex(index);
            var backgroundRect = rect;
            var color = GUI.color;
            GUI.color = inputColor;
            //  Background
            GUI.Box(backgroundRect, "");
            GUI.color = color;

            rect.width = fieldWidth;

            //  Field
            color = GUI.color;
            var result = GetRuleCriterionResult(item,
                out var isResultReversed,
                out var fact,
                out var compare,
                out var detection,
                out var operation);


            GUI.color = Color.Lerp(inputColor, result ? Color.green : new Color(1f, 0.4f, 0.4f), 0.6f);

            //  isResultReversed
            rect.width = fieldWidth / 8;
            GUIStyle resultReversedStyle = new();
            GUI.Label(rect, isResultReversed ? "!" : "·", new GUIStyle(GUI.skin.label)
                { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
            item.FindPropertyRelative("isResultReversed").boolValue = GUI.Toggle(
                rect,
                isResultReversed,
                GUIContent.none,
                resultReversedStyle
            );
            rect.x += fieldWidth / 8 + 2.5f;
            rect.width = fieldWidth;

            //  fact
            EntrySaver.FactEntryField(rect, fact);


            //  compare
            rect.x += fieldWidth + 2.5f;
            rect.width = fieldWidth / 2;
            EditorService.EnumPopup(rect, compare, CompareMap,
                new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter });
            rect.x += fieldWidth / 2 + 2.5f;

            //  detection
            rect.width = fieldWidth / 2;
            EditorGUI.PropertyField(rect, detection, new GUIContent());
            rect.x += fieldWidth / 2 + 2.5f;

            GUI.color = color;

            //  operation
            rect.width = fieldWidth / 2;
            EditorService.EnumPopup(rect, operation, operationMap,
                new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter });
            rect.x += fieldWidth / 2 + 2.5f;

            var mousePosition = Event.current.mousePosition;
            if (backgroundRect.Contains(mousePosition))
                rect = CriteriaButtonGroup(rect, fieldWidth, criteria, index, nestedLevel);

            SerializedProperty itemNext = null;
            if (index + 1 < criteria.arraySize)
                itemNext = criteria.GetArrayElementAtIndex(index + 1).FindPropertyRelative("criteria");
            var isGrouped = itemNext != null && itemNext.arraySize != 0;

            // ReSharper disable once InvertIf
            if (index == criteria.arraySize - 1 || isGrouped)
            {
                rect.y += rectHeight;
                rect.x = rectX;
                rect.width = fieldWidth * 3;
                AddNestedLevelTag(criteriaParentOperation, rect, fieldWidth, inputColor, nestedLevel, item,
                    GetRuleCriterionResult(ruleCriterion, out _, out _, out _, out _, out _), ruleCriteriaRectY);
                GUI.color = color;
            }

            return rect;
        }

        private static void AddNestedLevelTag(SerializedProperty criteriaParentOperation, Rect rect, float fieldWidth,
            Color inputColor, int nestedLevel, SerializedProperty item, bool result, float ruleCriteriaRectY)
        {
            var color = GUI.color;
            GUI.color = inputColor;
            rect.x += nestedLevel * fieldWidth * 0.1f;


            GUI.Label(rect,
                $"\u25b2 Layer{nestedLevel}: <color={(result ? "green" : "red")}>{(result ? "True" : "False")}</color>",
                new GUIStyle(GUI.skin.label) { richText = true });

            var enumSerializedProperty = criteriaParentOperation;
            if (nestedLevel == 0)
            {
                GUI.enabled = false;
                enumSerializedProperty = item.FindPropertyRelative("operation");
            }

            GUI.color = color;

            if ((int)rect.y >= (int)ruleCriteriaRectY) return;

            var operationRect = rect;
            operationRect.x += fieldWidth * 1.125f;

            operationRect.width = fieldWidth / 2;
            EditorService.EnumPopup(operationRect, enumSerializedProperty,
                GUI.enabled ? OperationMapWithNext : OperationMap,
                new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter });
            GUI.enabled = true;
        }

        public static bool GetRuleCriterionResult(SerializedProperty item, out bool isResultReversed,
            out SerializedProperty fact, out SerializedProperty compare, out SerializedProperty detection,
            out SerializedProperty operation)
        {
            isResultReversed = item.FindPropertyRelative("isResultReversed").boolValue;
            var factEntry = ParseFactEntry(item, out fact);
            compare = item.FindPropertyRelative("compare");
            detection = item.FindPropertyRelative("detection");
            operation = item.FindPropertyRelative("operation");
            var criteria = item.FindPropertyRelative("criteria");
            var result = RuleCriterion.GetResult(
                isResultReversed,
                factEntry,
                (CriteriaCompare)compare.intValue,
                detection.intValue,
                GetRuleCriteria(criteria));
            return result;
        }

        private static FactEntry ParseFactEntry(SerializedProperty item, out SerializedProperty fact)
        {
            fact = item.FindPropertyRelative("fact");
            var factEntry = new FactEntry
            {
                name = fact.FindPropertyRelative("name").stringValue,
                scope = (Scope)fact.FindPropertyRelative("scope").enumValueIndex,
                value = fact.FindPropertyRelative("value").intValue,
                area = (Area)fact.FindPropertyRelative("area").enumValueIndex,
                scene = fact.FindPropertyRelative("scene").stringValue
            };
            return factEntry;
        }

        private static List<RuleCriterion> GetRuleCriteria(SerializedProperty property, int maxDepth = 5)
        {
            return ParseCriteria(property, 0, maxDepth);
        }

        private static List<RuleCriterion> ParseCriteria(SerializedProperty property, int currentDepth, int maxDepth)
        {
            var criteriaList = new List<RuleCriterion>();

            if (currentDepth >= maxDepth)
            {
                UCT.Other.Debug.LogWarning("Reached maximum depth of criteria parsing. Skipping deeper levels.");
                return criteriaList;
            }

            if (property == null || property.arraySize == 0) return criteriaList;

            for (var i = 0; i < property.arraySize; i++)
            {
                var element = property.GetArrayElementAtIndex(i);

                var ruleCriterion = new RuleCriterion
                {
                    isResultReversed = element.FindPropertyRelative("isResultReversed").boolValue,
                    fact = ParseFactEntry(element, out _),
                    compare = (CriteriaCompare)element.FindPropertyRelative("compare").enumValueIndex,
                    detection = element.FindPropertyRelative("detection").intValue,
                    operation = (RuleLogicalOperation)element.FindPropertyRelative("operation").enumValueIndex,
                    criteria = ParseCriteria(element.FindPropertyRelative("criteria"), currentDepth + 1, maxDepth)
                };

                criteriaList.Add(ruleCriterion);
            }

            return criteriaList;
        }

        protected override void RenameDetection(SerializedProperty element)
        {
            var nameProperty = element.FindPropertyRelative("name");
            var nameP = nameProperty.stringValue;
            EntrySaver.rules = EntrySaver.GetAllRuleEntry();
            while (EntrySaver.rules.Any(bRule => bRule.name == nameP))
                nameP += "_b";
            nameProperty.stringValue = nameP;
            SetAllRuleEntry();
        }

        protected override void DrawHeader(Rect rect)
        {
            var rectX = rect.x + 15;
            var fieldWidth = rect.width - 10;

            EditorGUI.LabelField(new Rect(rectX, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight), "Rules");
        }

        protected override void OnAddElement(SerializedProperty listProperty)
        {
            listProperty.arraySize++;
            var element = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
            var nameProperty = element.FindPropertyRelative("name");
            nameProperty.stringValue = "Rule" + listProperty.arraySize;
            RenameDetection(element);
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
}