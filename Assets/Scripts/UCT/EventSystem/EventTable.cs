using System.Collections.Generic;
using UnityEngine;

namespace UCT.EventSystem
{
    [CreateAssetMenu(fileName = "EventTable", menuName = "UCT-EventSystem/EventTable")]
    public class EventTable : ScriptableObject
    {
        public List<EventEntry> events;
    }
}