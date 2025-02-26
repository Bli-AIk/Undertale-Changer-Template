using System;
using System.Collections.Generic;
using DG.Tweening;
using UCT.Battle;
using UCT.Extensions;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.Settings;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace UCT.Service
{
    /// <summary>
    ///     提供一些较为全局的，与游戏内容相关的函数。
    /// </summary>
    public static class GameUtilityService
    {
        private static readonly Random Rng = new();

        private static readonly Random ColorGenerator = new(); // 线程安全的实例

        /// <summary>
        ///     设置16:9边框的Sprite
        /// </summary>
        /// <param name="framePic"></param>
        public static void SetCanvasFrameSprite(int framePic = 2) //一般为CanvasController.instance.framePic
        {
            var frame = SettingsController.Instance.Frame;
            frame.sprite = framePic < 0 ? null : MainControl.Instance.overworldControl.frames[framePic];
        }

        /// <summary>
        ///     分辨率设置
        /// </summary>
        public static void SetResolution(int resolution)
        {
            if (!MainControl.Instance.cameraMainInBattle)
            {
                if (!MainControl.Instance.cameraShake)
                {
                    var mainCamera = Camera.main;
                    if (mainCamera)
                    {
                        MainControl.Instance.cameraShake = mainCamera.GetComponent<CameraShake>();
                    }
                }
                else
                {
                    MainControl.Instance.cameraMainInBattle = MainControl.Instance.cameraShake.GetComponent<Camera>();
                }
            }

            if (!SettingsStorage.isUsingHdFrame)
            {
                if (SettingsStorage.resolutionLevel > 4)
                {
                    SettingsStorage.resolutionLevel = 0;
                }

                if (MainControl.Instance.mainCamera)
                {
                    MainControl.Instance.mainCamera.rect = new Rect(0, 0, 1, 1);
                }

                if (MainControl.Instance.sceneState == MainControl.SceneState.InBattle)
                {
                    if (MainControl.Instance.cameraMainInBattle)
                    {
                        MainControl.Instance.cameraMainInBattle.rect = new Rect(0, 0, 1, 1);
                    }
                }

                SettingsController.Instance.DOKill();
                SettingsController.Instance.Frame.color = ColorEx.WhiteClear;
            }
            else
            {
                if (SettingsStorage.resolutionLevel < 5)
                {
                    SettingsStorage.resolutionLevel = 5;
                }

                if (MainControl.Instance.mainCamera)
                {
                    MainControl.Instance.mainCamera.rect = new Rect(0, 0.056f, 1, 0.888f);
                }

                if (MainControl.Instance.sceneState == MainControl.SceneState.InBattle)
                {
                    if (MainControl.Instance.cameraMainInBattle)
                    {
                        MainControl.Instance.cameraMainInBattle.rect = new Rect(0, 0.056f, 1, 0.888f);
                    }
                }

                SettingsController.Instance.DOKill();

                if (SettingsController.Instance.frameSpriteIndex < 0)
                {
                    SettingsController.Instance.Frame.color = Color.black;
                }

                SettingsController.Instance.Frame.DOColor(
                    Color.white * Convert.ToInt32(SettingsController.Instance.frameSpriteIndex >= 0), 1f);
            }

            var resolutionHeights = new List<int> { 480, 768, 864, 960, 1080, 540, 1080 };
            var resolutionWidths = MathUtilityService.GetResolutionWidthsWithHeights(resolutionHeights, 5);

            var currentResolutionWidth = resolutionWidths[resolution];
            var currentResolutionHeight = resolutionHeights[resolution];

            Screen.SetResolution(currentResolutionWidth, currentResolutionHeight,
                SettingsStorage.fullScreen);
            SettingsStorage.resolution =
                new Vector2(currentResolutionWidth, currentResolutionHeight);
        }

        /// <summary>
        ///     切换场景。
        ///     优先在此项目中使用这个封装后的方法进行场景切换操作。
        /// </summary>
        public static void SwitchScene(string sceneName, bool isAsync = true)
        {
            SetCanvasFrameSprite();
            if (SceneManager.GetActiveScene().name != "Menu" && SceneManager.GetActiveScene().name != "Rename" &&
                SceneManager.GetActiveScene().name != "Story" && SceneManager.GetActiveScene().name != "Start" &&
                SceneManager.GetActiveScene().name != "GameOver")
            {
                MainControl.Instance.playerControl.lastScene = SceneManager.GetActiveScene().name;
            }

            if (isAsync)
            {
                SceneManager.LoadSceneAsync(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }

            SetResolution(SettingsStorage.resolutionLevel);
            MainControl.Instance.isSceneSwitching = false;
        }

        public static void FadeOutToWhiteAndSwitchScene(string scene)
        {
            MainControl.Instance.isSceneSwitching = true;
            MainControl.Instance.sceneSwitchingFadeImage.color = ColorEx.WhiteClear;
            AudioController.Instance.PlayFx(6, MainControl.Instance.AudioControl.fxClipUI);
            MainControl.Instance.sceneSwitchingFadeImage.DOColor(Color.white, 5.5f).SetEase(Ease.Linear)
                .OnKill(() => SwitchScene(scene));
        }

        /// <summary>
        ///     淡出并切换场景。
        /// </summary>
        /// <param name="scene">要切换到的场景名称</param>
        /// <param name="fadeColor">淡出的颜色</param>
        /// <param name="action"></param>
        /// <param name="isBgmMuted">是否静音背景音乐</param>
        /// <param name="fadeTime">淡出时间，默认为0.5秒</param>
        /// <param name="isAsync">是否异步切换场景，默认为true</param>
        public static void FadeOutAndSwitchScene(string scene,
            Color fadeColor,
            Action action = null,
            bool isBgmMuted = false,
            float fadeTime = 0.5f,
            bool isAsync = true)
        {
            action += () => SwitchScene(scene, isAsync);
            MainControl.Instance.isSceneSwitching = true;
            if (isBgmMuted)
            {
                var bgm = AudioController.Instance.audioSource;
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

            SettingsStorage.pause = true;
            switch (fadeTime)
            {
                case > 0:
                {
                    MainControl.Instance.sceneSwitchingFadeImage.DOColor(fadeColor, fadeTime).SetEase(Ease.Linear)
                        .OnKill(() => action.Invoke());
                    if (!SettingsStorage.isUsingHdFrame)
                    {
                        SettingsController.Instance.Frame.color = ColorEx.WhiteClear;
                    }

                    break;
                }
                case 0:
                    MainControl.Instance.sceneSwitchingFadeImage.color = fadeColor;
                    action.Invoke();
                    break;
                default:
                {
                    fadeTime = Mathf.Abs(fadeTime);
                    MainControl.Instance.sceneSwitchingFadeImage.color = fadeColor;
                    MainControl.Instance.sceneSwitchingFadeImage.DOColor(fadeColor, fadeTime).SetEase(Ease.Linear)
                        .OnKill(() => action.Invoke());
                    if (!SettingsStorage.isUsingHdFrame)
                    {
                        SettingsController.Instance.Frame.color = ColorEx.WhiteClear;
                    }

                    break;
                }
            }
        }

        /// <summary>
        ///     更新游戏的分辨率并返回更改后的resolutionLevel值。
        /// </summary>
        public static int UpdateResolutionSettings(bool isUsingHdFrame, int resolutionLevel)
        {
            if (!isUsingHdFrame)
            {
                if (resolutionLevel is >= 0 and < 4)
                {
                    resolutionLevel += 1;
                }
                else
                {
                    resolutionLevel = 0;
                }
            }
            else
            {
                if (resolutionLevel is >= 5 and < 6)
                {
                    resolutionLevel += 1;
                }
                else
                {
                    resolutionLevel = 5;
                }
            }

            SetResolution(resolutionLevel);

            return resolutionLevel;
        }

        /// <summary>
        ///     开/关 SFX
        /// </summary>
        public static void ToggleAllSfx(bool isClose)
        {
            foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(Light2D)))
            {
                var light2D = (Light2D)obj;
                light2D.enabled = !isClose;
            }

            if (MainControl.Instance.mainCamera)
            {
                MainControl.Instance.mainCamera.GetUniversalAdditionalCameraData().renderPostProcessing = !isClose;
            }

            if (MainControl.Instance.sceneState != MainControl.SceneState.InBattle)
            {
                return;
            }

            if (!MainControl.Instance.cameraMainInBattle)
            {
                if (!MainControl.Instance.cameraShake)
                {
                    MainControl.Instance.cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
                }

                MainControl.Instance.cameraMainInBattle = MainControl.Instance.cameraShake.GetComponent<Camera>();
            }

            MainControl.Instance.cameraMainInBattle.GetUniversalAdditionalCameraData().renderPostProcessing = !isClose;
        }

        public static Color GetRandomColor()
        {
            lock (Rng) // 确保线程安全
            {
                return new Color(
                    (float)Rng.NextDouble(),
                    (float)Rng.NextDouble(),
                    (float)Rng.NextDouble(),
                    1f
                );
            }
        }

        public static Color GetDifferentRandomColor(Color colorToAvoid)
        {
            Color newColor;
            do
            {
                newColor = GetRandomColor();
            } while (newColor == colorToAvoid);

            return newColor;
        }

        public static Color GetSimilarButDifferentColor(Color originalColor,
            float hueOffset = 0.05f,
            float saturationOffset = 0.1f,
            float valueOffset = 0.1f)
        {
            Color.RGBToHSV(originalColor, out var h, out var s, out var v);

            lock (ColorGenerator) // 确保线程安全
            {
                h = Mathf.Repeat((float)(h + (ColorGenerator.NextDouble() * 2.0 - 1.0) * hueOffset), 1.0f);
                s = Mathf.Clamp01(s + (float)(ColorGenerator.NextDouble() * 2.0 - 1.0) * saturationOffset);
                v = Mathf.Clamp01(v + (float)(ColorGenerator.NextDouble() * 2.0 - 1.0) * valueOffset);
            }

            return Color.HSVToRGB(h, s, v);
        }

        public static void RefreshTheScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public static bool IsGamePausedOrSetting()
        {
            return MainControl.Instance.overworldControl.isSetting || SettingsStorage.pause;
        }
    }
}