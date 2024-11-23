using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using MEC;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.Scene;
using UCT.Global.Settings;
using UCT.Overworld;
using UCT.Service;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace UCT.Global.UI
{
    /// <summary>
    ///     打字机系统
    /// </summary>
    public class TypeWritter : MonoBehaviour
    {
        public enum TypeMode
        {
            Normal, //正常的打字机
            CantZx //不能按ZX的打字机，使用富文本进行控制。
        }

        private static readonly int Open = Animator.StringToHash("Open");
        public string originString, endString, passTextString;
        public bool isRunning; //打字机是否在运行
        public bool isTyping; //是否在 打出字符
        public int hpIn;
        public int hpSave;
        public bool canNotX;
        public bool pressX;
        public float clockTime; //实际上计数
        public bool isStop;
        public int fx; //音效
        public bool fxRandomPitch;

        [Header("打字速度与检测停顿字符后的打字速度")] public float speed = 0.075f, speedSlow = 0.15f;

        [Header("打字后多少秒可以按X跳过，0为不能跳")] public float clock = 0.01f; //设置

        public bool passText;


        public SpriteChanger spriteChanger;

        [Header("适配OW框")] public bool isOverworld;

        public float pitch = 1;
        public float volume = 0.5f;
        public AudioMixerGroup audioMixerGroup;

        [Header("字体")] public int useFont;

        [Header("打字动效")] public OverworldControl.DynamicType dynamicType;

        [Header("总有那么一些情况需要强硬手段（拔枪")] public bool forceReturn;

        private Animator _canvasAnim;

        private List<Vector2> _dynamicPos;

        private bool _endInBattle;
        private bool _isJumpingText;

        private bool _isUsedFx;

        private TalkBoxPositionChanger _talkBoxPositionChanger;

        private TMP_Text _tmpText;

        private TypeMode _typeMode = TypeMode.Normal;

        private void Start()
        {
            if (isOverworld)
                _talkBoxPositionChanger = TalkBoxPositionChanger.Instance;
            spriteChanger = GetComponent<SpriteChanger>();
        }

        private void Update()
        {
            if (MainControl.Instance.overworldControl.isSetting || forceReturn) //pause在OW检测的时候会用
                return;

            if (clockTime > 0)
                clockTime -= Time.deltaTime;
            if (!isRunning && !passText && !isTyping && InputService.GetKeyDown(KeyCode.Z) &&
                _typeMode != TypeMode.CantZx)
            {
                if (spriteChanger != null)
                    spriteChanger.ChangeImage(-1);
                if (_endInBattle)
                    _canvasAnim.SetBool(Open, true);
            }

            if (passText && InputService.GetKeyDown(KeyCode.Z) && _typeMode != TypeMode.CantZx)
                PassText("<passText>");
            else if (!(pressX || _isJumpingText) && !canNotX && InputService.GetKeyDown(KeyCode.X) &&
                     _typeMode != TypeMode.CantZx) //跳字
                if (clock != 0 && clockTime <= 0)
                    pressX = true;
        }

        /// <summary>
        ///     开启打字机。若打字正在进行，可强行终止。
        ///     一般情况下不需要强行打断对话。
        ///     若传入的语句中含有autoFood，请输入hp。若输入0，此字符将跳过。
        /// </summary>
        public void TypeOpen(string text, bool force, int hp, int inputFX, TMP_Text tmpText,
            TypeMode typeMode = TypeMode.Normal)
        {
            isRunning = true;
            _typeMode = typeMode;

            if (!force && isTyping)
                return;
            StopAllCoroutines();
            passText = false;
            endString = "";
            tmpText.text = "";
            passTextString = "";
            originString = text;
            hpIn = hp;
            hpSave = MainControl.Instance.playerControl.hp;
            clockTime = clock;
            pressX = false;
            isStop = false;
            fx = inputFX;
            if (isOverworld)
                _talkBoxPositionChanger.Change(true, originString[.."<passText>".Length] == "<passText>", true, this);

            _tmpText = tmpText;
            Timing.RunCoroutine(_Typing(_tmpText));
        }

        public void TypeStop()
        {
            isRunning = false;
            Timing.KillCoroutines();
            isTyping = false;
            endString = "";
            if (_tmpText) _tmpText.text = "";
            passTextString = "";
        }

        public static void TypePause(bool pause)
        {
            if (pause)
                Timing.PauseCoroutines();
            else
                Timing.ResumeCoroutines();
        }

        private IEnumerator<float> _Typing(TMP_Text tmpText)
        {
            isRunning = true;
            for (var i = 0; i < originString.Length; i++)
            {
                if (spriteChanger)
                    spriteChanger.justSaying = false;

                isTyping = true;
                _isUsedFx = false;

                if (fxRandomPitch)
                    pitch = Random.Range(0.25f, 1.25f);


                if (originString[i] == '<')
                {
                    var fix0 = i == 0;

                    while (originString[i] == '<')
                    {
                        var spText = "";
                        while (fix0 || originString[i - 1] != '>')
                        {
                            spText += originString[i];
                            i++;
                            if (fix0)
                                fix0 = false;
                        }

                        passTextString += spText;


                        if (TextProcessingService.IsSameFrontTexts(spText, "<fx="))
                        {
                            var save = spText[4..];
                            save = save[..^1];
                            fx = int.Parse(save);
                        }
                        else if (TextProcessingService.IsSameFrontTexts(spText, "<font="))
                        {
                            var save = spText[6..];
                            save = save[..^1];
                            useFont = int.Parse(save);
                            tmpText.font = MainControl.Instance.overworldControl.tmpFonts[useFont];
                        }
                        else if (TextProcessingService.IsSameFrontTexts(spText, "<stop*"))
                        {
                            if (!(pressX || _isJumpingText))
                            {
                                isTyping = false;
                                var number = float.Parse(spText.Substring(6, spText.Length - 7));
                                for (var p = 0; p < number; p++)
                                {
                                    if (pressX || _isJumpingText)
                                        break;
                                    yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f *
                                        Convert.ToInt32(!MainControl.Instance.overworldControl.textWidth));
                                }

                                isTyping = false;
                            }

                            isStop = true;
                        }
                        else if (TextProcessingService.IsSameFrontTexts(spText, "<image="))
                        {
                            var save = spText[7..];
                            save = save[..^1];
                            var s = int.Parse(save);
                            if (spriteChanger)
                                spriteChanger.ChangeImage(s);
                            _talkBoxPositionChanger.Change(true, s >= 0, false);
                        }
                        else if (TextProcessingService.IsSameFrontTexts(spText, "<stop...*"))
                        {
                            if (!(pressX || _isJumpingText))
                            {
                                isTyping = false;

                                var number = float.Parse(spText.Substring(9, spText.Length - 10));
                                for (var l = 0; l < 3; l++)
                                {
                                    for (var p = 0; p < number; p++)
                                    {
                                        if (pressX || _isJumpingText)
                                            break;
                                        yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f *
                                            Convert.ToInt32(!MainControl.Instance.overworldControl.textWidth));
                                    }

                                    AudioController.Instance.GetFx(fx, MainControl.Instance.AudioControl.fxClipType,
                                        volume, pitch, audioMixerGroup);
                                    endString += '.';
                                    tmpText.text = endString;
                                }
                            }
                            else
                            {
                                endString += "...";
                            }

                            isStop = true;
                            tmpText.text = endString;
                        }
                        else if (TextProcessingService.IsSameFrontTexts(spText, "<passText="))
                        {
                            var delay = float.Parse(spText[10..^1]);
                            passText = true;
                            passTextString = passTextString[..^spText.Length];
                            PassTextWithDelay(spText, delay);
                            goto PassText;
                        }
                        else if (TextProcessingService.IsSameFrontTexts(spText, "<storyFade"))
                        {
                            var save = spText[11..];
                            save = save[..^1];
                            StorySceneController.Instance.Fade(int.Parse(save));
                        }
                        else if (TextProcessingService.IsSameFrontTexts(spText, "<stop......*"))
                        {
                            if (!(pressX || _isJumpingText))
                            {
                                isTyping = false;

                                var number = float.Parse(spText.Substring(12, spText.Length - 13));
                                for (var l = 0; l < 6; l++)
                                {
                                    for (var p = 0; p < number; p++)
                                    {
                                        if (pressX || _isJumpingText)
                                            break;
                                        yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f *
                                            Convert.ToInt32(!MainControl.Instance.overworldControl.textWidth));
                                    }

                                    AudioController.Instance.GetFx(fx, MainControl.Instance.AudioControl.fxClipType,
                                        volume, pitch, audioMixerGroup);
                                    endString += '.';
                                    tmpText.text = endString;
                                }
                            }
                            else
                            {
                                endString += "......";
                            }

                            isStop = true;
                            tmpText.text = endString;
                        }
                        else
                        {
                            if (spText.Length >= 2 && spText[0] == '<' && spText[2] == '>')
                                spText = spText[1].ToString();

                            switch (spText)
                            {
                                case "<storyMaskT>":
                                    StorySceneController.Instance.mask.SetActive(true);
                                    break;

                                case "<storyMaskF>":
                                    StorySceneController.Instance.mask.SetActive(false);
                                    break;

                                case "<storyExit>":
                                    TypeStop();
                                    GameUtilityService.FadeOutAndSwitchScene("Start", Color.black, true);
                                    break;

                                case "<sprite=0>":
                                    if (!(pressX || _isJumpingText))
                                        AudioController.Instance.GetFx(fx, MainControl.Instance.AudioControl.fxClipType,
                                            volume, pitch, audioMixerGroup);
                                    goto default;
                                case "<stop>":
                                    if (!(pressX || _isJumpingText))
                                        //单独一个Stop的时候，不设置isTyping，这是因为有的时候这个stop的时间很短，
                                        //所以 isTyping = true 之后，显示起来有点怪。
                                        //如果需要长的Stop，建议还是使用<stop*x>的方式来做。
                                        //isTyping = false;
                                        yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f *
                                            Convert.ToInt32(!MainControl.Instance.overworldControl.textWidth));
                                    isStop = true;
                                    break;

                                case "<autoFood>":
                                    var plusString = hpIn + hpSave >= MainControl.Instance.playerControl.hpMax
                                        ? MainControl.Instance.ItemControl.itemTextMaxData[22]
                                        : MainControl.Instance.ItemControl.itemTextMaxData[12];

                                    plusString = plusString[..^1];
                                    originString = TextProcessingService.RemoveSubstring(originString,
                                        i - "<autoFood>".Length, i - 1, plusString);
                                    i -= spText.Length;
                                    passTextString = passTextString[..^spText.Length];
                                    break;
                                case "<itemHp>":
                                    spText = hpIn.ToString();
                                    yield return TypeStopSeconds();
                                    goto default;
                                case "<changeX>":
                                    canNotX = !canNotX;
                                    break;
                                case "<jumpText>":
                                    _isJumpingText = true;
                                    break;
                                case "</jumpText>":
                                    _isJumpingText = false;
                                    break;
                                case "<passText>":
                                    passText = true;
                                    passTextString = passTextString[..^spText.Length];
                                    goto PassText;
                                default: //富文本

                                    if (spText.Length - 2 > 0 && spText[1] == '-' && spText[^2] == '-')
                                    {
                                        spText = spText.Substring(2, spText.Length - 4);
                                        if (!(pressX || _isJumpingText))
                                        {
                                            AudioController.Instance.GetFx(fx,
                                                MainControl.Instance.AudioControl.fxClipType, volume, pitch,
                                                audioMixerGroup);
                                            _isUsedFx = true;

                                            if (spriteChanger != null)
                                                spriteChanger.justSaying = true;
                                        }
                                    }

                                    endString += spText;
                                    tmpText.text = endString;

                                    break;
                            }
                        }

                        if (i >= originString.Length)
                        {
                            originString += " ";
                            break;
                        }

                        fix0 = true;
                    }
                }

                //string cantString = ",.:;!?，。：；！？ \n\r";

                var cantString = "* \n\r";
                for (var j = 0; j < cantString.Length; j++)
                    if (cantString[j] == originString[i])
                        cantString = "";
                if (cantString != "" && !(pressX || _isJumpingText) && !_isUsedFx)
                    AudioController.Instance.GetFx(fx, MainControl.Instance.AudioControl.fxClipType, volume, pitch,
                        audioMixerGroup);

                if (!passText)
                {
                    endString += originString[i];
                    passTextString += originString[i];
                }

                if (!(pressX || _isJumpingText))
                    yield return TypeStopSeconds();

                if (tmpText)
                {
                    tmpText.text = endString;

                    Timing.RunCoroutine(_Dynamic(endString.Length - 1, dynamicType));


                    if (tmpText.font != MainControl.Instance.overworldControl.tmpFonts[useFont])
                        tmpText.font = MainControl.Instance.overworldControl.tmpFonts[useFont];
                }
                else
                {
                    Other.Debug.Log("缺失tmp_Text", "#FFFF00");
                }

                if (!passText)
                {
                    isTyping = false;
                }
                else
                {
                    originString = originString[passTextString.Length] != '<'
                        ? originString[(passTextString.Length - 1)..]
                        : originString[passTextString.Length..];
                    break;
                }

                canNotX = false;
                isStop = false;
                PassText: ; //这是个标签注意
            }

            isRunning = false;
            var prefix = ExtractPassTextPrefix(originString);
            if (!string.IsNullOrEmpty(prefix) && originString.Length > prefix.Length)
                originString = originString[prefix.Length..];
            yield break;

            float TypeStopSeconds()
            {
                return Timing.WaitForSeconds(speed -
                                             speed * 0.25f * Convert.ToInt32(!MainControl.Instance
                                                 .overworldControl.textWidth));
            }
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
                            if (pressX || _isJumpingText)
                                break;

                            _tmpText.ForceMeshUpdate();

                            var randomNumber = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);

                            charInfo = textInfo.characterInfo[number];

                            if (!charInfo.isVisible) break;

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
                        if (!charInfo.isVisible) break;

                        var colors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;
                        var startColor = colors[charInfo.vertexIndex];
                        var endColor = new Color32(startColor.r, startColor.g, startColor.b, 255);

                        // 设置初始颜色为透明
                        for (var j = 0; j < 4; j++)
                            colors[charInfo.vertexIndex + j] = new Color32(startColor.r, startColor.g, startColor.b, 0);

                        _tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                        var elapsedTime = 0f;
                        while (elapsedTime < fadeDuration)
                        {
                            if (pressX || _isJumpingText)
                                break;
                            elapsedTime += Time.deltaTime;
                            var alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

                            var currentColor = Color32.Lerp(new Color32(startColor.r, startColor.g, startColor.b, 0),
                                endColor, alpha);

                            for (var j = 0; j < 4; j++) colors[charInfo.vertexIndex + j] = currentColor;

                            _tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                            yield return 0;
                        }

                        break;

                    case OverworldControl.DynamicType.Up:

                        for (var i = 0; i < 30; i++)
                        {
                            if (pressX || _isJumpingText)
                                break;

                            _tmpText.ForceMeshUpdate();

                            var down = new Vector3(0, -0.1f);

                            charInfo = textInfo.characterInfo[number];

                            if (!charInfo.isVisible) break;

                            verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                            for (var j = 0; j < 4; j++)
                            {
                                orig = verts[charInfo.vertexIndex + j];
                                //动画
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
                        throw new ArgumentOutOfRangeException(nameof(inputDynamicType), inputDynamicType, null);
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
            if (_tmpText) _tmpText.text = "";
            passText = false;

            passTextString = "";
            if (isOverworld)
            {
                _talkBoxPositionChanger.Change(false, false, true, this);
                if (originString[..inputPassText.Length] == inputPassText)
                    originString = originString[inputPassText.Length..];
            }

            pressX = false;
            Timing.RunCoroutine(_Typing(_tmpText));
        }

        public void EndInBattle()
        {
            _canvasAnim = SettingsController.Instance.Animator;
            _endInBattle = true;
        }


        private async void PassTextWithDelay(string inputText, float delayInSeconds)
        {
            var delayInMilliseconds = (int)(delayInSeconds * 1000);
            await Task.Delay(delayInMilliseconds);
            PassText(inputText);
        }

        private string ExtractPassTextPrefix(string input)
        {
            if (input.StartsWith("<passText>", StringComparison.Ordinal)) return "<passText>";

            if (!input.StartsWith("<passText=", StringComparison.Ordinal)) return null;
            var startIndex = "<passText=".Length;
            var endIndex = input.IndexOf('>', startIndex);

            if (endIndex <= startIndex) return null;
            var numberPart = input[startIndex..endIndex];
            return double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out _)
                ? input[..(endIndex + 1)]
                : null;
        }
    }
}