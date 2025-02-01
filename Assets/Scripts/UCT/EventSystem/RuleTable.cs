using System;
using System.Collections.Generic;
using UnityEngine;

namespace UCT.EventSystem
{
    [CreateAssetMenu(fileName = "RuleTable", menuName = "UCT/RuleTable")]
    public class RuleTable : ScriptableObject
    {
        public List<RuleEntry> rules;
    }
}