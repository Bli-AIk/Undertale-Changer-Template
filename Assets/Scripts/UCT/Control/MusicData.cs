using UnityEngine;

namespace UCT.Control
{
    [CreateAssetMenu(fileName = "MusicData", menuName = "UCT/MusicData")]
    public class MusicData : ScriptableObject
    {
        public AudioClip clip;
        public Sprite cover;
        public int bpm;
        public string musicDataName;
        public string authorDataName;
        public string informationDataName;
    }
}