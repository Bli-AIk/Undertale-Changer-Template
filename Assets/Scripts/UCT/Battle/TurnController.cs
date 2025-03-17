using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using UCT.Battle.MultiEnemiesConfigs;
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
            _yellowBulletPool.FillPool<YellowBulletController>();
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
                    var enemy = MainControl.Instance.selectUIController.enemiesControllers[0].Enemy;
                    Timing.RunCoroutine(enemy
                        ._EnemyTurns(enemy.TurnGenerator.GetNextValue(), _bulletPool, _boardPool));
                }
                else
                {
                    GetEnemiesTurn();
                }
            }

            yield return Timing.WaitUntilDone(
                Timing.RunCoroutine(MainControl.Instance.BattleControl.BattleConfig.Turn(turnNumber, _bulletPool)));

            turn++;
            MainControl.Instance.selectUIController.EnterPlayerTurn();
        }

        private void GetEnemiesTurn()
        {
            var multiEnemiesConfigs = GetAllImplementationsOf<IMultiEnemiesConfig>();
            var enemyNames = MainControl.Instance.selectUIController.enemiesControllers
                .Select(item => item.name).ToArray();
            foreach (var item in MainControl.Instance.selectUIController.enemiesControllers)
            {
                item.Enemy.TurnGenerator.GetNextValue();
            }

            var isMultiEnemiesTurn = false;
            foreach (var enemiesConfig in from config in multiEnemiesConfigs
                     where typeof(IMultiEnemiesConfig).IsAssignableFrom(config)
                     select (IMultiEnemiesConfig)Activator.CreateInstance(config))
            {
                var allContained = enemiesConfig.EnemyNames.All(enemyNames.Contains);
                if (!allContained)
                {
                    continue;
                }

                var indices = MainControl.Instance.selectUIController.enemiesControllers
                    .Select(item => item.Enemy.TurnGenerator.value).ToArray();

                if (enemiesConfig.validIndicesList.Any(arr => arr.SequenceEqual(indices)))
                {
                    Timing.RunCoroutine(enemiesConfig._EnemyTurns(indices, _bulletPool, _boardPool));
                    isMultiEnemiesTurn = true;
                }

                break;
            }

            if (!isMultiEnemiesTurn)
            {
                MainControl.Instance.selectUIController.enemiesControllers
                    .ToList()
                    .ForEach(item =>
                        Timing.RunCoroutine(item.Enemy._EnemyTurns(item.Enemy.TurnGenerator.value,
                            _bulletPool, _boardPool)));
            }
        }

        private static List<Type> GetAllImplementationsOf<T>()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .ToList();
        }

        public void GetYellowBullet(Vector3 playerPosition)
        {
            var obj = _yellowBulletPool.GetFromPool<YellowBulletController>();
            obj.transform.position = playerPosition + Vector3.up * 0.25f;
            obj.OnKill = () => _yellowBulletPool.ReturnPool(obj);
        }
    }
}