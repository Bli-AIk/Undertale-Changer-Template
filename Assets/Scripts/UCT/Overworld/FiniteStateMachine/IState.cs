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
        private readonly FiniteStateMachineData _data;
        private readonly FiniteStateMachine _fsm;

        public IdleState(FiniteStateMachine fsm, FiniteStateMachineData data)
        {
            _fsm = fsm;
            _data = data;
        }

        public void OnEnter()
        {
            if (_data.animator)
            {
                _data.animator.Play("Idle Tree");
            }
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
        private readonly FiniteStateMachine _fsm;

        public WalkState(FiniteStateMachine fsm, FiniteStateMachineData data)
        {
            _fsm = fsm;
            _data = data;
        }

        public void OnEnter()
        {
            if (_data.animator)
            {
                _data.animator.Play("Walk Tree");
            }
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

            if (!_data.animator)
            {
                return;
            }

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

        private readonly FiniteStateMachineData _data;
        private readonly FiniteStateMachine _fsm;

        public RunState(FiniteStateMachine fsm, FiniteStateMachineData data)
        {
            _fsm = fsm;
            _data = data;
        }

        public void OnEnter()
        {
            if (_data.animator)
            {
                _data.animator.Play("Run Tree");
            }
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
            if (!_data.animator)
            {
                return;
            }

            _data.animator.SetFloat(MoveX, _data.directionWithoutZero.x);
            _data.animator.SetFloat(MoveY, _data.directionWithoutZero.y);
        }

        public void OnExit()
        {
            //  无事发生
        }
    }

    public class SpinState : IState
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");

        private readonly FiniteStateMachineData _data;

        private readonly Vector2[] _directions = { Vector2.up, Vector2.left, Vector2.down, Vector2.right };
        private readonly FiniteStateMachine _fsm;
        private int _currentIndex;
        private float _timer;

        public SpinState(FiniteStateMachine fsm, FiniteStateMachineData data)
        {
            _fsm = fsm;
            _data = data;
        }

        public void OnEnter()
        {
            if (_data.animator)
            {
                _data.animator.Play("Idle Tree");
            }
        }

        public void OnUpdate()
        {
            //  无事发生
        }


        public void OnFixedUpdate()
        {
            _timer += Time.fixedDeltaTime;
            if (_timer >= 0.15f)
            {
                _timer -= 0.15f;
                _currentIndex = (_currentIndex + 1) % _directions.Length;
            }

            var direction = _directions[_currentIndex];
            if (!_data.animator)
            {
                return;
            }

            _data.animator.SetFloat(MoveX, direction.x);
            _data.animator.SetFloat(MoveY, direction.y);
        }

        public void OnExit()
        {
            //  无事发生
        }
    }
}