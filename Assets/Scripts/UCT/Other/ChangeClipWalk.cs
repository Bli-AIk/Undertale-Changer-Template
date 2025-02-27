using UCT.Global.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Other
{
    /// <summary>
    ///     玩家触发后更改移动范围
    /// </summary>
    public class ChangeClipWalk : MonoBehaviour
    {
        [FormerlySerializedAs("range")] [Header("新范围")]
        public Vector2 walkFxRange;

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collision.transform.CompareTag("Player"))
            {
                return;
            }

            var playerBehaviour = MainControl.OverworldPlayerBehaviour;
            if (playerBehaviour)
            {
                playerBehaviour.walkFxRange = walkFxRange;
            }
        }
    }
}