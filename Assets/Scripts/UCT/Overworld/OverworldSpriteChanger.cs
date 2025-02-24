using System;
using Alchemy.Inspector;
using UCT.Control;
using UCT.Global.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Overworld
{
    /// <summary>
    ///     管理Overworld对话系统中的精灵显示逻辑
    /// </summary>
    [RequireComponent(typeof(TypeWritter))]
    public class OverworldSpriteChanger : MonoBehaviour
    {
        private const float SpeakUpdateInterval = 0.2f;
        private const float BlinkUpdateInterval = 0.1f;

        [Title("Component Settings")] [SerializeField]
        private string spritePath = "BackpackCamera/TalkBox/HeadSculpture";


        [Title("Animation Settings")] [SerializeField]
        private float frameInterval = 10f;

        [Title("Sprite Resources")] [SerializeField] [ReadOnly]
        public SpriteExpressionCollection spriteExpressionCollection;

        [ReadOnly] public SpriteExpressionCollection.State state;

        private float _blinkTimer;
        private int _spriteIndex = -1;

        private SpriteRenderer _spriteRenderer;

        private float _spriteUpdateTimer;


        private void Start()
        {
            FindSpriteComponent();
            _blinkTimer = Random.Range(5, 10);
        }

        private void Update()
        {
            if (MainControl.Instance.overworldControl.isSetting)
            {
                return;
            }

            _spriteUpdateTimer += Time.deltaTime;
            switch (state)
            {
                case SpriteExpressionCollection.State.Default:
                {
                    _spriteUpdateTimer = 0f;

                    if (_blinkTimer <= 0)
                    {
                        _blinkTimer = Random.Range(5, 10);
                        state = SpriteExpressionCollection.State.Blinking;
                    }
                    else
                    {
                        _blinkTimer -= Time.deltaTime;
                    }

                    if (!spriteExpressionCollection || !spriteExpressionCollection.defaultSprite)
                    {
                        return;
                    }

                    var sprite = spriteExpressionCollection.defaultSprite;

                    if (_spriteRenderer.sprite != sprite)
                    {
                        SetSprite(sprite);
                    }

                    return;
                }
                case SpriteExpressionCollection.State.Speaking:
                {
                    if (_spriteUpdateTimer < SpeakUpdateInterval)
                    {
                        return;
                    }

                    break;
                }

                case SpriteExpressionCollection.State.Blinking:
                {
                    if (_spriteUpdateTimer < BlinkUpdateInterval)
                    {
                        return;
                    }

                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            _spriteUpdateTimer = 0f;

            UpdateSpriteDisplay();
        }

        private void FindSpriteComponent()
        {
            var targetTransform = string.IsNullOrEmpty(spritePath)
                ? transform
                : transform.Find(spritePath);

            _spriteRenderer = targetTransform.GetComponent<SpriteRenderer>();
        }

        /// <summary>
        ///     更新当前显示的精灵
        /// </summary>
        public void UpdateSpriteDisplay()
        {
            if (!spriteExpressionCollection)
            {
                _spriteIndex = -1;
            }
            else if (_spriteIndex < 0)
            {
                _spriteIndex = 0;
            }

            TalkBoxController.Instance.SetHead(_spriteIndex >= 0);
            if (_spriteIndex < 0)
            {
                ClearSprite();
                return;
            }

            Sprite sprite;

            switch (state)
            {
                case SpriteExpressionCollection.State.Default:
                {
                    sprite = spriteExpressionCollection.defaultSprite;
                    break;
                }
                case SpriteExpressionCollection.State.Speaking:
                {
                    if (spriteExpressionCollection.speakingSprites.Count == 0)
                    {
                        goto case SpriteExpressionCollection.State.Default;
                    }

                    _spriteIndex = Mathf.Clamp(_spriteIndex, 0, spriteExpressionCollection.speakingSprites.Count - 1);
                    sprite = spriteExpressionCollection.speakingSprites[_spriteIndex];
                    _spriteIndex++;
                    if (_spriteIndex >= spriteExpressionCollection.speakingSprites.Count)
                    {
                        _spriteIndex = 0;
                    }

                    break;
                }
                case SpriteExpressionCollection.State.Blinking:
                {
                    if (spriteExpressionCollection.blinkingSprites.Count == 0)
                    {
                        goto case SpriteExpressionCollection.State.Default;
                    }

                    _spriteIndex = Mathf.Clamp(_spriteIndex, 0, spriteExpressionCollection.blinkingSprites.Count - 1);
                    sprite = spriteExpressionCollection.blinkingSprites[_spriteIndex];
                    _spriteIndex++;
                    if (_spriteIndex >= spriteExpressionCollection.blinkingSprites.Count)
                    {
                        _spriteIndex = 0;
                        state = SpriteExpressionCollection.State.Default;
                    }

                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }

            SetSprite(sprite);
        }

        private void SetSprite(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
            _spriteRenderer.color = Color.white;
        }

        private void ClearSprite()
        {
            _spriteRenderer.sprite = null;
            _spriteRenderer.color = Color.clear;
        }
    }
}