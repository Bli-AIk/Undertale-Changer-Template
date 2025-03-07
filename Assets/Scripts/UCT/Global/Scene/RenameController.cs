using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using DG.Tweening;
using hyjiacan.py4n;
using TMPro;
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

        private const string ColorYellow = "<color=yellow>";

        private const float RandomSetNameTimeMax = 1.5f;
        private const float BackspaceTimeMax = 0.5f;
        private const int MaxCharactersPerLine = 7;

        private const int MaxSetNameLength = 6;
        private const string EndColor = "</color>";

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

        public int selectedCharactersId;
        public bool isSelectedDone;
        public string setName;

        public SceneState sceneState;
        private string _alphabetCapital;
        private int _alphabetLength;
        private string _alphabetLowercase;
        private Tween _animMove, _animScale;

        private float _backspaceTime;
        private int _lowercaseRemainder;
        private int _maxLowercaseCharactersPerLine;
        private int _maxUppercaseCharactersPerLine;

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

        private int _uppercaseRemainder;
        private static string PinYin { get; set; } = "";
        private static int PinYinNum { get; set; }

        private static List<string> CutHanZiString { get; set; } = new() { "" };

        private static SelectedAlphabet AlphabetNum { get; set; }
        private static bool IsSwitchedAlphabetNum { get; set; }

        private void Start()
        {
            StartGetComponent();

            AlphabetNum = 0;
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
                MainControl.Instance.LanguagePackControl.sceneTexts, "BackspaceTip");
            _randomNameTip.text = TextProcessingService.GetFirstChildStringByPrefix(
                MainControl.Instance.LanguagePackControl.sceneTexts, "RandomNameTip");
        }

        private void Update()
        {
            if (MainControl.Instance.overworldControl.isSetting)
            {
                return;
            }

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
                    throw new ArgumentOutOfRangeException($"Unexpected sceneState value: {sceneState}");
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
            MainControl.Instance.playerControl.wearWeapon = "TKnife";
            MainControl.Instance.playerControl.wearArmor = "TPS";
            MainControl.Instance.playerControl.saveScene = "Example-Corridor";
            MainControl.Instance.playerControl.items = new List<string> { "", "", "", "", "", "", "", "" };
        }

        private void SetPinYin()
        {
            if (_pinYinText.text == PinYin)
            {
                return;
            }

            _pinYinText.text = PinYin;

            //以下部分只在_pinYin更新时触发

            if (sceneState != SceneState.Naming)
            {
                return;
            }

            if (AlphabetNum != SelectedAlphabet.Chinese && AlphabetNum != SelectedAlphabet.Chosung &&
                AlphabetNum != SelectedAlphabet.Chosung && AlphabetNum != SelectedAlphabet.Jungsung &&
                AlphabetNum != SelectedAlphabet.Jongsung)
            {
                return;
            }

            if (AlphabetNum != SelectedAlphabet.Chinese)
            {
                return;
            }

            var hanZiString = GetHanZiString(_pinYinText.text);
            CutHanZiString = new List<string> { "" };
            var cutHanZiListIndex = 0;
            for (var i = 0; i < hanZiString.Length; i++)
            {
                CutHanZiString[cutHanZiListIndex] += hanZiString[i];

                if ((cutHanZiListIndex != 0 || i == 0 || i % 19 != 0) &&
                    (cutHanZiListIndex <= 0 || i % 19 != 0))
                {
                    continue;
                }

                CutHanZiString.Add("");
                cutHanZiListIndex++;
            }

            SetHanZiToAlphabetLowercaseWith(PinYinNum);
        }

        private void SetHanZiToAlphabetLowercaseWith(int pinYinNum)
        {
            if (string.IsNullOrEmpty(PinYin))
            {
                SetSpellingRequiredLowercase();
                return;
            }

            if (CutHanZiString.Count > 0)
            {
                var leftArrow = pinYinNum > 0 ? "<" : "";
                var rightArrow = pinYinNum != CutHanZiString.Count - 1 && CutHanZiString.Count > 1 ? ">" : "";

                AlphabetLowercase[(int)AlphabetNum] = leftArrow + CutHanZiString[pinYinNum] + rightArrow;
            }
            else
            {
                AlphabetLowercase[(int)AlphabetNum] = CutHanZiString[pinYinNum];
            }

            if (AlphabetLowercase[(int)AlphabetNum].Length <= 0)
            {
                return;
            }

            TmpDynamicFontController.Instance.SimsunClear(new List<TMP_Text>
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
            if (ConfirmInput())
            {
                return;
            }

            ConfirmUpdateText();
        }

        private void ConfirmUpdateText()
        {
            var noText = TextProcessingService.GetFirstChildStringByPrefix(
                MainControl.Instance.LanguagePackControl.sceneTexts, "No");
            var yesText = TextProcessingService.GetFirstChildStringByPrefix(
                MainControl.Instance.LanguagePackControl.sceneTexts, "Yes");

            if (selectedCharactersId == 0)
            {
                selectedCharactersId = 1;

                var formattedText = SettingsStorage.TextWidth
                    ? $"{noText}    {ColorYellow}{yesText}{EndColor}"
                    : $"{noText}    <indent=85>{ColorYellow}{yesText}{EndColor}";

                _selectText.text = formattedText;
            }
            else
            {
                selectedCharactersId = 0;

                var formattedText = SettingsStorage.TextWidth
                    ? $"{ColorYellow}{noText}{EndColor}    <color=white>{yesText}"
                    : $"{ColorYellow}{noText}{EndColor}    <indent=85>{yesText}";

                _selectText.text = formattedText;
            }
        }


        private bool ConfirmInput()
        {
            if (InputService.GetKeyDown(KeyCode.Z))
            {
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (selectedCharactersId == 0)
                {
                    sceneState = SceneState.Naming;
                    _animMove.Kill();
                    _animScale.Kill();
                    _nameText.transform.localPosition = new Vector3(0, 0.6f);
                    _nameText.transform.localScale = Vector3.one;
                    _nameText.GetComponent<DynamicTmp>().effectType = 0;
                    _selectText.text =
                        TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts, "Quit") +
                        TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts, "Backspace") +
                        TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts, "Done");
                }
                else if (selectedCharactersId == 1)
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
                        Convert.ToInt32(SettingsStorage.IsUsingHdFrame));
                    PlayerPrefs.SetInt("noSFX", Convert.ToInt32(SettingsStorage.IsSimplifySfx));
                    PlayerPrefs.SetInt("vsyncMode",
                        Convert.ToInt32(SettingsStorage.VsyncMode));

                    GameUtilityService.FadeOutToWhiteAndSwitchScene("Menu");
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
                _nameText.GetComponent<DynamicTmp>().effectType = 0;
                _selectText.text =
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.LanguagePackControl.sceneTexts, "Quit") +
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.LanguagePackControl.sceneTexts, "Backspace") +
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.LanguagePackControl.sceneTexts, AlphabetNum.ToString()) +
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.LanguagePackControl.sceneTexts, "Done");
            }

            if (!isSelectedDone ||
                (!InputService.GetKeyDown(KeyCode.LeftArrow) && !InputService.GetKeyDown(KeyCode.RightArrow)))
            {
                return true;
            }

            return false;
        }

        private void NamingUpdate()
        {
            var alphabetNum = (int)AlphabetNum;
            NamingSetValue(alphabetNum);

            if (IsSwitchedAlphabetNum)
            {
                selectedCharactersId = _alphabetLength + 2;
                IsSwitchedAlphabetNum = false;
            }

            if (NamingInput())
            {
                return;
            }

            NamingInputUpdate();

            _titleText.text =
                TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "NameTheHuman");
            _nameText.text = setName;
            _characterText.text = GenerateSelectableTextForRename(selectedCharactersId);
            HighlightSelectedOptions(selectedCharactersId);
            _teachText.text = "";
            _textMessage.text = "";
        }

        private bool NamingInput()
        {
            var breaker = false;
            if (InputService.GetKeyDown(KeyCode.Z) && setName.Length <= MaxSetNameLength)
            {
                breaker = AddChar();
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

            if (!InputService.GetKey(KeyCode.X))
            {
                _backspaceTime = _backspaceTime > 0 ? _backspaceTime - Time.deltaTime : 0;
            }

            InputRandomName();
            return breaker;
        }

        private void InputRandomName()
        {
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
                        _alphabetCapital + _alphabetLowercase);
                    AudioController.Instance.PlayFx(2, MainControl.Instance.AudioControl.fxClipBattle);
                }
            }
            else if (_randomSetNameTime < 0.5f && InputService.GetKeyUp(KeyCode.C))
            {
                AddAlphabetNum();
            }


            if (!InputService.GetKey(KeyCode.C))
            {
                _randomSetNameTime = _randomSetNameTime > 0 ? _randomSetNameTime - Time.deltaTime : 0;
            }
        }

        private void NamingSetValue(int alphabetNum)
        {
            _alphabetCapital = AlphabetCapital[alphabetNum];
            _alphabetLowercase = AlphabetLowercase[alphabetNum];
            _alphabetLength = _alphabetCapital.Length + _alphabetLowercase.Length;
            _uppercaseRemainder = _alphabetCapital.Length % MaxCharactersPerLine;
            _lowercaseRemainder = _alphabetLowercase.Length % MaxCharactersPerLine;
            _maxUppercaseCharactersPerLine = MaxCharactersPerLine < _alphabetCapital.Length
                ? MaxCharactersPerLine
                : _alphabetCapital.Length;
            _maxLowercaseCharactersPerLine = MaxCharactersPerLine < _alphabetLowercase.Length
                ? MaxCharactersPerLine
                : _alphabetLowercase.Length;
        }

        private void NamingInputUpdate()
        {
            if (InputService.GetKeyDown(KeyCode.UpArrow))
            {
                NamingGetUpArrowDown();
            }
            else if (InputService.GetKeyDown(KeyCode.DownArrow))
            {
                NamingGetDownArrowDown();
            }

            if (InputService.GetKeyDown(KeyCode.LeftArrow))
            {
                selectedCharactersId -= 1;
                if (selectedCharactersId < 0)
                {
                    selectedCharactersId = _alphabetLength + 3;
                }
            }
            else if (InputService.GetKeyDown(KeyCode.RightArrow))
            {
                selectedCharactersId += 1;
                if (selectedCharactersId > _alphabetLength + 3)
                {
                    selectedCharactersId = 0;
                }
            }
        }

        private void NamingGetDownArrowDown()
        {
            var alphabetNum = (int)AlphabetNum;
            var alphabetLowercaseFix = AlphabetLowercase[alphabetNum].Length < MaxCharactersPerLine
                ? AlphabetLowercase[alphabetNum].Length
                : 0;
            ReduceSelectedCharactersId(alphabetLowercaseFix);

            if (selectedCharactersId > _alphabetLength + 3)
            {
                selectedCharactersId = 0;
            }
        }

        private void ReduceSelectedCharactersId(int alphabetLowercaseFix)
        {
            ReduceIdTop();

            if (ReduceIdCommon(alphabetLowercaseFix))
            {
                return;
            }

            if (ReduceIdBottom())
            {
                return;
            }

            selectedCharactersId += _uppercaseRemainder;
        }

        private bool ReduceIdBottom()
        {
            if (selectedCharactersId < _alphabetLength - MaxCharactersPerLine)
            {
                return false;
            }

            var lowercaseRemainderFix = _lowercaseRemainder;

            if (selectedCharactersId < _alphabetCapital.Length)
            {
                lowercaseRemainderFix += _uppercaseRemainder;
            }

            if (_alphabetLowercase.Length > 0 && (_lowercaseRemainder == 0 ||
                                                  selectedCharactersId < _alphabetLength - _lowercaseRemainder))
            {
                lowercaseRemainderFix += MaxCharactersPerLine;
            }

            Dictionary<int, int> fixedIdMapping = new()
            {
                { _alphabetLength, 0 },
                { _alphabetLength + 1, 2 },
                { _alphabetLength + 2, 4 },
                { _alphabetLength + 3, 5 }
            };

            if (fixedIdMapping.TryGetValue(selectedCharactersId, out var mappedId))
            {
                selectedCharactersId = mappedId;
                return true;
            }

            var adjustedBase = _alphabetLength - lowercaseRemainderFix;
            int[] ranges = { 2, 4, 5, 7 };
            int[] replacements = { _alphabetLength, _alphabetLength + 1, _alphabetLength + 2, _alphabetLength + 3 };

            for (var i = 0; i < ranges.Length; i++)
            {
                if (selectedCharactersId >= adjustedBase + ranges[i])
                {
                    continue;
                }

                selectedCharactersId = replacements[i];
                return true;
            }

            return true;
        }


        private bool ReduceIdCommon(int alphabetLowercaseFix)
        {
            if (selectedCharactersId >= _alphabetCapital.Length - MaxCharactersPerLine &&
                selectedCharactersId <
                _alphabetCapital.Length - _uppercaseRemainder + alphabetLowercaseFix) //大写转小写的情况
            {
                var idAdd = _maxLowercaseCharactersPerLine > MaxCharactersPerLine
                    ? _maxLowercaseCharactersPerLine
                    : MaxCharactersPerLine;
                selectedCharactersId += alphabetLowercaseFix == 0 ? idAdd : 0;
                selectedCharactersId += _uppercaseRemainder;
                return true;
            }

            if (selectedCharactersId >= _alphabetCapital.Length &&
                selectedCharactersId < _alphabetLength - MaxCharactersPerLine) //大写常规情况
            {
                selectedCharactersId += _maxUppercaseCharactersPerLine;
                return true;
            }

            if (selectedCharactersId >= _alphabetCapital.Length - _uppercaseRemainder &&
                (selectedCharactersId < _alphabetCapital.Length ||
                 selectedCharactersId >= _alphabetLength - MaxCharactersPerLine)) //小写常规情况
            {
                return false;
            }

            selectedCharactersId += _maxLowercaseCharactersPerLine > MaxCharactersPerLine
                ? _maxLowercaseCharactersPerLine
                : MaxCharactersPerLine;
            return true;
        }

        private void ReduceIdTop()
        {
            if (selectedCharactersId >= _alphabetCapital.Length - MaxCharactersPerLine &&
                selectedCharactersId < _alphabetCapital.Length - _uppercaseRemainder &&
                _alphabetLowercase.Length < _maxUppercaseCharactersPerLine)
            {
                selectedCharactersId += MaxCharactersPerLine * 2 - (MaxCharactersPerLine - _uppercaseRemainder);
            }
        }

        private void NamingGetUpArrowDown()
        {
            selectedCharactersId = IncreaseSelectedCharactersId(selectedCharactersId);

            if (selectedCharactersId < 0)
            {
                selectedCharactersId = _alphabetLength + 3;
            }
        }

        private int IncreaseSelectedCharactersId(int id)
        {
            if (IncreaseIdTop(id, out var idTop))
            {
                return idTop;
            }

            if (IncreaseIdCommon(id, out var idCommon))
            {
                return idCommon;
            }

            if (IncreaseIdBottom(id, out var idBottom))
            {
                return idBottom;
            }

            id -= _uppercaseRemainder;
            return id;
        }

        private bool IncreaseIdTop(int id, out int idTop)
        {
            idTop = id;
            if (id >= MaxCharactersPerLine)
            {
                return false;
            }

            id = id switch
            {
                < 2 => _alphabetLength,
                < 4 => _alphabetLength + 1,
                < 5 => _alphabetLength + 2,
                _ => _alphabetLength + 3
            };
            idTop = id;
            return true;
        }

        private bool IncreaseIdBottom(int id, out int idBottom)
        {
            idBottom = id;
            if (id < _alphabetLength ||
                id > _alphabetLength + 3) //底部键的情况
            {
                return false;
            }

            var lowercaseRemainderFix = _lowercaseRemainder > 0
                ? _lowercaseRemainder
                : MaxCharactersPerLine;

            var possibleId = IncreaseGetPossibleId(id, lowercaseRemainderFix);

            id = possibleId < _alphabetLength
                ? possibleId
                : possibleId - MaxCharactersPerLine;
            idBottom = id;
            return true;
        }

        private int IncreaseGetPossibleId(int id, int lowercaseRemainderFix)
        {
            if (id == _alphabetLength && _alphabetLowercase.Length == 0 &&
                lowercaseRemainderFix == MaxCharactersPerLine)
            {
                lowercaseRemainderFix = _uppercaseRemainder;
            }

            var baseIdOffset = id > _alphabetLength ? (id - _alphabetLength) * 2 : 0;
            var possibleId = _alphabetLength - lowercaseRemainderFix + baseIdOffset + 1;
            if (_alphabetLowercase.Length < (id - _alphabetLength) * 2 + 3)
            {
                possibleId += MaxCharactersPerLine - _uppercaseRemainder;
            }

            return possibleId;
        }


        private bool IncreaseIdCommon(int id, out int idCommon)
        {
            idCommon = id;
            if (id >= _alphabetCapital.Length + _uppercaseRemainder &&
                id < _alphabetCapital.Length + MaxCharactersPerLine) //小写转大写的情况
            {
                id -= _maxUppercaseCharactersPerLine + _uppercaseRemainder;
                idCommon = id;
                return true;
            }

            if (id < _alphabetCapital.Length) //大写常规情况
            {
                id -= _maxUppercaseCharactersPerLine;
                idCommon = id;
                return true;
            }

            if (id < _alphabetCapital.Length + MaxCharactersPerLine ||
                id >= _alphabetLength) //小写常规情况
            {
                return false;
            }

            id -= _maxLowercaseCharactersPerLine;
            idCommon = id;
            return true;
        }

        private bool AddChar()
        {
            var breaker = false;
            if (selectedCharactersId < _alphabetLength)
            {
                if (setName.Length >= MaxSetNameLength)
                {
                    return false;
                }

                var setNameChar =
                    (_alphabetCapital + _alphabetLowercase)[selectedCharactersId];
                var nameChar = setNameChar != '▌' ? setNameChar : ' ';
                UpdateCjk(nameChar);
            }
            else if (selectedCharactersId == _alphabetLength)
            {
                ReturnGame();
            }
            else if (selectedCharactersId == _alphabetLength + 1)
            {
                Backspace();
            }
            else if (selectedCharactersId == _alphabetLength + 2)
            {
                AddAlphabetNum();
            }
            else if (selectedCharactersId == _alphabetLength + 3 && setName != "")
            {
                ReadyToConfirm();
                breaker = true;
            }

            return breaker;
        }

        private void ReadyToConfirm()
        {
            InitializePinyinAndCutHanZiString();
            isSelectedDone = true;
            selectedCharactersId = 0;
            sceneState = SceneState.ConfirmName;

            _titleText.text =
                TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "DefaultQuestion");
            SetConfirmSelectText();


            ConvertSpecialNames();

            _animMove = DOTween.To(() => _nameText.transform.localPosition,
                    x => _nameText.transform.localPosition = x, new Vector3(0, -7.85f),
                    5)
                .SetEase(Ease.Linear);
            _animScale = DOTween.To(() => _nameText.transform.localScale,
                    x => _nameText.transform.localScale = x, Vector3.one * 3, 5)
                .SetEase(Ease.Linear);
            _nameText.GetComponent<DynamicTmp>().effectType =
                DynamicTmpType.RandomShakeAll;
            _characterText.text = "";
            _teachText.text = "";
            _textMessage.text = "";
        }

        private void ConvertSpecialNames()
        {
            var list = TextProcessingService.GetAllChildStringsByPrefix(
                MainControl.Instance.LanguagePackControl.sceneTexts, "RenameSp");
            foreach (var item in list)
            {
                var lister = new List<string>();
                TextProcessingService.SplitStringToListWithDelimiter($"{item}|", lister, '|');

                var specialName = lister[0];
                var canRename = lister[1];
                var isCaseSensitive = lister[2];
                var narration = lister[3];

                if ((specialName != TextProcessingService.ConvertLettersCase(setName, true) ||
                     bool.Parse(isCaseSensitive)) &&
                    (specialName != setName || !bool.Parse(isCaseSensitive)))
                {
                    continue;
                }

                if (narration == "<gaster>")
                {
                    Application.Quit();
                }
                else
                {
                    _titleText.text = narration;
                }

                if (!bool.Parse(canRename))
                {
                    isSelectedDone = false;
                    _selectText.text = new StringBuilder()
                        .Append(ColorYellow)
                        .Append(TextProcessingService
                            .GetFirstChildStringByPrefix(
                                MainControl.Instance.LanguagePackControl
                                    .sceneTexts, "GoBack"))
                        .Append(EndColor)
                        .ToString();
                }

                break;
            }
        }

        private void SetConfirmSelectText()
        {
            if (SettingsStorage.TextWidth)
            {
                _selectText.text =
                    new StringBuilder()
                        .Append(ColorYellow)
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts, "No"))
                        .Append(EndColor)
                        .Append("    ")
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts, "Yes"))
                        .ToString();
            }
            else
            {
                _selectText.text =
                    new StringBuilder()
                        .Append(ColorYellow)
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts, "No"))
                        .Append(EndColor)
                        .Append("    <indent=85>")
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts, "Yes"))
                        .ToString();
            }
        }

        private void ReturnGame()
        {
            if (!string.IsNullOrEmpty(MainControl.Instance.playerControl.playerName))
            {
                GameUtilityService.FadeOutAndSwitchScene("Menu", Color.black);
                sceneState = SceneState.SwitchScene;
            }
            else
            {
                sceneState = SceneState.Instruction;
            }

            InitializePinyinAndCutHanZiString();
        }

        private void UpdateCjk(char nameChar)
        {
            if ((AlphabetNum != SelectedAlphabet.Chinese &&
                 AlphabetNum != SelectedAlphabet.Chosung &&
                 AlphabetNum != SelectedAlphabet.Jungsung &&
                 AlphabetNum != SelectedAlphabet.Jongsung) ||
                selectedCharactersId > _alphabetCapital.Length - 1)
            {
                IdAddChar(nameChar);
            }
            else if (PinYin.Length < MaxSetNameLength)
            {
                AddPinYin(nameChar);
            }
        }

        private void AddPinYin(char nameChar)
        {
            PinYin += nameChar;
            HandleHangul();
        }

        private void HandleHangul()
        {
            if (AlphabetNum is not (SelectedAlphabet.Chosung or SelectedAlphabet.Jungsung
                or SelectedAlphabet.Jongsung))
            {
                return;
            }

            switch (AlphabetNum)
            {
                case SelectedAlphabet.Chosung or SelectedAlphabet.Jungsung:
                {
                    if (AlphabetNum == SelectedAlphabet.Jungsung &&
                        (PinYin[0] == '√' || PinYin[1] == '√') && PinYin != "√√")
                    {
                        ComposeHangul();
                    }
                    else
                    {
                        AlphabetNum += 1;
                    }

                    break;
                }
                case SelectedAlphabet.Jongsung:
                {
                    ComposeHangul();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected AlphabetNum value: {AlphabetNum}");
            }
        }

        private void IdAddChar(char nameChar)
        {
            switch (nameChar)
            {
                case '<':
                {
                    PinYinNum--;
                    SetHanZiToAlphabetLowercaseWith(PinYinNum);
                    selectedCharactersId = _alphabetLength - 1;
                    break;
                }
                case '>':
                {
                    PinYinNum++;
                    SetHanZiToAlphabetLowercaseWith(PinYinNum);
                    selectedCharactersId = _alphabetCapital.Length;
                    break;
                }
                default:
                {
                    setName += nameChar;
                    if (AlphabetNum is SelectedAlphabet.Chinese or SelectedAlphabet.Chosung
                            or SelectedAlphabet.Jungsung or SelectedAlphabet.Jongsung &&
                        selectedCharactersId > _alphabetCapital.Length - 1)
                    {
                        selectedCharactersId = 0;
                        PinYin = "";
                    }

                    break;
                }
            }
        }

        private void ComposeHangul()
        {
            if (PinYin != "√√√")
            {
                setName += HangulComposerService.ComposeHangul(PinYin);
            }

            PinYin = "";
            AlphabetNum = SelectedAlphabet.Chosung;
            selectedCharactersId = 0;
        }

        private void Backspace()
        {
            if ((AlphabetNum != SelectedAlphabet.Chinese &&
                 AlphabetNum != SelectedAlphabet.Chosung &&
                 AlphabetNum != SelectedAlphabet.Jungsung &&
                 AlphabetNum != SelectedAlphabet.Jongsung) || PinYin.Length == 0)
            {
                if (setName.Length > 0)
                {
                    setName = setName[..^1];
                }
            }
            else
            {
                if (AlphabetNum is SelectedAlphabet.Jungsung or SelectedAlphabet.Jongsung)
                {
                    AlphabetNum -= 1;
                }

                if (PinYin.Length <= 0)
                {
                    return;
                }

                PinYin = PinYin[..^1];
                selectedCharactersId = selectedCharactersId > AlphabetCapital[(int)AlphabetNum].Length - 1
                    ? AlphabetCapital[(int)AlphabetNum].Length - 1
                    : selectedCharactersId;
                if (PinYin.Length != 0)
                {
                    return;
                }

                InitializePinyinAndCutHanZiString();
            }
        }

        private static void AddAlphabetNum()
        {
            InitializePinyinAndCutHanZiString();

            if (AlphabetNum is SelectedAlphabet.Chosung or SelectedAlphabet.Jungsung)
            {
                AlphabetNum = 0;
            }
            else
            {
                AlphabetNum = AlphabetNum < (SelectedAlphabet)(AlphabetCapital.Count - 1) ? AlphabetNum + 1 : 0;
            }

            IsSwitchedAlphabetNum = true;
        }

        private static void SubAlphabetNum()
        {
            InitializePinyinAndCutHanZiString();
            AlphabetNum = AlphabetNum switch
            {
                SelectedAlphabet.Jungsung or SelectedAlphabet.Jongsung => SelectedAlphabet.Katakana3,
                SelectedAlphabet.Latin => SelectedAlphabet.Chosung,
                _ => AlphabetNum > 0 ? AlphabetNum - 1 : (SelectedAlphabet)(AlphabetCapital.Count - 1)
            };
            IsSwitchedAlphabetNum = true;
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
            CutHanZiString = new List<string> { "" };
            PinYinNum = 0;
            if (isCleanPinYin)
            {
                PinYin = "";
            }
        }

        private void InstructionUpdate()
        {
            _titleText.text = "";
            _nameText.text = "";
            setName = "";
            _characterText.text = "";
            _selectText.text = "";
            _teachText.text =
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.sceneTexts,
                    "Teach");
            _textMessage.text =
                new StringBuilder().Append(TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.LanguagePackControl.sceneTexts, "MenuUnder"))
                    .Append(Application.version)
                    .ToString();
            if (InputService.GetKeyDown(KeyCode.Z) && setName.Length < MaxSetNameLength)
            {
                sceneState = SceneState.Naming;
            }
        }

        /// <summary>
        ///     输出起名时用于选择的文字。
        /// </summary>
        private static string GenerateSelectableTextForRename(int selectNumber)
        {
            var alphabetBuilder = new StringBuilder()
                .Append(GenerateCharacterTextFrom(AlphabetCapital[(int)AlphabetNum]))
                .Append("\n")
                .Append(GenerateCharacterTextFrom(AlphabetLowercase[(int)AlphabetNum]));

            var alphabet = alphabetBuilder.ToString();
            var finalBuilder = new StringBuilder();

            for (var i = 0; i < alphabet.Length; i++)
            {
                if (i == selectNumber * 2)
                {
                    finalBuilder.Append(ColorYellow);
                }
                else if (i == selectNumber * 2 + 1)
                {
                    finalBuilder.Append(EndColor);
                }

                if (AlphabetNum == SelectedAlphabet.Chinese && i == AlphabetCapital[(int)AlphabetNum].Length * 2)
                {
                    finalBuilder.Append("<mspace=3.225>");
                }

                finalBuilder.Append(alphabet[i]);
            }

            if (AlphabetNum is SelectedAlphabet.Hiragana1 or SelectedAlphabet.Hiragana2 or SelectedAlphabet.Katakana1
                or SelectedAlphabet.Katakana2 or SelectedAlphabet.Katakana3 or SelectedAlphabet.Chosung
                or SelectedAlphabet.Jungsung or SelectedAlphabet.Jongsung)
            {
                finalBuilder.Insert(0, "<mspace=3.225>");
            }

            return finalBuilder.ToString();
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
                {
                    formatted.Append("\n");
                }
                else if (i != input.Length - 1)
                {
                    formatted.Append(" ");
                }
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
                           (AlphabetCapital[(int)AlphabetNum].Length + AlphabetLowercase[(int)AlphabetNum].Length);
            for (var i = 0; i < 4; i++)
            {
                strings.Add(i == selectId ? ColorYellow : "");
            }

            _selectText.text = new StringBuilder().Append((object)strings[0])
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "Quit"))
                .Append(EndColor)
                .Append(" ")
                .Append((object)strings[1])
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "Backspace"))
                .Append(EndColor)
                .Append(" ")
                .Append((object)strings[2])
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, AlphabetNum.ToString()))
                .Append(EndColor)
                .Append(" ")
                .Append((object)strings[3])
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "Done"))
                .Append(EndColor)
                .ToString();
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