using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MEC;
using UCT.Control;
using UCT.Core;
using UnityEngine;
// ReSharper disable UnusedType.Global

namespace UCT.Battle.MultiEnemiesConfigs
{
    public interface IMultiEnemiesConfig
    {
        string[] EnemyNames { get; }
        List<int[]> validIndicesList { get; }
        IEnumerator<float> _EnemyTurns(int[] indices, ObjectPool bulletPool, ObjectPool boardPool);
    }

    public class Npc1AndNpc2Config : IMultiEnemiesConfig
    {
        public string[] EnemyNames => new[] { "NPC1", "NPC2" };

        public List<int[]> validIndicesList => new()
        {
            new[] { 0, 0 },
            new[] { 0, 1 },
            new[] { 0, 2 },
            new[] { 1, 0 },
            new[] { 1, 1 },
            new[] { 1, 3 },
        };

        public IEnumerator<float> _EnemyTurns(int[] indices, ObjectPool bulletPool, ObjectPool boardPool)
        {
            switch (indices)
            {
                case var _ when indices.SequenceEqual(new[] { 0, 0 }):
                {
                    Debug.Log("这是NPC1和NPC2的复合回合");
                    for (var i = 0; i < 10; i++)
                    {
                        const string cupCake = "CupCake";

                        var obj = bulletPool.GetFromPool<BulletController>();
                        obj.SetBullet(cupCake, cupCake, new InitialTransform(new Vector3(0, -1.6f)),
                            (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);
                        obj.spriteRenderer.color = Color.clear;
                        obj.spriteRenderer.DOColor(Color.white, 0.2f);

                        yield return Timing.WaitForSeconds(0.2f);

                        obj.transform.DOLocalPath(
                            GenerateCirclePath(new Vector3(0, -1.6f), 1f, 36),
                            1f
                        ).SetEase(Ease.Linear);

                        yield return Timing.WaitForSeconds(1f);

                        obj.spriteRenderer.DOColor(Color.clear, 0.2f).OnKill(
                            () => bulletPool.ReturnPool(obj.gameObject, obj));

                        yield return Timing.WaitForSeconds(Random.Range(0.5f, 1.5f));
                    }

                    break;
                }
            }
        }

        private static Vector3[] GenerateCirclePath(Vector3 center, float radius, int points)
        {
            var path = new Vector3[points];
            for (var i = 0; i < points; i++)
            {
                var angle = i * (360f / points) * Mathf.Deg2Rad;
                path[i] = new Vector3(center.x + Mathf.Cos(angle) * radius, center.y + Mathf.Sin(angle) * radius, 0);
            }

            return path;
        }
    }
}