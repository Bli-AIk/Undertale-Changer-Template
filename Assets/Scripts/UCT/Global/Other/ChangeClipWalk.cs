using UCT.Global.Core;
using UCT.Overworld;
using UnityEngine;

namespace UCT.Global.Other
{
    /// <summary>
    /// 玩家触发后更改移动范围
    /// </summary>
    public class ChangeClipWalk : MonoBehaviour
    {
        [Header("新范围")]
        public Vector2 range;

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.transform.CompareTag("Player"))
            {
                PlayerBehaviour playerBehaviour = MainControl.instance.playerBehaviour;
                if (playerBehaviour != null)
                {
                    playerBehaviour.walk = range;
                }
            }
        }
    }
}