using TMPro;
using UnityEngine;

/// <summary>
/// In-combat enemy dialog bubble control
/// </summary>
public class DialogBubbleBehaviour : MonoBehaviour
{
    public Vector2 position, size;
    public bool isBackRight;
    public float backY;
    private SpriteRenderer sprite, spriteBack;
    public TypeWritter typeWritter;
    public TextMeshPro tmp;

    private void Awake()
    {
        typeWritter = transform.Find("Text").GetComponent<TypeWritter>();
        tmp = transform.Find("Text").GetComponent<TextMeshPro>();
    }

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        spriteBack = transform.Find("DialogBubbleBack").GetComponent<SpriteRenderer>();

        PositionChange();
    }

    /// <summary>
    /// Change the size of the bubbles and the left and right arrows to assign values and so on.
    /// </summary>
    public void PositionChange()
    {
        transform.localPosition = position;
        sprite.size = size;
        spriteBack.flipX = isBackRight;
        if (!isBackRight)
        {
            spriteBack.transform.localPosition = new Vector3(-0.2396f, backY, 0);
        }
        else
        {
            spriteBack.transform.localPosition = new Vector3(4.2396f + sprite.size.x - 4, backY, 0);
        }
    }
}
