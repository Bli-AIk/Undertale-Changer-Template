
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

    private TalkUIPositionChanger talkUIPositionChanger;

    public float pitch = 1;
    public float volume = 0.5f;
    public AudioMixerGroup audioMixerGroup;

    [Header("字体")]
    public int useFont;

    [Header("打字动效")]
    public OverworldControl.DynamicType dynamicType;

    [Header("总有那么一些情况需要强硬手段（拔枪")]
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
        Normal,//正常的打字机
        CantZX,//不能按ZX的打字机，使用富文本进行控制。
    }

    private TypeMode typeMode = TypeMode.Normal;

    /// <summary>
    /// 开启打字机。若打字正在进行，可强行终止。
    /// 一般情况下不需要强行打断对话。
    /// 若传入的语句中含有<autoFood>，请输入hp。若输入0，此字符将跳过。
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
                                    //单独一个Stop的时候，不设置isTyping，这是因为有的时候这个stop的时间很短，如果true看起来有点怪。
                                    //如果需要长的Stop，建议你还是使用<stop*x>的方式来做。
                                    //isTyping = false;
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
                            default://富文本

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

            //string cantString = ",.:;!?，。：；！？ \n\r";

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
                tmp_Text.text = endString;

                Timing.RunCoroutine(_Dynamic(endString.Length - 1, dynamicType));
                

                if (tmp_Text.font != MainControl.instance.OverworldControl.tmpFonts[useFont])
                    tmp_Text.font = MainControl.instance.OverworldControl.tmpFonts[useFont];
            }
            else Debug.Log("缺失tmp_Text", "#FFFF00");

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

    List<Vector2> dynamicPos;

    IEnumerator<float> _Dynamic(int num, OverworldControl.DynamicType dynamicType)
    {
        if (dynamicType != OverworldControl.DynamicType.None)//动效相关
        {
            var textInfo = tmp_Text.textInfo;

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

                        tmp_Text.ForceMeshUpdate();

                        Vector3 randomer = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);

                        charInfo = textInfo.characterInfo[num];

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
                            tmp_Text.UpdateGeometry(meshInfo.mesh, k);
                        }
                        yield return 0;
                    }


                    break;
                case OverworldControl.DynamicType.Fade:

                    float fadeDuration = 0.1f; // 渐入时间
                    Color32 startColor;
                    Color32 endColor;

                    tmp_Text.ForceMeshUpdate();

                    charInfo = textInfo.characterInfo[num];
                    if (!charInfo.isVisible) break;

                    colors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;
                    startColor = colors[charInfo.vertexIndex];
                    endColor = new Color32(startColor.r, startColor.g, startColor.b, 255);

                    // 设置初始颜色为透明
                    for (int j = 0; j < 4; j++)
                    {
                        colors[charInfo.vertexIndex + j] = new Color32(startColor.r, startColor.g, startColor.b, 0);
                    }

                    tmp_Text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

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

                        tmp_Text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                        yield return 0;
                    }
                    break;

                case OverworldControl.DynamicType.Up:

                    for (int i = 0; i < 30; i++)
                    {
                        if (pressX)
                            break;

                        tmp_Text.ForceMeshUpdate();

                        Vector3 down = new Vector3(0, -0.1f);

                        charInfo = textInfo.characterInfo[num];

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
                            tmp_Text.UpdateGeometry(meshInfo.mesh, k);
                        }

                        yield return 0;
                    }
                    break;
            }

        }
        yield return 0;
        tmp_Text.ForceMeshUpdate();
    }
    private void Update()
    {
        if (MainControl.instance.OverworldControl.isSetting || forceReturn)//pause在OW检测的时候会用
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
        else if (!pressX && !canNotX && MainControl.instance.KeyArrowToControl(KeyCode.X) && typeMode != TypeMode.CantZX)//跳字
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