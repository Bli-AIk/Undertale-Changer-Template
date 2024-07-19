using UnityEngine;

/// <summary>
//// Changes movement range when triggered by the player.
/// </summary>
public class ChangeClipWalk : MonoBehaviour
{
    [Header("New scope")]
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
