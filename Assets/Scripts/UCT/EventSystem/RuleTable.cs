using System.Collections.Generic;
using UnityEngine;

namespace UCT.EventSystem
{
    [CreateAssetMenu(fileName = "RuleTable", menuName = "UCT-EventSystem/RuleTable")]
    public class RuleTable : ScriptableObject
    {
        public List<RuleEntry> rules;
    }
}