using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Mainly used for Overworld data and general basic data
/// </summary>
[CreateAssetMenu(fileName = "OverworldControl", menuName = "OverworldControl")]
public class OverworldControl : ScriptableObject
{
    //public int languagePack;
    public bool pause;//Prevent player actions from causing errors during black screen switching

    [Header("--UI--")]
    [Header("Font storage")]
    public List<TMPro.TMP_FontAsset> tmpFonts;

    [Header("Font full width, half width")]
    public bool textWidth;//Font full width, half width
    [Header("Resolution level")]
    public int resolutionLevel;//Resolution level
    [Header("FullScreen")]
    public bool fullScreen;//FullScreen



    [Header("Global Volume")]
    public float mainVolume;//Global Volume
    [Header("Simplify special effects")]
    public bool noSFX;//Light effect and post-processing special effect display switch
    [Header("OpenFPS")]
    public bool openFPS;//OpenFPS
    [Header("Resolution")]
    public Vector2 resolution;//Resolution

    [Header("Text packet reading")]
    public string sceneTextsAsset;
    public List<string> sceneTextsSave;

    public string settingAsset;
    public List<string> settingSave;
    public bool isSetting;
    public List<KeyCode> keyCodes, keyCodesBack1, keyCodesBack2;//According to the setting order

    [Header("Player related")]
    public Vector3 playerDeadPos;

    [Header("Scene switching storage")]
    public Vector3 playerScenePos;
    public Vector2 animDirection;

    [Header("HD Border")]
    public bool hdResolution;
    public List<Sprite> frames;


    //[Header("游戏内需要存档的数据在下面写")]
    [Header("Vertical Sync")]
    public VSyncMode vsyncMode;
    public enum VSyncMode
    {
        DonNotSync, // out of sync
        SyncToRefreshRate, // Synchronize to screen refresh rate
        HalfSync // Synchronize to half the screen refresh rate
    }
}
