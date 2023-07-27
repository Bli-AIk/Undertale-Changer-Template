using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
/// <summary>
/// 基于对象池 控制音频
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
    public void GetFx(AudioClip list, float volume = 0.5f, float pitch = 1, AudioMixerGroup audioMixerGroup = null)
    {
        GameObject fx = GetFromPool();
        fx.GetComponent<AudioSource>().volume = volume;
        fx.GetComponent<AudioSource>().pitch = pitch;
        fx.GetComponent<AudioSource>().outputAudioMixerGroup = audioMixerGroup;
        //AudioPlayer是字类！！不是unity自带的
        fx.GetComponent<AudioPlayer>().Playing(list);
    }

    public void GetFx(int fxNum, List<AudioClip> list, float volume = 0.5f, float pitch = 1, AudioMixerGroup audioMixerGroup = null)
    {
        if (fxNum < 0)
            return;
        GameObject fx = GetFromPool();
        fx.GetComponent<AudioSource>().volume = volume;
        fx.GetComponent<AudioSource>().pitch = pitch;
        fx.GetComponent<AudioSource>().outputAudioMixerGroup = audioMixerGroup;
        //AudioPlayer是字类！！不是unity自带的
        fx.GetComponent<AudioPlayer>().Playing(list[fxNum]);
    }

    public IEnumerator LayerGetFx(float time, int fxNum, List<AudioClip> list, float volume = 0.5f, float pitch = 1, UnityEngine.Audio.AudioMixerGroup audioMixerGroup = null)
    {
        yield return new WaitForSeconds(time);
        GetFx(fxNum, list, volume, pitch, audioMixerGroup);
    }
}
