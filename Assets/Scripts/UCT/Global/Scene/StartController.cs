using DG.Tweening;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UnityEngine;

namespace UCT.Global.Scene
{
    /// <summary>
    /// 最初始场景（模板信息）的控制器
    /// </summary>
    public class StartController : MonoBehaviour
    {
        private TextMeshPro textUnder, text;
        private bool textUnderOpen;
        private float time;
        private GameObject title;
        private int layer;

        private float afkTimer = 20;

        private void Start()
        {
            title = transform.Find("Title").gameObject;
            textUnder = transform.Find("Title/Text Message").GetComponent<TextMeshPro>();
            text = transform.Find("SafeText").GetComponent<TextMeshPro>();
            text.color = Color.clear;
            textUnder.color = Color.clear;
            AudioController.instance.GetFx(11, MainControl.Instance.AudioControl.fxClipUI);
            text.text = MainControl.Instance.OverworldControl.sceneTextsAsset;

            PlayerControl playerControl = SaveController.LoadData("Data" + MainControl.Instance.dataNumber);
        }

        private void Update()
        {
            if (time < 5)
                time += Time.deltaTime;
            else if (!textUnderOpen)
            {
                textUnderOpen = true;
                textUnder.DOColor(Color.white, 0.5f).SetEase(Ease.Linear);
            }

            if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
            {
                switch (layer)
                {
                    case 0:
                        layer = -1;
                        title.transform.DOLocalMoveY(-12, 2).SetEase(Ease.InOutSine).OnKill(TextAnim);
                        break;

                    case 1:

                        text.DOColor(Color.clear, 1).SetEase(Ease.Linear);
                        if (MainControl.Instance.PlayerControl.playerName == "" || MainControl.Instance.PlayerControl.playerName == null)
                        {
                            MainControl.Instance.OutBlack("Rename", Color.black, false, 2f);
                        }
                        else
                        {
                            MainControl.Instance.OutBlack("Menu", Color.black, false, 2f);
                        }
                        break;
                }
            }
            if (Input.anyKeyDown)
            {
                afkTimer = 20;
                return;
            }
            if (afkTimer > 0)
                afkTimer -= Time.deltaTime;
            else
            {
                MainControl.Instance.OutBlack("Story", Color.black);
                afkTimer = 10000000000;
            }
        }

        private void TextAnim()
        {
            text.DOColor(Color.white, 1).SetEase(Ease.Linear).OnKill(() => ChangeLayer(1));
        }

        private void ChangeLayer(int lay)
        {
            layer = lay;
        }
    }
}