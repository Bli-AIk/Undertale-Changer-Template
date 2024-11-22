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

            obj = new GameObject();
            var audioPlayer = obj.AddComponent<AudioPlayer>();
            audioPlayer.audioSource = obj.AddComponent<AudioSource>();

            obj.gameObject.name = "FX Source";
            obj.SetActive(false);
            FillPool();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            audioSource.outputAudioMixerGroup =
                MainControl.Instance.AudioControl.globalAudioMixer.FindMatchingGroups("BGM")[0];
        }

        public void GetFx(int fxNumber, List<AudioClip> list, float volume = 0.5f, float pitch = 1,
            AudioMixerGroup audioMixerGroup = default)
        {
            if (fxNumber < 0)
                return;
            var fx = GetFromPool();
            fx.GetComponent<AudioSource>().volume = volume;
            fx.GetComponent<AudioSource>().pitch = pitch;
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

            fx.GetComponent<AudioSource>().outputAudioMixerGroup = audioMixerGroup;
            //AudioPlayer是字类！！不是unity自带的
            fx.GetComponent<AudioPlayer>().Playing(list[fxNumber]);
        }

        public IEnumerator LayerGetFx(float time, int fxNumber, List<AudioClip> list, float volume = 0.5f,
            float pitch = 1, AudioMixerGroup audioMixerGroup = null)
        {
            yield return new WaitForSeconds(time);
            GetFx(fxNumber, list, volume, pitch, audioMixerGroup);
        }
    }
}