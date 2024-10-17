using System.Collections.Generic;
using UCT.Global.Audio;
using UCT.Global.UI;
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
        public float pitch = 1;

        [Header("BGM循环播放初始状态")]
        public bool loop = true;

        [Header("BGM BPM")]
        public float bpm = 120;

        [Header("BGM BPM偏移")]
        public float bpmDeviation = 0;

        [Header("初始化时开启节拍器")]
        public bool openMetronome = false;

        [Header("-MainControl设置-")]
        [Space]
        [Header("黑场状态相关")]
        public SceneState sceneState;

        public bool haveInOutBlack, noInBlack;
        public bool notPauseIn;

        [Space]
        [Header("战斗内场景额外设置")]
        public List<int> poolCount;

        private void Awake()
        {
            GameObject canvas;
            if (CanvasController.instance == null)
            {
                canvas = Instantiate(Resources.Load<GameObject>("Prefabs/Canvas"));
                canvas.name = "Canvas";
                CanvasController.instance.framePic = framePic;
                CanvasController.instance.renderMode = renderMode;
                CanvasController.instance.openTurn = sceneState == SceneState.InBattle;
                DontDestroyOnLoad(canvas);
            }
            else
            {
                CanvasController.instance.framePic = framePic;
                CanvasController.instance.renderMode = renderMode;
                CanvasController.instance.openTurn = sceneState == SceneState.InBattle;
                CanvasController.instance.Start();
            }

            GameObject bgm;
            if (AudioController.instance == null)
            {
                bgm = Instantiate(Resources.Load<GameObject>("Prefabs/BGM Source"));
                bgm.name = "BGM Source";
                DontDestroyOnLoad(bgm);
            }
            else
            {
                bgm = AudioController.instance.gameObject;
            }
            AudioSource audioSource = bgm.GetComponent<AudioSource>();
            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.loop = loop;
            if (audioSource.clip != bgmClip)
            {
                audioSource.clip = bgmClip;
                audioSource.Play();
            }

            MainControl mainControl;
            GameObject gameObjectM = GameObject.Find("MainControl");
            if (gameObjectM != null && gameObjectM.TryGetComponent(out mainControl))
            {
                //Debug.LogWarning("<color=yellow>检测到本场景内有MainControl</color>", gameObject);

                mainControl.sceneState = sceneState;
                mainControl.haveInOutBlack = haveInOutBlack;
                mainControl.noInBlack = noInBlack;
                mainControl.notPauseIn = notPauseIn;

                mainControl.bpm = bpm;
                mainControl.bpmDeviation = bpmDeviation;
                mainControl.isMetronome = openMetronome;

                mainControl.InitializationOverworld();
                mainControl.Start();
                mainControl.SetResolution(instance.OverworldControl.resolutionLevel);
                return;
            }
            //生成
            DontDestroyOnLoad(transform);

            mainControl = gameObject.AddComponent<MainControl>();
            mainControl.sceneState = sceneState;
            mainControl.haveInOutBlack = haveInOutBlack;
            mainControl.noInBlack = noInBlack;
            mainControl.notPauseIn = notPauseIn;

            mainControl.bpm = bpm;
            mainControl.bpmDeviation = bpmDeviation;
            mainControl.isMetronome = openMetronome;

            mainControl.gameObject.name = "MainControl";
            mainControl.InitializationOverworld();
            mainControl.SetResolution(instance.OverworldControl.resolutionLevel);
        }
    }
}