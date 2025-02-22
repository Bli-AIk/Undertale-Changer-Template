using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.EventSystem
{
    [CreateAssetMenu(fileName = "FactTable", menuName = "UCT-EventSystem/FactTable")]
    public class FactTable : ScriptableObject
    {
        public List<FactEntry> facts;
    }
}