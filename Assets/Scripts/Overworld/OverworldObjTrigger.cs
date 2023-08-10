using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Reflection;
using UnityEngine.SceneManagement;
using System;
/// <summary>
/// OWObj��������� ����������
/// ���ڶ�ȡ����ʾ�ı�Ȼ����ʾ����
/// </summary>

public class OverworldObjTrigger : MonoBehaviour
{
    //��Ϊtrue���������ʹ�����false����Z������
    public bool isTriggerMode;
    public bool setIsUp;
    public bool isUp;
    public string text;
    [Header("�����Ҷ������� 0,0Ϊ�����")]
    public Vector2 playerDir;
    [Header("�浵���")]
    public bool isSave;
    public bool saveFullHp;
    int saveSelect;
    bool saveOpen;
    [Header("����������������")]
    public bool openAnim;
    public Vector3 animEndPosPlus;
    public float animTime;
    public Ease animEase;
    public CameraFollowPlayer mainCamera;
    public bool endInBattle;
    [Header("��Ҫ������������ʱ��")]
    public float stopTime = -1;

    [Header("OW������ ֻ��trigger")]
    public bool changeScene;
    public bool banMusic;
    public string sceneName;
    public Vector3 newPlayerPos;
    [Header("OW��������������ʱ���� 0�� -1���� 1����")]
    public int onlyDir;
    AudioSource bgm;
    TalkUIPositionChanger talkUI;
    TypeWritter typeWritter;

    [Header("����ʱ���ö�������������Ϊtrue")]
    public bool endAnim;
    public string animRoute;
    public string animBoolName;

    [Header("����ʱ��������ر�")]
    public bool endSelf;
	
	[Header("ȷ��Ŀǰ���ֵ�����")]
	bool isTyping;

    [Header("����ʱִ�з���")]
    public List<string> funNames;

    
    void Start()
    {
        transform.tag = "owObjTrigger";
        talkUI = GameObject.Find("Main Camera/TalkUI").GetComponent<TalkUIPositionChanger>();
        mainCamera = talkUI.transform.parent.GetComponent<CameraFollowPlayer>();
        typeWritter = BackpackBehaviour.instance.typeWritter;
        bgm = AudioController.instance.audioSource;
    }
    private void Update()
    {
        if (saveOpen)
        {
            if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow) || MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
            {
                saveSelect = Convert.ToInt32(!Convert.ToBoolean(saveSelect));


                BackpackBehaviour.instance.saveUIHeart.anchoredPosition = new Vector2(-258 + saveSelect * 180, -44);
            }
            if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
            {
                switch (saveSelect)
                {
                    case 0:

                        SaveController.SaveData(MainControl.instance.PlayerControl, "Data"+ MainControl.instance.dataNum);
                        saveSelect = 2;
                        AudioController.instance.GetFx(12, MainControl.instance.AudioControl.fxClipUI);
                        string name = MainControl.instance.PlayerControl.playerName;

                        BackpackBehaviour.instance.saveUIHeart.anchoredPosition = new Vector2(10000, 10000);

                        BackpackBehaviour.instance.saveUI.text = "<color=yellow>" + MainControl.instance.FillString(name, 10) + "LV" + MainControl.instance.FillString(MainControl.instance.PlayerControl.lv.ToString(), 7) +
                        MainControl.instance.GetRealTime((int)MainControl.instance.PlayerControl.gameTime) + "\n" +
                        MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, SceneManager.GetActiveScene().name) + "\n<size=1>\n</size>  " +
                        MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Saved");
                        MainControl.instance.PlayerControl.saveScene = SceneManager.GetActiveScene().name;
                        PlayerPrefs.SetInt("languagePack", MainControl.instance.languagePack);
                        PlayerPrefs.SetInt("dataNum", MainControl.instance.dataNum);
                        PlayerPrefs.SetInt("hdResolution", Convert.ToInt32(MainControl.instance.OverworldControl.hdResolution));
                        PlayerPrefs.SetInt("noSFX", Convert.ToInt32(MainControl.instance.OverworldControl.noSFX));
                        PlayerPrefs.SetInt("vsyncMode", Convert.ToInt32(MainControl.instance.OverworldControl.vsyncMode));
                        break;
                    case 1:
                        goto default;
                    default:
                        BackpackBehaviour.instance.saveUIHeart.anchoredPosition = new Vector2(10000, 10000);
                        BackpackBehaviour.instance.saveBack.transform.localPosition = new Vector3(BackpackBehaviour.instance.saveBack.transform.localPosition.x, BackpackBehaviour.instance.saveBack.transform.localPosition.y, -50);
                        BackpackBehaviour.instance.saveUI.text = "";
                        PressZ();
                        saveOpen = false;
                        break;
                    case 2:
                        goto default;
                }
            }
            else if (MainControl.instance.KeyArrowToControl(KeyCode.X))
            {
                BackpackBehaviour.instance.saveUIHeart.anchoredPosition = new Vector2(10000, 10000);
                BackpackBehaviour.instance.saveBack.transform.localPosition = new Vector3(BackpackBehaviour.instance.saveBack.transform.localPosition.x, BackpackBehaviour.instance.saveBack.transform.localPosition.y, -50);
                BackpackBehaviour.instance.saveUI.text = "";
                PressZ();
                saveOpen = false;
            }
                
        }

        //�����ؼ�PlayerBehaviour
        if (isTyping && MainControl.instance.KeyArrowToControl(KeyCode.Z) && !typeWritter.isTyping)
        {
            PressZ();
        }
    }
    public void PressZ()
    {
        if (BackpackBehaviour.instance.typeMessage.text != "")
        {
            if (endAnim)
            {
                GameObject.Find(animRoute).GetComponent<Animator>().SetBool(animBoolName, true);
            }

            if (endSelf)
                gameObject.SetActive(false);
        }
        isTyping = false;
        BackpackBehaviour.instance.typeMessage.text = "";
        talkUI.transform.localPosition = new Vector3(talkUI.transform.localPosition.x, talkUI.transform.localPosition.y, -50);
        //Debug.Log(talkUI.transform.localPosition.z);
        if (isSave && !saveOpen)
        {
            Save();
            return;
        }

        MainControl.instance.PlayerControl.canMove = true;
        MainControl.instance.OverworldControl.pause = false;

        foreach (var item in funNames)
        {
            MethodInfo methodInfo = typeof(OverworldObjTrigger).GetMethod(item);
            if (methodInfo == null)
            {
                Debug.LogError(item + "���ʧ��", gameObject);
            }
            else
            {
                methodInfo.Invoke(this, new object[0]);
            }
        }
      
    }

    public void Save()
    {
        saveOpen = true;
        saveSelect = 0;


        BackpackBehaviour.instance.saveBack.transform.localPosition = new Vector3(BackpackBehaviour.instance.saveBack.transform.localPosition.x, BackpackBehaviour.instance.saveBack.transform.localPosition.y, 5);
        string name = MainControl.instance.PlayerControl.playerName;

        BackpackBehaviour.instance.saveUI.text = MainControl.instance.FillString(name, 10) + "LV" + MainControl.instance.FillString(MainControl.instance.PlayerControl.lv.ToString(), 7) +
            MainControl.instance.GetRealTime((int)MainControl.instance.PlayerControl.gameTime) + "\n" +
            MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, SceneManager.GetActiveScene().name) + "\n<size=1>\n</size>  " +
            MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Save") + "         " + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Back")
            ;
        BackpackBehaviour.instance.saveUIHeart.anchoredPosition = new Vector2(-258, -44);


    }
    /// <summary>
    /// ������֡��ڶ���������
    /// </summary>
    public void TypeText(bool isUp,bool isMusic = true)
    {
		isTyping = true;
        MainControl.instance.PlayerControl.canMove = false;
        MainControl.instance.OverworldControl.pause = true;
        talkUI.Change(true);

        //if (!talkUI.gameObject.activeSelf)
        if (talkUI.transform.localPosition.z < 0)
        {
            talkUI.transform.localPosition = new Vector3(talkUI.transform.localPosition.x, talkUI.transform.localPosition.y, 5);
            //Debug.Log(talkUI.transform.localPosition.z);
        }
        talkUI.isUp = isUp;

        if(typeWritter == null)
            typeWritter = BackpackBehaviour.instance.typeWritter;

        typeWritter.TypeOpen(MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, text), false, 0, 1, BackpackBehaviour.instance.typeMessage);
        if (endInBattle)
            typeWritter.EndInBattle();


        if (isMusic && stopTime >= 0)
            bgm.DOFade(0, stopTime);
    }

    public void AnimTypeText(bool isUp)
    {
        MainControl.instance.PlayerControl.canMove = false;
        MainControl.instance.OverworldControl.pause = true;
        mainCamera.isFollow = false;
        mainCamera.transform.DOLocalMove(animEndPosPlus, animTime).SetEase(animEase).OnKill(() => TypeText(isUp, false));
        DOTween.To(() => mainCamera.followPosition, x => mainCamera.followPosition = x, animEndPosPlus, animTime).SetEase(animEase);

        if (stopTime >= 0)
            bgm.DOFade(0, stopTime);


    }

    
}
