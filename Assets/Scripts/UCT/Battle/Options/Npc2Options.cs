using System;
using UnityEngine;

namespace UCT.Battle.Options
{
    public class Npc2Options : MonoBehaviour, IEnemyOptions
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
    }
}