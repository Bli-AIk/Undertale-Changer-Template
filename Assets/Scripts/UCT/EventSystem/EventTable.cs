using System.Collections.Generic;
using UnityEngine;

namespace UCT.EventSystem
{
    [CreateAssetMenu(fileName = "EventTable", menuName = "UCT/EventTable")]
    public class EventTable : ScriptableObject
    {
        public List<EventEntry> events;
    }
}