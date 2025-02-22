using System.Collections.Generic;
using UnityEngine;

namespace UCT.Control
{
    [CreateAssetMenu(fileName = "SpriteExpressionCollection", menuName = "UCT/SpriteExpressionCollection")]
    [System.Serializable]
    public class SpriteExpressionCollection : ScriptableObject
    {
        public Sprite defaultSprite;
        public List<Sprite> speakingSprites;
        public List<Sprite> blinkingSprites;
    }
}