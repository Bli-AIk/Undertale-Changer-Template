using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AudioControl", menuName = "AudioControl")]
public class AudioControl : ScriptableObject
{
    //public List<AudioClip> bgmClip;
    [Header("For UI sound effects")]
    public List<AudioClip> fxClipUI;
    [Header("For typewriter sound effects")]
    public List<AudioClip> fxClipType;
    [Header("For Battle sound effects")]
    public List<AudioClip> fxClipBattle;

    [Header("For walking sound effects")]
    public List<AudioClip> fxClipWalk;

}
