using System.Collections.Generic;
using System.Linq;
using Alchemy.Inspector;
using UCT.Global.Core;
using UCT.Global.Settings;
using UCT.Overworld.FiniteStateMachine;
using UCT.Service;
using UnityEngine;

namespace UCT.Overworld
{ 
    [RequireComponent(typeof(OverworldPlayerAnimEventHelper))]
    public class OverworldPlayerBehaviour : FiniteStateMachine.FiniteStateMachine
    {
        
        
        public float owTimer; 
        public Vector2 walkFxRange = new(0, 9);
        
        
        [Title("开启倒影")] 
        public bool isShadow;
        private SpriteRenderer _spriteRenderer, _shadowSpriteRenderer;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _shadowSpriteRenderer = transform.Find("BottomAxis/Shadow").GetComponent<SpriteRenderer>();
        }
        private void Update()
        {
            MainControl.Instance.overworldControl.playerLastPos = transform.position;
            if (owTimer > 0) 
                owTimer -= Time.deltaTime;
            
            if (MainControl.Instance.overworldControl.isSetting || SettingsStorage.pause ||
                BackpackBehaviour.Instance.select > 0)
            {
                TransitionToStateIfNeeded(States[DefaultStateType.Idle]);
                return;
            }
            
            InputPlayerMove();
            SetShadow();
        }

        private void SetShadow()
        {
            _shadowSpriteRenderer.transform.parent.gameObject.SetActive(isShadow);
            if (isShadow)
            {
                _shadowSpriteRenderer.sprite = _spriteRenderer.sprite;
            }
        }


        private void InputPlayerMove()
        {
            data.direction = Vector3.zero;
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

        private static void SetKeyMap(out Dictionary<KeyCode, Vector3> directionMapping, out List<(KeyCode, KeyCode)> conflictingKeys)
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
                data.directionForAnim = data.direction;
        }

        private void UpdatePlayerState(bool isGetKey)
        {
            if (isGetKey)
            {
                TransitionToStateIfNeeded(!InputService.GetKey(KeyCode.X)
                    ? States[DefaultStateType.Walk]
                    : States[DefaultStateType.Run]);
            }
            else
            {
                TransitionToStateIfNeeded(States[DefaultStateType.Idle]);
            }
        }


        private void TransitionToStateIfNeeded(IState targetState)
        {
            if (CurrentState != targetState)
                TransitionState(targetState);
        }

    }
}