using System;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Global.Scene
{
    public class RenameController : MonoBehaviour
    {
        private TextMeshPro _titleText, _nameText, _characterText, _selectText, _teachText, _textMessage;
        public int select;
        public bool selectMax;
        public string setName;
        public int mode;
        private Tween _animMove, _animScale;

        private enum SelectedAlphabet
        {
            Latin,
            Symbol,
            LatinExtended,
            Cyrillic,
            Greek,
            ChineseSimplified,
            ChineseTraditional,
            Japanese,
            Korean,
        }
        
        private static readonly List<string> AlphabetCapital = new()
        {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            "TEST1"
        };

        private static readonly List<string> AlphabetLowercase = new()
        {
            "abcdefghijklmnopqrstuvwxyz",
            "test1"
        };

        private const int AlphabetNum = 1;
        private void Start()
        {
            _titleText = transform.Find("TitleText").GetComponent<TextMeshPro>();
            _nameText = transform.Find("NameText").GetComponent<TextMeshPro>();
            _characterText = transform.Find("CharacterText").GetComponent<TextMeshPro>();
            _selectText = transform.Find("SelectText").GetComponent<TextMeshPro>();
            _teachText = transform.Find("TeachText").GetComponent<TextMeshPro>();
            _textMessage = transform.Find("TextMessage").GetComponent<TextMeshPro>();
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
        
        private void Update()
        {
            if (MainControl.Instance.OverworldControl.isSetting) return;
            
            switch (mode)
            {
                case 1:
                {
                    var breaker = false;
                    if (GameUtilityService.KeyArrowToControl(KeyCode.Z) && setName.Length <= 6)
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
                                {
                                    if (!string.IsNullOrEmpty(MainControl.Instance.playerControl.playerName))
                                    {
                                        GameUtilityService.FadeOutAndSwitchScene("Menu", Color.black);
                                        mode = 0;
                                    }
                                    else
                                        mode = 3;
                                    break;
                                }

                                case 53:
                                {
                                    if (setName.Length > 0)
                                        setName = setName[..^1];
                                    break;
                                }

                                case 54:
                                {
                                    if (setName != "")
                                    {
                                        select = 0;
                                        mode = 2;
                                        var list = TextProcessingService.GetAllChildStringsByPrefix(
                                            MainControl.Instance.OverworldControl.sceneTextsSave, "RenameSp");
                                        _titleText.text =
                                            TextProcessingService.GetFirstChildStringByPrefix(
                                                MainControl.Instance.OverworldControl.sceneTextsSave, "Rename");
                                        if (MainControl.Instance.OverworldControl.textWidth)
                                            _selectText.text =
                                                "<size=0>wwww</size><color=yellow>" +
                                                TextProcessingService.GetFirstChildStringByPrefix(
                                                    MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                                "</color>    " +
                                                TextProcessingService.GetFirstChildStringByPrefix(
                                                    MainControl.Instance.OverworldControl.sceneTextsSave, "Yes");
                                        else
                                            _selectText.text =
                                                "<size=2><color=#00000000>wwww</color></size><color=yellow>" +
                                                TextProcessingService.GetFirstChildStringByPrefix(
                                                    MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                                "</color>    <color=#00000000><size=5>wwwwwwwww</size></color>" +
                                                TextProcessingService.GetFirstChildStringByPrefix(
                                                    MainControl.Instance.OverworldControl.sceneTextsSave, "Yes");
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
                                                _titleText.text = lister[3];
                                            if (!bool.Parse(lister[1]))
                                            {
                                                selectMax = false;
                                                if (MainControl.Instance.OverworldControl.textWidth)
                                                    _selectText.text = "<size=0>wwww</size><color=yellow>" +
                                                                       TextProcessingService
                                                                           .GetFirstChildStringByPrefix(
                                                                               MainControl.Instance.OverworldControl
                                                                                   .sceneTextsSave, "GoBack") +
                                                                       "</color>";
                                                else
                                                    _selectText.text =
                                                        "<size=2><color=#00000000>wwww</color></size><color=yellow>" +
                                                        TextProcessingService.GetFirstChildStringByPrefix(
                                                            MainControl.Instance.OverworldControl.sceneTextsSave,
                                                            "GoBack") + "</color>";
                                            }

                                            break;
                                        }

                                        _animMove = DOTween.To(() => _nameText.transform.localPosition,
                                                x => _nameText.transform.localPosition = x, new Vector3(26.95f, -7.85f),
                                                5)
                                            .SetEase(Ease.Linear);
                                        _animScale = DOTween.To(() => _nameText.transform.localScale,
                                                x => _nameText.transform.localScale = x, Vector3.one * 3, 5)
                                            .SetEase(Ease.Linear);
                                        _nameText.GetComponent<DynamicTMP>().dynamicMode =
                                            OverworldControl.DynamicTMP.RandomShakeSingle;
                                        _characterText.text = "";
                                        _teachText.text = "";
                                        _textMessage.text = "";
                                        breaker = true;
                                    }
                                    break;
                                }
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
                            {
                                select -= 12;
                                break;
                            }
                            case < 26:
                            case > 32 and < 52:
                            {
                                select -= 7;
                                break;
                            }
                            case >= 52 and <= 54:
                            {
                                select = select switch
                                {
                                    52 => 47,
                                    53 => 49,
                                    54 => 45,
                                    _ => select
                                };

                                break;
                            }
                            default:
                            {
                                select -= 5;
                                break;
                            }
                        }

                        if (select < 0)
                            select = 54;
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.DownArrow))
                    {
                        switch (select)
                        {
                            case >= 19 and <= 20:
                            {
                                select += 12;
                                break;
                            }
                            case < 21 or > 25 and < 45:
                            {
                                select += 7;
                                break;
                            }
                            case >= 45:
                            {
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
                            }
                            default:
                            {
                                select += 5;
                                break;
                            }
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

                    _titleText.text =
                        TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.OverworldControl.sceneTextsSave, "Rename0");
                    _nameText.text = setName;
                    _characterText.text = GenerateSelectableTextForRename(select);
                    HighlightSelectedOptions(select);
                    _teachText.text = "";
                    _textMessage.text = "";
                    break;
                }

                case 2:
                {
                    if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                    {
                        switch (select)
                        {
                            case 0:
                            {
                                mode = 1;
                                _animMove.Kill();
                                _animScale.Kill();
                                _nameText.transform.localPosition = new Vector3(8.95f, 0.6f);
                                _nameText.transform.localScale = Vector3.one;
                                _nameText.GetComponent<DynamicTMP>().dynamicMode = 0;
                                _selectText.text =
                                    TextProcessingService.GetFirstChildStringByPrefix(
                                        MainControl.Instance.OverworldControl.sceneTextsSave, "Rename1") +
                                    TextProcessingService.GetFirstChildStringByPrefix(
                                        MainControl.Instance.OverworldControl.sceneTextsSave, "Rename2") +
                                    TextProcessingService.GetFirstChildStringByPrefix(
                                        MainControl.Instance.OverworldControl.sceneTextsSave, "Rename3");
                                break;
                            }

                            case 1:
                            {
                                mode = -1;
                                MainControl.Instance.playerControl.playerName = setName;
                                AudioController.Instance.transform.GetComponent<AudioSource>().Pause();
                                //Volume v = GameObject.Find("Global Volume").transform.GetComponent<Volume>();
                                var v2 = GameObject.Find("Global Volume (1)").transform
                                    .GetComponent<UnityEngine.Rendering.Volume>();

                                //DOTween.To(() => v.weight, x => v.weight = x, 0, 5.5f).SetEase(Ease.Linear);
                                DOTween.To(() => v2.weight, x => v2.weight = x, 1, 5.5f)
                                    .SetEase(Ease.Linear);

                                SaveController.SaveData(MainControl.Instance.playerControl,
                                    "Data" + MainControl.Instance.saveDataId);
                                PlayerPrefs.SetInt("languagePack", MainControl.Instance.languagePackId);
                                PlayerPrefs.SetInt("dataNumber", MainControl.Instance.saveDataId);
                                PlayerPrefs.SetInt("hdResolution",
                                    Convert.ToInt32(MainControl.Instance.OverworldControl.isUsingHDFrame));
                                PlayerPrefs.SetInt("noSFX", Convert.ToInt32(MainControl.Instance.OverworldControl.noSfx));
                                PlayerPrefs.SetInt("vsyncMode",
                                    Convert.ToInt32(MainControl.Instance.OverworldControl.vsyncMode));

                                GameUtilityService.FadeOutToWhiteAndSwitchScene("Menu");
                                break;
                            }
                        }
                    }
                    if (GameUtilityService.KeyArrowToControl(KeyCode.X))
                    {
                        mode = 1;
                        _animMove.Kill();
                        _animScale.Kill();
                        _nameText.transform.localPosition = new Vector3(8.95f, 0.6f);
                        _nameText.transform.localScale = Vector3.one;
                        _nameText.GetComponent<DynamicTMP>().dynamicMode = 0;
                        _selectText.text =
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.OverworldControl.sceneTextsSave, "Rename1") +
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.OverworldControl.sceneTextsSave, "Rename2") +
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.OverworldControl.sceneTextsSave, "Rename3");
                    }
                    if (selectMax && (GameUtilityService.KeyArrowToControl(KeyCode.LeftArrow) || GameUtilityService.KeyArrowToControl(KeyCode.RightArrow)))
                    {
                        if (select == 0)
                        {
                            select = 1;
                            if (MainControl.Instance.OverworldControl.textWidth)
                                _selectText.text =
                                    "<size=0>wwww</size>" +
                                    TextProcessingService.GetFirstChildStringByPrefix(
                                        MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                    "    <color=yellow>" +
                                    TextProcessingService.GetFirstChildStringByPrefix(
                                        MainControl.Instance.OverworldControl.sceneTextsSave, "Yes") + "</color>";
                            else
                                _selectText.text = "<color=#00000000><size=2>wwww</size></color>" +
                                                   TextProcessingService.GetFirstChildStringByPrefix(
                                                       MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                                   "    <color=#00000000><size=5>wwwwwwwww</size></color><color=yellow>" +
                                                   TextProcessingService.GetFirstChildStringByPrefix(
                                                       MainControl.Instance.OverworldControl.sceneTextsSave, "Yes") +
                                                   "</color>";
                        }
                        else
                        {
                            select = 0;
                            if (MainControl.Instance.OverworldControl.textWidth)
                                _selectText.text = "<color=#00000000><size=0>wwww</size><color=yellow>" +
                                                   TextProcessingService.GetFirstChildStringByPrefix(
                                                       MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                                   "</color>    <color=white>" +
                                                   TextProcessingService.GetFirstChildStringByPrefix(
                                                       MainControl.Instance.OverworldControl.sceneTextsSave, "Yes");
                            else
                                _selectText.text = "<color=#00000000><size=2>wwww</size></color><color=yellow>" +
                                                   TextProcessingService.GetFirstChildStringByPrefix(
                                                       MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                                   "</color>    <color=#00000000><size=5>wwwwwwwww</size></color>" +
                                                   TextProcessingService.GetFirstChildStringByPrefix(
                                                       MainControl.Instance.OverworldControl.sceneTextsSave, "Yes");
                        }
                    }
                    break;
                }

                case 3:
                {
                    _titleText.text = "";
                    _nameText.text = "";
                    setName = "";
                    _characterText.text = "";
                    _selectText.text = "";
                    _teachText.text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Teach");
                    _textMessage.text =
                        TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.OverworldControl.sceneTextsSave, "MenuUnder") + Application.version;
                    if (GameUtilityService.KeyArrowToControl(KeyCode.Z) && setName.Length < 6)
                        mode = 1;
                    break;
                }
            }
        }
        /// <summary>
        /// 输出起名时用于选择的文字。
        /// </summary>
        private static string GenerateSelectableTextForRename(int selectNumber)
        {
            
            var alphabet = GenerateCharacterTextFrom(AlphabetCapital[AlphabetNum]) + "\n" +
                           GenerateCharacterTextFrom(AlphabetLowercase[AlphabetNum]);
            
            var final = "";
            for (var i = 0; i < alphabet.Length; i++)
            {
                if (i == selectNumber * 2)
                {
                    final += "<color=yellow>";
                }
                else if (i == selectNumber * 2 + 1)
                {
                    final += "</color>";
                }
                final += alphabet[i];
            }

            return final;
        }
        /// <summary>
        /// 将输入的字符转为起名界面字符的格式
        /// </summary>
        private static string GenerateCharacterTextFrom(string input)
        {
            var formatted = new StringBuilder();
            for (var i = 0; i < input.Length; i++)
            {
                formatted.Append(input[i]);
                if ((i + 1) % 7 == 0 && i != input.Length - 1)
                    formatted.Append("\n");
                else if (i != input.Length - 1)
                    formatted.Append(" ");
            }
            return formatted.ToString();
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

            _selectText.text = strings[0] +
                               TextProcessingService.GetFirstChildStringByPrefix(
                                   MainControl.Instance.OverworldControl.sceneTextsSave, "Rename1") + "</color> " +
                               strings[1] +
                               TextProcessingService.GetFirstChildStringByPrefix(
                                   MainControl.Instance.OverworldControl.sceneTextsSave, "Rename2") + "</color>  " +
                               strings[2] +
                               TextProcessingService.GetFirstChildStringByPrefix(
                                   MainControl.Instance.OverworldControl.sceneTextsSave, "Rename3") + "</color>";
        }

    }
}