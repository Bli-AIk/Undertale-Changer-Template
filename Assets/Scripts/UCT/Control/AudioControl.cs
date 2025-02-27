using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace UCT.Control
{
    [CreateAssetMenu(fileName = "AudioControl", menuName = "UCT-Disposable/AudioControl")]
    public class AudioControl : ScriptableObject
    {
        public AudioMixer globalAudioMixer;

        [Header("用于UI音效")]
        public List<AudioClip> fxClipUI;

        [Header("用于战斗音效")]
        public List<AudioClip> fxClipBattle;

        [Header("用于走路脚步音效")]
        public List<AudioClip> fxClipWalk;
    }
}