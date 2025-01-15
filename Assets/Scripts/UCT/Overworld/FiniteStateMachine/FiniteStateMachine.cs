using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;

namespace UCT.Overworld.FiniteStateMachine
{
    public enum DefaultStateType
    {
        Idle,
        Walk,
        Run
    }
    
    public class FiniteStateMachine : MonoBehaviour
    {
        protected IState CurrentState;
        [ReadOnly]
        public FiniteStateMachineData data;
        public string dataRoute;
        protected readonly Dictionary<DefaultStateType, IState> States = new();
        private void Awake()
        {
            InitializeData();
            InitializeStates();
        }

        private void InitializeData()
        {
            if (string.IsNullOrEmpty(dataRoute))
                dataRoute = "FiniteStateMachine/Default";
            data = Resources.Load<FiniteStateMachineData>($"FiniteStateMachine/{dataRoute}");

            data.animator = GetComponent<Animator>();
            data.rigidbody2D = GetComponent<Rigidbody2D>();
        }


        protected virtual void InitializeStates()
        {
            States.Add(DefaultStateType.Idle, new IdleState(this, data));
            States.Add(DefaultStateType.Walk, new WalkState(this, data));
            States.Add(DefaultStateType.Run, new RunState(this, data));
            TransitionState(States[DefaultStateType.Idle]);
        }
        private void Update()
        {
            CurrentState.OnUpdate();
        }

        private void FixedUpdate()
        {
            CurrentState.OnFixedUpdate();
        }

        protected void TransitionState(IState newState)
        {
            CurrentState?.OnExit();
            CurrentState = newState;
            CurrentState.OnEnter();
        }
    }
}