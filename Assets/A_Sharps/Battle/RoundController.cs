using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using MEC;
/// <summary>
/// 回合控制，同时也是弹幕的对象池
/// </summary>
public class RoundController : MonoBehaviour
{
    public static RoundController instance;
    public int round;
    public bool isMyRound;

    SelectUIController selectUIController;
    BattlePlayerController player;
    GameObject mainFrame;
    public int poolCount;
    public List<string> inheritList = new List<string>();
    public List<ObjectPool> objectPools = new List<ObjectPool>();

    private void Awake()
    {
        instance = this;
        //弹幕
        objectPools.Add(gameObject.AddComponent<ObjectPool>());
        objectPools[^1].obj = Resources.Load<GameObject>("Template/Bullet Template");
        objectPools[^1].FillPool();

        //挡板
        objectPools.Add(gameObject.AddComponent<ObjectPool>());
        objectPools[^1].obj = Resources.Load<GameObject>("Template/Board Template");
        objectPools[^1].FillPool();
    }
    // Start is called before the first frame update
    void Start()
    {
        selectUIController = GameObject.Find("SelectUI").GetComponent<SelectUIController>();
        player = GameObject.Find("Player").GetComponent<BattlePlayerController>();
        mainFrame = GameObject.Find("MainFrame");
        //OutYourRound();



    }

    /// <summary>
    /// 进入敌方回合
    /// </summary>
    public void OutYourRound()
    {
        isMyRound = false;

        Timing.RunCoroutine(_RoundExecute(round));
    }

    /// <summary>
    /// 回合执行系统
    /// 根据回合编号进行相应的执行
    /// </summary>
    IEnumerator<float> _RoundExecute(int round)
    {
        switch (round)
        {
            case 0://示例回合
                Debug.Log("这是一个示例回合");
                yield return Timing.WaitForSeconds(0.5f);
                Debug.Log("请注意查看控制台发出的Debug文本介绍");
                yield return Timing.WaitForSeconds(1.5f);

                Debug.Log("战斗框缩放：更改四个点的坐标");
                mainFrame.transform.GetChild(0).DOLocalMoveX(2.8f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(3).DOLocalMoveX(2.8f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(1).DOLocalMoveX(-2.8f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(2).DOLocalMoveX(-2.8f, 0.5f).SetEase(Ease.InOutSine);

                yield return Timing.WaitForSeconds(1);

                Debug.Log("通过更改点坐标实现的战斗框轴点旋转");
                for (int i = 0; i < 4; i++)
                {
                    mainFrame.transform.GetChild(0).DOLocalMove(mainFrame.transform.GetChild(3).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(1).DOLocalMove(mainFrame.transform.GetChild(0).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(2).DOLocalMove(mainFrame.transform.GetChild(1).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(3).DOLocalMove(mainFrame.transform.GetChild(2).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    yield return Timing.WaitForSeconds(0.5f);
                }

                //Debug.Log("简单弹幕编写示例");


                Debug.Log("简单嵌套弹幕编写示例");
                for (int i = 0; i < 5; i++)
                {
                    Timing.RunCoroutine(_RoundNest(Nest.simpleNestBullet));
                    yield return Timing.WaitForSeconds(0.2f);
                }

                Debug.Log("战斗框缩放回初始坐标以结束回合");
                yield return Timing.WaitForSeconds(1f);
                mainFrame.transform.GetChild(0).DOLocalMoveX(11.86f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(3).DOLocalMoveX(11.86f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(1).DOLocalMoveX(-11.86f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(2).DOLocalMoveX(-11.86f, 0.5f).SetEase(Ease.InOutSine);
                yield return Timing.WaitForSeconds(0.5f);
                break;
        }

        this.round++;
        selectUIController.InRound(this.round);
        yield return 0;

    }
    /// <summary>
    /// 回合嵌套
    /// 首先在枚举Nest中定义嵌套名称，然后在此编写嵌套内容
    /// 用于重复复杂弹幕的嵌套使用
    /// </summary>
    IEnumerator<float> _RoundNest(Nest nest)
    {
        switch (nest)
        {
            case Nest.simpleNestBullet:
                var obj = objectPools[0].GetFromPool().GetComponent<BulletController>();

                obj.SetBullet("DemoBullet", 50, Resources.Load<Sprite>("Sprites/CupCake"), SpriteMaskInteraction.VisibleInsideMask, BattleControl.BulletColor.white,
                    new Vector3(0, -3.35f), Vector3.zero, Vector3.one * 0.4f, new List<Vector2> { Vector2.zero }, new List<Vector2> { Vector2.zero }, new List<int> { 5 });
                obj.transform.localPosition += new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0);
                obj.transform.DOMoveY(0, 1).SetEase(Ease.OutSine).SetLoops(2, LoopType.Yoyo);
                DOTween.To(() => obj.tweenRotationCorrection.euler, x => obj.tweenRotationCorrection.euler = x, new Vector3(0, 0, 360), 2).SetEase(Ease.InOutSine);

                yield return Timing.WaitForSeconds(0.5f);
                obj.spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
                yield return Timing.WaitForSeconds(1f);
                obj.spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
           
                break;

        }
    }
    enum Nest 
    {
        simpleNestBullet
    };
}
