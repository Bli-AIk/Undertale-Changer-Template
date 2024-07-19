using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using Log;
using System.Collections.Generic;
///<summary>
////Controls the relevant attributes of the player (heart) in the battle.
///</summary>
public class BattlePlayerController : MonoBehaviour
{

    public float test1 = -0.5f;
    public float test2 = 0.05f;
    [Header("Speed of ding animation when heart changes color, 0 is off")]
    public float dingTime;
    [Header("Heart gradient animation speed, 0 is off")]
    public float gradientTime;

    [Header("Basic Attribute Adjustment")]
    public float speed;
    public float speedWeightX, speedWeightY;
    // speed and weight (multiplied by X), speed test is 3, weight 0.5f
    private float speedWeight = 0.5f;
    public float hitCD, hitCDMax;
    //Invincibility time
    public float displacement = 0.175f;
    //Collision distance determination
    public bool isMoving;
    // for blue and orange bone judgment: whether the player is really moving or not
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

    public PlayerDirEnum playerDir;
    //Direction
    public Vector3 moving;
    public bool isJump;
    //Whether it is in the "jump up" state.
    public float jumpAcceleration;
    //Jumping acceleration
    public float jumpRayDistance;
    //Ray distance
    public float jumpRayDistanceForBoard;

    private Rigidbody2D rigidBody;
    public CircleCollider2D collideCollider;
    //Circular collision body
    private SpriteRenderer spriteRenderer, dingSpriteRenderer;
    public BattleControl.PlayerColor playerColor;
    //Color with attributes Read enum PlayerColor from BattleControl. color transformations are performed by the function of the specific transformation
    private Tween missAnim, changeColor, changeDingColor, changeDingScale;

    public Volume hitVolume;

    //LayerMask mask.
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
        //mask = 1 << 6.
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
        RaycastHit2D infoF = Physics2D.Raycast(transform.position, dirReal * -1, jumpRayDistance);
        //Reverse detection (top head)

        //------------------------ moving ------------------------
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

                    case PlayerDirEnum.down:
                    ////////////////////////////////////////////////
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

                    case PlayerDirEnum.left:
                    ////////////////////////////////////////////////
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


        //Whether to move or not for blue and orange bones
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

        Vector3 newPos = transform.position + new Vector3(speedWeightX * speed * moving.x * Time.deltaTime, speedWeightY * speed * moving.y * Time.deltaTime);
        //Speed reference: 3

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
        //Blue heart touches the board to make sure it's possible to jump again.
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
    //// Dropped out
    ///</summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("board") && playerColor == BattleControl.PlayerColor.blue && !isJump)
        {
            BlueDown();
        }
    }

    ///<summary>
    /// Changes the player's color through a gradient animation.
    /// If gradientTime/dingTime is equal to 0 then there will be no gradient/ding animation;
    /// If gradientTime/dingTime is less than 0 then use the gradientTime/dingTime variable within this script.
    /// If PlayerColor is entered as nullColor, the actual color attribute of the player is not changed.
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
    /// Let the Blue Heart Fall
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
    ///////////////////////////////////////// decision related
    // Define the method used to determine if a point is inside a polygon.
    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        bool isInside = false;
        //false flag to initialize whether the point is inside the polygon or not
                               // Iterate over each edge of the polygon and use the ray method to determine if the point is inside the polygon.
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            //If the point is on either side of the Y-axis with one of the two endpoints of the current edge and to the left of the X-axis, reverse the internal flag
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
             (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                isInside = !isInside;
            }
        }
        return isInside;
        //returns the final result of whether the point is inside the polygon or not
    }

    // Define the method of calculating the nearest point to a line segment (calculating the vertical foot)
    private Vector2 GetNearestPointOnLine(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 line = end - start;
        //Calculate the vector of the line segments
        float len = line.magnitude;
        //get the length of the line segment
        line.Normalize();
        //Normalized line vectors

        Vector2 v = point - start;
        //calculate the vector from the point to the start of the line segment
        float d = Vector2.Dot(v, line);
        // Calculate the length of the projection of the point on the line vector
        d = Mathf.Clamp(d, 0f, len);
        // Limit the projection length to between 0 and the length of the line segment
        return start + line * d;
        //calculate and return the coordinates of the nearest point
    }

    // Define the method to calculate the position of the vertical point after displacement.
    private Vector2 CalculateDisplacedPoint(Vector2 nearestPoint, Vector2 point, Vector2 lineStart, Vector2 lineEnd, float displacement)
    {
        Vector2 lineDirection = (lineEnd - lineStart).normalized;
        //calculate line direction vector
        Vector2 perpendicularDirection = new Vector2(-lineDirection.y, lineDirection.x);
        //calculate vertical vector (rotate 90 degrees counterclockwise)

        return nearestPoint + perpendicularDirection * -displacement;
        //calculate and return the position of the vertical point after displacement
    }

    // Define the method for calculating the vertices of an interior polygon.
    public List<Vector2> CalculateInwardOffset(List<Vector2> vertices, float offset)
    {
        if (vertices == null || vertices.Count < 3) return null;
        // Return null if the vertex list is empty or has less than 3 vertices.

        List<Vector2> offsetVertices = new List<Vector2>();
        //Initialize the list of vertices after storing the displacement.
        List<Vector2> intersectionPoints = new List<Vector2>();
        //Initialize list of stored intersection points

        int count = vertices.Count;
        //get the number of vertices
        for (int i = 0; i < count; i++)
        {
            Vector2 currentVertex = vertices[i];
            //get current vertex
            Vector2 nextVertex = vertices[(i + 1) % count];
            //get next vertex (ring list)

            Vector2 edgeDirection = (nextVertex - currentVertex).normalized;
            //Calculate the direction vectors of the edges.
            Vector2 perpendicularDirection = new Vector2(-edgeDirection.y, edgeDirection.x);
            //calculate the vertical vector

            Vector2 offsetCurrentVertex = currentVertex + perpendicularDirection * offset;
            //Calculate the displacement of the current vertex.
            Vector2 offsetNextVertex = nextVertex + perpendicularDirection * offset;
            //calculate the displacement of the next vertex

            offsetVertices.Add(offsetCurrentVertex);
            //Add the displaced vertex to the list.
            offsetVertices.Add(offsetNextVertex);
            //add the next vertex to the list after displacement

            if (i > 0)
            //calculate the intersection from the second edge
            {
                bool foundIntersection = LineLineIntersection(out Vector2 intersection, offsetVertices[i * 2 - 2], offsetVertices[i * 2 - 1], offsetCurrentVertex, offsetNextVertex);
                if (foundIntersection)
                {
                    intersectionPoints.Add(intersection);
                    // if intersection is found, add to intersection list
                }
            }
        }

        //calculate the intersection of the first and last edges
        bool foundFinalIntersection = LineLineIntersection(out Vector2 finalIntersection, offsetVertices[offsetVertices.Count - 2], offsetVertices[offsetVertices.Count - 1], offsetVertices[0], offsetVertices[1]);
        if (foundFinalIntersection)
        {
            intersectionPoints.Add(finalIntersection);
            // if intersection is found, add to intersection list
        }

        return intersectionPoints;
        //returns a list of intersections, i.e. the vertices of the interior polygon.
    }

    // Define the method for calculating the intersection of lines.
    private bool LineLineIntersection(out Vector2 intersection, Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
    {
        intersection = new Vector2();
        //Initialize intersection coordinates

        float d = (point1.x - point2.x) * (point3.y - point4.y) - (point1.y - point2.y) * (point3.x - point4.x);
        //calculate the denominator
        if (d == 0) return false;
        // if the denominator is 0, the lines are parallel or coincident, no intersection

        float pre = (point1.x * point2.y - point1.y * point2.x), post = (point3.x * point4.y - point3.y * point4.x);
        intersection.x = (pre * (point3.x - point4.x) - (point1.x - point2.x) * post) / d;
        //calculate the X coordinate of the intersection point
        intersection.y = (pre * (point3.y - point4.y) - (point1.y - point2.y) * post) / d;
        //calculate the Y coordinate of the intersection point

        return true;
        //returns true, indicating that the intersection is found
    }
    //define the method of checking and adjusting the position of the point according to the displacement
    public Vector3 CheckPoint(Vector3 point, float displacement, int maxDepth = 10, int currentDepth = 0, bool isInitialCall = true)
    {
        Vector2 originalPoint = point;
        //Save the original point position
        float z = point.z;
        if (currentDepth >= maxDepth)
        //Check to see if the recursion limit has been reached.
        {
            return point;
            //If the maximum number of times is reached, return the current point
        }

        foreach (var box in BoxController.instance.boxes)
        //Iterate through all combat boxes
        {
            if (box.localPosition.z != z)
            //exclude Z-axis different
                continue;

            float rDis;
            if (box.sonBoxDrawer.Count > 0)
                rDis = displacement + test1;
            else
                rDis = displacement;
            List<Vector2> movedVertices = CalculateInwardOffset(box.GetRealPoints(false), -rDis);
            //calculate scaled polygon vertices
            /*
            foreach (var item in movedVertices)
            //Iterate over the moved vertices.
            {
                //DebugLogger.Log(item, DebugLogger.Type.err); // logging logs
            }
            */
            if (IsPointInPolygon(point, movedVertices))
            //If the point is within the adjusted polygon
            {
                //DebugLogger.Log(point, DebugLogger.Type.war, "#FF00FF"); //logging the logs
                return point;
                //return original coordinates
            }

        }
        //If the point is not in the adjusted polygon

        Vector2 nearestPoint = Vector2.zero;
        //nearest point
        Vector2 lineStart = Vector2.zero;
        Vector2 lineEnd = Vector2.zero;
        float nearestDistance = float.MaxValue;
        //Maximize the closest distance.
        bool isParent = false;
        // Determine if the box is a composite box, if so, you need to additionally adjust the movement distance

        foreach (var box in BoxController.instance.boxes)
        //Iterate through all combat boxes
        {
            if (box.localPosition.z != z)
            //Excluding Z-axis different
                continue;


            for (int i = 0, j = box.GetRealPoints(false).Count - 1; i < box.GetRealPoints(false).Count; j = i++)
            // Iterate over all sides of the box
            {
                Vector2 tempNearestPoint = GetNearestPointOnLine(point, box.GetRealPoints(false)[i], box.GetRealPoints(false)[j]);
                //calculate the closest point to the current edge
                float tempDistance = Vector2.Distance(point, tempNearestPoint);
                //calculate the distance
                if (tempDistance < nearestDistance)
                //If the distance is shorter
                {
                    nearestPoint = tempNearestPoint;
                    //Update most recent point
                    lineStart = box.GetRealPoints(false)[i];
                    //update the starting point of the line segment
                    lineEnd = box.GetRealPoints(false)[j];
                    //Update end of line segment
                    nearestDistance = tempDistance;
                    //Update closest distance
                    isParent = box.sonBoxDrawer.Count > 0;

                }
            }
        }

        if (nearestDistance < float.MaxValue)
        //If the closest point is found
        {
            if (isParent)
                displacement -= test2;

            Vector3 moved = (Vector3)CalculateDisplacedPoint(nearestPoint, point, lineStart, lineEnd, -displacement) + new Vector3(0, 0, z);
            //calculate the position of the point after displacement
            //DebugLogger.Log(moved, DebugLogger.Type.war, "#FF0000"); //logging the logs

            if (isInitialCall || (Vector2)moved != originalPoint)
            // if it is the first call or the point after the move is not equal to the original point
            {
                Vector3 newCheck = (Vector3)(Vector2)CheckPoint(moved, displacement, maxDepth, currentDepth + 1, false) + new Vector3(0, 0, z);
                //recursive call, increase recursion depth
                if (newCheck != moved)
                //If the moved point does not pass the test
                {
                    // Since recursion depth is already handled in the recursion, there is no need to call CheckPoint again.
                    return newCheck;
                    //return to new checkpoints
                }
                return moved;
                //return to the point after the move
            }
        }

        return point;
        // if no closer point is found, return to the origin
    }
}

//Miscellaneous
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
                            RaycastHit2D info = Physics2D.Raycast(transform.position, dirReal, collideCollider.radius + 0.05f);
                            // Distance to circle collider +0.05f
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
