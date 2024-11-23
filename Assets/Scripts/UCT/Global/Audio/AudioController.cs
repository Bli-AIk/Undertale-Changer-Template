using System.Collections;
using System.Collections.Generic;
using UCT.Global.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace UCT.Global.Audio
{
    /// <summary>
    ///     基于对象池 控制音频
    /// </summary>
    public class AudioController : ObjectPool
    {
        public static AudioController Instance;
        public AudioSource audioSource;

        private void Awake()
        {
            Instance = this;

            poolObject = new GameObject();
            var audioPlayer = poolObject.AddComponent<AudioPlayer>();
            audioPlayer.audioSource = poolObject.AddComponent<AudioSource>();

            poolObject.gameObject.name = "FX Source";
            poolObject.SetActive(false);
            FillPool<AudioPlayer>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            audioSource.outputAudioMixerGroup =
                MainControl.Instance.AudioControl.globalAudioMixer.FindMatchingGroups("BGM")[0];
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void GetFx(int fxNumber, List<AudioClip> list, float volume = 0.5f, float pitch = 1,
            AudioMixerGroup audioMixerGroup = default)
        {
            if (fxNumber < 0)
                return;
            var fxAudioPlayer = GetFromPool<AudioPlayer>();
            var fxAudioSource = fxAudioPlayer.audioSource;
            fxAudioSource.volume = volume;
            fxAudioSource.pitch = pitch;
            if (audioMixerGroup == default)
            {
                if (list == MainControl.Instance.AudioControl.fxClipUI)
                    audioMixerGroup = MainControl.Instance.AudioControl.globalAudioMixer.FindMatchingGroups("FX/UI")[0];
                else if (list == MainControl.Instance.AudioControl.fxClipWalk)
                    audioMixerGroup =
                        MainControl.Instance.AudioControl.globalAudioMixer.FindMatchingGroups("FX/Walk")[0];
                else if (list == MainControl.Instance.AudioControl.fxClipBattle)
                    audioMixerGroup =
                        MainControl.Instance.AudioControl.globalAudioMixer.FindMatchingGroups("FX/Battle")[0];
                else if (list == MainControl.Instance.AudioControl.fxClipType)
                    audioMixerGroup =
                        MainControl.Instance.AudioControl.globalAudioMixer.FindMatchingGroups("FX/Type")[0];
            }

            fxAudioSource.outputAudioMixerGroup = audioMixerGroup;
            fxAudioPlayer.Playing(list[fxNumber]);
        }

        public IEnumerator LayerGetFx(float time, int fxNumber, List<AudioClip> list, float volume = 0.5f,
            float pitch = 1, AudioMixerGroup audioMixerGroup = null)
        {
            yield return new WaitForSeconds(time);
            GetFx(fxNumber, list, volume, pitch, audioMixerGroup);
        }
    }
}