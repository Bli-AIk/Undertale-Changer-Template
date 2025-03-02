using System;
using UnityEngine;

namespace UCT.Battle
{
    public class Npc1Options : MonoBehaviour, IEnemyOptions
    {
        public Action[] GetOptions()
        {
            return new Action[]
            {
                () => Other.Debug.Log("NPC1选项1"),
                () => Other.Debug.Log("NPC1选项2"),
                () => Other.Debug.Log("NPC1选项3"),
                () => Other.Debug.Log("NPC1选项4"),
            };
        }
    }
}