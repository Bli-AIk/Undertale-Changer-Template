using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alchemy.Inspector;
using DG.Tweening;
using TMPro;
using UCT.Audio;
using UCT.Control;
using UCT.Core;
using UCT.Service;
using UnityEngine;

namespace UCT.Scene
{
    public class MusicRoomController : MonoBehaviour
    {
        private const float MusicDataTmpSpacing = 0.75f;
        private static readonly int Crop = Shader.PropertyToID("_Crop");
        private static readonly int IsPause = Animator.StringToHash("isPause");

        [SerializeField] [ReadOnly] private int currentMusicDataIndex = -1;

        [SerializeField] [ReadOnly] private int musicDataIndex;

        [SerializeField] [ReadOnly] private List<MusicData> musicData;

        private TextMeshPro _information;

        private bool _isSettingMusic = true;

        private bool _isSwitching;
        private SpriteRenderer _musicCover;
        private List<Tween> _musicDataColorTweenList;
        private List<TextMeshPro> _musicDataTmpList;
        private List<Tween> _musicDataTmpTweenList;
        private TextMeshPro _musicTimeText;

        private TextMeshPro _musicUI;

        private SpriteRenderer _progressBar;
        private Animator _progressBarPoint;

        private SpriteRenderer _settingMusicGradient;
        private GameObject _settingMusicNames;

        private TextMeshPro _settingMusicSettings;


        private void Start()
        {
            _musicUI = transform.Find("MusicUI").GetComponent<TextMeshPro>();
            _information = transform.Find("MusicUI/Information").GetComponent<TextMeshPro>();
            _musicCover = transform.Find("MusicUI/Cover").GetComponent<SpriteRenderer>();

            _progressBar = transform.Find("MusicUI/ProgressBar").GetComponent<SpriteRenderer>();
            _progressBarPoint = transform.Find("MusicUI/ProgressBarPoint").GetComponent<Animator>();
            _musicTimeText = transform.Find("MusicUI/TimeText").GetComponent<TextMeshPro>();

            _progressBarPoint.SetBool(IsPause, true);
            _progressBar.material.SetFloat(Crop, 0);

            _settingMusicGradient = transform.Find("MusicNames/Background/Gradient").GetComponent<SpriteRenderer>();
            _settingMusicSettings = transform.Find("MusicNames/MusicSettings").GetComponent<TextMeshPro>();
            _settingMusicNames = transform.Find("MusicNames/MusicNames").gameObject;

            _settingMusicSettings.text = new StringBuilder()
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "SortBy"))
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "Title"))
                .Append("\n")
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "Order"))
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "Ascending"))
                .Append("\n\n")
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "Back"))
                .ToString();
            SpawnMusicData();

            var audioSource = AudioController.Instance.audioSource;
            SetMusic(audioSource.isPlaying);

            _musicDataTmpTweenList = new List<Tween>();
            _musicDataColorTweenList = new List<Tween>();

            MoveMusicDataTmp();
        }

        private void Update()
        {
            var audioSource = AudioController.Instance.audioSource;

            if (_isSwitching)
            {
                return;
            }

            if (!_isSettingMusic)
            {
                MusicPlayingInput(audioSource);
                SetMusicProgressUI(audioSource);
            }
            else
            {
                var prevIndex = musicDataIndex;

                if (InputService.GetKeyDown(KeyCode.UpArrow))
                {
                    musicDataIndex = Mathf.Clamp(musicDataIndex - 1, 0, musicData.Count - 1);
                }

                if (InputService.GetKeyDown(KeyCode.DownArrow))
                {
                    musicDataIndex = Mathf.Clamp(musicDataIndex + 1, 0, musicData.Count - 1);
                }

                if (prevIndex != musicDataIndex)
                {
                    AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    MoveMusicDataTmp();
                }

                if (InputService.GetKeyDown(KeyCode.Z))
                {
                    AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);

                    if (musicDataIndex == currentMusicDataIndex)
                    {
                        currentMusicDataIndex = -1;
                    }
                    else
                    {
                        currentMusicDataIndex = musicDataIndex;
                    }

                    SetMusic(true);
                }


                if (InputService.GetKeyDown(KeyCode.C) || InputService.GetKeyDown(KeyCode.X))
                {
                    OutSettingMusic();
                }
            }
        }

        private void MoveMusicDataTmp()
        {
            for (var i = 0; i < _musicDataTmpList.Count; i++)
            {
                while (_musicDataTmpTweenList.Count < _musicDataTmpList.Count)
                {
                    _musicDataTmpTweenList.Add(null);
                }

                while (_musicDataColorTweenList.Count < _musicDataTmpList.Count)
                {
                    _musicDataColorTweenList.Add(null);
                }

                var distance = i - musicDataIndex;
                var newY = -distance * MusicDataTmpSpacing;
                if (!Mathf.Approximately(_musicDataTmpList[i].transform.position.y, newY))
                {
                    if (_musicDataTmpTweenList[i] != null)
                    {
                        _musicDataTmpTweenList[i].Kill();
                    }

                    _musicDataTmpTweenList[i] = _musicDataTmpList[i].transform.DOMoveY(newY, 0.25f)
                        .SetEase(Ease.InOutSine);
                }

                var newV = Mathf.Clamp01(1 - Mathf.Abs(distance / 5f));
                Color.RGBToHSV(_musicDataTmpList[i].color, out _, out _, out var v);
                if (!Mathf.Approximately(v, newV))
                {
                    if (_musicDataColorTweenList[i] != null)
                    {
                        _musicDataColorTweenList[i].Kill();
                    }

                    _musicDataColorTweenList[i] = _musicDataTmpList[i].DOColor(Color.HSVToRGB(0, 0, newV), 0.25f);
                }
            }
        }

        private void SpawnMusicData()
        {
            musicData = Resources.LoadAll<MusicData>("Audios").ToList();
            _musicDataTmpList = new List<TextMeshPro>();
            for (var index = 0; index < musicData.Count; index++)
            {
                var data = musicData[index];
                var musicName = new GameObject();
                musicName.transform.SetParent(_settingMusicNames.transform);
                musicName.name = "MusicName";
                var musicNameTmp = musicName.AddComponent<TextMeshPro>();
                musicNameTmp.font = _settingMusicSettings.font;
                musicNameTmp.fontSize = _settingMusicSettings.fontSize;
                musicNameTmp.alignment = TextAlignmentOptions.MidlineLeft;
                musicNameTmp.sortingLayerID = SortingLayer.NameToID("UI");
                musicNameTmp.sortingOrder = 1;
                musicNameTmp.text = new StringBuilder()
                    .Append(TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.LanguagePackControl.sceneTexts, data.musicDataName))
                    .Append(" - ")
                    .Append(TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.LanguagePackControl.sceneTexts, data.authorDataName))
                    .ToString();
                musicNameTmp.rectTransform.sizeDelta = new Vector2(15, 0);
                _musicDataTmpList.Add(musicNameTmp);

                if (index > 0)
                {
                    musicNameTmp.transform.position =
                        _musicDataTmpList[index - 1].transform.position + Vector3.down * MusicDataTmpSpacing;
                }
            }
        }

        private void InSettingMusic()
        {
            _isSwitching = true;
            _settingMusicGradient.DOColor(Color.white, 0.5f)
                .SetEase(Ease.InOutCubic);
            _settingMusicNames.transform.DOMoveX(0, 0.5f)
                .SetEase(Ease.OutCubic);
            _settingMusicSettings.transform.DOMoveX(0, 0.5f)
                .SetEase(Ease.OutCubic).OnKill(() =>
                {
                    _isSettingMusic = true;
                    _isSwitching = false;
                });
        }

        private void OutSettingMusic()
        {
            _isSwitching = true;
            _settingMusicGradient.DOColor(Color.clear, 0.5f)
                .SetEase(Ease.InOutCubic);
            _settingMusicNames.transform.DOMoveX(-20, 0.5f)
                .SetEase(Ease.InCubic);
            _settingMusicSettings.transform.DOMoveX(20, 0.5f)
                .SetEase(Ease.InCubic).OnKill(() =>
                {
                    _isSettingMusic = false;
                    _isSwitching = false;
                });
        }

        private void MusicPlayingInput(AudioSource audioSource)
        {
            if (InputService.GetKeyDown(KeyCode.C))
            {
                InSettingMusic();
            }

            if (!audioSource.clip)
            {
                return;
            }

            float step = 5;

            if (InputService.GetKey(KeyCode.X))
            {
                step = 1;
            }

            if (InputService.GetKeyDown(KeyCode.LeftArrow))
            {
                audioSource.time = Mathf.Clamp(audioSource.time - step, 0, audioSource.clip.length);
            }

            if (InputService.GetKeyDown(KeyCode.RightArrow))
            {
                audioSource.time = Mathf.Clamp(audioSource.time + step, 0, audioSource.clip.length - 0.1f);
            }

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                _progressBarPoint.SetBool(IsPause, audioSource.isPlaying);
                if (audioSource.isPlaying)
                {
                    audioSource.Pause();
                }
                else
                {
                    audioSource.Play();
                }

                SetMusic(audioSource.isPlaying);
            }
        }

        private void SetMusic(bool isPlaying)
        {
            var audioSource = AudioController.Instance.audioSource;
            if (currentMusicDataIndex < 0 || currentMusicDataIndex >= musicData.Count)
            {
                OffMusic(audioSource);
                return;
            }

            var data = musicData[currentMusicDataIndex];
            if (!data)
            {
                OffMusic(audioSource);
                return;
            }

            audioSource.clip = data.clip;
            audioSource.Play();

            _musicUI.text = new StringBuilder().Append("<size=9>")
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, isPlaying ? "NowPlaying" : "Paused"))
                .Append("</size>\n")
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, data.musicDataName))
                .Append(" - ")
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, data.authorDataName))
                .ToString();
            _information.text = TextProcessingService.GetFirstChildStringByPrefix(
                MainControl.Instance.LanguagePackControl.sceneTexts, data.informationDataName);
            _musicCover.sprite = data.cover;
            _musicCover.size = Vector2.one * 4;
        }

        private void OffMusic(AudioSource audioSource)
        {
            _musicUI.text = new StringBuilder().Append("<size=9>")
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "NothingIsPlaying"))
                .Append("</size>")
                .ToString();
            _information.text = null;
            _musicCover.sprite = null;
            audioSource.Pause();
        }

        private void SetMusicProgressUI(AudioSource audioSource)
        {
            if (!audioSource.clip)
            {
                _progressBarPoint.SetBool(IsPause, true);
                _progressBar.material.SetFloat(Crop, 0);
                return;
            }

            _musicTimeText.text = new StringBuilder()
                .Append(TextProcessingService.FormatTimeToMinutesSeconds((int)audioSource.time))
                .Append(" / ")
                .Append(TextProcessingService.FormatTimeToMinutesSeconds((int)audioSource.clip.length))
                .ToString();
            var normalizedTime = audioSource.time / audioSource.clip.length;
            var mappedPosition = Mathf.Lerp(-4.375f, 4.375f, normalizedTime);
            _progressBar.material.SetFloat(Crop, normalizedTime);
            _progressBarPoint.transform.localPosition = new Vector2(mappedPosition, -4.5f);
        }
    }
}