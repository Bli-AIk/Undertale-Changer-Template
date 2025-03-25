using System.Collections.Generic;
using System.Linq;
using Alchemy.Inspector;
using UCT.Core;
using UCT.Overworld.FiniteStateMachine;
using UCT.Service;
using UCT.Settings;
using UnityEngine;

namespace UCT.Overworld
{
    [RequireComponent(typeof(OverworldPlayerAnimEventHelper))]
    public class OverworldPlayerBehaviour : FiniteStateMachine.FiniteStateMachine
    {
        public float owTimer;
        public Vector2 walkFxRange = new(0, 9);
        public StateType stateType;

        [Title("开启倒影")] public bool isShadow;
        [HideInInspector] public SpriteRenderer spriteRenderer;
        [HideInInspector] public SpriteRenderer shadowSpriteRenderer;
        [HideInInspector] public SpriteRenderer outline, heart;

        
        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            shadowSpriteRenderer = transform.Find("BottomAxis/Shadow").GetComponent<SpriteRenderer>();
            outline = transform.Find("Outline").GetComponent<SpriteRenderer>();

            heart = transform.Find("Heart").GetComponent<SpriteRenderer>();

            outline.gameObject.SetActive(false);
            heart.gameObject.SetActive(false);
        }


        private void Update()
        {
            spriteRenderer.enabled = MainControl.Instance.sceneState == MainControl.SceneState.Overworld;
            if (!spriteRenderer.enabled)
            {
                return;
            }
            
            if (MainControl.Instance.playerControl.hp <= 0)
            {
                MainControl.Instance.KillPlayer(transform.position - MainControl.Instance.mainCamera.transform.position);
            }
            
            if (!MainControl.Instance.isSceneSwitching)
            {
                MainControl.Instance.playerControl.playerLastPos = transform.position;
            }

            if (owTimer > 0)
            {
                owTimer -= Time.deltaTime;
            }

            if (GameUtilityService.IsGamePausedOrSetting() || BackpackBehaviour.Instance.select > 0)
            {
                if (!IsSpecialState())
                {
                    TransitionToStateIfNeeded(StateType.Idle);
                }

                return;
            }

            InputPlayerMove();
            SetShadow();
        }


        private bool IsSpecialState()
        {
            return stateType == StateType.Spin;
        }

        protected override void InitializeStates()
        {
            States.Add(StateType.Idle, new IdleState(this, data));
            States.Add(StateType.Walk, new WalkState(this, data));
            States.Add(StateType.Run, new RunState(this, data));
            States.Add(StateType.Spin, new SpinState(this, data));
            TransitionState(States[StateType.Idle]);
        }

        private void SetShadow()
        {
            shadowSpriteRenderer.transform.parent.gameObject.SetActive(isShadow);
            if (isShadow)
            {
                shadowSpriteRenderer.sprite = spriteRenderer.sprite;
            }
        }


        private void InputPlayerMove()
        {
            data.direction = Vector3.zero;
            if (!MainControl.Instance.playerControl.canMove)
            {
                UpdateAnimationDirection();
                UpdatePlayerState(false);
                return;
            }

            var isGetKey = ProcessInputDirection();
            UpdateAnimationDirection();
            UpdatePlayerState(isGetKey);
        }


        private bool ProcessInputDirection()
        {
            SetKeyMap(out var directionMapping,
                out var conflictingKeys);

            var keyValuePairs = from pair in directionMapping
                let isConflicting = conflictingKeys.Any(conflict =>
                    (pair.Key == conflict.Item1 && InputService.GetKey(conflict.Item2)) ||
                    (pair.Key == conflict.Item2 && InputService.GetKey(conflict.Item1))
                )
                where !isConflicting && InputService.GetKey(pair.Key)
                select pair;

            var isGetKey = false;
            foreach (var pair in keyValuePairs)
            {
                data.direction += pair.Value;
                isGetKey = true;
            }

            return isGetKey;
        }

        private static void SetKeyMap(out Dictionary<KeyCode, Vector3> directionMapping,
            out List<(KeyCode, KeyCode)> conflictingKeys)
        {
            directionMapping = new Dictionary<KeyCode, Vector3>
            {
                { KeyCode.UpArrow, Vector3.up },
                { KeyCode.DownArrow, Vector3.down },
                { KeyCode.LeftArrow, Vector3.left },
                { KeyCode.RightArrow, Vector3.right }
            };

            conflictingKeys = new List<(KeyCode, KeyCode)>
            {
                (KeyCode.UpArrow, KeyCode.DownArrow),
                (KeyCode.LeftArrow, KeyCode.RightArrow)
            };
        }

        private void UpdateAnimationDirection()
        {
            if (data.direction != Vector3.zero)
            {
                data.directionWithoutZero = data.direction;
            }

            data.directionPlayer = Mathf.Abs(data.directionWithoutZero.x) > Mathf.Abs(data.directionWithoutZero.y)
                ? new Vector3(Mathf.Sign(data.directionWithoutZero.x), 0, 0)
                : new Vector3(0, Mathf.Sign(data.directionWithoutZero.y), 0);
        }

        private void UpdatePlayerState(bool isGetKey)
        {
            if (isGetKey)
            {
                stateType = !InputService.GetKey(KeyCode.X) ? StateType.Walk : StateType.Run;
            }
            else if (!IsSpecialState())
            {
                stateType = StateType.Idle;
            }

            TransitionToStateIfNeeded(stateType);
        }


        private void TransitionToStateIfNeeded(IState targetState)
        {
            if (CurrentState != targetState)
            {
                TransitionState(targetState);
            }
        }

        public void TransitionToStateIfNeeded(StateType inputStateType)
        {
            stateType = inputStateType;
            TransitionToStateIfNeeded(States[inputStateType]);
        }
    }
}