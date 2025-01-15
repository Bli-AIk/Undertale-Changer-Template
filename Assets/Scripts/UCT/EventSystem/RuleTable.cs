using System;
using System.Collections.Generic;
using UnityEngine;

namespace UCT.EventSystem
{
    [CreateAssetMenu(fileName = "RuleTable", menuName = "UCT/RuleTable")]
    public class RuleTable : ScriptableObject
    {
        public List<RuleEntry> rules;
        
        public int AddFact(FactEntry fact, int valueToAdd)
        {
            return fact.value + valueToAdd;
        }

        public int SubtractFact(FactEntry fact, int valueToSubtract)
        {
            return fact.value - valueToSubtract;
        }

        public int MultiplyFact(FactEntry fact, int valueToMultiply)
        {
            return fact.value * valueToMultiply;
        }

        public int DivideFact(FactEntry fact, int valueToDivide)
        {
            if (valueToDivide != 0)
                return fact.value / valueToDivide;
            throw new DivideByZeroException("Fact cannot divide by zero.");
        }
    }
}