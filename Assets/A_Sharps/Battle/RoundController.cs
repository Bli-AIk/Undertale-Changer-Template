using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
/// <summary>
/// 回合控制，同时也是弹幕的对象池
/// </summary>
public class RoundController : MonoBehaviour
{
    public static RoundController instance;
    public int round;
    public float roundClock;
    public bool isMyRound;

    SelentUIController selentUIController;
    BattlePlayerController player;

    public int poolCount;
    Queue<GameObject> bulletAvailble = new Queue<GameObject>();//弹幕对象池
    Queue<GameObject> boardAvailble = new Queue<GameObject>();//挡板对象池
    public List<string> inheritList = new List<string>();
    
    public int roundAssetLine;//检测回合文本的行数 0开
    float roundAssetTime;//暂存回合文本行的执行时间


    private void Awake()
    {
        instance = this;
        FillPool("bullet");
        FillPool("board");
    }
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<BattlePlayerController>();
        selentUIController = GameObject.Find("SelentUI").GetComponent<SelentUIController>();
        //OutYourRound();

    }
    IEnumerator GetFx(float fxPlayWhileTime, AudioClip audioClipSave, int fxPlayNum)
    {
        if (fxPlayNum == 0)
            fxPlayNum = 1;
        while (true)
        {

            if (fxPlayNum > 0)
            {
                AudioController.instance.GetFx(audioClipSave);
                fxPlayNum--;
                yield return new WaitForSeconds(fxPlayWhileTime);
            }
            else break;

        }
    }
    // Update is called once per frame
    void Update()
    {
       


        if (!isMyRound)
        {

            if (MainControl.instance.BattleControl.roundSave.Count > roundAssetLine)
            {
                string asset = MainControl.instance.BattleControl.roundSave[roundAssetLine];
                string save = "";
                for (int i = asset.Length - 2; i > 0; i--)//检测最后一个
                {
                    if (asset[i] != '\\')
                        save = asset[i] + save;
                    else
                        break;
                }

                if (MainControl.instance.BattleControl.roundSave[roundAssetLine].Length > 7 && MainControl.instance.BattleControl.roundSave[roundAssetLine].Substring(0, 7) == "Inherit")
                {
                    string text = MainControl.instance.BattleControl.roundSave[roundAssetLine].Substring(8);
                    text = text.Substring(0, text.Length - 1 - MainControl.instance.MaxToLastFloat(text).ToString().Length);
                    inheritList.Add(text);
                    //Debug.LogWarning(MainControl.instance.BattleControl.roundSave[roundAssetLine] + " / " + roundAssetLine);
                    roundAssetLine += 1;

                }
                else
                {
                    roundAssetTime = float.Parse(save);

                    if (roundClock >= roundAssetTime)
                    {
                        float i;
                        i = RunRoundAsset(true, "", gameObject,1);//执行
                        if (i < 99999999 && MainControl.instance.BattleControl.roundSave[roundAssetLine].Substring(0, 7) != "Inherit")
                        {

                            while (roundAssetLine + 1  < MainControl.instance.BattleControl.roundSave.Count && i == MainControl.instance.MaxToLastFloat(MainControl.instance.BattleControl.roundSave[roundAssetLine + 1]))
                            {
                                RunRoundAsset(true, "", gameObject,2);
                            }

                        }



                    }
                }
            }



            roundClock += Time.deltaTime;
        }
        else roundClock = 0;
    }
    /// <summary>
    /// 执行
    /// 若使用RoundAsset，那就true
    /// 否则就自己输入string
    /// </summary>
    float RunRoundAsset(bool isRoundAssetLine, string notRoundAssetString, GameObject gameObject,int debug = 0)
    {

        List<string> lister = new List<string>();
        if (isRoundAssetLine)
        {
            notRoundAssetString = MainControl.instance.BattleControl.roundSave[roundAssetLine];
            //Debug.Log(MainControl.instance.BattleControl.roundSave[roundAssetLine] + "    " + roundAssetLine);
        }

        MainControl.instance.MaxToOneSon(notRoundAssetString, lister);




        int animInt = 0;
        switch (lister[0])
        {
            case "Nest":
                Transform tr = transform.Find(lister[1]);
                if (tr == null)
                {
                    GameObject newNest;
                    if (transform.Find("Nest") == null)
                        newNest = Instantiate(new GameObject(), transform);
                    else
                        newNest = transform.Find("Nest").gameObject;

                    newNest.AddComponent<TweenRotationCorrection>();
                    newNest.name = lister[1];
                    tr = newNest.transform;
                }
                RunRoundAsset(false, notRoundAssetString.Substring(lister[0].Length + 2 + lister[1].Length), tr.gameObject);
                roundAssetLine -= 1;
                break;
            case "Summon":
                switch (lister[1])
                {
                    case "Bullet":
                        for (int i = 0; i < MainControl.instance.BattleControl.roundSave.Count; i++)
                        {
                            if (MainControl.instance.BattleControl.barrgeSetUpSave[i].Substring(0, lister[2].Length) == lister[2] &&
                                MainControl.instance.BattleControl.barrgeSetUpSave[i].Substring(lister[2].Length + 1, lister[1].Length) == lister[1])
                            {
                                BulletController bulleter = GetFromPool("bullet").GetComponent<BulletController>();
                                bulleter.SetBullet(lister[3], lister[4], lister[5], lister[6], lister[7], lister[8], MainControl.instance.BattleControl.barrgeSetUpSave[i]);
                                bulleter.transform.SetParent(gameObject.transform);
                                break;
                            }
                        }
                        break;
                    case "Board":

                        BoardController board = GetFromPool("board").GetComponent<BoardController>();
                        board.SetBoard(lister[2], lister[3], lister[4], lister[5]);
                        board.transform.SetParent(gameObject.transform);
                        break;

                    default:
                        break;
                }
                break;
            case "Color"://属性颜色
                switch (lister[1])
                {
                    case "Bullet":
                        gameObject.transform.Find(lister[2]).GetComponent<BulletController>().bulletColor = (BattleControl.BulletColor)Enum.Parse(typeof(BattleControl.BulletColor), lister[3]);
                        break;
                    case "Player":
                        //player.playerColor = (BattleControl.PlayerColor)Enum.Parse(typeof(BattleControl.PlayerColor), lister[2]);
                        player.ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[(int)(BattleControl.PlayerColor)Enum.Parse(typeof(BattleControl.PlayerColor), lister[2])], (BattleControl.PlayerColor)Enum.Parse(typeof(BattleControl.PlayerColor), lister[2]));
                        break;
                }
                break;

            case "Delete":
                switch (lister[1])
                {
                    case "Bullet":
                        gameObject.transform.Find(lister[2]).GetComponent<TweenRotationCorrection>().KillAllTween();
                        ReturnPool(gameObject.transform.Find(lister[2]).gameObject, "bullet");
                        break;
                    case "Board":
                        gameObject.transform.Find(lister[2]).GetComponent<TweenRotationCorrection>().KillAllTween();
                        ReturnPool(gameObject.transform.Find(lister[2]).gameObject, "board");
                        break;
                    case "Nest":
                        //Debug.Log(111111111111);
                        Transform objTr = gameObject.transform.Find(lister[2]);
                        
                        int forNum = objTr.childCount;
                        for (int i = 0; i < forNum; i++)//挨个放生
                        {
                            objTr.GetChild(0).GetComponent<TweenRotationCorrection>().KillAllTween();
                            string type = objTr.transform.tag;


                            ReturnPool(objTr.GetChild(0).gameObject, type);
                        }
                        
                        objTr.name = "Nest";

                        objTr.position = Vector3.zero;
                        objTr.GetComponent<TweenRotationCorrection>().euler = Vector3.zero;
                        objTr.GetComponent<TweenRotationCorrection>().KillAllTween();
                        objTr.localScale = Vector3.zero;

                        objTr.gameObject.SetActive(false);
                        break;
                    default:
                        break;
                }
                break;
            case "FX":

                AudioClip audioClipSave = null;

                switch (lister[1])
                {
                    case "Battle":
                        audioClipSave = MainControl.instance.AudioControl.fxClipBattle[int.Parse(lister[2])];
                        goto default;

                    case "UI":
                        audioClipSave = MainControl.instance.AudioControl.fxClipUI[int.Parse(lister[2])];
                        goto default;
                    default:
                        StartCoroutine(GetFx(float.Parse(lister[4]), audioClipSave, int.Parse(lister[3])));
                        break;
                }



                break;
            case "EndRound":
                isMyRound = true;
                round++;
                roundClock = 0;
                selentUIController.InRound(round);
                return 99999999;
            
            case "ChangeData":
                GameObject objcd;
                switch (lister[1])
                {
                    case "Frame":
                        objcd = GameObject.Find("MainFrame");
                        break;

                    case "FramePointLU":
                        objcd = GameObject.Find("MainFrame/Point1");
                        break;

                    case "FramePointRU":
                        objcd = GameObject.Find("MainFrame/Point0");
                        break;
                    case "FramePointLD":
                        objcd = GameObject.Find("MainFrame/Point2");
                        break;
                    case "FramePointRD":
                        objcd = GameObject.Find("MainFrame/Point3");
                        break;
                    case "Player":
                        objcd = GameObject.Find("Player");
                        break;
                    case "Curtain":
                        objcd = GameObject.Find("Curtain");
                        break;
                    default:
                        //Debug.LogWarning(notRoundAssetString);
                        objcd = gameObject.transform.Find(lister[1]).gameObject;
                        break;
                }


                switch (lister[2])
                {
                    case "LocalPosition":
                        objcd.transform.localPosition = (Vector3)MainControl.instance.StringVector2ToRealVector2(lister[3], objcd.transform.localPosition);
                        break;
                    case "LocalRotation":
                        TweenRotationCorrection objTween = objcd.GetComponent<TweenRotationCorrection>();
                        objTween.notLocal = false;
                        objTween.euler = new Vector3(0, 0, float.Parse(lister[2]));
                        break;
                    case "Position":
                        objcd.transform.position = (Vector3)MainControl.instance.StringVector2ToRealVector2(lister[3], objcd.transform.position);
                        break;
                    case "Rotation":
                        TweenRotationCorrection objTweener = objcd.GetComponent<TweenRotationCorrection>();
                        objTweener.notLocal = true;
                        objTweener.euler = new Vector3(0, 0, float.Parse(lister[2]));
                        break;
                    case "Scale":
                        objcd.transform.localScale = (Vector3)MainControl.instance.StringVector2ToRealVector2(lister[3], objcd.transform.localScale);
                        break;
                    case "ColorAnim"://显示颜色
                        SpriteRenderer spriteRenderer = objcd.GetComponent<SpriteRenderer>();
                        spriteRenderer.color = MainControl.instance.StringVector4ToRealColor(lister[3], spriteRenderer.color);
                        break;
                    case "Type"://挡板改模式
                        var objcder = objcd.GetComponent<BoardController>();
                        objcder.canMove = bool.Parse(lister[3]);
                        objcder.ChangeMove();

                        break;
                    case "Size":
                        SpriteRenderer spriteRendererer = objcd.GetComponent<SpriteRenderer>();
                        spriteRendererer.size = MainControl.instance.StringVector2ToRealVector2(lister[3], spriteRendererer.size);
                        break;
                    case "Mask":
                        BulletController bulletController = objcd.GetComponent<BulletController>();
                        bulletController.SetMask(lister[3]);
                        break;
                }
                break;
            case "LocalPosition":
                animInt = 1;
                goto default;
            case "LocalRotation":
                animInt = 2;
                goto default;
            case "Scale":
                animInt = 3;
                goto default;
            case "ColorAnim"://显示颜色
                animInt = 4;
                goto default;
            case "Position":
                animInt = 5;
                goto default;
            case "Rotation":
                animInt = 6;
                goto default;
            case "Size":
                animInt = 7;
                goto default;
            default://执行动画
                //Debug.Log(lister[0]);
                //Debug.Log(lister[1]);
                GameObject obj;
                switch (lister[1])
                {
                    case "Frame":
                        obj = GameObject.Find("MainFrame");
                        break;

                    case "FramePointLU":
                         obj = GameObject.Find("MainFrame/Point1");
                        break;

                        case "FramePointRU":
                        obj = GameObject.Find("MainFrame/Point0");
                        break;
                    case "FramePointLD":
                        obj = GameObject.Find("MainFrame/Point2");
                        break;
                    case "FramePointRD":
                        obj = GameObject.Find("MainFrame/Point3");
                        break;
                    case "Player":
                        obj = GameObject.Find("Player");
                        break;
                    case "Curtain":
                        obj = GameObject.Find("Curtain");
                        break;
                    default:
                        //Debug.LogWarning(notRoundAssetString + "    " + debug);
                        obj = gameObject.transform.Find(lister[1]).gameObject;
                        break;
                }
                //Debug.LogWarning(obj.GetComponent<TweenRotationCorrection>(),obj);
                TweenRotationCorrection tweenRotationCorrection = obj.GetComponent<TweenRotationCorrection>();
                switch (animInt)
                {
                    case 1://局部坐标
                        if (lister[2] != "null")
                            obj.transform.localPosition = MainControl.instance.StringVector2ToRealVector2(lister[2], obj.transform.localPosition);

                        tweenRotationCorrection.tweens.Add(
                            DOTween.To(() => obj.transform.localPosition, x => obj.transform.localPosition = x, (Vector3)MainControl.instance.StringVector2ToRealVector2(lister[3], obj.transform.localPosition), float.Parse(lister[5]))
                            .SetEase((Ease)System.Enum.Parse(typeof(Ease), lister[4]))
                            .SetLoops(int.Parse(lister[6]), (LoopType)System.Enum.Parse(typeof(LoopType), lister[7]))
                            .OnKill(() => LoadInheritList(lister[8], gameObject)));
                        break;

                    case 2://局部旋转
                        TweenRotationCorrection objTween = tweenRotationCorrection;
                        objTween.notLocal = false;
                        if (lister[2] != "null")
                            objTween.euler = new Vector3(0, 0, float.Parse(lister[2]));

                        objTween.tweens.Add(
                        DOTween.To(() => objTween.euler, x => objTween.euler = x, new Vector3(0, 0, float.Parse(lister[3])), float.Parse(lister[5]))
                           .SetEase((Ease)System.Enum.Parse(typeof(Ease), lister[4]))
                            .SetLoops(int.Parse(lister[6]), (LoopType)System.Enum.Parse(typeof(LoopType), lister[7]))
                            .OnKill(() => LoadInheritList(lister[8], gameObject)));
                        break;

                    case 3://局部缩放

                        if (lister[2] != "null")
                            obj.transform.localScale = MainControl.instance.StringVector2ToRealVector2(lister[2], obj.transform.localScale);
                        
                        
                        tweenRotationCorrection.tweens.Add(
                        DOTween.To(() => obj.transform.localScale, x => obj.transform.localScale = x, (Vector3)MainControl.instance.StringVector2ToRealVector2(lister[3], obj.transform.localScale), float.Parse(lister[5]))
                            .SetEase((Ease)System.Enum.Parse(typeof(Ease), lister[4]))
                            .SetLoops(int.Parse(lister[6]), (LoopType)System.Enum.Parse(typeof(LoopType), lister[7]))
                            .OnKill(() => LoadInheritList(lister[8], gameObject)));
                        break;
                    case 4://显色
                        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                        if (lister[2] != "null")
                            spriteRenderer.color = MainControl.instance.StringVector4ToRealColor(lister[2], spriteRenderer.color);

                        tweenRotationCorrection.tweens.Add(
                        DOTween.To(() => spriteRenderer.color, x => spriteRenderer.color = x, MainControl.instance.StringVector4ToRealColor(lister[3], spriteRenderer.color), float.Parse(lister[5]))
                            .SetEase((Ease)System.Enum.Parse(typeof(Ease), lister[4]))
                            .SetLoops(int.Parse(lister[6]), (LoopType)System.Enum.Parse(typeof(LoopType), lister[7]))
                            .OnKill(() => LoadInheritList(lister[8], gameObject)));
                        break;
                    case 5://世界坐标
                        if (lister[2] != "null")
                            obj.transform.position = MainControl.instance.StringVector2ToRealVector2(lister[2], obj.transform.position);

                        tweenRotationCorrection.tweens.Add(
                            DOTween.To(() => obj.transform.position, x => obj.transform.position = x, (Vector3)MainControl.instance.StringVector2ToRealVector2(lister[3], obj.transform.position), float.Parse(lister[5]))
                            .SetEase((Ease)System.Enum.Parse(typeof(Ease), lister[4]))
                            .SetLoops(int.Parse(lister[6]), (LoopType)System.Enum.Parse(typeof(LoopType), lister[7]))
                            .OnKill(() => LoadInheritList(lister[8], gameObject)));
                        break;

                    case 6://世界旋转
                        TweenRotationCorrection objTweener = tweenRotationCorrection;
                        if (lister[2] != "null")
                            objTweener.euler = new Vector3(0, 0, float.Parse(lister[2]));
                        objTweener.notLocal = true;
                        objTweener.tweens.Add(
                        DOTween.To(() => objTweener.euler, x => objTweener.euler = x, new Vector3(0, 0, float.Parse(lister[3])), float.Parse(lister[5]))
                           .SetEase((Ease)System.Enum.Parse(typeof(Ease), lister[4]))
                            .SetLoops(int.Parse(lister[6]), (LoopType)System.Enum.Parse(typeof(LoopType), lister[7]))
                            .OnKill(() => LoadInheritList(lister[8], gameObject)));
                        break;
                    case 7://精灵尺寸
                        SpriteRenderer spriteRendererer = obj.GetComponent<SpriteRenderer>();
                        if (lister[2] != "null")
                            spriteRendererer.size = MainControl.instance.StringVector2ToRealVector2(lister[2], spriteRendererer.size);

                        tweenRotationCorrection.tweens.Add(
                        DOTween.To(() => spriteRendererer.size, x => spriteRendererer.size = x, MainControl.instance.StringVector2ToRealVector2(lister[3], spriteRendererer.size), float.Parse(lister[5]))
                            .SetEase((Ease)System.Enum.Parse(typeof(Ease), lister[4]))
                            .SetLoops(int.Parse(lister[6]), (LoopType)System.Enum.Parse(typeof(LoopType), lister[7]))
                            .OnKill(() => LoadInheritList(lister[8], gameObject)));
                        break;
                }
                break;
        }
        roundAssetLine += 1;

        return MainControl.instance.MaxToLastFloat(notRoundAssetString);
    }
    /// <summary>
    /// 执行继承动画。
    /// </summary>
    void LoadInheritList(string name, GameObject gameObj)
    {
        switch (name)
        {
            case "null":
                break;
            default:
                //Debug.LogWarning(name);
                //Debug.LogWarning(roundAssetLine);

                for (int i = 0; i < inheritList.Count; i++)
                {
                    string asset = inheritList[i];
                    string save = "";
                    for (int j = asset.Length - 2; j > 0; j--)
                    {
                        if (asset[j] != '\\')
                            save = asset[j] + save;
                        else
                            break;
                    }
                    if (save == name)
                    {
                        //Debug.Log(save + " / " + roundAssetLine);
                        string returnText = inheritList[i];

                        if (returnText.Substring(0, 6) != "Delete")
                        {
                            RunRoundAsset(false, returnText, gameObject);
                            roundAssetLine -= 1;

                        }
                        else
                        {
                            GameObject returnObj = gameObj.transform.Find(MainControl.instance.MaxToOneSon(returnText.Substring(7))).gameObject;
                            DOTween.Kill(returnObj);
                            ReturnPool(returnObj, returnObj.tag);
                        }
                        inheritList.RemoveAt(i);
                        break;
                    }

                }
                break;
        }
    }
    /// <summary>
    /// 进入敌方回合
    /// </summary>
    public void OutYourRound()
    {
        isMyRound = false;
        roundClock = 0;
        roundAssetLine = 0;
        roundAssetTime = -1;
    }

    //-----对象池部分-----

    /// <summary>
    /// 初始化/填充对象池
    /// type:Bullet Board
    /// </summary>
    public void FillPool(string type)
    {
        for (int i = 0; i < poolCount; i++)
        {
            switch (type)
            {
                case "bullet":
                    goto default;
                default:

                    var newBullet = Instantiate(Resources.Load<GameObject>("Template/Bullet Template"), transform);
                    ReturnPool(newBullet, "bullet");
                    break;

                case "board":
                    var newBoard = Instantiate(Resources.Load<GameObject>("Template/Board Template"), transform);
                    ReturnPool(newBoard, "board");
                    break;
                
            }

        }
    }
    /// <summary>
    /// 返回对象池
    /// </summary>
    public void ReturnPool(GameObject gameObject,string type)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(transform);
        SwitchAvailble(type).Enqueue(gameObject);
    }
    /// <summary>
    /// 喜提对象 
    /// Bullet弹幕 Board挡板
    /// </summary>
    public GameObject GetFromPool(string type)
    {
        if (SwitchAvailble(type).Count == 0)
            FillPool(type);

        var fx = SwitchAvailble(type).Dequeue();

        fx.SetActive(true);
        return fx;
    }
    /// <summary>
    /// 切换
    /// </summary>
    Queue<GameObject> SwitchAvailble(string type)
    {
        switch (type)
        {
            case "bullet":
                return bulletAvailble;
            case "board":
                return boardAvailble;
            default:
                goto case "bullet";
        }
    }
}
