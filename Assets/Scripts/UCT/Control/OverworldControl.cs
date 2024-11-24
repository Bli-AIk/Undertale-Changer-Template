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
        public enum DynamicTMP
        {
            None,
            RandomShake,
            RandomShakeSingle,
            RandomShakeAll,
            CrazyShake,
            NapShake,
            NapFloat,
            Wave, //这仨
            Explode, //是
            Bounce //AI做的 
        }

        public enum DynamicType
        {
            None,
            Shake,
            Fade,
            Up
        }

        public enum VSyncMode
        {
            DonNotSync, // 不同步
            SyncToRefreshRate, // 同步到屏幕刷新率
            HalfSync // 同步到屏幕刷新率的一半
        }

        //public int languagePack;
        public bool pause; //黑切屏的时候防止玩家操作导致报错

        [Header("--UI--")] [Header("字体存储")] public List<TMP_FontAsset> tmpFonts;

        [Header("字体全半角")] public bool textWidth; //字体全半角

        [Header("分辨率等级")] public int resolutionLevel; //分辨率等级

        [Header("全屏")] public bool fullScreen; //全屏开关

        [Header("全局音量")] public float mainVolume; //全局音量

        [FormerlySerializedAs("noSfx")] [FormerlySerializedAs("noSFX")] [Header("简化特效")]
        public bool isSimplifySfx; //光效 后处理特效显示

        [FormerlySerializedAs("openFPS")] [Header("显示FPS")] public bool isDisplayFPS; //显示FPS

        [Header("分辨率（显示用）")] public Vector2 resolution; //分辨率

        [Header("文本包读取")] public string sceneTextsAsset;

        public List<string> sceneTextsSave;

        public string settingAsset;
        public List<string> settingSave;
        public bool isSetting;

        [Header("玩家相关")] public Vector3 playerDeadPos;

        [Header("场景衔接存储")] public Vector3 playerScenePos;

        public Vector2 animDirection;

        [FormerlySerializedAs("isUsingHDFrame")] [Header("HD边框")] [FormerlySerializedAs("hdResolution")]
        public bool isUsingHdFrame;

        public List<Sprite> frames;

        //[Header("游戏内需要存档的数据在下面写")]
        [Header("垂直同步")] public VSyncMode vsyncMode;

        public List<KeyCode>[] KeyCodes; //依照设置顺序
    }
}