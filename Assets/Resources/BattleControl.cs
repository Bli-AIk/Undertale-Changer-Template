using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ����ս��ϵͳ������������������������������
/// ���а�����ɱ�Ļغϱ༭�����ݰ�������������������������������
/// </summary>
[CreateAssetMenu(fileName = "BattleControl", menuName = "BattleControl")]
public class BattleControl : ScriptableObject
{
    [Header("��Ļ��ʼ��")]
    public TextAsset barrgeSetUpAsset;//��Ļ��ʼ��
    public List<string> barrgeSetUpSave;
    public List<TextAsset> roundAsset;//�غ�
    public List<string> roundSave;
    [Header("��������ʶ��Ϊ��������")]
    public List<GameObject> enemies;//�з���Obj��
    [Header("HP ǰΪ��Ѫ���Max")]
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

    /*  ��  ��  ��  ��  ��  */

    [Header("�غϱ༭��")]
    public string roundEditorData;//��Ļ��ʼ��
    public List<string> roundEditorMax;//��List�洢


}
