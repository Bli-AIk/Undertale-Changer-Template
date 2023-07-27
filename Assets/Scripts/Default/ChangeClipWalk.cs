using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            PlayerBehaviour playerBehaviour = collision.transform.GetComponent<PlayerBehaviour>();
            if (playerBehaviour != null)
            {
                playerBehaviour.walk = range;
            }
        }
    }

}
