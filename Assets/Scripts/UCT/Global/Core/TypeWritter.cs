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
using Random = UnityEngine.Random;


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
                _itemScroller.UpdateHandleItemInput(ref SelectController.GlobalItemIndex,
                    ref SelectController.VisibleItemIndex, SelectController.Story.currentChoices.Count, _ =>
                    {
                        AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        UpdateChoiceText();
                    });
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
                typeMode != TypeMode.IgnorePlayerInput)
            {
                if (overworldSpriteChanger)
                {
                    overworldSpriteChanger.spriteExpressionCollection = null;
                    overworldSpriteChanger.UpdateSpriteDisplay();
                }
            }

            if (passText && InputService.GetKeyDown(KeyCode.Z) && typeMode != TypeMode.IgnorePlayerInput)
            {
                PassText("<waitForUpdate>");
            }
            else if (!(isSkip || isJumpingText) &&
                     !cantSkip && InputService.GetKeyDown(KeyCode.X) &&
                     typeMode != TypeMode.IgnorePlayerInput &&
                     skipClock != 0 && clockTime <= 0)
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
                originString.Length > "<waitForUpdate>".Length)
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
            if (overworldSpriteChanger)
            {
                overworldSpriteChanger.state = SpriteExpressionCollection.State.Speaking;
            }

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

                    if (yieldNum != 0)
                    {
                        for (var j = 0; j < yieldNum; j++)
                        {
                            if (!string.IsNullOrEmpty(yieldString) && yieldString.Length > j)
                            {
                                endString += yieldString[j];
                                UpdateTmpText(tmpText);
                            }
                            else if (overworldSpriteChanger)
                            {
                                overworldSpriteChanger.state = SpriteExpressionCollection.State.Default;
                            }

                            if (isSkip)
                            {
                                continue;
                            }

                            yield return TypeWritterTagProcessor.GetTypeWritterStopTime(this);

                            if (overworldSpriteChanger)
                            {
                                overworldSpriteChanger.state = SpriteExpressionCollection.State.Speaking;
                            }
                        }
                    }
                }

                var cantString = "* \n\r";
                for (var j = 0; j < cantString.Length; j++)
                {
                    if (cantString[j] == originString[i])
                    {
                        cantString = "";
                    }
                }

                if (cantString != "" && !(isSkip || isJumpingText) && !isUsedFx)
                {
                    TypeWritterTagProcessor.TypeWritterPlayFx(this);
                }

                if (!passText)
                {
                    endString += originString[i];
                    passTextString += originString[i];
                }

                if (!(isSkip || isJumpingText))
                {
                    yield return TypeStopSeconds();
                }

                UpdateTmpText(tmpText);

                if (!passText)
                {
                    isTyping = false;
                }
                else
                {
                    originString = originString[passTextString.Length..];
                    break;
                }

                cantSkip = false;
            }

            if (overworldSpriteChanger)
            {
                overworldSpriteChanger.state = SpriteExpressionCollection.State.Default;
            }


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

        private void UpdateTmpText(TMP_Text tmpText)
        {
            if (tmpText)
            {
                tmpText.text = endString;

                Timing.RunCoroutine(_Dynamic(endString.Length - 1, dynamicType));


                if (tmpText.font != MainControl.Instance.overworldControl.tmpFonts[fontIndex])
                {
                    tmpText.font = MainControl.Instance.overworldControl.tmpFonts[fontIndex];
                }
            }
            else
            {
                Other.Debug.Log("缺失tmp_Text", "#FFFF00");
            }
        }

        private float TypeStopSeconds()
        {
            return Timing.WaitForSeconds(currentSpeed -
                                         currentSpeed * 0.25f * Convert.ToInt32(!SettingsStorage.textWidth));
        }

        private IEnumerator<float> _Dynamic(int number, OverworldControl.DynamicType inputDynamicType)
        {
            if (inputDynamicType != OverworldControl.DynamicType.None) //动效相关
            {
                var textInfo = _tmpText.textInfo;

                Vector3 orig;
                TMP_CharacterInfo charInfo;
                Vector3[] verts;

                switch (inputDynamicType)
                {
                    case OverworldControl.DynamicType.Shake:
                        for (var i = 0; i < 30; i++)
                        {
                            if (isSkip || isJumpingText)
                            {
                                break;
                            }

                            _tmpText.ForceMeshUpdate();

                            var randomNumber = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);

                            charInfo = textInfo.characterInfo[number];

                            if (!charInfo.isVisible)
                            {
                                break;
                            }

                            verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                            for (var j = 0; j < 4; j++)
                            {
                                orig = verts[charInfo.vertexIndex + j];
                                //动画
                                verts[charInfo.vertexIndex + j] = orig + randomNumber;
                            }

                            for (var k = 0; k < textInfo.meshInfo.Length; k++)
                            {
                                var meshInfo = textInfo.meshInfo[k];
                                meshInfo.mesh.vertices = meshInfo.vertices;
                                _tmpText.UpdateGeometry(meshInfo.mesh, k);
                            }

                            yield return 0;
                        }


                        break;
                    case OverworldControl.DynamicType.Fade:

                        const float fadeDuration = 0.1f; // 渐入时间

                        _tmpText.ForceMeshUpdate();

                        charInfo = textInfo.characterInfo[number];
                        if (!charInfo.isVisible)
                        {
                            break;
                        }

                        var colors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;
                        var startColor = colors[charInfo.vertexIndex];
                        var endColor = new Color32(startColor.r, startColor.g, startColor.b, 255);

                        // 设置初始颜色为透明
                        for (var j = 0; j < 4; j++)
                        {
                            colors[charInfo.vertexIndex + j] = new Color32(startColor.r, startColor.g, startColor.b, 0);
                        }

                        _tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                        var elapsedTime = 0f;
                        while (elapsedTime < fadeDuration)
                        {
                            if (isSkip || isJumpingText)
                            {
                                break;
                            }

                            elapsedTime += Time.deltaTime;
                            var alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

                            var currentColor = Color32.Lerp(new Color32(startColor.r, startColor.g, startColor.b, 0),
                                endColor, alpha);

                            for (var j = 0; j < 4; j++)
                            {
                                colors[charInfo.vertexIndex + j] = currentColor;
                            }

                            _tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                            yield return 0;
                        }

                        break;

                    case OverworldControl.DynamicType.Up:

                        for (var i = 0; i < 30; i++)
                        {
                            if (isSkip || isJumpingText)
                            {
                                break;
                            }

                            _tmpText.ForceMeshUpdate();

                            var down = new Vector3(0, -0.1f);

                            charInfo = textInfo.characterInfo[number];

                            if (!charInfo.isVisible)
                            {
                                break;
                            }

                            verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                            for (var j = 0; j < 4; j++)
                            {
                                orig = verts[charInfo.vertexIndex + j];

                                verts[charInfo.vertexIndex + j] = orig + down * (1 - (float)i / 30);
                            }

                            for (var k = 0; k < textInfo.meshInfo.Length; k++)
                            {
                                var meshInfo = textInfo.meshInfo[k];
                                meshInfo.mesh.vertices = meshInfo.vertices;
                                _tmpText.UpdateGeometry(meshInfo.mesh, k);
                            }

                            yield return 0;
                        }

                        break;
                    case OverworldControl.DynamicType.None:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(inputDynamicType), inputDynamicType, null);
                }
            }

            yield return 0;
            _tmpText.ForceMeshUpdate();
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

        private string ExtractPassTextPrefix(string input)
        {
            if (input.StartsWith("<waitForUpdate>", StringComparison.Ordinal))
            {
                return "<waitForUpdate>";
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