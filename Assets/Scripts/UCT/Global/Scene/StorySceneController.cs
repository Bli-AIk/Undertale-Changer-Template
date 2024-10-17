using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UCT.Global.Core;
using UCT.Global.UI;
using UnityEngine;

namespace UCT.Global.Scene
{
    /// <summary>
    /// ¿ØÖÆ½²¹ÊÊÂ³¡¾°£¨²¥PPT£©
    /// </summary>
    public class StorySceneController : MonoBehaviour
    {
        public static StorySceneController instance;
        public List<Sprite> pics;
        private SpriteRenderer spriteRenderer;
        private TypeWritter typeWritter;
        private TextMeshPro tmp;
        public GameObject mask;
        private int picNumber;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            typeWritter = GetComponent<TypeWritter>();
            spriteRenderer = transform.Find("Pic").GetComponent<SpriteRenderer>();
            tmp = transform.Find("Text").GetComponent<TextMeshPro>();
            mask = transform.Find("Mask").gameObject;

            typeWritter.TypeOpen(MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "Text"), false, 0, 1, tmp, TypeWritter.TypeMode.CantZX);
        }

        private void Update()
        {
            if (MainControl.Instance.OverworldControl.isSetting || MainControl.Instance.OverworldControl.pause)
                return;
            if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
            {
                typeWritter.TypeStop();
                tmp.text = "";
                MainControl.Instance.OutBlack("Start", Color.black);
            }
        }

        public void Fade(int number)
        {
            picNumber = number;
            spriteRenderer.DOColor(new Color(1, 1, 1, 0), 0.5f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo);
            Invoke(nameof(ChangePic), 0.5f);
        }

        private void ChangePic()
        {
            spriteRenderer.sprite = pics[picNumber];
        }
    }
}