using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// 死了啦 都你害的啦）
/// </summary>
public class GameoverController : MonoBehaviour
{
    GameObject player;
    ParticleSystem m_ParticleSystem;
    AudioSource bgm;
    TypeWritter typeWritter;
    public List<AudioClip> clips;
    TextMeshPro tmp;
    public bool canChangeSence, canChangeSenceForC;

    bool foolDay;
    // Start is called before the first frame update
    void Start()
    {
        canChangeSence = false;
        canChangeSenceForC = true;
        typeWritter = GetComponent<TypeWritter>();
        m_ParticleSystem = transform.Find("Player/Particle System").GetComponent<ParticleSystem>();
        tmp = transform.Find("Text Options").GetComponent<TextMeshPro>();
        player = m_ParticleSystem.transform.parent.gameObject;
        
        m_ParticleSystem.transform.localPosition = new Vector3(0, 0, -5);
        foolDay = DateTime.Now.Month == 4 && DateTime.Now.Day == 1;
        bgm = GameObject.Find("BGM Source").GetComponent<AudioSource>();
        bgm.clip = clips[Convert.ToInt32(foolDay)];
        player.transform.position = MainControl.instance.PlayerControl.deadPos;
        m_ParticleSystem.transform.position = MainControl.instance.PlayerControl.deadPos;
        m_ParticleSystem.Pause();
        m_ParticleSystem.gameObject.SetActive(false);

    }

    //接下来交给Animator表演
    public void PlayFX(int i)
    {
        if (i < 0)
        {
            bgm.Play();
        }
        else
            AudioController.instance.GetFx(i, MainControl.instance.AudioControl.fxClipUI);

    }

    public void StartParticleSystem()
    {
        m_ParticleSystem.transform.position = MainControl.instance.PlayerControl.deadPos;
        m_ParticleSystem.gameObject.SetActive(true);
        m_ParticleSystem.Play();
    }

    public void Type()
    {
        List<string> strings = new List<string>
        {
            MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "GameOver1"),
            MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "GameOver2"),
            MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "GameOver3"),
            MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "GameOver4")
    };
        typeWritter.TypeOpen(strings[UnityEngine.Random.Range(0, 4)], false, 0, 4);
        canChangeSence = true;
    }
    public void Follish()
    {
        if (foolDay)
        {
            var main = m_ParticleSystem.main;
            main.loop = true;
            main.startLifetime = UnityEngine.Random.Range(1.5f, 3);
            var emission = m_ParticleSystem.emission;
            emission.rateOverDistance = UnityEngine.Random.Range(5, 51);
            //m_ParticleSystem.transform.position = new Vector3(UnityEngine.Random.Range(-6.85f, 6.85f), UnityEngine.Random.Range(-5.25f, 5.25f));
            float time = UnityEngine.Random.Range(0.5f, 1f);

            m_ParticleSystem.transform.DOMoveX(UnityEngine.Random.Range(-6.85f, 6.85f), time).SetEase((Ease)UnityEngine.Random.Range(1, 35));
            m_ParticleSystem.transform.DOMoveY(UnityEngine.Random.Range(-5.25f, 5.25f), time).SetEase((Ease)UnityEngine.Random.Range(1, 35)).OnKill(Follish);


        }
    }
    private void Update()
    {

        if (!typeWritter.isTyping && MainControl.instance.KeyArrowToControl(KeyCode.Z) && canChangeSence)
        {
            typeWritter.endString = "";
            MainControl.instance.OutBlack("Corridor", true, 2);
            canChangeSence = false;
        }

        if (MainControl.instance.KeyArrowToControl(KeyCode.C) && canChangeSenceForC)
        {
            MainControl.instance.OutBlack("Corridor", true);
            typeWritter.TypeStop();
            canChangeSenceForC = false;
        }
        tmp.text = typeWritter.endString;

    }
}
