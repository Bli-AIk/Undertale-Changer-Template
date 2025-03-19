using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MEC;
using UCT.Audio;
using UCT.Control;
using UCT.Core;
using UCT.Service;
using UCT.Settings;
using UCT.UI;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    ///     控制战斗内玩家(心)的相关属性
    /// </summary>
    public class BattlePlayerController : MonoBehaviour
    {
        public enum PlayerDirection
        {
            Up,
            Down,
            Left,
            Right,
            NullDir
        }

        private const float SpeedWeight = 0.5f;
        private const string Board = "board";
        private const float YellowTimerMax = 0.5f;

        [Header("心变色时的ding动画速度，0为关")]
        public float dingTime;

        [Header("心渐变动画速度，0为关")]
        public float gradientTime;

        [Header("基本属性调整")]
        public float speed;

        /// <summary>
        ///     速度与权重(按X乘以倍数)，速度测试为3，权重0.5f
        /// </summary>
        public float speedWeightX, speedWeightY;

        /// <summary>
        ///     碰撞距离判定
        /// </summary>
        public float displacement = 0.175f;

        /// <summary>
        ///     用于蓝橙骨判断：玩家是否真的在移动
        /// </summary>
        public bool isMoving;

        /// <summary>
        ///     时间插值
        /// </summary>
        public float timeInterpolation = -0.225f;

        /// <summary>
        ///     场景偏移
        /// </summary>
        public Vector2 sceneDrift = new(-1000, 0);

        /// <summary>
        ///     玩家方向
        /// </summary>
        public PlayerDirection playerDir;

        /// <summary>
        ///     玩家移动方向，用于橙心和绿心判断
        /// </summary>
        public float angle;

        /// <summary>
        ///     用于判断绿魂旋转角度
        /// </summary>
        public bool isRotate;

        /// <summary>
        ///     当前角度
        /// </summary>
        public float currentAngle;

        /// <summary>
        ///     箭头位置
        /// </summary>
        public Transform arrowPosition;

        /// <summary>
        ///     橙魂移动残影开关
        /// </summary>
        public ParticleSystem orangeDash;

        /// <summary>
        ///     移动向量
        /// </summary>
        public Vector3 moving;

        /// <summary>
        ///     是否处于“跳起”状态
        /// </summary>
        public bool isJump;

        /// <summary>
        ///     跳跃的加速度
        /// </summary>
        public float jumpAcceleration;

        /// <summary>
        ///     跳跃射线距离
        /// </summary>
        public float jumpRayDistance;

        /// <summary>
        ///     板上的跳跃射线距离
        /// </summary>
        public float jumpRayDistanceForBoard;

        /// <summary>
        ///     圆形碰撞体
        /// </summary>
        [HideInInspector] public CircleCollider2D collideCollider;

        [HideInInspector] public Animator animator;

        /// <summary>
        ///     含有属性的颜色，读取 <see cref="BattleControl.PlayerColor" />，颜色变换通过具体变换的函数来执行
        /// </summary>
        public BattleControl.PlayerColor playerColor;

        [HideInInspector] public UnityEngine.Rendering.Volume hitVolume;

        /// <summary>
        ///     当前行
        /// </summary>
        private int _currentLineIndex;

        private Tween _missAnim, _changeColor, _changeDingColor, _changeDingScale;

        private Rigidbody2D _rigidBody;
        private SpriteRenderer _spriteRenderer, _dingSpriteRenderer;
        private float _yellowTimer;

        private void Start()
        {
            speedWeightX = 1;
            speedWeightY = 1;
            jumpRayDistance = 0.2f;
            jumpRayDistanceForBoard = 0.2f;
            jumpAcceleration = 1.25f;
            playerColor = BattleControl.PlayerColor.Red;
            playerDir = PlayerDirection.Down;
            _rigidBody = GetComponent<Rigidbody2D>();
            collideCollider = GetComponent<CircleCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            _dingSpriteRenderer = transform.Find("Ding").GetComponent<SpriteRenderer>();
            _dingSpriteRenderer.color = Color.clear;
            hitVolume = GetComponent<UnityEngine.Rendering.Volume>();
            hitVolume.weight = 0;
            MainControl.Instance.playerControl.missTime = 0;
            orangeDash.Stop();
            arrowPosition.GetComponent<SpriteRenderer>().enabled = false;
        }

        private void Update()
        {
            UpdateHitVolume();

            PurpleSwitchLineInput();

            if (MainControl.Instance.playerControl.hp <= 0)
            {
                PlayerDead();
            }

            if (GameUtilityService.IsGamePausedOrSetting())
            {
                return;
            }

            UpdatePlayerMissTime();

            if (MainControl.Instance.playerControl.isDebug)
            {
                DebugInput();
            }
        }

        private void PurpleSwitchLineInput()
        {
            if (playerColor != BattleControl.PlayerColor.Purple || TurnController.Instance.isMyTurn)
            {
                return;
            }

            var playerLineController = MainControl.Instance.selectUIController.PlayerLineController;

            if (InputService.GetKeyDown(KeyCode.UpArrow) && _currentLineIndex > 0)
            {
                _currentLineIndex -= 1;
            }

            if (InputService.GetKeyDown(KeyCode.DownArrow) && _currentLineIndex < playerLineController.lines.Count - 1)
            {
                _currentLineIndex += 1;
            }
        }

        private void FixedUpdate()
        {
            if (GameUtilityService.IsGamePausedOrSetting())
            {
                return;
            }

            if (!TurnController.Instance.isMyTurn)
            {
                PlayerMoving();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            //蓝心碰板子确保再次可以跳
            if (collision.transform.CompareTag(Board) &&
                collision.transform.GetComponent<EdgeCollider2D>().IsTouching(collideCollider) &&
                playerColor == BattleControl.PlayerColor.Blue)
            {
                BlueJumpReady();
            }
        }


        /// <summary>
        ///     掉出
        /// </summary>
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.transform.CompareTag(Board) && playerColor == BattleControl.PlayerColor.Blue &&
                !isJump)
            {
                BlueDown(0, playerDir);
            }
        }

        private void UpdatePlayerMissTime()
        {
            if (MainControl.Instance.playerControl.missTime >= 0)
            {
                MainControl.Instance.playerControl.missTime -= Time.deltaTime;
                if (_missAnim == null && MainControl.Instance.playerControl.missTimeMax >= 0.4f)
                {
                    _missAnim = _spriteRenderer
                        .DOColor(MainControl.Instance.BattleControl.playerMissColorList[(int)playerColor], 0.2f)
                        .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                }
            }
            else
            {
                if (_missAnim == null)
                {
                    return;
                }

                _missAnim.Kill();
                _missAnim = null;
                _spriteRenderer.color = MainControl.Instance.BattleControl.playerColorList[(int)playerColor];
            }
        }

        private void PlayerDead()
        {
            MainControl.Instance.playerControl.hp = MainControl.Instance.playerControl.hpMax;

            if (!(MainControl.Instance.playerControl.isDebug && MainControl.Instance.playerControl.invincible))
            {
                _spriteRenderer.color = Color.red;
                MainControl.Instance.playerControl.playerLastPosInBattle = transform.position - (Vector3)sceneDrift;
                SettingsStorage.Pause = true;
                Timing.KillCoroutines();
                GameUtilityService.SwitchScene("GameOver", false);
            }
            else
            {
                MainControl.Instance.selectUIController.UITextUpdate(SelectUIController.UITextMode.Hit);
            }
        }

        private void UpdateHitVolume()
        {
            if (!SettingsStorage.IsSimplifySfx && hitVolume.weight > 0)
            {
                hitVolume.weight -= Time.deltaTime;
            }
        }

        private void DebugInput()
        {
            var keyMappings = new Dictionary<KeyCode, int>
            {
                { KeyCode.Keypad1, 0 },
                { KeyCode.Keypad2, 1 },
                { KeyCode.Keypad3, 2 },
                { KeyCode.Keypad4, 3 },
                { KeyCode.Keypad6, 5 },
                { KeyCode.Keypad7, 6 },
                { KeyCode.Alpha1, 0 },
                { KeyCode.Alpha2, 1 },
                { KeyCode.Alpha3, 2 },
                { KeyCode.Alpha4, 3 },
                { KeyCode.Alpha6, 5 },
                { KeyCode.Alpha7, 6 }
            };

            var isCtrlPressed = Input.GetKey(KeyCode.Tab);

            foreach (var value in keyMappings
                         .Where(kvp => Input.GetKeyDown(kvp.Key))
                         .Where(kvp => kvp.Key.ToString().StartsWith("Keypad") || isCtrlPressed)
                         .Select(kvp => kvp.Value))
            {
                ChangePlayerColor(
                    MainControl.Instance.BattleControl.playerColorList[value],
                    (BattleControl.PlayerColor)value);
            }


            if (Input.GetKeyDown(KeyCode.I))
            {
                ChangePlayerColor(MainControl.Instance.BattleControl.playerColorList[5],
                    (BattleControl.PlayerColor)5, 2.5f, 0);
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                ChangePlayerColor(MainControl.Instance.BattleControl.playerColorList[5],
                    (BattleControl.PlayerColor)5, 2.5f, (PlayerDirection)1);
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                ChangePlayerColor(MainControl.Instance.BattleControl.playerColorList[5],
                    (BattleControl.PlayerColor)5, 2.5f, (PlayerDirection)2);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                ChangePlayerColor(MainControl.Instance.BattleControl.playerColorList[5],
                    (BattleControl.PlayerColor)5, 2.5f, (PlayerDirection)3);
            }
        }

        private void PlayerMoving()
        {
            var dirReal = playerDir switch
            {
                PlayerDirection.Up => Vector2.up,
                PlayerDirection.Down => Vector2.down,
                PlayerDirection.Left => Vector2.left,
                PlayerDirection.Right => Vector2.right,
                _ => new Vector2()
            };

            Ray2D ray = new(transform.position, dirReal);
            Debug.DrawRay(ray.origin, ray.direction, Color.blue);
            var info = Physics2D.Raycast(transform.position, dirReal, jumpRayDistance);

            Ray2D rayHead = new(transform.position, dirReal * -1);
            Debug.DrawRay(rayHead.origin, rayHead.direction, Color.red);
            var infoHead = Physics2D.Raycast(transform.position, dirReal * -1, jumpRayDistance); //反向检测(顶头)


            //------------------------移动------------------------
            PlayerColorMoving(dirReal, infoHead, info);


            isMoving = IsReallyMoving();

            float movingSave = 0;
            if (playerColor == BattleControl.PlayerColor.Blue && isJump)
            {
                movingSave = playerDir switch
                {
                    PlayerDirection.Up => moving.y,
                    PlayerDirection.Down => moving.y,
                    PlayerDirection.Left => moving.x,
                    PlayerDirection.Right => moving.x,
                    _ => movingSave
                };
            }

            moving.x = MathUtilityService.GetGreaterNumber(moving.x, -5);
            moving.y = MathUtilityService.GetGreaterNumber(moving.y, -5);
            moving.x = MathUtilityService.GetSmallerNumber(moving.x, 5);
            moving.y = MathUtilityService.GetSmallerNumber(moving.y, 5);

            var newPos = transform.position + new Vector3(speedWeightX * speed * moving.x * Time.deltaTime,
                speedWeightY * speed * moving.y * Time.deltaTime);

            var checkPos = CheckPoint(newPos, displacement + BoxController.Instance.width / 2);

            if (newPos == checkPos)
            {
                _rigidBody.MovePosition(newPos);
            }
            else
            {
                transform.position = checkPos;
            }


            if (!Mathf.Approximately(movingSave, 0))
            {
                moving.y = movingSave;
            }
        }

        private bool IsReallyMoving()
        {
            bool isMove;
            Vector2 dirMoveX = new();
            Vector2 dirMoveY = new();
            bool isMoveX, isMoveY;
            (dirMoveY, isMoveY, dirMoveX, isMoveX) = GetMovingXy(dirMoveY, false, dirMoveX, false);

            Ray2D rayMoveX = new(transform.position, dirMoveX);
            Debug.DrawRay(rayMoveX.origin, rayMoveX.direction, Color.green);
            var infoMoveX = Physics2D.Raycast(transform.position, dirMoveX, 0.2f);
            Ray2D rayMoveY = new(transform.position, dirMoveY);
            Debug.DrawRay(rayMoveY.origin, rayMoveY.direction, new Color(0, 0.5f, 0, 1));
            var infoMoveY = Physics2D.Raycast(transform.position, dirMoveY, 0.2f);


            if (isMoveX || isMoveY)
            {
                var (x, y) = GetXy(infoMoveX, infoMoveY);

                isMove = !(x || y);
            }
            else
            {
                isMove = playerColor == BattleControl.PlayerColor.Blue &&
                         !Mathf.Approximately(jumpRayDistance, 0);
            }

            return isMove;
        }

        private static (bool x, bool y) GetXy(RaycastHit2D infoMoveX, RaycastHit2D infoMoveY)
        {
            var x = infoMoveX.collider &&
                    (infoMoveX.collider.gameObject.CompareTag("Box") ||
                     infoMoveX.collider.gameObject.CompareTag(Board));
            var y = infoMoveY.collider &&
                    (infoMoveY.collider.gameObject.CompareTag("Box") ||
                     infoMoveY.collider.gameObject.CompareTag(Board));
            if (x && !y && (InputService.GetKey(KeyCode.UpArrow) ||
                            InputService.GetKey(KeyCode.DownArrow)))
            {
                x = false;
            }

            if (y && !x && (InputService.GetKey(KeyCode.LeftArrow) ||
                            InputService.GetKey(KeyCode.RightArrow)))
            {
                y = false;
            }

            return (x, y);
        }

        private static (Vector2 dirMoveY, bool isMoveY, Vector2 dirMoveX, bool isMoveX) GetMovingXy(Vector2 dirMoveY,
            bool isMoveY,
            Vector2 dirMoveX,
            bool isMoveX)
        {
            if (InputService.GetKey(KeyCode.UpArrow))
            {
                dirMoveY = Vector2.up;
                isMoveY = true;
            }
            else if (InputService.GetKey(KeyCode.DownArrow))
            {
                dirMoveY = Vector2.down;
                isMoveY = true;
            }

            if (InputService.GetKey(KeyCode.UpArrow) &&
                InputService.GetKey(KeyCode.DownArrow))
            {
                isMoveY = false;
            }

            if (InputService.GetKey(KeyCode.LeftArrow))
            {
                dirMoveX = Vector2.left;
                isMoveX = true;
            }
            else if (InputService.GetKey(KeyCode.RightArrow))
            {
                dirMoveX = Vector2.right;
                isMoveX = true;
            }

            if (InputService.GetKey(KeyCode.LeftArrow) &&
                InputService.GetKey(KeyCode.RightArrow))
            {
                isMoveX = false;
            }

            return (dirMoveY, isMoveY, dirMoveX, isMoveX);
        }

        private void PlayerColorMoving(Vector2 dirReal, RaycastHit2D infoHead, RaycastHit2D info)
        {
            switch (playerColor)
            {
                case BattleControl.PlayerColor.Red:
                {
                    PlayerCommonMove();
                    break;
                }

                case BattleControl.PlayerColor.Orange:
                {
                    PlayerContinuouslyMove();
                    break;
                }

                case BattleControl.PlayerColor.Yellow:
                {
                    PlayerCommonMove();
                    transform.rotation = Quaternion.Euler(0, 0, 180);
                    if (_yellowTimer < 0 && InputService.GetKey(KeyCode.Z))
                    {
                        _yellowTimer = YellowTimerMax;
                        TurnController.Instance.GetYellowBullet(transform.position);
                    }

                    _yellowTimer -= Time.deltaTime;

                    break;
                }

                case BattleControl.PlayerColor.Green:
                {
                    PlayerFixedDefense();
                    break;
                }

                case BattleControl.PlayerColor.Cyan:
                {
                    break;
                }

                case BattleControl.PlayerColor.Blue:
                {
                    PlayerWithGravity(dirReal, infoHead, info);
                    break;
                }

                case BattleControl.PlayerColor.Purple:
                {
                    PlayerMoveWithLine();
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException($"Unexpected playerColor value: {playerColor}");
                }
            }
        }

        private void PlayerFixedDefense()
        {
            arrowPosition.GetComponent<SpriteRenderer>().enabled = true;
            if (InputService.GetKeyDown(KeyCode.UpArrow))
            {
                angle = 0;
                isRotate = true;
            }

            if (InputService.GetKeyDown(KeyCode.DownArrow))
            {
                angle = 90;
                isRotate = true;
            }

            if (InputService.GetKeyDown(KeyCode.LeftArrow))
            {
                angle = 180;
                isRotate = true;
            }

            if (InputService.GetKeyDown(KeyCode.RightArrow))
            {
                angle = 270;
                isRotate = true;
            }

            if (!isRotate)
            {
                return;
            }

            var step = Mathf.Min(90f * Time.deltaTime, angle - currentAngle);
            arrowPosition.RotateAround(transform.position, Vector3.forward, step);
            currentAngle += step;
            if (currentAngle >= angle)
            {
                isRotate = false;
            }
        }

        private void PlayerWithGravity(Vector2 dirReal, RaycastHit2D infoHead, RaycastHit2D info)
        {
            HandleBoardInteraction(dirReal);

            switch (playerDir)
            {
                case PlayerDirection.Up:
                    ProcessVerticalMovement(true, infoHead, info);
                    break;
                case PlayerDirection.Down:
                    ProcessVerticalMovement(false, infoHead, info);
                    break;
                case PlayerDirection.Left:
                    ProcessHorizontalMovement(true, infoHead, info);
                    break;
                case PlayerDirection.Right:
                    ProcessHorizontalMovement(false, infoHead, info);
                    break;
                case PlayerDirection.NullDir:
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected playerDir value: {playerDir}");
            }
        }

        /// <summary>
        ///     处理与板子（Board）的碰撞和父物体设置逻辑
        /// </summary>
        private void HandleBoardInteraction(Vector2 dirReal)
        {
            var infoForBoard = Physics2D.Raycast(transform.position, dirReal, jumpRayDistanceForBoard);
            if (infoForBoard.collider)
            {
                var obj = infoForBoard.collider.gameObject;
                if (obj.transform.CompareTag(tag) && !isJump && moving == Vector3.zero)
                {
                    BlueDown(0, playerDir);
                }

                if (obj.transform.CompareTag(Board))
                {
                    var board = obj.transform.GetComponent<BoardController>();
                    if (!infoForBoard.collider.isTrigger &&
                        infoForBoard.collider is EdgeCollider2D &&
                        board.canMove)
                    {
                        BlueJumpReady();
                        transform.SetParent(infoForBoard.transform);
                        return;
                    }
                }
            }

            transform.SetParent(null);
        }

        private void ProcessVerticalMovement(bool isGravityUp, RaycastHit2D infoHead, RaycastHit2D info)
        {
            speedWeightX = InputService.GetKey(KeyCode.X) ? SpeedWeight : 1;
            SetGravityAndRotation(isGravityUp, true);
            var moveX = GetMoveAxis(KeyCode.RightArrow, KeyCode.LeftArrow);
            moving = new Vector3(moveX, moving.y, moving.z);
            HandleJump(isGravityUp ? KeyCode.DownArrow : KeyCode.UpArrow, isGravityUp, infoHead, info, true);
        }

        private void ProcessHorizontalMovement(bool isGravityLeft, RaycastHit2D infoHead, RaycastHit2D info)
        {
            speedWeightY = InputService.GetKey(KeyCode.X) ? SpeedWeight : 1;
            SetGravityAndRotation(isGravityLeft, false);
            var moveY = GetMoveAxis(KeyCode.UpArrow, KeyCode.DownArrow);
            moving = new Vector3(moving.x, moveY, moving.z);
            HandleJump(isGravityLeft ? KeyCode.RightArrow : KeyCode.LeftArrow, isGravityLeft, infoHead, info, false);
        }

        private void SetGravityAndRotation(bool isGravityPositive, bool isVertical)
        {
            if (isVertical)
            {
                transform.rotation = Quaternion.Euler(0, 0, isGravityPositive ? 180 : 0);
                Physics2D.gravity = new Vector2(0, isGravityPositive ? 9.8f : -9.8f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, isGravityPositive ? 270 : 90);
                Physics2D.gravity = new Vector2(isGravityPositive ? -9.8f : 9.8f, 0);
            }
        }

        private static float GetMoveAxis(KeyCode positiveKey, KeyCode negativeKey)
        {
            if (InputService.GetKey(positiveKey) && InputService.GetKey(negativeKey))
            {
                return 0;
            }

            if (InputService.GetKey(positiveKey))
            {
                return 1;
            }

            if (InputService.GetKey(negativeKey))
            {
                return -1;
            }

            return 0;
        }

        private void HandleJump(KeyCode jumpKey,
            bool isGravityPositive,
            RaycastHit2D infoHead,
            RaycastHit2D info,
            bool isVertical)
        {
            UpdateJumpRayDistance(jumpKey);
            TryStartJump(jumpKey, isGravityPositive, isVertical);
            TryStopJump(jumpKey, infoHead, isGravityPositive, isVertical);
            ApplyJumpAcceleration(isGravityPositive, isVertical);
            CheckJumpCollision(info);
            ResetJump(isVertical);
            HandleDown(jumpKey, info);
        }

        private void HandleDown(KeyCode jumpKey, RaycastHit2D info)
        {
            if (!info.collider && !isJump && !InputService.GetKey(jumpKey))
            {
                BlueDown(0, playerDir);
            }
        }

        private void UpdateJumpRayDistance(KeyCode jumpKey)
        {
            if (!InputService.GetKey(jumpKey))
            {
                jumpRayDistanceForBoard =
                    InputService.GetKey(KeyCode.RightArrow) || InputService.GetKey(KeyCode.LeftArrow) ? 0 : 0.2f;
            }
        }

        private void TryStartJump(KeyCode jumpKey, bool isGravityPositive, bool isVertical)
        {
            if (!InputService.GetKey(jumpKey) || isJump || !Mathf.Approximately(isVertical ? moving.y : moving.x, 0))
            {
                return;
            }

            if (isVertical)
            {
                moving.y = isGravityPositive ? -2.15f : 2.15f;
            }
            else
            {
                moving.x = isGravityPositive ? 2.15f : -2.15f;
            }

            isJump = true;
            jumpRayDistance = 0.2f;
            jumpRayDistanceForBoard = 0;
        }

        private void TryStopJump(KeyCode jumpKey, RaycastHit2D infoHead, bool isGravityPositive, bool isVertical)
        {
            if (!isJump || (InputService.GetKey(jumpKey) &&
                            (!infoHead.collider || !infoHead.collider.gameObject.CompareTag("Box"))) ||
                ((!isGravityPositive || (isVertical ? moving.y >= 0 : moving.x <= 0)) &&
                 (isGravityPositive || (isVertical ? moving.y <= 0 : moving.x >= 0))) ||
                !infoHead.collider || !Mathf.Approximately(infoHead.transform.position.z, transform.position.z))
            {
                return;
            }

            jumpRayDistanceForBoard = 0.2f;
            if (isVertical)
            {
                moving.y = 0;
            }
            else
            {
                moving.x = 0;
            }
        }

        private void ApplyJumpAcceleration(bool isGravityPositive, bool isVertical)
        {
            if (!isJump)
            {
                return;
            }

            var delta = Time.deltaTime * (float)Math.Pow(3, jumpAcceleration);
            if (isVertical)
            {
                moving.y += isGravityPositive ? delta : -delta;
            }
            else
            {
                moving.x += isGravityPositive ? -delta : delta;
            }
        }

        private void CheckJumpCollision(RaycastHit2D info)
        {
            if (!isJump || !info.collider || !Mathf.Approximately(info.transform.position.z, transform.position.z))
            {
                return;
            }

            var obj = info.collider.gameObject;
            if (obj.transform.CompareTag("Box"))
            {
                BlueJumpReady();
            }
        }

        private void ResetJump(bool isVertical)
        {
            if (!isJump)
            {
                jumpAcceleration = 1.25f;
                if (isVertical)
                {
                    moving.y = 0;
                }
                else
                {
                    moving.x = 0;
                }
            }

            jumpAcceleration += Time.deltaTime * timeInterpolation;
        }


        private void PlayerMoveWithLine()
        {
            var playerLineController = MainControl.Instance.selectUIController.PlayerLineController;
            _currentLineIndex = Mathf.Clamp(_currentLineIndex, 0, playerLineController.lines.Count - 1);

            var lineRenderer = playerLineController.lines[_currentLineIndex];
            var weight = InputService.GetKey(KeyCode.X) ? SpeedWeight : 1;
            speedWeightX = weight;
            speedWeightY = weight;

            var playerPos = transform.position;
            var tangent = ComputeTangentAndProjection(lineRenderer, playerPos, out var projection);

            const float tolerance = 0.01f;
            if (Vector3.Distance(playerPos, projection) > tolerance)
            {
                transform.position = Vector3.Lerp(playerPos, projection, 0.5f);
            }

            var horizontal = 0;
            if (InputService.GetKey(KeyCode.RightArrow) && !InputService.GetKey(KeyCode.LeftArrow))
            {
                horizontal = 1;
            }
            else if (InputService.GetKey(KeyCode.LeftArrow) && !InputService.GetKey(KeyCode.RightArrow))
            {
                horizontal = -1;
            }

            if (horizontal != 0)
            {
                moving = horizontal * tangent;
            }
            else
            {
                moving = Vector3.zero;
            }
        }

        /// <summary>
        ///     根据玩家当前所在的位置，在LineRenderer构成的路径上找到最近的线段，
        ///     返回该段的归一化切线方向，并通过out参数返回在该线段上的投影点。
        /// </summary>
        private static Vector3 ComputeTangentAndProjection(LineRenderer lineRenderer,
            Vector3 playerPos,
            out Vector3 projectionResult)
        {
            var posCount = lineRenderer.positionCount;
            if (posCount < 2)
            {
                projectionResult = playerPos;
                return Vector3.right;
            }

            var closestTangent = Vector3.zero;
            var closestDistance = float.MaxValue;
            var closestProjection = playerPos;

            for (var i = 0; i < posCount - 1; i++)
            {
                var p0 = lineRenderer.GetPosition(i);
                var p1 = lineRenderer.GetPosition(i + 1);
                if (!lineRenderer.useWorldSpace)
                {
                    p0 += lineRenderer.transform.position;
                    p1 += lineRenderer.transform.position;
                }
                var segment = p1 - p0;
                var segSqrLen = segment.sqrMagnitude;
                if (Mathf.Approximately(segSqrLen, 0))
                {
                    continue;
                }

                var t = Vector3.Dot(playerPos - p0, segment) / segSqrLen;
                t = Mathf.Clamp01(t);
                var projection = p0 + t * segment;
                var distance = (playerPos - projection).sqrMagnitude;

                if (distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = distance;
                closestTangent = segment.normalized;
                closestProjection = projection;
            }

            projectionResult = closestProjection;
            return closestTangent;
        }


        private void PlayerContinuouslyMove()
        {
            UpdateSpeedWeight();
            UpdateMovementDirection();
        }

        private void UpdateSpeedWeight()
        {
            var isBoosting = InputService.GetKey(KeyCode.X);
            speedWeightX = isBoosting ? SpeedWeight : 1;
            speedWeightY = isBoosting ? SpeedWeight : 1;
        }

        private void UpdateMovementDirection()
        {
            Dictionary<(bool, bool, bool, bool), float> directionMap = new()
            {
                { (true, false, true, false), 45 },
                { (true, false, false, true), -45 },
                { (false, true, true, false), 135 },
                { (false, true, false, true), -135 },
                { (true, false, false, false), 0 },
                { (false, true, false, false), 180 },
                { (false, false, false, true), -90 },
                { (false, false, true, false), 90 }
            };

            var up = InputService.GetKey(KeyCode.UpArrow);
            var down = InputService.GetKey(KeyCode.DownArrow);
            var left = InputService.GetKey(KeyCode.LeftArrow);
            var right = InputService.GetKey(KeyCode.RightArrow);

            if (directionMap.TryGetValue((up, down, left, right), out var newAngle))
            {
                angle = newAngle;
            }

            moving = Quaternion.Euler(0, 0, angle) * transform.up;
        }

        private void BlueJumpReady()
        {
            jumpRayDistance = 0;
            if (isJump)
            {
                isJump = false;
            }
        }

        /// <summary>
        ///     通过渐变动画将玩家的颜色改变。
        ///     若inputGradientTime/inputDingTime等于0 则不会有渐变动画/ding动画；
        ///     若inputGradientTime/inputDingTime小于0 则使用该脚本内的gradientTime/dingTime变量。
        ///     若PlayerColor输入为nullColor，则不会更改玩家的实际颜色属性。
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void ChangePlayerColor(Color aimColor,
            BattleControl.PlayerColor aimPlayerColor,
            float startForce = 0,
            PlayerDirection dir = PlayerDirection.NullDir,
            float inputGradientTime = -1,
            float inputDingTime = -1,
            int fx = 2)
        {
            AudioController.Instance.PlayFx(fx, MainControl.Instance.AudioControl.fxClipBattle);

            if (inputGradientTime < 0)
            {
                inputGradientTime = gradientTime;
            }

            if (inputDingTime < 0)
            {
                inputDingTime = dingTime;
            }


            if (inputGradientTime <= 0)
            {
                _spriteRenderer.color = aimColor;
                if (inputDingTime > 0)
                {
                    _changeDingColor.Kill(true);
                    _changeDingScale.Kill(true);

                    _dingSpriteRenderer.transform.localScale = Vector3.one;
                    _dingSpriteRenderer.color = aimColor;
                    _changeDingColor = _dingSpriteRenderer
                        .DOColor(_dingSpriteRenderer.color * ColorEx.WhiteClear, inputDingTime)
                        .SetEase(Ease.InOutSine);
                    _changeDingScale = _dingSpriteRenderer.transform.DOScale(Vector3.one * 2.5f, inputDingTime)
                        .SetEase(Ease.InOutSine);
                }
            }
            else
            {
                _changeColor.Kill(true);
                _changeColor = _spriteRenderer.DOColor(aimColor, inputGradientTime).SetEase(Ease.InOutSine);
                if (inputDingTime > 0)
                {
                    _changeDingColor.Kill(true);
                    _changeDingScale.Kill(true);

                    _dingSpriteRenderer.transform.localScale = Vector3.one;
                    _dingSpriteRenderer.color += Color.black;
                    _changeDingColor = _dingSpriteRenderer.DOColor(aimColor * ColorEx.WhiteClear, inputDingTime)
                        .SetEase(Ease.InOutSine);
                    _changeDingScale = _dingSpriteRenderer.transform.DOScale(Vector3.one * 2.5f, inputDingTime)
                        .SetEase(Ease.InOutSine);
                }
            }

            if (playerColor != aimPlayerColor)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
                transform.SetParent(null);
                playerColor = aimPlayerColor;
                moving = Vector3.zero;
                isJump = false;
                jumpAcceleration = 1.25f;
            }

            if (aimPlayerColor == BattleControl.PlayerColor.Blue)
            {
                transform.SetParent(null);
                BlueDown(startForce, dir);
            }

            if (aimPlayerColor != BattleControl.PlayerColor.Orange)
            {
                orangeDash.Stop();
            }
            else
            {
                orangeDash.Play();
            }
        }

        /// <summary>
        ///     让蓝心坠落
        /// </summary>
        private void BlueDown(float startForce = 0, PlayerDirection dir = PlayerDirection.NullDir)
        {
            if (dir != PlayerDirection.NullDir)
            {
                playerDir = dir;
            }

            jumpRayDistance = 0.2f;
            isJump = true;
            moving = playerDir switch
            {
                PlayerDirection.Up => new Vector3(moving.x, startForce),
                PlayerDirection.Down => new Vector3(moving.x, -startForce),
                PlayerDirection.Left => new Vector3(-startForce, moving.y),
                PlayerDirection.Right => new Vector3(startForce, moving.y),
                _ => moving
            };
        }

        /// <summary>
        ///     根据位移检查并调整点位置的方法
        /// </summary>
        private static Vector3 CheckPoint(Vector3 point,
            float inputDisplacement,
            int maxDepth = 10,
            int currentDepth = 0,
            bool isInitialCall = true)
        {
            Vector2 originalPoint = point;
            var z = point.z;
            if (currentDepth >= maxDepth)
            {
                return point;
            }

            foreach (var box in BoxController.Instance.boxes.Where(box => Mathf.Approximately(box.localPosition.z, z)))
            {
                float rDis;
                if (box.sonBoxDrawer.Count > 0)
                {
                    rDis = inputDisplacement + -0.5f;
                }
                else
                {
                    rDis = inputDisplacement;
                }

                var movedVertices =
                    MathUtilityService.CalculateInwardOffset(box.GetRealPoints(false), -rDis); //计算缩放后的多边形顶点

                if (MathUtilityService.IsPointInPolygon(point, movedVertices))
                {
                    return point;
                }
            }


            if (MathUtilityService.CheckPointBeyondPolygon(point, z, out var nearestPoint, out var lineStart,
                    out var lineEnd,
                    out var isParent, out var checkPoint))
            {
                return checkPoint;
            }

            if (isParent)
            {
                inputDisplacement -= 0.05f;
            }

            var moved = (Vector3)MathUtilityService.CalculateDisplacedPoint(nearestPoint, lineStart, lineEnd,
                            -inputDisplacement) +
                        new Vector3(0, 0, z);

            if (!isInitialCall && (Vector2)moved == originalPoint)
            {
                return point;
            }

            var newCheck = (Vector3)(Vector2)CheckPoint(moved, inputDisplacement, maxDepth, currentDepth + 1, false) +
                           new Vector3(0, 0, z);
            return newCheck != moved
                ? newCheck
                : moved;
        }

        public void KillPlayer(MainControl mainControl)
        {
            mainControl.playerControl.hp = mainControl.playerControl.hpMax;

            if (!(mainControl.playerControl.isDebug && mainControl.playerControl.invincible))
            {
                mainControl.playerControl.playerLastPosInBattle = transform.position - (Vector3)sceneDrift;
                SettingsStorage.Pause = true;
                Timing.KillCoroutines();
                GameUtilityService.SwitchScene("GameOver", false);
            }
            else
            {
                mainControl.selectUIController.UITextUpdate(SelectUIController.UITextMode.Hit);
                Debug.Log("Debug无敌模式已将血量恢复", "#FF0000");
            }
        }

        private void PlayerCommonMove()
        {
            var weight = InputService.GetKey(KeyCode.X) ? SpeedWeight : 1;
            speedWeightX = weight;
            speedWeightY = weight;

            var up = InputService.GetKey(KeyCode.UpArrow);
            var down = InputService.GetKey(KeyCode.DownArrow);
            var vertical = 0;
            if (up && !down)
            {
                vertical = 1;
            }
            else if (down && !up)
            {
                vertical = -1;
            }

            var right = InputService.GetKey(KeyCode.RightArrow);
            var left = InputService.GetKey(KeyCode.LeftArrow);
            var horizontal = 0;
            if (right && !left)
            {
                horizontal = 1;
            }
            else if (left && !right)
            {
                horizontal = -1;
            }

            moving = new Vector3(horizontal, vertical);
        }
    }
}