using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// 控制Menu，sodayo)
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("玩家名-LV-时间-位置-具体选项-底部字")]
    public List<TextMeshPro> tmps;
    public int selent, selentMax;
    public int layer;
    CanvasController canvasController;
    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            tmps.Add(transform.GetChild(i).GetComponent<TextMeshPro>());
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        canvasController = GameObject.Find("Canvas").GetComponent<CanvasController>();
        layer = 0;
        LoadLayer0();


    }
    void LoadLayer0()
    {
        tmps[0].text = MainControl.instance.PlayerControl.playerName;
        tmps[1].text = "LV " + MainControl.instance.PlayerControl.lv;
        //tmps[2]在update内设置
        tmps[3].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "MenuSetPlace");

        List<string> list = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            if (i != selent)
                list.Add("");
            else list.Add("<color=yellow>");
        }
        tmps[4].text = list[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Menu0") + "</color> " + list[1] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Menu1") + "</color>\n" + list[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Menu2") + "</color> " + list[3] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Menu3") + "</color>";
        tmps[5].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "MenuUnder") + Application.version;
    }

    private void Update()
    {
        if (MainControl.instance.OverwroldControl.isDebug)
            tmps[0].text = MainControl.instance.PlayerControl.playerName;

        if (MainControl.instance.OverwroldControl.isSetting || MainControl.instance.OverwroldControl.pause)
            return;

        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
        {
            selent--;
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
        {
            selent -= 2;
        }
        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
        {
            selent++;
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
        {
            selent += 2;
        }
        if (selent < 0)
        {
            if (selent % 2 != 0)
                selent = selentMax;
            else
                selent = selentMax - 1;
        }
        if (selent > selentMax)
        {
            if (selent % 2 == 0)
                selent = 0;
            else selent = 1;

        }
        if (layer == 0)
        {
            if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow) || MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow) || MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
            {
                List<string> list = new List<string>();
                for (int i = 0; i < 4; i++)
                {
                    if (i != selent)
                        list.Add("");
                    else list.Add("<color=yellow>");
                }
                tmps[4].text = list[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Menu0") + "</color> " + list[1] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Menu1") + "</color>\n" + list[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Menu2") + "</color> " + list[3] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Menu3") + "</color>";
            }
            if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
            {
                switch (selent)
                {
                    case 0:
                        MainControl.instance.OutBlack("Corridor", true);
                        break;
                    case 1:
                        MainControl.instance.OutBlack("Rename", true);
                        break;
                    case 2:
                        canvasController.InSetting();
                        break;
                    case 3:
                        canvasController.settingLevel = 2;
                        goto case 2;
                }
            }
        }
          

    }
   
    void FixedUpdate()
    {
        tmps[2].text = GetTime((int)Time.time);
    }
    private string GetTime(int sec)
    {
        int shi;
        int fen;
        int miao;
        if (sec < 0)
            sec = 0;
        miao = sec % 60;
        sec = sec - miao;
        sec /= 60;
        fen = sec % 60;
        sec -= fen;
        shi = sec / 60;
        string shiS, fenS;
        if (shi < 10)
            shiS = "0" + shi;
        else shiS = "" + shi;
        if (fen < 10)
            fenS = "0" + fen;
        else fenS = "" + fen;

        return (shiS + ":" + fenS);
    }
}
