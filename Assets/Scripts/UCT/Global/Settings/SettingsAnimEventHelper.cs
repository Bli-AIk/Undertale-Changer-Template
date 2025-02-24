using System;
using DG.Tweening;
using UCT.Extensions;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;
using UnityEngine.UI;

namespace UCT.Global.Settings
{
    /// <summary>
    ///     给设置页面的Animator提供事件脚本。
    /// </summary>
    public class SettingsAnimEventHelper : MonoBehaviour
    {
        private SettingsController _settingsController;
        private void Start()
        {
            _settingsController = GetComponent<SettingsController>();
        }

        public void AnimSetHeartRed(int isRed)
        {
            _settingsController.transform.Find("Heart").GetComponent<Image>().color =
                Convert.ToBoolean(isRed) ? Color.red : ColorEx.WhiteClear;
        }

        public void AnimHeartGo()
        {
            var i = _settingsController.transform.Find("Heart").GetComponent<RectTransform>();
            var j = i.GetComponent<Image>();
            j.DOColor(ColorEx.RedClear, SettingsController.AnimSpeed).SetEase(Ease.Linear);
            DOTween.To(() => i.anchoredPosition, x => i.anchoredPosition = x, new Vector2(-330, -250), 1.5f)
                .SetEase(Ease.OutCirc).OnKill(() =>
                {
                    _settingsController.Animator.SetBool(SettingsController.Open, false);
                    GameUtilityService.FadeOutAndSwitchScene("Battle", Color.black, null, false, -0.5f);
                });
        }

        public void AnimPlayFX(int i)
        {
            AudioController.Instance.PlayFx(i, MainControl.Instance.AudioControl.fxClipUI);
        }
        
        // 供Animator使用
        public void AnimSetHeartPos()
        {
            var uiPos = _settingsController.WorldPositionToUGUI(MainControl.Instance.playerControl.playerLastPos);
            transform.Find("Heart").GetComponent<RectTransform>().anchoredPosition = uiPos;
        }

    }

}