using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class StartController : MonoBehaviour
{
    TextMeshPro textUnder, text;
    bool textUnderOpen = false;
    float time = 0;
    GameObject title;
    int layer = 0;
    // Start is called before the first frame update
    void Start()
    {
        title = transform.Find("Title").gameObject;
        textUnder = transform.Find("Title/Text Message").GetComponent<TextMeshPro>();
        text = transform.Find("SafeText").GetComponent<TextMeshPro>();
        text.color = Color.clear;
        textUnder.color = Color.clear;
        AudioController.instance.GetFx(11, MainControl.instance.AudioControl.fxClipUI);
        text.text = MainControl.instance.OverwroldControl.safeText;
    }

    // Update is called once per frame
    void Update()
    {
        if (time < 5)
            time += Time.deltaTime;
        else if(!textUnderOpen)
        {
            textUnderOpen = true;
            textUnder.DOColor(Color.white, 0.5f).SetEase(Ease.Linear);
        }

        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
        {
            switch (layer)
            {
                case 0:
                    layer = -1;
                    title.transform.DOLocalMoveY(-12, 2).SetEase(Ease.InOutSine).OnKill(TextAnim);
                    break;

                case 1:

                    text.DOColor(Color.clear, 1).SetEase(Ease.Linear);
                    if (MainControl.instance.PlayerControl.playerName == "")
                    {
                        MainControl.instance.OutBlack("Rename", Color.black, false, 2f);
                    }
                    else
                    {
                        MainControl.instance.OutBlack("Menu", Color.black, false, 2f);
                    }
                    break;
            }
        }
    }
    void TextAnim()
    {
        text.DOColor(Color.white, 1).SetEase(Ease.Linear).OnKill(() => ChangeLayer(1));
    }

    void ChangeLayer(int lay)
    {
        layer = lay;
    }
}
