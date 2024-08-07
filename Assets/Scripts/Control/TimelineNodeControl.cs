using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弹幕文件
/// </summary>
[CreateAssetMenu(fileName = "TimelineNodeControl", menuName = "UCT-Other/TimelineNodeControl")]
public class TimelineNodeControl : ScriptableObject
{
    [Header("节点开始时间")]
    public float startTime;
    [Header("节点结束时间")]
    public float endTime;



}
