using System;
using System.Collections.Generic;
using UnityEngine;

namespace UCT.Control
{
    [CreateAssetMenu(fileName = "CharacterSpriteManager", menuName = "UCT/CharacterSpriteManager")]
    [Serializable]
    public class CharacterSpriteManager : ScriptableObject
    {
        public List<string> fxKeys;
        public List<AudioClip> fxValues;

        public List<string> spriteKeys;
        public List<SpriteExpressionCollection> spriteValues;
    }
}