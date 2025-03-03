using System;
using System.Collections.Generic;

namespace UCT.Battle
{
    public interface IEnemy
    {
        Action[] GetOptions();

        IEnumerator<float> EnemyTurns(int index);
    }
}