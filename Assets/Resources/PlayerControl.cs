using System.Collections;
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
    public Vector3 deadPos;

    [Header("�����νӴ洢")]
    public Vector3 scenePos;
    public Vector2 animDirection;
    public string lastScene;
}
