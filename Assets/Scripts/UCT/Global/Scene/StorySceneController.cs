using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;

namespace UCT.Global.Scene
{
    /// <summary>
    /// ¿ØÖÆ½²¹ÊÊÂ³¡¾°£¨²¥PPT£©
    /// </summary>
    public class StorySceneController : MonoBehaviour
    {
        public static StorySceneController Instance;
        public List<Sprite> pics;
        private SpriteRenderer _spriteRenderer;
        private TypeWritter _typeWritter;
        private TextMeshPro _tmp;
        public GameObject mask;
        private int _picNumber;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _typeWritter = GetComponent<TypeWritter>();
            _spriteRenderer = transform.Find("Pic").GetComponent<SpriteRenderer>();
            _tmp = transform.Find("Text").GetComponent<TextMeshPro>();
            mask = transform.Find("Mask").gameObject;

            _typeWritter.TypeOpen(TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Text"), false, 0, 1, _tmp, TypeWritter.TypeMode.CantZx);
        }

        private void Update()
        {
            if (MainControl.Instance.OverworldControl.isSetting || MainControl.Instance.OverworldControl.pause)
                return;
            if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
            {
                _typeWritter.TypeStop();
                _tmp.text = "";
                MainControl.Instance.FadeOutAndSwitchScene("Start", Color.black);
            }
        }

        public void Fade(int number)
        {
            _picNumber = number;
            _spriteRenderer.DOColor(new Color(1, 1, 1, 0), 0.5f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo);
            Invoke(nameof(ChangePic), 0.5f);
        }

        private void ChangePic()
        {
            _spriteRenderer.sprite = pics[_picNumber];
        }
    }
}