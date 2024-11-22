using System.Collections;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    ///     SpriteSplitController的子级控制器
    /// </summary>
    public class SpriteSplitFly : MonoBehaviour
    {
        private Rigidbody2D _rbody;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = transform.GetComponent<SpriteRenderer>();
            _rbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            _spriteRenderer.color -= new Color(0, 0, 0, Time.deltaTime);
        }

        private void OnEnable()
        {
            _rbody.AddForce(new Vector2(Random.Range(-1f, 1) * 1, 0), ForceMode2D.Impulse);
            StartCoroutine(ReturnThis());
        }

        private IEnumerator ReturnThis()
        {
            yield return new WaitForSeconds(1f);
            transform.parent.GetComponent<SpriteSplitController>().ReturnPool(gameObject);
        }
    }
}