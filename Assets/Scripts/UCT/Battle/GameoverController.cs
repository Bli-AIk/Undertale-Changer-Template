using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Battle
{
    /// <summary>
    /// Gameover控制器
    /// </summary>
    public class GameoverController : MonoBehaviour
    {
        private GameObject player;
        private ParticleSystem m_ParticleSystem;
        private AudioSource bgm;
        private TypeWritter typeWritter;
        public List<AudioClip> clips;
        private TextMeshPro tmp;
        public bool canChangeSence, canChangeSenceForC;

        private bool foolDay;

        private void Start()
        {
            canChangeSence = false;
            canChangeSenceForC = true;
            typeWritter = GetComponent<TypeWritter>();
            m_ParticleSystem = transform.Find("Player/Particle System").GetComponent<ParticleSystem>();
            tmp = transform.Find("Text Options").GetComponent<TextMeshPro>();
            player = m_ParticleSystem.transform.parent.gameObject;

            m_ParticleSystem.transform.localPosition = new Vector3(0, 0, -5);
            foolDay = DateTime.Now.Month == 4 && DateTime.Now.Day == 1;
            bgm = AudioController.instance.audioSource;
            bgm.clip = clips[Convert.ToInt32(foolDay)];
            player.transform.position = MainControl.Instance.OverworldControl.playerDeadPos;
            m_ParticleSystem.transform.position = MainControl.Instance.OverworldControl.playerDeadPos;
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
                AudioController.instance.GetFx(i, MainControl.Instance.AudioControl.fxClipUI);
        }

        public void StartParticleSystem()
        {
            m_ParticleSystem.transform.position = MainControl.Instance.OverworldControl.playerDeadPos;
            m_ParticleSystem.gameObject.SetActive(true);
            m_ParticleSystem.Play();
        }

        public void Type()
        {
            List<string> strings = new List<string>
            {
                MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "GameOver1"),
                MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "GameOver2"),
                MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "GameOver3"),
                MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "GameOver4")
            };
            typeWritter.TypeOpen(strings[Random.Range(0, 4)], false, 0, 4, tmp);
            canChangeSence = true;
        }

        public void Follish()
        {
            if (foolDay)
            {
                var main = m_ParticleSystem.main;
                main.loop = true;
                main.startLifetime = Random.Range(1.5f, 3);
                var emission = m_ParticleSystem.emission;
                emission.rateOverDistance = Random.Range(5, 51);
                //m_ParticleSystem.transform.position = new Vector3(UnityEngine.Random.Range(-6.85f, 6.85f), UnityEngine.Random.Range(-5.25f, 5.25f));
                float time = Random.Range(0.5f, 1f);

                m_ParticleSystem.transform.DOMoveX(Random.Range(-6.85f, 6.85f), time).SetEase((Ease)Random.Range(1, 35));
                m_ParticleSystem.transform.DOMoveY(Random.Range(-5.25f, 5.25f), time).SetEase((Ease)Random.Range(1, 35)).OnKill(Follish);
            }
        }

        private void Update()
        {
            if (!typeWritter.isTyping && MainControl.Instance.KeyArrowToControl(KeyCode.Z) && canChangeSence)
            {
                tmp.text = "";
                MainControl.Instance.OutBlack("Example-Corridor", Color.black, true, 2);
                canChangeSence = false;
            }

            if (MainControl.Instance.KeyArrowToControl(KeyCode.C) && canChangeSenceForC)
            {
                MainControl.Instance.OutBlack("Example-Corridor", Color.black, true);
                typeWritter.TypeStop();
                canChangeSenceForC = false;
            }
        }
    }
}