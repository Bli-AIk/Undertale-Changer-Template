using System;
using System.Collections.Generic;
using MEC;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.Scene;
using UCT.Overworld;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace UCT.Global.UI
{
    /// <summary>
    /// 打字机系统
    /// </summary>
    public class TypeWritter : MonoBehaviour
    {
        public string originString, endString, passTextString;
        public bool isRunning;//打字机是否在运行
        public bool isTyping;//是否在 打出字符
        public int hpIn;
        public int hpSave;
        public bool canNotX;
        public bool pressX;
        public float clockTime;//实际上计数
        public bool isStop;
        public int fx;//音效
        public bool fxRandomPitch;

        [Header("打字速度与检测停顿字符后的打字速度")]
        public float speed = 0.075f, speedSlow = 0.15f;

        [Header("打字后多少秒可以按X跳过，0为不能跳")]
        public float clock = 0.01f;//设置

        public bool passText;


        public SpriteChanger spriteChanger;

        [Header("适配OW框")]
        public bool isOverworld;

        private TalkUIPositionChanger _talkUIPositionChanger;

        public float pitch = 1;
        public float volume = 0.5f;
        public AudioMixerGroup audioMixerGroup;

        [Header("字体")]
        public int useFont;

        [Header("打字动效")]
        public OverworldControl.DynamicType dynamicType;

        [Header("总有那么一些情况需要强硬手段（拔枪")]
        public bool forceReturn;

        private TMP_Text _tmpText;

        private void Start()
        {
            if (isOverworld)
                _talkUIPositionChanger = TalkUIPositionChanger.Instance;
            spriteChanger = GetComponent<SpriteChanger>();
        }

        public enum TypeMode
        {
            Normal,//正常的打字机
            CantZx,//不能按ZX的打字机，使用富文本进行控制。
        }

        private TypeMode _typeMode = TypeMode.Normal;

        /// <summary>
        /// 开启打字机。若打字正在进行，可强行终止。
        /// 一般情况下不需要强行打断对话。
        /// 若传入的语句中含有<autoFood>，请输入hp。若输入0，此字符将跳过。
        /// </summary>
        public void TypeOpen(string text, bool force, int hp, int fx, TMP_Text tmpText, TypeMode typeMode = TypeMode.Normal)
        {
            isRunning = true;
            this._typeMode = typeMode;

            if (!force && isTyping)
                return;
            StopAllCoroutines();
            passText = false;
            endString = "";
            tmpText.text = "";
            passTextString = "";
            originString = text;
            hpIn = hp;
            hpSave = MainControl.Instance.PlayerControl.hp;
            clockTime = clock;
            pressX = false;
            isStop = false;
            this.fx = fx;
            if (isOverworld)
                _talkUIPositionChanger.Change(true, originString.Substring(0, "<passText>".Length) == "<passText>", true, this);

            this._tmpText = tmpText;
            Timing.RunCoroutine(_Typing(this._tmpText));
        }

        public void TypeStop()
        {
            isRunning = false;
            Timing.KillCoroutines();
            isTyping = false;
            endString = "";
            if (_tmpText != null)
            {
                _tmpText.text = "";
            }
            passTextString = "";
        }

        public void TypePause(bool pause)
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

        private bool _isUsedFx;

        private IEnumerator<float> _Typing(TMP_Text tmpText)
        {
            isRunning = true;
            for (int i = 0; i < originString.Length; i++)
            {
                if (spriteChanger != null)
                    spriteChanger.justSaying = false;

                isTyping = true;
                _isUsedFx = false;

                if (fxRandomPitch)
                    pitch = Random.Range(0.25f, 1.25f);


                if (originString[i] == '<')
                {
                    bool fix0 = false;
                    if (i == 0)
                        fix0 = true;

                    while (originString[i] == '<')
                    {
                        string spText = "";
                        while (fix0 || originString[i - 1] != '>')
                        {
                            spText += originString[i];
                            i++;
                            if (fix0)
                                fix0 = false;
                        }

                        passTextString += spText;

                        if (MainControl.Instance.IsFrontCharactersMatch("<fx=", spText))
                        {
                            string save = spText.Substring(4);
                            save = save.Substring(0, save.Length - 1);
                            fx = int.Parse(save);
                        }
                        else if (MainControl.Instance.IsFrontCharactersMatch("<font=", spText))
                        {
                            string save = spText.Substring(6);
                            save = save.Substring(0, save.Length - 1);
                            useFont = int.Parse(save);
                            tmpText.font = MainControl.Instance.OverworldControl.tmpFonts[useFont];
                        }
                        else if (MainControl.Instance.IsFrontCharactersMatch("<stop*", spText))
                        {
                            if (!pressX)
                            {
                                isTyping = false;
                                float number = float.Parse(spText.Substring(6, spText.Length - 7));
                                for (int p = 0; p < number; p++)
                                {
                                    if (pressX)
                                        break;
                                    yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f * Convert.ToInt32(!MainControl.Instance.OverworldControl.textWidth));
                                }
                                isTyping = false;
                            }

                            isStop = true;
                        }
                        else if (MainControl.Instance.IsFrontCharactersMatch("<image=", spText))
                        {
                            string save = spText.Substring(7);
                            save = save.Substring(0, save.Length - 1);
                            int s = int.Parse(save);
                            if (spriteChanger != null)
                                spriteChanger.ChangeImage(s);
                            _talkUIPositionChanger.Change(true, s >= 0, false);
                        }
                        else if (MainControl.Instance.IsFrontCharactersMatch("<stop...*", spText))
                        {
                            if (!pressX)
                            {
                                isTyping = false;

                                float number = float.Parse(spText.Substring(9, spText.Length - 10));
                                for (int l = 0; l < 3; l++)
                                {
                                    for (int p = 0; p < number; p++)
                                    {
                                        if (pressX)
                                            break;
                                        yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f * Convert.ToInt32(!MainControl.Instance.OverworldControl.textWidth));
                                    }
                                    AudioController.Instance.GetFx(fx, MainControl.Instance.AudioControl.fxClipType, volume, pitch, audioMixerGroup);
                                    endString += '.';
                                    tmpText.text = endString;
                                }
                            }
                            else endString += "...";
                            isStop = true;
                            tmpText.text = endString;
                        }
                        else if (MainControl.Instance.IsFrontCharactersMatch("<passText=", spText))
                        {
                            string save = spText.Substring(10);
                            save = save.Substring(0, save.Length - 1);

                            Invoke(nameof(PassText), float.Parse(save));

                            passText = true;
                        }
                        else if (MainControl.Instance.IsFrontCharactersMatch("<storyFade", spText))
                        {
                            string save = spText.Substring(11);
                            save = save.Substring(0, save.Length - 1);
                            StorySceneController.Instance.Fade(int.Parse(save));
                        }
                        else if (MainControl.Instance.IsFrontCharactersMatch("<stop......*", spText))
                        {
                            if (!pressX)
                            {
                                isTyping = false;

                                float number = float.Parse(spText.Substring(12, spText.Length - 13));
                                for (int l = 0; l < 6; l++)
                                {
                                    for (int p = 0; p < number; p++)
                                    {
                                        if (pressX)
                                            break;
                                        yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f * Convert.ToInt32(!MainControl.Instance.OverworldControl.textWidth));
                                    }
                                    AudioController.Instance.GetFx(fx, MainControl.Instance.AudioControl.fxClipType, volume, pitch, audioMixerGroup);
                                    endString += '.';
                                    tmpText.text = endString;
                                }
                            }
                            else endString += "......";
                            isStop = true;
                            tmpText.text = endString;
                        }
                        else
                        {
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
                                    MainControl.Instance.OutBlack("Start", Color.black, true);
                                    break;

                                case "<sprite=0>":
                                    if (!pressX)
                                        AudioController.Instance.GetFx(fx, MainControl.Instance.AudioControl.fxClipType, volume, pitch, audioMixerGroup);
                                    goto default;
                                case "<stop>":
                                    if (!pressX)
                                    {
                                        //单独一个Stop的时候，不设置isTyping，这是因为有的时候这个stop的时间很短，如果true看起来有点怪。
                                        //如果需要长的Stop，建议你还是使用<stop*x>的方式来做。
                                        //isTyping = false;
                                        yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f * Convert.ToInt32(!MainControl.Instance.OverworldControl.textWidth));
                                    }
                                    isStop = true;
                                    break;

                                case "<autoFood>":

                                    string plusString;
                                    if (hpIn + hpSave >= MainControl.Instance.PlayerControl.hpMax)
                                    {
                                        plusString = MainControl.Instance.ItemControl.itemTextMaxData[22];
                                        plusString = plusString.Substring(0, plusString.Length - 1);
                                    }
                                    else
                                    {
                                        plusString = MainControl.Instance.ItemControl.itemTextMaxData[12];
                                        plusString = plusString.Substring(0, plusString.Length - 1);
                                    }
                                    originString = MainControl.Instance.StringRemover(originString, i - "<autoFood>".Length, i - 1, plusString);
                                    i -= spText.Length;
                                    passTextString = passTextString.Substring(0, passTextString.Length - spText.Length);
                                    break;

                                case "<changeX>":
                                    canNotX = !canNotX;
                                    break;

                                case "<passText>":
                                    passText = true;
                                    passTextString = passTextString.Substring(0, passTextString.Length - spText.Length);
                                    //passTextString += spText.Length * 2 - 5;
                                    goto PassText;
                                default://富文本

                                    if (spText.Length - 2 > 0 && spText[1] == '-' && spText[spText.Length - 2] == '-')
                                    {
                                        spText = spText.Substring(2, spText.Length - 4);
                                        if (!pressX)
                                        {
                                            AudioController.Instance.GetFx(fx, MainControl.Instance.AudioControl.fxClipType, volume, pitch, audioMixerGroup);
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

                string cantString = "* \n\r";
                for (int j = 0; j < cantString.Length; j++)
                {
                    if (cantString[j] == originString[i])
                    {
                        cantString = "";
                    }
                }
                if (cantString != "" && !pressX && !_isUsedFx)
                    AudioController.Instance.GetFx(fx, MainControl.Instance.AudioControl.fxClipType, volume, pitch, audioMixerGroup);

                if (!passText)
                {
                    endString += originString[i];
                    passTextString += originString[i];
                
                }

                if (!pressX)
                    yield return Timing.WaitForSeconds(speed - speed * 0.25f * Convert.ToInt32(!MainControl.Instance.OverworldControl.textWidth));

                if (tmpText != null)
                {
                    tmpText.text = endString;

                    Timing.RunCoroutine(_Dynamic(endString.Length - 1, dynamicType));
                

                    if (tmpText.font != MainControl.Instance.OverworldControl.tmpFonts[useFont])
                        tmpText.font = MainControl.Instance.OverworldControl.tmpFonts[useFont];
                }
                else Other.Debug.Log("缺失tmp_Text", "#FFFF00");

                if (!passText)
                {
                    isTyping = false;
                }
                else
                {
                    if (originString[passTextString.Length] != '<')
                    {
                        originString = originString.Substring(passTextString.Length - 1);
                        i -= passTextString.Length - 1;
                    }
                    else// == '<'
                    {
                        originString = originString.Substring(passTextString.Length);
                        i -= passTextString.Length;

                    }
                    break;
                }
                //pressX = false;
                canNotX = false;
                isStop = false;
                PassText:;//这是个标签注意
            }

            isRunning = false;
            if (originString.Length > "<passText>".Length)
                originString = originString.Substring("<passText>".Length);
        }

        List<Vector2> _dynamicPos;

        IEnumerator<float> _Dynamic(int number, OverworldControl.DynamicType dynamicType)
        {
            if (dynamicType != OverworldControl.DynamicType.None)//动效相关
            {
                var textInfo = _tmpText.textInfo;

                Vector3 orig;
                TMP_CharacterInfo charInfo;
                Vector3[] verts;
                Color32[] colors;

                switch (dynamicType)
                {
                    case OverworldControl.DynamicType.Shake:
                        for (int i = 0; i < 30; i++)
                        {
                            if (pressX)
                                break;

                            _tmpText.ForceMeshUpdate();

                            Vector3 randomer = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);

                            charInfo = textInfo.characterInfo[number];

                            if (!charInfo.isVisible) break;

                            verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                            for (int j = 0; j < 4; j++)
                            {
                                orig = verts[charInfo.vertexIndex + j];
                                //动画
                                verts[charInfo.vertexIndex + j] = orig + randomer;
                            }

                            for (int k = 0; k < textInfo.meshInfo.Length; k++)
                            {
                                var meshInfo = textInfo.meshInfo[k];
                                meshInfo.mesh.vertices = meshInfo.vertices;
                                _tmpText.UpdateGeometry(meshInfo.mesh, k);
                            }
                            yield return 0;
                        }


                        break;
                    case OverworldControl.DynamicType.Fade:

                        float fadeDuration = 0.1f; // 渐入时间
                        Color32 startColor;
                        Color32 endColor;

                        _tmpText.ForceMeshUpdate();

                        charInfo = textInfo.characterInfo[number];
                        if (!charInfo.isVisible) break;

                        colors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;
                        startColor = colors[charInfo.vertexIndex];
                        endColor = new Color32(startColor.r, startColor.g, startColor.b, 255);

                        // 设置初始颜色为透明
                        for (int j = 0; j < 4; j++)
                        {
                            colors[charInfo.vertexIndex + j] = new Color32(startColor.r, startColor.g, startColor.b, 0);
                        }

                        _tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                        float elapsedTime = 0f;
                        while (elapsedTime < fadeDuration)
                        {

                            if (pressX)
                                break;
                            elapsedTime += Time.deltaTime;
                            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

                            Color32 currentColor = Color32.Lerp(new Color32(startColor.r, startColor.g, startColor.b, 0), endColor, alpha);

                            for (int j = 0; j < 4; j++)
                            {
                                colors[charInfo.vertexIndex + j] = currentColor;
                            }

                            _tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                            yield return 0;
                        }
                        break;

                    case OverworldControl.DynamicType.Up:

                        for (int i = 0; i < 30; i++)
                        {
                            if (pressX)
                                break;

                            _tmpText.ForceMeshUpdate();

                            Vector3 down = new Vector3(0, -0.1f);

                            charInfo = textInfo.characterInfo[number];

                            if (!charInfo.isVisible) break;

                            verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                            for (int j = 0; j < 4; j++)
                            {
                                orig = verts[charInfo.vertexIndex + j];
                                //动画
                                verts[charInfo.vertexIndex + j] = orig + down * (1 - (float)i / 30);
                            }

                            for (int k = 0; k < textInfo.meshInfo.Length; k++)
                            {
                                var meshInfo = textInfo.meshInfo[k];
                                meshInfo.mesh.vertices = meshInfo.vertices;
                                _tmpText.UpdateGeometry(meshInfo.mesh, k);
                            }

                            yield return 0;
                        }
                        break;
                }

            }
            yield return 0;
            _tmpText.ForceMeshUpdate();
        }
        private void Update()
        {
            if (MainControl.Instance.OverworldControl.isSetting || forceReturn)//pause在OW检测的时候会用
                return;

            if (clockTime > 0)
                clockTime -= Time.deltaTime;
            if (!isRunning && !passText && !isTyping && MainControl.Instance.KeyArrowToControl(KeyCode.Z) && _typeMode != TypeMode.CantZx)
            {
                if (spriteChanger != null)
                    spriteChanger.ChangeImage(-1);
                if (_endInBattle)
                    _canvasAnim.SetBool("Open", true);
            }
            if (passText && MainControl.Instance.KeyArrowToControl(KeyCode.Z) && _typeMode != TypeMode.CantZx)
            {
                PassText();
            }
            else if (!pressX && !canNotX && MainControl.Instance.KeyArrowToControl(KeyCode.X) && _typeMode != TypeMode.CantZx)//跳字
            {
                if (clock != 0 && clockTime <= 0)
                {
                    pressX = true;
                }
            }




        }

        private void PassText()
        {
            endString = "";
            if (_tmpText != null)
            {
                _tmpText.text = "";
            }
            passText = false;

            passTextString = "";
            if (isOverworld)
            {
                _talkUIPositionChanger.Change(false, false, true, this);
                if (originString.Substring(0, "<passText>".Length) == "<passText>")
                    originString = originString.Substring("<passText>".Length);
            }
            pressX = false;
            Timing.RunCoroutine(_Typing(_tmpText));
        }

        private bool _endInBattle;
        private Animator _canvasAnim;

        public void EndInBattle()
        {
            _canvasAnim = CanvasController.Instance.animator;
            _endInBattle = true;
        }
    }
}