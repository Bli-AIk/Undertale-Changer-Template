using System;
using System.Collections.Generic;
using UCT.EventSystem;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Inspector.EventSystem
{
    [CustomEditor(typeof(OverworldStatusChanger))]
    public class OverworldStatusChangerInspector : UnityEditor.Editor
    {
        private static float _ruleCriteriaRectY;
        public List<float> elementHeight;
        private SerializedProperty _methodTypes;
        private ReorderableList _reorderableList;
        private SerializedProperty _ruleCriteria;
        private SerializedProperty _targetEnables;
        private SerializedProperty _targetSprites;

        private void OnEnable()
        {
            _ruleCriteria = serializedObject.FindProperty("ruleCriteria");
            _methodTypes = serializedObject.FindProperty("methodTypes");
            _targetSprites = serializedObject.FindProperty("targetSprites");
            _targetEnables = serializedObject.FindProperty("targetEnables");
            InitializeReorderableList();
        }

        private static string GetSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((OverworldStatusChanger)target),
                typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            _reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeReorderableList()
        {
            _reorderableList = new ReorderableList(serializedObject, _ruleCriteria, true,
                true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Overworld Status Changer"),

                drawElementCallback = (rect, index, _, _) =>
                {
                    _methodTypes.arraySize = _ruleCriteria.arraySize;
                    _targetSprites.arraySize = _ruleCriteria.arraySize;
                    _targetEnables.arraySize = _ruleCriteria.arraySize;


                    var ruleCriterion = _ruleCriteria.GetArrayElementAtIndex(index);
                    var method = _methodTypes.GetArrayElementAtIndex(index);
                    var sprite = _targetSprites.GetArrayElementAtIndex(index);
                    var enable = _targetEnables.GetArrayElementAtIndex(index);


                    var originalCriteria = ruleCriterion.FindPropertyRelative("criteria");

                    if (originalCriteria.arraySize == 0)
                    {
                        originalCriteria.arraySize = 1;
                    }

                    var calculateCriteriaSize = RuleTableInspector.CalculateCriteriaSize(originalCriteria);

                    var lineCount = calculateCriteriaSize + 2;
                    elementHeight[index] = (EditorGUIUtility.singleLineHeight + 2.5f) * lineCount;

                    var fieldWidth = rect.width / 4 - 10;
                    var rectHeight = rect.height / lineCount;
                    var criteriaLabelRect = rect;
                    criteriaLabelRect.height = rectHeight;
                    // line 1
                    var result = RuleTableInspector.GetRuleCriterionResult(ruleCriterion,
                        out _, out _, out _, out _, out _, out _);

                    GUI.Label(criteriaLabelRect,
                        $"Rule Criterion: <color={(result ? "green" : "red")}>{(result ? "True" : "False")}</color>",
                        new GUIStyle(GUI.skin.label) { richText = true });

                    // line 2
                    criteriaLabelRect.y += rectHeight;
                    var ruleCriteriaRect = criteriaLabelRect;
                    _ruleCriteriaRectY = ruleCriteriaRect.y + (lineCount - 2) * rectHeight;
                    var originalCriteriaParentOperation = ruleCriterion.FindPropertyRelative("operation");
                    var factModificationsRect = RuleTableInspector.CreateRuleCriteria(ruleCriteriaRect,
                        ruleCriteriaRect.y,
                        ruleCriterion,
                        originalCriteriaParentOperation,
                        fieldWidth, rectHeight, 0, 0, _ruleCriteriaRectY, GetSceneName());
                    // line 3
                    factModificationsRect.width = rect.width / 2;

                    method.enumValueIndex =
                        (int)(OverworldStatusChanger.MethodType)EditorGUI.EnumPopup(factModificationsRect,
                            (OverworldStatusChanger.MethodType)method.enumValueIndex);
                    factModificationsRect.x += factModificationsRect.width + 2.5f;

                    switch ((OverworldStatusChanger.MethodType)method.enumValueIndex)
                    {
                        case OverworldStatusChanger.MethodType.ChangeSprite:
                            sprite.objectReferenceValue =
                                EditorGUI.ObjectField(factModificationsRect, sprite.objectReferenceValue,
                                    typeof(Sprite), false);
                            break;
                        case OverworldStatusChanger.MethodType.SetColliderEnable:
                            enable.boolValue = GUI.Toggle(factModificationsRect, enable.boolValue, new GUIContent());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                },

                onAddCallback = _ =>
                {
                    serializedObject.Update();

                    _ruleCriteria.arraySize++;
                    _methodTypes.arraySize = _ruleCriteria.arraySize;
                    _targetSprites.arraySize = _ruleCriteria.arraySize;
                    _targetEnables.arraySize = _ruleCriteria.arraySize;

                    serializedObject.ApplyModifiedProperties();
                },

                onRemoveCallback = _ =>
                {
                    serializedObject.Update();

                    _ruleCriteria.arraySize--;
                    _methodTypes.arraySize = _ruleCriteria.arraySize;
                    _targetSprites.arraySize = _ruleCriteria.arraySize;
                    _targetEnables.arraySize = _ruleCriteria.arraySize;

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
        }
    }
}