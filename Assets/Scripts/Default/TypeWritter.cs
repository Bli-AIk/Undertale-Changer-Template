using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using System;
using UnityEngine.Audio;
using TMPro;
/// <summary>
/// 打字机系统
/// </summary>
public class TypeWritter : MonoBehaviour
{
    public string originString, endString;
    public bool isTyping;
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
    public float clock = 0.1f;//设置
    public int passTextString;
    public bool passText;
    [Header("有就勾上")]
    public bool haveSpriteChanger;
    public SpriteChanger spriteChanger;
    [Header("适配OW框")]
    public bool isOverworld;
    TalkUIPositionChanger talkUIPositionChanger;

    public float pitch = 1;
    public float volume = 0.5f;
    public AudioMixerGroup audioMixerGroup;

    public int useFont;
    [Header("总有那么一些情况需要强硬手段（拔枪")]
    public bool forceReturn = false;

    TMP_Text tmp_Text;
    private void Start()
    {
        if (isOverworld)
            talkUIPositionChanger = GameObject.Find("Main Camera/TalkUI").GetComponent<TalkUIPositionChanger>();
        if (haveSpriteChanger)
            spriteChanger = GetComponent<SpriteChanger>();
    }

    public enum TypeMode
    {
        Normal,//正常的打字机
        CantZ,//不能按Z的打字机，使用富文本进行控制。
    }
    TypeMode typeMode = TypeMode.Normal;
    /// <summary>
    /// 开启打字机。若打字正在进行，可强行终止。
    /// 一般情况下不需要强行打断对话。
    /// 若传入的语句中含有 齉 字符，请输入hp。若输入0，此字符将跳过。
    /// </summary>
    public void TypeOpen(string text, bool force, int hp, int fx, TMP_Text tmp_Text, TypeMode typeMode = TypeMode.Normal)
    {
        this.typeMode = typeMode;

        if (!force && isTyping)
            return;
        else
            StopAllCoroutines();
        passText = false;
        endString = "";
        tmp_Text.text = "";
        passTextString = 0;
        originString = text;
        hpIn = hp;
        hpSave = MainControl.instance.PlayerControl.hp;
        clockTime = clock;
        pressX = false;
        isStop = false;
        this.fx = fx;
        if (isOverworld)
            talkUIPositionChanger.Change(true, originString[0] == '鼵' && originString[1] != '-', true, this);

        this.tmp_Text = tmp_Text;
        Timing.RunCoroutine(_Typing(this.tmp_Text));
    }
    public void TypeStop()
    {
        Timing.KillCoroutines();
        isTyping = false;
        endString = "";
        if (tmp_Text != null)
        {
            tmp_Text.text = "";
        }
        passTextString = 0;
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
    IEnumerator<float> _Typing(TMP_Text tmp_Text)
    {

        isTyping = true;

        for (int i = 0; i < originString.Length; i++)
        {
            if (fxRandomPitch)
                pitch = UnityEngine.Random.Range(0.25f, 1.25f);

            if (originString[i] != '齉')
                passTextString++;
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
                    if (MainControl.instance.IsFrontCharactersMatch("<Font", spText))
                    {
                        string fontSave = spText.Substring(6);
                        fontSave = fontSave.Substring(0, fontSave.Length - 1);
                        useFont = int.Parse(fontSave);
                        tmp_Text.font = MainControl.instance.OverworldControl.tmpFonts[useFont];
                        passTextString += spText.Length;
                    }
                    else if (MainControl.instance.IsFrontCharactersMatch("<passText", spText))
                    {
                        string save = spText.Substring(10);
                        save = save.Substring(0, save.Length - 1);

                        Invoke(nameof(PassText), float.Parse(save));

                        passTextString += spText.Length;
                        passText = true;
                    }
                    else if (MainControl.instance.IsFrontCharactersMatch("<storyFade", spText))
                    {
                        string save = spText.Substring(11);
                        save = save.Substring(0, save.Length - 1);
                        StorySceneController.instance.Fade(int.Parse(save));
                        passTextString += spText.Length;
                    }
                    else
                    {
                        switch (spText)
                        {
                            case "<StoryStart1>":
                                GameObject.Find("Grid/Fog").GetComponent<Animator>().enabled = true;
                                passTextString += spText.Length;
                                break;
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
                            default://富文本
                                endString += spText;
                                passTextString += spText.Length;
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
            
            while (originString[i] == '粜')//跳字
            {
                endString += originString[i + 1];
                passTextString += 2;
                i += 2;
            }
            isStop = originString[i] == '龘';
            if (originString[i] == '鯈')
            {
                canNotX = !canNotX;
            }
            else if (originString[i] == '禤')
            {
                int q = i;
                string text = "";
                while (i < originString.Length - 1)
                {
                    i++;
                    text += originString[i];
                }
                i = q;
                originString = originString.Substring(0, originString.Length - text.Length);
                //passTextString--;
                originString += '轂';
                GetComponent<OverworldTalkSelect>().typeText = text;
                GetComponent<OverworldTalkSelect>().Open();
            }
            else if (originString[i] == '龘')//停顿
            {
                if (!pressX)
                    yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f * Convert.ToInt32(!MainControl.instance.OverworldControl.textWidth));
               
            }
            else if (originString[i] == '鼵')//变脸
            {
                string num = "";
                i++;
                while (originString[i] != '鼵')
                {
                    num += originString[i];
                    i++;
                    passTextString++;
                }
                passTextString++;
                spriteChanger.ChangeImage(int.Parse(num));
                continue;
            }
            else if (originString[i] == '菔')//改打字机FX
            {
                string num = "";
                i++;
                while (originString[i] != '菔')
                {
                    num += originString[i];
                    i++;
                    passTextString++;
                }
                passTextString++;
                fx = int.Parse(num);
                continue;
            }
            else
            {
                if (originString[i] == '齉'&& hpIn != 0)//检测hp状态并插字儿
                {
                    i -= 2;
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
                        string plusStringChanger = "";
                        for (int j = 0; j < plusString.Length; j++) 
                        {
                            if (plusString[j] == '齉')
                                plusStringChanger += hpIn;
                            else
                                plusStringChanger += plusString[j];

                        }
                        plusString = plusStringChanger;
                    }
                    string lengthString = "";
                    
                    if (originString.Length - (endString.Length + 1) > 0) 
                    {
                        lengthString = originString.Substring(endString.Length + 1, originString.Length - (endString.Length + 1));
                        
                    }
                    originString = endString + plusString + lengthString;
                    string correctString = "";
                    for (int j = 0; j < originString.Length; j++)
                    {
                        if (originString[j] != '齉')
                        {
                            correctString += originString[j];
                        }
                    }

                    originString = correctString;
                }
                else if (originString[i] == '轂')
                {
                    passText = true;
                }
                else 
                {
                    //string cantString = ",.:;!?，。：；！？ \n\r";
                    string cantString = "* \n\r";
                    for (int j = 0; j < cantString.Length; j++)
                    {
                        if(cantString[j] == originString[i])
                        {
                            cantString = "";
                        }
                    }
                    if (cantString != "" && !pressX)
                        AudioController.instance.GetFx(fx, MainControl.instance.AudioControl.fxClipType, volume, pitch, audioMixerGroup);
                    endString += originString[i];
                }

                if (passText)
                    break;

                if (!pressX)
                    yield return Timing.WaitForSeconds(speed - speed * 0.25f * Convert.ToInt32(!MainControl.instance.OverworldControl.textWidth));
            }

            if (tmp_Text != null)
            {

                tmp_Text.text = endString;
                if (tmp_Text.font != MainControl.instance.OverworldControl.tmpFonts[useFont])
                    tmp_Text.font = MainControl.instance.OverworldControl.tmpFonts[useFont];

            }
            else Debug.Log("你tmp_Text呢");

        }

        if (!passText)
        {
            isTyping = false;
        }
        else if (passTextString < originString.Length) 
        {
            if (originString[passTextString] != '<')
            {
                originString = originString.Substring(passTextString - 1);
            }
            else
            {
                originString = originString.Substring(passTextString);
            }
        }
        pressX = false;
        canNotX = false;
        isStop = false;
    }
    private void Update()
    {
        if (MainControl.instance.OverworldControl.isSetting || forceReturn)//pause在OW检测的时候会用
            return;

        if (clockTime > 0)
            clockTime -= Time.deltaTime;
        if (!passText && !isTyping && MainControl.instance.KeyArrowToControl(KeyCode.Z) && typeMode != TypeMode.CantZ)
        {
            if (haveSpriteChanger)
                spriteChanger.ChangeImage(-1);
            if (endInBattle)
                canvasAnim.SetBool("Open", true);

        }
        if (passText && MainControl.instance.KeyArrowToControl(KeyCode.Z) && typeMode != TypeMode.CantZ)
        {
            PassText();
        }
        else if (!pressX && !canNotX && MainControl.instance.KeyArrowToControl(KeyCode.X))//跳字
        {
            if (clock != 0 && clockTime <= 0 && isTyping)
                pressX = true;
        }

    }

    void PassText()
    {
        //Debug.Log(2);
        endString = "";
        if (tmp_Text != null)
        {
            tmp_Text.text = "";
        }
        passText = !passText;
        if (originString[0] == '轂')
        {
            originString = originString.Substring(1);
        }
        passTextString = 0;
        if (isOverworld)
        {
            talkUIPositionChanger.Change(true, originString[0] == '鼵' && originString[1] != '-', true, this);
        }
        pressX = false;
        Timing.RunCoroutine(_Typing(tmp_Text));
    }
    bool endInBattle;
    Animator canvasAnim;
    public void EndInBattle()
    {
        canvasAnim = CanvasController.instance.animator;
        endInBattle = true;
    }
    /*
     * 测试用
    void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            TypeOpen("OK", false);
        }   
    }
    */
}
