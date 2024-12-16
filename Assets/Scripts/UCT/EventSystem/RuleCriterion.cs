using System;

namespace UCT.EventSystem
{
    [Serializable]
    public struct RuleCriterion
    {
        public FactEntry fact;
        public CriteriaCompare compare;
        public int detection;
    }
}