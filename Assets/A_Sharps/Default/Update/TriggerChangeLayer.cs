using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerChangeLayer : MonoBehaviour
{
	GameObject player;
	SpriteRenderer spriteRenderer, spriteRendererP;
	public int upLayer = 5, downLayer = -2;
    [Header("跟随父父物体的层级变化而加上1")]
	public bool followParentPlus;
	void Start()
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
			if(followParentPlus)
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
