using System.Collections.Generic;
using DG.Tweening;
using MEC;
using UCT.Control;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    /// 回合控制，同时也是弹幕的对象池
    /// </summary>
    public class TurnController : MonoBehaviour
    {
        public static TurnController Instance;
        public int turn;
        public bool isMyTurn;

        public List<int> poolCount;

        //public List<string> inheritList = new List<string>();
        public List<ObjectPool> objectPools = new List<ObjectPool>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            var saveBullet = GameObject.Find("SaveBullet");
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
        private IEnumerator<float> _TurnExecute(int turnNumber)
        {
            switch (turnNumber)
            {
                case 1:
                    Global.Other.Debug.Log("这是个摆烂回合……也许吧。");
           
                    var obj = objectPools[0].GetFromPool().GetComponent<BulletController>();
                    obj.SetBullet("CupCake", "CupCake", new Vector3(1, -1.6f), (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);

                    var obj2 = objectPools[0].GetFromPool().GetComponent<BulletController>();
                    obj2.SetBullet("CupCake", "CupCake", new Vector3(-1, -1.6f), (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);


                    for (var i = 600; i > 0; i--)
                    {
                        Global.Other.Debug.Log($"你先别急，先摆{TextProcessingService.RandomStringColor(i.ToString())}秒");
                        yield return Timing.WaitForSeconds(1f);
                    }

                    objectPools[0].ReturnPool(obj.gameObject);

                    objectPools[0].ReturnPool(obj2.gameObject);
                    break;

                case 0://示例回合
                    Global.Other.Debug.Log("这是一个示例回合");
                    yield return Timing.WaitForSeconds(0.5f);
                    Global.Other.Debug.Log("请注意查看控制台发出的Debug文本介绍");
                    yield return Timing.WaitForSeconds(1.5f);

                    Global.Other.Debug.Log("战斗框缩放：更改四个点的坐标");

                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[0], x => MainControl.Instance.mainBox.vertexPoints[0] = x, new Vector2(1.4f, MainControl.Instance.mainBox.vertexPoints[0].y), 0.5f).SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[1], x => MainControl.Instance.mainBox.vertexPoints[1] = x, new Vector2(1.4f, MainControl.Instance.mainBox.vertexPoints[1].y), 0.5f).SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[2], x => MainControl.Instance.mainBox.vertexPoints[2] = x, new Vector2(-1.4f, MainControl.Instance.mainBox.vertexPoints[2].y), 0.5f).SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[3], x => MainControl.Instance.mainBox.vertexPoints[3] = x, new Vector2(-1.4f, MainControl.Instance.mainBox.vertexPoints[3].y), 0.5f).SetEase(Ease.InOutSine);


                    yield return Timing.WaitForSeconds(1);

                    Global.Other.Debug.Log("通过更改点坐标实现的战斗框轴点旋转");
                    for (var i = 0; i < 4; i++)
                    {

                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[0], x => MainControl.Instance.mainBox.vertexPoints[0] = x, MainControl.Instance.mainBox.vertexPoints[3], 0.5f).SetEase(Ease.InOutSine);
                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[1], x => MainControl.Instance.mainBox.vertexPoints[1] = x, MainControl.Instance.mainBox.vertexPoints[0], 0.5f).SetEase(Ease.InOutSine);
                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[2], x => MainControl.Instance.mainBox.vertexPoints[2] = x, MainControl.Instance.mainBox.vertexPoints[1], 0.5f).SetEase(Ease.InOutSine);
                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[3], x => MainControl.Instance.mainBox.vertexPoints[3] = x, MainControl.Instance.mainBox.vertexPoints[2], 0.5f).SetEase(Ease.InOutSine);

                        yield return Timing.WaitForSeconds(0.5f);
                    }

                    Global.Other.Debug.Log("简单嵌套弹幕编写示例");
                    for (var i = 0; i < 5 * 20; i++)
                    {
                        Timing.RunCoroutine(_TurnNest(Nest.SimpleNestBullet));
                        yield return Timing.WaitForSeconds(0.2f);
                    }

                    Global.Other.Debug.Log("战斗框缩放回初始坐标以结束回合");
                    yield return Timing.WaitForSeconds(1f);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[0], x => MainControl.Instance.mainBox.vertexPoints[0] = x, new Vector2(5.93f, MainControl.Instance.mainBox.vertexPoints[0].y), 0.5f).SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[1], x => MainControl.Instance.mainBox.vertexPoints[1] = x, new Vector2(5.93f, MainControl.Instance.mainBox.vertexPoints[1].y), 0.5f).SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[2], x => MainControl.Instance.mainBox.vertexPoints[2] = x, new Vector2(-5.93f, MainControl.Instance.mainBox.vertexPoints[2].y), 0.5f).SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[3], x => MainControl.Instance.mainBox.vertexPoints[3] = x, new Vector2(-5.93f, MainControl.Instance.mainBox.vertexPoints[3].y), 0.5f).SetEase(Ease.InOutSine);
                    yield return Timing.WaitForSeconds(0.5f);

                    break;
            }

            turn++;
            MainControl.Instance.selectUIController.InTurn();
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
                case Nest.SimpleNestBullet:
                    var obj = objectPools[0].GetFromPool().GetComponent<BulletController>();

                    obj.SetBullet("CupCake", "CupCake", new Vector3(0, -3.35f), (BattleControl.BulletColor)Random.Range(0,3), SpriteMaskInteraction.VisibleInsideMask);

                    obj.transform.localPosition += new Vector3(Random.Range(-0.5f, 0.5f), 0);

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
            SimpleNestBullet
        }
    }
}