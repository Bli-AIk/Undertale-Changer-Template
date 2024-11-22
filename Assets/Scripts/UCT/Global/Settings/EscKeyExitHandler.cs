using System.Collections.Generic;
using DG.Tweening;
using UCT.Extensions;
using UCT.Service;
using UnityEngine;
using UnityEngine.UI;

namespace UCT.Global.Settings
{
    /// <summary>
    /// 处理长按ESC退出游戏的逻辑
    /// </summary>
    public class EscKeyExitHandler : MonoBehaviour
    {
        private List<Sprite> _exitImageSprites;

        private readonly List<Vector2> _exitImageSizes = new()
        {
            new Vector2(82, 14),
            new Vector2(86, 14),
            new Vector2(90, 14)
        };

        private float _exitImageClock;
        private Image _exitImage;
        private Tween _exitImageTween;

        private void Awake()
        {
            _exitImage = GetComponent<Image>();
            _exitImageSprites = new List<Sprite>
            {
                Resources.Load<Sprite>("Sprites/Other/spr_quitting_message_0"),
                Resources.Load<Sprite>("Sprites/Other/spr_quitting_message_1"),
                Resources.Load<Sprite>("Sprites/Other/spr_quitting_message_2")
            };
        }

        private void Update()
        {
            if (InputService.GetKeyDown(KeyCode.Escape))
                _exitImageTween.Kill();
            
            if (InputService.GetKey(KeyCode.Escape))
                UpdateHandleExitInput();

            if (!InputService.GetKeyUp(KeyCode.Escape)) return;
            _exitImageClock = 0;
            _exitImageTween = _exitImage.DOColor(ColorEx.WhiteClear,0.5f);
        }
        
        
        /// <summary>
        /// 处理游戏退出的逻辑
        /// </summary>
        private void UpdateHandleExitInput()
        {
            if (_exitImage.color.a < 1)
                _exitImage.color += Time.deltaTime * Color.white;
            if (_exitImageClock < 3)
            {
                _exitImage.sprite = _exitImageSprites[(int)_exitImageClock];
                _exitImage.rectTransform.sizeDelta = _exitImageSizes[(int)_exitImageClock];
                _exitImageClock += Time.deltaTime;
            }
            else
            {
                Application.Quit();
                Other.Debug.LogWarning("Application.Quit被执行了！");
            }
        }
    }
}