using System.Collections.Generic;
using System.Linq;
using UCT.Audio;
using UnityEngine;

namespace UCT.Scene
{
    public class AudioSpectrumVisualizer : MonoBehaviour
    {
        public GameObject prefab;
        public int objectCount = 128;

        public float scaleMultiplier = 10f;

        public float decayFactor = 0.95f;

        private int _leftEnd, _rightStart;
        private float[] _spectrumData;

        private List<SpriteRenderer> _spriteRenderers;

        private float[] _waveData;

        private void Start()
        {
            var objects = new GameObject[objectCount];
            _spectrumData = new float[objectCount];
            _waveData = new float[objectCount];

            if (objectCount % 2 == 0)
            {
                _leftEnd = objectCount / 2 - 1;
            }
            else
            {
                _leftEnd = objectCount / 2;
            }

            _rightStart = objectCount / 2;

            const float startX = -8f;
            var step = 16f / (objectCount - 1);
            for (var i = 0; i < objectCount; i++)
            {
                var position = new Vector3(startX + i * step, 0, 0);
                objects[i] = Instantiate(prefab, position, Quaternion.identity);
                objects[i].transform.SetParent(transform);
                _waveData[i] = 1f;
            }

            _spriteRenderers = new List<SpriteRenderer>();
            foreach (var obj in objects)
            {
                _spriteRenderers.Add(obj.GetComponent<SpriteRenderer>());
            }
        }

        private void Update()
        {
            var audioSource = AudioController.Instance.audioSource;
            if (!audioSource.isPlaying)
            {
                for (var i = 0; i < objectCount; i++)
                {
                    _waveData[i] = Mathf.Lerp(_waveData[i], 1f, Time.deltaTime * 5f);
                }
            }
            else
            {
                audioSource.GetSpectrumData(_spectrumData, 0, FFTWindow.BlackmanHarris);

                var sum = _spectrumData.Sum();
                var average = sum / _spectrumData.Length;
                var newHeight = average * scaleMultiplier * 10000f + 1f;

                for (var i = _leftEnd; i > 0; i--)
                {
                    var t = (float)i / _leftEnd;
                    var factor = Mathf.Lerp(1f, decayFactor, t);
                    _waveData[i] = Mathf.Max(_waveData[i - 1] * factor, 1f);
                }

                _waveData[0] = newHeight;

                for (var i = _rightStart; i < objectCount - 1; i++)
                {
                    var t = (float)(objectCount - 1 - i) / (objectCount - 1 - _rightStart);
                    var factor = Mathf.Lerp(1f, decayFactor, t);
                    _waveData[i] = Mathf.Max(_waveData[i + 1] * factor, 1f);
                }

                _waveData[objectCount - 1] = newHeight;
            }


            for (var i = 0; i < objectCount; i++)
            {
                var currentScale = _spriteRenderers[i].transform.localScale;
                currentScale.y = Mathf.Lerp(currentScale.y, _waveData[i], Time.deltaTime * 20f);
                _spriteRenderers[i].transform.localScale = currentScale;
                var t = 1 - scaleMultiplier / currentScale.y;
                var alpha = Mathf.SmoothStep(0.5f, 1f, t);
                _spriteRenderers[i].color = new Color(1, 1, 1, alpha);
            }
        }
    }
}