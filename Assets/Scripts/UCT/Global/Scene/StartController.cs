using System;
using System.Linq;
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
        private TextMeshPro _textMessage, _textNotice;
        private bool _textUnderOpen;
        private float _time;
        private GameObject _title;
        private int _layer;

        private float _afkTimer = 20;

        private void Start()
        {
            _title = transform.Find("Title").gameObject;
            _textMessage = transform.Find("Title/TextMessage").GetComponent<TextMeshPro>();
            _textNotice = transform.Find("TextNotice").GetComponent<TextMeshPro>();
            _textNotice.color = Color.clear;
            _textMessage.color = Color.clear;
            AudioController.Instance.GetFx(11, MainControl.Instance.AudioControl.fxClipUI);
            
            var text = MainControl.Instance.overworldControl.sceneTextsAsset;
            var lines = text.Split(new[] { '\n' }, StringSplitOptions.None);
            var messageText = lines.Last(); // 获取最后一行文本
            var noticeText = string.Join("\n", lines.Take(lines.Length - 1)); // 获取除最后一行外的所有文本
            _textNotice.text = noticeText;
            _textMessage.text = messageText;

            SaveController.LoadData("Data" + MainControl.Instance.saveDataId);
        }

        private void Update()
        {
            if (_time < 5)
                _time += Time.deltaTime;
            else if (!_textUnderOpen)
            {
                _textUnderOpen = true;
                _textMessage.DOColor(Color.white, 0.5f).SetEase(Ease.Linear);
            }

            if (GameUtilityService.ConvertKeyDownToControl(KeyCode.Z))
            {
                switch (_layer)
                {
                    case 0:
                        _layer = -1;
                        _title.transform.DOLocalMoveY(-12, 2).SetEase(Ease.InOutSine).OnKill(TextAnim);
                        break;

                    case 1:

                        _textNotice.DOColor(Color.clear, 1).SetEase(Ease.Linear);
                        GameUtilityService.FadeOutAndSwitchScene(
                            string.IsNullOrEmpty(MainControl.Instance.playerControl.playerName) ? "Rename" : "Menu",
                            Color.black, false, 2f);
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
            _textNotice.DOColor(Color.white, 1).SetEase(Ease.Linear).OnKill(() => ChangeLayer(1));
        }

        private void ChangeLayer(int lay)
        {
            _layer = lay;
        }
    }
}