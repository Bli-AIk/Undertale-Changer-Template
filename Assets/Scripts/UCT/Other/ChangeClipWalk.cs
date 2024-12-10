using UCT.Global.Core;
using UnityEngine;

namespace UCT.Other
{
    /// <summary>
    ///     玩家触发后更改移动范围
    /// </summary>
    public class ChangeClipWalk : MonoBehaviour
    {
        [Header("新范围")] public Vector2 range;

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collision.transform.CompareTag("Player")) return;
            var playerBehaviour = MainControl.Instance.playerBehaviour;
            if (playerBehaviour) playerBehaviour.walk = range;
        }
    }
}