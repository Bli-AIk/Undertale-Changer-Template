using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 最初始场景（模板信息）的控制器
/// </summary>
public class StartController : MonoBehaviour
{
    private TextMeshPro textUnder, text;
    private bool textUnderOpen = false;
    private float time = 0;
    private GameObject title;
    private int layer = 0;

    private float afkTimer = 20;

    private void Start()
    {
        title = transform.Find("Title").gameObject;
        textUnder = transform.Find("Title/Text Message").GetComponent<TextMeshPro>();
        text = transform.Find("SafeText").GetComponent<TextMeshPro>();
        text.color = Color.clear;
        textUnder.color = Color.clear;
        AudioController.instance.GetFx(11, MainControl.instance.AudioControl.fxClipUI);
        text.text = MainControl.instance.OverworldControl.sceneTextsAsset;

        PlayerControl playerControl = SaveController.LoadData("Data" + MainControl.instance.datanumber);
    }

    private void Update()
    {
        if (time < 5)
            time += Time.deltaTime;
        else if (!textUnderOpen)
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
                    if (MainControl.instance.PlayerControl.playerName == "" || MainControl.instance.PlayerControl.playerName == null)
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
        if (Input.anyKeyDown)
        {
            afkTimer = 20;
            return;
        }
        if (afkTimer > 0)
            afkTimer -= Time.deltaTime;
        else
        {
            MainControl.instance.OutBlack("Story", Color.black);
            afkTimer = 10000000000;
        }
    }

    private void TextAnim()
    {
        text.DOColor(Color.white, 1).SetEase(Ease.Linear).OnKill(() => ChangeLayer(1));
    }

    private void ChangeLayer(int lay)
    {
        layer = lay;
    }
}