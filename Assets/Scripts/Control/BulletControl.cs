using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleControl;
using static BulletController;

[CreateAssetMenu(fileName = "BulletControl", menuName = "UCT-Common/BulletControl")]
public class BulletControl : ScriptableObject
{
    public string objName;
    public string typeName;
    public int layer;
    public Sprite sprite;
    public List<Vector2> triggerOffset = new List<Vector2>() { new Vector2() };
    public List<Vector2> triggerSize = new List<Vector2>() { new Vector2(1, 1) };
    public List<int> triggerHit = new List<int> { 1 };
    public Vector3 startPosition = new Vector3();
    public Vector3 startRotation = new Vector3();
    public Vector3 startScale = new Vector3();
    public BulletColor bulletColor = BulletColor.white;
    public SpriteMaskInteraction startMask = SpriteMaskInteraction.None;
    public FollowMode triggerFollowMode = FollowMode.NoFollow;
}
