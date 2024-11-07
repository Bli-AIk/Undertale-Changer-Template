using System;
using UCT.Global.Audio;
using UCT.Global.UI;
using UCT.Overworld;
using UCT.Service;
using UnityEngine;
using UnityEngine.Serialization;

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
        [Header("-OWCamera设置-")]
        public bool isCameraLimit;
        public bool isCameraFollow;
        public Vector2 cameraLimitX;//限制摄像机最大XY范围 0则不动
        public Vector2 cameraLimitY;//限制摄像机最大XY范围 0则不动
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
        public MainControl.SceneState sceneState;

        public bool haveInOutBlack, noInBlack;
        public bool notPauseIn;

        private void Awake()
        {
            SetupController(Camera.main, ExistingCameraSetup, NonexistentCameraSetup);
            SetupController(CanvasController.Instance, ExistingCanvasSetup, NonexistentCanvasSetup);
            SetupController(AudioController.Instance, ExistingAudioSetup, NonexistentAudioSetup);
            SetupController(MainControl.Instance, ExistingMainControlSetup, NonexistentMainControlSetup);
        }

      
        private static void SetupController<T>(T instance, Action existingSetup, Action nonexistentSetup)
            where T : class
        {
            if (instance != null)
                existingSetup();
            else
                nonexistentSetup();
        }
        private void ExistingCameraSetup()
        {
            if (sceneState != MainControl.SceneState.Overworld) return;
            System.Diagnostics.Debug.Assert(Camera.main != null, "Camera.main != null");
            var mainCamera = Camera.main.GetComponent<CameraFollowPlayer>();
            CameraSetup(mainCamera);
        }
        private void NonexistentCameraSetup()
        {
            if (sceneState != MainControl.SceneState.Overworld) return;
            var mainCamera = Instantiate(Resources.Load<GameObject>("Prefabs/MainCameraOverworld"))
                .GetComponent<CameraFollowPlayer>();
            mainCamera.name = "MainCameraOverworld";
            CameraSetup(mainCamera);
            DontDestroyOnLoad(mainCamera);
        }

        private void CameraSetup(CameraFollowPlayer cameraFollowPlayer)
        {
            cameraFollowPlayer.isLimit = isCameraLimit;
            cameraFollowPlayer.isFollow = isCameraFollow;
            cameraFollowPlayer.limitX = cameraLimitX;
            cameraFollowPlayer.limitY = cameraLimitY;
        }
        private void ExistingCanvasSetup()
        {
            CanvasSetup();
            CanvasController.Instance.Start();
        }
        private void NonexistentCanvasSetup()
        {
            var canvas = Instantiate(Resources.Load<GameObject>("Prefabs/Canvas"));
            canvas.name = "Canvas";
            CanvasSetup();
            DontDestroyOnLoad(canvas);
        }
        private void CanvasSetup()
        {
            CanvasController.Instance.framePic = framePic;
            CanvasController.Instance.renderMode = renderMode;
            CanvasController.Instance.openTurn = sceneState == MainControl.SceneState.InBattle;
        }
        private void ExistingAudioSetup()
        {
            var bgm = AudioController.Instance.gameObject;
            AudioSetup(bgm);
        }
        private void NonexistentAudioSetup()
        {
            var bgm = Instantiate(Resources.Load<GameObject>("Prefabs/BGM Source"));
            bgm.name = "BGM Source";
            DontDestroyOnLoad(bgm);
            AudioSetup(bgm);
        }
        private void AudioSetup(GameObject bgm)
        {
            var audioSource = bgm.GetComponent<AudioSource>();
            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.loop = loop;
            if (audioSource.clip == bgmClip) return;
            audioSource.clip = bgmClip;
            audioSource.Play();
        }
      

        
        private void ExistingMainControlSetup()
        {
            var mainControl = MainControl.Instance;
            MainControlSetup(mainControl);
            mainControl.Start();
        }
        private void NonexistentMainControlSetup()
        {
            DontDestroyOnLoad(transform);
            var mainControl = gameObject.AddComponent<MainControl>();
            MainControlSetup(mainControl);
            mainControl.gameObject.name = "MainControl";
        }
        
        private void MainControlSetup(MainControl mainControl)
        {
            mainControl.sceneState = sceneState;
            mainControl.isSceneSwitchingFadeTransitionEnabled = haveInOutBlack;
            mainControl.isSceneSwitchingFadeInDisabled = noInBlack;
            mainControl.isSceneSwitchingFadeInUnpaused = notPauseIn;
            mainControl.InitializationOverworld();
            mainControl.mainCamera = Camera.main;
            GameUtilityService.SetResolution(mainControl.OverworldControl.resolutionLevel);
        }
      
    }
}