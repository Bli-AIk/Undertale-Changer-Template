using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using MEC;
/// <summary>
/// �غϿ��ƣ�ͬʱҲ�ǵ�Ļ�Ķ����
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
        //��Ļ
        objectPools.Add(gameObject.AddComponent<ObjectPool>());
        objectPools[^1].obj = Resources.Load<GameObject>("Template/Bullet Template");
        objectPools[^1].FillPool();

        //����
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
    /// ����з��غ�
    /// </summary>
    public void OutYourRound()
    {
        isMyRound = false;

        Timing.RunCoroutine(_RoundExecute(round));
    }

    /// <summary>
    /// �غ�ִ��ϵͳ
    /// ���ݻغϱ�Ž�����Ӧ��ִ��
    /// </summary>
    IEnumerator<float> _RoundExecute(int round)
    {
        switch (round)
        {
            case 0://ʾ���غ�
                Debug.Log("����һ��ʾ���غ�");
                yield return Timing.WaitForSeconds(0.5f);
                Debug.Log("��ע��鿴����̨������Debug�ı�����");
                yield return Timing.WaitForSeconds(1.5f);

                Debug.Log("ս�������ţ������ĸ��������");
                mainFrame.transform.GetChild(0).DOLocalMoveX(2.8f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(3).DOLocalMoveX(2.8f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(1).DOLocalMoveX(-2.8f, 0.5f).SetEase(Ease.InOutSine);
                mainFrame.transform.GetChild(2).DOLocalMoveX(-2.8f, 0.5f).SetEase(Ease.InOutSine);

                yield return Timing.WaitForSeconds(1);

                Debug.Log("ͨ�����ĵ�����ʵ�ֵ�ս���������ת");
                for (int i = 0; i < 4; i++)
                {
                    mainFrame.transform.GetChild(0).DOLocalMove(mainFrame.transform.GetChild(3).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(1).DOLocalMove(mainFrame.transform.GetChild(0).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(2).DOLocalMove(mainFrame.transform.GetChild(1).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    mainFrame.transform.GetChild(3).DOLocalMove(mainFrame.transform.GetChild(2).transform.localPosition, 0.5f).SetEase(Ease.InOutSine);
                    yield return Timing.WaitForSeconds(0.5f);
                }

                //Debug.Log("�򵥵�Ļ��дʾ��");


                Debug.Log("��Ƕ�׵�Ļ��дʾ��");
                for (int i = 0; i < 5; i++)
                {
                    Timing.RunCoroutine(_RoundNest(Nest.simpleNestBullet));
                    yield return Timing.WaitForSeconds(0.2f);
                }

                Debug.Log("ս�������Żس�ʼ�����Խ����غ�");
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
    /// �غ�Ƕ��
    /// ������ö��Nest�ж���Ƕ�����ƣ�Ȼ���ڴ˱�дǶ������
    /// �����ظ����ӵ�Ļ��Ƕ��ʹ��
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
