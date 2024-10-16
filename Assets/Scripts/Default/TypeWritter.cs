using Log;
using MEC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using static Unity.Burst.Intrinsics.X86.Avx;
using Random = UnityEngine.Random;

/// <summary>
/// Typewriter system
/// </summary>
public class TypeWritter : MonoBehaviour
{
    public string originString, endString, passTextString;
    public bool isRunning;
    //Is the typewriter running?
    public bool isTyping;
    //Is the character typed in the
    public int hpIn;
    public int hpSave;
    public bool canNotX;
    public bool pressX;
    public float clockTime;
    //actually counting
    public bool isStop;
    public int fx;
    //Sound effects
    public bool fxRandomPitch;

    [Header("Typing speed with detection of stop characters")]
    public float speed = 0.075f, speedSlow = 0.15f;

    [Header("How many seconds after typing can you press X to skip, 0 is not skippable")]
    public float clock = 0.01f;
    //Setting

    public bool passText;


    public SpriteChanger spriteChanger;

    [Header("Adapt OW box")]
    public bool isOverworld;

    private TalkUIPositionChanger talkUIPositionChanger;

    public float pitch = 1;
    public float volume = 0.5f;
    public AudioMixerGroup audioMixerGroup;

    [Header("Font")]
    public int useFont;

    [Header("Typing animation")]
    public OverworldControl.DynamicType dynamicType;

    [Header("There will always be situations that require strong-arm tactics (drawing guns)")]
    public bool forceReturn = false;

    private TMP_Text tmp_Text;

    private void Start()
    {
        if (isOverworld)
            talkUIPositionChanger = TalkUIPositionChanger.instance;
        spriteChanger = GetComponent<SpriteChanger>();
    }

    public enum TypeMode
    {
        Normal,
        //Normal typewriter
        CantZX,
        //Can't press ZX's typewriter, use rich text for control.
    }

    private TypeMode typeMode = TypeMode.Normal;

    /// <summary>
    /// Starts the typewriter. If typing is in progress, it can be forcibly terminated.
    /// Generally there is no need to forcefully interrupt a conversation.
    /// If the incoming statement contains <autoFood>, enter hp. If you enter 0, this character is skipped.
    /// </summary>
    public void TypeOpen(string text, bool force, int hp, int fx, TMP_Text tmp_Text, TypeMode typeMode = TypeMode.Normal)
    {
        isRunning = true;
        this.typeMode = typeMode;

        if (!force && isTyping)
            return;
        else
            StopAllCoroutines();
        passText = false;
        endString = "";
        tmp_Text.text = "";
        passTextString = "";
        originString = text;
        hpIn = hp;
        hpSave = MainControl.instance.PlayerControl.hp;
        clockTime = clock;
        pressX = false;
        isStop = false;
        this.fx = fx;
        if (isOverworld)
            talkUIPositionChanger.Change(true, originString.Substring(0, "<passText>".Length) == "<passText>", true, this);

        this.tmp_Text = tmp_Text;
        Timing.RunCoroutine(_Typing(this.tmp_Text));
    }

    public void TypeStop()
    {
        isRunning = false;
        Timing.KillCoroutines();
        isTyping = false;
        endString = "";
        if (tmp_Text != null)
        {
            tmp_Text.text = "";
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

    private bool isUsedFx;

    private IEnumerator<float> _Typing(TMP_Text tmp_Text)
    {
        isRunning = true;
        for (int i = 0; i < originString.Length; i++)
        {
            if (spriteChanger != null)
                spriteChanger.justSaying = false;

            isTyping = true;
            isUsedFx = false;

            if (fxRandomPitch)
                pitch = UnityEngine.Random.Range(0.25f, 1.25f);


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

                    if (MainControl.instance.IsFrontCharactersMatch("<fx=", spText))
                    {
                        string save = spText.Substring(4);
                        save = save.Substring(0, save.Length - 1);
                        fx = int.Parse(save);
                    }
                    else if (MainControl.instance.IsFrontCharactersMatch("<font=", spText))
                    {
                        string save = spText.Substring(6);
                        save = save.Substring(0, save.Length - 1);
                        useFont = int.Parse(save);
                        tmp_Text.font = MainControl.instance.OverworldControl.tmpFonts[useFont];
                    }
                    else if (MainControl.instance.IsFrontCharactersMatch("<stop*", spText))
                    {
                        if (!pressX)
                        {
                            isTyping = false;
                            float num = float.Parse(spText.Substring(6, spText.Length - 7));
                            for (int p = 0; p < num; p++)
                            {
                                if (pressX)
                                    break;
                                yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f * Convert.ToInt32(!MainControl.instance.OverworldControl.textWidth));
                            }
                            isTyping = false;
                        }

                        isStop = true;
                    }
                    else if (MainControl.instance.IsFrontCharactersMatch("<image=", spText))
                    {
                        string save = spText.Substring(7);
                        save = save.Substring(0, save.Length - 1);
                        int s = int.Parse(save);
                        if (spriteChanger != null)
                            spriteChanger.ChangeImage(s);
                        talkUIPositionChanger.Change(true, s >= 0, false);
                    }
                    else if (MainControl.instance.IsFrontCharactersMatch("<stop...*", spText))
                    {
                        if (!pressX)
                        {
                            isTyping = false;

                            float num = float.Parse(spText.Substring(9, spText.Length - 10));
                            for (int l = 0; l < 3; l++)
                            {
                                for (int p = 0; p < num; p++)
                                {
                                    if (pressX)
                                        break;
                                    yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f * Convert.ToInt32(!MainControl.instance.OverworldControl.textWidth));
                                }
                                AudioController.instance.GetFx(fx, MainControl.instance.AudioControl.fxClipType, volume, pitch, audioMixerGroup);
                                endString += '.';
                                tmp_Text.text = endString;
                            }
                        }
                        else endString += "...";
                        isStop = true;
                        tmp_Text.text = endString;
                    }
                    else if (MainControl.instance.IsFrontCharactersMatch("<passText=", spText))
                    {
                        string save = spText.Substring(10);
                        save = save.Substring(0, save.Length - 1);

                        Invoke(nameof(PassText), float.Parse(save));

                        passText = true;
                    }
                    else if (MainControl.instance.IsFrontCharactersMatch("<storyFade", spText))
                    {
                        string save = spText.Substring(11);
                        save = save.Substring(0, save.Length - 1);
                        StorySceneController.instance.Fade(int.Parse(save));
                    }
                    else if (MainControl.instance.IsFrontCharactersMatch("<stop......*", spText))
                    {
                        if (!pressX)
                        {
                            isTyping = false;

                            float num = float.Parse(spText.Substring(12, spText.Length - 13));
                            for (int l = 0; l < 6; l++)
                            {
                                for (int p = 0; p < num; p++)
                                {
                                    if (pressX)
                                        break;
                                    yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f * Convert.ToInt32(!MainControl.instance.OverworldControl.textWidth));
                                }
                                AudioController.instance.GetFx(fx, MainControl.instance.AudioControl.fxClipType, volume, pitch, audioMixerGroup);
                                endString += '.';
                                tmp_Text.text = endString;
                            }
                        }
                        else endString += "......";
                        isStop = true;
                        tmp_Text.text = endString;
                    }
                    else
                    {
                        switch (spText)
                        {
                            case "<storyMaskT>":
                                StorySceneController.instance.mask.SetActive(true);
                                break;

                            case "<storyMaskF>":
                                StorySceneController.instance.mask.SetActive(false);
                                break;

                            case "<storyExit>":
                                TypeStop();
                                MainControl.instance.OutBlack("Start", Color.black, true);
                                break;

                            case "<sprite=0>":
                                if (!pressX)
                                    AudioController.instance.GetFx(fx, MainControl.instance.AudioControl.fxClipType, volume, pitch, audioMixerGroup);
                                goto default;
                            case "<stop>":
                                if (!pressX)
                                {
                                    // A separate Stop without setting isTyping, this is because there are times when this stop is very short and it looks a bit weird if true.
                                    //If you need a long Stop, it is recommended that you still do it the <stop*x> way.
                                    //isTyping = false.
                                    yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f * Convert.ToInt32(!MainControl.instance.OverworldControl.textWidth));
                                }
                                isStop = true;
                                break;

                            case "<autoFood>":

                                string plusString;
                                if (hpIn + hpSave >= MainControl.instance.PlayerControl.hpMax)
                                {
                                    plusString = MainControl.instance.ItemControl.itemTextMaxData[22];
                                    plusString = plusString.Substring(0, plusString.Length - 1);
                                }
                                else
                                {
                                    plusString = MainControl.instance.ItemControl.itemTextMaxData[12];
                                    plusString = plusString.Substring(0, plusString.Length - 1);
                                }
                                originString = MainControl.instance.StringRemover(originString, i - "<autoFood>".Length, i - 1, plusString);
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
                            default:
                            // Rich Text

                                if (spText.Length - 2 > 0 && spText[1] == '-' && spText[spText.Length - 2] == '-')
                                {
                                    spText = spText.Substring(2, spText.Length - 4);
                                    if (!pressX)
                                    {
                                        AudioController.instance.GetFx(fx, MainControl.instance.AudioControl.fxClipType, volume, pitch, audioMixerGroup);
                                        isUsedFx = true;

                                        if (spriteChanger != null)
                                            spriteChanger.justSaying = true;
                                    }
                                }

                                endString += spText;
                                tmp_Text.text = endString;

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

            //string cantString = ",. :;! ,. :;! \n\r";

            string cantString = "* \n\r";
            for (int j = 0; j < cantString.Length; j++)
            {
                if (cantString[j] == originString[i])
                {
                    cantString = "";
                }
            }
            if (cantString != "" && !pressX && !isUsedFx)
                AudioController.instance.GetFx(fx, MainControl.instance.AudioControl.fxClipType, volume, pitch, audioMixerGroup);

            if (!passText)
            {
                endString += originString[i];
                passTextString += originString[i];

            }

            if (!pressX)
                yield return Timing.WaitForSeconds(speed - speed * 0.25f * Convert.ToInt32(!MainControl.instance.OverworldControl.textWidth));

            if (tmp_Text != null)
            {
                Timing.RunCoroutine(_Dynamic(endString.Length - 1));
                tmp_Text.text = endString;


                if (tmp_Text.font != MainControl.instance.OverworldControl.tmpFonts[useFont])
                    tmp_Text.font = MainControl.instance.OverworldControl.tmpFonts[useFont];
            }
            else DebugLogger.Log("缺失tmp_Text", DebugLogger.Type.war, "#FFFF00");

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
                else
                // == '<'
                {
                    originString = originString.Substring(passTextString.Length);
                    i -= passTextString.Length;

                }
                break;
            }
            //pressX = false.
            canNotX = false;
            isStop = false;
            PassText:;
            //This is a tag note
        }

        isRunning = false;
        if (originString.Length > "<passText>".Length)
            originString = originString.Substring("<passText>".Length);
    }

    private IEnumerator<float> _Dynamic(int num)
    {
        if (dynamicType != OverworldControl.DynamicType.None)
        //Kinetic effects related
        {
            var textInfo = tmp_Text.textInfo;

            Debug.LogWarning(num);
            Vector3 orig = default;
            TMP_CharacterInfo charInfo = default;
            Vector3[] verts = default;
            switch (dynamicType)
            {
                case OverworldControl.DynamicType.Shake:
                    for (int i = 0; i < 30; i++)
                    {
                        if (pressX)
                            break;

                        tmp_Text.ForceMeshUpdate();

                        Vector3 randomer = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);

                         charInfo = textInfo.characterInfo[num];

                        if (!charInfo.isVisible) break;

                        verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                        for (int j = 0; j < 4; j++)
                        {
                            orig = verts[charInfo.vertexIndex + j];
                            //Animation
                            verts[charInfo.vertexIndex + j] = orig + randomer;
                        }

                        for (int k = 0; k < textInfo.meshInfo.Length; k++)
                        {
                            var meshInfo = textInfo.meshInfo[k];
                            meshInfo.mesh.vertices = meshInfo.vertices;
                            tmp_Text.UpdateGeometry(meshInfo.mesh, k);
                        }
                        yield return 0;
                    }


                    break;
                case OverworldControl.DynamicType.Fade:
                    break;
                case OverworldControl.DynamicType.FadeUp:
                    break;
                case OverworldControl.DynamicType.Garbled:
                    break;
            }

        }
        yield return 0;
    }
    private void Update()
    {
        if (MainControl.instance.OverworldControl.isSetting || forceReturn)
        //pause will be used during OW detection
            return;

        if (clockTime > 0)
            clockTime -= Time.deltaTime;
        if (!isRunning && !passText && !isTyping && MainControl.instance.KeyArrowToControl(KeyCode.Z) && typeMode != TypeMode.CantZX)
        {
            if (spriteChanger != null)
                spriteChanger.ChangeImage(-1);
            if (endInBattle)
                canvasAnim.SetBool("Open", true);
        }
        if (passText && MainControl.instance.KeyArrowToControl(KeyCode.Z) && typeMode != TypeMode.CantZX)
        {
            PassText();
        }
        else if (!pressX && !canNotX && MainControl.instance.KeyArrowToControl(KeyCode.X) && typeMode != TypeMode.CantZX)
        //skipping words
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
        if (tmp_Text != null)
        {
            tmp_Text.text = "";
        }
        passText = false;

        passTextString = "";
        if (isOverworld)
        {
            talkUIPositionChanger.Change(false, false, true, this);
            if (originString.Substring(0, "<passText>".Length) == "<passText>")
                originString = originString.Substring("<passText>".Length);
        }
        pressX = false;
        Timing.RunCoroutine(_Typing(tmp_Text));
    }

    private bool endInBattle;
    private Animator canvasAnim;

    public void EndInBattle()
    {
        canvasAnim = CanvasController.instance.animator;
        endInBattle = true;
    }
}
