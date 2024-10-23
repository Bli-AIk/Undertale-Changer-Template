using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace UCT.Global.Scene
{
    public class RenameController : MonoBehaviour
    {
        // ReSharper disable once StringLiteralTypo
        [FormerlySerializedAs("tmps")] public List<TextMeshPro> texts;
        public int select;
        public bool selectMax;
        public string setName;
        public int mode;
        private Tween _animMove, _animScale;

        private void Start()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                texts.Add(transform.GetChild(i).GetComponent<TextMeshPro>());
            }
            if (!string.IsNullOrEmpty(MainControl.Instance.playerControl.playerName))
            {
                mode = 1;
            }
            else
            {
                mode = 3;
                MainControl.Instance.playerControl.hp = 92;
                MainControl.Instance.playerControl.hpMax = 92;
                MainControl.Instance.playerControl.lv = 19;
                MainControl.Instance.playerControl.gold = 1000;
                MainControl.Instance.playerControl.wearArm = 10001;
                MainControl.Instance.playerControl.wearArmor = 20001;
                MainControl.Instance.playerControl.wearAtk = 1;
                MainControl.Instance.playerControl.wearDef = 123;
                MainControl.Instance.playerControl.saveScene = "Example-Corridor";

                MainControl.Instance.playerControl.myItems = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
            }
        }

        /// <summary>
        /// 输出起名时用于选择的文字。
        /// </summary>
        private static string GenerateSelectableTextForRename(int selectNumber)
        {
            const string ALPHABET = "A B C D E F G\nH I J K L M N\nO P Q R S T U\nV W X Y Z\na b c d e f g\nh i j k l m n\no p q r s t u\nv w x y z";
            var final = "";
            for (var i = 0; i < ALPHABET.Length; i++)
            {
                if (i == selectNumber * 2)
                {
                    final += "<color=yellow>";
                }
                else if (i == selectNumber * 2 + 1)
                {
                    final += "</color>";
                }
                final += ALPHABET[i];
            }

            return final;
        }

        /// <summary>
        /// 根据给定的选择索引更新可选项的高亮状态.
        /// 高亮显示所选的选项为黄色.
        /// </summary>
        private void HighlightSelectedOptions(int selectNumber)
        {
            var strings = new List<string>();
            var selectId = selectNumber - 52;
            for (var i = 0; i < 3; i++)
            {
                strings.Add(i == selectId ? "<color=yellow>" : "");
            }
            texts[3].text = strings[0] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Rename1") + "</color> " +
                           strings[1] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Rename2") + "</color>  " +
                           strings[2] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Rename3") + "</color>";
        }

        private void Update()
        {
            if (MainControl.Instance.OverworldControl.isSetting) return;
            
            switch (mode)
            {
                case 1:
                    var breaker = false;
                    if ((GameUtilityService.KeyArrowToControl(KeyCode.Z)) && setName.Length <= 6)
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
                                    if (!string.IsNullOrEmpty(MainControl.Instance.playerControl.playerName))
                                    {
                                        GameUtilityService.FadeOutAndSwitchScene("Menu", Color.black);
                                        mode = 0;
                                    }
                                    else
                                        mode = 3;
                                    break;

                                case 53:
                                    if (setName.Length > 0)
                                        setName = setName[..^1];
                                    break;

                                case 54:
                                    if (setName != "")
                                    {
                                        select = 0;
                                        mode = 2;
                                        var list = TextProcessingService.GetAllChildStringsByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "RenameSp");
                                        texts[0].text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Rename");
                                        if (MainControl.Instance.OverworldControl.textWidth)
                                            texts[3].text = "<size=0>wwww</size><color=yellow>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                                           "</color>    " + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Yes");
                                        else
                                            texts[3].text = "<size=2><color=#00000000>wwww</color></size><color=yellow>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                                           "</color>    <color=#00000000><size=5>wwwwwwwww</size></color>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Yes");
                                        selectMax = true;
                                        foreach (var item in list)
                                        {
                                            var lister = new List<string>();
                                            TextProcessingService.SplitStringToListWithDelimiter(item + '|', lister, '|');
                                            if ((lister[0] != TextProcessingService.ConvertLettersCase(setName, true) ||
                                                 bool.Parse(lister[2])) &&
                                                (lister[0] != setName || !bool.Parse(lister[2]))) continue;
                                            if (lister[3] == "<gaster>")
                                                Application.Quit();
                                            else
                                                texts[0].text = lister[3];
                                            if (!bool.Parse(lister[1]))
                                            {
                                                selectMax = false;
                                                if (MainControl.Instance.OverworldControl.textWidth)
                                                    texts[3].text = "<size=0>wwww</size><color=yellow>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "GoBack") + "</color>";
                                                else
                                                    texts[3].text = "<size=2><color=#00000000>wwww</color></size><color=yellow>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "GoBack") + "</color>";
                                            }

                                            break;
                                        }

                                        _animMove = DOTween.To(() => texts[1].transform.localPosition, x => texts[1].transform.localPosition = x, new Vector3(26.95f, -7.85f), 5).SetEase(Ease.Linear);
                                        _animScale = DOTween.To(() => texts[1].transform.localScale, x => texts[1].transform.localScale = x, Vector3.one * 3, 5).SetEase(Ease.Linear);
                                        texts[1].GetComponent<DynamicTMP>().dynamicMode = OverworldControl.DynamicTMP.RandomShakeSingle;
                                        texts[2].text = "";
                                        texts[4].text = "";
                                        texts[5].text = "";
                                        breaker = true;
                                    }
                                    break;
                            }
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.X))
                    {
                        if (setName.Length > 0)
                            setName = setName[..^1];
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.C, 1))
                    {
                        setName = TextProcessingService.RandomString(Random.Range(1, 7));
                    }
                    if (breaker) break;
                    if (GameUtilityService.KeyArrowToControl(KeyCode.UpArrow))
                    {
                        switch (select)
                        {
                            case >= 31 and <= 32:
                                select -= 12;
                                break;
                            case < 26:
                            case > 32 and < 52:
                                select -= 7;
                                break;
                            case >= 52 and <= 54:
                                select = select switch
                                {
                                    52 => 47,
                                    53 => 49,
                                    54 => 45,
                                    _ => select
                                };

                                break;
                            default:
                                select -= 5;
                                break;
                        }

                        if (select < 0)
                            select = 54;
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.DownArrow))
                    {
                        switch (select)
                        {
                            case >= 19 and <= 20:
                                select += 12;
                                break;
                            case < 21 or > 25 and < 45:
                                select += 7;
                                break;
                            case >= 45:
                                select = select switch
                                {
                                    <= 46 => 54,
                                    <= 48 => 52,
                                    <= 51 => 53,
                                    _ => select switch
                                    {
                                        52 => 0,
                                        53 => 2,
                                        54 => 5,
                                        _ => select
                                    }
                                };
                                break;
                            default:
                                select += 5;
                                break;
                        }

                        if (select > 54)
                            select = 0;
                    }
                    if (GameUtilityService.KeyArrowToControl(KeyCode.LeftArrow))
                    {
                        select -= 1;
                        if (select < 0)
                            select = 54;
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.RightArrow))
                    {
                        select += 1;
                        if (select > 54)
                            select = 0;
                    }
                    texts[0].text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Rename0");
                    texts[1].text = setName;
                    texts[2].text = GenerateSelectableTextForRename(select);
                    HighlightSelectedOptions(select);
                    texts[4].text = "";
                    texts[5].text = "";
                    break;

                case 2:
                    if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                    {
                        switch (select)
                        {
                            case 0:
                                mode = 1;
                                _animMove.Kill();
                                _animScale.Kill();
                                texts[1].transform.localPosition = new Vector3(8.95f, 0.6f);
                                texts[1].transform.localScale = Vector3.one;
                                texts[1].GetComponent<DynamicTMP>().dynamicMode = 0;
                                texts[3].text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Rename1") +
                                               TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Rename2") +
                                               TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Rename3");
                                break;

                            case 1:
                                mode = -1;
                                MainControl.Instance.playerControl.playerName = setName;
                                AudioController.Instance.transform.GetComponent<AudioSource>().Pause();
                                //Volume v = GameObject.Find("Global Volume").transform.GetComponent<Volume>();
                                var v2 = GameObject.Find("Global Volume (1)").transform.GetComponent<UnityEngine.Rendering.Volume>();

                                //DOTween.To(() => v.weight, x => v.weight = x, 0, 5.5f).SetEase(Ease.Linear);
                                DOTween.To(() => v2.weight, x => v2.weight = x, 1, 5.5f).SetEase(Ease.Linear);

                                SaveController.SaveData(MainControl.Instance.playerControl, "Data" + MainControl.Instance.saveDataId);
                                PlayerPrefs.SetInt("languagePack", MainControl.Instance.languagePackId);
                                PlayerPrefs.SetInt("dataNumber", MainControl.Instance.saveDataId);
                                PlayerPrefs.SetInt("hdResolution", Convert.ToInt32(MainControl.Instance.OverworldControl.hdResolution));
                                PlayerPrefs.SetInt("noSFX", Convert.ToInt32(MainControl.Instance.OverworldControl.noSfx));
                                PlayerPrefs.SetInt("vsyncMode", Convert.ToInt32(MainControl.Instance.OverworldControl.vsyncMode));

                                GameUtilityService.FadeOutToWhiteAndSwitchScene("Menu");
                                break;
                        }
                    }
                    if (GameUtilityService.KeyArrowToControl(KeyCode.X))
                    {
                        mode = 1;
                        _animMove.Kill();
                        _animScale.Kill();
                        texts[1].transform.localPosition = new Vector3(8.95f, 0.6f);
                        texts[1].transform.localScale = Vector3.one;
                        texts[1].GetComponent<DynamicTMP>().dynamicMode = 0;
                        texts[3].text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Rename1") +
                                       TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Rename2") +
                                       TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Rename3");
                    }
                    if (selectMax && (GameUtilityService.KeyArrowToControl(KeyCode.LeftArrow) || GameUtilityService.KeyArrowToControl(KeyCode.RightArrow)))
                    {
                        if (select == 0)
                        {
                            select = 1;
                            if (MainControl.Instance.OverworldControl.textWidth)
                                texts[3].text = "<size=0>wwww</size>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                               "    <color=yellow>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Yes") + "</color>";
                            else
                                texts[3].text = "<color=#00000000><size=2>wwww</size></color>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                               "    <color=#00000000><size=5>wwwwwwwww</size></color><color=yellow>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Yes") + "</color>";
                        }
                        else
                        {
                            select = 0;
                            if (MainControl.Instance.OverworldControl.textWidth)
                                texts[3].text = "<color=#00000000><size=0>wwww</size><color=yellow>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                               "</color>    <color=white>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Yes");
                            else
                                texts[3].text = "<color=#00000000><size=2>wwww</size></color><color=yellow>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                               "</color>    <color=#00000000><size=5>wwwwwwwww</size></color>" + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Yes");
                        }
                    }
                    break;

                case 3:
                    texts[0].text = "";
                    texts[1].text = "";
                    setName = "";
                    texts[2].text = "";
                    texts[3].text = "";
                    texts[4].text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Teach");
                    texts[5].text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "MenuUnder") + Application.version;
                    if (GameUtilityService.KeyArrowToControl(KeyCode.Z) && setName.Length < 6)
                        mode = 1;
                    break;
            }
        }
    }
}