using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
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

    AudioSource bgm;
    //BoxCollider2D boxCollider2D;
    //TextMeshProUGUI typeMessage;
    TalkUIPositionChanger talkUI;
    TypeWritter typeWritter;
    //BackpackBehaviour backpackBehaviour;
    // Start is called before the first frame update
    void Start()
    {
        transform.tag = "owObjTrigger";
        //boxCollider2D = GetComponent<BoxCollider2D>();
        talkUI = GameObject.Find("Main Camera/TalkUI").GetComponent<TalkUIPositionChanger>();
        mainCamera = talkUI.transform.parent.GetComponent<CameraFollowPlayer>();
        //typeMessage = GameObject.Find("BackpackCanvas/RawImage/Talk/UITalk").GetComponent<TextMeshProUGUI>();
        typeWritter = GameObject.Find("BackpackCanvas").GetComponent<TypeWritter>();
        bgm = GameObject.Find("BGM Source").GetComponent<AudioSource>();
        
        //backpackBehaviour = typeWritter.transform.GetComponent<BackpackBehaviour>();
    }
    private void Update()
    {
        if (MainControl.instance.KeyArrowToControl(KeyCode.Z) && !typeWritter.isTyping)
        {
            typeWritter.endString = "";
            talkUI.transform.localPosition = new Vector3(talkUI.transform.localPosition.x, talkUI.transform.localPosition.y, -50);
            Debug.Log(talkUI.transform.localPosition.z);
            MainControl.instance.PlayerControl.canMove = true;
            MainControl.instance.OverwroldControl.pause = false;

        }
    }

    /// <summary>
    /// ������֡��ڶ���������
    /// </summary>
    public void TypeText(bool isUp,bool isMusic = true)
    {
        MainControl.instance.PlayerControl.canMove = false;
        MainControl.instance.OverwroldControl.pause = true;
        talkUI.Change();
        //if (!talkUI.gameObject.activeSelf)
        if (talkUI.transform.localPosition.z < 0)
        {
            talkUI.transform.localPosition = new Vector3(talkUI.transform.localPosition.x, talkUI.transform.localPosition.y, 5);
            Debug.Log(talkUI.transform.localPosition.z);
        }
        talkUI.isUp = isUp;
        typeWritter.TypeOpen(MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.owTextsSave, text), false, 0, 0);
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
