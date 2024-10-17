using System.Collections;
using UnityEngine;

/// <summary>
/// SpriteSplitController的子级控制器
/// </summary>
public class SpriteSplitFly : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rbody;

    private void Awake()
    {
        spriteRenderer = transform.GetComponent<SpriteRenderer>();
        rbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        spriteRenderer.color -= new Color(0, 0, 0, Time.deltaTime);
    }

    private void OnEnable()
    {
        rbody.AddForce(new Vector2(Random.Range(-1f, 1) * 1, 0), ForceMode2D.Impulse);
        StartCoroutine(ReturnThis());
    }

    private IEnumerator ReturnThis()
    {
        yield return new WaitForSeconds(1f);
        transform.parent.GetComponent<SpriteSplitController>().ReturnPool(gameObject);
    }
}