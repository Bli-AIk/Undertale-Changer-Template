using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using Log;
using System.Collections.Generic;
/// <summary>
/// ����ս�������(��)���������
/// </summary>
public class BattlePlayerController : MonoBehaviour
{
    [Header("�ı�ɫʱ��ding�����ٶȣ�0Ϊ��")]
    public float dingTime;
    [Header("�Ľ��䶯���ٶȣ�0Ϊ��")]
    public float gradientTime;

    [Header("�������Ե���")]
    public float speed;
    public float speedWeightX, speedWeightY;//�ٶ���Ȩ��(��X���Ա���)���ٶȲ���Ϊ3��Ȩ��0.5f
    private float speedWeight = 0.5f;
    public float hitCD, hitCDMax;//�޵�ʱ��
    public bool isMoving;//�������ȹ��жϣ�����Ƿ�������ƶ�
    public float timeInterpolation = -0.225f;
    public Vector3 sceneDrift = new Vector3(-1000, 0);
    public enum PlayerDirEnum
    {
        up,
        down,
        left,
        right,
        nullDir
    };

    public PlayerDirEnum playerDir;//����
    public Vector3 moving;
    public bool isJump;//�Ƿ��ڡ�����״̬
    public float jumpAcceleration;//��Ծ�ļ��ٶ�
    public float jumpRayDistance;//���߾���
    public float jumpRayDistanceForBoard;

    private Rigidbody2D rigidBody;
    public CircleCollider2D collideCollider;//Բ����ײ��
    private SpriteRenderer spriteRenderer, dingSpriteRenderer;
    public BattleControl.PlayerColor playerColor;//�������Ե���ɫ ��ȡBattleControl�е�enum PlayerColor.��ɫ�任ͨ������任�ĺ�����ִ��
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
                MainControl.instance.OverworldControl.playerDeadPos = transform.position - sceneDrift;
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
        RaycastHit2D infoF = Physics2D.Raycast(transform.position, dirReal * -1, jumpRayDistance);//������(��ͷ)

        //------------------------�ƶ�------------------------
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
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moving.y < -0)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moving = new Vector3(moving.x, -0);

                        }
                        if (isJump)
                        {
                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
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
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moving.y > 0)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moving = new Vector3(moving.x, 0);
                        }
                        if (isJump)
                        {
                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
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
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moving.x > 0)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moving = new Vector3(0, moving.y);

                        }
                        if (isJump)
                        {
                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
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
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moving.x < -0)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moving = new Vector3(-0, moving.y);

                        }
                        if (isJump)
                        {
                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
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


        //���ȹ����õ��Ƿ��ƶ��ж�
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

        if (isMoveX && infoMoveX.collider != null && (infoMoveX.collider.gameObject.CompareTag("frame") || infoMoveX.collider.gameObject.CompareTag("board")))
            isMoving = false;

        if (isMoveX || isMoveY)
        {
            bool x = (isMoveX || isMoveY) && infoMoveX.collider != null && (infoMoveX.collider.gameObject.CompareTag("frame") || infoMoveX.collider.gameObject.CompareTag("board"));
            bool y = (isMoveX || isMoveY) && infoMoveY.collider != null && (infoMoveY.collider.gameObject.CompareTag("frame") || infoMoveY.collider.gameObject.CompareTag("board"));
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

        Vector3 newPos = transform.position + new Vector3(speedWeightX * speed * moving.x * Time.deltaTime, speedWeightY * speed * moving.y * Time.deltaTime);//�ٶȲο���3
        Vector3 checkPos = CheckPoint(newPos, 0.175f + BoxController.instance.width / 2);

        if (newPos == checkPos)
            rigidBody.MovePosition(newPos);
        else
            transform.position = checkPos;
        //transform.position = transform.position + new Vector3(speed * moving.x * Time.deltaTime, speed * moving.y * Time.deltaTime);//���Ļ��о޴�ƫ�� ������
        if (movingSave != 0)
            moving.y = movingSave;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //����������ȷ���ٴο�����
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

    /// <summary>
    /// ����
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("board") && playerColor == BattleControl.PlayerColor.blue && !isJump)
        {
            BlueDown();
        }
    }

    /// <summary>
    /// ͨ�����䶯������ҵ���ɫ�ı䡣
    /// ��gradientTime/dingTime����0 �򲻻��н��䶯��/ding������
    /// ��gradientTime/dingTimeС��0 ��ʹ�øýű��ڵ�gradientTime/dingTime������
    /// ��PlayerColor����ΪnullColor���򲻻������ҵ�ʵ����ɫ���ԡ�
    /// </summary>
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
    /// <summary>
    /// ������׹��
    /// </summary>
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
    /////////////////////////////////////////�ж����
    // ���������жϵ��Ƿ��ڶ�����ڵķ���
    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        bool isInside = false; // ��ʼ�����Ƿ��ڶ�����ڵı�־Ϊfalse
                               // ��������ε�ÿһ���ߣ�ʹ�����߷��жϵ��Ƿ��ڶ������
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            // ������뵱ǰ�ߵ������˵�֮һ��Y������࣬������X�����࣬��ת�ڲ���־
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
             (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                isInside = !isInside;
            }
        }
        return isInside; // ���ص��Ƿ��ڶ�����ڵ����ս��
    }

    // �������㵽�߶������ķ��������㴹�㣩
    private Vector2 GetNearestPointOnLine(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 line = end - start; // �����߶ε�����
        float len = line.magnitude; // ��ȡ�߶γ���
        line.Normalize(); // ��׼���߶�����

        Vector2 v = point - start; // ����㵽�߶���������
        float d = Vector2.Dot(v, line); // ��������߶������ϵ�ͶӰ����
        d = Mathf.Clamp(d, 0f, len); // ����ͶӰ������0���߶γ���֮��
        return start + line * d; // ���㲢��������������
    }

    // �������λ�ƺ󴹵�λ�õķ���
    private Vector2 CalculateDisplacedPoint(Vector2 nearestPoint, Vector2 point, Vector2 lineStart, Vector2 lineEnd, float displacement)
    {
        Vector2 lineDirection = (lineEnd - lineStart).normalized; // �����߶η�������
        Vector2 perpendicularDirection = new Vector2(-lineDirection.y, lineDirection.x); // ���㴹ֱ������������ʱ����ת90�ȣ�

        return nearestPoint + perpendicularDirection * -displacement; // ���㲢����λ�ƺ�Ĵ���λ��
    }

    // ���������������ζ���ķ���
    public List<Vector2> CalculateInwardOffset(List<Vector2> vertices, float offset)
    {
        if (vertices == null || vertices.Count < 3) return null; // ��������б�Ϊ�ջ�����3��������null

        List<Vector2> offsetVertices = new List<Vector2>(); // ��ʼ���洢λ�ƺ󶥵���б�
        List<Vector2> intersectionPoints = new List<Vector2>(); // ��ʼ���洢������б�

        int count = vertices.Count; // ��ȡ��������
        for (int i = 0; i < count; i++)
        {
            Vector2 currentVertex = vertices[i]; // ��ȡ��ǰ����
            Vector2 nextVertex = vertices[(i + 1) % count]; // ��ȡ��һ�����㣨�����б�

            Vector2 edgeDirection = (nextVertex - currentVertex).normalized; // ����ߵķ�������
            Vector2 perpendicularDirection = new Vector2(-edgeDirection.y, edgeDirection.x); // ���㴹ֱ��������

            Vector2 offsetCurrentVertex = currentVertex + perpendicularDirection * offset; // ���㵱ǰ�����λ��
            Vector2 offsetNextVertex = nextVertex + perpendicularDirection * offset; // ������һ�������λ��

            offsetVertices.Add(offsetCurrentVertex); // ���λ�ƺ�ĵ�ǰ���㵽�б�
            offsetVertices.Add(offsetNextVertex); // ���λ�ƺ����һ�����㵽�б�

            if (i > 0) // �ӵڶ����߿�ʼ���㽻��
            {
                bool foundIntersection = LineLineIntersection(out Vector2 intersection, offsetVertices[i * 2 - 2], offsetVertices[i * 2 - 1], offsetCurrentVertex, offsetNextVertex);
                if (foundIntersection)
                {
                    intersectionPoints.Add(intersection); // ����ҵ����㣬��ӵ������б�
                }
            }
        }

        // ������β�����ߵĽ���
        bool foundFinalIntersection = LineLineIntersection(out Vector2 finalIntersection, offsetVertices[offsetVertices.Count - 2], offsetVertices[offsetVertices.Count - 1], offsetVertices[0], offsetVertices[1]);
        if (foundFinalIntersection)
        {
            intersectionPoints.Add(finalIntersection); // ����ҵ����㣬��ӵ������б�
        }

        return intersectionPoints; // ���ؽ����б�����������εĶ���
    }

    // �������߽������ķ���
    private bool LineLineIntersection(out Vector2 intersection, Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
    {
        intersection = new Vector2(); // ��ʼ����������

        float d = (point1.x - point2.x) * (point3.y - point4.y) - (point1.y - point2.y) * (point3.x - point4.x); // �����ĸ
        if (d == 0) return false; // �����ĸΪ0�����߶�ƽ�л��غϣ��޽���

        float pre = (point1.x * point2.y - point1.y * point2.x), post = (point3.x * point4.y - point3.y * point4.x);
        intersection.x = (pre * (point3.x - point4.x) - (point1.x - point2.x) * post) / d; // ���㽻��X����
        intersection.y = (pre * (point3.y - point4.y) - (point1.y - point2.y) * post) / d; // ���㽻��Y����

        return true; // ����true����ʾ�ҵ�����
    }
    // �������λ�Ƽ�鲢������λ�õķ���
    public Vector2 CheckPoint(Vector2 point, float displacement, int maxDepth = 10, int currentDepth = 0, bool isInitialCall = true)
    {
        Vector2 originalPoint = point; // ����ԭʼ��λ��
        if (currentDepth >= maxDepth) // ����Ƿ�ﵽ�ݹ��������
        {
            return point; // ����ﵽ�����������ص�ǰ��
        }

        foreach (var box in BoxController.instance.boxes) // �������к��Ӷ���
        {
            List<Vector2> movedVertices = CalculateInwardOffset(box.GetRealPoints(), -displacement); // �������ź�Ķ���ζ���

            foreach (var item in movedVertices) // �����ƶ���Ķ���
            {
                //DebugLogger.Log(item, DebugLogger.Type.err); // ��¼��־
            }
            if (IsPointInPolygon(point, movedVertices)) // ������ڵ�����Ķ������
            {
                //DebugLogger.Log(point, DebugLogger.Type.war, "#FF00FF"); // ��¼��־
                return point; // ����ԭʼ����
            }
        }

        Vector2 nearestPoint = Vector2.zero; // ��ʼ�������
        Vector2 lineStart = Vector2.zero; // ��ʼ���߶����
        Vector2 lineEnd = Vector2.zero; // ��ʼ���߶��յ�
        float nearestDistance = float.MaxValue; // ��ʼ���������Ϊ���ֵ
        foreach (var box in BoxController.instance.boxes) // �������к���
        {
            for (int i = 0, j = box.GetRealPoints().Count - 1; i < box.GetRealPoints().Count; j = i++) // �������ӵ����б�
            {
                Vector2 tempNearestPoint = GetNearestPointOnLine(point, box.GetRealPoints()[i], box.GetRealPoints()[j]); // ���㵽��ǰ�ߵ������
                float tempDistance = Vector2.Distance(point, tempNearestPoint); // �������
                if (tempDistance < nearestDistance) // ����������
                {
                    nearestPoint = tempNearestPoint; // ���������
                    lineStart = box.GetRealPoints()[i]; // �����߶����
                    lineEnd = box.GetRealPoints()[j]; // �����߶��յ�
                    nearestDistance = tempDistance; // �����������
                }
            }
        }

        if (nearestDistance < float.MaxValue) // ����ҵ������
        {
            Vector2 moved = CalculateDisplacedPoint(nearestPoint, point, lineStart, lineEnd, -displacement); // ����λ�ƺ�ĵ�λ��
            //DebugLogger.Log(moved, DebugLogger.Type.war, "#FF0000"); // ��¼��־

            if (isInitialCall || moved != originalPoint) // ����ǳ��ε��û��ƶ���ĵ㲻����ԭ��
            {
                Vector2 newCheck = CheckPoint(moved, displacement, maxDepth, currentDepth + 1, false); // �ݹ���ã����ӵݹ����
                if (newCheck != moved) // ����ƶ���ĵ�δͨ�����
                {
                    // ��Ϊ�Ѿ��ڵݹ��д���ݹ���ȣ��������ﲻ��Ҫ�ٴε���CheckPoint
                    return newCheck; // �����¼���
                }
                return moved; // �����ƶ���ĵ�
            }
        }

        return originalPoint; // ���û���ҵ������ĵ㣬����ԭ��
    }
}

//����
/*
 д���ĵ�ʱ�������и�����ĵ���ʽ���ģ�

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
                            RaycastHit2D info = Physics2D.Raycast(transform.position, dirReal, collideCollider.radius + 0.05f);//����ΪԲ��ײ��+0.05f
                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame")
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