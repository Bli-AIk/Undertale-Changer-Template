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
    public Vector2 size;
    public int hit;
    public Vector2 offset;
    public Vector3 startPosition = new Vector3();
    public BulletColor bulletColor = BulletColor.white;
    public SpriteMaskInteraction startMask = SpriteMaskInteraction.None;
    public Vector3 startRotation = new Vector3();
    public Vector3 startScale = new Vector3();
    public FollowMode followMode = FollowMode.NoFollow;



}
