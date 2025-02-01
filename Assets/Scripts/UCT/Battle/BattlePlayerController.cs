using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UCT.Control;
using UCT.Extensions;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.Settings;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Battle
{
    /// <summary>
    ///     控制战斗内玩家(心)的相关属性
    /// </summary>
    public class BattlePlayerController : MonoBehaviour
    {
        public enum PlayerDirEnum
        {
            Up,
            Down,
            Left,
            Right,
            NullDir
        }

        private const float SpeedWeight = 0.5f;

        [Header("心变色时的ding动画速度，0为关")] public float dingTime;

        [Header("心渐变动画速度，0为关")] public float gradientTime;

        [Header("基本属性调整")] public float speed;

        public float speedWeightX, speedWeightY; //速度与权重(按X乘以倍数)，速度测试为3，权重0.5f
        [FormerlySerializedAs("hitCD")] public float hitCd; //无敌时间
        [FormerlySerializedAs("hitCDMax")] public float hitCdMax; //无敌时间
        public float displacement = 0.175f; //碰撞距离判定
        public bool isMoving; //用于蓝橙骨判断：玩家是否真的在移动
        public float timeInterpolation = -0.225f;
        public Vector2 sceneDrift = new(-1000, 0);

        public PlayerDirEnum playerDir; //方向
        public Vector3 moving;
        public bool isJump; //是否处于“跳起”状态
        public float jumpAcceleration; //跳跃的加速度
        public float jumpRayDistance; //射线距离
        public float jumpRayDistanceForBoard;
        public CircleCollider2D collideCollider; //圆形碰撞体
        public BattleControl.PlayerColor playerColor; //含有属性的颜色 读取BattleControl中的enum PlayerColor.颜色变换通过具体变换的函数来执行

        public UnityEngine.Rendering.Volume hitVolume;
        private Tween _missAnim, _changeColor, _changeDingColor, _changeDingScale;

        private Rigidbody2D _rigidBody;
        private SpriteRenderer _spriteRenderer, _dingSpriteRenderer;

        //LayerMask mask;
        private void Start()
        {
            speedWeightX = 1;
            speedWeightY = 1;
            jumpRayDistance = 0.2f;
            jumpRayDistanceForBoard = 0.2f;
            jumpAcceleration = 1.25f;
            playerColor = BattleControl.PlayerColor.Red;
            playerDir = PlayerDirEnum.Down;
            _rigidBody = GetComponent<Rigidbody2D>();
            collideCollider = GetComponent<CircleCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _dingSpriteRenderer = transform.Find("Ding").GetComponent<SpriteRenderer>();
            _dingSpriteRenderer.color = Color.clear;
            hitVolume = GetComponent<UnityEngine.Rendering.Volume>();
            hitVolume.weight = 0;
            //mask = 1 << 6;
            MainControl.Instance.playerControl.missTime = 0;
        }

        private void Update()
        {
            if (!SettingsStorage.isSimplifySfx && hitVolume.weight > 0)
                hitVolume.weight -= Time.deltaTime;

            if (MainControl.Instance.playerControl.hp <= 0)
            {
                MainControl.Instance.playerControl.hp = MainControl.Instance.playerControl.hpMax;

                if (!(MainControl.Instance.playerControl.isDebug && MainControl.Instance.playerControl.invincible))
                {
                    _spriteRenderer.color = Color.red;
                    MainControl.Instance.playerControl.playerLastPos = transform.position - (Vector3)sceneDrift;
                    SettingsStorage.pause = true;
                    TurnController.Instance.KillIEnumerator();
                    GameUtilityService.SwitchScene("GameOver", false);
                }
                else
                {
                    MainControl.Instance.selectUIController.UITextUpdate(SelectUIController.UITextMode.Hit);
                }
            }

            if (GameUtilityService.IsGamePausedOrSetting()) return;

            if (MainControl.Instance.playerControl.missTime >= 0)
            {
                MainControl.Instance.playerControl.missTime -= Time.deltaTime;
                if (_missAnim == null && MainControl.Instance.playerControl.missTimeMax >= 0.4f)
                    _missAnim = _spriteRenderer
                        .DOColor(MainControl.Instance.BattleControl.playerMissColorList[(int)playerColor], 0.2f)
                        .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
            else
            {
                if (_missAnim != null)
                {
                    _missAnim.Kill();
                    _missAnim = null;
                    _spriteRenderer.color = MainControl.Instance.BattleControl.playerColorList[(int)playerColor];
                }
            }

            //Debug
            if (MainControl.Instance.playerControl.isDebug)
            {
                if (Input.GetKeyDown(KeyCode.Keypad1))
                    ChangePlayerColor(MainControl.Instance.BattleControl.playerColorList[0], 0);
                else if (Input.GetKeyDown(KeyCode.Keypad2))
                    ChangePlayerColor(MainControl.Instance.BattleControl.playerColorList[1],
                        (BattleControl.PlayerColor)1);
                else if (Input.GetKeyDown(KeyCode.Keypad6))
                    ChangePlayerColor(MainControl.Instance.BattleControl.playerColorList[5],
                        (BattleControl.PlayerColor)5);

                if (Input.GetKeyDown(KeyCode.I))
                    ChangePlayerColor(MainControl.Instance.BattleControl.playerColorList[5],
                        (BattleControl.PlayerColor)5, 2.5f, 0);
                else if (Input.GetKeyDown(KeyCode.K))
                    ChangePlayerColor(MainControl.Instance.BattleControl.playerColorList[5],
                        (BattleControl.PlayerColor)5, 2.5f, (PlayerDirEnum)1);
                else if (Input.GetKeyDown(KeyCode.J))
                    ChangePlayerColor(MainControl.Instance.BattleControl.playerColorList[5],
                        (BattleControl.PlayerColor)5, 2.5f, (PlayerDirEnum)2);
                else if (Input.GetKeyDown(KeyCode.L))
                    ChangePlayerColor(MainControl.Instance.BattleControl.playerColorList[5],
                        (BattleControl.PlayerColor)5, 2.5f, (PlayerDirEnum)3);

                if (Input.GetKeyDown(KeyCode.P))
                    MainControl.Instance.playerControl.hp = 0;
            }
        }

        private void FixedUpdate()
        {
            if (GameUtilityService.IsGamePausedOrSetting())
                return;
            if (!TurnController.Instance.isMyTurn)
                Moving();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            //蓝心碰板子确保再次可以跳
            if (collision.transform.CompareTag("board") &&
                collision.transform.GetComponent<EdgeCollider2D>().IsTouching(collideCollider) &&
                playerColor == BattleControl.PlayerColor.Blue) BlueJumpReady();
        }


        /// <summary>
        ///     掉出
        /// </summary>
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.transform.CompareTag("board") && playerColor == BattleControl.PlayerColor.Blue &&
                !isJump) BlueDown();
        }

        private void Moving()
        {
            Vector2 dirReal = new();
            switch (playerDir)
            {
                case PlayerDirEnum.Up:
                    dirReal = Vector2.up;
                    break;

                case PlayerDirEnum.Down:
                    dirReal = Vector2.down;
                    break;

                case PlayerDirEnum.Left:
                    dirReal = Vector2.left;
                    break;

                case PlayerDirEnum.Right:
                    dirReal = Vector2.right;
                    break;
            }

            Ray2D ray = new(transform.position, dirReal);
            Other.Debug.DrawRay(ray.origin, ray.direction, Color.blue);
            var info = Physics2D.Raycast(transform.position, dirReal, jumpRayDistance);

            Ray2D rayF = new(transform.position, dirReal * -1);
            Other.Debug.DrawRay(rayF.origin, rayF.direction, Color.red);
            var infoF = Physics2D.Raycast(transform.position, dirReal * -1, jumpRayDistance); //反向检测(顶头)

            //------------------------移动------------------------
            switch (playerColor)
            {
                case BattleControl.PlayerColor.Red:
                    if (InputService.GetKey(KeyCode.X))
                    {
                        speedWeightX = SpeedWeight;
                        speedWeightY = SpeedWeight;
                    }
                    else
                    {
                        speedWeightX = 1;
                        speedWeightY = 1;
                    }

                    if (InputService.GetKey(KeyCode.UpArrow))
                        moving = new Vector3(moving.x, 1);
                    else if (InputService.GetKey(KeyCode.DownArrow))
                        moving = new Vector3(moving.x, -1);
                    else
                        moving = new Vector3(moving.x, 0);

                    if (InputService.GetKey(KeyCode.UpArrow) &&
                        InputService.GetKey(KeyCode.DownArrow))
                        moving = new Vector3(moving.x, 0);

                    if (InputService.GetKey(KeyCode.RightArrow))
                        moving = new Vector3(1, moving.y);
                    else if (InputService.GetKey(KeyCode.LeftArrow))
                        moving = new Vector3(-1, moving.y);
                    else
                        moving = new Vector3(0, moving.y);

                    if (InputService.GetKey(KeyCode.RightArrow) &&
                        InputService.GetKey(KeyCode.LeftArrow))
                        moving = new Vector3(0, moving.y);
                    break;

                case BattleControl.PlayerColor.Orange:
                    break;

                case BattleControl.PlayerColor.Yellow:
                    break;

                case BattleControl.PlayerColor.Green:
                    break;

                case BattleControl.PlayerColor.Cyan:
                    break;

                case BattleControl.PlayerColor.Blue:
                    var infoForBoard = Physics2D.Raycast(transform.position, dirReal, jumpRayDistanceForBoard);
                    if (infoForBoard.collider != null)
                    {
                        var obj = infoForBoard.collider.gameObject;
                        if (obj.transform.CompareTag(tag))
                            if (!isJump && moving == Vector3.zero)
                                BlueDown(0, playerDir);

                        if (obj.transform.CompareTag("board"))
                        {
                            var board = obj.transform.GetComponent<BoardController>();
                            if (!infoForBoard.collider.isTrigger && infoForBoard.collider is EdgeCollider2D &&
                                board.canMove)
                            {
                                BlueJumpReady();
                                transform.SetParent(infoForBoard.transform);
                            }
                        }
                        else
                        {
                            transform.SetParent(null);
                        }
                    }
                    else
                    {
                        transform.SetParent(null);
                    }

                    switch (playerDir)
                    {
                        case PlayerDirEnum.Up:
                            if (InputService.GetKey(KeyCode.X))
                                speedWeightX = SpeedWeight;
                            else
                                speedWeightX = 1;

                            transform.rotation = Quaternion.Euler(0, 0, 180);
                            Physics2D.gravity = new Vector2(0, 9.8f);
                            if (InputService.GetKey(KeyCode.RightArrow))
                                moving = new Vector3(1, moving.y);
                            else if (InputService.GetKey(KeyCode.LeftArrow))
                                moving = new Vector3(-1, moving.y);
                            else
                                moving = new Vector3(0, moving.y);

                            if (InputService.GetKey(KeyCode.RightArrow) &&
                                InputService.GetKey(KeyCode.LeftArrow))
                                moving = new Vector3(0, moving.y);
                            if (!InputService.GetKey(KeyCode.DownArrow))
                            {
                                if (!InputService.GetKey(KeyCode.RightArrow) ||
                                    !InputService.GetKey(KeyCode.LeftArrow))
                                    jumpRayDistanceForBoard = 0.2f;
                                if (InputService.GetKey(KeyCode.RightArrow) ||
                                    InputService.GetKey(KeyCode.LeftArrow))
                                    jumpRayDistanceForBoard = 0;
                            }

                            if (InputService.GetKey(KeyCode.DownArrow) && !isJump && moving.y == 0)
                            {
                                moving = new Vector3(moving.x, -2.15f);
                                isJump = true;
                                jumpRayDistance = 0.2f;
                                jumpRayDistanceForBoard = 0;
                            }

                            if (isJump && (!InputService.GetKey(KeyCode.DownArrow) ||
                                           (infoF.collider && infoF.collider.gameObject.CompareTag("Box"))) &&
                                moving.y < -0)
                                if (infoF.collider &&
                                    Mathf.Approximately(infoF.transform.position.z, transform.position.z))
                                {
                                    jumpRayDistanceForBoard = 0.2f;
                                    moving = new Vector3(moving.x, -0);
                                }

                            if (isJump)
                            {
                                if (info.collider &&
                                    Mathf.Approximately(info.transform.position.z, transform.position.z))
                                {
                                    var obj = info.collider.gameObject;
                                    if (obj.transform.CompareTag("Box")) BlueJumpReady();
                                }

                                moving.y += Time.deltaTime * (float)Math.Pow(3, jumpAcceleration);
                            }
                            else
                            {
                                jumpAcceleration = 1.25f;
                                moving.y = 0;
                            }

                            jumpAcceleration += Time.deltaTime * timeInterpolation;
                            break;

                        case PlayerDirEnum.Down: ////////////////////////////////////////////////
                            if (InputService.GetKey(KeyCode.X))
                                speedWeightX = SpeedWeight;
                            else
                                speedWeightX = 1;
                            transform.rotation = Quaternion.Euler(0, 0, 0);
                            Physics2D.gravity = new Vector2(0, -9.8f);
                            if (InputService.GetKey(KeyCode.RightArrow))
                                moving = new Vector3(1, moving.y);
                            else if (InputService.GetKey(KeyCode.LeftArrow))
                                moving = new Vector3(-1, moving.y);
                            else
                                moving = new Vector3(0, moving.y);
                            if (InputService.GetKey(KeyCode.RightArrow) &&
                                InputService.GetKey(KeyCode.LeftArrow))
                                moving = new Vector3(0, moving.y);

                            if (!InputService.GetKey(KeyCode.UpArrow))
                            {
                                if (!InputService.GetKey(KeyCode.RightArrow) ||
                                    !InputService.GetKey(KeyCode.LeftArrow))
                                    jumpRayDistanceForBoard = 0.2f;
                                if (InputService.GetKey(KeyCode.RightArrow) ||
                                    InputService.GetKey(KeyCode.LeftArrow))
                                    jumpRayDistanceForBoard = 0;
                            }


                            if (InputService.GetKey(KeyCode.UpArrow) && !isJump && moving.y == 0)
                            {
                                moving = new Vector3(moving.x, 2.15f);
                                isJump = true;
                                jumpRayDistance = 0.2f;
                                jumpRayDistanceForBoard = 0;
                            }

                            if (isJump && (!InputService.GetKey(KeyCode.UpArrow) ||
                                           (infoF.collider != null && infoF.collider.gameObject.CompareTag("Box"))) &&
                                moving.y > 0)
                                if (infoF.collider &&
                                    Mathf.Approximately(infoF.transform.position.z, transform.position.z))
                                {
                                    jumpRayDistanceForBoard = 0.2f;
                                    moving = new Vector3(moving.x, 0);
                                }

                            if (isJump)
                            {
                                if (info.collider &&
                                    Mathf.Approximately(info.transform.position.z, transform.position.z))
                                {
                                    var obj = info.collider.gameObject;
                                    if (obj.transform.CompareTag("Box")) BlueJumpReady();
                                }

                                moving.y -= Time.deltaTime * (float)Math.Pow(3, jumpAcceleration);
                            }
                            else
                            {
                                jumpAcceleration = 1.25f;
                                moving.y = 0;
                            }

                            jumpAcceleration += Time.deltaTime * timeInterpolation;
                            break;

                        case PlayerDirEnum.Left: ////////////////////////////////////////////////
                            if (InputService.GetKey(KeyCode.X))
                                speedWeightY = SpeedWeight;
                            else
                                speedWeightY = 1;
                            transform.rotation = Quaternion.Euler(0, 0, 270);
                            Physics2D.gravity = new Vector2(-9.8f, 0);
                            if (InputService.GetKey(KeyCode.UpArrow))
                                moving = new Vector3(moving.x, 1);
                            else if (InputService.GetKey(KeyCode.DownArrow))
                                moving = new Vector3(moving.x, -1);
                            else
                                moving = new Vector3(moving.x, 0);
                            if (InputService.GetKey(KeyCode.UpArrow) &&
                                InputService.GetKey(KeyCode.DownArrow))
                                moving = new Vector3(moving.x, 0);
                            if (!InputService.GetKey(KeyCode.RightArrow))
                            {
                                if (!InputService.GetKey(KeyCode.UpArrow) ||
                                    !InputService.GetKey(KeyCode.DownArrow))
                                    jumpRayDistanceForBoard = 0.2f;
                                if (InputService.GetKey(KeyCode.UpArrow) ||
                                    InputService.GetKey(KeyCode.DownArrow))
                                    jumpRayDistanceForBoard = 0;
                            }

                            if (InputService.GetKey(KeyCode.RightArrow) && !isJump && moving.x == 0)
                            {
                                moving = new Vector3(2.15f, moving.y);
                                isJump = true;
                                jumpRayDistance = 0.2f;
                                jumpRayDistanceForBoard = 0;
                            }

                            if (isJump && (!InputService.GetKey(KeyCode.RightArrow) ||
                                           (infoF.collider && infoF.collider.gameObject.CompareTag("Box"))) &&
                                moving.x > 0)
                                if (infoF.collider &&
                                    Mathf.Approximately(infoF.transform.position.z, transform.position.z))
                                {
                                    jumpRayDistanceForBoard = 0.2f;
                                    moving = new Vector3(0, moving.y);
                                }

                            if (isJump)
                            {
                                if (info.collider &&
                                    Mathf.Approximately(info.transform.position.z, transform.position.z))
                                {
                                    var obj = info.collider.gameObject;
                                    if (obj.transform.CompareTag("Box")) BlueJumpReady();
                                }

                                moving.x -= Time.deltaTime * (float)Math.Pow(3, jumpAcceleration);
                            }
                            else
                            {
                                jumpAcceleration = 1.25f;
                                moving.x = 0;
                            }

                            jumpAcceleration += Time.deltaTime * timeInterpolation;
                            break;

                        case PlayerDirEnum.Right:
                            if (InputService.GetKey(KeyCode.X))
                                speedWeightY = SpeedWeight;
                            else
                                speedWeightY = 1;
                            transform.rotation = Quaternion.Euler(0, 0, 90);
                            Physics2D.gravity = new Vector2(9.8f, 0);
                            if (InputService.GetKey(KeyCode.UpArrow))
                                moving = new Vector3(moving.x, 1);
                            else if (InputService.GetKey(KeyCode.DownArrow))
                                moving = new Vector3(moving.x, -1);
                            else
                                moving = new Vector3(moving.x, 0);
                            if (InputService.GetKey(KeyCode.UpArrow) &&
                                InputService.GetKey(KeyCode.DownArrow))
                                moving = new Vector3(moving.x, 0);
                            if (!InputService.GetKey(KeyCode.LeftArrow))
                            {
                                if (!InputService.GetKey(KeyCode.UpArrow) ||
                                    !InputService.GetKey(KeyCode.DownArrow))
                                    jumpRayDistanceForBoard = 0.2f;
                                if (InputService.GetKey(KeyCode.UpArrow) ||
                                    InputService.GetKey(KeyCode.DownArrow))
                                    jumpRayDistanceForBoard = 0;
                            }

                            if (InputService.GetKey(KeyCode.LeftArrow) && !isJump && moving.x == 0)
                            {
                                moving = new Vector3(-2.15f, moving.y);
                                isJump = true;
                                jumpRayDistance = 0.2f;
                                jumpRayDistanceForBoard = 0;
                            }

                            if (isJump && (!InputService.GetKey(KeyCode.LeftArrow) ||
                                           (infoF.collider && infoF.collider.gameObject.CompareTag("Box"))) &&
                                moving.x < -0)
                                if (infoF.collider &&
                                    Mathf.Approximately(infoF.transform.position.z, transform.position.z))
                                {
                                    jumpRayDistanceForBoard = 0.2f;
                                    moving = new Vector3(-0, moving.y);
                                }

                            if (isJump)
                            {
                                if (info.collider &&
                                    Mathf.Approximately(info.transform.position.z, transform.position.z))
                                {
                                    var obj = info.collider.gameObject;
                                    if (obj.transform.CompareTag("Box")) BlueJumpReady();
                                }

                                moving.x += Time.deltaTime * (float)Math.Pow(3, jumpAcceleration);
                            }
                            else
                            {
                                jumpAcceleration = 1.25f;
                                moving.x = 0;
                            }

                            jumpAcceleration += Time.deltaTime * timeInterpolation;
                            break;
                        case PlayerDirEnum.NullDir:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;

                case BattleControl.PlayerColor.Purple:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            //蓝橙骨所用的是否移动判定
            Vector2 dirMoveX = new();
            Vector2 dirMoveY = new();
            bool isMoveX = false, isMoveY = false;
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
                isMoveY = false;

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
                isMoveX = false;

            Ray2D rayMoveX = new(transform.position, dirMoveX);
            Other.Debug.DrawRay(rayMoveX.origin, rayMoveX.direction, Color.green);
            var infoMoveX = Physics2D.Raycast(transform.position, dirMoveX, 0.2f);
            Ray2D rayMoveY = new(transform.position, dirMoveY);
            Other.Debug.DrawRay(rayMoveY.origin, rayMoveY.direction, new Color(0, 0.5f, 0, 1));
            var infoMoveY = Physics2D.Raycast(transform.position, dirMoveY, 0.2f);

            if (isMoveX && infoMoveX.collider && (infoMoveX.collider.gameObject.CompareTag("Box") ||
                                                  infoMoveX.collider.gameObject.CompareTag("board")))
                isMoving = false;

            if (isMoveX || isMoveY)
            {
                var x = (isMoveX || isMoveY) && infoMoveX.collider != null &&
                        (infoMoveX.collider.gameObject.CompareTag("Box") ||
                         infoMoveX.collider.gameObject.CompareTag("board"));
                var y = (isMoveX || isMoveY) && infoMoveY.collider != null &&
                        (infoMoveY.collider.gameObject.CompareTag("Box") ||
                         infoMoveY.collider.gameObject.CompareTag("board"));
                if (x && !y && (InputService.GetKey(KeyCode.UpArrow) ||
                                InputService.GetKey(KeyCode.DownArrow)))
                    x = y;
                if (y && !x && (InputService.GetKey(KeyCode.LeftArrow) ||
                                InputService.GetKey(KeyCode.RightArrow)))
                    y = x;

                isMoving = !(x || y);

                /*
            ////Debug.Log("X:" + x);
            ////Debug.Log("Y:" + y);
            */
            }
            else
            {
                if (playerColor == BattleControl.PlayerColor.Blue && jumpRayDistance != 0)
                    isMoving = true;
                else
                    isMoving = false;
            }

            float movingSave = 0;
            if (playerColor == BattleControl.PlayerColor.Blue && isJump)
                movingSave = playerDir switch
                {
                    PlayerDirEnum.Up => moving.y,
                    PlayerDirEnum.Down => moving.y,
                    PlayerDirEnum.Left => moving.x,
                    PlayerDirEnum.Right => moving.x,
                    _ => movingSave
                };

            moving.x = MathUtilityService.GetGreaterNumber(moving.x, -5);
            moving.y = MathUtilityService.GetGreaterNumber(moving.y, -5);
            moving.x = MathUtilityService.GetSmallerNumber(moving.x, 5);
            moving.y = MathUtilityService.GetSmallerNumber(moving.y, 5);

            var newPos = transform.position + new Vector3(speedWeightX * speed * moving.x * Time.deltaTime,
                speedWeightY * speed * moving.y * Time.deltaTime); //速度参考：3

            var checkPos = CheckPoint(newPos, displacement + BoxController.Instance.width / 2);

            if (newPos == checkPos)
                _rigidBody.MovePosition(newPos);
            else
                transform.position = checkPos;


            if (movingSave != 0)
                moving.y = movingSave;
        }

        private void BlueJumpReady()
        {
            jumpRayDistance = 0;
            if (isJump) isJump = false;
        }

        /// <summary>
        ///     通过渐变动画将玩家的颜色改变。
        ///     若inputGradientTime/inputDingTime等于0 则不会有渐变动画/ding动画；
        ///     若inputGradientTime/inputDingTime小于0 则使用该脚本内的gradientTime/dingTime变量。
        ///     若PlayerColor输入为nullColor，则不会更改玩家的实际颜色属性。
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void ChangePlayerColor(Color aimColor, BattleControl.PlayerColor aimPlayerColor, float startForce = 0,
            PlayerDirEnum dir = PlayerDirEnum.NullDir, float inputGradientTime = -1, float inputDingTime = -1,
            int fx = 2)
        {
            AudioController.Instance.GetFx(fx, MainControl.Instance.AudioControl.fxClipBattle);

            if (inputGradientTime < 0)
                inputGradientTime = gradientTime;
            if (inputDingTime < 0)
                inputDingTime = dingTime;


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
        }

        /// <summary>
        ///     让蓝心坠落
        /// </summary>
        private void BlueDown(float startForce = 0, PlayerDirEnum dir = PlayerDirEnum.NullDir)
        {
            if (dir != PlayerDirEnum.NullDir) playerDir = dir;
            jumpRayDistance = 0.2f;
            isJump = true;
            switch (playerDir)
            {
                case PlayerDirEnum.Up:
                    moving = new Vector3(moving.x, startForce);
                    break;

                case PlayerDirEnum.Down:
                    moving = new Vector3(moving.x, -startForce);
                    break;

                case PlayerDirEnum.Left:
                    moving = new Vector3(-startForce, moving.y);
                    break;

                case PlayerDirEnum.Right:
                    moving = new Vector3(startForce, moving.y);
                    break;
            }
        }

        /////////////////////////////////////////判定相关
        //定义用于判断点是否在多边形内的方法
        private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            var isInside = false; //初始化点是否在多边形内的标志为false
            //遍历多边形的每一条边，使用射线法判断点是否在多边形内
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
                //如果点与当前边的两个端点之一在Y轴的两侧，并且在X轴的左侧，则反转内部标志
                if (polygon[i].y > point.y != polygon[j].y > point.y &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) +
                    polygon[i].x)
                    isInside = !isInside;
            return isInside; //返回点是否在多边形内的最终结果
        }

        //定义计算点到线段最近点的方法（计算垂足）
        private static Vector2 GetNearestPointOnLine(Vector2 point, Vector2 start, Vector2 end)
        {
            var line = end - start; //计算线段的向量
            var len = line.magnitude; //获取线段长度
            line.Normalize(); //标准化线段向量

            var v = point - start; //计算点到线段起点的向量
            var d = Vector2.Dot(v, line); //计算点在线段向量上的投影长度
            d = Mathf.Clamp(d, 0f, len); //限制投影长度在0到线段长度之间
            return start + line * d; //计算并返回最近点的坐标
        }

        //定义计算位移后垂点位置的方法
        private static Vector2 CalculateDisplacedPoint(Vector2 nearestPoint, Vector2 lineStart, Vector2 lineEnd,
            float displacement)
        {
            var lineDirection = (lineEnd - lineStart).normalized; //计算线段方向向量
            Vector2 perpendicularDirection = new(-lineDirection.y, lineDirection.x); //计算垂直方向向量（逆时针旋转90度）

            return nearestPoint + perpendicularDirection * -displacement; //计算并返回位移后的垂点位置
        }

        //定义计算内缩多边形顶点的方法
        private List<Vector2> CalculateInwardOffset(List<Vector2> vertices, float offset)
        {
            if (vertices == null || vertices.Count < 3) return null; //如果顶点列表为空或少于3个，返回null

            List<Vector2> offsetVertices = new(); //初始化存储位移后顶点的列表
            List<Vector2> intersectionPoints = new(); //初始化存储交点的列表

            var count = vertices.Count; //获取顶点数量
            for (var i = 0; i < count; i++)
            {
                var currentVertex = vertices[i]; //获取当前顶点
                var nextVertex = vertices[(i + 1) % count]; //获取下一个顶点（环形列表）

                var edgeDirection = (nextVertex - currentVertex).normalized; //计算边的方向向量
                Vector2 perpendicularDirection = new(-edgeDirection.y, edgeDirection.x); //计算垂直方向向量

                var offsetCurrentVertex = currentVertex + perpendicularDirection * offset; //计算当前顶点的位移
                var offsetNextVertex = nextVertex + perpendicularDirection * offset; //计算下一个顶点的位移

                offsetVertices.Add(offsetCurrentVertex); //添加位移后的当前顶点到列表
                offsetVertices.Add(offsetNextVertex); //添加位移后的下一个顶点到列表

                if (i <= 0) continue; //从第二条边开始计算交点
                var foundIntersection = LineLineIntersection(out var intersection, offsetVertices[i * 2 - 2],
                    offsetVertices[i * 2 - 1], offsetCurrentVertex, offsetNextVertex);
                if (foundIntersection) intersectionPoints.Add(intersection); //如果找到交点，添加到交点列表
            }

            //计算首尾两条边的交点
            var foundFinalIntersection = LineLineIntersection(out var finalIntersection, offsetVertices[^2],
                offsetVertices[^1], offsetVertices[0], offsetVertices[1]);
            if (foundFinalIntersection) intersectionPoints.Add(finalIntersection); //如果找到交点，添加到交点列表

            return intersectionPoints; //返回交点列表，即内缩多边形的顶点
        }

        //定义线线交点计算的方法
        private static bool LineLineIntersection(out Vector2 intersection, Vector2 point1, Vector2 point2,
            Vector2 point3, Vector2 point4)
        {
            intersection = new Vector2(); //初始化交点坐标

            var d = (point1.x - point2.x) * (point3.y - point4.y) -
                    (point1.y - point2.y) * (point3.x - point4.x); //计算分母
            if (d == 0) return false; //如果分母为0，则线段平行或重合，无交点

            float pre = point1.x * point2.y - point1.y * point2.x, post = point3.x * point4.y - point3.y * point4.x;
            intersection.x = (pre * (point3.x - point4.x) - (point1.x - point2.x) * post) / d; //计算交点X坐标
            intersection.y = (pre * (point3.y - point4.y) - (point1.y - point2.y) * post) / d; //计算交点Y坐标

            return true; //返回true，表示找到交点
        }

        // 定义根据位移检查并调整点位置的方法
        private Vector3 CheckPoint(Vector3 point, float inputDisplacement, int maxDepth = 10, int currentDepth = 0,
            bool isInitialCall = true)
        {
            Vector2 originalPoint = point; //保存原始点位置
            var z = point.z;
            if (currentDepth >= maxDepth) //检查是否达到递归次数限制
                return point; //如果达到最大次数，返回当前点

            foreach (var box in BoxController.Instance.boxes.Where(box => Mathf.Approximately(box.localPosition.z, z)))
            {
                float rDis;
                if (box.sonBoxDrawer.Count > 0)
                    rDis = inputDisplacement + -0.5f;
                else
                    rDis = inputDisplacement;
                var movedVertices = CalculateInwardOffset(box.GetRealPoints(false), -rDis); //计算缩放后的多边形顶点

                if (IsPointInPolygon(point, movedVertices)) //如果点 在 调整后的多边形内
                    //Debug.Log(point, "#FF00FF"); //记录日志
                    return point; //返回原始坐标
            }
            //如果点 不在 调整后的多边形内

            var nearestPoint = Vector2.zero; //最近点
            var lineStart = Vector2.zero;
            var lineEnd = Vector2.zero;
            var nearestDistance = float.MaxValue; //最近距离设为最大值
            var isParent = false; //确定框是否为复合的框，如果是，需要额外调整移动距离

            foreach (var box in BoxController.Instance.boxes.Where(box => Mathf.Approximately(box.localPosition.z, z)))
                for (int i = 0, j = box.GetRealPoints(false).Count - 1;
                     i < box.GetRealPoints(false).Count;
                     j = i++) //遍历框的所有边
                {
                    var tempNearestPoint =
                        GetNearestPointOnLine(point, box.GetRealPoints(false)[i],
                            box.GetRealPoints(false)[j]); //计算到当前边的最近点
                    var tempDistance = Vector2.Distance(point, tempNearestPoint); //计算距离
                    if (!(tempDistance < nearestDistance)) continue;
                    //如果距离更短
                    nearestPoint = tempNearestPoint; //更新最近点
                    lineStart = box.GetRealPoints(false)[i]; //更新线段起点
                    lineEnd = box.GetRealPoints(false)[j]; //更新线段终点
                    nearestDistance = tempDistance; //更新最近距离
                    isParent = box.sonBoxDrawer.Count > 0;
                }

            if (!(nearestDistance < float.MaxValue)) return point; //如果没有找到更近的点，返回原点
            //如果找到最近点
            if (isParent)
                inputDisplacement -= 0.05f;

            var moved = (Vector3)CalculateDisplacedPoint(nearestPoint, lineStart, lineEnd, -inputDisplacement) +
                        new Vector3(0, 0, z); //计算位移后的点位置
            //Debug.Log(moved, "#FF0000"); //记录日志

            if (!isInitialCall && (Vector2)moved == originalPoint) return point; //如果没有找到更近的点，返回原点
            //如果是初次调用或移动后的点不等于原点
            var newCheck = (Vector3)(Vector2)CheckPoint(moved, inputDisplacement, maxDepth, currentDepth + 1, false) +
                           new Vector3(0, 0, z); //递归调用，增加递归深度
            return newCheck != moved
                ? //如果移动后的点未通过检测
                //因为已经在递归中处理递归深度，所以这里不需要再次调用CheckPoint
                newCheck
                : //返回新检查点
                moved; //返回移动后的点
        }

        public void KillPlayer(MainControl mainControl)
        {
            mainControl.playerControl.hp = mainControl.playerControl.hpMax;

            if (!(mainControl.playerControl.isDebug && mainControl.playerControl.invincible))
            {
                //spriteRenderer.color = Color.red;
                mainControl.playerControl.playerLastPos = transform.position - (Vector3)sceneDrift;
                SettingsStorage.pause = true;
                TurnController.Instance.KillIEnumerator();
                GameUtilityService.SwitchScene("GameOver", false);
            }
            else
            {
                mainControl.selectUIController.UITextUpdate(SelectUIController.UITextMode.Hit);
                Other.Debug.Log("Debug无敌模式已将您的血量恢复", "#FF0000");
            }
        }
    }
}