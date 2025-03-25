using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UCT.Control
{
    /// <summary>
    ///     主要用于Overworld的数据与通用基本数据
    /// </summary>
    [CreateAssetMenu(fileName = "OverworldControl", menuName = "UCT-Disposable/OverworldControl")]
    public class OverworldControl : ScriptableObject
    {
        public enum DynamicType
        {
            None,
            Shake,
            Fade,
            Up
        }

        public List<Sprite> frames;

        [Header("状态")]
        public bool isSetting;

        [Header("--UI--")]
        [Header("字体存储")] public List<TMP_FontAsset> tmpFonts;
    }
}