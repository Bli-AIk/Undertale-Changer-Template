using System.Collections.Generic;
using UCT.Global.Audio;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using static UCT.Global.Core.MainControl;

namespace UCT.Global.Core
{
    /// <summary>
    /// 生成总控，并在切换场景时保留已生成的总控。
    /// 以此只调用一次MainControl的数据加载。
    ///
    /// 同时会生成BGMControl
    /// </summary>
    public class MainControlSummon : MonoBehaviour
    {
        [Header("-Canvas设置-")]
        public RenderMode renderMode;

        public int framePic;

        [Space]
        [Header("-BGMControl设置-")]
        [Space]
        [Header("BGM本体音频 空为无音频")]
        public AudioClip bgmClip;

        [Header("BGM音量")]
        public float volume = 0.5f;

        [Header("BGM音调")]
        public float pitch = 0.5f;

        [Header("BGM循环播放初始状态")]
        public bool loop = true;

        [Header("-MainControl设置-")]
        [Space]
        [Header("黑场状态相关")]
        public SceneState sceneState;

        public bool haveInOutBlack, noInBlack;
        public bool notPauseIn;

        private void Awake()
        {
            if (CanvasController.Instance == null)
            {
                var canvas = Instantiate(Resources.Load<GameObject>("Prefabs/Canvas"));
                canvas.name = "Canvas";
                CanvasController.Instance.framePic = framePic;
                CanvasController.Instance.renderMode = renderMode;
                CanvasController.Instance.openTurn = sceneState == SceneState.InBattle;
                DontDestroyOnLoad(canvas);
            }
            else
            {
                CanvasController.Instance.framePic = framePic;
                CanvasController.Instance.renderMode = renderMode;
                CanvasController.Instance.openTurn = sceneState == SceneState.InBattle;
                CanvasController.Instance.Start();
            }

            GameObject bgm;
            if (AudioController.Instance == null)
            {
                bgm = Instantiate(Resources.Load<GameObject>("Prefabs/BGM Source"));
                bgm.name = "BGM Source";
                DontDestroyOnLoad(bgm);
            }
            else
            {
                bgm = AudioController.Instance.gameObject;
            }
            var audioSource = bgm.GetComponent<AudioSource>();
            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.loop = loop;
            if (audioSource.clip != bgmClip)
            {
                audioSource.clip = bgmClip;
                audioSource.Play();
            }

            var gameObjectM = GameObject.Find("MainControl");
            if (gameObjectM != null && gameObjectM.TryGetComponent(out MainControl mainControl))
            {
                ExistingMainControlSetup(mainControl);
                return;
            }
            NonexistentMainControlSetup();
        }
        /// <summary>
        /// 在未生成MainControl的情况下初始化MainControl
        /// </summary>
        private void NonexistentMainControlSetup()
        {
            DontDestroyOnLoad(transform);

            var mainControl = gameObject.AddComponent<MainControl>();
            mainControl.sceneState = sceneState;
            mainControl.isSceneSwitchingFadeTransitionEnabled = haveInOutBlack;
            mainControl.isSceneSwitchingFadeInDisabled = noInBlack;
            mainControl.isSceneSwitchingFadeInUnpaused = notPauseIn;

            mainControl.gameObject.name = "MainControl";
            mainControl.InitializationOverworld();
            mainControl.mainCamera = Camera.main;
            GameUtilityService.SetResolution(Instance.OverworldControl.resolutionLevel);
        }
        /// <summary>
        /// 在已生成MainControl的情况下初始化MainControl
        /// </summary>
        private void ExistingMainControlSetup(MainControl mainControl)
        {
            mainControl.sceneState = sceneState;
            mainControl.sceneState = sceneState;
            mainControl.isSceneSwitchingFadeTransitionEnabled = haveInOutBlack;
            mainControl.isSceneSwitchingFadeInDisabled = noInBlack;
            mainControl.isSceneSwitchingFadeInUnpaused = notPauseIn;

            mainControl.InitializationOverworld();
            mainControl.Start();
            mainControl.mainCamera = Camera.main;
            GameUtilityService.SetResolution(Instance.OverworldControl.resolutionLevel);
        }
    }
}