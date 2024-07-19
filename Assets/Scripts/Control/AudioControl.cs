using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioControl", menuName = "AudioControl")]
public class AudioControl : ScriptableObject
{
    public AudioMixer globalAudioMixer;

    //public List<AudioClip> bgmClip;
    [Header("for UI sound")]
    public List<AudioClip> fxClipUI;

    [Header("For typewriter sound effects")]
    public List<AudioClip> fxClipType;

    [Header("for combat sound effects")]
    public List<AudioClip> fxClipBattle;

    [Header("For walking footsteps sound effect")]
    public List<AudioClip> fxClipWalk;
}
