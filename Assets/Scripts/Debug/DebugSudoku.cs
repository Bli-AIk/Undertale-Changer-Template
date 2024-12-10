using UnityEngine;

namespace Debug
{
    public class DebugSudoku : MonoBehaviour
    {
        public bool change;
        public bool booooool;
        private BoxCollider2D _boxCollider2D;
        private SpriteRenderer _sprite;

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _boxCollider2D = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            if (booooool)
                _sprite.color = Color.yellow;
            else _sprite.color = Color.white;

            Changed();
        }

        public void Changed()
        {
            if (change)
            {
                _boxCollider2D.enabled = false;
                booooool = !booooool;
                for (var i = 0; i < 4; i++)
                {
                    var dir = i switch
                    {
                        0 => Vector2.left,
                        1 => Vector2.up,
                        2 => Vector2.down,
                        3 => Vector2.right,
                        _ => Vector2.zero
                    };
                    var ray = new Ray2D(transform.position, dir);
                    var info = Physics2D.Raycast(ray.origin, ray.direction, 5);
                    UCT.Other.Debug.DrawRay(ray.origin, ray.direction);
                    if (info.collider != null && info.collider.transform != transform)
                    {
                        var obj = info.collider.gameObject;
                        obj.GetComponent<DebugSudoku>().booooool = !obj.GetComponent<DebugSudoku>().booooool;
                    }
                }

                change = !change;
                _boxCollider2D.enabled = true;
            }
        }
    }
}