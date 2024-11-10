using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UCT.Battle;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Overworld;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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
            frame.sprite = framePic < 0 ? null : MainControl.Instance.overworldControl.frames[framePic];
        }

        /// <summary>
        /// 分辨率设置
        /// </summary>
        public static void SetResolution(int resolution)
        {
            if (!MainControl.Instance.cameraMainInBattle)
            {
                if (!MainControl.Instance.cameraShake)
                {
                    var mainCamera = Camera.main;
                    if (mainCamera) MainControl.Instance.cameraShake = mainCamera.GetComponent<CameraShake>();
                }
                else
                    MainControl.Instance.cameraMainInBattle = MainControl.Instance.cameraShake.GetComponent<Camera>();
            }

            if (!MainControl.Instance.overworldControl.isUsingHDFrame)
            {
                if (MainControl.Instance.overworldControl.resolutionLevel > 4)
                    MainControl.Instance.overworldControl.resolutionLevel = 0;
            }
            else
            {
                if (MainControl.Instance.overworldControl.resolutionLevel < 5)
                    MainControl.Instance.overworldControl.resolutionLevel = 5;
            }

            if (!MainControl.Instance.overworldControl.isUsingHDFrame)
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
            }
            else
            {
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
            }

            
            var resolutionHeights = new List<int> { 480, 768, 864, 960, 1080, 540, 1080 };
            var resolutionWidths = MathUtilityService.GetResolutionWidthsWithHeights(resolutionHeights, 5);

            var currentResolutionWidth = resolutionWidths[resolution];
            var currentResolutionHeight = resolutionHeights[resolution];
            
            Screen.SetResolution(currentResolutionWidth, currentResolutionHeight, MainControl.Instance.overworldControl.fullScreen);
            MainControl.Instance.overworldControl.resolution = new Vector2(currentResolutionWidth, currentResolutionHeight);
        }

        public static void SwitchScene(string sceneName, bool async = true)
        {
            SetCanvasFrameSprite();
            if (SceneManager.GetActiveScene().name != "Menu" && SceneManager.GetActiveScene().name != "Rename" && SceneManager.GetActiveScene().name != "Story" && SceneManager.GetActiveScene().name != "Start" && SceneManager.GetActiveScene().name != "Gameover")
                MainControl.Instance.playerControl.lastScene = SceneManager.GetActiveScene().name;
            if (async)
                SceneManager.LoadSceneAsync(sceneName);
            else SceneManager.LoadScene(sceneName);

            SetResolution(MainControl.Instance.overworldControl.resolutionLevel);
            MainControl.Instance.isSceneSwitching = false;
        }

        public static void FadeOutToWhiteAndSwitchScene(string scene)
        {
            MainControl.Instance.isSceneSwitching = true;
            MainControl.Instance.sceneSwitchingFadeImage.color = new Color(1, 1, 1, 0);
            AudioController.Instance.GetFx(6, MainControl.Instance.AudioControl.fxClipUI);
            MainControl.Instance.sceneSwitchingFadeImage.DOColor(Color.white, 5.5f).SetEase(Ease.Linear).OnKill(() => SwitchScene(scene));
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
            MainControl.Instance.overworldControl.pause = true;
            switch (fadeTime)
            {
                case > 0:
                {
                    MainControl.Instance.sceneSwitchingFadeImage.DOColor(fadeColor, fadeTime).SetEase(Ease.Linear).OnKill(() => GameUtilityService.SwitchScene(scene));
                    if (!MainControl.Instance.overworldControl.isUsingHDFrame)
                        CanvasController.Instance.frame.color = new Color(1, 1, 1, 0);
                    break;
                }
                case 0:
                    MainControl.Instance.sceneSwitchingFadeImage.color = fadeColor;
                    GameUtilityService.SwitchScene(scene, isAsync);
                    break;
                default:
                {
                    fadeTime = Mathf.Abs(fadeTime);
                    MainControl.Instance.sceneSwitchingFadeImage.color = fadeColor;
                    MainControl.Instance.sceneSwitchingFadeImage.DOColor(fadeColor, fadeTime).SetEase(Ease.Linear).OnKill(() => GameUtilityService.SwitchScene(scene));
                    if (!MainControl.Instance.overworldControl.isUsingHDFrame)
                        CanvasController.Instance.frame.color = new Color(1, 1, 1, 0);
                    break;
                }
            }
        }

        /// <summary>
        /// 更新游戏的分辨率并返回更改后的resolutionLevel值。
        /// </summary>
        public static int UpdateResolutionSettings(bool isUsingHdFrame, int resolutionLevel)
        {
            if (!isUsingHdFrame)
            {
                if (resolutionLevel is >= 0 and < 4)
                    resolutionLevel += 1;
                else
                    resolutionLevel = 0;
            }
            else
            {
                if (resolutionLevel is >= 5 and < 6)
                    resolutionLevel += 1;
                else
                    resolutionLevel = 5;
            }

            SetResolution(resolutionLevel);

            return resolutionLevel;
        }

        /// <summary>
        /// 开/关 SFX
        /// </summary>
        public static void ToggleAllSfx(bool isClose)
        {
            foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(Light2D)))
            {
                var light2D = (Light2D)obj;
                light2D.enabled = !isClose;
            }

            if (MainControl.Instance.mainCamera) 
                MainControl.Instance.mainCamera.GetUniversalAdditionalCameraData().renderPostProcessing = !isClose;

            if (MainControl.Instance.sceneState != MainControl.SceneState.InBattle) return;
            
            if (!MainControl.Instance.cameraMainInBattle)
            {
                if (!MainControl.Instance.cameraShake)
                    MainControl.Instance.cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
                MainControl.Instance.cameraMainInBattle = MainControl.Instance.cameraShake.GetComponent<Camera>();
            }
            MainControl.Instance.  cameraMainInBattle.GetUniversalAdditionalCameraData().renderPostProcessing = !isClose;
        }

        /// <summary>
        /// 传入使用背包的哪个物体
        /// 然后就使用 打true会顺带把背包顺序整理下
        /// 然后再让打字机打个字
        /// plusText填0就自己计算
        /// </summary>
        public static void UseItem(TypeWritter typeWritter, TMP_Text tmpText, int sonSelect, int plusText = 0)
        {
            if (plusText == 0)
            {
                plusText = MainControl.Instance.playerControl.myItems[sonSelect - 1] switch
                {
                    >= 20000 => -20000 + MainControl.Instance.ItemControl.itemFoods.Count / 3 + MainControl.Instance.ItemControl.itemArms.Count / 2,
                    >= 10000 => -10000 + MainControl.Instance.ItemControl.itemFoods.Count / 3,
                    _ => 0
                };
            }

            switch (MainControl.Instance.playerControl.myItems[sonSelect - 1])
            {
                case >= 20000:// 防具
                    ProcessArmor(typeWritter, tmpText, sonSelect, plusText);
                    break;
                case >= 10000:// 武器
                    ProcessArm(typeWritter, tmpText, sonSelect, plusText);
                    break;
                // 食物
                default:
                {
                    ProcessFood(typeWritter, tmpText, sonSelect);
                    break;
                }
            }
        }

        private static void ProcessFood(TypeWritter typeWritter, TMP_Text tmpText, int sonSelect)
        {
            var plusHp = int.Parse(DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[sonSelect - 1], "Auto", 2));
            //if (MainControl.Instance.playerControl.wearArm == 10001)
            //    plusHp += 4;

            typeWritter.TypeOpen(
                MainControl.Instance.ItemControl.itemTextMaxItemSon[
                    MainControl.Instance.playerControl.myItems[sonSelect - 1] * 5 - 3], false,
                plusHp, 0, tmpText);

            MainControl.Instance. playerControl.hp += plusHp;

            if (MainControl.Instance.playerControl.hp > MainControl.Instance.playerControl.hpMax)
                MainControl.Instance.playerControl.hp = MainControl.Instance.playerControl.hpMax;
            for (var i = 0; i < MainControl.Instance.ItemControl.itemFoods.Count; i++)
            {
                if (MainControl.Instance.ItemControl.itemTextMaxItemSon[MainControl.Instance.playerControl.myItems[sonSelect - 1] * 5 - 5] !=
                    MainControl.Instance.ItemControl.itemFoods[i]) continue;
                var text = MainControl.Instance.ItemControl.itemFoods[i + 1];
                text = text.Substring(1, text.Length - 1);
                MainControl.Instance. playerControl.myItems[sonSelect - 1] = DataHandlerService.ItemNameGetId(MainControl.Instance.ItemControl, text, "Foods");
                break;
            }
            AudioController.Instance.GetFx(2, MainControl.Instance.AudioControl.fxClipUI);
        }

        private static void ProcessArm(TypeWritter typeWritter, TMP_Text tmpText, int sonSelect, int plusText)
        {
            typeWritter.TypeOpen(MainControl.Instance.ItemControl.itemTextMaxItemSon[(MainControl.Instance.playerControl.myItems[sonSelect - 1] + plusText) * 5 - 3], false, 0, 0, tmpText);
            MainControl.Instance.playerControl.wearAtk = int.Parse(DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[sonSelect - 1], "Auto", 1));
            (MainControl.Instance.playerControl.wearArm, MainControl.Instance.playerControl.myItems[sonSelect - 1]) = (MainControl.Instance.playerControl.myItems[sonSelect - 1],MainControl.Instance. playerControl.wearArm);

            AudioController.Instance.GetFx(3, MainControl.Instance.AudioControl.fxClipUI);
        }

        private static void ProcessArmor(TypeWritter typeWritter, TMP_Text tmpText, int sonSelect, int plusText)
        {
            typeWritter.TypeOpen(MainControl.Instance.ItemControl.itemTextMaxItemSon[(MainControl.Instance.playerControl.myItems[sonSelect - 1] + plusText) * 5 - 3], false, 0, 0, tmpText);
            MainControl.Instance.playerControl.wearDef = int.Parse(DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[sonSelect - 1], "Auto", 1));
            (MainControl.Instance.playerControl.wearArmor, MainControl.Instance.playerControl.myItems[sonSelect - 1]) = (MainControl.Instance.playerControl.myItems[sonSelect - 1], MainControl.Instance.playerControl.wearArmor);

            AudioController.Instance.GetFx(3, MainControl.Instance.AudioControl.fxClipUI);
        }

        /// <summary>
        /// 传入默认KeyCode并转换为游戏内键位。
        /// </summary>
        public static bool ConvertKeyDownToControl(KeyCode key)
        {
            var keyCodes = MainControl.Instance.overworldControl.KeyCodes;
            return key switch
            {
                KeyCode.DownArrow => GetKeyDownFrom(keyCodes, 0),
                KeyCode.RightArrow => GetKeyDownFrom(keyCodes, 1),
                KeyCode.UpArrow => GetKeyDownFrom(keyCodes, 2),
                KeyCode.LeftArrow => GetKeyDownFrom(keyCodes, 3),
                KeyCode.Z => GetKeyDownFrom(keyCodes, 4),
                KeyCode.X => GetKeyDownFrom(keyCodes, 5),
                KeyCode.C => GetKeyDownFrom(keyCodes, 6),
                KeyCode.V => GetKeyDownFrom(keyCodes, 7),
                KeyCode.F4 => GetKeyDownFrom(keyCodes, 8),
                KeyCode.Tab => GetKeyDownFrom(keyCodes, 9),
                KeyCode.Semicolon => GetKeyDownFrom(keyCodes, 10),
                KeyCode.Escape => GetKeyDownFrom(keyCodes, 11),
                _ => false,
            };
        }

        private static bool GetKeyDownFrom(List<KeyCode>[] keyCodes, int index)
        {
            return keyCodes.Any(keyCode => Input.GetKeyDown(keyCode[index]));
        }
        public static bool ConvertKeyToControl(KeyCode key)
        {
            var keyCodes = MainControl.Instance.overworldControl.KeyCodes;
            return key switch
            {
                KeyCode.DownArrow => GetKeyFrom(keyCodes, 0),
                KeyCode.RightArrow => GetKeyFrom(keyCodes, 1),
                KeyCode.UpArrow => GetKeyFrom(keyCodes, 2),
                KeyCode.LeftArrow => GetKeyFrom(keyCodes, 3),
                KeyCode.Z => GetKeyFrom(keyCodes, 4),
                KeyCode.X => GetKeyFrom(keyCodes, 5),
                KeyCode.C => GetKeyFrom(keyCodes, 6),
                KeyCode.V => GetKeyFrom(keyCodes, 7),
                KeyCode.F4 => GetKeyFrom(keyCodes, 8),
                KeyCode.Tab => GetKeyFrom(keyCodes, 9),
                KeyCode.Semicolon => GetKeyFrom(keyCodes, 10),
                KeyCode.Escape => GetKeyFrom(keyCodes, 11),
                _ => false,
            };
        }
        private static bool GetKeyFrom(List<KeyCode>[] keyCodes, int index)
        {
            return keyCodes.Any(keyCode => Input.GetKey(keyCode[index]));
        }
        public static bool ConvertKeyUpToControl(KeyCode key)
        {
            var keyCodes = MainControl.Instance.overworldControl.KeyCodes;
            return key switch
            {
                KeyCode.DownArrow => GetKeyUpFrom(keyCodes, 0),
                KeyCode.RightArrow => GetKeyUpFrom(keyCodes, 1),
                KeyCode.UpArrow => GetKeyUpFrom(keyCodes, 2),
                KeyCode.LeftArrow => GetKeyUpFrom(keyCodes, 3),
                KeyCode.Z => GetKeyUpFrom(keyCodes, 4),
                KeyCode.X => GetKeyUpFrom(keyCodes, 5),
                KeyCode.C => GetKeyUpFrom(keyCodes, 6),
                KeyCode.V => GetKeyUpFrom(keyCodes, 7),
                KeyCode.F4 => GetKeyUpFrom(keyCodes, 8),
                KeyCode.Tab => GetKeyUpFrom(keyCodes, 9),
                KeyCode.Semicolon => GetKeyUpFrom(keyCodes, 10),
                KeyCode.Escape => GetKeyUpFrom(keyCodes, 11),
                _ => false,
            };
        }
        private static bool GetKeyUpFrom(List<KeyCode>[] keyCodes, int index)
        {
            return keyCodes.Any(keyCode => Input.GetKeyUp(keyCode[index]));
        }
        /// <summary>
        /// 应用默认键位
        /// </summary>
        public static List<KeyCode>[] ApplyDefaultControl()
        {
            var keyCodes = new List<KeyCode>[3];
            keyCodes[0] = new List<KeyCode>
            {
                KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.LeftArrow,
                KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V,
                KeyCode.F4, KeyCode.None, KeyCode.None, KeyCode.Escape
            };
            keyCodes[1] = new List<KeyCode>
            {
                KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.A,
                KeyCode.Return, KeyCode.RightShift, KeyCode.RightControl, KeyCode.None, 
                KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None
            };
            keyCodes[2] = new List<KeyCode>
            {
                KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None,
                KeyCode.None, KeyCode.LeftShift, KeyCode.LeftControl, KeyCode.None,
                KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None
            };

            return keyCodes;
        }

        public static Color GetRandomColor()
        {
            var random = new System.Random();
            return new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1);
        }

        public static Color GetDifferentRandomColor(Color colorToAvoid)
        {
            Color newColor;
            do
            {
                newColor = GetRandomColor();
            } 
            while (newColor == colorToAvoid);

            return newColor;
        }
        
        public static Color GetSimilarButDifferentColor(Color originalColor, float hueOffset = 0.05f, float saturationOffset = 0.1f, float valueOffset = 0.1f)
        {
            // 使用可控的随机生成器
            var random = new System.Random();
    
            Color.RGBToHSV(originalColor, out var h, out var s, out var v);

            h = Mathf.Repeat(h + (float)(random.NextDouble() * 2 - 1) * hueOffset, 1.0f);
            s = Mathf.Clamp01(s + (float)(random.NextDouble() * 2 - 1) * saturationOffset);
            v = Mathf.Clamp01(v + (float)(random.NextDouble() * 2 - 1) * valueOffset);

            return Color.HSVToRGB(h, s, v);
        }
    }
}