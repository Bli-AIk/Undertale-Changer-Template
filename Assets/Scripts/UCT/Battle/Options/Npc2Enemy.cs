using System;
using System.Collections.Generic;
using DG.Tweening;
using MEC;
using UCT.Control;
using UCT.Global.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Battle.Options
{
    public class Npc2Enemy : MonoBehaviour, IEnemy
    {
        private void Start()
        {
            TurnGenerator = new WeightedRandomTurnNumber(new HashSet<int> { 0, 1, 2, 3 });
        }

        public IEnemyTurnNumber TurnGenerator { get; set; }

        public Action[] GetOptions()
        {
            return new Action[]
            {
                () => Other.Debug.Log("NPC2选项1"),
                () => Other.Debug.Log("NPC2选项2"),
                () => Other.Debug.Log("NPC2选项3"),
                () => Other.Debug.Log("NPC2选项4")
            };
        }

        public IEnumerator<float> _EnemyTurns(List<ObjectPool> objectPools)
        {
            var index = TurnGenerator.GetNextValue();
            switch (index)
            {
                default:
                {
                    Other.Debug.Log($"这是NPC2的第{index}回合");
                    for (var i = 0; i < 10; i++)
                    {
                        const string cupCake = "CupCake";

                        var obj = objectPools[0].GetFromPool<BulletController>();
                        obj.SetBullet(cupCake, cupCake, new InitialTransform(new Vector3(1, -1.6f)),
                            (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);
                        obj.spriteRenderer.color = Color.clear;
                        obj.spriteRenderer.DOColor(Color.white, 0.2f);

                        var obj2 = objectPools[0].GetFromPool<BulletController>();
                        obj2.SetBullet(cupCake, cupCake, new InitialTransform(new Vector3(-1, -1.6f)),
                            (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);
                        obj2.spriteRenderer.color = Color.clear;
                        obj2.spriteRenderer.DOColor(Color.white, 0.2f);


                        yield return Timing.WaitForSeconds(Random.Range(0.5f, 1.5f));

                        obj.spriteRenderer.DOColor(Color.clear, 0.2f).OnKill(
                            () => objectPools[0].ReturnPool(obj.gameObject, obj));
                        
                        obj2.spriteRenderer.DOColor(Color.clear, 0.2f).OnKill(
                            () => objectPools[0].ReturnPool(obj2.gameObject, obj2));

                        yield return Timing.WaitForSeconds(Random.Range(0.5f, 1.5f));
                    }
                    break;
                }
            }
        }
    }
}