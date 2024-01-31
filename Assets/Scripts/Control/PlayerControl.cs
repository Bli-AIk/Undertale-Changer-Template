using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ҵ���Ϣ
/// �Լ�һЩ��ص�����
/// </summary>
[CreateAssetMenu(fileName = "PlayerControl", menuName = "PlayerControl")]
public class PlayerControl : ScriptableObject
{
    public int hp, hpMax, lv, exp, gold, wearAtk, wearDef, nextExp;
    public float missTime, missTimeMax;

    [Header("OW��������ʾ��AT��DF��-10")]
    public int atk;

    public int def;

    public string playerName;
    public List<int> myItems;//��ұ������� ������
    public int wearArm, wearArmor;
    public bool canMove;

    public float gameTime;

    [Header("�����νӴ洢")]
    public string lastScene;

    public string saveScene;

    [Header("��������")]
    public bool isDebug;

    [Header("--����ģʽ�趨--")]
    [Header("��Ѫ")]
    public bool invincible;
}