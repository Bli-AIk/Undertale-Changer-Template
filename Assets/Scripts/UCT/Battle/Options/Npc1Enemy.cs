using System;
using System.Collections.Generic;
using UnityEngine;

namespace UCT.Battle.Options
{
    public class Npc1Enemy : MonoBehaviour, IEnemy
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

        public IEnumerator<float> EnemyTurns(int index)
        {
            throw new NotImplementedException();
        }
    }
}