using UnityEngine;

namespace UCT.Overworld
{
    public class TriggerChangeLayer : MonoBehaviour
    {
        private GameObject _player;
        private SpriteRenderer _spriteRenderer, _spriteRendererP;
        public int upLayer = 5, downLayer = -2;

        [Header("跟随父父物体的层级变化而加上1")]
        public bool followParentPlus;

        private void Start()
        {
            _player = GameObject.Find("Player");
            _spriteRenderer = transform.parent.GetComponent<SpriteRenderer>();
            if (followParentPlus)
                _spriteRendererP = transform.parent.parent.GetComponent<SpriteRenderer>();
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject == _player)
            {
                if (followParentPlus)
                {
                    if (_player.transform.position.y > transform.position.y)
                    {
                        _spriteRenderer.sortingOrder = _spriteRendererP.sortingOrder + upLayer;
                    }
                    else
                        _spriteRenderer.sortingOrder = _spriteRendererP.sortingOrder + downLayer;
                    return;
                }

                if (_player.transform.position.y > transform.position.y)
                {
                    _spriteRenderer.sortingOrder = upLayer;
                }
                else
                    _spriteRenderer.sortingOrder = downLayer;
            }
        }
    }
}