using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace UCT.Battle
{
    /// <summary>
    /// Gameover控制器
    /// </summary>
    public class GameoverController : MonoBehaviour
    {
        public List<AudioClip> clips;
        public bool canChangeScene;
        public bool canChangeSceneForC;
        private GameObject _player;
        private ParticleSystem _mParticleSystem;
        private AudioSource _bgmSource;
        private TypeWritter _typeWritter;
        private TextMeshPro _textOptions;

        private bool _foolDay;

        private void Start()
        {
            canChangeScene = false;
            canChangeSceneForC = true;
            _typeWritter = GetComponent<TypeWritter>();
            _mParticleSystem = transform.Find("Player/Particle System").GetComponent<ParticleSystem>();
            _textOptions = transform.Find("Text Options").GetComponent<TextMeshPro>();
            _player = _mParticleSystem.transform.parent.gameObject;

            _mParticleSystem.transform.localPosition = new Vector3(0, 0, -5);
            _foolDay = DateTime.Now.Month == 4 && DateTime.Now.Day == 1;
            _bgmSource = AudioController.Instance.audioSource;
            _bgmSource.clip = clips[Convert.ToInt32(_foolDay)];
            _player.transform.position = MainControl.Instance.overworldControl.playerDeadPos;
            _mParticleSystem.transform.position = MainControl.Instance.overworldControl.playerDeadPos;
            _mParticleSystem.Pause();
            _mParticleSystem.gameObject.SetActive(false);
        }

        //接下来交给Animator表演
        public void PlayFX(int i)
        {
            if (i < 0)
            {
                _bgmSource.Play();
            }
            else
                AudioController.Instance.GetFx(i, MainControl.Instance.AudioControl.fxClipUI);
        }

        public void StartParticleSystem()
        {
            _mParticleSystem.transform.position = MainControl.Instance.overworldControl.playerDeadPos;
            _mParticleSystem.gameObject.SetActive(true);
            _mParticleSystem.Play();
        }

        public void Type()
        {
            var strings = new List<string>
            {
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.overworldControl.sceneTextsSave, "GameOver1"),
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.overworldControl.sceneTextsSave, "GameOver2"),
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.overworldControl.sceneTextsSave, "GameOver3"),
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.overworldControl.sceneTextsSave, "GameOver4")
            };
            _typeWritter.TypeOpen(strings[Random.Range(0, 4)], false, 0, 4, _textOptions);
            canChangeScene = true;
        }

        public void Follish()
        {
            if (_foolDay)
            {
                var main = _mParticleSystem.main;
                main.loop = true;
                main.startLifetime = Random.Range(1.5f, 3);
                var emission = _mParticleSystem.emission;
                emission.rateOverDistance = Random.Range(5, 51);
                //m_ParticleSystem.transform.position = new Vector3(UnityEngine.Random.Range(-6.85f, 6.85f), UnityEngine.Random.Range(-5.25f, 5.25f));
                var time = Random.Range(0.5f, 1f);

                _mParticleSystem.transform.DOMoveX(Random.Range(-6.85f, 6.85f), time).SetEase((Ease)Random.Range(1, 35));
                _mParticleSystem.transform.DOMoveY(Random.Range(-5.25f, 5.25f), time).SetEase((Ease)Random.Range(1, 35)).OnKill(Follish);
            }
        }

        private void Update()
        {
            if (!_typeWritter.isTyping && GameUtilityService.ConvertKeyDownToControl(KeyCode.Z) && canChangeScene)
            {
                _textOptions.text = "";
                GameUtilityService.FadeOutAndSwitchScene("Example-Corridor", Color.black, true, 2);
                canChangeScene = false;
            }

            if (GameUtilityService.ConvertKeyDownToControl(KeyCode.C) && canChangeSceneForC)
            {
                GameUtilityService.FadeOutAndSwitchScene("Example-Corridor", Color.black, true);
                _typeWritter.TypeStop();
                canChangeSceneForC = false;
            }
        }
    }
}