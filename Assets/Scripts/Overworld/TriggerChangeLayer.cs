using UnityEngine;

public class TriggerChangeLayer : MonoBehaviour
{
    private GameObject player;
    private SpriteRenderer spriteRenderer, spriteRendererP;
    public int upLayer = 5, downLayer = -2;

    [Header("Add 1 to follow the hierarchy of the parent object")]
    public bool followParentPlus;

    private void Start()
    {
        player = GameObject.Find("Player");
        spriteRenderer = transform.parent.GetComponent<SpriteRenderer>();
        if (followParentPlus)
            spriteRendererP = transform.parent.parent.GetComponent<SpriteRenderer>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject == player)
        {
            if (followParentPlus)
            {
                if (player.transform.position.y > transform.position.y)
                {
                    spriteRenderer.sortingOrder = spriteRendererP.sortingOrder + upLayer;
                }
                else
                    spriteRenderer.sortingOrder = spriteRendererP.sortingOrder + downLayer;
                return;
            }

            if (player.transform.position.y > transform.position.y)
            {
                spriteRenderer.sortingOrder = upLayer;
            }
            else
                spriteRenderer.sortingOrder = downLayer;
        }
    }
}
