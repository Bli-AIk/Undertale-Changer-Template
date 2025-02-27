using System.Collections.Generic;
using UnityEngine;
using static UCT.Control.BattleControl;
using static UCT.Battle.BulletController;

namespace UCT.Control
{
    /// <summary>
    ///     ��Ļ�ļ�
    /// </summary>
    [CreateAssetMenu(fileName = "BulletControl", menuName = "UCT/BulletControl")]
    public class BulletControl : ScriptableObject
    {
        [Header("��Ļ����������")]
        public string typeName;

        [Header("��ĻĬ�ϵ�Obj����")]
        public string objName;

        [Header("���Ϊ100��ս�����ԵΪ50���ɲο�")]
        public int layer;

        [Header("����")]
        public Sprite sprite;

        [Header("�ж����С")]
        public List<Vector2> triggerSize = new() { new Vector2(1, 1) };

        [Header("�ж����˺�")]
        public List<int> triggerHit = new() { 1 };

        [Header("�ж���ƫ��")]
        public List<Vector2> triggerOffset = new() { new Vector2() };

        [Header("��ʼ�������")]
        public Vector3 startPosition;

        [Header("��ʼ��ת�Ƕ�")]
        public Vector3 startRotation;

        [Header("��ʼ����")]
        public Vector3 startScale = new(1, 1, 1);

        [Header("������ɫ����")]
        public BulletColor bulletColor = BulletColor.White;

        [Header("Sprite����ģʽ")]
        public SpriteMaskInteraction startMask = SpriteMaskInteraction.None;

        [Header("��ײ������ģʽ")]
        public FollowMode triggerFollowMode = FollowMode.NoFollow;
    }
}