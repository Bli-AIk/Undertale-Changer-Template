using System.Collections.Generic;
using UnityEngine;

/// <summary>
//// Mainly used for Overworld data and generalized basic data.
/// </summary>
[CreateAssetMenu(fileName = "OverworldControl", menuName = "OverworldControl")]
public class OverworldControl : ScriptableObject
{
    //public int languagePack;
    public bool pause;
    //Prevent players from reporting errors during black cut-scenes.

    [Header("--UI--")]
    [Header("Font Storage")]
    public List<TMPro.TMP_FontAsset> tmpFonts;

    [Header("Font Full Half")]
    public bool textWidth;
    //Font full half-angle

    [Header("Resolution Level")]
    public int resolutionLevel;
    //Resolution level

    [Header("Full Screen")]
    public bool fullScreen;
    //Full Screen Switch

    [Header("Global Volume")]
    public float mainVolume;
    //Global Volume

    [Header("Simplify Effects")]
    public bool noSFX;
    //Lighting effect Post-processing effect display

    [Header("Show FPS")]
    public bool openFPS;
    //Display FPS

    [Header("Resolution (for display)")]
    public Vector2 resolution;
    //Resolution

    [Header("Packet read")]
    public string sceneTextsAsset;

    public List<string> sceneTextsSave;

    public string settingAsset;
    public List<string> settingSave;
    public bool isSetting;
    public List<KeyCode> keyCodes, keyCodesBack1, keyCodesBack2;
    //In the order of setting

    [Header("Player Related")]
    public Vector3 playerDeadPos;

    [Header("Scene articulation store")]
    public Vector3 playerScenePos;

    public Vector2 animDirection;

    [Header("HD Border")]
    public bool hdResolution;

    public List<Sprite> frames;

    //[Header("In-game data that needs to be archived is written below.")]
    [Header("Vertical Synchronization")]
    public VSyncMode vsyncMode;

    public enum VSyncMode
    {
        DonNotSync,
        // Unsynchronized
        SyncToRefreshRate,
        // Synchronize to screen refresh rate
        HalfSync
        // Synchronize to half the screen refresh rate
    }

    public enum DynamicTMP
    {
        None,
        RandomShake,
        RandomShakeSingle,
        RandomShakeAll,
        CrazyShake,
        NapShake,
        NapFloat,
        Wave,
        //These three
        Explode,
        //Yes
        Bounce,
        // AI did it
    }

    public enum DynamicType
    {
        None,
        Shake,
        Fade,
        FadeUp,
        Garbled
    }
}
