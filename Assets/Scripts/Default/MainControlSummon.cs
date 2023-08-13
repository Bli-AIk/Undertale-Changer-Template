using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static MainControl;
/// <summary>
/// Generate a MainControl and retain the generated master control when switching scenes.
/// Only call MainControl once to load data.
/// 
/// BGMControl will also be generated
/// </summary>
public class MainControlSummon : MonoBehaviour
{
    [Header("-Canvas Settings-")]
    public RenderMode renderMode;
    public int framePic;
    [Space]

    [Header("-BGMControl Settings-")]
    [Space]
    [Header("BGM. Empty for no audio")]
    public AudioClip bgmClip;
    [Header("BGM Volume")]
    public float volume = 0.5f;
    [Header("BGM Pitch")]
    public float pitch = 1;
    [Header("BGM loop playback initial state")]
    public bool loop = true;


    [Header("-MainControl Settings-")]

    [Space]

    [Header("Black field state related")]
    public SceneState sceneState;
    public bool haveInOutBlack, noInBlack;
    public bool notPauseIn;

    [Space]
    [Header("Additional settings for combat scenes")]
    public List<int> poolCount;

    void Awake()
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
            //Debug.LogWarning("<color=yellow>MainControl detected in this scene</color>", gameObject);

            mainControl.sceneState = sceneState;
            mainControl.haveInOutBlack = haveInOutBlack;
            mainControl.noInBlack = noInBlack;
            mainControl.notPauseIn = notPauseIn;
            mainControl.InitializationOverworld();
            mainControl.Start();
            mainControl.SetResolution(instance.OverworldControl.resolutionLevel);
            return;
        }
        //Summon
        DontDestroyOnLoad(transform);





        mainControl = gameObject.AddComponent<MainControl>();
        mainControl.sceneState = sceneState;
        mainControl.haveInOutBlack = haveInOutBlack;
        mainControl.noInBlack = noInBlack;
        mainControl.notPauseIn = notPauseIn;

        mainControl.gameObject.name = "MainControl";
        mainControl.InitializationOverworld();
        mainControl.SetResolution(instance.OverworldControl.resolutionLevel);

    }
}
