using System;
using Alchemy.Inspector;
using DG.Tweening;
using JetBrains.Annotations;
using UCT.Audio;
using UCT.Overworld;
using UCT.Service;
using UCT.Settings;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Core
{
    /// <summary>
    ///     生成总控，并在切换场景时保留已生成的总控。
    ///     以此只调用一次MainControl的数据加载。
    ///     同时会生成BGMControl
    /// </summary>
    public class MainControlSummon : MonoBehaviour
    {
        [Title("OverWorld")] [TabGroup("OWLayer", "OWCamera")]
        public bool isCameraLimit;

        [TabGroup("OWLayer", "OWCamera")] public bool isCameraFollow;

        [TabGroup("OWLayer", "OWCamera")] [EnableIf("isCameraFollow")] [Indent]
        public float cameraMinX;

        [TabGroup("OWLayer", "OWCamera")] [EnableIf("isCameraFollow")] [Indent]
        public float cameraMinY;

        [TabGroup("OWLayer", "OWCamera")] [EnableIf("isCameraFollow")] [Indent]
        public float cameraMaxX;

        [TabGroup("OWLayer", "OWCamera")] [EnableIf("isCameraFollow")] [Indent]
        public float cameraMaxY;


        // ---------------------------------------------------

        [TabGroup("OWLayer", "OWPlayer")] public Vector2 walkFxRange = new(0, 9);

        [TabGroup("OWLayer", "OWPlayer")] public bool isShadow;

        // ---------------------------------------------------
        [Title("Other")] [TabGroup("OtherLayer", "Canvas")]
        public RenderMode renderMode;

        [TabGroup("OtherLayer", "Canvas")] [FormerlySerializedAs("framePic")]
        public int frameSpriteIndex;

        // ---------------------------------------------------

        [TabGroup("OtherLayer", "BGMControl")] [Title("BGM本体音频 空为无音频")]
        public AudioClip bgmClip;

        [TabGroup("OtherLayer", "BGMControl")] [Title("BGM音量")]
        public float volume = 0.5f;

        [TabGroup("OtherLayer", "BGMControl")] [Title("BGM音调")]
        public float pitch = 0.5f;

        [TabGroup("OtherLayer", "BGMControl")] [Title("BGM循环播放初始状态")]
        public bool loop = true;

        [TabGroup("OtherLayer", "BGMControl")] [Title("渐入")]
        public bool fadeIn;

        // ---------------------------------------------------

        [TabGroup("OtherLayer", "MainControl")] [Title("黑场状态相关")]
        public MainControl.SceneState sceneState;

        [FormerlySerializedAs("haveInOutBlack")] [TabGroup("OtherLayer", "MainControl")]
        public bool isFadeTransitionEnabled;

        [FormerlySerializedAs("noInBlack")]
        [TabGroup("OtherLayer", "MainControl")]
        [EnableIf("isFadeTransitionEnabled")]
        [Indent]
        public bool isFadeInDisabled;

        [FormerlySerializedAs("notPauseIn")]
        [TabGroup("OtherLayer", "MainControl")]
        [EnableIf("isFadeTransitionEnabled")]
        [Indent]
        public bool isFadeInUnpaused;

        private void Awake()
        {
            if (sceneState == MainControl.SceneState.Overworld)
            {
                SetupController(MainControl.OverworldPlayerBehaviour, PlayerSetup, NonexistentOverworldPlayerSetup);
            }

            SetupController(Camera.main, ExistingCameraSetup, NonexistentCameraSetup);
            SetupController(SettingsController.Instance, ExistingCanvasSetup, NonexistentCanvasSetup);
            SetupController(AudioController.Instance, ExistingAudioSetup, NonexistentAudioSetup);
            SetupController(MainControl.Instance, ExistingMainControlSetup, NonexistentMainControlSetup);
        }


        private static void SetupController<T>(T instance,
            [CanBeNull] Action existingSetup,
            [CanBeNull] Action nonexistentSetup)
            where T : class
        {
            if (instance != null)
            {
                existingSetup?.Invoke();
            }
            else
            {
                nonexistentSetup?.Invoke();
            }
        }

        private void ExistingCameraSetup()
        {
            System.Diagnostics.Debug.Assert(Camera.main != null, "Camera.main != null");
            var mainCamera = CameraFollowPlayer.Instance;
            if (!mainCamera)
            {
                return;
            }

            if (sceneState != MainControl.SceneState.Overworld)
            {
                Destroy(mainCamera.gameObject);
            }
            else
            {
                CameraSetup(mainCamera);
            }
        }

        private void NonexistentCameraSetup()
        {
            if (sceneState != MainControl.SceneState.Overworld)
            {
                return;
            }

            var mainCamera = Instantiate(Resources.Load<GameObject>("Prefabs/MainCameraOverworld"));
            mainCamera.name = "MainCameraOverworld";
            CameraSetup(CameraFollowPlayer.Instance);
            DontDestroyOnLoad(mainCamera);
        }

        private void CameraSetup(CameraFollowPlayer cameraFollowPlayer)
        {
            cameraFollowPlayer.isLimit = isCameraLimit;
            cameraFollowPlayer.isFollow = isCameraFollow;
            cameraFollowPlayer.minX = cameraMinX;
            cameraFollowPlayer.maxX = cameraMaxX;
            cameraFollowPlayer.minY = cameraMinY;
            cameraFollowPlayer.maxY = cameraMaxY;
        }

        private void NonexistentOverworldPlayerSetup()
        {
            var player = Instantiate(Resources.Load<GameObject>("Prefabs/Player"));
            player.name = "Player";
            MainControl.GetOverworldPlayerBehaviour();
            PlayerSetup();
            DontDestroyOnLoad(player);
        }

        private void PlayerSetup()
        {
            MainControl.OverworldPlayerBehaviour.walkFxRange = walkFxRange;
            MainControl.OverworldPlayerBehaviour.isShadow = isShadow;
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

            if (sceneState == MainControl.SceneState.Battle &&
                MainControl.Instance &&
                MainControl.Instance.BattleControl &&
                MainControl.Instance.BattleControl.BattleConfig != null)
            {
                var config = MainControl.Instance.BattleControl.BattleConfig;
                pitch = config.pitch;
                volume = config.volume;
                bgmClip = config.bgmClip;
            }

            audioSource.pitch = pitch;
            audioSource.loop = loop;
            if (audioSource.clip == bgmClip)
            {
                return;
            }

            audioSource.clip = bgmClip;
            audioSource.Play();
            if (fadeIn)
            {
                audioSource.volume = 0;
                audioSource.DOFade(volume, 0.5f);
            }
            else
            {
                audioSource.volume = volume;
            }
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
            mainControl.InitializationScene();
            mainControl.mainCamera = Camera.main;
            GameUtilityService.SetResolution(SettingsStorage.ResolutionLevel);
        }
    }
}