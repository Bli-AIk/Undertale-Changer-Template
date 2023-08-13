using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// Bubble control of enemy dialogue within combat
/// </summary>
public class DialogBubbleBehaviour : MonoBehaviour
{ 
    public Vector2 position, size;
    public bool isBackRight;
    public float backY;
    SpriteRenderer sprite, spriteBack;
    public TypeWritter typeWritter;
    public TextMeshPro tmp;
    private void Awake()
    {
        typeWritter = transform.Find("Text").GetComponent<TypeWritter>();
        tmp = transform.Find("Text").GetComponent<TextMeshPro>();
    }
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        spriteBack = transform.Find("DialogBubbleBack").GetComponent<SpriteRenderer>(); 

        PositionChange();
    }


    /// <summary>
    /// Change the size of bubbles and the situation of left and right arrows, assign values, and so on
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
