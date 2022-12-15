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

    //UI
    public bool textWidth;//字体全半角
    public int resolutionLevel;//分辨率档
    public bool fullScreen;//全屏开关
    public bool backGround;//是否启用侧边框
    public float mainVolume;//全局音量
    public bool noSFX;//光效 后处理特效显示
    public bool openFPS;//显示FPS
    public Vector2 resolution;//分辨率
    public string owTextsAsset;
    public List<string> owTextsSave;
    public string menuAndSettingAsset;
    public List<string> menuAndSettingSave;
    public string safeText;
    public bool isSetting;
    public List<KeyCode> keyCodes, keyCodesBack1, keyCodesBack2;//依照设置顺序




    public bool isDebug;
}
