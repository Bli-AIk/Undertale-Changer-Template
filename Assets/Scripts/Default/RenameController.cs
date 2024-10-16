using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class RenameController : MonoBehaviour
{
    public List<TextMeshPro> tmps;
    public int select;
    public bool selectMax;
    public string setName;
    public int mode;
    private Tween animMove, animScale;

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            tmps.Add(transform.GetChild(i).GetComponent<TextMeshPro>());
        }
        if (MainControl.instance.PlayerControl.playerName != "" && MainControl.instance.PlayerControl.playerName != null)
        {
            mode = 1;
        }
        else
        {
            mode = 3;
            MainControl.instance.PlayerControl.hp = 92;
            MainControl.instance.PlayerControl.hpMax = 92;
            MainControl.instance.PlayerControl.lv = 19;
            MainControl.instance.PlayerControl.gold = 1000;
            MainControl.instance.PlayerControl.wearArm = 10001;
            MainControl.instance.PlayerControl.wearArmor = 20001;
            MainControl.instance.PlayerControl.wearAtk = 1;
            MainControl.instance.PlayerControl.wearDef = 123;
            MainControl.instance.PlayerControl.saveScene = "Example-Corridor";

            MainControl.instance.PlayerControl.myItems = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0 };
        }
    }

    private string Alphabet(int selectNum)
    {
        string bet = "A B C D E F G\nH I J K L M N\nO P Q R S T U\nV W X Y Z\na b c d e f g\nh i j k l m n\no p q r s t u\nv w x y z";
        string final = "";
        for (int i = 0; i < bet.Length; i++)
        {
            if (i == selectNum * 2)
            {
                final += "<color=yellow>";
            }
            else if (i == selectNum * 2 + 1)
            {
                final += "</color>";
            }
            final += bet[i];
        }

        return final;
    }

    private void Selectbet(int selectNum)
    {
        List<string> strings = new List<string>();
        /*
        if (!(selectNum >= 52 && selectNum <= 54))
            return;
        */
        int selecter = selectNum - 52;
        for (int i = 0; i < 3; i++)
        {
            if (i == selecter)
                strings.Add("<color=yellow>");
            else strings.Add("");
        }
        tmps[3].text = strings[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Rename1") + "</color> " +
strings[1] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Rename2") + "</color>  " +
strings[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Rename3") + "</color>";
    }

    private void Update()
    {
        if (MainControl.instance.OverworldControl.isSetting)
            return;
        switch (mode)
        {
            case 1:
                bool breaker = false;
                if ((MainControl.instance.KeyArrowToControl(KeyCode.Z)) && setName.Length <= 6)
                {
                    if (select < 52)
                    {
                        if (setName.Length < 6)
                            setName += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"[select];
                    }
                    else
                        switch (select)
                        {
                            case 52:
                                if (MainControl.instance.PlayerControl.playerName != "" && MainControl.instance.PlayerControl.playerName != null)
                                {
                                    MainControl.instance.OutBlack("Menu", Color.black);
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
                                    select = 0;
                                    mode = 2;
                                    List<string> list = MainControl.instance.ScreenMaxToAllSon(MainControl.instance.OverworldControl.sceneTextsSave, "RenameSp");
                                    tmps[0].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Rename");
                                    if (MainControl.instance.OverworldControl.textWidth)
                                        tmps[3].text = "<size=0>wwww</size><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "No") +
                                                       "</color>    " + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Yes");
                                    else
                                        tmps[3].text = "<size=2><color=#00000000>wwww</color></size><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "No") +
                                                       "</color>    <color=#00000000><size=5>wwwwwwwww</size></color>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Yes");
                                    selectMax = true;
                                    foreach (var item in list)
                                    {
                                        List<string> lister = new List<string>();
                                        MainControl.instance.MaxToOneSon(item + '|', lister, '|');
                                        if ((lister[0] == MainControl.instance.UppercaseToLowercase(setName) && !bool.Parse(lister[2])) || (lister[0] == setName && bool.Parse(lister[2])))
                                        {
                                            if (lister[3] == "<gaster>")
                                                Application.Quit();
                                            else
                                                tmps[0].text = lister[3];
                                            if (!bool.Parse(lister[1]))
                                            {
                                                selectMax = false;
                                                if (MainControl.instance.OverworldControl.textWidth)
                                                    tmps[3].text = "<size=0>wwww</size><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "GoBack") + "</color>";
                                                else
                                                    tmps[3].text = "<size=2><color=#00000000>wwww</color></size><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "GoBack") + "</color>";
                                            }

                                            break;
                                        }
                                    }

                                    animMove = DOTween.To(() => tmps[1].transform.localPosition, x => tmps[1].transform.localPosition = x, new Vector3(26.95f, -7.85f), 5).SetEase(Ease.Linear);
                                    animScale = DOTween.To(() => tmps[1].transform.localScale, x => tmps[1].transform.localScale = x, Vector3.one * 3, 5).SetEase(Ease.Linear);
                                    tmps[1].GetComponent<DynamicTMP>().dynamicMode = OverworldControl.DynamicTMP.RandomShakeSingle;
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
                    setName = MainControl.instance.RandomName(UnityEngine.Random.Range(1, 7));
                }
                if (breaker) break;
                if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                {
                    if (select >= 31 && select <= 32)
                    {
                        select -= 12;
                    }
                    else if (select < 26 || (select > 32 && select < 52))
                        select -= 7;
                    else if (select >= 52 && select <= 54)
                    {
                        switch (select)
                        {
                            case 52:
                                select = 47;
                                break;

                            case 53:
                                select = 49;
                                break;

                            case 54:
                                select = 45;
                                break;
                        }
                    }
                    else select -= 5;

                    if (select < 0)
                        select = 54;
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                {
                    if (select >= 19 && select <= 20)
                    {
                        select += 12;
                    }
                    else if (select < 21 || (select > 25 && select < 45))
                        select += 7;
                    else if (select >= 45)
                    {
                        if (select <= 46)
                            select = 54;
                        else if (select <= 48)
                            select = 52;
                        else if (select <= 51)
                            select = 53;
                        else switch (select)
                            {
                                case 52:
                                    select = 0;
                                    break;

                                case 53:
                                    select = 2;
                                    break;

                                case 54:
                                    select = 5;
                                    break;
                            }
                    }
                    else select += 5;

                    if (select > 54)
                        select = 0;
                }
                if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                {
                    select -= 1;
                    if (select < 0)
                        select = 54;
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
                {
                    select += 1;
                    if (select > 54)
                        select = 0;
                }
                tmps[0].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Rename0");
                tmps[1].text = setName;
                tmps[2].text = Alphabet(select);
                Selectbet(select);
                tmps[4].text = "";
                tmps[5].text = "";
                break;

            case 2:
                if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                {
                    switch (select)
                    {
                        case 0:
                            mode = 1;
                            animMove.Kill();
                            animScale.Kill();
                            tmps[1].transform.localPosition = new Vector3(8.95f, 0.6f);
                            tmps[1].transform.localScale = Vector3.one;
                            tmps[1].GetComponent<DynamicTMP>().dynamicMode = 0;
                            tmps[3].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Rename1") +
                                           MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Rename2") +
                                           MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Rename3");
                            break;

                        case 1:
                            mode = -1;
                            MainControl.instance.PlayerControl.playerName = setName;
                            AudioController.instance.transform.GetComponent<AudioSource>().Pause();
                            Volume v2 = GameObject.Find("Global Volume (1)").transform.GetComponent<Volume>();

                            DOTween.To(() => v2.weight, x => v2.weight = x, 1, 5.5f).SetEase(Ease.Linear);

                            SaveController.SaveData(MainControl.instance.PlayerControl, "Data" + MainControl.instance.dataNum);
                            PlayerPrefs.SetInt("languagePack", MainControl.instance.languagePack);
                            PlayerPrefs.SetInt("dataNum", MainControl.instance.dataNum);
                            PlayerPrefs.SetInt("hdResolution", Convert.ToInt32(MainControl.instance.OverworldControl.hdResolution));
                            PlayerPrefs.SetInt("noSFX", Convert.ToInt32(MainControl.instance.OverworldControl.noSFX));
                            PlayerPrefs.SetInt("vsyncMode", Convert.ToInt32(MainControl.instance.OverworldControl.vsyncMode));

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
                    tmps[1].GetComponent<DynamicTMP>().dynamicMode = 0;
                    tmps[3].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Rename1") +
                                   MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Rename2") +
                                   MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Rename3");
                }
                if (selectMax && (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow) || MainControl.instance.KeyArrowToControl(KeyCode.RightArrow)))
                {
                    if (select == 0)
                    {
                        select = 1;
                        if (MainControl.instance.OverworldControl.textWidth)
                            tmps[3].text = "<size=0>wwww</size>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "No") +
                                           "    <color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Yes") + "</color>";
                        else
                            tmps[3].text = "<color=#00000000><size=2>wwww</size></color>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "No") +
                                           "    <color=#00000000><size=5>wwwwwwwww</size></color><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Yes") + "</color>";
                    }
                    else
                    {
                        select = 0;
                        if (MainControl.instance.OverworldControl.textWidth)
                            tmps[3].text = "<color=#00000000><size=0>wwww</size><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "No") +
                                           "</color>    <color=white>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Yes");
                        else
                            tmps[3].text = "<color=#00000000><size=2>wwww</size></color><color=yellow>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "No") +
                                           "</color>    <color=#00000000><size=5>wwwwwwwww</size></color>" + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Yes");
                    }
                }
                break;

            case 3:
                tmps[0].text = "";
                tmps[1].text = "";
                setName = "";
                tmps[2].text = "";
                tmps[3].text = "";
                tmps[4].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Teach");
                tmps[5].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "MenuUnder") + Application.version;
                if (MainControl.instance.KeyArrowToControl(KeyCode.Z) && setName.Length < 6)
                    mode = 1;
                break;

            default:
                break;
        }
    }
}
