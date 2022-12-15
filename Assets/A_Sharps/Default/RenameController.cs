using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.Rendering;

public class RenameController : MonoBehaviour
{
    public List<TextMeshPro> tmps;
    public int selent;
    public bool selentMax;
    public string setName;
    public int mode;
    Tween animMove, animScale;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            tmps.Add(transform.GetChild(i).GetComponent<TextMeshPro>());
        }
        if (MainControl.instance.PlayerControl.playerName != "")
        {
            mode = 1;
        }
        else mode = 3;


    }
    string Alphabet(int selentNum)
    {
        string bet = "A B C D E F G\nH I J K L M N\nO P Q R S T U\nV W X Y Z\na b c d e f g\nh i j k l m n\no p q r s t u\nv w x y z";
        string final = "";
        for (int i = 0; i < bet.Length; i++)
        {
            if (i == selentNum * 2)
            {
                final += "<color=yellow>";
            }
            else if (i == selentNum * 2 + 1)
            {
                final += "</color>";
            }
            final += bet[i];
        }

        return final;
    }
    void Selentbet(int selentNum)
    {
        List<string> strings = new List<string>();
        /*
        if (!(selentNum >= 52 && selentNum <= 54))
            return;
        */
        int selenter = selentNum - 52;
        for (int i = 0; i < 3; i++)
        {
            if(i == selenter)
                strings.Add("<color=yellow>");
            else strings.Add("");
        }
        tmps[3].text = strings[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Rename1") + "</color> " +
strings[1] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Rename2") + "</color>  " +
strings[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Rename3") + "</color>";
        
    }
    // Update is called once per frame
    void Update()
    {
        if (MainControl.instance.OverwroldControl.isSetting)
            return;
        switch (mode)
        {
            case 1:
                bool breaker = false;
                if ((MainControl.instance.KeyArrowToControl(KeyCode.Z)) && setName.Length <= 6)
                {
                    if (selent < 52)
                    {
                        if (setName.Length < 6)
                            setName += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"[selent];
                    }
                    else
                        switch (selent)
                        {
                            case 52:
                                if (MainControl.instance.PlayerControl.playerName != "")
                                {
                                    MainControl.instance.OutBlack("Menu");
                                    mode = 0;
                                }
                                else
                                    mode = 3;
                                break;
                            case 53:
                                if (setName.Length > 0)
                                    setName = setName.Substring(0, setName.Length - 1);
                                break;
                            case 54:
                                if (setName != "")
                                {
                                    selent = 0;
                                    mode = 2;
                                    List<string> list = MainControl.instance.ScreenMaxToAllSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "RenameSp");
                                    tmps[0].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Rename");
                                    if (MainControl.instance.OverwroldControl.textWidth)
                                        tmps[3].text = "<size=0>wwww</size><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "No") + "</color>    " + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Yes");
                                    else
                                        tmps[3].text = "<size=2><color=#00000000>wwww</color></size><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "No") + "</color>    <color=#00000000><size=5>wwwwwwwww</size></color>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Yes");
                                    selentMax = true;
                                    foreach (var item in list)
                                    {
                                        List<string> lister = new List<string>();
                                        MainControl.instance.MaxToOneSon(item+'|', lister, '|');
                                        if ((lister[0] == MainControl.instance.UppercaseToLowercase(setName) && !bool.Parse(lister[2]))|| (lister[0] == setName && bool.Parse(lister[2]))) 
                                        {
                                            if (lister[3] == "</gaster>")
                                                Application.Quit();
                                            else
                                                tmps[0].text = lister[3];
                                            if (!bool.Parse(lister[1]))
                                            {
                                                selentMax = false;
                                                if (MainControl.instance.OverwroldControl.textWidth)
                                                    tmps[3].text = "<size=0>wwww</size><color=yellow>"+ MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "GoBack") + "</color>";
                                            else
                                                    tmps[3].text = "<size=2><color=#00000000>wwww</color></size><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "GoBack") + "</color>";

                                            }


                                            break;
                                        }
                                    }

                                    animMove = DOTween.To(() => tmps[1].transform.localPosition, x => tmps[1].transform.localPosition = x, new Vector3(26.95f, -7.85f), 5).SetEase(Ease.Linear);
                                    animScale = DOTween.To(() => tmps[1].transform.localScale, x => tmps[1].transform.localScale = x, Vector3.one * 3, 5).SetEase(Ease.Linear);
                                    tmps[1].GetComponent<DynamicTMP>().mode = 2;
                                    tmps[2].text = "";
                                    tmps[4].text = "";
                                    tmps[5].text = "";
                                    breaker = true;
                                }
                                break;
                        }
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                {
                    if (setName.Length > 0)
                        setName = setName.Substring(0, setName.Length - 1);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.C, 1))
                {
                    setName = MainControl.instance.RandomName(Random.Range(1, 7));
                }
                if (breaker) break;
                if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                {
                    if (selent >= 31 && selent <= 32)
                    {
                        selent -= 12;
                    }
                    else if (selent < 26 || (selent > 32 && selent < 52))
                        selent -= 7;
                    else if (selent >= 52 && selent <= 54)
                    {
                        switch (selent)
                        {
                            case 52:
                                selent = 47;
                                break;
                            case 53:
                                selent = 49;
                                break;
                            case 54:
                                selent = 45;
                                break;
                        }
                    }
                    else selent -= 5;

                    if (selent < 0)
                        selent = 54;
                    
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                {
                    if (selent >= 19 && selent <= 20)
                    {
                        selent += 12;
                    }
                    else if (selent < 21 || (selent > 25 && selent < 45))
                        selent += 7;
                    else if (selent >= 45)
                    {
                        if (selent <= 46)
                            selent = 54;
                        else if (selent <= 48)
                            selent = 52;
                        else if (selent <= 51)
                            selent = 53;
                        else switch (selent)
                            {
                                case 52:
                                    selent = 0;
                                    break;
                                case 53:
                                    selent = 2;
                                    break;
                                case 54:
                                    selent = 5;
                                    break;
                            }
                    }
                    else selent += 5;

                    if (selent > 54)
                        selent = 0;
                    
                }
                if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                {
                    selent -= 1;
                    if (selent < 0)
                        selent = 54;
                    
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
                {
                    selent += 1;
                    if (selent > 54)
                        selent = 0;


                }
                tmps[0].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Rename0");
                tmps[1].text = setName;
                tmps[2].text = Alphabet(selent);
                Selentbet(selent);
                tmps[4].text = "";
                tmps[5].text = "";
                break;
            case 2:
                if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                {
                    switch (selent)
                    {
                        case 0:
                            mode = 1;
                            animMove.Kill();
                            animScale.Kill();
                            tmps[1].transform.localPosition = new Vector3(8.95f, 0.6f);
                            tmps[1].transform.localScale = Vector3.one;
                            tmps[1].GetComponent<DynamicTMP>().mode = -1;
                            tmps[3].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Rename1") +
                                           MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Rename2") +
                                           MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Rename3");
                            break;
                        case 1:
                            mode = -1;
                            MainControl.instance.PlayerControl.playerName = setName;
                            GameObject.Find("BGM Source").transform.GetComponent<AudioSource>().Pause();
                            Volume v = GameObject.Find("Global Volume").transform.GetComponent<Volume>();
                            Volume v2 = GameObject.Find("Global Volume (1)").transform.GetComponent<Volume>();
                            
                            DOTween.To(() => v.weight, x => v.weight = x, 0, 5.5f).SetEase(Ease.Linear);
                            DOTween.To(() => v2.weight, x => v2.weight = x, 1, 5.5f).SetEase(Ease.Linear);

                            MainControl.instance.OutWhite("Menu");
                            break;
                    }

                }
                if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                {
                    mode = 1;
                    animMove.Kill();
                    animScale.Kill();
                    tmps[1].transform.localPosition = new Vector3(8.95f, 0.6f);
                    tmps[1].transform.localScale = Vector3.one;
                    tmps[1].GetComponent<DynamicTMP>().mode = -1;
                    tmps[3].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Rename1") +
                                   MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Rename2") +
                                   MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Rename3");
                }
                if (selentMax && (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow) || MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))) 
                {
                    if (selent == 0)
                    {
                        selent = 1;
                        if (MainControl.instance.OverwroldControl.textWidth)
                            tmps[3].text = "<size=0>wwww</size>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "No") + "    <color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Yes") + "</color>";
                        else
                            tmps[3].text = "<color=#00000000><size=2>wwww</size></color>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "No") + "    <color=#00000000><size=5>wwwwwwwww</size></color><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Yes") + "</color>";

                    }
                    else
                    {
                        selent = 0;
                        if (MainControl.instance.OverwroldControl.textWidth)
                            tmps[3].text = "<color=#00000000><size=0>wwww</size><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "No") + "</color>    <color=white>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Yes");
                        else
                            tmps[3].text = "<color=#00000000><size=2>wwww</size></color><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "No") + "</color>    <color=#00000000><size=5>wwwwwwwww</size></color>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Yes");

                    }
                }
                break;
            case 3:
                tmps[0].text = "";
                tmps[1].text = "";
                setName = "";
                tmps[2].text = "";
                tmps[3].text = "";
                tmps[4].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Teach");
                tmps[5].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "MenuUnder") + Application.version;
                if ((MainControl.instance.KeyArrowToControl(KeyCode.Z)) && setName.Length < 6)
                    mode = 1;
                    break;
            default:
                break;
        }
    }
}
