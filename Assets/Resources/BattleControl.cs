using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ս��ϵͳ����������ʹ����ս��������
/// </summary>
[CreateAssetMenu(fileName = "BattleControl", menuName = "BattleControl")]
public class BattleControl : ScriptableObject
{
    [Header("����OBJ")]
    [Header("��������ʶ��Ϊ��������")]
    public List<GameObject> enemies;//�з���Obj��

    [Header("HP żΪĿǰѪ�� ��Ϊ���Max ��ͬ")]
    public List<int> enemiesHp;
    public List<int> enemiesATK, enemiesDEF;
    [Header("ս����UIText��ȡ")]
    public string uiText;
    public List<string> uiTextSave;

    [Header("�洢ACTѡ���ѡ����ı�")]
    public List<string> actSave;//4��һ��Ӧ ����enemies������
    [Header("�洢MERCYѡ���ѡ����ı�")]
    public List<string> mercySave;
    [Header("���غϴ洢�԰�")]
    public List<string> roundTextSave;
    [Header("�洢���˶Ի��ļ�")]
    public List<string> roundDialogAsset;//ֱ����ս�������ڶ�ȡ
    public List<TextAsset> otherDialogAsset;

    /// <summary>
    /// ��Ļ��ɫ���ݣ�ԭ�����ⵯĻ�������Ӿ���ɫ
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
    /// ���������ɫ
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

    


    [Header("��Ŀ����")]
    public int randomRoundDir;
    
}
