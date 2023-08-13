using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// Control storytelling scenes (broadcast PPT)
/// </summary>
public class StorySceneController : MonoBehaviour
{
    public static StorySceneController instance;
    public List<Sprite> pics;
    SpriteRenderer spriteRenderer;
    TypeWritter typeWritter;
    TextMeshPro tmp;
    public GameObject mask;
    int picNum;
    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        typeWritter = GetComponent<TypeWritter>();
        spriteRenderer = transform.Find("Pic").GetComponent<SpriteRenderer>();
        tmp = transform.Find("Text").GetComponent<TextMeshPro>();
        mask = transform.Find("Mask").gameObject;

        typeWritter.TypeOpen(MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Text"), false, 0, 1, tmp, TypeWritter.TypeMode.CantZX);
    }
    private void Update()
    {
        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;
        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
        {
            typeWritter.TypeStop();
            tmp.text = "";
            MainControl.instance.OutBlack("Start", Color.black);
        }
    }
    public void Fade(int Num)
    {
        picNum = Num;
        spriteRenderer.DOColor(new Color(1, 1, 1, 0), 0.5f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo);
        Invoke(nameof(ChangePic), 0.5f);
    }
    void ChangePic()
    {
        spriteRenderer.sprite = pics[picNum];
    }
}
