using System.Collections.Generic;
using UnityEngine;
using static MainControl;

/// <summary>
/// �����ܿأ������л�����ʱ���������ɵ��ܿء�
/// �Դ�ֻ����һ��MainControl�����ݼ��ء�
///
/// ͬʱ������BGMControl
/// </summary>
public class MainControlSummon : MonoBehaviour
{
    [Header("-Canvas����-")]
    public RenderMode renderMode;

    public int framePic;

    [Space]
    [Header("-BGMControl����-")]
    [Space]
    [Header("BGM������Ƶ ��Ϊ����Ƶ")]
    public AudioClip bgmClip;

    [Header("BGM����")]
    public float volume = 0.5f;

    [Header("BGM����")]
    public float pitch = 1;

    [Header("BGMѭ�����ų�ʼ״̬")]
    public bool loop = true;

    [Header("-MainControl����-")]
    [Space]
    [Header("�ڳ�״̬���")]
    public SceneState sceneState;

    public bool haveInOutBlack, noInBlack;
    public bool notPauseIn;

    [Space]
    [Header("ս���ڳ�����������")]
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
            //DebugLogger.LogWarning("<color=yellow>��⵽����������MainControl</color>", gameObject);

            mainControl.sceneState = sceneState;
            mainControl.haveInOutBlack = haveInOutBlack;
            mainControl.noInBlack = noInBlack;
            mainControl.notPauseIn = notPauseIn;
            mainControl.InitializationOverworld();
            mainControl.Start();
            mainControl.SetResolution(instance.OverworldControl.resolutionLevel);
            return;
        }
        //����
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