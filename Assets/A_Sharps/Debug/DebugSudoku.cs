using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSudoku : MonoBehaviour
{
    public bool change;
    public bool booooool;
    SpriteRenderer sprite;
    BoxCollider2D BoxCollider2D;

    // Start is called before the first frame update
    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        BoxCollider2D = GetComponent<BoxCollider2D>();
    }
    public void Changed()
    {
        if (change)
        {
            BoxCollider2D.enabled = false;
            booooool = !booooool;
            for (int i = 0; i < 4; i++)
            {
                var dir = i switch
                {
                    0 => Vector2.left,
                    1 => Vector2.up,
                    2 => Vector2.down,
                    3 => Vector2.right,
                    _ => Vector2.zero,
                };
                Ray2D ray = new Ray2D(transform.position, dir);
                RaycastHit2D info = Physics2D.Raycast(ray.origin, ray.direction, 5);
                Debug.DrawRay(ray.origin, ray.direction);
                if (info.collider != null && info.collider.transform != transform)
                {
                    GameObject obj = info.collider.gameObject;
                    obj.GetComponent<DebugSudoku>().booooool = !obj.GetComponent<DebugSudoku>().booooool;
                }

            }
            change = !change;
            BoxCollider2D.enabled = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (booooool)
        {
            sprite.color = Color.yellow;
        }
        else sprite.color = Color.white;

        Changed();
    }
}
