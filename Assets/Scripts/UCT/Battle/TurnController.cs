using System.Collections.Generic;
using MEC;
using UCT.Battle.BattleConfigs;
using UCT.Core;
using UnityEngine;

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
        private IBattleConfig _config;
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

            _config = new DemoBattle();
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

            yield return Timing.WaitUntilDone(Timing.RunCoroutine(_config.Turn(turnNumber, _bulletPool)));

            turn++;
            MainControl.Instance.selectUIController.EnterPlayerTurn();
        }

        public void GetYellowBullet(Vector3 playerPosition)
        {
            var obj = _yellowBulletPool.GetFromPool<YellowBulletController>();
            obj.transform.position = playerPosition + Vector3.up * 0.25f;
            obj.OnKill = () => _yellowBulletPool.ReturnPool(obj);
        }
    }
}