using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Reflection;
/// <summary>
/// 非常简单的一个脚本
/// 最常用的方法是挂在obj上面配一个碰撞器 配合玩家射线
/// 用于读取并显示文本然后显示出来
/// </summary>

public class OverworldObjTrigger : MonoBehaviour
{
    //若为true，则碰到就触发。false，按Z触发。
    public bool isTriggerMode;
    public bool setIsUp;
    public bool isUp;
    public string text;
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
    public string sceneName;
    public Vector3 newPlayerPos;
    [Header("OW跳场景锁定进入时方向 0无 -1左右 1上下")]
    public int onlyDir;
    AudioSource bgm;
    //BoxCollider2D boxCollider2D;
    //TextMeshProUGUI typeMessage;
    TalkUIPositionChanger talkUI;
    TypeWritter typeWritter;
    //BackpackBehaviour backpackBehaviour;

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
                //Debug.LogError(item + "检测失败", gameObject);
            }
            else
            {
                methodInfo.Invoke(this, new object[0]);
            }
        }
      
    }
    /// <summary>
    /// 激活打字。第二个参数别动
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
