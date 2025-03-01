using System.Collections.Generic;
using UnityEngine;
using static UCT.Control.BattleControl;
using static UCT.Battle.BulletController;

namespace UCT.Control
{
    /// <summary>
    ///     弹幕文件
    /// </summary>
    [CreateAssetMenu(fileName = "BulletControl", menuName = "UCT/BulletControl")]
    public class BulletControl : ScriptableObject
    {
        [Header("弹幕类型名称")]
        public string typeName;

        [Header("弹幕默认Obj名称")]
        public string objName;

        [Header("图层")]
        public int layer;

        [Header("精灵")]
        public Sprite sprite;

        [Header("触发器尺寸")]
        public List<Vector2> triggerSize = new() { new Vector2(1, 1) };

        [Header("触发器偏移")]
        public List<Vector2> triggerOffset = new() { new Vector2() };

        [Header("触发器伤害")]
        public List<int> triggerHit = new() { 1 };

        [Header("初始位置")]
        public Vector3 startPosition;

        [Header("初始旋转角度")]
        public Vector3 startRotation;

        [Header("初始缩放")]
        public Vector3 startScale = new(1, 1, 1);

        [Header("弹幕属性颜色")]
        public BulletColor bulletColor = BulletColor.White;

        [Header("Sprite遮罩模式")]
        public SpriteMaskInteraction startMask = SpriteMaskInteraction.None;

        [Header("触发后跟随模式")]
        public FollowMode triggerFollowMode = FollowMode.NoFollow;
    }
}