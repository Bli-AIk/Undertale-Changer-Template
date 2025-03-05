using System;
using JetBrains.Annotations;
using UCT.Global.Audio;
using UCT.Global.Core;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    ///     怪物控制脚本
    ///     主要用于动画控制和存储ATK DEF
    /// </summary>
    public class EnemiesController : MonoBehaviour
    {
        private static readonly int Hit = Animator.StringToHash("Hit");
        public Animator anim;
        public int atk, def;
        public int hp, hpMax;

        [HideInInspector] public SpriteSplitController spriteSplitController;

        [CanBeNull] public Action[] OnOptions;
        public IEnemy Enemy { get; private set; }

        private void Start()
        {
            anim = GetComponent<Animator>();
            Enemy = GetComponent<IEnemy>();

            if (Enemy == null)
            {
                Other.Debug.LogError("_enemy 不应为空！");
            }
            else
            {
                OnOptions = Enemy.GetOptions();
            }

            spriteSplitController = transform.GetChild(0).GetComponent<SpriteSplitController>();
            spriteSplitController.enabled = false;
        }

        private void AnimCheckHit()
        {
            if (!anim.GetBool(Hit))
            {
                return;
            }

            AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipBattle);
            anim.SetBool(Hit, false);
        }

        private void AnimCheckDeath()
        {
            if (hp >= 0)
            {
                return;
            }

            Enemy.state = EnemyState.Dead;
            spriteSplitController.enabled = true;
        }
    }


    public enum EnemyState
    {
        Default,
        Spaced,
        Dead
    }
}