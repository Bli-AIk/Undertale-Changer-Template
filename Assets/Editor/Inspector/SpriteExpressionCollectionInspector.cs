using UCT.Control;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspector
{
    [CustomEditor(typeof(SpriteExpressionCollection), true)]
    public class SpriteExpressionCollectionInspector : UnityEditor.Editor
    {
        private int _blinkingIndex;
        private float _blinkingLastUpdateTime;
        private int _speakingIndex;
        private float _speakingLastUpdateTime;

        private void OnEnable()
        {
            EditorApplication.update += UpdateSpeakingTimer;
            EditorApplication.update += UpdateBlinkingTimer;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateSpeakingTimer;
            EditorApplication.update -= UpdateBlinkingTimer;
        }

        private void UpdateExpressionTimer(ref int index, ref float lastUpdateTime, float interval, int count)
        {
            if (!(Time.realtimeSinceStartup - lastUpdateTime >= interval) || count == 0)
            {
                return;
            }

            lastUpdateTime = Time.realtimeSinceStartup;
            index = (index + 1) % count;

            Repaint();
        }

        private void UpdateSpeakingTimer()
        {
            var spriteExpressionCollection = (SpriteExpressionCollection)target;
            if (spriteExpressionCollection.speakingSprites == null)
            {
                return;
            }
            UpdateExpressionTimer(ref _speakingIndex, ref _speakingLastUpdateTime, 0.2f,
                spriteExpressionCollection.speakingSprites.Count);
        }

        private void UpdateBlinkingTimer()
        {
            var spriteExpressionCollection = (SpriteExpressionCollection)target;  
            if (spriteExpressionCollection.blinkingSprites == null)
            {
                return;
            }
            UpdateExpressionTimer(ref _blinkingIndex, ref _blinkingLastUpdateTime, 0.2f,
                spriteExpressionCollection.blinkingSprites.Count);
        }


        public override void OnInspectorGUI()
        {
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Object", target,
                typeof(ScriptableObject), false);
            EditorGUI.EndDisabledGroup();
            base.OnInspectorGUI();
            var spriteExpressionCollection = (SpriteExpressionCollection)target;


            GUILayout.Label("Preview", EditorStyles.boldLabel);

            var lastRect = GUILayoutUtility.GetLastRect();
            var width = lastRect.width;
            lastRect.y += 5;
            var lineHeight = lastRect.height;
            const int spriteWidth = 25;
            lastRect.y += lineHeight;
            lastRect.width = spriteWidth * 4;
            lastRect.height = spriteWidth * 4;

            if (spriteExpressionCollection.defaultSprite)
            {
                lastRect = ShowSprite(lastRect, spriteExpressionCollection.defaultSprite.texture, "Default", lineHeight,
                    width);
            }

            if (spriteExpressionCollection.speakingSprites is { Count: > 0 } &&
                spriteExpressionCollection.speakingSprites[_speakingIndex])
            {
                lastRect = ShowSprite(lastRect,
                    spriteExpressionCollection.speakingSprites[_speakingIndex].texture, "Speaking", lineHeight, width);
            }

            if (spriteExpressionCollection.blinkingSprites is { Count: > 0 } &&
                spriteExpressionCollection.blinkingSprites[_blinkingIndex])
            {
                ShowSprite(lastRect,
                    spriteExpressionCollection.blinkingSprites[_blinkingIndex].texture, "Blinking", lineHeight, width);
            }
        }

        private static Rect ShowSprite(Rect rect,
            Texture2D texture,
            string text,
            float lineHeight,
            float width)
        {
            var centeredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            GUI.DrawTexture(rect, texture,
                ScaleMode.ScaleToFit);
            rect.y += (rect.height + lineHeight) / 2;
            GUI.Label(rect, text, centeredStyle);
            rect.y -= (rect.height + lineHeight) / 2;
            rect.x += width / 3 + 20;
            return rect;
        }
        
        
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var characterSpriteManager = (SpriteExpressionCollection)target;

            if (!characterSpriteManager.defaultSprite) 
            {
                return base.RenderStaticPreview(assetPath, subAssets, width, height);
            }

            var firstSprite = characterSpriteManager.defaultSprite.texture;
            var previewIcon = new Texture2D(width, height);
            EditorUtility.CopySerialized(firstSprite, previewIcon);
            return previewIcon;
        }
    }
}