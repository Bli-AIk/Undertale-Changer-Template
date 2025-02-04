using System;
using System.Collections.Generic;
using System.Linq;
using Plugins.Timer.Source;
using UCT.Global.Core;
using UCT.Overworld.FiniteStateMachine;
using UCT.Service;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.EventSystem
{
    public class OverworldEventTrigger : MonoBehaviour
    {
        public Action OnTriggerEvent;
        
        public string fsmObjectName;

        public FiniteStateMachine fsmObject;

        public List<string> tags;
        
        public List<EventTriggerMode> eventTriggerModes;

        [FormerlySerializedAs("events")] 
        public List<string> eventNames;

        [FormerlySerializedAs("useSimpleRule")] public bool useSimpleRules;
        
        public List<RuleEntry> simpleRules;
        
        /// <summary>
        /// 能见度，取值范围为0-1。
        /// 用于视线检测。
        /// </summary>
        public float clarity = 0.5f;
        
        private void Start()
        {
            GetFsmObject();
        }

        private bool IsCompareTag(GameObject obj)
        {
            return tags.Any(obj.CompareTag);
        }
        
        public bool IsEventTriggerModeActive(EventTriggerMode mode)
        {
            return eventTriggerModes != null && eventTriggerModes.Contains(mode);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!IsCompareTag(other.gameObject)) return;

            if (IsEventTriggerModeActive(EventTriggerMode.ColliderEnter))
                TriggerEvent();
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (!IsCompareTag(other.gameObject)) return;

            if (IsEventTriggerModeActive(EventTriggerMode.ColliderExit))
                TriggerEvent();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsCompareTag(other.gameObject)) return;

            
            if (IsEventTriggerModeActive(EventTriggerMode.TriggerEnter))
                TriggerEvent();
        }

        private bool _hasTriggeredInteract; //   避免Interact状态被多次触发
        private void OnTriggerStay2D(Collider2D other)
        {

            if (InputService.GetKeyUp(KeyCode.Z))
                _hasTriggeredInteract = false;

            if (!IsEventTriggerModeActive(EventTriggerMode.Interact) || !other.CompareTag("PlayerTrigger") ||
                !InputService.GetKey(KeyCode.Z) || _hasTriggeredInteract) return;
            TriggerEvent();
            _hasTriggeredInteract = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {

            if (!IsCompareTag(other.gameObject)) return;

            if (IsEventTriggerModeActive(EventTriggerMode.TriggerExit))
                TriggerEvent();
        }


        private void GetFsmObject()
        {
            if (!fsmObject && string.IsNullOrEmpty(fsmObjectName))
                fsmObject = MainControl.overworldPlayerBehaviour;
            else if (!fsmObject || fsmObjectName != fsmObject.name)
                fsmObject = GameObject.Find(fsmObjectName).GetComponent<FiniteStateMachine>();
        }

        /// <summary>
        ///     触发事件。
        /// </summary>
        public void TriggerEvent()
        {
            if (GameUtilityService.IsGamePausedOrSetting()) return;

            if (!useSimpleRules)
            {
                for (var i = 0; i < eventNames.Count; i++) 
                {
                    var localName = eventNames[i];
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var j = 0; j < EventController.eventTable.events.Count; j++)
                    {
                        var eventEntry = EventController.eventTable.events[j];

                        if (eventEntry.name != localName) continue;

                        if (eventEntry.closeTime >= 0)
                        {
                            var index = j;
                            Timer.Register(eventEntry.closeTime, () =>
                            {
                                if (!EventController.eventTable.events[index].isTriggering)
                                    return;
                                var falseEntry = eventEntry;
                        
                                eventEntry.isTriggering = false;
                                eventNames[index] = localName;
                                EventController.eventTable.events[index] = falseEntry;
                            });
                        }

                        eventEntry.isTriggering = true;
                        eventNames[i] = localName;
                        EventController.eventTable.events[j] = eventEntry;
                    }
                }
            }
            else
            {
                foreach (var rule in simpleRules)
                {
                    EventController.DetectionRule(new EventEntry(), rule, out var isTriggered, true);
                    if (isTriggered) break;
                }
            }
            
            OnTriggerEvent?.Invoke();
        }
    }

    /// <summary>
    ///     定义事件触发模式的枚举类型。
    /// </summary>
    public enum EventTriggerMode
    {
        /// <summary>
        ///     调查型：按下调查键后触发。仅用于玩家，无需配置Tags。
        /// </summary>
        Interact,

        /// <summary>
        ///     碰撞器触发型：开始碰撞后触发。
        /// </summary>
        ColliderEnter,

        /// <summary>
        ///     碰撞器离开型：结束碰撞后触发。
        /// </summary>
        ColliderExit,

        /// <summary>
        ///     触发器触发型：进入触发器范围后触发。
        /// </summary>
        TriggerEnter,

        /// <summary>
        ///     触发器离开型：离开触发器范围后触发。
        /// </summary>
        TriggerExit,

        /// <summary>
        ///     视线触发型：进入视野（射线范围）时触发。
        /// </summary>
        LineOfSightEnter,

        /// <summary>
        ///     视线离开型：离开视野（射线范围）时触发。
        /// </summary>
        LineOfSightExit
        
    }
    
}