using DG.Tweening;
using TMPro;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;

namespace UCT.Global.Scene
{
    /// <summary>
    /// 最初始场景（模板信息）的控制器
    /// </summary>
    public class StartController : MonoBehaviour
    {
        private TextMeshPro _textUnder, _text;
        private bool _textUnderOpen;
        private float _time;
        private GameObject _title;
        private int _layer;

        private float _afkTimer = 20;

        private void Start()
        {
            _title = transform.Find("Title").gameObject;
            _textUnder = transform.Find("Title/Text Message").GetComponent<TextMeshPro>();
            _text = transform.Find("SafeText").GetComponent<TextMeshPro>();
            _text.color = Color.clear;
            _textUnder.color = Color.clear;
            AudioController.Instance.GetFx(11, MainControl.Instance.AudioControl.fxClipUI);
            _text.text = MainControl.Instance.OverworldControl.sceneTextsAsset;

            var playerControl = SaveController.LoadData("Data" + MainControl.Instance.dataNumber);
        }

        private void Update()
        {
            if (_time < 5)
                _time += Time.deltaTime;
            else if (!_textUnderOpen)
            {
                _textUnderOpen = true;
                _textUnder.DOColor(Color.white, 0.5f).SetEase(Ease.Linear);
            }

            if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
            {
                switch (_layer)
                {
                    case 0:
                        _layer = -1;
                        _title.transform.DOLocalMoveY(-12, 2).SetEase(Ease.InOutSine).OnKill(TextAnim);
                        break;

                    case 1:

                        _text.DOColor(Color.clear, 1).SetEase(Ease.Linear);
                        if (MainControl.Instance.PlayerControl.playerName == "" || MainControl.Instance.PlayerControl.playerName == null)
                        {
                            GameUtilityService.FadeOutAndSwitchScene("Rename", Color.black, false, 2f);
                        }
                        else
                        {
                            GameUtilityService.FadeOutAndSwitchScene("Menu", Color.black, false, 2f);
                        }
                        break;
                }
            }
            if (Input.anyKeyDown)
            {
                _afkTimer = 20;
                return;
            }
            if (_afkTimer > 0)
                _afkTimer -= Time.deltaTime;
            else
            {
                GameUtilityService.FadeOutAndSwitchScene("Story", Color.black);
                _afkTimer = 10000000000;
            }
        }

        private void TextAnim()
        {
            _text.DOColor(Color.white, 1).SetEase(Ease.Linear).OnKill(() => ChangeLayer(1));
        }

        private void ChangeLayer(int lay)
        {
            _layer = lay;
        }
    }
}