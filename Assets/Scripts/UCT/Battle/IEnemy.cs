using System;
using System.Collections.Generic;
using UCT.Global.Core;

namespace UCT.Battle
{
    public interface IEnemy
    {
        IEnemyTurnNumber TurnGenerator { get; set; } 
        
        Action[] GetOptions();

        IEnumerator<float> _EnemyTurns(List<ObjectPool> objectPools);
        public EnemyState state { get; set; }
    }
}