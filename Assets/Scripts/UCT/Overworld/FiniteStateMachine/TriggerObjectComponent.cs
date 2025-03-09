using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;

namespace UCT.Overworld.FiniteStateMachine
{
    public class TriggerObjectComponent : MonoBehaviour
    {
        [ReadOnly] public FiniteStateMachine fsm;

        public List<Vector2> specifiedDirectionsList = new();
        public List<Vector2> colliderOffsetsList = new();
        public List<Vector2> colliderSizesList = new();
        public List<Vector3> localPositionsList = new();
        public List<Vector3> localRotationsList = new();

        private BoxCollider2D _triggerCollider;

        private void Start()
        {
            fsm = transform.GetComponent<FiniteStateMachine>();

            var triggerTransform = transform.Find("Trigger");
            if (triggerTransform)
            {
                _triggerCollider = triggerTransform.GetComponent<BoxCollider2D>();
                if (!_triggerCollider)
                {
                    Debug.LogError("Trigger object must have a BoxCollider2D component.");
                }
            }
            else
            {
                Debug.LogError("No child object named 'Trigger' found.");
            }
        }

        private void Update()
        {
            if (!fsm || !_triggerCollider)
            {
                return;
            }

            var direction = fsm.data.directionPlayer;

            if (specifiedDirectionsList.Count == 0 || colliderOffsetsList.Count == 0)
            {
                return;
            }

            var closestAngle = float.MaxValue;
            var closestIndex = -1;

            for (var i = 0; i < specifiedDirectionsList.Count; i++)
            {
                var angle = Vector2.Angle(direction, specifiedDirectionsList[i]);
                if (angle >= closestAngle)
                {
                    continue;
                }

                closestAngle = angle;
                closestIndex = i;
            }

            if (closestIndex >= 0)
            {
                ApplyTriggerSettings(closestIndex);
            }
        }

        private void ApplyTriggerSettings(int index)
        {
            var triggerTransform = _triggerCollider.transform;
            if (!triggerTransform)
            {
                return;
            }

            if (index < localPositionsList.Count)
            {
                triggerTransform.localPosition = localPositionsList[index];
            }


            if (index < localRotationsList.Count)
            {
                triggerTransform.localRotation = Quaternion.Euler(localRotationsList[index]);
            }


            if (index >= colliderOffsetsList.Count || index >= colliderSizesList.Count)
            {
                return;
            }

            _triggerCollider.offset = new Vector3(colliderOffsetsList[index].x, colliderOffsetsList[index].y);
            _triggerCollider.size = new Vector3(colliderSizesList[index].x, colliderSizesList[index].y);
        }
    }
}