using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Alchemy.Inspector;
using MEC;
using Plugins.Timer.Source;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Settings;
using UCT.Overworld;
using UCT.Service;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace UCT.Global.Core
{
    /// <summary>
    ///     打字机系统
    /// </summary>
    public class TypeWritter : MonoBehaviour
    {
        public enum TypeMode
        {
            Default,
            IgnorePlayerInput
        }

        private const string WaitForUpdate = "<waitForUpdate>";

        [TabGroup("TypeWritter", "Basic(ReadOnly)")] [ReadOnly]
        public string originString, endString, passTextString;

        [TabGroup("TypeWritter", "Basic(ReadOnly)")] [ReadOnly]
        public bool isRunning;

        [TabGroup("TypeWritter", "Basic(ReadOnly)")] [ReadOnly]
        public bool isTyping;

        [HideInInspector] public int hpSave;

        [FormerlySerializedAs("canNotX")] [HideInInspector]
        public bool cantSkip;

        [FormerlySerializedAs("pressX")] [HideInInspector]
        public bool isSkip;

        [HideInInspector] public float clockTime;
        [HideInInspector] public bool passText;

        [FormerlySerializedAs("spriteChanger")] [HideInInspector]
        public OverworldSpriteChanger overworldSpriteChanger;

        [Title("FX data")] [TabGroup("TypeWritter", "Data")]
        public AudioClip fxClip;

        [TabGroup("TypeWritter", "Data")] public CharacterSpriteManager characterSpriteManager;

        [TabGroup("TypeWritter", "Data")] public float pitch = 1;

        [TabGroup("TypeWritter", "Data")] public float volume = 0.5f;

        [Title("TypeWritter data")] [TabGroup("TypeWritter", "Data")]
        public float speed = 0.075f;

        public float currentSpeed;

        [TabGroup("TypeWritter", "Data")] [Indent]
        public float speedSlow = 0.15f;

        [FormerlySerializedAs("clock")] [TabGroup("TypeWritter", "Data")] [Indent]
        public float skipClock = 0.01f;

        [TabGroup("TypeWritter", "Data")] public AudioMixerGroup audioMixerGroup;

        [FormerlySerializedAs("useFont")] [TabGroup("TypeWritter", "Data")]
        public int fontIndex;

        [TabGroup("TypeWritter", "Data")] public OverworldControl.DynamicType dynamicType;

        [HideInInspector] public bool isJumpingText;

        public bool isUsedFx;

        public TypeMode typeMode;

        private List<Vector2> _dynamicPos;

        private bool _isReadyToClose;
        private ItemScroller _itemScroller;

        private TMP_Text _tmpText;

        /// <summary>
        ///     打字机关闭时调用
        /// </summary>
        public Action OnClose;

        public TypeWritterSelectController SelectController;

        private void Start()
        {
            overworldSpriteChanger = GetComponent<OverworldSpriteChanger>();
            SelectController = new TypeWritterSelectController();
        }

        private void Update()
        {
            if (MainControl.Instance.overworldControl.isSetting)
            {
                return;
            }


            CloseTypeWritter();

            if (_itemScroller && SelectController.IsSelecting && SelectController.Story.currentChoices.Count > 0)
            {
                var updateHandleItemInput =
                    _itemScroller.UpdateHandleItemInput(SelectController.GlobalItemIndex,
                        SelectController.VisibleItemIndex,
                        SelectController.Story.currentChoices.Count, _ =>
                        {
                            AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                            UpdateChoiceText();
                        });
                SelectController.GlobalItemIndex = updateHandleItemInput.globalItemIndex;
                SelectController.VisibleItemIndex = updateHandleItemInput.visibleItemIndex;
            }
            else
            {
                SelectController.IsSelecting = false;
            }

            if (clockTime > 0)
            {
                clockTime -= Time.deltaTime;
            }

            if (!isRunning && !passText && !isTyping && InputService.GetKeyDown(KeyCode.Z) &&
                typeMode != TypeMode.IgnorePlayerInput &&
                overworldSpriteChanger)
            {
                overworldSpriteChanger.spriteExpressionCollection = null;
                overworldSpriteChanger.UpdateSpriteDisplay();
            }

            if (passText && InputService.GetKeyDown(KeyCode.Z) && typeMode != TypeMode.IgnorePlayerInput)
            {
                PassText(WaitForUpdate);
            }
            else if (!(isSkip || isJumpingText) &&
                     !cantSkip && InputService.GetKeyDown(KeyCode.X) &&
                     typeMode != TypeMode.IgnorePlayerInput &&
                     !Mathf.Approximately(skipClock, 0) && clockTime <= 0)
            {
                isSkip = true;
            }
        }

        private void UpdateChoiceText()
        {
            var choices = SelectController.Story.currentChoices;
            var baseIndex = SelectController.GlobalItemIndex - SelectController.VisibleItemIndex;
            var result = new StringBuilder();

            for (var i = 0; i < 3 && baseIndex + i < choices.Count; i++)
            {
                if (i > 0)
                {
                    result.Append("\n");
                }

                result.Append("<indent=10></indent>* ").Append(choices[baseIndex + i].text);
            }

            _tmpText.text = result.ToString();

            if (MainControl.Instance.sceneState == MainControl.SceneState.Overworld)
            {
                BackpackBehaviour.Instance.Heart.transform.localPosition = new Vector3(-6.2f,
                    (TalkBoxController.Instance.isUp ? 4.85f : -2.95f) - 0.9f * SelectController.VisibleItemIndex, 5);
            }
        }


        private void CloseTypeWritter()
        {
            if (!_isReadyToClose || !InputService.GetKeyDown(KeyCode.Z))
            {
                return;
            }

            if (_itemScroller && SelectController.IsSelecting && SelectController.Story.currentChoices.Count > 0 &&
                InputService.GetKeyDown(KeyCode.Z))
            {
                SelectController.IsSelecting = false;
                SelectController.Story.ChooseChoiceIndex(SelectController.GlobalItemIndex);
                var dialogue = SelectController.GetStoryDialogue();
                dialogue = DataHandlerService.ChangeItemData(dialogue, false, new List<string>());
                StartTypeWritter(dialogue, _tmpText);
                _itemScroller.Close();
                BackpackBehaviour.Instance.Heart.transform.localPosition = new Vector3(
                    BackpackBehaviour.Instance.Heart.transform.localPosition.x,
                    BackpackBehaviour.Instance.Heart.transform.localPosition.y, 0);
                SelectController.VisibleItemIndex = 0;
                SelectController.GlobalItemIndex = 0;
                return;
            }

            if (_itemScroller && SelectController.Story != null && SelectController.Story.currentChoices.Count > 0)
            {
                if (SelectController.IsSelecting)
                {
                    return;
                }

                SelectController.IsSelecting = true;
                UpdateChoiceText();
                _itemScroller.Open(SelectController.Story.currentChoices.Count, 1.175f);
            }
            else
            {
                SelectController.IsSelecting = false;
                OnClose?.Invoke();
                _isReadyToClose = false;
            }
        }

        /// <summary>
        ///     开启打字机。
        /// </summary>
        public void StartTypeWritter(string text, TMP_Text tmpText)
        {
            isRunning = true;
            TypeWritterTagProcessor.SetSpeedMode(this);

            if (isTyping)
            {
                return;
            }

            StopAllCoroutines();
            SetUpTypeWritter(text, tmpText);

            if (MainControl.Instance.sceneState == MainControl.SceneState.Overworld &&
                originString.Length > WaitForUpdate.Length)
            {
                TalkBoxController.Instance.CleanText(this);
            }

            if (!_itemScroller && MainControl.Instance.sceneState == MainControl.SceneState.Overworld)
            {
                _itemScroller = TalkBoxController.Instance.ItemScroller;
            }

            Timing.RunCoroutine(_Typing(_tmpText));
        }

        private void SetUpTypeWritter(string text, TMP_Text tmpText)
        {
            passText = false;
            endString = "";
            tmpText.text = "";
            passTextString = "";
            originString = text;
            hpSave = MainControl.Instance.playerControl.hp;
            clockTime = skipClock;
            isSkip = false;
            _tmpText = tmpText;
        }

        public void TypeStop()
        {
            isRunning = false;
            Timing.KillCoroutines();
            isTyping = false;
            endString = "";
            if (_tmpText)
            {
                _tmpText.text = "";
            }

            passTextString = "";
        }

        public static void TypePause(bool pause)
        {
            if (pause)
            {
                Timing.PauseCoroutines();
            }
            else
            {
                Timing.ResumeCoroutines();
            }
        }

        private IEnumerator<float> _Typing(TMP_Text tmpText)
        {
            _isReadyToClose = false;
            isRunning = true;
            SetOverworldSpriteState(SpriteExpressionCollection.State.Speaking);

            for (var i = 0; i < originString.Length; i++)
            {
                isTyping = true;
                isUsedFx = false;

                if (!passText)
                {
                    TypeWritterTagProcessor.TypeWritterProcessTag(this, tmpText, ref i,
                        out var yieldNum,
                        out var yieldString,
                        out var startPassText);

                    if (startPassText)
                    {
                        continue;
                    }

                    for (var j = 0; j < yieldNum && !isSkip; j++)
                    {
                        ConvertYieldString(tmpText, yieldString, j);
                        yield return TypeWritterTagProcessor.GetTypeWritterStopTime(this);
                        SetOverworldSpriteState(SpriteExpressionCollection.State.Speaking);
                    }
                }

                TypingPlayFx(i);
                TypingAddText(i);

                if (!(isSkip || isJumpingText))
                {
                    yield return TypeStopSeconds();
                }

                UpdateTmpText(tmpText);

                if (IsTypingPassText())
                {
                    break;
                }

                cantSkip = false;
            }

            SetOverworldSpriteState(SpriteExpressionCollection.State.Default);
            CloseTyping();
        }

        private bool IsTypingPassText()
        {
            if (!passText)
            {
                isTyping = false;
            }
            else
            {
                originString = originString[passTextString.Length..];
                return true;
            }

            return false;
        }

        private void ConvertYieldString(TMP_Text tmpText, string yieldString, int j)
        {
            if (!string.IsNullOrEmpty(yieldString) && yieldString.Length > j)
            {
                endString += yieldString[j];
                UpdateTmpText(tmpText);
            }
            else
            {
                SetOverworldSpriteState(SpriteExpressionCollection.State.Default);
            }
        }

        private void CloseTyping()
        {
            if (!passText)
            {
                _isReadyToClose = true;
            }

            isRunning = false;
            var prefix = ExtractPassTextPrefix(originString);
            if (!string.IsNullOrEmpty(prefix) && originString.Length > prefix.Length)
            {
                originString = originString[prefix.Length..];
            }
        }

        private void TypingAddText(int i)
        {
            if (passText)
            {
                return;
            }

            endString += originString[i];
            passTextString += originString[i];
        }

        private void TypingPlayFx(int i)
        {
            var excludedFxChars = "* \n\r";
            for (var j = 0; j < excludedFxChars.Length; j++)
            {
                if (excludedFxChars[j] == originString[i])
                {
                    excludedFxChars = "";
                }
            }

            if (excludedFxChars != "" && !(isSkip || isJumpingText) && !isUsedFx)
            {
                TypeWritterTagProcessor.TypeWritterPlayFx(this);
            }
        }

        private void SetOverworldSpriteState(SpriteExpressionCollection.State state)
        {
            if (overworldSpriteChanger)
            {
                overworldSpriteChanger.state = state;
            }
        }

        private void UpdateTmpText(TMP_Text tmpText)
        {
            if (tmpText)
            {
                tmpText.text = endString;

                StartCoroutine(
                    TypeWritterDynamicController.DynamicTmp(this, _tmpText, endString.Length - 1, dynamicType));

                tmpText.font = MainControl.Instance.overworldControl.tmpFonts[fontIndex];
            }
            else
            {
                Other.Debug.Log("缺失tmp_Text", "#FFFF00");
            }
        }

        private float TypeStopSeconds()
        {
            return Timing.WaitForSeconds(currentSpeed -
                                         currentSpeed * 0.25f * Convert.ToInt32(!SettingsStorage.TextWidth));
        }


        private void PassText(string inputPassText)
        {
            endString = "";
            if (_tmpText)
            {
                _tmpText.text = "";
            }

            passText = false;

            passTextString = "";

            if (MainControl.Instance.sceneState == MainControl.SceneState.Overworld)
            {
                TalkBoxController.Instance.CleanText(this);
                if (originString[..inputPassText.Length] == inputPassText)
                {
                    originString = originString[inputPassText.Length..];
                }
            }

            isSkip = false;
            Timing.RunCoroutine(_Typing(_tmpText));
        }


        public void PassTextWithDelay(string inputText, float delayInSeconds)
        {
            Timer.Register(delayInSeconds, () => PassText(inputText));
        }

        private static string ExtractPassTextPrefix(string input)
        {
            if (input.StartsWith(WaitForUpdate, StringComparison.Ordinal))
            {
                return WaitForUpdate;
            }

            if (!input.StartsWith("<waitForTime=", StringComparison.Ordinal))
            {
                return null;
            }

            var startIndex = "<waitForTime=".Length;
            var endIndex = input.IndexOf('>', startIndex);

            if (endIndex <= startIndex)
            {
                return null;
            }

            var numberPart = input[startIndex..endIndex];
            return double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out _)
                ? input[..(endIndex + 1)]
                : null;
        }
    }
}