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
    public int select, selectMax = 3;
    public int layer;
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
        layer = 0;
        LoadLayer0();


    }
    void LoadLayer0()
    {
        tmps[0].text = MainControl.instance.PlayerControl.playerName;
        tmps[1].text = "LV " + MainControl.instance.PlayerControl.lv;
        //tmps[2]在update内设置
        tmps[3].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "MenuSetPlace");

        List<string> list = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            if (i != select)
                list.Add("");
            else list.Add("<color=yellow>");
        }
        tmps[4].text = list[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu0") + "</color> "
                     + list[1] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu1") + "</color>\n" 
                     + list[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu2") + "</color> " 
                     + list[3] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu3") + "</color>";

        tmps[5].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "MenuUnder") + Application.version;
    }

    private void Update()
    {
        if (MainControl.instance.OverworldControl.isDebug)
            tmps[0].text = MainControl.instance.PlayerControl.playerName;

        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;

        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
        {
            select--;
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
        {
            select -= 2;
        }
        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
        {
            select++;
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
        {
            select += 2;
        }
        if (select < 0)
        {
            if (select % 2 != 0)
                select = selectMax;
            else
                select = selectMax - 1;
        }
        if (select > selectMax)
        {
            if (select % 2 == 0)
                select = 0;
            else select = 1;

        }
        if (layer == 0)
        {
            if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow) 
            ||  MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow) || MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
            {
                List<string> list = new List<string>();
                for (int i = 0; i < 4; i++)
                {
                    if (i != select)
                        list.Add("");
                    else list.Add("<color=yellow>");
                }
                tmps[4].text = list[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu0") + "</color> "
                             + list[1] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu1") + "</color>\n"
                             + list[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu2") + "</color> "
                             + list[3] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu3") + "</color>";
            }
            if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
            {
                switch (select)
                {
                    case 0:
                        MainControl.instance.OutBlack("Example-Corridor", Color.black, true);
                        break;
                    case 1:
                        MainControl.instance.OutBlack("Rename", Color.black, true);
                        break;
                    case 2:
                        CanvasController.instance.InSetting();
                        break;
                    case 3:
                        CanvasController.instance.settingLevel = 2;
                        goto case 2;
                }
            }
        }
          

    }
   
    void FixedUpdate()
    {
        tmps[2].text = GetTime((int)Time.time);
    }
    private string GetTime(int second)
    {
        int hr;
        int min;
        int sec;
        if (second < 0)
            second = 0;
        sec = second % 60;
        second = second - sec;
        second /= 60;
        min = second % 60;
        second -= min;
        hr = second / 60;
        string shiS, fenS;
        if (hr < 10)
            shiS = "0" + hr;
        else shiS = "" + hr;
        if (min < 10)
            fenS = "0" + min;
        else fenS = "" + min;

        return (shiS + ":" + fenS);
    }
}
