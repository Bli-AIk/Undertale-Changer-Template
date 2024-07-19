using System.Collections.Generic;
using UnityEngine;
using static MainControl;

/// <summary>
/// Generate a master control and keep the generated master control when switching scenes.
/// In this way the MainControl data load is called only once.
///
/// The BGMControl will be generated at the same time.
/// </summary>
public class MainControlSummon : MonoBehaviour
{
    [Header("-Canvas Settings-")]
    public RenderMode renderMode;

    public int framePic;

    [Space]
    [Header("-BGMControl settings-")]
    [Space]
    [Header("BGM body audio Empty for no audio")]
    public AudioClip bgmClip;

    [Header("BGM Volume")]
    public float volume = 0.5f;

    [Header("BGM Tone")]
    public float pitch = 1;

    [Header("Initial state of BGM loop")]
    public bool loop = true;

    [Header("BGM BPM")]
    public float bpm = 120;

    [Header("BGM BPM Offset")]
    public float bpmDeviation = 0;

    [Header("Turn on metronome at initialization")]
    public bool openMetronome = false;

    [Header("-MainControl settings-")]
    [Space]
    [Header("Blackfield status related")]
    public SceneState sceneState;

    public bool haveInOutBlack, noInBlack;
    public bool notPauseIn;

    [Space]
    [Header("Additional settings for in-combat scenarios")]
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
            //DebugLogger.LogWarning("<color=yellow>MainControl detected within this scene</color>", gameObject);

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
        // Generate
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
