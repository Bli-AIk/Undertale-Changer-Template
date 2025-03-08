using System.Collections.Generic;
using DG.Tweening;
using MEC;
using UCT.Control;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Battle
{
    /// <summary>
    ///     回合控制，同时也是弹幕的对象池
    /// </summary>
    public class TurnController : MonoBehaviour
    {
        public int turn;
        public bool isMyTurn;

        [SerializeField] private int bulletPoolCount = 25;

        [SerializeField] private int boardPoolCount = 5;

        [SerializeField] private int yellowBulletPoolCount = 5;
        private readonly HashSet<int> _overlapTurns = new() { 0, 1 };

        private ObjectPool _boardPool;
        private ObjectPool _bulletPool;
        private ObjectPool _yellowBulletPool;
        public static TurnController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            var saveBullet = GameObject.Find("SaveBullet");

            SetUpObjectPools(saveBullet);
        }

        private void SetUpObjectPools(GameObject saveBullet)
        {
            _bulletPool = gameObject.AddComponent<ObjectPool>();
            _bulletPool.parent = saveBullet.transform;
            _bulletPool.count = bulletPoolCount;
            _bulletPool.poolObject = Resources.Load<GameObject>("Template/Bullet Template");
            _bulletPool.FillPool<BulletController>();

            _boardPool = gameObject.AddComponent<ObjectPool>();
            _boardPool.parent = saveBullet.transform;
            _boardPool.count = boardPoolCount;
            _boardPool.poolObject = Resources.Load<GameObject>("Template/Board Template");
            _boardPool.FillPool<BoardController>();

            _yellowBulletPool = gameObject.AddComponent<ObjectPool>();
            _yellowBulletPool.parent = saveBullet.transform;
            _yellowBulletPool.count = yellowBulletPoolCount;
            _yellowBulletPool.poolObject = Resources.Load<GameObject>("Template/YellowBullet Template");
            _yellowBulletPool.FillPool<BoardController>();
        }

        /// <summary>
        ///     进入敌方回合
        /// </summary>
        public void EnterEnemyTurn()
        {
            isMyTurn = false;
            Timing.RunCoroutine(_TurnExecute(turn));
        }

        /// <summary>
        ///     回合执行系统
        ///     根据回合编号进行相应的执行
        /// </summary>
        private IEnumerator<float> _TurnExecute(int turnNumber)
        {
            if (_overlapTurns.Contains(turnNumber))
            {
                if (MainControl.Instance.selectUIController.enemiesControllers.Count == 1)
                {
                    Timing.RunCoroutine(MainControl.Instance.selectUIController.enemiesControllers[0].Enemy
                        ._EnemyTurns(_bulletPool, _boardPool));
                }
                else
                {
                    //TODO: 检测是否有重叠定义
                    foreach (var item in MainControl.Instance.selectUIController.enemiesControllers)
                    {
                        //测试s
                        Timing.RunCoroutine(item.Enemy._EnemyTurns(_bulletPool, _boardPool));
                    }
                }
            }

            yield return Timing.WaitUntilDone(Timing.RunCoroutine(_TestTurns0(turnNumber)));

            turn++;
            MainControl.Instance.selectUIController.EnterPlayerTurn();
        }

        private IEnumerator<float> _TestTurns0(int turnNumber)
        {
            switch (turnNumber)
            {
                case 0: //示例回合
                {
                    Other.Debug.Log("这是一个示例回合");
                    yield return Timing.WaitForSeconds(0.5f);
                    Other.Debug.Log("请注意查看控制台发出的Debug文本介绍");
                    yield return Timing.WaitForSeconds(1.5f);

                    Other.Debug.Log("战斗框缩放：更改四个点的坐标");

                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[0],
                            x => MainControl.Instance.mainBox.vertexPoints[0] = x,
                            new Vector2(1.4f, MainControl.Instance.mainBox.vertexPoints[0].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[1],
                            x => MainControl.Instance.mainBox.vertexPoints[1] = x,
                            new Vector2(1.4f, MainControl.Instance.mainBox.vertexPoints[1].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[2],
                            x => MainControl.Instance.mainBox.vertexPoints[2] = x,
                            new Vector2(-1.4f, MainControl.Instance.mainBox.vertexPoints[2].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[3],
                            x => MainControl.Instance.mainBox.vertexPoints[3] = x,
                            new Vector2(-1.4f, MainControl.Instance.mainBox.vertexPoints[3].y), 0.5f)
                        .SetEase(Ease.InOutSine);


                    yield return Timing.WaitForSeconds(1);

                    Other.Debug.Log("通过更改点坐标实现的战斗框轴点旋转");
                    for (var i = 0; i < 4; i++)
                    {
                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[0],
                            x => MainControl.Instance.mainBox.vertexPoints[0] = x,
                            MainControl.Instance.mainBox.vertexPoints[3], 0.5f).SetEase(Ease.InOutSine);
                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[1],
                            x => MainControl.Instance.mainBox.vertexPoints[1] = x,
                            MainControl.Instance.mainBox.vertexPoints[0], 0.5f).SetEase(Ease.InOutSine);
                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[2],
                            x => MainControl.Instance.mainBox.vertexPoints[2] = x,
                            MainControl.Instance.mainBox.vertexPoints[1], 0.5f).SetEase(Ease.InOutSine);
                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[3],
                            x => MainControl.Instance.mainBox.vertexPoints[3] = x,
                            MainControl.Instance.mainBox.vertexPoints[2], 0.5f).SetEase(Ease.InOutSine);

                        yield return Timing.WaitForSeconds(0.5f);
                    }

                    Other.Debug.Log("简单嵌套弹幕编写示例");
                    for (var i = 0; i < 25; i++)
                    {
                        Timing.RunCoroutine(_SimpleNestBullet());
                        yield return Timing.WaitForSeconds(0.2f);
                    }

                    yield return Timing.WaitForSeconds(2f);

                    Other.Debug.Log("等待嵌套播放完毕的嵌套弹幕编写示例");
                    for (var i = 0; i < 3; i++)
                    {
                        yield return Timing.WaitUntilDone(Timing.RunCoroutine(_SimpleNestBullet()));
                    }

                    Other.Debug.Log("战斗框缩放回初始坐标以结束回合");
                    yield return Timing.WaitForSeconds(1f);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[0],
                            x => MainControl.Instance.mainBox.vertexPoints[0] = x,
                            new Vector2(5.93f, MainControl.Instance.mainBox.vertexPoints[0].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[1],
                            x => MainControl.Instance.mainBox.vertexPoints[1] = x,
                            new Vector2(5.93f, MainControl.Instance.mainBox.vertexPoints[1].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[2],
                            x => MainControl.Instance.mainBox.vertexPoints[2] = x,
                            new Vector2(-5.93f, MainControl.Instance.mainBox.vertexPoints[2].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[3],
                            x => MainControl.Instance.mainBox.vertexPoints[3] = x,
                            new Vector2(-5.93f, MainControl.Instance.mainBox.vertexPoints[3].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    yield return Timing.WaitForSeconds(0.5f);

                    break;
                }
                case 1:
                {
                    Other.Debug.Log("这是个测试回合。");
                    const string cupCake = "CupCake";

                    var obj = _bulletPool.GetFromPool<BulletController>();
                    obj.SetBullet(cupCake, cupCake, new InitialTransform(new Vector3(1, -1.6f)),
                        (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);

                    var obj2 = _bulletPool.GetFromPool<BulletController>();
                    obj2.SetBullet(cupCake, cupCake, new InitialTransform(new Vector3(-1, -1.6f)),
                        (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);


                    for (var i = 10; i > 0; i--)
                    {
                        if (i % 5 == 0)
                        {
                            Other.Debug.Log($"{TextProcessingService.RandomStringColor(i.ToString())}秒后结束本回合");
                        }

                        yield return Timing.WaitForSeconds(1f);
                    }

                    _bulletPool.ReturnPool(obj.gameObject, obj);

                    _bulletPool.ReturnPool(obj2.gameObject, obj2);
                    break;
                }
                default:
                    break;
            }
        }

        public void GetYellowBullet(Vector3 playerPosition)
        {
            var obj = _yellowBulletPool.GetFromPool<YellowBulletController>();
            obj.transform.position = playerPosition + Vector3.up * 0.25f;
            obj.OnKill = () => _yellowBulletPool.ReturnPool(obj);
        }

        private IEnumerator<float> _SimpleNestBullet()
        {
            var obj = _bulletPool.GetFromPool<BulletController>();
            const string cupCake = "CupCake";

            obj.SetBullet(cupCake, cupCake, new InitialTransform(new Vector3(0, -3.35f)),
                (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);

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

            _bulletPool.ReturnPool(obj.gameObject, obj);
        }
    }
}