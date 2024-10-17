using UnityEngine;

namespace Debug
{
    public class DebugSudoku : MonoBehaviour
    {
        public bool change;
        public bool booooool;
        private SpriteRenderer _sprite;
        private BoxCollider2D _boxCollider2D;

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _boxCollider2D = GetComponent<BoxCollider2D>();
        }

        public void Changed()
        {
            if (change)
            {
                _boxCollider2D.enabled = false;
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
                    UCT.Global.Other.Debug.DrawRay(ray.origin, ray.direction);
                    if (info.collider != null && info.collider.transform != transform)
                    {
                        GameObject obj = info.collider.gameObject;
                        obj.GetComponent<DebugSudoku>().booooool = !obj.GetComponent<DebugSudoku>().booooool;
                    }
                }
                change = !change;
                _boxCollider2D.enabled = true;
            }
        }

        private void Update()
        {
            if (booooool)
            {
                _sprite.color = Color.yellow;
            }
            else _sprite.color = Color.white;

            Changed();
        }
    }
}