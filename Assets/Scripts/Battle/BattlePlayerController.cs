using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Rendering;
using MEC;

/// <summary>
/// Control the relevant attributes of the player (heart) in the battle scene
/// </summary>
public class BattlePlayerController : MonoBehaviour
{
    public float speed, speedWeightX, speedWeightY;//Speed and weight (multiplied by X), the speed test is 3, and the weight is 0.5f
    float speedWeight = 0.5f;
    public float hitCD, hitCDMax;//Invincible time
    public bool isMoveing;//Used for Blue or Orange Bone Determination: Is the player really moving?
    public enum PlayerDirEnum
    {
        up,
        down,
        left,
        right,
        nullDir

    };
    public PlayerDirEnum playerDir;//direction
    public Vector3 moveing;
    public bool isJump;//Is it in the 'start jumping' state
    public float jumpAcceleration;//Jumping acceleration
    public float jumpRayDistance;//Ray distance
    public float jumpRayDistanceForBoard;
    public float jumpFrozen, jumpFrozenMax;//Used for timing the number of frames in the air when the jump key is raised midway through the jump.

    Rigidbody2D rigidBody;
    public CircleCollider2D collideCollider;//Circular collision
    SpriteRenderer spriteRenderer;
    public BattleControl.PlayerColor playerColor;//Colors containing attributes. Read enum PlayerColor in BattleControl. Color transformation is performed through specific transformation functions
    Tween missAnim = null;

    public Volume hitVolume;
    //LayerMask mask;
    void Start()
    {
        speedWeightX = 1;
        speedWeightY = 1;
        jumpFrozenMax = 0.25f;
        jumpRayDistance = 0.2f;
        jumpRayDistanceForBoard = 0.2f;
        jumpAcceleration = 1.25f;
        playerColor = BattleControl.PlayerColor.red;
        playerDir = PlayerDirEnum.down;
        rigidBody = GetComponent<Rigidbody2D>();
        collideCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
                MainControl.instance.OverworldControl.playerDeadPos = transform.position;
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
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 0.1f, 0);
            else if (Input.GetKeyDown(KeyCode.K))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 0.1f, (PlayerDirEnum)1);
            else if (Input.GetKeyDown(KeyCode.J))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 0.1f, (PlayerDirEnum)2);
            else if (Input.GetKeyDown(KeyCode.L))
                ChangePlayerColor(MainControl.instance.BattleControl.playerColorList[5], (BattleControl.PlayerColor)5, 0.1f, (PlayerDirEnum)3);

            if (Input.GetKeyDown(KeyCode.P))
                MainControl.instance.PlayerControl.hp = 0;

        }
    }
    
    void FixedUpdate()
    {
        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;
        if (!TurnController.instance.isMyTurn)
            Moveing();

       


    }
    void Moveing()
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
        RaycastHit2D infoF = Physics2D.Raycast(transform.position, dirReal * -1, jumpRayDistance);//Reverse detection (touching the top)

        //------------------------MOVE------------------------
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
                    moveing = new Vector3(moveing.x, 1);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                {
                    moveing = new Vector3(moveing.x, -1);
                }
                else
                    moveing = new Vector3(moveing.x, 0);

                if(MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1)&& MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                    moveing = new Vector3(moveing.x, 0);


                if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))
                {
                    moveing = new Vector3(1, moveing.y);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                {
                    moveing = new Vector3(-1, moveing.y);
                }
                else
                    moveing = new Vector3(0, moveing.y);

                if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                    moveing = new Vector3(0, moveing.y);
                break;
            case BattleControl.PlayerColor.orange:
                /*Pressing both direction keys simultaneously has a bug
                if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                {
                    moveing = new Vector3(moveing.x, 1);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                {
                    moveing = new Vector3(moveing.x, -1);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow) || MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                {
                    moveing = new Vector3(moveing.x, 0);
                }

                if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
                {
                    moveing = new Vector3(1, moveing.y);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                {
                    moveing = new Vector3(-1, moveing.y);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                    moveing = new Vector3(0, moveing.y);

                while (moveing == Vector3.zero || (moveing.x != 0 && moveing.y != 0))
                    moveing = new Vector3(UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));
                */
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
                    if (obj.transform.CompareTag("board") && !obj.transform.GetComponent<EdgeCollider2D>().isTrigger && infoForBoard.collider == obj.transform.GetComponent<EdgeCollider2D>() && obj.transform.GetComponent<BoardController>().canMove)
                    {
                        jumpRayDistance = 0;
                        isJump = false;
                        transform.SetParent(infoForBoard.transform);
                    }
                    else
                        transform.SetParent(null);
                }
                else
                {
                    transform.SetParent(null);

                }

                int gravity = 10;
                if (rigidBody.gravityScale != gravity)
                rigidBody.gravityScale = gravity;
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

                            moveing = new Vector3(1, moveing.y);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        {

                            moveing = new Vector3(-1, moveing.y);
                        }
                        else
                            moveing = new Vector3(0, moveing.y);

                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow,1))
                            moveing = new Vector3(0, moveing.y);

                        if (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow,1))
                            jumpRayDistanceForBoard = 0.2f;
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        {
                            jumpRayDistanceForBoard = 0;
                            transform.SetParent(null);
                        }

                        if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow,1) && !isJump && moveing.y == 0)
                        {
                            transform.SetParent(null);

                            rigidBody.gravityScale = 0;
                            moveing = new Vector3(moveing.x, -2.15f);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.DownArrow,1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moveing.y < -0.55f)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moveing = new Vector3(moveing.x, -0.55f);
                            jumpFrozen = jumpFrozenMax;
                        }
                        if (isJump)
                        {

                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
                                {
                                    jumpRayDistance = 0;
                                    isJump = false;
                                }
                            }

                            moveing.y += Time.deltaTime * (float)Math.Pow(2, jumpAcceleration);
                        }
                        else
                        {
                            jumpAcceleration = 1.25f;
                            moveing.y = 0;
                        }
                        jumpAcceleration += Time.deltaTime * 0.425f;
                        break;


                    case PlayerDirEnum.down:////////////////////////////////////////////////
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X,1))
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
                            moveing = new Vector3(1, moveing.y);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        {

                            moveing = new Vector3(-1, moveing.y);
                        }
                        else
                            moveing = new Vector3(0, moveing.y);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                            moveing = new Vector3(0, moveing.y);

                        if (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                            jumpRayDistanceForBoard = 0.2f;
                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        {
                            jumpRayDistanceForBoard = 0;
                            transform.SetParent(null);
                        }


                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && !isJump && moveing.y == 0)
                        {
                            transform.SetParent(null);

                            rigidBody.gravityScale = 0;
                            moveing = new Vector3(moveing.x, 2.15f);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moveing.y > 0.55f)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moveing = new Vector3(moveing.x, 0.55f);
                            jumpFrozen = jumpFrozenMax;
                        }
                        if (isJump)
                        {

                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
                                {
                                    jumpRayDistance = 0;
                                    isJump = false;

                                }
                            }

                            moveing.y -= Time.deltaTime * (float)Math.Pow(2, jumpAcceleration);
                        }
                        else
                        {
                            jumpAcceleration = 1.25f;
                            moveing.y = 0;
                        }
                        jumpAcceleration += Time.deltaTime * 0.425f;
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

                            moveing = new Vector3(moveing.x, 1);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                        {

                            moveing = new Vector3(moveing.x, -1);
                        }
                        else
                            moveing = new Vector3(moveing.x, 0);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            moveing = new Vector3(moveing.x, 0);

                        if (!MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            jumpRayDistanceForBoard = 0.2f;
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                        {
                            jumpRayDistanceForBoard = 0;
                            transform.SetParent(null);
                        }


                        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && !isJump && moveing.x == 0)
                        {
                            transform.SetParent(null);

                            rigidBody.gravityScale = 0;
                            moveing = new Vector3(2.15f, moveing.y);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moveing.x > 0.55f)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moveing = new Vector3(0.55f, moveing.y);
                            jumpFrozen = jumpFrozenMax;
                        }
                        if (isJump)
                        {

                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
                                {
                                    jumpRayDistance = 0;
                                    isJump = false;
                                }
                            }

                            moveing.x -= Time.deltaTime * (float)Math.Pow(2, jumpAcceleration);
                        }
                        else
                        {
                            jumpAcceleration = 1.25f;
                            moveing.x = 0;
                        }
                        jumpAcceleration += Time.deltaTime * 0.425f;
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

                            moveing = new Vector3(moveing.x, 1);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                        {

                            moveing = new Vector3(moveing.x, -1);
                        }
                        else
                            moveing = new Vector3(moveing.x, 0);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            moveing = new Vector3(moveing.x, 0);

                        if (!MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || !MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                            jumpRayDistanceForBoard = 0.2f;
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                        {
                            jumpRayDistanceForBoard = 0;
                            transform.SetParent(null);
                        }


                        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) && !isJump && moveing.x == 0)
                        {
                            transform.SetParent(null);

                            rigidBody.gravityScale = 0;
                            moveing = new Vector3(-2.15f, moveing.y);
                            isJump = true;
                            jumpRayDistance = 0.2f;
                            jumpRayDistanceForBoard = 0;
                        }
                        if (isJump && (!MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || (infoF.collider != null && infoF.collider.gameObject.CompareTag("frame"))) && moveing.x < -0.55f)
                        {
                            jumpRayDistanceForBoard = 0.2f;
                            moveing = new Vector3(-0.55f, moveing.y);
                            jumpFrozen = jumpFrozenMax;
                        }
                        if (isJump)
                        {

                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame"))
                                {
                                    jumpRayDistance = 0;
                                    isJump = false;
                                }
                            }

                            moveing.x += Time.deltaTime * (float)Math.Pow(2, jumpAcceleration);
                        }
                        else
                        {
                            jumpAcceleration = 1.25f;
                            moveing.x = 0;
                        }
                        jumpAcceleration += Time.deltaTime * 0.425f;
                        break;
                }


                break;
            case BattleControl.PlayerColor.purple:
                break;
            default:
                break;
        }

        if (jumpFrozen > 0)
        {
            jumpFrozen -= Time.deltaTime;
        }





        //Used by Blue Orange Bone. Judgment on whether to move or not
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
            isMoveing = false;

        if (isMoveX || isMoveY)
        {
            bool x = (isMoveX || isMoveY) && infoMoveX.collider != null && (infoMoveX.collider.gameObject.CompareTag("frame") || infoMoveX.collider.gameObject.CompareTag("board"));
            bool y = (isMoveX || isMoveY) && infoMoveY.collider != null && (infoMoveY.collider.gameObject.CompareTag("frame") || infoMoveY.collider.gameObject.CompareTag("board"));
            if (x && !y && (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))) 
                x = y;
            if (y && !x && (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1))) 
                y = x;


            isMoveing = !(x || y);

            /*
            //Debug.Log("X:" + x);
            //Debug.Log("Y:" + y);
            */
        }
        else
        {
            if (playerColor == BattleControl.PlayerColor.blue && jumpRayDistance != 0)
                isMoveing = true;
            else
                isMoveing = false;
        }

        float moveingSave = 0;
        if (playerColor == BattleControl.PlayerColor.blue && isJump && jumpFrozen > 0)
        {
            switch (playerDir)
            {
                case PlayerDirEnum.up:
                    moveingSave = moveing.y;
                    moveing.y = -0.5f * jumpFrozen;
                    rigidBody.gravityScale = 0;
                    break;
                case PlayerDirEnum.down:
                    moveingSave = moveing.y;
                    moveing.y = 0.5f * jumpFrozen;
                    rigidBody.gravityScale = 0;
                    break;
                case PlayerDirEnum.left:
                    moveingSave = moveing.x;
                    moveing.x = 0.5f * jumpFrozen;
                    rigidBody.gravityScale = 0;
                    break;
                case PlayerDirEnum.right:
                    moveingSave = moveing.x;
                    moveing.x = -0.5f * jumpFrozen;
                    rigidBody.gravityScale = 0;
                    break;
            }

            
        }
      
        moveing.x = MainControl.instance.JudgmentNumber(false, moveing.x, -5);
        moveing.y = MainControl.instance.JudgmentNumber(false, moveing.y, -5);
        moveing.x = MainControl.instance.JudgmentNumber(true, moveing.x, 5);
        moveing.y = MainControl.instance.JudgmentNumber(true, moveing.y, 5);


        rigidBody.MovePosition(transform.position + new Vector3(speedWeightX * speed * moveing.x * Time.deltaTime, speedWeightY * speed * moveing.y * Time.deltaTime));//Speed Reference: 3
        //transform.position = transform.position + new Vector3(speed * moveing.x * Time.deltaTime, speed * moveing.y * Time.deltaTime);//Using this will cause significant deviation in the Blue Heart, so it is not recommended to use it
        if (moveingSave != 0)
            moveing.y = moveingSave;



    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //The blue heart touches the board to ensure it can jump again
        if (collision.transform.CompareTag("board") && collision.transform.GetComponent<EdgeCollider2D>().IsTouching(collideCollider) && playerColor == BattleControl.PlayerColor.blue)
        {
            jumpRayDistance = 0;
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
    /// fall out
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("board") && playerColor == BattleControl.PlayerColor.blue && !isJump)
        {
            jumpRayDistance = 0.2f;
            isJump = true;
            switch (playerDir)
            {
                case PlayerDirEnum.up:
                    moveing = new Vector3(moveing.x, -0.55f);
                    break;
                case PlayerDirEnum.down:
                    moveing = new Vector3(moveing.x, 0.55f);
                    break;
                case PlayerDirEnum.left:
                    moveing = new Vector3(0.55f, moveing.y);
                    break;
                case PlayerDirEnum.right:
                    moveing = new Vector3(-0.55f, moveing.y);
                    break;
            }
           

        }

    }

    /// <summary>
    /// Change the player's color through gradient animation.
    /// If time is less than or equal to 0, there will be no gradient animation;
    /// If the PlayerColor input is nullColor, the player's actual color attribute will not be changed.
    /// </summary>
    public void ChangePlayerColor(Color aimColor, BattleControl.PlayerColor aimPlayerColor, float time = 0.1f, PlayerDirEnum dir = PlayerDirEnum.nullDir)
    {
        if (time <= 0)
            spriteRenderer.color = aimColor;
        else
        {
            spriteRenderer.DOColor(aimColor, time).SetEase(Ease.InOutSine);
        }
        if (playerColor != aimPlayerColor)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.SetParent(null);
            playerColor = aimPlayerColor;
            moveing = Vector3.zero;
            isJump = false;
            jumpAcceleration = 1.25f;
            rigidBody.gravityScale = 0;

            if (aimPlayerColor == BattleControl.PlayerColor.blue)
            {
                jumpRayDistance = 0.2f;
                isJump = true;
                switch (playerDir)
                {
                    case PlayerDirEnum.up:
                        moveing = new Vector3(moveing.x, -0.55f);
                        break;
                    case PlayerDirEnum.down:
                        moveing = new Vector3(moveing.x, 0.55f);
                        break;
                    case PlayerDirEnum.left:
                        moveing = new Vector3(0.55f, moveing.y);
                        break;
                    case PlayerDirEnum.right:
                        moveing = new Vector3(-0.55f, moveing.y);
                        break;
                }
            }
            
        }
        if (dir != PlayerDirEnum.nullDir)
        {
            transform.SetParent(null);
            playerDir = dir;
            jumpRayDistance = 0.2f;
            isJump = true;
            switch (playerDir)
            {
                case PlayerDirEnum.up:
                    moveing = new Vector3(moveing.x, -0.55f);
                    break;
                case PlayerDirEnum.down:
                    moveing = new Vector3(moveing.x, 0.55f);
                    break;
                case PlayerDirEnum.left:
                    moveing = new Vector3(0.55f, moveing.y);
                    break;
                case PlayerDirEnum.right:
                    moveing = new Vector3(-0.55f, moveing.y);
                    break;
            }
        }
    }
}




//miscellaneous
/*
 The pinball style blue heart that was accidentally created while writing blue heart:

  case BattleControl.PlayerColor.blue:
                int gravity = 10;
                if (rigidBody.gravityScale != gravity)
                rigidBody.gravityScale = gravity;
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
                            moveing = new Vector3(1, moveing.y);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                        {
                            moveing = new Vector3(-1, moveing.y);
                        }
                        else
                            moveing = new Vector3(0, moveing.y);

                        if(MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && !isJump)
                        {
                            rigidBody.gravityScale = 0;
                            moveing = new Vector3(moveing.x, 2.15f);
                            isJump = true;
                        }
                        if (isJump && !MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && moveing.y > 0.55f)
                            moveing = new Vector3(moveing.x, 0.55f);
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
                            RaycastHit2D info = Physics2D.Raycast(transform.position, dirReal, collideCollider.radius + 0.05f);//¾àÀëÎªÔ²Åö×²Æ÷+0.05f
                            if (info.collider != null)
                            {
                                GameObject obj = info.collider.gameObject;
                                if (obj.transform.CompareTag("frame")
                                {
                                    isJump = false;
                                }
                            }

                            moveing.y -= Time.deltaTime * (float)Math.Pow(1.85f,jumpAcceleration);
                        }
                        else
                        {
                            jumpAcceleration = 1.25f;
                            moveing.y = 0;
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