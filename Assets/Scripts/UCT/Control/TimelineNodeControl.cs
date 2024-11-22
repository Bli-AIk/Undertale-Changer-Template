using UnityEngine;

namespace UCT.Control
{
    /// <summary>
    ///     ��Ļ�ļ�
    /// </summary>
    [CreateAssetMenu(fileName = "TimelineNodeControl", menuName = "UCT-Other/TimelineNodeControl")]
    public class TimelineNodeControl : ScriptableObject
    {
        [Header("�ڵ㿪ʼʱ��")] public float startTime;

        [Header("�ڵ����ʱ��")] public float endTime;
    }
}