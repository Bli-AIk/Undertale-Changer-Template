using UCT.Control;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Inspector
{
    [CustomEditor(typeof(CharacterSpriteManager), true)]
    public class CharacterSpriteManagerInspector : UnityEditor.Editor
    {
        private SerializedProperty _fxKeys;

        private ReorderableList _fxList;
        private SerializedProperty _fxValues;
        private float _spriteFieldWidth;
        private SerializedProperty _spriteKeys;

        private ReorderableList _spriteList;
        private SerializedProperty _spriteValues;

        private void OnEnable()
        {
            _fxKeys = serializedObject.FindProperty("fxKeys");
            _fxValues = serializedObject.FindProperty("fxValues");
            _spriteKeys = serializedObject.FindProperty("spriteKeys");
            _spriteValues = serializedObject.FindProperty("spriteValues");
            InitializeReorderableList();
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var characterSpriteManager = (CharacterSpriteManager)target;

            if (characterSpriteManager.spriteValues.Count == 0 || !characterSpriteManager.spriteValues[0])
            {
                return base.RenderStaticPreview(assetPath, subAssets, width, height);
            }

            var firstSprite = characterSpriteManager.spriteValues[0].defaultSprite.texture;
            var previewIcon = new Texture2D(width, height);
            EditorUtility.CopySerialized(firstSprite, previewIcon);
            return previewIcon;
        }

        private void InitializeReorderableList()
        {
            _fxList = new ReorderableList(serializedObject, _fxKeys, true,
                true, true, true)
            {
                drawHeaderCallback = rect =>
                    EditorGUI.LabelField(rect, "Fx"),

                drawElementCallback = (rect, index, _, _) =>
                {
                    var fieldWidth = rect.width / 2 - 10;
                    var fxKeyRect = new Rect(rect.x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
                    var fxValueRect =
                        new Rect(rect.x + fieldWidth + 5, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight);
                    if (_fxKeys.arraySize <= index)
                    {
                        _fxKeys.arraySize = index + 1;
                    }

                    _fxValues.arraySize = _fxKeys.arraySize;

                    var fxKey = _fxKeys.GetArrayElementAtIndex(index);
                    var fxValue = _fxValues.GetArrayElementAtIndex(index);

                    fxKey.stringValue = GUI.TextField(fxKeyRect,
                        fxKey.stringValue);
                    if (string.IsNullOrEmpty(fxKey.stringValue))
                    {
                        if (fxValue.objectReferenceValue)
                        {
                            fxKey.stringValue = ((AudioClip)fxValue.objectReferenceValue).name;
                        }
                        else
                        {
                        
                            GUI.Label(fxKeyRect, "Fx Key",
                                new GUIStyle(GUI.skin.label) { normal = { textColor = Color.gray } });
                        }
                    }

                    fxValue.objectReferenceValue = EditorGUI.ObjectField(fxValueRect,
                        fxValue.objectReferenceValue, typeof(AudioClip), false);
                },

                onAddCallback = _ =>
                {
                    serializedObject.Update();
                    _fxKeys.arraySize++;
                    _fxValues.arraySize = _fxKeys.arraySize;
                    serializedObject.ApplyModifiedProperties();
                },

                onRemoveCallback = _ =>
                {
                    serializedObject.Update();
                    _fxKeys.arraySize--;
                    _fxValues.arraySize = _fxKeys.arraySize;
                    serializedObject.ApplyModifiedProperties();
                },
                elementHeightCallback = _ => EditorGUIUtility.singleLineHeight
            };

            _spriteList = new ReorderableList(serializedObject, _spriteKeys, true,
                true, true, true)
            {
                drawHeaderCallback = rect =>
                    EditorGUI.LabelField(rect, "Sprite"),

                drawElementCallback = (rect, index, _, _) =>
                {
                    _spriteFieldWidth = rect.width / 2 - 10;
                    if (_spriteKeys.arraySize <= index)
                    {
                        _spriteKeys.arraySize = index + 1;
                    }

                    var spriteKeyRect = new Rect(rect.x, rect.y, _spriteFieldWidth, EditorGUIUtility.singleLineHeight);
                    Rect spriteValueRect;
                    if (!_spriteValues.GetArrayElementAtIndex(index).objectReferenceValue)
                    {
                        spriteValueRect = new Rect(rect.x + _spriteFieldWidth + 5, rect.y, _spriteFieldWidth,
                            EditorGUIUtility.singleLineHeight);
                    }
                    else
                    {
                        spriteValueRect = spriteKeyRect;
                        spriteValueRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
                    }

                    _spriteValues.arraySize = _spriteKeys.arraySize;
                    var spriteKey = _spriteKeys.GetArrayElementAtIndex(index);
                    var spriteValue = _spriteValues.GetArrayElementAtIndex(index);

                    spriteKey.stringValue = GUI.TextField(spriteKeyRect,
                        spriteKey.stringValue);
                    if (string.IsNullOrEmpty(spriteKey.stringValue))
                    {
                        if (spriteValue.objectReferenceValue)
                        {
                            spriteKey.stringValue = ((SpriteExpressionCollection)spriteValue.objectReferenceValue).name;
                        }
                        else
                        {
                            GUI.Label(spriteKeyRect, "Sprite Key",
                                new GUIStyle(GUI.skin.label) { normal = { textColor = Color.gray } });
                        }
                    }

                    spriteValue.objectReferenceValue = EditorGUI.ObjectField(spriteValueRect,
                        spriteValue.objectReferenceValue, typeof(SpriteExpressionCollection), false);
                    
                    var spriteTextureRect =
                        new Rect(rect.x + _spriteFieldWidth + 5, rect.y, _spriteFieldWidth,
                            EditorGUIUtility.singleLineHeight * 3);
                    
                    if (spriteValue.objectReferenceValue is SpriteExpressionCollection collection)
                    {
                        GUI.DrawTexture(spriteTextureRect, collection.defaultSprite.texture, ScaleMode.ScaleToFit);
                    }
                },

                onAddCallback = _ =>
                {
                    serializedObject.Update();
                    _spriteKeys.arraySize++;
                    _spriteValues.arraySize = _spriteKeys.arraySize;
                    serializedObject.ApplyModifiedProperties();
                },

                onRemoveCallback = _ =>
                {
                    serializedObject.Update();
                    _spriteKeys.arraySize--;
                    _spriteValues.arraySize = _spriteKeys.arraySize;
                    serializedObject.ApplyModifiedProperties();
                },
                elementHeightCallback = index =>
                    !_spriteValues.GetArrayElementAtIndex(index).objectReferenceValue
                        ? EditorGUIUtility.singleLineHeight
                        : EditorGUIUtility.singleLineHeight * 3
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((CharacterSpriteManager)target),
                typeof(ScriptableObject), false);
            EditorGUI.EndDisabledGroup();
            _fxList.DoLayoutList();
            GUILayout.Space(20);
            _spriteList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

    }
}