using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
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

    void Awake()
    {
        GameObject canvas;
        if (CanvasController.instance == null)
        {
            canvas = Instantiate(Resources.Load<GameObject>("Prefabs/Canvas"));
            canvas.name = "Canvas";
            CanvasController.instance.renderMode = renderMode;
            DontDestroyOnLoad(canvas);
        }
        else
        {
            CanvasController.instance.renderMode = renderMode;
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
            //Debug.LogWarning("<color=yellow>��⵽����������MainControl</color>", gameObject);

            mainControl.sceneState = sceneState;
            mainControl.haveInOutBlack = haveInOutBlack;
            mainControl.noInBlack = noInBlack;
            mainControl.notPauseIn = notPauseIn;
            mainControl.Start();
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

    }
}