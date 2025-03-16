using System;
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
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace UCT.Scene
{
    public class MusicRoomController : MonoBehaviour
    {
        private const float MusicDataTmpSpacing = 0.75f;
        private static readonly int Crop = Shader.PropertyToID("_Crop");
        private static readonly int IsPause = Animator.StringToHash("isPause");

        [SerializeField] [ReadOnly] private int currentMusicDataIndex = -1;

        [SerializeField] [ReadOnly] private int musicDataIndex;
        [SerializeField] [ReadOnly] private SortOption sortIndex;
        [SerializeField] [ReadOnly] private List<MusicData> musicData;
        private TextMeshPro _information;

        private bool _isSettingMusic = true;
        private bool _isSettingSort;
        private bool _isSwitching;
        private SpriteRenderer _musicCover;
        private List<Tween> _musicDataColorTweenList;
        private List<Tween> _musicDataIndentTmpTweenList;
        private List<TextMeshPro> _musicDataTmpList;
        private List<Tween> _musicDataTmpTweenList;
        private TextMeshPro _musicTimeText;

        private TextMeshPro _musicUI;
        private OrderMode _orderMode;

        private SpriteRenderer _progressBar;
        private Animator _progressBarPoint;
        private TextMeshPro _settingMusicBack;

        private SpriteRenderer _settingMusicGradient;
        private GameObject _settingMusicNames;

        private TextMeshPro _settingMusicSettings;

        private SortMode _sortMode;

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
            _settingMusicBack = transform.Find("MusicNames/MusicBack").GetComponent<TextMeshPro>();
            _settingMusicNames = transform.Find("MusicNames/MusicNames").gameObject;

            SpawnMusicData();

            var audioSource = AudioController.Instance.audioSource;
            SetMusic(audioSource.isPlaying);
            audioSource.Play();

            _musicDataTmpTweenList = new List<Tween>();
            _musicDataColorTweenList = new List<Tween>();
            _musicDataIndentTmpTweenList = new List<Tween>();

            while (_musicDataTmpTweenList.Count < _musicDataTmpList.Count)
            {
                _musicDataTmpTweenList.Add(null);
            }

            while (_musicDataColorTweenList.Count < _musicDataTmpList.Count)
            {
                _musicDataColorTweenList.Add(null);
            }

            while (_musicDataIndentTmpTweenList.Count < _musicDataTmpList.Count)
            {
                _musicDataIndentTmpTweenList.Add(null);
            }

            SortList();
        }

        private void Update()
        {
            var audioSource = AudioController.Instance.audioSource;

            SetMusicProgressUI(audioSource);
            ScrollText(_information);
            if (_isSwitching || GameUtilityService.IsGamePausedOrSetting())
            {
                return;
            }

            if (!_isSettingMusic)
            {
                MusicPlayingInput(audioSource);
            }
            else
            {
                var shouldUpdateSortText = false;
                if (InputService.GetKeyDown(KeyCode.LeftArrow) || InputService.GetKeyDown(KeyCode.RightArrow))
                {
                    _isSettingSort = !_isSettingSort;
                    shouldUpdateSortText = true;
                }

                if (!_isSettingSort)
                {
                    SettingMusicInput(audioSource);
                }
                else
                {
                    shouldUpdateSortText = SettingSortInput(shouldUpdateSortText);
                }

                if (shouldUpdateSortText)
                {
                    UpdateSortText();
                }
            }
        }

        private bool SettingSortInput(bool shouldUpdateSortText)
        {
            var optionCount = Enum.GetValues(typeof(SortOption)).Length;

            if (InputService.GetKeyDown(KeyCode.DownArrow))
            {
                sortIndex = (SortOption)(((int)sortIndex + 1 + optionCount) % optionCount);
                shouldUpdateSortText = true;
            }

            if (InputService.GetKeyDown(KeyCode.UpArrow))
            {
                sortIndex = (SortOption)(((int)sortIndex - 1 + optionCount) % optionCount);
                shouldUpdateSortText = true;
            }

            if (!InputService.GetKeyDown(KeyCode.Z))
            {
                return shouldUpdateSortText;
            }

            switch (sortIndex)
            {
                case SortOption.Sort:
                {
                    _sortMode++;
                    if ((int)_sortMode >= Enum.GetValues(typeof(SortMode)).Length)
                    {
                        _sortMode = 0;
                    }

                    break;
                }
                case SortOption.Order:
                {
                    _orderMode++;
                    if ((int)_orderMode >= Enum.GetValues(typeof(OrderMode)).Length)
                    {
                        _orderMode = 0;
                    }

                    break;
                }
                case SortOption.Back:
                {
                    return shouldUpdateSortText;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SortList();

            return shouldUpdateSortText;
        }

        private void SortList()
        {
            Func<MusicData, string> stringSelector = _sortMode switch
            {
                SortMode.Title => data => TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, data.musicDataName),
                SortMode.Author => data => TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, data.authorDataName),
                _ => null
            };

            Func<MusicData, float> floatSelector = _sortMode switch
            {
                SortMode.Length => data => data.clip ? data.clip.length : 0f,
                _ => null
            };

            if (stringSelector == null && floatSelector == null)
            {
                throw new ArgumentOutOfRangeException();
            }

            var indexedItems = musicData
                .Select((data, index) => new
                {
                    Index = index,
                    Value = stringSelector != null ? stringSelector(data) : floatSelector(data).ToString()
                })
                .OrderBy(item => item.Value, StringComparer.OrdinalIgnoreCase)
                .Select(item => item.Index)
                .ToList();

            if (_orderMode == OrderMode.Descending)
            {
                indexedItems.Reverse();
            }

            ApplySortedIndices(indexedItems);
        }

        private void ApplySortedIndices(List<int> sortedIndices)
        {
            musicDataIndex = sortedIndices.IndexOf(musicDataIndex);
            if (currentMusicDataIndex >= 0)
            {
                currentMusicDataIndex = sortedIndices.IndexOf(currentMusicDataIndex);
            }

            musicData = sortedIndices.Select(i => musicData[i]).ToList();
            _musicDataColorTweenList = sortedIndices.Select(i => _musicDataColorTweenList[i]).ToList();
            _musicDataIndentTmpTweenList = sortedIndices.Select(i => _musicDataIndentTmpTweenList[i]).ToList();
            _musicDataTmpList = sortedIndices.Select(i => _musicDataTmpList[i]).ToList();
            _musicDataTmpTweenList = sortedIndices.Select(i => _musicDataTmpTweenList[i]).ToList();

            UpdateMusicDataTween();
            UpdateSortText();
        }


        private void UpdateSortText()
        {
            _settingMusicSettings.text = new StringBuilder()
                .Append(sortIndex == SortOption.Sort && _isSettingSort ? "<color=yellow>" : "")
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "SortBy"))
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, _sortMode.ToString()))
                .Append("</color>\n")
                .Append(sortIndex == SortOption.Order && _isSettingSort ? "<color=yellow>" : "")
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "Order"))
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, _orderMode.ToString()))
                .Append("</color>")
                .ToString();
            _settingMusicBack.text = new StringBuilder()
                .Append(sortIndex == SortOption.Back && _isSettingSort ? "<color=yellow>" : "")
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, "Back"))
                .Append("</color>")
                .ToString();
        }

        private void SettingMusicInput(AudioSource audioSource)
        {
            var prevIndex = musicDataIndex;

            if (InputService.GetKeyDown(KeyCode.UpArrow))
            {
                musicDataIndex = (musicDataIndex - 1 + musicData.Count) % musicData.Count;
            }

            if (InputService.GetKeyDown(KeyCode.DownArrow))
            {
                musicDataIndex = (musicDataIndex + 1) % musicData.Count;
            }

            if (prevIndex != musicDataIndex)
            {
                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                UpdateMusicDataTween();
            }

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);

                if (musicDataIndex == currentMusicDataIndex)
                {
                    currentMusicDataIndex = -1;
                    audioSource.clip = null;
                }
                else
                {
                    currentMusicDataIndex = musicDataIndex;
                }

                UpdateMusicDataTween();
                SetMusic(true);

                if (musicDataIndex == currentMusicDataIndex)
                {
                    audioSource.Play();
                }
                else
                {
                    audioSource.Pause();
                }

                audioSource.time = 0;
            }


            if (InputService.GetKeyDown(KeyCode.C) || InputService.GetKeyDown(KeyCode.X))
            {
                OutSettingMusic();
            }
        }

        private void UpdateMusicDataTween()
        {
            for (var i = 0; i < _musicDataTmpList.Count; i++)
            {
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

                var factor = Mathf.Pow(Mathf.Abs(distance / 5f), 0.5f); // 0.5f 控制曲线形状
                var newA = Mathf.Clamp01(1 - factor);

                var currentBlueValue = i == currentMusicDataIndex ? 0 : 1;
                if (!Mathf.Approximately(_musicDataTmpList[i].color.a, newA) ||
                    !Mathf.Approximately(_musicDataTmpList[i].color.b, currentBlueValue))
                {
                    if (_musicDataColorTweenList[i] != null)
                    {
                        _musicDataColorTweenList[i].Kill();
                    }

                    _musicDataColorTweenList[i] = _musicDataTmpList[i]
                        .DOColor(new Color(1, 1, currentBlueValue, newA), 0.25f)
                        .SetEase(Ease.InOutSine);
                }

                var currentXValue = i == currentMusicDataIndex ? 0.25f : 0;


                if (!Mathf.Approximately(_musicDataTmpList[i].transform.position.x, currentXValue))
                {
                    if (_musicDataIndentTmpTweenList[i] != null)
                    {
                        _musicDataIndentTmpTweenList[i].Kill();
                    }

                    _musicDataIndentTmpTweenList[i] = _musicDataTmpList[i].transform.DOMoveX(currentXValue, 0.25f)
                        .SetEase(Ease.InOutSine);
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
                musicNameTmp.extraPadding = true;
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
            _settingMusicBack.transform.DOMoveX(0, 0.5f)
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
            _settingMusicBack.transform.DOMoveX(20, 0.5f)
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

        private void SetMusic(bool showPlayingText)
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

            _musicUI.text = new StringBuilder().Append("<size=9>")
                .Append(TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.LanguagePackControl.sceneTexts, showPlayingText ? "NowPlaying" : "Paused"))
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
                _musicTimeText.text = new StringBuilder()
                    .Append(TextProcessingService.FormatTimeToMinutesSeconds(0))
                    .Append(" / ")
                    .Append(TextProcessingService.FormatTimeToMinutesSeconds(0))
                    .ToString();
                return;
            }

            _progressBarPoint.SetBool(IsPause, !audioSource.isPlaying);

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

        private static void ScrollText(TextMeshPro text, int maxVisibleLines = 3, float scrollSpeed = 0.5f)
        {
            if (!text)
            {
                return;
            }

            text.ForceMeshUpdate();
            if (text.textInfo.lineCount <= maxVisibleLines)
            {
                ResetTextPosition(text);
                return;
            }

            UpdateTextPosition(text, maxVisibleLines, scrollSpeed);
            ApplyLineTransparency(text);
        }

        private static void ResetTextPosition(TextMeshPro text)
        {
            text.transform.position = new Vector3(text.transform.position.x, -6.5f, text.transform.position.z);
        }

        private static void UpdateTextPosition(TextMeshPro text, int maxVisibleLines, float scrollSpeed)
        {
            var lineHeight = 0.5f * (text.textInfo.lineCount + maxVisibleLines * 1.5f);
            var currentOffset = -Mathf.Repeat(Time.time * scrollSpeed, lineHeight) + 6.5f + maxVisibleLines / 2f;
            text.transform.position = new Vector3(text.transform.position.x, -currentOffset, text.transform.position.z);
        }

        private static void ApplyLineTransparency(TextMeshPro text)
        {
            for (var lineIndex = 0; lineIndex < text.textInfo.lineCount; lineIndex++)
            {
                var lineInfo = text.textInfo.lineInfo[lineIndex];
                var firstVisibleCharIndex =
                    GetFirstVisibleCharIndex(text, lineInfo.firstCharacterIndex, lineInfo.lastCharacterIndex);
                if (firstVisibleCharIndex == -1)
                {
                    continue;
                }

                var firstCharInfo = text.textInfo.characterInfo[firstVisibleCharIndex];
                var worldY = GetCharacterWorldY(text, firstCharInfo);
                var alpha = CalculateAlpha(worldY);
                var newColor = new Color32(255, 255, 255, (byte)(alpha * 255));
                for (var i = lineInfo.firstCharacterIndex; i <= lineInfo.lastCharacterIndex; i++)
                {
                    if (!text.textInfo.characterInfo[i].isVisible)
                    {
                        continue;
                    }

                    var charInfo = text.textInfo.characterInfo[i];
                    var matIndex = charInfo.materialReferenceIndex;
                    var vertexIndex = charInfo.vertexIndex;
                    var colors = text.textInfo.meshInfo[matIndex].colors32;
                    colors[vertexIndex] = newColor;
                    colors[vertexIndex + 1] = newColor;
                    colors[vertexIndex + 2] = newColor;
                    colors[vertexIndex + 3] = newColor;
                }
            }

            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        private static int GetFirstVisibleCharIndex(TextMeshPro text, int firstCharIndex, int lastCharIndex)
        {
            for (var i = firstCharIndex; i <= lastCharIndex; i++)
            {
                if (text.textInfo.characterInfo[i].isVisible)
                {
                    return i;
                }
            }

            return -1;
        }

        private static float GetCharacterWorldY(TextMeshPro text, TMP_CharacterInfo charInfo)
        {
            var matIndex = charInfo.materialReferenceIndex;
            var vertexIndex = charInfo.vertexIndex;
            var vertices = text.textInfo.meshInfo[matIndex].vertices;
            if (vertexIndex + 3 >= vertices.Length)
            {
                return text.transform.TransformPoint(Vector3.zero).y;
            }

            var avgY = (vertices[vertexIndex].y + vertices[vertexIndex + 1].y + vertices[vertexIndex + 2].y +
                        vertices[vertexIndex + 3].y) / 4f;
            return text.transform.TransformPoint(new Vector3(0, avgY, 0)).y;
        }

        private static float CalculateAlpha(float worldY)
        {
            const float fadeRange = 0.1f;
            return worldY switch
            {
                < -4 - fadeRange => 0f,
                < -4 => Mathf.SmoothStep(0f, 1f, (worldY + 4 + fadeRange) / fadeRange),
                > -2.5f + fadeRange => 0f,
                > -2.5f => Mathf.SmoothStep(1f, 0f, (worldY + 2.5f) / fadeRange),
                _ => 1f
            };
        }


        private enum SortMode
        {
            Title,
            Author,
            Length
        }

        private enum OrderMode
        {
            Ascending,
            Descending
        }

        private enum SortOption
        {
            Sort,
            Order,
            Back
        }
    }
}