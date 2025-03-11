using System;
using System.Collections.Generic;
using DG.Tweening;
using MEC;
using UCT.Control;
using UCT.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Battle.Enemies
{
    public class Npc2Enemy : MonoBehaviour, IEnemy
    {
        private void Start()
        {
            TurnGenerator = new WeightedRandomTurnNumber(new HashSet<int> { 0, 1, 2, 3 });
        }

        public IEnemyTurnNumber TurnGenerator { get; set; }

        public Action[] GetActOptions()
        {
            return new Action[]
            {
                () => Debug.Log("NPC2选项1"),
                () => Debug.Log("NPC2选项2"),
                () =>
                {
                    Debug.Log("NPC2选项3");
                    state = EnemyState.CanSpace;
                },
                () => Debug.Log("NPC2选项4")
            };
        }

        public MercyType[] MercyTypes => new[] { MercyType.Mercy, MercyType.Flee, MercyType.ActLike };


        public Action[] GetActLikeOptions()
        {
            
            return new Action[]
            {
                () => Debug.Log("NPC2类Mercy选项1")
            };
        }

        public IEnumerator<float> _EnemyTurns(int index, ObjectPool bulletPool, ObjectPool boardPool)
        {
            if (state is not (EnemyState.Default or EnemyState.CanSpace))
            {
                yield break;
            }

            switch (index)
            {
                default:
                {
                    Debug.Log($"这是NPC2的第{index}回合");
                    for (var i = 0; i < 10; i++)
                    {
                        const string cupCake = "CupCake";

                        var obj = bulletPool.GetFromPool<BulletController>();
                        obj.SetBullet(cupCake, cupCake, new InitialTransform(new Vector3(1, -1.6f)),
                            (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);
                        obj.spriteRenderer.color = Color.clear;
                        obj.spriteRenderer.DOColor(Color.white, 0.2f);

                        var obj2 = bulletPool.GetFromPool<BulletController>();
                        obj2.SetBullet(cupCake, cupCake, new InitialTransform(new Vector3(-1, -1.6f)),
                            (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);
                        obj2.spriteRenderer.color = Color.clear;
                        obj2.spriteRenderer.DOColor(Color.white, 0.2f);


                        yield return Timing.WaitForSeconds(Random.Range(0.5f, 1.5f));

                        obj.spriteRenderer.DOColor(Color.clear, 0.2f).OnKill(
                            () => bulletPool.ReturnPool(obj.gameObject, obj));

                        obj2.spriteRenderer.DOColor(Color.clear, 0.2f).OnKill(
                            () => bulletPool.ReturnPool(obj2.gameObject, obj2));

                        yield return Timing.WaitForSeconds(Random.Range(0.5f, 1.5f));
                    }

                    break;
                }
            }
        }

        public EnemyState state { get; set; }
        public int exp => 130;
        public int gold => 85;
    }
}