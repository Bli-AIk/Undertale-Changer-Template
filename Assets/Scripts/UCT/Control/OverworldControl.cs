using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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

        [Header("--UI--")] [Header("字体存储")]
        public List<TMP_FontAsset> tmpFonts;

        [Header("文本包读取")] 
        public string sceneTextsAsset;

        public List<string> sceneTextsSave;

        public string settingAsset;
        public List<string> settingSave;
        public bool isSetting;

        [Header("玩家相关")] public Vector3 playerLastPos;

        [Header("场景衔接存储")] public Vector3 playerScenePos;

        public Vector2 animDirection;

        public List<Sprite> frames;
    }
}