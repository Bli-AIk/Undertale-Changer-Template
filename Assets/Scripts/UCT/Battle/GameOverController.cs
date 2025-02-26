using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Battle
{
    /// <summary>
    ///     GameOver控制器
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        public List<AudioClip> clips;
        public bool canChangeScene;
        public bool canChangeSceneForC;
        private AudioSource _bgmSource;

        private bool _foolDay;
        private ParticleSystem _mParticleSystem;
        private TextMeshPro _textOptions;
        private TypeWritter _typeWritter;

        private void Start()
        {
            canChangeScene = false;
            canChangeSceneForC = true;
            _typeWritter = GetComponent<TypeWritter>();
            _mParticleSystem = transform.Find("Player/Particle System").GetComponent<ParticleSystem>();
            _textOptions = transform.Find("Text Options").GetComponent<TextMeshPro>();

            _mParticleSystem.transform.localPosition = new Vector3(0, 0, -5);
            _foolDay = DateTime.Now.Month == 4 && DateTime.Now.Day == 1;
            _bgmSource = AudioController.Instance.audioSource;
            _bgmSource.clip = clips[Convert.ToInt32(_foolDay)];
            
            var player = _mParticleSystem.transform.parent.gameObject;
            player.transform.position = MainControl.Instance.playerControl.playerLastPos;
            
            _mParticleSystem.transform.position = MainControl.Instance.playerControl.playerLastPos;
            _mParticleSystem.Pause();
            _mParticleSystem.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_typeWritter.isTyping && InputService.GetKeyDown(KeyCode.Z) && canChangeScene)
            {
                _textOptions.text = "";
                GameUtilityService.FadeOutAndSwitchScene("Example-Corridor", Color.black, null, true, 2);
                canChangeScene = false;
            }

            if (!InputService.GetKeyDown(KeyCode.C) || !canChangeSceneForC)
            {
                return;
            }

            GameUtilityService.FadeOutAndSwitchScene("Example-Corridor", Color.black, null, true);
            _typeWritter.TypeStop();
            canChangeSceneForC = false;
        }

        //接下来交给Animator表演
        public void PlayFX(int i)
        {
            if (i < 0)
            {
                _bgmSource.Play();
            }
            else
            {
                AudioController.Instance.PlayFx(i, MainControl.Instance.AudioControl.fxClipUI);
            }
        }

        public void StartParticleSystem()
        {
            _mParticleSystem.transform.position = MainControl.Instance.playerControl.playerLastPos;
            _mParticleSystem.gameObject.SetActive(true);
            _mParticleSystem.Play();
        }

        public void Type()
        {
            var strings = new List<string>
            {
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.sceneTexts,
                    "GameOver1"),
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.sceneTexts,
                    "GameOver2"),
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.sceneTexts,
                    "GameOver3"),
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.sceneTexts,
                    "GameOver4")
            };
            _typeWritter.StartTypeWritter(strings[Random.Range(0, 4)], _textOptions);
            canChangeScene = true;
        }

        public void Prank()
        {
            if (!_foolDay)
            {
                return;
            }

            var main = _mParticleSystem.main;
            main.loop = true;
            main.startLifetime = Random.Range(1.5f, 3);
            var emission = _mParticleSystem.emission;
            emission.rateOverDistance = Random.Range(5, 51);
            var time = Random.Range(0.5f, 1f);

            _mParticleSystem.transform.DOMoveX(Random.Range(-6.85f, 6.85f), time)
                .SetEase((Ease)Random.Range(1, 35));
            _mParticleSystem.transform.DOMoveY(Random.Range(-5.25f, 5.25f), time).SetEase((Ease)Random.Range(1, 35))
                .OnKill(Prank);
        }
    }
}