using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AudioControl", menuName = "AudioControl")]
public class AudioControl : ScriptableObject
{
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
