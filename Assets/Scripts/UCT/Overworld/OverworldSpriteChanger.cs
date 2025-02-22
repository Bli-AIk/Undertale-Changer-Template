using System.Collections.Generic;
using Alchemy.Inspector;
using UCT.Global.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UCT.Overworld
{
    /// <summary>
    ///     管理Overworld对话系统中的精灵显示逻辑
    /// </summary>
    [RequireComponent(typeof(TypeWritter))]
    public class OverworldSpriteChanger : MonoBehaviour
    {
        private const float SecondsPerFrame = 1f / 60f;

        [Title("Component Settings")] [SerializeField]
        private string spritePath = "BackpackCamera/TalkBox/HeadSculpture";

        [SerializeField] private bool hasSecondSprite;

        [Title("Animation Settings")] [SerializeField]
        private float frameInterval = 10f;


        public bool isIgnoreConditions;

        [Title("Sprite Resources")] [SerializeField]
        private List<Sprite> sprites;

        [FormerlySerializedAs("spritesSayBack")]
        [SerializeField] private List<Sprite> spritesSecond;

        private float _currentFrameTime;
        private int _currentSpriteIndex = -1;
        public bool isBackState;

        private SpriteRenderer _spriteRenderer;
        private TypeWritter _typeWritter;

        private void Start()
        {
            InitializeComponents();
            FindSpriteComponent();
        }

        private void Update()
        {
            if (MainControl.Instance.overworldControl.isSetting)
            {
                return;
            }

            UpdateClock();

            if (!ShouldUpdateSprite())
            {
                return;
            }

            HandleSpriteChange();
            ResetClock();
        }

        private void InitializeComponents()
        {
            _currentFrameTime = frameInterval * SecondsPerFrame;
            _typeWritter = GetComponent<TypeWritter>();
        }

        private void FindSpriteComponent()
        {
            var targetTransform = string.IsNullOrEmpty(spritePath)
                ? transform
                : transform.Find(spritePath);

            _spriteRenderer = targetTransform.GetComponent<SpriteRenderer>();
        }

        private void UpdateClock()
        {
            _currentFrameTime -= Time.deltaTime;
        }

        private bool ShouldUpdateSprite()
        {
            if (isIgnoreConditions)
            {
                return true;
            }

            if (!hasSecondSprite || !_typeWritter.isTyping || _typeWritter.passText)
            {
                return false;
            }

            return _currentFrameTime <= 0;
        }

        public void HandleSpriteChange()
        {
            if (!isIgnoreConditions)
            {
                isBackState = !isBackState && _currentSpriteIndex >= 0;
            }

            UpdateSpriteDisplay(_currentSpriteIndex);
        }

        private void ResetClock()
        {
            _currentFrameTime = frameInterval * SecondsPerFrame;
        }

        /// <summary>
        ///     更新当前显示的精灵
        /// </summary>
        /// <param name="spriteIndex">目标精灵索引</param>
        public void UpdateSpriteDisplay(int spriteIndex)
        {
            TalkBoxController.Instance.haveHead = spriteIndex >= 0;
            _currentSpriteIndex = spriteIndex;

            if (spriteIndex < 0)
            {
                ClearSprite();
                return;
            }

            SetSprite(isBackState
                ? spritesSecond[spriteIndex]
                : sprites[spriteIndex]);
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