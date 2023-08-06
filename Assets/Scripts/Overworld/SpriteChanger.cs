using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Overworld对话中更改Sprite
/// </summary>
public class SpriteChanger : MonoBehaviour
{
    public bool useImage;
    public bool haveBack;
    public List<Sprite> sprites, spritesSayBack;
    public string path;
    public float backFrame;
    public bool inOverWorld;

    TypeWritter typeWritter;
    TalkUIPositionChanger talkUIPositionChanger;
    SpriteRenderer sprite;
    Image spriteImage;

    int num;
    float clock;
    bool back;

    
    void Start()
    {
        clock = backFrame / 60f;
        if (inOverWorld)
            talkUIPositionChanger = GameObject.Find("Main Camera/TalkUI").GetComponent<TalkUIPositionChanger>();
        if (haveBack)
            typeWritter = GetComponent<TypeWritter>();

        if (path != "")
        {

            if (!useImage)
                sprite = transform.Find(path).GetComponent<SpriteRenderer>();
            else
                spriteImage = transform.Find(path).GetComponent<Image>();

        }
        else
        {

            if (!useImage)
                sprite = GetComponent<SpriteRenderer>();
            else
                spriteImage = GetComponent<Image>();
        }
    }
    private void Update()
    {
        if (haveBack && typeWritter.isTyping && !typeWritter.passText && !typeWritter.isStop)
        {
            if (clock > 0)
            {
                clock -= Time.deltaTime;
            }
            else if (num >= 0)
            {
                back = !back;
                ChangeImage(num, back);
                clock = backFrame / 60f;
            }
        }
        else
        {
            if (back)
            {
                back = false;
                ChangeImage(num, back);
            }
            clock = backFrame / 60f;
        }
        
    }
    public void ChangeImage(int i, bool back = false)
    {

        talkUIPositionChanger.haveHead = i >= 0;
        if (i >= 0)
        {
            Sprite spriter;
            if (!back)
                spriter = sprites[i];
            else
                spriter = spritesSayBack[i];
            if (!useImage)
            {
                sprite.sprite = spriter;
                sprite.color = Color.white;
            }
            else
            {
                spriteImage.sprite = spriter;
                spriteImage.rectTransform.sizeDelta = new Vector2(spriter.texture.width, spriter.texture.height);
                spriteImage.color = Color.white;
            }

        }
        else
        {

            if (!useImage)
            {
                sprite.sprite = null;
                sprite.color = Color.clear;

            }
            else
            {
                spriteImage.sprite = null;
                spriteImage.color = Color.clear;

            }

        }

        num = i;
    }

}
