using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Rendering;

using System.Collections.Generic;
///<summary>
///控制战斗内玩家(心)的相关属性
///</summary>
public class BattlePlayerController : MonoBehaviour
{

    public float test1 = -0.5f;
    public float test2 = 0.05f;
    [Header("心变色时的ding动画速度，0为关")]
    public float dingTime;
    [Header("心渐变动画速度，0为关")]
    public float gradientTime;

    [Header("基本属性调整")]
    public float speed;
    public float speedWeightX, speedWeightY;//速度与权重(按X乘以倍数)，速度测试为3，权重0.5f
    private float speedWeight = 0.5f;
    public float hitCD, hitCDMax;//无敌时间
    public float displacement = 0.175f;//碰撞距离判定
    public bool isMoving;//用于蓝橙骨判断：玩家是否真的在移动
    public float timeInterpolation = -0.225f;
    public Vector2 sceneDrift = new Vector2(-1000, 0);
    public enum PlayerDirEnum
    {
        up,
        down,
        left,
        right,
        nullDir
    };

    public PlayerDirEnum playerDir;//方向
    public Vector3 moving;
    public bool isJump;//是否处于“跳起”状态
    public float jumpAcceleration;//跳跃的加速度
    public float jumpRayDistance;//射线距离
    public float jumpRayDistanceForBoard;

    private Rigidbody2D rigidBody;
    public CircleCollider2D collideCollider;//圆形碰撞体
    private SpriteRenderer spriteRenderer, dingSpriteRenderer;
    public BattleControl.PlayerColor playerColor;//含有属性的颜色 读取BattleControl中的enum PlayerColor.颜色变换通过具体变换的函数来执行
    private Tween missAnim, changeColor, changeDingColor, changeDingScale;

    public Volume hitVolume;

    //LayerMask mask;
    private void Start()
    {
        speedWeightX = 1;
        speedWeightY = 1;
        jumpRayDistance = 0.2f;
        jumpRayDistanceForBoard = 0.2f;
        jumpAcceleration = 1.25f;
        playerColor = BattleControl.PlayerColor.red;
        playerDir = PlayerDirEnum.down;
        rigidBody = GetComponent<Rigidbody2D>();
        collideCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dingSpriteRenderer = transform.Find("Ding").GetComponent<SpriteRenderer>();
        dingSpriteRenderer.color = Color.clear;
        hitVolume = GetComponent<Volume>();
        hitVolume.weight = 0;
        //mask = 1 << 6;
        MainControl.instance.PlayerControl.missTime = 0;

    }
    private void Update()
    {
        if (!MainControl.instance.OverworldControl.noSFX && hitVolume.weight > 0)
            hitVolume.weight -= Time.deltaTime;

        if (MainControl.instance.PlayerControl.hp <= 0)
        {
            MainControl.instance.PlayerControl.hp = MainControl.instance.PlayerControl.hpMax;

            if (!(MainControl.instance.PlayerControl.isDebug && MainControl.instance.PlayerControl.invincible))
            {
                spriteRenderer.color = Color.red;
                MainControl.instance.OverworldControl.playerDeadPos = transform.position - (Vector3)sceneDrift;
                MainControl.instance.OverworldControl.pause = true;
                TurnController.instance.KillIEnumerator();
                MainControl.instance.SwitchScene("Gameover", false);
            }
            else
                MainControl.instance.selectUIController.UITextUpdate(SelectUIController.UITextMode.Hit);
        }

        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;

        if (MainControl.instance.PlayerControl.missTime >= 0)
        {
            MainControl.instance.PlayerControl.missTime -= Time.deltaTime;
            if (missAnim == null && MainControl.instance.PlayerControl.missTimeMax >= 0.4f)
                missAnim = spriteRenderer.DOColor(MainControl.instance.BattleControl.playerMissColorList[(int)playerColor], 0.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
        else
        {
            if (missAnim != null)
            {
                missAnim.Kill();
                missAnim = null;
                spriteRenderer.color = MainControl.instance.BattleControl.playerColorList[(int)playerColor];
            }
        }

        //Debug
        if (MainControl.instance.PlayerControl.isDebug)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[0], 0);
            else if (Input.GetKeyDown(KeyCode.Keypad2))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[1], (BattleControl.PlayerColor)1);
            else if (Input.GetKeyDown(KeyCode.Keypad6))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5);

            if (Input.GetKeyDown(KeyCode.I))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 2.5f, 0);
            else if (Input.GetKeyDown(KeyCode.K))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 2.5f, (PlayerDirEnum)1);
            else if (Input.GetKeyDown(KeyCode.J))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 2.5f, (PlayerDirEnum)2);
            else if (Input.GetKeyDown(KeyCode.L))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 2.5f, (PlayerDirEnum)3);

            if (Input.GetKeyDown(KeyCode.P))
                MainControl.instance.PlayerControl.hp = 0;
        }
    }

    private void FixedUpdate()
    {
        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;
        if (!TurnController.instance.isMyTurn)
            Moving();
    }
    private void Moving()
    {
        Vector2 dirReal = new Vector2();
        switch (playerDir)
        {
            case PlayerDirEnum.up:
                dirReal = Vector2.up;
                break;

            case PlayerDirEnum.down:
                dirReal = Vector2.down;
                break;

            case PlayerDirEnum.left:
                dirReal = Vector2.left;
                break;

            case PlayerDirEnum.right:
                dirReal = Vector2.right;
                break;
        }
        Ray2D ray = new Ray2D(transform.position, dirReal);
        Debug.DrawRay(ray.origin, ray.direction, Color.blue);
        RaycastHit2D info = Physics2D.Raycast(transform.position, dirReal, jumpRayDistance);

        Ray2D rayF = new Ray2D(transform.position, dirReal * -1);
        Debug.DrawRay(rayF.origin, rayF.direction, Color.red);
        RaycastHit2D infoF = Physics2D.Raycast(transform.position, dirReal * -1, jumpRayDistance);//反向检测(顶头)

        //------------------------移动------------------------
        switch (playerColor)
        {
            case BattleControl.PlayerColor.red:
                if (MainControl.instance.KeyArrowToControl(KeyCode.X, 1))
                {
                    speedWeightX = speedWeight;
                    speedWeightY = speedWeight;
                }
                else
                {
                    speedWeightX = 1;
                    speedWeightY = 1;
                }
                if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1))
                {
                    moving = new Vector3(moving.x, 1);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                {
                    moving = new Vector3(moving.x, -1);
                }
                else
                    moving = new Vector3(moving.x, 0);

                if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                    moving = new Vector3(moving.x, 0);

                if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
                {
                    moving = new Vector3(1, moving.y);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                {
                    moving = new Vector3(-1, moving.y);
                }
                else
                    moving = new Vector3(0, moving.y);

                if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                    moving = new Vector3(0, moving.y);
                break;

            case BattleControl.PlayerColor.orange:
                break;

            case BattleControl.PlayerColor.yellow:
                break;

            case BattleControl.PlayerColor.green:
                break;

            case BattleControl.PlayerColor.cyan:
                break;

            case BattleControl.PlayerColor.blue:
                RaycastHit2D infoForBoard = Physics2D.Raycast(transform.position, dirReal, jumpRayDistanceForBoard);
                if (infoForBoard.collider != null)
                {
                    GameObject obj = infoForBoard.collider.gameObject;
                    if (obj.transform.CompareTag(tag))
                    {
                        if (!isJump && moving == Vector3.zero)
                        {
                            BlueDown(0, playerDir);
                        }
                    }

                    if (obj.transform.CompareTag("board"))
                    {
                        BoardController board = obj.transform.GetComponent<BoardController>();
                        if (!infoForBoard.collider.isTrigger && infoForBoard.collider.GetType() == typeof(EdgeCollider2D) && board.canMove)
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
                    case PlayerDirEnum.up:
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X, 1))
                        {
                            speedWeightX = speedWeight;
                        }
                        else
                        {
                            speedWeightX = 1;
                        }

                        transform.rotation = Quaternion.Euler(0, 0, 180);
                        Physics2D.gravity = new Vector2(0, 9.8f);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
                        {
                            moving = new Vector3(1, moving.y);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        {
                            moving = new Vector3(-1, moving.y);
                        }
                        else
                            moving = new Vector3(0, moving.y);

                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                            moving = new Vector3(0, moving.y);
                        if (!MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                        {
                            if (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                                jumpRayDistanceForBoard = 0.2f;
                            if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                            {
                                jumpRayDistanceForBoard = 0;
                            }
                        }

                        if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1) && !isJump && moving.y == 0)
                        {


                            moving = new Vector3(moving.x, -2.15f);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("Box"))) && moving.y < -0)
                        {
                            if (infoF.collider != null && infoF.transform.position.z == transform.position.z)
                            {
                                jumpRayDistanceForBoard = 0.2f;
                                moving = new Vector3(moving.x, -0);
                            }
                        }
                        if (isJump)
                        {
                            if (info.collider != null && info.transform.position.z == transform.position.z)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("Box"))
                                {
                                    BlueJumpReady();
                                }
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

                    case PlayerDirEnum.down:////////////////////////////////////////////////
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X, 1))
                        {
                            speedWeightX = speedWeight;
                        }
                        else
                        {
                            speedWeightX = 1;
                        }
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        Physics2D.gravity = new Vector2(0, -9.8f);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
                        {
                            moving = new Vector3(1, moving.y);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        {
                            moving = new Vector3(-1, moving.y);
                        }
                        else
                            moving = new Vector3(0, moving.y);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                            moving = new Vector3(0, moving.y);

                        if (!MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1))
                        {
                            if (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                                jumpRayDistanceForBoard = 0.2f;
                            if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                            {
                                jumpRayDistanceForBoard = 0;

                            }
                        }


                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && !isJump && moving.y == 0)
                        {
                            moving = new Vector3(moving.x, 2.15f);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("Box"))) && moving.y > 0)
                        {
                            if (infoF.collider != null && infoF.transform.position.z == transform.position.z)
                            {
                                jumpRayDistanceForBoard = 0.2f;
                                moving = new Vector3(moving.x, 0);
                            }

                        }
                        if (isJump)
                        {
                            if (info.collider != null && info.transform.position.z == transform.position.z)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("Box"))
                                {
                                    BlueJumpReady();
                                }
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

                    case PlayerDirEnum.left:////////////////////////////////////////////////
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X, 1))
                        {
                            speedWeightY = speedWeight;
                        }
                        else
                        {
                            speedWeightY = 1;
                        }
                        transform.rotation = Quaternion.Euler(0, 0, 270);
                        Physics2D.gravity = new Vector2(-9.8f, 0);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1))
                        {
                            moving = new Vector3(moving.x, 1);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                        {
                            moving = new Vector3(moving.x, -1);
                        }
                        else
                            moving = new Vector3(moving.x, 0);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            moving = new Vector3(moving.x, 0);
                        if (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
                        {
                            if (!MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                                jumpRayDistanceForBoard = 0.2f;
                            if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            {
                                jumpRayDistanceForBoard = 0;

                            }
                        }

                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && !isJump && moving.x == 0)
                        {
                            

                            moving = new Vector3(2.15f, moving.y);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("Box"))) && moving.x > 0)
                        {

                            if (infoF.collider != null && infoF.transform.position.z == transform.position.z)
                            {
                                jumpRayDistanceForBoard = 0.2f;
                                moving = new Vector3(0, moving.y);
                            }
                        }
                        if (isJump)
                        {
                            if (info.collider != null && info.transform.position.z == transform.position.z)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("Box"))
                                {
                                    BlueJumpReady();
                                }
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

                    case PlayerDirEnum.right:
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X, 1))
                        {
                            speedWeightY = speedWeight;
                        }
                        else
                        {
                            speedWeightY = 1;
                        }
                        transform.rotation = Quaternion.Euler(0, 0, 90);
                        Physics2D.gravity = new Vector2(9.8f, 0);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1))
                        {
                            moving = new Vector3(moving.x, 1);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                        {
                            moving = new Vector3(moving.x, -1);
                        }
                        else
                            moving = new Vector3(moving.x, 0);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            moving = new Vector3(moving.x, 0);
                        if (!MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        {
                            if (!MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                                jumpRayDistanceForBoard = 0.2f;
                            if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            {
                                jumpRayDistanceForBoard = 0;

                            }

                        }

                        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) && !isJump && moving.x == 0)
                        {
                            

                            moving = new Vector3(-2.15f, moving.y);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("Box"))) && moving.x < -0)
                        {
                            if (infoF.collider != null && infoF.transform.position.z == transform.position.z)
                            {
                                jumpRayDistanceForBoard = 0.2f;
                                moving = new Vector3(-0, moving.y);
                            }
                        }
                        if (isJump)
                        {
                            if (info.collider != null && info.transform.position.z == transform.position.z)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("Box"))
                                {
                                    BlueJumpReady();
                                }
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
                }
                break;

            case BattleControl.PlayerColor.purple:
                break;

            default:
                break;
        }


        //蓝橙骨所用的是否移动判定
        Vector2 dirMoveX = new Vector2();
        Vector2 dirMoveY = new Vector2();
        bool isMoveX = false, isMoveY = false;
        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1))
        {
            dirMoveY = Vector2.up;
            isMoveY = true;
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
        {
            dirMoveY = Vector2.down;
            isMoveY = true;
        }

        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
            isMoveY = false;

        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
        {
            dirMoveX = Vector2.left;
            isMoveX = true;
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
        {
            dirMoveX = Vector2.right;
            isMoveX = true;
        }

        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
            isMoveX = false;

        Ray2D rayMoveX = new Ray2D(transform.position, dirMoveX);
        Debug.DrawRay(rayMoveX.origin, rayMoveX.direction, Color.green);
        RaycastHit2D infoMoveX = Physics2D.Raycast(transform.position, dirMoveX, 0.2f);
        Ray2D rayMoveY = new Ray2D(transform.position, dirMoveY);
        Debug.DrawRay(rayMoveY.origin, rayMoveY.direction, new Color(0, 0.5f, 0, 1));
        RaycastHit2D infoMoveY = Physics2D.Raycast(transform.position, dirMoveY, 0.2f);

        if (isMoveX && infoMoveX.collider != null && (infoMoveX.collider.gameObject.CompareTag("Box") || infoMoveX.collider.gameObject.CompareTag("board")))
            isMoving = false;

        if (isMoveX || isMoveY)
        {
            bool x = (isMoveX || isMoveY) && infoMoveX.collider != null && (infoMoveX.collider.gameObject.CompareTag("Box") || infoMoveX.collider.gameObject.CompareTag("board"));
            bool y = (isMoveX || isMoveY) && infoMoveY.collider != null && (infoMoveY.collider.gameObject.CompareTag("Box") || infoMoveY.collider.gameObject.CompareTag("board"));
            if (x && !y && (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1)))
                x = y;
            if (y && !x && (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1)))
                y = x;

            isMoving = !(x || y);

            /*
            ////DebugLogger.Log("X:" + x);
            ////DebugLogger.Log("Y:" + y);
            */
        }
        else
        {
            if (playerColor == BattleControl.PlayerColor.blue && jumpRayDistance != 0)
                isMoving = true;
            else
                isMoving = false;
        }

        float movingSave = 0;
        if (playerColor == BattleControl.PlayerColor.blue && isJump)
        {
            switch (playerDir)
            {
                case PlayerDirEnum.up:
                    movingSave = moving.y;
                    break;

                case PlayerDirEnum.down:
                    movingSave = moving.y;
                    break;

                case PlayerDirEnum.left:
                    movingSave = moving.x;
                    break;

                case PlayerDirEnum.right:
                    movingSave = moving.x;
                    break;
            }
        }

        moving.x = MainControl.instance.JudgmentNumber(false, moving.x, -5);
        moving.y = MainControl.instance.JudgmentNumber(false, moving.y, -5);
        moving.x = MainControl.instance.JudgmentNumber(true, moving.x, 5);
        moving.y = MainControl.instance.JudgmentNumber(true, moving.y, 5);
        
        Vector3 newPos = transform.position + new Vector3(speedWeightX * speed * moving.x * Time.deltaTime, speedWeightY * speed * moving.y * Time.deltaTime);//速度参考：3

        Vector3 checkPos = CheckPoint(newPos, displacement + BoxController.instance.width / 2);

        if (newPos == checkPos)
            rigidBody.MovePosition(newPos);
        else
            transform.position = checkPos;


        if (movingSave != 0)
            moving.y = movingSave;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //蓝心碰板子确保再次可以跳
        if (collision.transform.CompareTag("board") && collision.transform.GetComponent<EdgeCollider2D>().IsTouching(collideCollider) && playerColor == BattleControl.PlayerColor.blue)
        {
            BlueJumpReady();
        }
    }

    void BlueJumpReady()
    {
        jumpRayDistance = 0;
        if (isJump)
        {
            isJump = false;
        }
    }

    /*
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("board" && collision.transform.GetComponent<BoardController>().canMove && playerColor == BattleControl.PlayerColor.blue)
        {
            transform.SetParent(collision.transform);
        }
    }
    */

    ///<summary>
    ///掉出
    ///</summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("board") && playerColor == BattleControl.PlayerColor.blue && !isJump)
        {
            BlueDown();
        }
    }

    ///<summary>
    ///通过渐变动画将玩家的颜色改变。
    ///若gradientTime/dingTime等于0 则不会有渐变动画/ding动画；
    ///若gradientTime/dingTime小于0 则使用该脚本内的gradientTime/dingTime变量。
    ///若PlayerColor输入为nullColor，则不会更改玩家的实际颜色属性。
    ///</summary>
    public void ChangePlayerColor(Color aimColor, BattleControl.PlayerColor aimPlayerColor, float startForce = 0, PlayerDirEnum dir = PlayerDirEnum.nullDir, float gradientTime = -1, float dingTime = -1, int fx = 2)
    {
        AudioController.instance.GetFx(fx, MainControl.instance.AudioControl.fxClipBattle);

        if (gradientTime < 0)
            gradientTime = this.gradientTime;
        if (dingTime < 0)
            dingTime = this.dingTime;


        if (gradientTime <= 0)
        {
            spriteRenderer.color = aimColor;
            if (dingTime > 0)
            {
                changeDingColor.Kill(true);
                changeDingScale.Kill(true);

                dingSpriteRenderer.transform.localScale = Vector3.one;
                dingSpriteRenderer.color = aimColor;
                changeDingColor = dingSpriteRenderer.DOColor(dingSpriteRenderer.color * new Color(1, 1, 1, 0), dingTime).SetEase(Ease.InOutSine);
                changeDingScale = dingSpriteRenderer.transform.DOScale(Vector3.one * 2.5f, dingTime).SetEase(Ease.InOutSine);
            }
        }
        else
        {
            changeColor.Kill(true);
            changeColor = spriteRenderer.DOColor(aimColor, gradientTime).SetEase(Ease.InOutSine);
            if (dingTime > 0)
            {

                changeDingColor.Kill(true);
                changeDingScale.Kill(true);

                dingSpriteRenderer.transform.localScale = Vector3.one;
                dingSpriteRenderer.color += new Color(0, 0, 0, 1);
                changeDingColor = dingSpriteRenderer.DOColor(aimColor * new Color(1, 1, 1, 0), dingTime).SetEase(Ease.InOutSine);
                changeDingScale = dingSpriteRenderer.transform.DOScale(Vector3.one * 2.5f, dingTime).SetEase(Ease.InOutSine);
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
        if (aimPlayerColor == BattleControl.PlayerColor.blue)
        {
            transform.SetParent(null);
            BlueDown(startForce, dir);
        }


    }
    ///<summary>
    ///让蓝心坠落
    ///</summary>
    void BlueDown(float startForce = 0, PlayerDirEnum dir = PlayerDirEnum.nullDir)
    {
        if (dir != PlayerDirEnum.nullDir)
        {
            playerDir = dir;
        }
        jumpRayDistance = 0.2f;
        isJump = true;
        switch (playerDir)
        {
            case PlayerDirEnum.up:
                moving = new Vector3(moving.x, startForce);
                break;

            case PlayerDirEnum.down:
                moving = new Vector3(moving.x, -startForce);
                break;

            case PlayerDirEnum.left:
                moving = new Vector3(-startForce, moving.y);
                break;

            case PlayerDirEnum.right:
                moving = new Vector3(startForce, moving.y);
                break;
        }
    }
    /////////////////////////////////////////判定相关
    //定义用于判断点是否在多边形内的方法
    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        bool isInside = false; //初始化点是否在多边形内的标志为false
                               //遍历多边形的每一条边，使用射线法判断点是否在多边形内
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            //如果点与当前边的两个端点之一在Y轴的两侧，并且在X轴的左侧，则反转内部标志
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
             (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                isInside = !isInside;
            }
        }
        return isInside; //返回点是否在多边形内的最终结果
    }

    //定义计算点到线段最近点的方法（计算垂足）
    private Vector2 GetNearestPointOnLine(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 line = end - start; //计算线段的向量
        float len = line.magnitude; //获取线段长度
        line.Normalize(); //标准化线段向量

        Vector2 v = point - start; //计算点到线段起点的向量
        float d = Vector2.Dot(v, line); //计算点在线段向量上的投影长度
        d = Mathf.Clamp(d, 0f, len); //限制投影长度在0到线段长度之间
        return start + line * d; //计算并返回最近点的坐标
    }

    //定义计算位移后垂点位置的方法
    private Vector2 CalculateDisplacedPoint(Vector2 nearestPoint, Vector2 point, Vector2 lineStart, Vector2 lineEnd, float displacement)
    {
        Vector2 lineDirection = (lineEnd - lineStart).normalized; //计算线段方向向量
        Vector2 perpendicularDirection = new Vector2(-lineDirection.y, lineDirection.x); //计算垂直方向向量（逆时针旋转90度）

        return nearestPoint + perpendicularDirection * -displacement; //计算并返回位移后的垂点位置
    }

    //定义计算内缩多边形顶点的方法
    public List<Vector2> CalculateInwardOffset(List<Vector2> vertices, float offset)
    {
        if (vertices == null || vertices.Count < 3) return null; //如果顶点列表为空或少于3个，返回null

        List<Vector2> offsetVertices = new List<Vector2>(); //初始化存储位移后顶点的列表
        List<Vector2> intersectionPoints = new List<Vector2>(); //初始化存储交点的列表

        int count = vertices.Count; //获取顶点数量
        for (int i = 0; i < count; i++)
        {
            Vector2 currentVertex = vertices[i]; //获取当前顶点
            Vector2 nextVertex = vertices[(i + 1) % count]; //获取下一个顶点（环形列表）

            Vector2 edgeDirection = (nextVertex - currentVertex).normalized; //计算边的方向向量
            Vector2 perpendicularDirection = new Vector2(-edgeDirection.y, edgeDirection.x); //计算垂直方向向量

            Vector2 offsetCurrentVertex = currentVertex + perpendicularDirection * offset; //计算当前顶点的位移
            Vector2 offsetNextVertex = nextVertex + perpendicularDirection * offset; //计算下一个顶点的位移

            offsetVertices.Add(offsetCurrentVertex); //添加位移后的当前顶点到列表
            offsetVertices.Add(offsetNextVertex); //添加位移后的下一个顶点到列表

            if (i > 0) //从第二条边开始计算交点
            {
                bool foundIntersection = LineLineIntersection(out Vector2 intersection, offsetVertices[i * 2 - 2], offsetVertices[i * 2 - 1], offsetCurrentVertex, offsetNextVertex);
                if (foundIntersection)
                {
                    intersectionPoints.Add(intersection); //如果找到交点，添加到交点列表
                }
            }
        }

        //计算首尾两条边的交点
        bool foundFinalIntersection = LineLineIntersection(out Vector2 finalIntersection, offsetVertices[offsetVertices.Count - 2], offsetVertices[offsetVertices.Count - 1], offsetVertices[0], offsetVertices[1]);
        if (foundFinalIntersection)
        {
            intersectionPoints.Add(finalIntersection); //如果找到交点，添加到交点列表
        }

        return intersectionPoints; //返回交点列表，即内缩多边形的顶点
    }

    //定义线线交点计算的方法
    private bool LineLineIntersection(out Vector2 intersection, Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
    {
        intersection = new Vector2(); //初始化交点坐标

        float d = (point1.x - point2.x) * (point3.y - point4.y) - (point1.y - point2.y) * (point3.x - point4.x); //计算分母
        if (d == 0) return false; //如果分母为0，则线段平行或重合，无交点

        float pre = (point1.x * point2.y - point1.y * point2.x), post = (point3.x * point4.y - point3.y * point4.x);
        intersection.x = (pre * (point3.x - point4.x) - (point1.x - point2.x) * post) / d; //计算交点X坐标
        intersection.y = (pre * (point3.y - point4.y) - (point1.y - point2.y) * post) / d; //计算交点Y坐标

        return true; //返回true，表示找到交点
    }
    //定义根据位移检查并调整点位置的方法
    public Vector3 CheckPoint(Vector3 point, float displacement, int maxDepth = 10, int currentDepth = 0, bool isInitialCall = true)
    {
        Vector2 originalPoint = point; //保存原始点位置
        float z = point.z;
        if (currentDepth >= maxDepth) //检查是否达到递归次数限制
        {
            return point; //如果达到最大次数，返回当前点
        }

        foreach (var box in BoxController.instance.boxes) //遍历所有战斗框
        {
            if (box.localPosition.z != z)//排除Z轴不同的
                continue;

            float rDis;
            if (box.sonBoxDrawer.Count > 0)
                rDis = displacement + test1;
            else
                rDis = displacement;
            List<Vector2> movedVertices = CalculateInwardOffset(box.GetRealPoints(false), -rDis); //计算缩放后的多边形顶点
            /*
            foreach (var item in movedVertices) //遍历移动后的顶点
            {
                //DebugLogger.Log(item, DebugLogger.Type.err); //记录日志
            }
            */
            if (IsPointInPolygon(point, movedVertices)) //如果点 在 调整后的多边形内
            {
                //DebugLogger.Log(point, DebugLogger.Type.war, "#FF00FF"); //记录日志
                return point; //返回原始坐标
            }

        }
        //如果点 不在 调整后的多边形内

        Vector2 nearestPoint = Vector2.zero; //最近点
        Vector2 lineStart = Vector2.zero; 
        Vector2 lineEnd = Vector2.zero; 
        float nearestDistance = float.MaxValue; //最近距离设为最大值
        bool isParent = false;//确定框是否为复合的框，如果是，需要额外调整移动距离

        foreach (var box in BoxController.instance.boxes) //遍历所有战斗框
        {
            if (box.localPosition.z != z)//排除Z轴不同的
                continue;


            for (int i = 0, j = box.GetRealPoints(false).Count - 1; i < box.GetRealPoints(false).Count; j = i++) //遍历框的所有边
            {
                Vector2 tempNearestPoint = GetNearestPointOnLine(point, box.GetRealPoints(false)[i], box.GetRealPoints(false)[j]); //计算到当前边的最近点
                float tempDistance = Vector2.Distance(point, tempNearestPoint); //计算距离
                if (tempDistance < nearestDistance) //如果距离更短
                {
                    nearestPoint = tempNearestPoint; //更新最近点
                    lineStart = box.GetRealPoints(false)[i]; //更新线段起点
                    lineEnd = box.GetRealPoints(false)[j]; //更新线段终点
                    nearestDistance = tempDistance; //更新最近距离
                    isParent = box.sonBoxDrawer.Count > 0;
                    
                }
            }
        }

        if (nearestDistance < float.MaxValue) //如果找到最近点
        {
            if (isParent)
                displacement -= test2;

            Vector3 moved = (Vector3)CalculateDisplacedPoint(nearestPoint, point, lineStart, lineEnd, -displacement) + new Vector3(0, 0, z); //计算位移后的点位置
            //DebugLogger.Log(moved, DebugLogger.Type.war, "#FF0000"); //记录日志

            if (isInitialCall || (Vector2)moved != originalPoint) //如果是初次调用或移动后的点不等于原点
            {
                Vector3 newCheck = (Vector3)(Vector2)CheckPoint(moved, displacement, maxDepth, currentDepth + 1, false) + new Vector3(0, 0, z); //递归调用，增加递归深度
                if (newCheck != moved) //如果移动后的点未通过检测
                {
                    //因为已经在递归中处理递归深度，所以这里不需要再次调用CheckPoint
                    return newCheck; //返回新检查点
                }
                return moved; //返回移动后的点
            }
        }

        return point;//如果没有找到更近的点，返回原点
    }
}

//杂项
/*
 写蓝心的时候无意中搞出来的弹球式蓝心：

  case BattleControl.PlayerColor.blue:
                switch (playerDir)
                {
                    case PlayerDirEnum.up:
                        transform.rotation = Quaternion.Euler(0, 0, 180);
                        Physics2D.gravity = new Vector2(0, 9.8f);
                        break;

                    case PlayerDirEnum.down:
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        Physics2D.gravity = new Vector2(0, -9.8f);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
                        {
                            moving = new Vector3(1, moving.y);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                        {
                            moving = new Vector3(-1, moving.y);
                        }
                        else
                            moving = new Vector3(0, moving.y);

                        if(MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && !isJump)
                        {
                            moving = new Vector3(moving.x, 2.15f);
                            isJump = true;
                        }
                        if (isJump && !MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && moving.y > 0)
                            moving = new Vector3(moving.x, 0);
                        if (isJump)
                        {
                            Vector2 dirReal = new Vector2();
                            switch (playerDir)
                            {
                                case PlayerDirEnum.up:
                                    dirReal = Vector2.up;
                                    break;

                                case PlayerDirEnum.down:
                                    dirReal = Vector2.down;
                                    break;

                                case PlayerDirEnum.left:
                                    dirReal = Vector2.left;
                                    break;

                                case PlayerDirEnum.right:
                                    dirReal = Vector2.right;
                                    break;
                            }
                            Ray2D ray = new Ray2D(transform.position, dirReal);
                            Debug.DrawRay(ray.origin, ray.direction, Color.blue, collideCollider.radius + 0.05f);
                            RaycastHit2D info = Physics2D.Raycast(transform.position, dirReal, collideCollider.radius + 0.05f);//距离为圆碰撞器+0.05f
                            if (info.collider != null && info.transform.position.z == transform.position.z)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("Box")
                                {
                                    isJump = false;
                                }
                            }

                            moving.y -= Time.deltaTime * (float)Math.Pow(1.85f,jumpAcceleration);
                        }
                        else
                        {
                            jumpAcceleration = 1.25f;
                            moving.y = 0;
                        }
                        jumpAcceleration += Time.deltaTime * 0.425f;
                            break;

                    case PlayerDirEnum.left:
                        transform.rotation = Quaternion.Euler(0, 0, 270);
                        Physics2D.gravity = new Vector2(-9.8f, 0);
                        break;

                    case PlayerDirEnum.right:
                        transform.rotation = Quaternion.Euler(0, 0, 90);
                        Physics2D.gravity = new Vector2(9.8f, 0);
                        break;
                }
                break;
*/