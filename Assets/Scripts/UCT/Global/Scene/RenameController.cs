using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
// ReSharper disable StringLiteralTypo

namespace UCT.Global.Scene
{
    public class RenameController : MonoBehaviour
    {
        private TextMeshPro _titleText, _nameText, _characterText, _selectText, _teachText, _textMessage, _randomNameTip;
        public int selectedCharactersId;
        public bool isSelectedDone;
        public string setName;

        public enum SceneState
        {
            Instruction,
            Naming,
            ConfirmName,
            SwitchScene,
        }
        
        public SceneState sceneState;
        private Tween _animMove, _animScale;

        
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private enum SelectedAlphabet
        {
            Latin,
            Symbol,
            LatinExtended1,
            LatinExtended2,
            LatinExtended3,
            Cyrillic1,
            Cyrillic2,
            Greek,
            //ChineseSimplified,
            //ChineseTraditional,
            //Japanese,
            //Korean,
        }
        private static readonly List<string> AlphabetCapital = new()
        {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            ",.?!:;#$&\"'<>@[]_`{}~",
            "ÁÀÂÃÄÅĀĂĄĆĈĊČĎĐĒĔĖĘĚÉÈÊËĜĞĠĢ",
            "ĤĦĨĪĬĮİĴĶĹĻĽĿŁŃŅŇŊÑŌŎŐŒÞßþ",
            "ŔŖŘŚŜŞŠŢŤŦŨŪŬŮŰŲŴŶŸÝŶŸŹŻŽ",
            "АБВГДЕЁЖЗИЙКЛМНОП",
            "РСТУФХЦЧШЩЪЫЬЭЮЯ",
            "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ",
        };

        private static readonly List<string> AlphabetLowercase = new()
        {
            "abcdefghijklmnopqrstuvwxyz",
            "0123456789+-*/|\\^=()▌",//▌符号会被转化为空格
            "áàâãäåāăąćĉċďđēĕėęěéèêëĝğġģ",
            "ĥħĩīĭįıĵķĸĺļľŀłńņňŉŋñōŏőœŧų",
            "ŕŗřśŝşšţťðùúûüũūŭůűŵýÿŷźżž",
            "абвгдеёжзийклмноп",
            "рстуфхцчшщъыьэюя",
            "αβγδεζηθικλμνξοπρστυφχψω"
        };

        private float _randomSetNameTime;
        private const float RandomSetNameTimeMax = 1.5f;

        
        private static SelectedAlphabet _alphabetNum;
        private const int MaxCharactersPerLine = 7;
        private static bool _isSwitchedAlphabetNum;
        private void Start()
        {
            StartGetComponent();

            if (string.IsNullOrEmpty(MainControl.Instance.playerControl.playerName))
            {
                sceneState = SceneState.Instruction;
                PlayerControlInitialization();
            }
            else
            {
                sceneState = SceneState.Naming;
            }

            _randomNameTip.text = TextProcessingService.GetFirstChildStringByPrefix(
                MainControl.Instance.OverworldControl.sceneTextsSave, "RandomNameTip");
        }

        private void StartGetComponent()
        {
            _titleText = transform.Find("TitleText").GetComponent<TextMeshPro>();
            _nameText = transform.Find("NameText").GetComponent<TextMeshPro>();
            _characterText = transform.Find("CharacterText").GetComponent<TextMeshPro>();
            _selectText = transform.Find("SelectText").GetComponent<TextMeshPro>();
            _teachText = transform.Find("TeachText").GetComponent<TextMeshPro>();
            _textMessage = transform.Find("TextMessage").GetComponent<TextMeshPro>();
            _randomNameTip = transform.Find("RandomNameTip").GetComponent<TextMeshPro>();
        }

        private static void PlayerControlInitialization()
        {
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

        private void Update()
        {
            if (MainControl.Instance.OverworldControl.isSetting) return;
            _randomNameTip.color = new Color(1,1,1,_randomSetNameTime / RandomSetNameTimeMax);
            switch (sceneState)
            {
                case SceneState.Instruction:
                {
                    InstructionUpdate();
                    break;
                }
                case SceneState.Naming:
                {
                    NamingUpdate();
                    break;
                }
                case SceneState.ConfirmName:
                {
                    ConfirmNameUpdate();
                    break;
                }

                case SceneState.SwitchScene:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ConfirmNameUpdate()
        {
            if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
            {
                switch (selectedCharactersId)
                {
                    case 0:
                    {
                        sceneState = SceneState.Naming;
                        _animMove.Kill();
                        _animScale.Kill();
                        _nameText.transform.localPosition = new Vector3(8.95f, 0.6f);
                        _nameText.transform.localScale = Vector3.one;
                        _nameText.GetComponent<DynamicTMP>().dynamicMode = 0;
                        _selectText.text =
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.OverworldControl.sceneTextsSave, "Quit") +
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.OverworldControl.sceneTextsSave, "Backspace") +
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.OverworldControl.sceneTextsSave, "Done");
                        break;
                    }

                    case 1:
                    {
                        sceneState = SceneState.SwitchScene;
                        MainControl.Instance.playerControl.playerName = setName;
                        AudioController.Instance.transform.GetComponent<AudioSource>().Pause();
                        var v2 = GameObject.Find("Global Volume (1)").transform
                            .GetComponent<UnityEngine.Rendering.Volume>();

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
                sceneState = SceneState.Naming;
                _animMove.Kill();
                _animScale.Kill();
                _nameText.transform.localPosition = new Vector3(8.95f, 0.6f);
                _nameText.transform.localScale = Vector3.one;
                _nameText.GetComponent<DynamicTMP>().dynamicMode = 0;
                _selectText.text =
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.OverworldControl.sceneTextsSave, "Quit") +
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.OverworldControl.sceneTextsSave, "Backspace") +
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.OverworldControl.sceneTextsSave, _alphabetNum.ToString()) +
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.OverworldControl.sceneTextsSave, "Done");
            }

            if (!isSelectedDone || (!GameUtilityService.KeyArrowToControl(KeyCode.LeftArrow) &&
                               !GameUtilityService.KeyArrowToControl(KeyCode.RightArrow))) return;
            if (selectedCharactersId == 0)
            {
                selectedCharactersId = 1;
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
                selectedCharactersId = 0;
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

        private void NamingUpdate()
        {
            var alphabetNum = (int)_alphabetNum;
            var alphabetLength = AlphabetCapital[alphabetNum].Length + AlphabetLowercase[alphabetNum].Length;
            var breaker = false;
            var uppercaseRemainder = AlphabetCapital[alphabetNum].Length % MaxCharactersPerLine;
            var lowercaseRemainder = AlphabetLowercase[alphabetNum].Length % MaxCharactersPerLine;
            var maxUppercaseCharactersPerLine = MaxCharactersPerLine < AlphabetCapital[alphabetNum].Length
                ? MaxCharactersPerLine
                : AlphabetCapital[alphabetNum].Length;
            var maxLowercaseCharactersPerLine = MaxCharactersPerLine < AlphabetLowercase[alphabetNum].Length
                ? MaxCharactersPerLine
                : AlphabetLowercase[alphabetNum].Length;

            if (_isSwitchedAlphabetNum)
            {
                selectedCharactersId = alphabetLength + 2;
                _isSwitchedAlphabetNum = false;
            }
            
            if (GameUtilityService.KeyArrowToControl(KeyCode.Z) && setName.Length <= 6)
            {
                if (selectedCharactersId < alphabetLength)
                {
                    if (setName.Length < 6)
                    {
                        var settNameChar =
                            (AlphabetCapital[alphabetNum] + AlphabetLowercase[alphabetNum])[selectedCharactersId];
                        
                        setName += settNameChar != '▌' ? settNameChar : ' ';
                    }
                }
                else if (selectedCharactersId == alphabetLength)
                {
                    if (!string.IsNullOrEmpty(MainControl.Instance.playerControl.playerName))
                    {
                        GameUtilityService.FadeOutAndSwitchScene("Menu", Color.black);
                        sceneState = SceneState.SwitchScene;
                    }
                    else
                        sceneState = SceneState.Instruction;
                }
                else if (selectedCharactersId == alphabetLength + 1)
                {
                    if (setName.Length > 0)
                        setName = setName[..^1];
                }
                else if (selectedCharactersId == alphabetLength + 2)
                {
                    SwitchAlphabetNum();
                }
                else if (selectedCharactersId == alphabetLength + 3)
                {
                    if (setName != "")
                    {
                        selectedCharactersId = 0;
                        sceneState = SceneState.ConfirmName;
                        var list = TextProcessingService.GetAllChildStringsByPrefix(
                            MainControl.Instance.OverworldControl.sceneTextsSave, "RenameSp");
                        _titleText.text =
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.OverworldControl.sceneTextsSave, "DefaultQuestion");
                        if (MainControl.Instance.OverworldControl.textWidth)
                        {
                            _selectText.text =
                                "<size=0>wwww</size><color=yellow>" +
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                "</color>    " +
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    MainControl.Instance.OverworldControl.sceneTextsSave, "Yes");
                        }
                        else
                        {
                            _selectText.text =
                                "<size=2><color=#00000000>wwww</color></size><color=yellow>" +
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    MainControl.Instance.OverworldControl.sceneTextsSave, "No") +
                                "</color>    <color=#00000000><size=5>wwwwwwwww</size></color>" +
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    MainControl.Instance.OverworldControl.sceneTextsSave, "Yes");
                        }

                        isSelectedDone = true;
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
                                isSelectedDone = false;
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
                        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                        // 该方法只会在输入完名称确定时执行一次。
                        _nameText.GetComponent<DynamicTMP>().dynamicMode =
                            OverworldControl.DynamicTMP.RandomShakeSingle;
                        _characterText.text = "";
                        _teachText.text = "";
                        _textMessage.text = "";
                        breaker = true;
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
                if (_randomSetNameTime < RandomSetNameTimeMax)
                    _randomSetNameTime += Time.deltaTime;
                else
                {
                    _randomSetNameTime = 0.5f;
                    setName = TextProcessingService.RandomString(Random.Range(1, 7));
                    AudioController.Instance.GetFx(2, MainControl.Instance.AudioControl.fxClipBattle);
                }
            }
            else if (_randomSetNameTime < 0.5f && GameUtilityService.KeyArrowToControl(KeyCode.C, 2))
            {
                SwitchAlphabetNum();
            }
            else
            {
                _randomSetNameTime = _randomSetNameTime > 0 ? _randomSetNameTime - Time.deltaTime : 0;
            }
            
            if (breaker) return;
            if (GameUtilityService.KeyArrowToControl(KeyCode.UpArrow))
            {
                if (selectedCharactersId < MaxCharactersPerLine)
                {
                    selectedCharactersId = selectedCharactersId switch
                    {
                        < 2 => alphabetLength,
                        < 4 => alphabetLength + 1,
                        < 5 => alphabetLength + 2,
                        _ => alphabetLength + 3
                    };
                }
                else if (selectedCharactersId >= AlphabetCapital[alphabetNum].Length + uppercaseRemainder &&
                    selectedCharactersId < AlphabetCapital[alphabetNum].Length + MaxCharactersPerLine)  //小写转大写的情况
                {
                    selectedCharactersId -= maxUppercaseCharactersPerLine + uppercaseRemainder;
                }
                else if (selectedCharactersId < AlphabetCapital[alphabetNum].Length)   //大写常规情况
                {
                    selectedCharactersId -= maxUppercaseCharactersPerLine;
                }
                else if (selectedCharactersId >= AlphabetCapital[alphabetNum].Length + MaxCharactersPerLine &&
                         selectedCharactersId < alphabetLength)   //小写常规情况
                {
                    selectedCharactersId -= maxLowercaseCharactersPerLine;
                }
                else if (selectedCharactersId >= alphabetLength &&
                         selectedCharactersId <= alphabetLength + 3) //底部三个键的情况
                {
                    if (selectedCharactersId == alphabetLength)
                    {
                        selectedCharactersId = alphabetLength - (lowercaseRemainder > 0 ? lowercaseRemainder : MaxCharactersPerLine);
                    }
                    else
                    {
                        int possibleCharacterId;
                        if (selectedCharactersId == alphabetLength + 1)
                        {
                            possibleCharacterId = alphabetLength - lowercaseRemainder + 2;
                        }
                        else if (selectedCharactersId == alphabetLength + 2)
                        {
                            possibleCharacterId = alphabetLength - lowercaseRemainder + 4;
                        }
                        else if (selectedCharactersId == alphabetLength + 3)
                        {
                            possibleCharacterId = alphabetLength - lowercaseRemainder + 5;
                        }
                        else
                        {
                            possibleCharacterId = alphabetLength - lowercaseRemainder + 5;
                        }

                        selectedCharactersId = possibleCharacterId <
                                               alphabetLength
                            ? possibleCharacterId
                            : possibleCharacterId - MaxCharactersPerLine;
                    }
                }

                else
                    selectedCharactersId -= uppercaseRemainder;

                if (selectedCharactersId < 0)
                    selectedCharactersId = alphabetLength + 3;
            }
            else if (GameUtilityService.KeyArrowToControl(KeyCode.DownArrow))
            {
                if (selectedCharactersId >= AlphabetCapital[alphabetNum].Length - MaxCharactersPerLine &&
                    selectedCharactersId < AlphabetCapital[alphabetNum].Length - uppercaseRemainder)  //大写转小写的情况
                {
                    selectedCharactersId += maxLowercaseCharactersPerLine + uppercaseRemainder;
                }
                else if (selectedCharactersId >= AlphabetCapital[alphabetNum].Length &&
                         selectedCharactersId < alphabetLength - MaxCharactersPerLine) //大写常规情况
                {
                    
                    selectedCharactersId += maxUppercaseCharactersPerLine;
                }
                else if (selectedCharactersId < AlphabetCapital[alphabetNum].Length - uppercaseRemainder ||
                         selectedCharactersId >= AlphabetCapital[alphabetNum].Length &&
                         selectedCharactersId < alphabetLength - MaxCharactersPerLine) //小写常规情况
                {
                    selectedCharactersId += maxLowercaseCharactersPerLine;
                }
                else if (selectedCharactersId >= alphabetLength - MaxCharactersPerLine) //到下面三个键的情况
                {
                    var lowercaseRemainderFix = lowercaseRemainder > 0 ? lowercaseRemainder : MaxCharactersPerLine;

                    if (selectedCharactersId == alphabetLength)
                    {
                        selectedCharactersId = 0;
                    }
                    else if (selectedCharactersId == alphabetLength + 1)
                    {
                        selectedCharactersId = 2;
                    }
                    else if (selectedCharactersId == alphabetLength + 2)
                    {
                        selectedCharactersId = 4;
                    }
                    else if (selectedCharactersId == alphabetLength + 3)
                    {
                        selectedCharactersId = 5;
                    }
                    else if (selectedCharactersId >= alphabetLength - lowercaseRemainderFix &&
                        selectedCharactersId < alphabetLength - lowercaseRemainderFix + 2) 
                        selectedCharactersId = alphabetLength;
                    else if (selectedCharactersId >= alphabetLength - lowercaseRemainderFix + 2 &&
                             selectedCharactersId < alphabetLength - lowercaseRemainderFix + 4)
                        selectedCharactersId = alphabetLength + 1;
                    else if (selectedCharactersId >= alphabetLength - lowercaseRemainderFix + 4 &&
                             selectedCharactersId < alphabetLength - lowercaseRemainderFix + 5)
                        selectedCharactersId = alphabetLength + 2;
                    else if (selectedCharactersId < alphabetLength - lowercaseRemainderFix + 7) 
                        selectedCharactersId = alphabetLength + 3; 
                }
                else
                    selectedCharactersId += uppercaseRemainder;

                if (selectedCharactersId > alphabetLength + 3)
                    selectedCharactersId = 0;
            }
            if (GameUtilityService.KeyArrowToControl(KeyCode.LeftArrow))
            {
                selectedCharactersId -= 1;
                if (selectedCharactersId < 0)
                    selectedCharactersId = alphabetLength + 3;
            }
            else if (GameUtilityService.KeyArrowToControl(KeyCode.RightArrow))
            {
                selectedCharactersId += 1;
                if (selectedCharactersId > alphabetLength + 3)
                    selectedCharactersId = 0;
            }

            _titleText.text =
                TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.OverworldControl.sceneTextsSave, "NameTheHuman");
            _nameText.text = setName;
            _characterText.text = GenerateSelectableTextForRename(selectedCharactersId);
            HighlightSelectedOptions(selectedCharactersId);
            _teachText.text = "";
            _textMessage.text = "";
        }

        private static void SwitchAlphabetNum()
        {
            _alphabetNum = _alphabetNum < (SelectedAlphabet)(AlphabetCapital.Count - 1) ? _alphabetNum + 1 : 0;
            _isSwitchedAlphabetNum = true;
        }

        private void InstructionUpdate()
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
                sceneState = SceneState.Naming;
        }

        /// <summary>
        /// 输出起名时用于选择的文字。
        /// </summary>
        private static string GenerateSelectableTextForRename(int selectNumber)
        {
            var alphabet = GenerateCharacterTextFrom(AlphabetCapital[(int)_alphabetNum]) + "\n" +
                           GenerateCharacterTextFrom(AlphabetLowercase[(int)_alphabetNum]);
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
                if ((i + 1) % MaxCharactersPerLine == 0 && i != input.Length - 1)
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
            var selectId = selectNumber -
                           (AlphabetCapital[(int)_alphabetNum].Length + AlphabetLowercase[(int)_alphabetNum].Length);
            for (var i = 0; i < 4; i++)
            {
                strings.Add(i == selectId ? "<color=yellow>" : "");
            }

            _selectText.text = strings[0] +
                               TextProcessingService.GetFirstChildStringByPrefix(
                                   MainControl.Instance.OverworldControl.sceneTextsSave, "Quit") + "</color> " +
                               strings[1] +
                               TextProcessingService.GetFirstChildStringByPrefix(
                                   MainControl.Instance.OverworldControl.sceneTextsSave, "Backspace") + "</color> " +
                               strings[2] +
                               TextProcessingService.GetFirstChildStringByPrefix(
                                   MainControl.Instance.OverworldControl.sceneTextsSave, _alphabetNum.ToString()) +
                               "</color> " +
                               strings[3] +
                               TextProcessingService.GetFirstChildStringByPrefix(
                                   MainControl.Instance.OverworldControl.sceneTextsSave, "Done") + "</color>";
        }

    }
}