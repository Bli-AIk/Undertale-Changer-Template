using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using System;
using UnityEngine.Audio;
using TMPro;
/// <summary>
/// ���ֻ�ϵͳ
/// </summary>
public class TypeWritter : MonoBehaviour
{
    public string originString, endString;
    public bool isTyping;
    public int hpIn;
    public int hpSave;
    public bool canNotX;
    public bool pressX;
    public float clockTime;//ʵ���ϼ���
    public bool isStop;
    public int fx;//��Ч
    public bool fxRandomPitch;
    
    [Header("�����ٶ�����ͣ���ַ���Ĵ����ٶ�")]
    public float speed = 0.075f, speedSlow = 0.15f;
    [Header("���ֺ��������԰�X������0Ϊ������")]
    public float clock = 0.1f;//����
    public int passTextString;
    public bool passText;
    [Header("�о͹���")]
    public bool haveSpriteChanger;
    public SpriteChanger spriteChanger;
    [Header("����OW��")]
    public bool isOverworld;
    TalkUIPositionChanger talkUIPositionChanger;

    public float pitch = 1;
    public float volume = 0.5f;
    public AudioMixerGroup audioMixerGroup;

    public int useFont;
    [Header("������ôһЩ�����ҪǿӲ�ֶΣ���ǹ")]
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
        Normal,//�����Ĵ��ֻ�
        CantZ,//���ܰ�Z�Ĵ��ֻ���ʹ�ø��ı����п��ơ�
    }
    TypeMode typeMode = TypeMode.Normal;
    /// <summary>
    /// �������ֻ������������ڽ��У���ǿ����ֹ��
    /// һ������²���Ҫǿ�д�϶Ի���
    /// �����������к��� �Q �ַ���������hp��������0�����ַ���������
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
            talkUIPositionChanger.Change(true, originString[0] == '�C' && originString[1] != '-', true, this);

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

            if (originString[i] != '�Q')
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
                            default://���ı�
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
            
            while (originString[i] == '��')//����
            {
                endString += originString[i + 1];
                passTextString += 2;
                i += 2;
            }
            isStop = originString[i] == '��';
            if (originString[i] == '��')
            {
                canNotX = !canNotX;
            }
            else if (originString[i] == '�P')
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
                originString += 'ݞ';
                GetComponent<OverworldTalkSelect>().typeText = text;
                GetComponent<OverworldTalkSelect>().Open();
            }
            else if (originString[i] == '��')//ͣ��
            {
                if (!pressX)
                    yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f * Convert.ToInt32(!MainControl.instance.OverworldControl.textWidth));
               
            }
            else if (originString[i] == '�C')//����
            {
                string num = "";
                i++;
                while (originString[i] != '�C')
                {
                    num += originString[i];
                    i++;
                    passTextString++;
                }
                passTextString++;
                spriteChanger.ChangeImage(int.Parse(num));
                continue;
            }
            else if (originString[i] == '��')//�Ĵ��ֻ�FX
            {
                string num = "";
                i++;
                while (originString[i] != '��')
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
                if (originString[i] == '�Q'&& hpIn != 0)//���hp״̬�����ֶ�
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
                            if (plusString[j] == '�Q')
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
                        if (originString[j] != '�Q')
                        {
                            correctString += originString[j];
                        }
                    }

                    originString = correctString;
                }
                else if (originString[i] == 'ݞ')
                {
                    passText = true;
                }
                else 
                {
                    //string cantString = ",.:;!?������������ \n\r";
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
            else Debug.Log("��tmp_Text��");

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
        if (MainControl.instance.OverworldControl.isSetting || forceReturn)//pause��OW����ʱ�����
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
        else if (!pressX && !canNotX && MainControl.instance.KeyArrowToControl(KeyCode.X))//����
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
        if (originString[0] == 'ݞ')
        {
            originString = originString.Substring(1);
        }
        passTextString = 0;
        if (isOverworld)
        {
            talkUIPositionChanger.Change(true, originString[0] == '�C' && originString[1] != '-', true, this);
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
     * ������
    void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            TypeOpen("OK", false);
        }   
    }
    */
}
