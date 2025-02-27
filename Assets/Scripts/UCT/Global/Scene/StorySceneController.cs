using System.Collections.Generic;
using DG.Tweening;
using Plugins.Timer.Source;
using TMPro;
using UCT.Extensions;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;

namespace UCT.Global.Scene
{
    /// <summary>
    ///     ¿ØÖÆ½²¹ÊÊÂ³¡¾°£¨²¥PPT£©
    /// </summary>
    public class StorySceneController : MonoBehaviour
    {
        public List<Sprite> pics;
        public GameObject mask;
        private int _picNumber;
        private SpriteRenderer _spriteRenderer;
        private TextMeshPro _tmp;
        private TypeWritter _typeWritter;
        public static StorySceneController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _typeWritter = GetComponent<TypeWritter>();
            _spriteRenderer = transform.Find("Pic").GetComponent<SpriteRenderer>();
            _tmp = transform.Find("Text").GetComponent<TextMeshPro>();
            mask = GameObject.Find("MaskCanvas").gameObject;

            _typeWritter.StartTypeWritter(
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.sceneTexts,
                    "Text"), _tmp);
            _typeWritter.typeMode = TypeWritter.TypeMode.IgnorePlayerInput;
        }

        private void Update()
        {
            if (GameUtilityService.IsGamePausedOrSetting())
            {
                return;
            }

            if (!InputService.GetKeyDown(KeyCode.Z))
            {
                return;
            }

            _typeWritter.TypeStop();
            _tmp.text = "";
            GameUtilityService.FadeOutAndSwitchScene("Start", Color.black);
        }

        public void Fade(int number)
        {
            _picNumber = number;
            _spriteRenderer.DOColor(ColorEx.WhiteClear, 0.5f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo);
            Timer.Register(0.5f, ChangePic);
        }

        private void ChangePic()
        {
            _spriteRenderer.sprite = pics[_picNumber];
        }
    }
}