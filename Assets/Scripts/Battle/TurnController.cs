using DG.Tweening;
using Log;
using MEC;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 回合控制，同时也是弹幕的对象池
/// </summary>
public class TurnController : MonoBehaviour
{
    public static TurnController instance;
    public int turn;
    public bool isMyTurn;

    private GameObject mainFrame;
    public List<int> poolCount;

    //public List<string> inheritList = new List<string>();
    public List<ObjectPool> objectPools = new List<ObjectPool>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GameObject saveBullet = GameObject.Find("SaveBullet");
        mainFrame = GameObject.Find("MainFrame");
        //OutYourTurn();
        //弹幕
        objectPools.Add(gameObject.AddComponent<ObjectPool>());
        objectPools[^1].parent = saveBullet.transform;
        objectPools[^1].count = poolCount[0];
        objectPools[^1].obj = Resources.Load<GameObject>("Template/Bullet Template");
        objectPools[^1].FillPool();

        //挡板
        objectPools.Add(gameObject.AddComponent<ObjectPool>());
        objectPools[^1].parent = saveBullet.transform;
        objectPools[^1].count = poolCount[1];
        objectPools[^1].obj = Resources.Load<GameObject>("Template/Board Template");
        objectPools[^1].FillPool();
    }

    public void KillIEnumerator()
    {
        Timing.KillCoroutines();
    }

    /// <summary>
    /// 进入敌方回合
    /// </summary>
    public void OutYourTurn()
    {
        isMyTurn = false;
        Timing.RunCoroutine(_TurnExecute(turn));
    }

    /// <summary>
    /// 回合执行系统
    /// 根据回合编号进行相应的执行
    /// </summary>
    private IEnumerator<float> _TurnExecute(int turn)
    {
        switch (turn)
        {
            case 0:
                DebugLogger.Log("这是个摆烂回合……也许吧。");
                MainControl.instance.battlePlayerController.ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], BattleControl.PlayerColor.blue,0,BattlePlayerController.PlayerDirEnum.down);
                var obj = objectPools[0].GetFromPool().GetComponent<BulletController>();
                obj.SetBullet(
                    "DemoBullet",
                    "CupCake",
                    40,
                    Resources.Load<Sprite>("Sprites/Bullet/CupCake"),
                    Vector2.zero,
                    1,
                    Vector2.zero,
                    new Vector3(1, -1.6f),
                    BattleControl.BulletColor.white,
                    SpriteMaskInteraction.None
                    );
                var obj2 = objectPools[0].GetFromPool().GetComponent<BulletController>();
                obj2.SetBullet(
                    "DemoBullet",
                    "CupCake",
                    40,
                    Resources.Load<Sprite>("Sprites/Bullet/CupCake"),
                    Vector2.zero,
                    1,
                    Vector2.zero,
                    new Vector3(-1, -1.6f),
                    BattleControl.BulletColor.white,
                    SpriteMaskInteraction.None
                    );
                for (int i = 600; i > 0; i--)
                {
                    DebugLogger.Log("你先别急，先摆" + MainControl.instance.RandomStringColor() + i + "</color>秒");
                    yield return Timing.WaitForSeconds(1f);
                }


                objectPools[0].ReturnPool(obj.gameObject);

                objectPools[0].ReturnPool(obj2.gameObject);
                break;

            case 1://示例回合
                DebugLogger.Log("这是一个示例回合");
                yield return Timing.WaitForSeconds(0.5f);
                DebugLogger.Log("请注意查看控制台发出的Debug文本介绍");
                yield return Timing.WaitForSeconds(1.5f);

                DebugLogger.Log("战斗框缩放：更改四个点的坐标");
                mainFrame.transform.GetChild(0).DOLocalMoveX(1.4f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(3).DOLocalMoveX(1.4f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(1).DOLocalMoveX(-1.4f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(2).DOLocalMoveX(-1.4f, 0.5f).SetEase(Ease.InOutSine);

                yield return Timing.WaitForSeconds(1);

                DebugLogger.Log("通过更改点坐标实现的战斗框轴点旋转");
                for (int i = 0; i < 4; i++)
                {
                    mainFrame.transform.GetChild(0).DOLocalMove(mainFrame.transform.GetChild(3).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(1).DOLocalMove(mainFrame.transform.GetChild(0).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(2).DOLocalMove(mainFrame.transform.GetChild(1).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(3).DOLocalMove(mainFrame.transform.GetChild(2).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    yield return Timing.WaitForSeconds(0.5f);
                }

                DebugLogger.Log("简单嵌套弹幕编写示例");
                for (int i = 0; i < 5 * 20; i++)
                {
                    Timing.RunCoroutine(_TurnNest(Nest.simpleNestBullet));
                    yield return Timing.WaitForSeconds(0.2f);
                }

                DebugLogger.Log("战斗框缩放回初始坐标以结束回合");
                yield return Timing.WaitForSeconds(1f);
                mainFrame.transform.GetChild(0).DOLocalMoveX(5.93f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(3).DOLocalMoveX(5.93f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(1).DOLocalMoveX(-5.93f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(2).DOLocalMoveX(-5.93f, 0.5f).SetEase(Ease.InOutSine);
                yield return Timing.WaitForSeconds(0.5f);

                break;
        }

        this.turn++;
        MainControl.instance.selectUIController.InTurn();
        yield return 0;
    }

    /// <summary>
    /// 回合嵌套
    /// 首先在枚举Nest中定义嵌套名称，然后在此编写嵌套内容
    /// 用于重复复杂弹幕的嵌套使用
    /// </summary>
    private IEnumerator<float> _TurnNest(Nest nest)
    {
        switch (nest)
        {
            case Nest.simpleNestBullet:
                var obj = objectPools[0].GetFromPool().GetComponent<BulletController>();

                obj.SetBullet(
                    "DemoBullet",
                    "CupCake",
                    40,
                    Resources.Load<Sprite>("Sprites/Bullet/CupCake"),
                    Vector2.zero,
                    1,
                    Vector2.zero,
                    new Vector3(0, -3.35f),
                    BattleControl.BulletColor.white,
                    SpriteMaskInteraction.VisibleInsideMask
                    );

                obj.transform.localPosition += new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0);

                obj.transform.DOMoveY(0, 1).SetEase(Ease.OutSine).SetLoops(2, LoopType.Yoyo);

                obj.transform.DORotate(new Vector3(0, 0, 360), 2, RotateMode.WorldAxisAdd).SetEase(Ease.InOutSine);
                yield return Timing.WaitForSeconds(0.5f);

                obj.spriteRenderer.sortingOrder = 60;
                obj.SetMask(SpriteMaskInteraction.None);

                yield return Timing.WaitForSeconds(1f);

                obj.spriteRenderer.sortingOrder = 40;
                obj.SetMask(SpriteMaskInteraction.VisibleInsideMask);

                yield return Timing.WaitForSeconds(1f);

                objectPools[0].ReturnPool(obj.gameObject);

                break;
        }
    }

    private enum Nest
    {
        simpleNestBullet
    };
}