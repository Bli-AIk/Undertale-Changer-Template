using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 主要用于Overworld的数据与通用基本数据
/// </summary>
[CreateAssetMenu(fileName = "OverwroldControl", menuName = "OverwroldControl")]
public class OverwroldControl : ScriptableObject
{
    public int languagePack;
    public bool pause;//黑切屏的时候防止玩家操作导致报错

    [Header("--UI--")]
    [Header("字体存储")]
    public List<TMPro.TMP_FontAsset> tmpFonts;

    [Header("字体全半角")]
    public bool textWidth;//字体全半角
    [Header("分辨率等级")]
    public int resolutionLevel;//分辨率等级
    [Header("全屏")]
    public bool fullScreen;//全屏开关

    //[Header("侧边框（废弃）")]
    //public bool backGround;//是否启用侧边框
    [Header("全局音量")]
    public float mainVolume;//全局音量
    [Header("简化特效")]
    public bool noSFX;//光效 后处理特效显示
    [Header("显示FPS")]
    public bool openFPS;//显示FPS
    [Header("分辨率（显示用）")]
    public Vector2 resolution;//分辨率

    [Header("OW文本包读取")]
    public string owTextsAsset;
    public List<string> owTextsSave;
    public string menuAndSettingAsset;
    public List<string> menuAndSettingSave;
    public string safeText;
    public bool isSetting;
    public List<KeyCode> keyCodes, keyCodesBack1, keyCodesBack2;//依照设置顺序



    [Header("开启调试")]
    public bool isDebug;
    [Header("--调试模式设定--")]
    [Header("锁血")]
    public bool invincible;


    //[Header("游戏内需要存档的数据在下面写")]

}
