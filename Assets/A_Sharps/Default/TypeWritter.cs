using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using System;

public class TypeWritter : MonoBehaviour
{
    public string originString, endString;
    public bool isTyping;
    public int hpIn;
    public bool pressX;
    public float clockTime;//ʵ���ϼ���
    public bool isStop;
    int fx;//��Ч
    [Header("�����ٶ�����ͣ���ַ���Ĵ����ٶ�")]
    public float speed = 0.075f, speedSlow = 0.15f;
    [Header("���ֺ��������԰�X������0Ϊ������")]
    public float clock;//����
    public int passTextString;
    public bool passText;
    [Header("�о͹���")]
    public bool haveSpriteChanger;
    public SpriteChanger spriteChanger;
    [Header("����OW��")]
    public bool isOverworld;
    TalkUIPositionChanger talkUIPositionChanger;

    

    private void Start()
    {
        if (isOverworld)
            talkUIPositionChanger = GameObject.Find("Main Camera/TalkUI").GetComponent<TalkUIPositionChanger>();
        if (haveSpriteChanger)
            spriteChanger = GetComponent<SpriteChanger>();
    }
    /// <summary>
    /// �������ֻ������������ڽ��У���ǿ����ֹ��
    /// һ������²���Ҫǿ�д�϶Ի���
    /// �����������к��� �Q �ַ���������hp��������0�����ַ���������
    /// </summary>
    public void TypeOpen(string text, bool force, int hp, int fx)
    {
        if (!force && isTyping)
            return;
        else
            StopAllCoroutines();
        passText = false;
        endString = "";
        passTextString = 0;
        originString = text;
        hpIn = hp;
        clockTime = clock;
        pressX = false;
        isStop = false;
        this.fx = fx;
        Timing.RunCoroutine(_Typing());
    }
    public void TypeStop()
    {
        isTyping = false;
        Timing.KillCoroutines();
        endString = "";
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
    IEnumerator<float> _Typing()
    {
        isTyping = true;

        for (int i = 0; i < originString.Length; i++)
        {

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
                    switch (spText)
                    {
                        default://���ı�
                            endString += spText;
                            passTextString += spText.Length;
                            break;
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
            if (originString[i] == '��')//ͣ��
            {
                if (!pressX)
                    yield return Timing.WaitForSeconds(speedSlow - speedSlow * 0.25f * Convert.ToInt32(!MainControl.instance.OverwroldControl.textWidth));
               
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
                    if (hpIn + MainControl.instance.PlayerControl.hp >= MainControl.instance.PlayerControl.hpMax)
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
                    break;
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
                        AudioController.instance.GetFx(fx, MainControl.instance.AudioControl.fxClipType);
                    endString += originString[i];
                }

                if (!pressX)
                    yield return Timing.WaitForSeconds(speed - speed * 0.25f * Convert.ToInt32(!MainControl.instance.OverwroldControl.textWidth));
            }

        }
        if (!passText)
        {
            isTyping = false;
        }
        else
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
        isStop = false;
    }
    private void Update()
    {



        if (MainControl.instance.OverwroldControl.isSetting)//pause��OW����ʱ�����
            return;

        if (clockTime > 0)
            clockTime -= Time.deltaTime;
        if (!passText && !isTyping && MainControl.instance.KeyArrowToControl(KeyCode.Z))
        {
            if (haveSpriteChanger)
                spriteChanger.ChangeImage(-1);
            if (endInBattle)
                canvasAnim.SetBool("Open", true);

        }
        if (passText && (MainControl.instance.KeyArrowToControl(KeyCode.Z)))
        {
            endString = "";
            passText = !passText;
            if (originString[0] == 'ݞ')
            {
                originString = originString.Substring(1);
            }
            passTextString = 0;
            if (isOverworld)
                talkUIPositionChanger.Change(true, originString[0] == '�C' && originString[1] != '-', true, this);
            pressX = false;
            Timing.RunCoroutine(_Typing());


        }
        else if (!pressX && MainControl.instance.KeyArrowToControl(KeyCode.X))//����
        {
            if (clock != 0 && clockTime <= 0 && isTyping)
                pressX = true;
        }

    }
    bool endInBattle;
    Animator canvasAnim;
    public void EndInBattle()
    {
        canvasAnim = GameObject.Find("Canvas").GetComponent<Animator>();
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
