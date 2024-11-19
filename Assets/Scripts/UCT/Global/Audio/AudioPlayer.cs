using UnityEngine;

namespace UCT.Global.Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        private bool _isPlay;
        public AudioSource audioSource;
        private float _clock;

        private void OnEnable()
        {
            _isPlay = false;
            _clock = 0;
        }

        private void Update()
        {
            if (_isPlay)
            {
                _clock += Time.deltaTime;
                if (_clock >= audioSource.clip.length)
                {
                    AudioController.Instance.ReturnPool(gameObject);
                }
            }
        }

        public void Playing(AudioClip clip)
        {
            if (!_isPlay)
            {
                audioSource.clip = clip;
                audioSource.Play();
                _isPlay = true;
            }
        }
    }
}