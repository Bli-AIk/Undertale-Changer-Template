using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
/// <summary>
/// Battle System Manager. Only used in Battle scene.
/// </summary>
[CreateAssetMenu(fileName = "BattleControl", menuName = "BattleControl")]
public class BattleControl : ScriptableObject
{
    [Header("Enemy OBJ")]
    [Header("Object names will be recognized as enemy names")]
    public List<GameObject> enemies;//Enemy Obj.

    [Header("HP. Even numbers represent the current HP,")]
    [Header("odd numbers represent the maximum Max,")]
    [Header("the same below.")]
    public List<int> enemiesHp;
    public List<int> enemiesATK, enemiesDEF;
    [Header("Reading UIText during battles")]
    public string uiText;
    public List<string> uiTextSave;

    [Header("Store ACT options and selected text")]
    public List<string> actSave;//Every 4 corresponds to one enemy. Sort by enemies
    [Header("Store MERCY options and selected text")]
    public List<string> mercySave;
    [Header("Store narration by turn")]
    public List<string> turnTextSave;
    [Header("Storing Enemy Conversation Files")]
    public List<string> turnDialogAsset;//Directly read within the Battle scene
    public List<TextAsset> otherDialogAsset;

    /// <summary>
    /// Bullet color data (original special bullet), non visual color
    /// </summary>
    public enum BulletColor
    {
        white,
        blue,
        orange,
        green
    }
    public List<Color> bulletColorList;
    /// <summary>
    /// Player Data Color
    /// </summary>
    public enum PlayerColor
    {
        red,
        orange,
        yellow,
        green,
        cyan,
        blue,
        purple,
    }
    public List<Color> playerColorList, playerMissColorList;

    


    [Header("Project Attachment")]
    public int randomTurnDir;
    
}
