using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Change the movement range after the player triggers it
/// </summary>
public class ChangeClipWalk : MonoBehaviour
{
    [Header("New range")]
    public Vector2 range;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            //PlayerBehaviour playerBehaviour = collision.transform.GetComponent<PlayerBehaviour>();
            PlayerBehaviour playerBehaviour = MainControl.instance.playerBehaviour;
            if (playerBehaviour != null)
            {
                playerBehaviour.walk = range;
            }
        }
    }

}
