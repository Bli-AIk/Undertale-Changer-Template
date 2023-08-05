using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AudioControl", menuName = "AudioControl")]
public class AudioControl : ScriptableObject
{
    //public List<AudioClip> bgmClip;
    [Header("����UI��Ч")]
    public List<AudioClip> fxClipUI;
    [Header("���ڴ��ֻ���Ч")]
    public List<AudioClip> fxClipType;
    [Header("����ս����Ч")]
    public List<AudioClip> fxClipBattle;

    [Header("������·�Ų���Ч")]
    public List<AudioClip> fxClipWalk;

}
