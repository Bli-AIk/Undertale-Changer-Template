using System.Text;
using TMPro;
using UCT.Audio;
using UCT.Service;
using UnityEngine;

namespace UCT.Scene
{
    public class MusicRoomController : MonoBehaviour
    {
        private static readonly int Crop = Shader.PropertyToID("_Crop");
        private TextMeshPro _musicTimeText;
        private SpriteRenderer _progressBar;
        private Animator _progressBarPoint;

        private void Start()
        {
            _progressBar = transform.Find("MusicUI/ProgressBar").GetComponent<SpriteRenderer>();
            _progressBarPoint = transform.Find("MusicUI/ProgressBarPoint").GetComponent<Animator>();
            _musicTimeText = transform.Find("MusicUI/TimeText").GetComponent<TextMeshPro>();
        }

        private void Update()
        {
            SetMusicProgressUI();
        }

        private void SetMusicProgressUI()
        {
            var audioSource = AudioController.Instance.audioSource;

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