using DG.Tweening;
using Log;
using MEC;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �غϿ��ƣ�ͬʱҲ�ǵ�Ļ�Ķ����
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
        //��Ļ
        objectPools.Add(gameObject.AddComponent<ObjectPool>());
        objectPools[^1].parent = saveBullet.transform;
        objectPools[^1].count = poolCount[0];
        objectPools[^1].obj = Resources.Load<GameObject>("Template/Bullet Template");
        objectPools[^1].FillPool();

        //����
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
    /// ����з��غ�
    /// </summary>
    public void OutYourTurn()
    {
        isMyTurn = false;
        Timing.RunCoroutine(_TurnExecute(turn));
    }

    /// <summary>
    /// �غ�ִ��ϵͳ
    /// ���ݻغϱ�Ž�����Ӧ��ִ��
    /// </summary>
    private IEnumerator<float> _TurnExecute(int turn)
    {
        switch (turn)
        {
            case 0:
                DebugLogger.Log("���Ǹ����ûغϡ���Ҳ��ɡ�");
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
                    DebugLogger.Log("���ȱ𼱣��Ȱ�" + MainControl.instance.RandomStringColor() + i + "</color>��");
                    yield return Timing.WaitForSeconds(1f);
                }


                objectPools[0].ReturnPool(obj.gameObject);

                objectPools[0].ReturnPool(obj2.gameObject);
                break;

            case 1://ʾ���غ�
                DebugLogger.Log("����һ��ʾ���غ�");
                yield return Timing.WaitForSeconds(0.5f);
                DebugLogger.Log("��ע��鿴����̨������Debug�ı�����");
                yield return Timing.WaitForSeconds(1.5f);

                DebugLogger.Log("ս�������ţ������ĸ��������");
                mainFrame.transform.GetChild(0).DOLocalMoveX(1.4f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(3).DOLocalMoveX(1.4f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(1).DOLocalMoveX(-1.4f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(2).DOLocalMoveX(-1.4f, 0.5f).SetEase(Ease.InOutSine);

                yield return Timing.WaitForSeconds(1);

                DebugLogger.Log("ͨ�����ĵ�����ʵ�ֵ�ս���������ת");
                for (int i = 0; i < 4; i++)
                {
                    mainFrame.transform.GetChild(0).DOLocalMove(mainFrame.transform.GetChild(3).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(1).DOLocalMove(mainFrame.transform.GetChild(0).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(2).DOLocalMove(mainFrame.transform.GetChild(1).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(3).DOLocalMove(mainFrame.transform.GetChild(2).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    yield return Timing.WaitForSeconds(0.5f);
                }

                DebugLogger.Log("��Ƕ�׵�Ļ��дʾ��");
                for (int i = 0; i < 5 * 20; i++)
                {
                    Timing.RunCoroutine(_TurnNest(Nest.simpleNestBullet));
                    yield return Timing.WaitForSeconds(0.2f);
                }

                DebugLogger.Log("ս�������Żس�ʼ�����Խ����غ�");
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
    /// �غ�Ƕ��
    /// ������ö��Nest�ж���Ƕ�����ƣ�Ȼ���ڴ˱�дǶ������
    /// �����ظ����ӵ�Ļ��Ƕ��ʹ��
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