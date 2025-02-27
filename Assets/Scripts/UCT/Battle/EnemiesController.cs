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

        private void Start()
        {
            anim = GetComponent<Animator>();
        }

        private void AnimHit()
        {
            if (!anim.GetBool(Hit))
            {
                return;
            }

            AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipBattle);
            anim.SetBool(Hit, false);
        }
    }
}