using UnityEngine;

namespace UCT.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        public AudioSource audioSource;
        private float _clock;
        private bool _isPlay;

        private void Update()
        {
            if (!_isPlay)
            {
                return;
            }

            _clock += Time.deltaTime;
            if (_clock >= audioSource.clip.length)
            {
                AudioController.Instance.ReturnPool(gameObject, this);
            }
        }

        private void OnEnable()
        {
            _isPlay = false;
            _clock = 0;
        }

        public void Playing(AudioClip clip)
        {
            if (_isPlay)
            {
                return;
            }

            audioSource.clip = clip;
            audioSource.Play();
            _isPlay = true;
        }
    }
}