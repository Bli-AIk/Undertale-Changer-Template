using System.Collections.Generic;
using UCT.Core;
using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable UnusedType.Global

namespace UCT.Battle.BattleConfigs
{
    /// <summary>
    ///     对战斗进行配置的接口类
    /// </summary>
    public interface IBattleConfig
    {
        GameObject[] enemies { get; }
        GameObject backGroundModel { get; }
        Material skyBox { get; }
        VolumeProfile volumeProfile { get; }
        AudioClip bgmClip { get; }
        float volume { get; }
        float pitch { get; }
        IEnumerator<float> Turn(int turnNumber, ObjectPool bulletPool);
    }
}