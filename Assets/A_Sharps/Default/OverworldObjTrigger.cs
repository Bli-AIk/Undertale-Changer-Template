using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Reflection;
/// <summary>
/// �ǳ��򵥵�һ���ű�
/// ��õķ����ǹ���obj������һ����ײ�� ����������
/// ���ڶ�ȡ����ʾ�ı�Ȼ����ʾ����
/// </summary>

public class OverworldObjTrigger : MonoBehaviour
{
    //��Ϊtrue���������ʹ�����false����Z������
    public bool isTriggerMode;
    public bool setIsUp;
    public bool isUp;
    public string text;
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
    public string sceneName;
    public Vector3 newPlayerPos;
    [Header("OW��������������ʱ���� 0�� -1���� 1����")]
    public int onlyDir;
    AudioSource bgm;
    //BoxCollider2D boxCollider2D;
    //TextMeshProUGUI typeMessage;
    TalkUIPositionChanger talkUI;
    TypeWritter typeWritter;
    //BackpackBehaviour backpackBehaviour;

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

    // Start is called before the first frame update
    void Start()
    {
        transform.tag = "owObjTrigger";
        //boxCollider2D = GetComponent<BoxCollider2D>();
        talkUI = GameObject.Find("Main Camera/TalkUI").GetComponent<TalkUIPositionChanger>();
        mainCamera = talkUI.transform.parent.GetComponent<CameraFollowPlayer>();
        //typeMessage = GameObject.Find("BackpackCanvas/RawImage/Talk/UITalk").GetComponent<TextMeshProUGUI>();
        typeWritter = GameObject.Find("BackpackCanvas").GetComponent<TypeWritter>();
        bgm = AudioController.instance.audioSource;
        
        //backpackBehaviour = typeWritter.transform.GetComponent<BackpackBehaviour>();

    }
    private void Update()
    {
        if (isTyping && MainControl.instance.KeyArrowToControl(KeyCode.Z) && !typeWritter.isTyping)
        {
            PressZ();
        }
    }
    public void PressZ()
    {
        if (typeWritter.endString != "")
        {
            if (endAnim)
            {
                GameObject.Find(animRoute).GetComponent<Animator>().SetBool(animBoolName, true);
            }

            if (endSelf)
                gameObject.SetActive(false);
        }
        isTyping = false;
        typeWritter.endString = "";
        talkUI.transform.localPosition = new Vector3(talkUI.transform.localPosition.x, talkUI.transform.localPosition.y, -50);
        //Debug.Log(talkUI.transform.localPosition.z);
        MainControl.instance.PlayerControl.canMove = true;
        MainControl.instance.OverwroldControl.pause = false;

        foreach (var item in funNames)
        {
            MethodInfo methodInfo = typeof(OverworldObjTrigger).GetMethod(item);
            if (methodInfo == null)
            {
                //Debug.LogError(item + "���ʧ��", gameObject);
            }
            else
            {
                methodInfo.Invoke(this, new object[0]);
            }
        }
      
    }
    /// <summary>
    /// ������֡��ڶ���������
    /// </summary>
    public void TypeText(bool isUp,bool isMusic = true)
    {
		isTyping = true;
        MainControl.instance.PlayerControl.canMove = false;
        MainControl.instance.OverwroldControl.pause = true;
        talkUI.Change(true);

        //if (!talkUI.gameObject.activeSelf)
        if (talkUI.transform.localPosition.z < 0)
        {
            talkUI.transform.localPosition = new Vector3(talkUI.transform.localPosition.x, talkUI.transform.localPosition.y, 5);
            //Debug.Log(talkUI.transform.localPosition.z);
        }
        talkUI.isUp = isUp;
        typeWritter.TypeOpen(MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.owTextsSave, text), false, 0, 1);
        if (endInBattle)
            typeWritter.EndInBattle();


        if (isMusic && stopTime >= 0)
            bgm.DOFade(0, stopTime);
    }

    public void AnimTypeText(bool isUp)
    {
        MainControl.instance.PlayerControl.canMove = false;
        MainControl.instance.OverwroldControl.pause = true;
        mainCamera.isFollow = false;
        mainCamera.transform.DOLocalMove(animEndPosPlus, animTime).SetEase(animEase).OnKill(() => TypeText(isUp, false));
        DOTween.To(() => mainCamera.followPosition, x => mainCamera.followPosition = x, animEndPosPlus, animTime).SetEase(animEase);

        if (stopTime >= 0)
            bgm.DOFade(0, stopTime);


    }
}
