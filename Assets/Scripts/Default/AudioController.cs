using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Based on object pools Control Audio
/// </summary>
public class AudioController : ObjectPool
{
    public static AudioController instance;
    public AudioSource audioSource;

    private void Awake()
    {
        instance = this;

        obj = new GameObject();
        AudioPlayer audioPlayer = obj.AddComponent<AudioPlayer>();
        audioPlayer.audioSource = obj.AddComponent<AudioSource>();

        obj.gameObject.name = "FX Source";
        obj.SetActive(false);
        FillPool();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        audioSource.outputAudioMixerGroup = MainControl.instance.AudioControl.globalAudioMixer.FindMatchingGroups("BGM")[0];
    }
    /*
    public void GetFx(AudioClip list, float volume = 0.5f, float pitch = 1, AudioMixerGroup audioMixerGroup = default)
    {
        GameObject fx = GetFromPool();
        fx.GetComponent<AudioSource>().volume = volume;
        fx.GetComponent<AudioSource>().pitch = pitch;
        fx.GetComponent<AudioSource>().outputAudioMixerGroup = audioMixerGroup;
        //AudioPlayer is a word class! It doesn't come with unity.
        fx.GetComponent<AudioPlayer>().Playing(list);
    }
    */
    public void GetFx(int fxNum, List<AudioClip> list, float volume = 0.5f, float pitch = 1, AudioMixerGroup audioMixerGroup = default)
    {
        if (fxNum < 0)
            return;
        GameObject fx = GetFromPool();
        fx.GetComponent<AudioSource>().volume = volume;
        fx.GetComponent<AudioSource>().pitch = pitch;
        if (audioMixerGroup == default)
        {
            if (list == MainControl.instance.AudioControl.fxClipUI)
            {
                audioMixerGroup = MainControl.instance.AudioControl.globalAudioMixer.FindMatchingGroups("FX/UI")[0];
            }
            else if (list == MainControl.instance.AudioControl.fxClipWalk)
            {
                audioMixerGroup = MainControl.instance.AudioControl.globalAudioMixer.FindMatchingGroups("FX/Walk")[0];

            }
            else if (list == MainControl.instance.AudioControl.fxClipBattle)
            {
                audioMixerGroup = MainControl.instance.AudioControl.globalAudioMixer.FindMatchingGroups("FX/Battle")[0];
            }
            else if (list == MainControl.instance.AudioControl.fxClipType)
            {
                audioMixerGroup = MainControl.instance.AudioControl.globalAudioMixer.FindMatchingGroups("FX/Type")[0];
            }
        }

        fx.GetComponent<AudioSource>().outputAudioMixerGroup = audioMixerGroup;
        //AudioPlayer is a word class! It doesn't come with unity.
        fx.GetComponent<AudioPlayer>().Playing(list[fxNum]);
    }

    public IEnumerator LayerGetFx(float time, int fxNum, List<AudioClip> list, float volume = 0.5f, float pitch = 1, UnityEngine.Audio.AudioMixerGroup audioMixerGroup = null)
    {
        yield return new WaitForSeconds(time);
        GetFx(fxNum, list, volume, pitch, audioMixerGroup);
    }
}
