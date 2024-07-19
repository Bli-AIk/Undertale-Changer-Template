using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Combat System Manager, used only for combat scenarios.
/// </summary>
[CreateAssetMenu(fileName = "BattleControl", menuName = "BattleControl")]
public class BattleControl : ScriptableObject
{
    [Header("Enemy OBJ")]
    [Header("Object name will be recognized as enemy name")]
    public List<GameObject> enemies;
    // Enemy Obj.

    [Header("HP Even is Current Blood Odd is Max. Same below")]
    public List<int> enemiesHp;

    public List<int> enemiesATK, enemiesDEF;

    [Header("In-combat UIText read")]
    public string uiText;

    public List<string> uiTextSave;

    [Header("Store ACT options and post selection text")]
    public List<string> actSave;
    //4 one to one, sorted by enemies.

    [Header("Store MERCY options and post-selection text")]
    public List<string> mercySave;

    [Header("Store narration by round")]
    public List<string> turnTextSave;

    [Header("Store enemy dialog file")]
    public List<string> turnDialogAsset;
    //Read directly in the battle scene

    public List<TextAsset> otherDialogAsset;

    /// <summary>
    /// Pop-up color data (original special pop-ups), non-visual colors
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
    /// Player data color
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
