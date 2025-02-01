using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using DG.Tweening;
using hyjiacan.py4n;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.Settings;
using UCT.Global.UI;
using UCT.Other;
using UCT.Service;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable StringLiteralTypo

namespace UCT.Global.Scene
{
    public class RenameController : MonoBehaviour
    {
        public enum SceneState
        {
            Instruction,
            Naming,
            ConfirmName,
            SwitchScene
        }

        private const float RandomSetNameTimeMax = 1.5f;
        private const float BackspaceTimeMax = 0.5f;
        private const int MaxCharactersPerLine = 7;

        private const int MaxSetNameLength = 6;
        private static string _pinYin = "";
        private static int _pinYinNum;

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

            "abcdefghijklmnopqrstuvwxyz",

            "ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜ",
            "べぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔ",
            "゠ァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセ",
            "ヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲン",
            "ㇰㇱㇲㇳㇴㇵㇶㇷㇸ",

            "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ√",
            "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ√",
            "ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ√"
        };

        private static readonly List<string> AlphabetLowercase = new()
        {
            "abcdefghijklmnopqrstuvwxyz",
            "0123456789+-*/|\\^=()▌", //▌符号会被转化为空格
            "áàâãäåāăąćĉċďđēĕėęěéèêëĝğġģ",
            "ĥħĩīĭįıĵķĸĺļľŀłńņňŉŋñōŏőœŧų",
            "ŕŗřśŝşšţťðùúûüũūŭůűŵýÿŷźżž",

            "абвгдеёжзийклмноп",
            "рстуфхцчшщъыьэюя",

            "αβγδεζηθικλμνξοπρστυφχψω",

            "",

            "そぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへ",
            "ゕゖ゙゚゛゜ゝゞゟっ・ー",
            "ゼソゾタダチヂッツヅテデトドナニヌネノハバパヒビピフブプ",
            "ヴヵヶヷヸヹヺ・ーヽヾヿッ",
            "ㇹㇺㇻㇼㇽㇾㇿ",

            "",
            "",
            ""
        };

        private static List<string> _cutHanZiString = new() { "" };

        private static SelectedAlphabet _alphabetNum;
        private static bool _isSwitchedAlphabetNum;
        public int selectedCharactersId;
        public bool isSelectedDone;
        public string setName;

        public SceneState sceneState;
        private Tween _animMove, _animScale;

        private float _backspaceTime;

        private float _randomSetNameTime;

        private TextMeshPro _titleText,
            _nameText,
            _pinYinText,
            _characterText,
            _selectText,
            _teachText,
            _textMessage,
            _backspaceTip,
            _randomNameTip;

        private void Start()
        {
            StartGetComponent();

            _alphabetNum = 0;
            InitializePinyinAndCutHanZiString();

            if (string.IsNullOrEmpty(MainControl.Instance.playerControl.playerName))
            {
                sceneState = SceneState.Instruction;
                PlayerControlInitialization();
            }
            else
            {
                sceneState = SceneState.Naming;
            }

            _backspaceTip.text = TextProcessingService.GetFirstChildStringByPrefix(
                MainControl.Instance.overworldControl.sceneTextsSave, "BackspaceTip");
            _randomNameTip.text = TextProcessingService.GetFirstChildStringByPrefix(
                MainControl.Instance.overworldControl.sceneTextsSave, "RandomNameTip");
        }

        private void Update()
        {
            if (MainControl.Instance.overworldControl.isSetting) return;
            _randomNameTip.color =
                new Color(1, 1, 1, Mathf.Sin(_randomSetNameTime / RandomSetNameTimeMax * Mathf.PI / 2));
            _backspaceTip.color =
                new Color(1, 1, 1, Mathf.Sin((_backspaceTime - 0.3f) / (BackspaceTimeMax - 0.3f) * Mathf.PI / 2));
            SetPinYin();

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

        private void StartGetComponent()
        {
            _titleText = transform.Find("TitleText").GetComponent<TextMeshPro>();
            _nameText = transform.Find("NameText").GetComponent<TextMeshPro>();
            _pinYinText = transform.Find("PinYinText").GetComponent<TextMeshPro>();
            _characterText = transform.Find("CharacterText").GetComponent<TextMeshPro>();
            _selectText = transform.Find("SelectText").GetComponent<TextMeshPro>();
            _teachText = transform.Find("TeachText").GetComponent<TextMeshPro>();
            _textMessage = transform.Find("TextMessage").GetComponent<TextMeshPro>();
            _backspaceTip = transform.Find("BackspaceTip").GetComponent<TextMeshPro>();
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

        private void SetPinYin()
        {
            if (_pinYinText.text == _pinYin) return;
            _pinYinText.text = _pinYin;

            //以下部分只在_pinYin更新时触发

            if (sceneState != SceneState.Naming) return;
            if (_alphabetNum != SelectedAlphabet.Chinese && _alphabetNum != SelectedAlphabet.Chosung &&
                _alphabetNum != SelectedAlphabet.Chosung && _alphabetNum != SelectedAlphabet.Jungsung &&
                _alphabetNum != SelectedAlphabet.Jongsung) return;

            if (_alphabetNum != SelectedAlphabet.Chinese) return;
            var hanZiString = GetHanZiString(_pinYinText.text);
            _cutHanZiString = new List<string> { "" };
            var cutHanZiListIndex = 0;
            for (var i = 0; i < hanZiString.Length; i++)
            {
                _cutHanZiString[cutHanZiListIndex] += hanZiString[i];

                if ((cutHanZiListIndex != 0 || i == 0 || i % 19 != 0) &&
                    (cutHanZiListIndex <= 0 || i % 19 != 0)) continue;
                _cutHanZiString.Add("");
                cutHanZiListIndex++;
            }

            SetHanZiToAlphabetLowercaseWith(_pinYinNum);
        }

        private void SetHanZiToAlphabetLowercaseWith(int pinYinNum)
        {
            if (string.IsNullOrEmpty(_pinYin))
            {
                SetSpellingRequiredLowercase();
                return;
            }

            if (_cutHanZiString.Count > 0)
            {
                var leftArrow = pinYinNum > 0 ? "<" : "";
                var rightArrow = pinYinNum != _cutHanZiString.Count - 1 && _cutHanZiString.Count > 1 ? ">" : "";

                AlphabetLowercase[(int)_alphabetNum] = leftArrow + _cutHanZiString[pinYinNum] + rightArrow;
            }
            else
            {
                AlphabetLowercase[(int)_alphabetNum] = _cutHanZiString[pinYinNum];
            }

            if (AlphabetLowercase[(int)_alphabetNum].Length <= 0) return;
            TMPDynamicFontController.Instance.SimsunClear(new List<TMP_Text>
            {
                _titleText,
                _nameText,
                _pinYinText,
                _characterText,
                _selectText,
                _teachText,
                _textMessage,
                _backspaceTip,
                _randomNameTip
            });
        }

        private void ConfirmNameUpdate()
        {
            if (InputService.GetKeyDown(KeyCode.Z))
                switch (selectedCharactersId)
                {
                    case 0:
                    {
                        sceneState = SceneState.Naming;
                        _animMove.Kill();
                        _animScale.Kill();
                        _nameText.transform.localPosition = new Vector3(0, 0.6f);
                        _nameText.transform.localScale = Vector3.one;
                        _nameText.GetComponent<DynamicTMP>().dynamicMode = 0;
                        _selectText.text =
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.overworldControl.sceneTextsSave, "Quit") +
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.overworldControl.sceneTextsSave, "Backspace") +
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.overworldControl.sceneTextsSave, "Done");
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
                            Convert.ToInt32(SettingsStorage.isUsingHdFrame));
                        PlayerPrefs.SetInt("noSFX", Convert.ToInt32(SettingsStorage.isSimplifySfx));
                        PlayerPrefs.SetInt("vsyncMode",
                            Convert.ToInt32(SettingsStorage.vsyncMode));

                        GameUtilityService.FadeOutToWhiteAndSwitchScene("Menu");
                        break;
                    }
                }

            if (InputService.GetKeyDown(KeyCode.X))
            {
                _backspaceTime = 0.25f;
                sceneState = SceneState.Naming;
                _animMove.Kill();
                _animScale.Kill();
                _nameText.transform.localPosition = new Vector3(0, 0.6f);
                _nameText.transform.localScale = Vector3.one;
                _nameText.GetComponent<DynamicTMP>().dynamicMode = 0;
                _selectText.text =
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.overworldControl.sceneTextsSave, "Quit") +
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.overworldControl.sceneTextsSave, "Backspace") +
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.overworldControl.sceneTextsSave, _alphabetNum.ToString()) +
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.overworldControl.sceneTextsSave, "Done");
            }

            if (!isSelectedDone || (!InputService.GetKeyDown(KeyCode.LeftArrow) &&
                                    !InputService.GetKeyDown(KeyCode.RightArrow))) return;
            if (selectedCharactersId == 0)
            {
                selectedCharactersId = 1;
                if (SettingsStorage.textWidth)
                    _selectText.text =
                        TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.overworldControl.sceneTextsSave, "No") +
                        "    <color=yellow>" +
                        TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.overworldControl.sceneTextsSave, "Yes") + "</color>";
                else
                    _selectText.text = TextProcessingService.GetFirstChildStringByPrefix(
                                           MainControl.Instance.overworldControl.sceneTextsSave, "No") +
                                       "    <indent=85><color=yellow>" +
                                       TextProcessingService.GetFirstChildStringByPrefix(
                                           MainControl.Instance.overworldControl.sceneTextsSave, "Yes") +
                                       "</color>";
            }
            else
            {
                selectedCharactersId = 0;
                if (SettingsStorage.textWidth)
                    _selectText.text = "<color=yellow>" +
                                       TextProcessingService.GetFirstChildStringByPrefix(
                                           MainControl.Instance.overworldControl.sceneTextsSave, "No") +
                                       "</color>    <color=white>" +
                                       TextProcessingService.GetFirstChildStringByPrefix(
                                           MainControl.Instance.overworldControl.sceneTextsSave, "Yes");
                else
                    _selectText.text = "<color=yellow>" +
                                       TextProcessingService.GetFirstChildStringByPrefix(
                                           MainControl.Instance.overworldControl.sceneTextsSave, "No") +
                                       "</color>    <indent=85>" +
                                       TextProcessingService.GetFirstChildStringByPrefix(
                                           MainControl.Instance.overworldControl.sceneTextsSave, "Yes");
            }
        }

        private void NamingUpdate()
        {
            var alphabetNum = (int)_alphabetNum;

            var alphabetCapital = AlphabetCapital[alphabetNum];
            var alphabetLowercase = AlphabetLowercase[alphabetNum];


            var alphabetLength = alphabetCapital.Length + alphabetLowercase.Length;
            var breaker = false;
            var uppercaseRemainder = alphabetCapital.Length % MaxCharactersPerLine;
            var lowercaseRemainder = alphabetLowercase.Length % MaxCharactersPerLine;
            var maxUppercaseCharactersPerLine = MaxCharactersPerLine < alphabetCapital.Length
                ? MaxCharactersPerLine
                : alphabetCapital.Length;
            var maxLowercaseCharactersPerLine = MaxCharactersPerLine < alphabetLowercase.Length
                ? MaxCharactersPerLine
                : alphabetLowercase.Length;

            if (_isSwitchedAlphabetNum)
            {
                selectedCharactersId = alphabetLength + 2;
                _isSwitchedAlphabetNum = false;
            }

            if (InputService.GetKeyDown(KeyCode.Z) && setName.Length <= MaxSetNameLength)
            {
                if (selectedCharactersId < alphabetLength)
                {
                    if (setName.Length < MaxSetNameLength)
                    {
                        var setNameChar =
                            (alphabetCapital + alphabetLowercase)[selectedCharactersId];
                        var nameChar = setNameChar != '▌' ? setNameChar : ' ';
                        if ((_alphabetNum != SelectedAlphabet.Chinese &&
                             _alphabetNum != SelectedAlphabet.Chosung &&
                             _alphabetNum != SelectedAlphabet.Jungsung &&
                             _alphabetNum != SelectedAlphabet.Jongsung) ||
                            selectedCharactersId > alphabetCapital.Length - 1)
                        {
                            switch (nameChar)
                            {
                                case '<':
                                {
                                    _pinYinNum--;
                                    SetHanZiToAlphabetLowercaseWith(_pinYinNum);
                                    selectedCharactersId = alphabetLength - 1;
                                    break;
                                }
                                case '>':
                                {
                                    _pinYinNum++;
                                    SetHanZiToAlphabetLowercaseWith(_pinYinNum);
                                    selectedCharactersId = alphabetCapital.Length;
                                    break;
                                }
                                default:
                                {
                                    setName += nameChar;
                                    if (_alphabetNum is SelectedAlphabet.Chinese or SelectedAlphabet.Chosung
                                        or SelectedAlphabet.Jungsung or SelectedAlphabet.Jongsung)
                                        if (selectedCharactersId > alphabetCapital.Length - 1)
                                        {
                                            selectedCharactersId = 0;
                                            _pinYin = "";
                                        }

                                    break;
                                }
                            }
                        }
                        else if (_pinYin.Length < MaxSetNameLength)
                        {
                            _pinYin += nameChar;

                            switch (_alphabetNum)
                            {
                                case SelectedAlphabet.Chosung
                                    or SelectedAlphabet.Jungsung:
                                {
                                    if (_alphabetNum == SelectedAlphabet.Jungsung &&
                                        (_pinYin[0] == '√' || _pinYin[1] == '√') && _pinYin != "√√")
                                        ComposeHangul();
                                    else
                                        _alphabetNum += 1;
                                    break;
                                }
                                case SelectedAlphabet.Jongsung:
                                {
                                    ComposeHangul();
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (selectedCharactersId == alphabetLength)
                {
                    if (!string.IsNullOrEmpty(MainControl.Instance.playerControl.playerName))
                    {
                        GameUtilityService.FadeOutAndSwitchScene("Menu", Color.black, null);
                        sceneState = SceneState.SwitchScene;
                    }
                    else
                    {
                        sceneState = SceneState.Instruction;
                    }

                    InitializePinyinAndCutHanZiString();
                }
                else if (selectedCharactersId == alphabetLength + 1)
                {
                    Backspace();
                }
                else if (selectedCharactersId == alphabetLength + 2)
                {
                    AddAlphabetNum();
                }
                else if (selectedCharactersId == alphabetLength + 3)
                {
                    if (setName != "")
                    {
                        InitializePinyinAndCutHanZiString();
                        selectedCharactersId = 0;
                        sceneState = SceneState.ConfirmName;
                        var list = TextProcessingService.GetAllChildStringsByPrefix(
                            MainControl.Instance.overworldControl.sceneTextsSave, "RenameSp");
                        _titleText.text =
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.overworldControl.sceneTextsSave, "DefaultQuestion");
                        if (SettingsStorage.textWidth)
                            _selectText.text =
                                "<color=yellow>" +
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    MainControl.Instance.overworldControl.sceneTextsSave, "No") +
                                "</color>    " +
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    MainControl.Instance.overworldControl.sceneTextsSave, "Yes");
                        else
                            _selectText.text =
                                "<color=yellow>" +
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    MainControl.Instance.overworldControl.sceneTextsSave, "No") +
                                "</color>    <indent=85>" +
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    MainControl.Instance.overworldControl.sceneTextsSave, "Yes");

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
                                _selectText.text = "<color=yellow>" +
                                                   TextProcessingService
                                                       .GetFirstChildStringByPrefix(
                                                           MainControl.Instance.overworldControl
                                                               .sceneTextsSave, "GoBack") +
                                                   "</color>";
                            }

                            break;
                        }

                        _animMove = DOTween.To(() => _nameText.transform.localPosition,
                                x => _nameText.transform.localPosition = x, new Vector3(0, -7.85f),
                                5)
                            .SetEase(Ease.Linear);
                        _animScale = DOTween.To(() => _nameText.transform.localScale,
                                x => _nameText.transform.localScale = x, Vector3.one * 3, 5)
                            .SetEase(Ease.Linear);
                        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                        // 该方法只会在输入完名称确定时执行一次。
                        _nameText.GetComponent<DynamicTMP>().dynamicMode =
                            DynamicTMPType.RandomShakeAll;
                        _characterText.text = "";
                        _teachText.text = "";
                        _textMessage.text = "";
                        breaker = true;
                    }
                }
            }
            else if (InputService.GetKey(KeyCode.X))
            {
                if (_backspaceTime < BackspaceTimeMax)
                {
                    _backspaceTime += Time.deltaTime;
                }
                else
                {
                    _backspaceTime = 0.25f;
                    Backspace();
                }
            }
            else if (_backspaceTime < 0.25f && InputService.GetKeyUp(KeyCode.X))
            {
                SubAlphabetNum();
            }

            if (InputService.GetKey(KeyCode.C))
            {
                if (_randomSetNameTime < RandomSetNameTimeMax)
                {
                    _randomSetNameTime += Time.deltaTime;
                }
                else
                {
                    _randomSetNameTime = 0.5f;
                    setName = TextProcessingService.RandomString(Random.Range(1, 7),
                        alphabetCapital + alphabetLowercase);
                    AudioController.Instance.GetFx(2, MainControl.Instance.AudioControl.fxClipBattle);
                }
            }
            else if (_randomSetNameTime < 0.5f && InputService.GetKeyUp(KeyCode.C))
            {
                AddAlphabetNum();
            }

            if (!InputService.GetKey(KeyCode.X))
                _backspaceTime = _backspaceTime > 0 ? _backspaceTime - Time.deltaTime : 0;
            if (!InputService.GetKey(KeyCode.C))
                _randomSetNameTime = _randomSetNameTime > 0 ? _randomSetNameTime - Time.deltaTime : 0;

            if (breaker) return;
            if (InputService.GetKeyDown(KeyCode.UpArrow))
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
                else if (selectedCharactersId >= alphabetCapital.Length + uppercaseRemainder &&
                         selectedCharactersId < alphabetCapital.Length + MaxCharactersPerLine) //小写转大写的情况
                {
                    selectedCharactersId -= maxUppercaseCharactersPerLine + uppercaseRemainder;
                }
                else if (selectedCharactersId < alphabetCapital.Length) //大写常规情况
                {
                    selectedCharactersId -= maxUppercaseCharactersPerLine;
                }
                else if (selectedCharactersId >= alphabetCapital.Length + MaxCharactersPerLine &&
                         selectedCharactersId < alphabetLength) //小写常规情况
                {
                    selectedCharactersId -= maxLowercaseCharactersPerLine;
                }
                else if (selectedCharactersId >= alphabetLength &&
                         selectedCharactersId <= alphabetLength + 3) //底部键的情况
                {
                    var lowercaseRemainderFix = lowercaseRemainder > 0
                        ? lowercaseRemainder
                        : MaxCharactersPerLine;
                    int possibleCharacterId;
                    if (selectedCharactersId == alphabetLength)
                    {
                        if (alphabetLowercase.Length == 0 && lowercaseRemainderFix == MaxCharactersPerLine)
                            lowercaseRemainderFix = uppercaseRemainder;
                        possibleCharacterId = alphabetLength - lowercaseRemainderFix;
                    }
                    else if (selectedCharactersId == alphabetLength + 1)
                    {
                        possibleCharacterId = alphabetLength - lowercaseRemainderFix + 2;
                        if (alphabetLowercase.Length < 3)
                            possibleCharacterId += MaxCharactersPerLine - uppercaseRemainder;
                    }
                    else if (selectedCharactersId == alphabetLength + 2)
                    {
                        possibleCharacterId = alphabetLength - lowercaseRemainderFix + 4;
                        if (alphabetLowercase.Length < 5)
                            possibleCharacterId += MaxCharactersPerLine - uppercaseRemainder;
                    }
                    else
                    {
                        possibleCharacterId = alphabetLength - lowercaseRemainderFix + 5;
                        if (alphabetLowercase.Length < 6)
                            possibleCharacterId += MaxCharactersPerLine - uppercaseRemainder;
                    }

                    selectedCharactersId = possibleCharacterId < alphabetLength
                        ? possibleCharacterId
                        : possibleCharacterId - MaxCharactersPerLine;
                }

                else
                {
                    selectedCharactersId -= uppercaseRemainder;
                }

                if (selectedCharactersId < 0)
                    selectedCharactersId = alphabetLength + 3;
            }
            else if (InputService.GetKeyDown(KeyCode.DownArrow))
            {
                var alphabetLowercaseFix = AlphabetLowercase[alphabetNum].Length < MaxCharactersPerLine
                    ? AlphabetLowercase[alphabetNum].Length
                    : 0;
                if (selectedCharactersId >= alphabetCapital.Length - MaxCharactersPerLine &&
                    selectedCharactersId < alphabetCapital.Length - uppercaseRemainder &&
                    alphabetLowercase.Length < maxUppercaseCharactersPerLine)
                    selectedCharactersId += MaxCharactersPerLine * 2 - (MaxCharactersPerLine - uppercaseRemainder);


                if (selectedCharactersId >= alphabetCapital.Length - MaxCharactersPerLine &&
                    selectedCharactersId <
                    alphabetCapital.Length - uppercaseRemainder + alphabetLowercaseFix) //大写转小写的情况
                {
                    var idAdd = maxLowercaseCharactersPerLine > MaxCharactersPerLine
                        ? maxLowercaseCharactersPerLine
                        : MaxCharactersPerLine;
                    selectedCharactersId += alphabetLowercaseFix == 0 ? idAdd : 0;
                    selectedCharactersId += uppercaseRemainder;
                }
                else if (selectedCharactersId >= alphabetCapital.Length &&
                         selectedCharactersId < alphabetLength - MaxCharactersPerLine) //大写常规情况
                {
                    selectedCharactersId += maxUppercaseCharactersPerLine;
                }
                else if (selectedCharactersId < alphabetCapital.Length - uppercaseRemainder ||
                         (selectedCharactersId >= alphabetCapital.Length &&
                          selectedCharactersId < alphabetLength - MaxCharactersPerLine)) //小写常规情况
                {
                    selectedCharactersId += maxLowercaseCharactersPerLine > MaxCharactersPerLine
                        ? maxLowercaseCharactersPerLine
                        : MaxCharactersPerLine;
                }
                else if (selectedCharactersId >= alphabetLength - MaxCharactersPerLine) //到下面三个键的情况
                {
                    var lowercaseRemainderFix = lowercaseRemainder;

                    if (selectedCharactersId < alphabetCapital.Length)
                        lowercaseRemainderFix += uppercaseRemainder;
                    if (alphabetLowercase.Length > 0)
                        if (lowercaseRemainder == 0 ||
                            selectedCharactersId < alphabetLength - lowercaseRemainder)
                            lowercaseRemainderFix += MaxCharactersPerLine;

                    if (selectedCharactersId == alphabetLength)
                        selectedCharactersId = 0;
                    else if (selectedCharactersId == alphabetLength + 1)
                        selectedCharactersId = 2;
                    else if (selectedCharactersId == alphabetLength + 2)
                        selectedCharactersId = 4;
                    else if (selectedCharactersId == alphabetLength + 3)
                        selectedCharactersId = 5;
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
                {
                    selectedCharactersId += uppercaseRemainder;
                }

                if (selectedCharactersId > alphabetLength + 3)
                    selectedCharactersId = 0;
            }

            if (InputService.GetKeyDown(KeyCode.LeftArrow))
            {
                selectedCharactersId -= 1;
                if (selectedCharactersId < 0)
                    selectedCharactersId = alphabetLength + 3;
            }
            else if (InputService.GetKeyDown(KeyCode.RightArrow))
            {
                selectedCharactersId += 1;
                if (selectedCharactersId > alphabetLength + 3)
                    selectedCharactersId = 0;
            }

            _titleText.text =
                TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.overworldControl.sceneTextsSave, "NameTheHuman");
            _nameText.text = setName;
            _characterText.text = GenerateSelectableTextForRename(selectedCharactersId);
            HighlightSelectedOptions(selectedCharactersId);
            _teachText.text = "";
            _textMessage.text = "";
        }

        private void ComposeHangul()
        {
            if (_pinYin != "√√√")
                setName += HangulComposerService.ComposeHangul(_pinYin);
            _pinYin = "";
            _alphabetNum = SelectedAlphabet.Chosung;
            selectedCharactersId = 0;
        }

        private void Backspace()
        {
            if ((_alphabetNum != SelectedAlphabet.Chinese &&
                 _alphabetNum != SelectedAlphabet.Chosung &&
                 _alphabetNum != SelectedAlphabet.Jungsung &&
                 _alphabetNum != SelectedAlphabet.Jongsung) || _pinYin.Length == 0)
            {
                if (setName.Length > 0)
                    setName = setName[..^1];
            }
            else
            {
                if (_alphabetNum is SelectedAlphabet.Jungsung or SelectedAlphabet.Jongsung)
                    _alphabetNum -= 1;

                if (_pinYin.Length <= 0) return;
                _pinYin = _pinYin[..^1];
                selectedCharactersId = selectedCharactersId > AlphabetCapital[(int)_alphabetNum].Length - 1
                    ? AlphabetCapital[(int)_alphabetNum].Length - 1
                    : selectedCharactersId;
                if (_pinYin.Length != 0) return;
                InitializePinyinAndCutHanZiString();
            }
        }

        private static void AddAlphabetNum()
        {
            InitializePinyinAndCutHanZiString();

            _alphabetNum = _alphabetNum switch
            {
                SelectedAlphabet.Chosung or SelectedAlphabet.Jungsung => 0,
                _ => _alphabetNum < (SelectedAlphabet)(AlphabetCapital.Count - 1) ? _alphabetNum + 1 : 0
            };
            _isSwitchedAlphabetNum = true;
        }

        private static void SubAlphabetNum()
        {
            InitializePinyinAndCutHanZiString();
            _alphabetNum = _alphabetNum switch
            {
                SelectedAlphabet.Jungsung or SelectedAlphabet.Jongsung => SelectedAlphabet.Katakana3,
                SelectedAlphabet.Latin => SelectedAlphabet.Chosung,
                _ => _alphabetNum > 0 ? _alphabetNum - 1 : (SelectedAlphabet)(AlphabetCapital.Count - 1)
            };
            _isSwitchedAlphabetNum = true;
        }

        private static void SetSpellingRequiredLowercase()
        {
            AlphabetLowercase[(int)SelectedAlphabet.Chinese] = "";
            AlphabetLowercase[(int)SelectedAlphabet.Chosung] = "";
            AlphabetLowercase[(int)SelectedAlphabet.Jungsung] = "";
            AlphabetLowercase[(int)SelectedAlphabet.Jongsung] = "";
        }

        private static void InitializePinyinAndCutHanZiString(bool isCleanPinYin = true)
        {
            SetSpellingRequiredLowercase();
            _cutHanZiString = new List<string> { "" };
            _pinYinNum = 0;
            if (isCleanPinYin)
                _pinYin = "";
        }

        private void InstructionUpdate()
        {
            _titleText.text = "";
            _nameText.text = "";
            setName = "";
            _characterText.text = "";
            _selectText.text = "";
            _teachText.text =
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.overworldControl.sceneTextsSave,
                    "Teach");
            _textMessage.text =
                TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.overworldControl.sceneTextsSave, "MenuUnder") + Application.version;
            if (InputService.GetKeyDown(KeyCode.Z) && setName.Length < MaxSetNameLength)
                sceneState = SceneState.Naming;
        }

        /// <summary>
        ///     输出起名时用于选择的文字。
        /// </summary>
        private static string GenerateSelectableTextForRename(int selectNumber)
        {
            var alphabet = GenerateCharacterTextFrom(AlphabetCapital[(int)_alphabetNum]) + "\n" +
                           GenerateCharacterTextFrom(AlphabetLowercase[(int)_alphabetNum]);
            var final = "";
            for (var i = 0; i < alphabet.Length; i++)
            {
                if (i == selectNumber * 2)
                    final += "<color=yellow>";
                else if (i == selectNumber * 2 + 1) final += "</color>";

                if (_alphabetNum == SelectedAlphabet.Chinese)
                    if (i == AlphabetCapital[(int)_alphabetNum].Length * 2)
                        final += "<mspace=3.225>";

                final += alphabet[i];
            }

            if (_alphabetNum is SelectedAlphabet.Hiragana1 or SelectedAlphabet.Hiragana2 or SelectedAlphabet.Katakana1
                or SelectedAlphabet.Katakana2 or SelectedAlphabet.Katakana3 or SelectedAlphabet.Chosung
                or SelectedAlphabet.Jungsung or SelectedAlphabet.Jongsung)
                final = "<mspace=3.225>" + final;
            return final;
        }

        /// <summary>
        ///     将输入的字符转为起名界面字符的格式
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
        ///     根据给定的选择索引更新可选项的高亮状态.
        ///     高亮显示所选的选项为黄色.
        /// </summary>
        private void HighlightSelectedOptions(int selectNumber)
        {
            var strings = new List<string>();
            var selectId = selectNumber -
                           (AlphabetCapital[(int)_alphabetNum].Length + AlphabetLowercase[(int)_alphabetNum].Length);
            for (var i = 0; i < 4; i++) strings.Add(i == selectId ? "<color=yellow>" : "");

            _selectText.text = strings[0] +
                               TextProcessingService.GetFirstChildStringByPrefix(
                                   MainControl.Instance.overworldControl.sceneTextsSave, "Quit") + "</color> " +
                               strings[1] +
                               TextProcessingService.GetFirstChildStringByPrefix(
                                   MainControl.Instance.overworldControl.sceneTextsSave, "Backspace") + "</color> " +
                               strings[2] +
                               TextProcessingService.GetFirstChildStringByPrefix(
                                   MainControl.Instance.overworldControl.sceneTextsSave, _alphabetNum.ToString()) +
                               "</color> " +
                               strings[3] +
                               TextProcessingService.GetFirstChildStringByPrefix(
                                   MainControl.Instance.overworldControl.sceneTextsSave, "Done") + "</color>";
        }

        private static string[] GetHanZi(string input, bool matchAll = true)
        {
            return ChineseCharacterStrokesService.GetCharactersByStrokeCount(Pinyin4Net.GetHanzi(input, matchAll));
        }

        private static string GetHanZiString(string input, bool matchAll = true)
        {
            return GetHanZi(input, matchAll).Aggregate("", (current, hanZi) => current + hanZi);
        }


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

            Chinese,

            Hiragana1,
            Hiragana2,
            Katakana1,
            Katakana2,
            Katakana3,

            Chosung,
            Jungsung,
            Jongsung
        }
    }
}