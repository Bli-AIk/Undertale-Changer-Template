using System;
using System.Collections.Generic;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using UnityEngine.UI;

namespace UCT.Overworld
{
    /// <summary>
    /// 用于在OW插入选项系统，与UT的选项一致，而不同于DR的选项。
    /// 在含有选项的场景内会添加heart。
    /// 在文本包内调用
    /// </summary>
    public class OverworldTalkSelect : MonoBehaviour
    {
        public static OverworldTalkSelect Instance;
        public int select;
        private Image _heart;
        private bool _canSelect;
        private TypeWritter _typeWritter;
        public List<string> texts;
        public string typeText;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (Camera.main != null)
                _typeWritter = Camera.main.GetComponent<TypeWritter>();
            else
                throw new NullReferenceException();
            var heartTrans = BackpackBehaviour.Instance.talkText.transform.parent.Find("Heart");
            var heartObj = !heartTrans
                ? Instantiate(new GameObject(), BackpackBehaviour.Instance.talkText.transform)
                : heartTrans.gameObject;

            _heart = heartObj.AddComponent<Image>() ?? heartObj.GetComponent<Image>();
            _heart.rectTransform.sizeDelta = 16 * Vector3.one;
            _heart.sprite = Resources.Load<Sprite>("Sprites/Soul");
            _heart.color = Color.clear;
        }

        public void Open()
        {
            select = 0;
            _heart.rectTransform.anchoredPosition = new Vector2(-143.3f + Convert.ToInt32(select) * 192.5f, -18.8f);
            _heart.color = Color.red;
            _canSelect = true;
        }

        private void Update()
        {
            if (!_canSelect) return;
            if (GameUtilityService.ConvertKeyDownToControl(KeyCode.LeftArrow))
            {
                if (select > 0)
                    select--;
                else select = 1;
                _heart.rectTransform.anchoredPosition = new Vector2(-143.3f + Convert.ToInt32(select) * 192.5f, -18.8f);
                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
            }
            else if (GameUtilityService.ConvertKeyDownToControl(KeyCode.RightArrow))
            {
                if (select < 1)
                    select++;
                else select = 0;
                _heart.rectTransform.anchoredPosition = new Vector2(-143.3f + Convert.ToInt32(select) * 192.5f, -18.8f);
                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
            }

            if (!GameUtilityService.ConvertKeyDownToControl(KeyCode.Z)) return;
            _typeWritter.TypeStop();
            switch (select)
            {
                case 0://选择了左侧选项
                    switch (typeText)
                    {
                        case "BackMenu":
                            _typeWritter.forceReturn = true;
                            GameUtilityService.FadeOutAndSwitchScene("Menu", Color.black, true, 0f);
                            AudioController.Instance.audioSource.volume = 0;
                            break;

                        case "Select":
                            AudioController.Instance.GetFx(2, MainControl.Instance.AudioControl.fxClipBattle);
                            break;
                    }
                    break;

                case 1://选择了右侧选项
                    break;
            }
            _heart.color = Color.clear;
            _canSelect = false;
        }
    }
}