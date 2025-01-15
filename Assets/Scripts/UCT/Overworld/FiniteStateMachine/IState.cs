using UnityEngine;

namespace UCT.Overworld.FiniteStateMachine
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate();
        void OnFixedUpdate();
        void OnExit();
    }

    public class IdleState : IState
    {
        private FiniteStateMachineData _data;
        private FiniteStateMachine _fsm;

        public IdleState(FiniteStateMachine fsm, FiniteStateMachineData data)
        {
            _fsm = fsm;
            _data = data;
        }

        public void OnEnter()
        {
            _data.animator.Play("Idle Tree");
        }

        public void OnUpdate()
        {
            //  无事发生
        }

        public void OnFixedUpdate()
        {
            //  无事发生
        }

        public void OnExit()
        {
            //  无事发生
        }
    }
    
    public class WalkState : IState
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");

        private readonly FiniteStateMachineData _data;
        private FiniteStateMachine _fsm;

        public WalkState(FiniteStateMachine fsm, FiniteStateMachineData data)
        {
            _fsm = fsm;
            _data = data;
        }

        public void OnEnter()
        {
            _data.animator.Play("Walk Tree");
        }

        public void OnUpdate()
        {
            //  无事发生
        }

        public void OnFixedUpdate()
        {
            _data.speedForReal = _data.speed;
            var step = _data.speedForReal * Time.deltaTime;
            var pos = _data.rigidbody2D.transform.position;
            _data.rigidbody2D.MovePosition(pos + _data.direction * step);
            
            _data.animator.SetFloat(MoveX, _data.directionWithoutZero.x);
            _data.animator.SetFloat(MoveY, _data.directionWithoutZero.y);
        }

        public void OnExit()
        {
            //  无事发生
        }
    }


    public class RunState : IState
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");

        private FiniteStateMachineData _data;
        private FiniteStateMachine _fsm;

        public RunState(FiniteStateMachine fsm, FiniteStateMachineData data)
        {
            _fsm = fsm;
            _data = data;
        }

        public void OnEnter()
        {
            _data.animator.Play("Run Tree");
        }

        public void OnUpdate()
        {
            //  无事发生
        }

        public void OnFixedUpdate()
        {
            _data.speedForReal = _data.speed * 1.5f;
            var step = _data.speedForReal * Time.deltaTime;
            var pos = _data.rigidbody2D.transform.position;
            _data.rigidbody2D.MovePosition(pos + _data.direction * step);
            
            _data.animator.SetFloat(MoveX, _data.directionWithoutZero.x);
            _data.animator.SetFloat(MoveY, _data.directionWithoutZero.y);
        }

        public void OnExit()
        {
            //  无事发生
        }
    }
}