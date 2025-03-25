using System;
using System.Collections.Generic;
using UnityEngine;

namespace UCT.Control
{
    [CreateAssetMenu(fileName = "SpriteExpressionCollection", menuName = "UCT/SpriteExpressionCollection")]
    [Serializable]
    public class SpriteExpressionCollection : ScriptableObject
    {
        public enum State
        {
            Default,
            Speaking,
            Blinking
        }

        public Sprite defaultSprite;
        public List<Sprite> speakingSprites;
        public List<Sprite> blinkingSprites;
    }
}