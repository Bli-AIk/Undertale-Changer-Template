using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Overworld.FiniteStateMachine
{
    
    [CreateAssetMenu(fileName = "FsmData", menuName = "UCT/FsmData")]
    public class FiniteStateMachineData : ScriptableObject
    {
        public float speed = 5;
        public float speedForReal = 5;
        public Vector3 direction;
        public Vector3 directionWithoutZero = Vector3.down;
        public Vector3 directionPlayer = Vector3.down;
        public Animator animator;
        public Rigidbody2D rigidbody2D;
    }
}