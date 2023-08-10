using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Reflection;
using UnityEngine.SceneManagement;
using System;
/// <summary>
/// OWObj触发器相关 配合玩家射线
/// 用于读取并显示文本然后显示出来
/// </summary>

public class OverworldObjTrigger : MonoBehaviour
{
    //若为true，则碰到就触发。false，按Z触发。
    public bool isTriggerMode;
    public bool setIsUp;
    public bool isUp;
    public string text;
    [Header("检测玩家动画方向 0,0为不检测")]
    public Vector2 playerDir;
    [Header("存档相关")]
    public bool isSave;
    public bool saveFullHp;
    int saveSelect;
    bool saveOpen;
    [Header("插入摄像机动画相关")]
    public bool openAnim;
    public Vector3 animEndPosPlus;
    public float animTime;
    public Ease animEase;
    public CameraFollowPlayer mainCamera;
    public bool endInBattle;
    [Header("需要渐出就填正数时间")]
    public float stopTime = -1;

    [Header("OW跳场景 只给trigger")]
    public bool changeScene;
    public bool banMusic;
    public string sceneName;
    public Vector3 newPlayerPos;
    [Header("OW跳场景锁定进入时方向 0无 -1左右 1上下")]
    public int onlyDir;
    AudioSource bgm;
    TalkUIPositionChanger talkUI;
    TypeWritter typeWritter;

    [Header("结束时调用动画器并将下设为true")]
    public bool endAnim;
    public string animRoute;
    public string animBoolName;

    [Header("结束时物体自身关闭")]
    public bool endSelf;
	
	[Header("确定目前打字的物体")]
	bool isTyping;

    [Header("结束时执行方法")]
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

        //检测相关见PlayerBehaviour
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
                Debug.LogError(item + "检测失败", gameObject);
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
    /// 激活打字。第二个参数别动
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
