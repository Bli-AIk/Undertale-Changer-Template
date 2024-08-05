using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioControl", menuName = "UCT-Disposable/AudioControl")]
public class AudioControl : ScriptableObject
{
    public AudioMixer globalAudioMixer;
    
    //public List<AudioClip> bgmClip;
    [Header("用于UI音效")]
    public List<AudioClip> fxClipUI;

    [Header("用于打字机音效")]
    public List<AudioClip> fxClipType;

    [Header("用于战斗音效")]
    public List<AudioClip> fxClipBattle;

    [Header("用于走路脚步音效")]
    public List<AudioClip> fxClipWalk;
}