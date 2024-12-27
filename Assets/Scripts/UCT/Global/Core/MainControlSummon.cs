using System;
using Alchemy.Inspector;
using UCT.Global.Audio;
using UCT.Global.Settings;
using UCT.Global.UI;
using UCT.Overworld;
using UCT.Service;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Global.Core
{
    /// <summary>
    ///     生成总控，并在切换场景时保留已生成的总控。
    ///     以此只调用一次MainControl的数据加载。
    ///     同时会生成BGMControl
    /// </summary>
    public class MainControlSummon : MonoBehaviour
    {
        [TabGroup("Layer1","OWCamera")]
        public bool isCameraLimit;
        [TabGroup("Layer1","OWCamera")]
        public bool isCameraFollow;
        [TabGroup("Layer1","OWCamera")]
        [EnableIf("isCameraFollow")][Indent] 
        public Vector2 cameraLimitX; //限制摄像机最大XY范围 0则不动
        [TabGroup("Layer1","OWCamera")]
        [EnableIf("isCameraFollow")][Indent] 
        public Vector2 cameraLimitY; //限制摄像机最大XY范围 0则不动
        
        // ---------------------------------------------------

        [TabGroup("Layer1","Canvas")]
        public RenderMode renderMode;
        [TabGroup("Layer1","Canvas")]
        [FormerlySerializedAs("framePic")] 
        public int frameSpriteIndex;
        
        // ---------------------------------------------------
      
        [TabGroup("Layer1","BGMControl")]
        [Title("BGM本体音频 空为无音频")]
        public AudioClip bgmClip;
        [TabGroup("Layer1","BGMControl")]
        [Title("BGM音量")] public float volume = 0.5f;
        [TabGroup("Layer1","BGMControl")]
        [Title("BGM音调")] public float pitch = 0.5f;
        [TabGroup("Layer1","BGMControl")]
        [Title("BGM循环播放初始状态")] public bool loop = true;
        
        // ---------------------------------------------------

        [TabGroup("Layer1","MainControl")]
        [Title("黑场状态相关")]
        public MainControl.SceneState sceneState;
        [FormerlySerializedAs("haveInOutBlack")] [TabGroup("Layer1","MainControl")]
        public bool isFadeTransitionEnabled;
        [FormerlySerializedAs("noInBlack")] [TabGroup("Layer1","MainControl")]
        [EnableIf("isFadeTransitionEnabled")][Indent]
        public bool isFadeInDisabled;
        [FormerlySerializedAs("notPauseIn")] [TabGroup("Layer1","MainControl")]
        [EnableIf("isFadeTransitionEnabled")][Indent]
        public bool isFadeInUnpaused;

        private void Awake()
        {
            SetupController(Camera.main, ExistingCameraSetup, NonexistentCameraSetup);
            SetupController(SettingsController.Instance, ExistingCanvasSetup, NonexistentCanvasSetup);
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
            System.Diagnostics.Debug.Assert(Camera.main != null, "Camera.main != null");
            var mainCamera = Camera.main.GetComponent<CameraFollowPlayer>();
            if (!mainCamera) return;
            if (sceneState != MainControl.SceneState.Overworld)
                Destroy(mainCamera.gameObject);
            else
                CameraSetup(mainCamera);
        }

        private void NonexistentCameraSetup()
        {
            if (sceneState != MainControl.SceneState.Overworld) return;
            var mainCamera = Instantiate(Resources.Load<GameObject>("Prefabs/MainCameraOverworld"));
            mainCamera.name = "MainCameraOverworld";
            CameraSetup(mainCamera.GetComponent<CameraFollowPlayer>());
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
            SettingsController.Instance.Start();
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
            SettingsController.Instance.frameSpriteIndex = frameSpriteIndex;
            SettingsController.Instance.renderMode = renderMode;
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
            mainControl.isFadeTransitionEnabled = isFadeTransitionEnabled;
            mainControl.isFadeInDisabled = isFadeInDisabled;
            mainControl.isFadeInUnpaused = isFadeInUnpaused;
            mainControl.InitializationOverworld();
            mainControl.mainCamera = Camera.main;
            GameUtilityService.SetResolution(SettingsStorage.resolutionLevel);
        }
    }
}