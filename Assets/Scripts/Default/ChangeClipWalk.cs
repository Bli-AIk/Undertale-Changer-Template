using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ��Ҵ���������ƶ���Χ
/// </summary>
public class ChangeClipWalk : MonoBehaviour
{
    [Header("�·�Χ")]
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
