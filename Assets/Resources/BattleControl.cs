using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 万恶的战斗系统啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊
/// 还有包含天杀的回合编辑器数据啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊
/// </summary>
[CreateAssetMenu(fileName = "BattleControl", menuName = "BattleControl")]
public class BattleControl : ScriptableObject
{
    [Header("弹幕初始化")]
    public TextAsset barrgeSetUpAsset;//弹幕初始化
    public List<string> barrgeSetUpSave;
    public List<TextAsset> roundAsset;//回合
    public List<string> roundSave;
    [Header("物体名会识别为敌人名称")]
    public List<GameObject> enemies;//敌方的Obj。
    [Header("HP 前为现血后接Max")]
    public List<int> enemiesHp;
    public List<int> enemiesATK, enemiesDEF;
    [Header("战斗内UIText读取")]
    public string uiText;
    public List<string> uiTextSave;

    [Header("存储ACT选项和选择后文本")]
    public List<string> actSave;//4个一对应 根据enemies而排序
    [Header("存储MERCY选项和选择后文本")]
    public List<string> mercySave;
    [Header("按回合存储旁白")]
    public List<string> roundTextSave;
    [Header("存储敌人对话文件")]
    public List<string> roundDialogAsset;//直接在战斗场景内读取
    public List<TextAsset> otherDialogAsset;


    public enum BulletColor
    {
        white,
        blue,
        orange,
        green
    }
    public List<Color> bulletColorList;
    public enum PlayerColor
    {
        red,
        orange,
        yellow,
        green,
        cyan,
        blue,
        purple,
        nullColor
    }
    public List<Color> playerColorList, playerMissColorList;

    /*  回  合  编  辑  器  */

    [Header("回合编辑器")]
    public string roundEditorData;//弹幕初始化
    public List<string> roundEditorMax;//总List存储


}
