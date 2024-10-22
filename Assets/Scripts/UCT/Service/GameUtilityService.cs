using System;
using System.Collections.Generic;
using DG.Tweening;
using UCT.Battle;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Overworld;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UCT.Service
{
    /// <summary>
    /// 提供一些较为全局的，与游戏内容相关的函数。
    /// </summary>
    public static class GameUtilityService
    {
        /// <summary>
        /// 设置16:9边框的Sprite
        /// </summary>
        /// <param name="framePic"></param>
        public static void SetCanvasFrameSprite(int framePic = 2)//一般为CanvasController.instance.framePic
        {
            var frame = CanvasController.Instance.frame;
            frame.sprite = framePic < 0 ? null : MainControl.Instance.OverworldControl.frames[framePic];
        }

        /// <summary>
        /// 分辨率设置
        /// </summary>
        public static void SetResolution(int resolution)
        {
            if (!MainControl.Instance.cameraMainInBattle)
            {
                if (!MainControl.Instance.cameraShake)
                    MainControl.Instance.cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
                else
                    MainControl.Instance.cameraMainInBattle = MainControl.Instance.cameraShake.GetComponent<Camera>();
            }

            if (!MainControl.Instance.OverworldControl.hdResolution)
            {
                if (MainControl.Instance.OverworldControl.resolutionLevel > 4)
                    MainControl.Instance.OverworldControl.resolutionLevel = 0;
            }
            else
            {
                if (MainControl.Instance.OverworldControl.resolutionLevel < 5)
                    MainControl.Instance.OverworldControl.resolutionLevel = 5;
            }

            if (!MainControl.Instance.OverworldControl.hdResolution)
            {
                MainControl.Instance.mainCamera.rect = new Rect(0, 0, 1, 1);
                if (MainControl.Instance.sceneState == MainControl.SceneState.InBattle)
                {
                    if (MainControl.Instance.cameraMainInBattle) MainControl.Instance.cameraMainInBattle.rect = new Rect(0, 0, 1, 1);
                }
                
                if (BackpackBehaviour.Instance)
                    BackpackBehaviour.Instance.SuitResolution();

                CanvasController.Instance.DOKill();
                CanvasController.Instance.fps.rectTransform.anchoredPosition = new Vector2();
                CanvasController.Instance.frame.color = new Color(1, 1, 1, 0);
                CanvasController.Instance.setting.transform.localScale = Vector3.one;
                CanvasController.Instance.setting.rectTransform.anchoredPosition = new Vector2(0, CanvasController.Instance.setting.rectTransform.anchoredPosition.y);
            }
            else
            {
                if (MainControl.Instance.mainCamera)
                    MainControl.Instance.mainCamera.rect = new Rect(0, 0.056f, 1, 0.888f);
                
                if (MainControl.Instance.sceneState == MainControl.SceneState.InBattle)
                    if (MainControl.Instance.cameraMainInBattle)
                        MainControl.Instance.cameraMainInBattle.rect = new Rect(0, 0.056f, 1, 0.888f);

                if (BackpackBehaviour.Instance)
                    BackpackBehaviour.Instance.SuitResolution();

                CanvasController.Instance.DOKill();

                if (CanvasController.Instance.framePic < 0)
                {
                    CanvasController.Instance.frame.color = Color.black;
                    CanvasController.Instance.fps.rectTransform.anchoredPosition = new Vector2(0, -30f);
                }
                else 
                    CanvasController.Instance.fps.rectTransform.anchoredPosition = new Vector2();

                CanvasController.Instance.frame.DOColor(new Color(1, 1, 1, 1) * Convert.ToInt32(CanvasController.Instance.framePic >= 0), 1f);
                CanvasController.Instance.setting.transform.localScale = Vector3.one * 0.89f;
                CanvasController.Instance.setting.rectTransform.anchoredPosition = new Vector2(142.5f, CanvasController.Instance.setting.rectTransform.anchoredPosition.y);
            }

            
            var resolutionHeights = new List<int> { 480, 768, 864, 960, 1080, 540, 1080 };
            var resolutionWidths = MathUtilityService.GetResolutionWidthsWithHeights(resolutionHeights, 5);

            var currentResolutionWidth = resolutionWidths[resolution];
            var currentResolutionHeight = resolutionHeights[resolution];
            
            Screen.SetResolution(currentResolutionWidth, currentResolutionHeight, MainControl.Instance.OverworldControl.fullScreen);
            MainControl.Instance.OverworldControl.resolution = new Vector2(currentResolutionWidth, currentResolutionHeight);
        }

        public static void SwitchScene(string sceneName, bool async = true)
        {
            SetCanvasFrameSprite();
            if (SceneManager.GetActiveScene().name != "Menu" && SceneManager.GetActiveScene().name != "Rename" && SceneManager.GetActiveScene().name != "Story" && SceneManager.GetActiveScene().name != "Start" && SceneManager.GetActiveScene().name != "Gameover")
                MainControl.Instance.PlayerControl.lastScene = SceneManager.GetActiveScene().name;
            if (async)
                SceneManager.LoadSceneAsync(sceneName);
            else SceneManager.LoadScene(sceneName);

            SetResolution(MainControl.Instance.OverworldControl.resolutionLevel);
            MainControl.Instance.isSceneSwitching = false;
        }

        public static void FadeOutToWhiteAndSwitchScene(string scene)
        {
            MainControl.Instance.isSceneSwitching = true;
            MainControl.Instance.inOutBlack.color = new Color(1, 1, 1, 0);
            AudioController.Instance.GetFx(6, MainControl.Instance.AudioControl.fxClipUI);
            MainControl.Instance.inOutBlack.DOColor(Color.white, 5.5f).SetEase(Ease.Linear).OnKill(() => SwitchScene(scene));
        }

        /// <summary>
        /// 淡出并切换场景。
        /// </summary>
        /// <param name="scene">要切换到的场景名称</param>
        /// <param name="fadeColor">淡出的颜色</param>
        /// <param name="isBgmMuted">是否静音背景音乐</param>
        /// <param name="fadeTime">淡出时间，默认为0.5秒</param>
        /// <param name="isAsync">是否异步切换场景，默认为true</param>
        public static void FadeOutAndSwitchScene(string scene, Color fadeColor, bool isBgmMuted = false, float fadeTime = 0.5f, bool isAsync = true)
        {
            MainControl.Instance.isSceneSwitching = true;
            if (isBgmMuted)
            {
                var bgm = AudioController.Instance.transform.GetComponent<AudioSource>();
                switch (fadeTime)
                {
                    case > 0:
                        DOTween.To(() => bgm.volume, x => bgm.volume = x, 0, fadeTime).SetEase(Ease.Linear);
                        break;
                    case 0:
                        bgm.volume = 0;
                        break;
                    default:
                        DOTween.To(() => bgm.volume, x => bgm.volume = x, 0, Mathf.Abs(fadeTime)).SetEase(Ease.Linear);
                        break;
                }
            }
            MainControl.Instance.OverworldControl.pause = true;
            switch (fadeTime)
            {
                case > 0:
                {
                    MainControl.Instance.inOutBlack.DOColor(fadeColor, fadeTime).SetEase(Ease.Linear).OnKill(() => GameUtilityService.SwitchScene(scene));
                    if (!MainControl.Instance.OverworldControl.hdResolution)
                        CanvasController.Instance.frame.color = new Color(1, 1, 1, 0);
                    break;
                }
                case 0:
                    MainControl.Instance.inOutBlack.color = fadeColor;
                    GameUtilityService.SwitchScene(scene, isAsync);
                    break;
                default:
                {
                    fadeTime = Mathf.Abs(fadeTime);
                    MainControl.Instance.inOutBlack.color = fadeColor;
                    MainControl.Instance.inOutBlack.DOColor(fadeColor, fadeTime).SetEase(Ease.Linear).OnKill(() => GameUtilityService.SwitchScene(scene));
                    if (!MainControl.Instance.OverworldControl.hdResolution)
                        CanvasController.Instance.frame.color = new Color(1, 1, 1, 0);
                    break;
                }
            }
        }

        /// <summary>
        /// 更改分辨率
        /// </summary>
        public static void UpdateResolutionSettings()
        {
            if (!MainControl.Instance.OverworldControl.hdResolution)
            {
                if (MainControl.Instance.OverworldControl.resolutionLevel is >= 0 and < 4)
                    MainControl.Instance.OverworldControl.resolutionLevel += 1;
                else
                    MainControl.Instance.OverworldControl.resolutionLevel = 0;
            }
            else
            {
                if (MainControl.Instance.OverworldControl.resolutionLevel is >= 5 and < 6)
                    MainControl.Instance.OverworldControl.resolutionLevel += 1;
                else
                    MainControl.Instance.OverworldControl.resolutionLevel = 5;
            }

            SetResolution(MainControl.Instance.OverworldControl.resolutionLevel);
        }
    }
}