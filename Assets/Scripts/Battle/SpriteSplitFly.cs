using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Child controllers of SpriteSplitController
/// </summary>
public class SpriteSplitFly : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Rigidbody2D rbody;
    void Awake()
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
    IEnumerator ReturnThis()
    {
        yield return new WaitForSeconds(1f);
        transform.parent.GetComponent<SpriteSplitController>().ReturnPool(gameObject);
    }
}
