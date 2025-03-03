using System;
using System.Collections.Generic;
using UnityEngine;

namespace UCT.Battle.Options
{
    public class Npc2Enemy : MonoBehaviour, IEnemy
    {
        public Action[] GetOptions()
        {
            return new Action[]
            {
                () => Other.Debug.Log("NPC2选项1"),
                () => Other.Debug.Log("NPC2选项2"),
                () => Other.Debug.Log("NPC2选项3"),
                //() => Other.Debug.Log("NPC2选项4"),
            };
        }

        public IEnumerator<float> EnemyTurns(int index)
        {
            throw new NotImplementedException();
        }
    }
}