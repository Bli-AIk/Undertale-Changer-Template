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

        protected static void DrawHorizontalLine(Rect rect, float width)
        {
            var lineRect = new Rect(rect.x, rect.y, width, 1);
            GUI.DrawTexture(lineRect, Texture2D.grayTexture);
        }

        protected static void DrawVerticalLine(Rect rect, float height)
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
        protected static void EnumPopup<TEnum>(Rect rect, SerializedProperty property,
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
            var fieldWidth = rect.width / 2 - 10;
            var nameRect = new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
            var isTriggeringRect =
                new Rect(rect.x + fieldWidth + 5, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);

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
            RenameDetection(element);
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

        private static int CalculateCriteriaSize(SerializedProperty criteria, int maxDepth = 5)
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
            extraHeight = 0.25f;
            var originalCriteria = element.FindPropertyRelative("ruleCriterion").FindPropertyRelative("criteria");
            var factModifications = element.FindPropertyRelative("factModifications");
            var methodNames = element.FindPropertyRelative("methodNames");
            var methodStrings = element.FindPropertyRelative("methodStrings");

            if (originalCriteria.arraySize == 0) originalCriteria.arraySize = 1;

            var calculateCriteriaSize = CalculateCriteriaSize(originalCriteria);

            int factAndMethodSize;
            if (factModifications.arraySize == 0 && methodNames.arraySize == 0)
                factAndMethodSize = 1;
            else
                factAndMethodSize = Math.Max(factModifications.arraySize, methodNames.arraySize);
            factAndMethodSize++;
            var lineCount = 3 + calculateCriteriaSize + factAndMethodSize;


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
                RenameDetection(nameProperty);

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
            var triggeredByFieldRect = new Rect(rect.x + fieldWidth + 5, rect.y + rectHeight, fieldWidth,
                EditorGUIUtility.singleLineHeight);

            var triggersLabelRect = new Rect(rect.x + 2 * (fieldWidth + 5), rect.y + rectHeight, fieldWidth,
                EditorGUIUtility.singleLineHeight);
            var triggersFieldRect = new Rect(rect.x + 3 * (fieldWidth + 5), rect.y + rectHeight, fieldWidth,
                EditorGUIUtility.singleLineHeight);

            GUI.Label(triggeredByLabelRect,
                new GUIContent("TriggeredBy", EditorGUIUtility.Load("Icons/EventSystem/Download_2.png") as Texture));
            var property = element.FindPropertyRelative("triggeredBy");
            EntrySaver.EventEntryField(triggeredByFieldRect, property);

            GUI.Label(triggersLabelRect,
                new GUIContent("Triggers", EditorGUIUtility.Load("Icons/EventSystem/Upload_2.png") as Texture));
            var property1 = element.FindPropertyRelative("triggers");
            EntrySaver.EventEntryField(triggersFieldRect, property1);


            //  line 3
            var originalRuleCriterion = element.FindPropertyRelative("ruleCriterion");

            var criteriaLabelRect = triggeredByLabelRect;
            criteriaLabelRect.width = rect.width;
            criteriaLabelRect.y += rectHeight;
            DrawHorizontalLine(criteriaLabelRect, rect.width);
            var result = GetRuleCriterionResult(originalRuleCriterion,
                out _, out _, out _, out _, out _);

            GUI.Label(criteriaLabelRect,
                $"Rule Criterion: <color={(result ? "green" : "red")}>{(result ? "True" : "False")}</color>",
                new GUIStyle(GUI.skin.label) { richText = true });
            //  TODO:计算应当在runtime脚本中进行
            //  element.FindPropertyRelative("isCriteriaPassed").boolValue = result;

            //  line 4
            _ruleCriteriaRectY = rect.y + (lineCount - 2) * rectHeight;
            var originalCriteriaParentOperation = originalRuleCriterion.FindPropertyRelative("operation");
            var factModificationsRect = CreateRuleCriteria(rect, rect.y + rectHeight * 3, originalRuleCriterion,
                originalCriteriaParentOperation,
                fieldWidth, rectHeight, 0, 0);
            GUI.enabled = true;

            //  line 5 left
            DrawHorizontalLine(factModificationsRect, rect.width);
            var lineFiveRect = factModificationsRect;
            factModificationsRect.y += 2.5f;
            GUI.Label(factModificationsRect, "Modifications");
            factModificationsRect.width = fieldWidth / 4;
            factModificationsRect.x += fieldWidth;
            factModificationsRect.height -= 0.25f;
            if (GUI.Button(factModificationsRect, EditorGUIUtility.IconContent("d_Toolbar Plus")))
                factModifications.arraySize++;
            factModificationsRect.x += fieldWidth / 4 + 2.5f;

            GUI.enabled = factModifications.arraySize > 0;
            if (GUI.Button(factModificationsRect, EditorGUIUtility.IconContent("d_Toolbar Minus")))
                factModifications.arraySize--;
            factModificationsRect.x += fieldWidth / 4 + 2.5f;
            GUI.enabled = true;

            factModificationsRect.y += rectHeight - 1f;
            factModificationsRect.height += 0.25f;

            if (factModifications.arraySize == 0)
            {
                factModificationsRect.x = rect.x;
                GUI.Label(factModificationsRect, "No Modifications.",
                    new GUIStyle { normal = { textColor = Color.grey } });
            }
            else
            {
                for (var i = 0; i < factModifications.arraySize; i++)
                {
                    factModificationsRect.x = rect.x;
                    factModificationsRect.width = fieldWidth;

                    if (i >= factModifications.arraySize)
                        factModifications.arraySize = i + 1;
                    var item = factModifications.GetArrayElementAtIndex(i);
                    var fact = item.FindPropertyRelative("fact");
                    EntrySaver.FactEntryField(factModificationsRect, fact);
                    factModificationsRect.x += fieldWidth + 2.5f;

                    var operation = item.FindPropertyRelative("operation");
                    var operationDictionary = new Dictionary<FactModification.Operation, string>
                    {
                        { FactModification.Operation.Change, "=" },
                        { FactModification.Operation.Add, "+" },
                        { FactModification.Operation.Subtract, "-" },
                        { FactModification.Operation.Multiply, "×" },
                        { FactModification.Operation.Divide, "÷" }
                    };
                    factModificationsRect.width /= 2;
                    EnumPopup(factModificationsRect, operation, operationDictionary,
                        new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter });
                    factModificationsRect.x += factModificationsRect.width + 2.5f;

                    var number = item.FindPropertyRelative("number");
                    EditorGUI.PropertyField(factModificationsRect, number, new GUIContent());
                    factModificationsRect.y += rectHeight;
                }
            }

            //  line 5 right
            var methodNameRect = lineFiveRect;
            methodNameRect.x += fieldWidth * 2 + 10f;
            DrawVerticalLine(methodNameRect, rectHeight * factAndMethodSize);
            methodNameRect.x += 2.5f;
            lineFiveRect = methodNameRect;
            methodNameRect.y += 2.5f;

            GUI.Label(methodNameRect, "Method Names");
            methodNameRect.width = fieldWidth / 4;
            methodNameRect.x += fieldWidth + 7.5f;
            methodNameRect.height -= 0.25f;
            if (GUI.Button(methodNameRect, EditorGUIUtility.IconContent("d_Toolbar Plus")))
            {
                methodNames.arraySize++;
                methodStrings.arraySize++;
            }

            methodNameRect.x += fieldWidth / 4 + 2.5f;

            GUI.enabled = methodNames.arraySize > 0;
            if (GUI.Button(methodNameRect, EditorGUIUtility.IconContent("d_Toolbar Minus")))
            {
                methodNames.arraySize--;
                methodStrings.arraySize--;
            }

            methodNameRect.x += fieldWidth / 4 + 2.5f;
            GUI.enabled = true;
            factModificationsRect.y += rectHeight;

            if (methodNames.arraySize == 0)
            {
                methodNameRect.y += rectHeight;
                methodNameRect.x = lineFiveRect.x;
                GUI.Label(methodNameRect, "No Method Names.",
                    new GUIStyle { normal = { textColor = Color.grey } });
            }
            else
            {
                var methodKeys = EventController.MethodDictionary.Keys.ToArray();
                for (var i = 0; i < methodNames.arraySize; i++)
                {
                    methodNameRect.y += rectHeight;
                    methodNameRect.x = lineFiveRect.x;
                    methodNameRect.width = fieldWidth * 1.125f;

                    if (i >= methodNames.arraySize)
                        methodNames.arraySize = i + 1;
                    var item = methodNames.GetArrayElementAtIndex(i);

                    if (i >= methodStrings.arraySize)
                        methodStrings.arraySize = i + 1;
                    var itemString = methodStrings.GetArrayElementAtIndex(i);
                    var methodKeysIndex = EditorGUI.Popup(methodNameRect, Array.IndexOf(methodKeys, item.stringValue),
                        methodKeys);
                    
                    if (methodKeysIndex >= methodKeys.Length) methodKeysIndex = methodKeys.Length - 1;
                    if (methodKeysIndex < 0) methodKeysIndex = 0;

                    item.stringValue = methodKeys[methodKeysIndex];

                    methodNameRect.x += methodNameRect.width + 2.5f;

                    itemString.stringValue = EditorGUI.TextField(methodNameRect, itemString.stringValue);
                    methodNameRect.x += 3;
                    methodNameRect.y += 2;
                    if (string.IsNullOrEmpty(itemString.stringValue))
                        GUI.Label(methodNameRect, "Parameter", new GUIStyle { normal = { textColor = Color.grey } });
                    methodNameRect.y -= 2;
                }
            }
        }

        private static Rect CreateRuleCriteria(Rect rect, float y, SerializedProperty ruleCriterion,
            SerializedProperty criteriaParentOperation, float fieldWidth,
            float rectHeight, int nestedLevel, int index)
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
                        criteriaBoxRect, fieldWidth, operationMap, inputColor, nestedLevel, rectHeight);
                    isGrouped = false;
                }
                else
                {
                    criteriaBoxRect = CreateRuleCriteria(rect, criteriaBoxRect.y, sonRuleCriterion,
                        sonCriteriaParentOperation, fieldWidth, rectHeight, nestedLevel + 1, i);
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
            Color inputColor, int nestedLevel, float rectHeight)
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
            EnumPopup(rect, compare, CompareMap,
                new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter });
            rect.x += fieldWidth / 2 + 2.5f;

            //  detection
            rect.width = fieldWidth / 2;
            EditorGUI.PropertyField(rect, detection, new GUIContent());
            rect.x += fieldWidth / 2 + 2.5f;

            GUI.color = color;

            //  operation
            rect.width = fieldWidth / 2;
            EnumPopup(rect, operation, operationMap,
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
                    GetRuleCriterionResult(ruleCriterion, out _, out _, out _, out _, out _));
                GUI.color = color;
            }

            return rect;
        }

        private static void AddNestedLevelTag(SerializedProperty criteriaParentOperation, Rect rect, float fieldWidth,
            Color inputColor, int nestedLevel, SerializedProperty item, bool result)
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

            if ((int)rect.y >= (int)_ruleCriteriaRectY) return;

            var operationRect = rect;
            operationRect.x += fieldWidth * 1.125f;

            operationRect.width = fieldWidth / 2;
            EnumPopup(operationRect, enumSerializedProperty, GUI.enabled ? OperationMapWithNext : OperationMap,
                new GUIStyle(EditorStyles.popup) { alignment = TextAnchor.MiddleCenter });
            GUI.enabled = true;
        }

        private static bool GetRuleCriterionResult(SerializedProperty item, out bool isResultReversed,
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
            fact.FindPropertyRelative("value").intValue = facts[popup].value;
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