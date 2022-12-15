using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Overworld中的玩家控制器
/// </summary>
public class PlayerBehaviour : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rbody;
    BoxCollider2D boxCollider;
    TypeWritter typeWritter;
    public int moveDirectionX, moveDirectionY;
    public int animDirectionX, animDirectionY;
    public float distance;
    public float speed;//玩家速度 编辑器标准为13 导出为5.5
    [Header("音效截取范围 int")]
    public Vector2 walk;

    //public LayerMask mask;
    //天杀的Ray检测范围太短了 还是拿box好使）
    BoxCollider2D boxTrigger;

    OverworldObjTrigger saveOwObj;
    GameObject backpackUI;
    BackpackBehaviour backpackBehaviour;
    public float owTimer;//0.1秒，防止调查OW冲突
    private void Awake()
    {
        backpackUI = GameObject.Find("Main Camera/BackpackUI");
        backpackBehaviour = GameObject.Find("BackpackCanvas").GetComponent<BackpackBehaviour>();
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody2D>();
        boxTrigger = transform.Find("Trigger").GetComponent<BoxCollider2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        typeWritter = GameObject.Find("BackpackCanvas").GetComponent<TypeWritter>();
        boxTrigger.transform.localPosition = boxCollider.offset;
        //mask = 1 << 6;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "owObjTrigger")
        {
            OverworldObjTrigger owObj = collision.transform.GetComponent<OverworldObjTrigger>();
            saveOwObj = owObj;
            
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "owObjTrigger")
        {
            OverworldObjTrigger owObj = collision.transform.GetComponent<OverworldObjTrigger>();
            if (owObj == saveOwObj)
            {
                saveOwObj = null;
            }
        }
    }

    private void Update()
    {
        MainControl.instance.PlayerControl.deadPos = transform.position;
        if (typeWritter.isTyping)
        {
            distance = 0;
        }
        else
        {
            if (moveDirectionX != 0 && moveDirectionY == 0)
                distance = 1;
            else if (moveDirectionX == 0 && moveDirectionY != 0)
                distance = 2;
        }

        if (MainControl.instance.OverwroldControl.isSetting || MainControl.instance.OverwroldControl.pause)
        {
            return;
        }
        if (owTimer > 0)
        {
            owTimer -= Time.deltaTime;
        }
        if (saveOwObj != null && backpackUI.transform.localPosition.z < 0)
        {
            if (saveOwObj.isTriggerMode || (!saveOwObj.isTriggerMode && MainControl.instance.KeyArrowToControl(KeyCode.Z) && backpackBehaviour.selent == 0))
            {
                if (owTimer <= 0)
                {

                    bool isUp;
                    if (saveOwObj.setIsUp)
                        isUp = saveOwObj.isUp;
                    else isUp = transform.position.y < saveOwObj.mainCamera.transform.position.y - 1.25f;
                    if (saveOwObj.openAnim)
                        saveOwObj.AnimTypeText(isUp);
                    else
                        saveOwObj.TypeText(isUp);

                }
                owTimer = 0.1f;
            }

        }
        /*
        Vector2 dirReal = new Vector2(animDirectionX, animDirectionY);
        Ray2D ray = new Ray2D((Vector2)transform.position + boxCollider.offset, dirReal);
        Debug.DrawRay(ray.origin, ray.direction, Color.blue);
        RaycastHit2D info = Physics2D.Raycast(transform.position, dirReal, distance, mask);
        

        if (info.collider != null && MainControl.instance.KeyArrowToControl(KeyCode.Z))
        {
            GameObject obj = info.collider.gameObject;
            Debug.Log(obj.transform.tag);
            if (obj.transform.tag == "owObjTrigger")
            {
                OverworldObjTrigger owObj = obj.transform.GetComponent<OverworldObjTrigger>();
                if (!owObj.isTriggerMode)
                {
                    owObj.TypeText(true);
                }
            }
        }
        */

        if (MainControl.instance.OverwroldControl.isSetting || MainControl.instance.OverwroldControl.pause)
            return;
        if (Input.GetKeyDown(KeyCode.B) && MainControl.instance.OverwroldControl.isDebug)
            MainControl.instance.OutBlack("Battle");
    }
    public void PlayWalkAudio()//动画器引用
    {
        AudioController.instance.GetFx(Random.Range((int)walk.x, (int)walk.y), MainControl.instance.AudioControl.fxClipWalk);
    }
    public void TriggerSpin(int i)
    {
        boxTrigger.transform.localRotation = Quaternion.Euler(0, 0, i * 90);

        if (i == 0 || i == 2)
        {
            boxTrigger.offset = new Vector2(0, -0.165f);
            boxTrigger.size = new Vector2(0.5f, 0.4f);
        }
        else
        {
            boxTrigger.offset = new Vector2(0, -0.255f);
            boxTrigger.size = new Vector2(0.5f, 0.575f);
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (MainControl.instance.OverwroldControl.isSetting || MainControl.instance.OverwroldControl.pause || backpackBehaviour.selent > 0)
        {
            animator.Play("Walk Tree", 0, 0);
            if (MainControl.instance.PlayerControl.canMove)
                animator.enabled = false;
            return;
        }
        else
            animator.enabled = true;

        if (MainControl.instance.PlayerControl.canMove)
        {

            if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow,1))
                moveDirectionX = 1;
            else if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                moveDirectionX = -1;
            else moveDirectionX = 0;
         
            if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                moveDirectionX = 0;

            if (moveDirectionX != 0)
                animDirectionX = moveDirectionX;

            if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1))
            {
                moveDirectionY = 1;
                if (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && !MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                    animDirectionX = 0;

            }
            else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
            {
                if (!MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) && !MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                    animDirectionX = 0;
                moveDirectionY = -1;
            }
            else moveDirectionY = 0;

            if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                moveDirectionX = 0;



            if (moveDirectionX != 0 || moveDirectionY != 0)
                animDirectionY = moveDirectionY;

            rbody.MovePosition(new Vector2(transform.position.x + speed * Time.deltaTime * moveDirectionX, transform.position.y + speed * Time.deltaTime * moveDirectionY));

            if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
            {

                animator.SetFloat("MoveX", Random.Range(-1, 2));
                animator.SetFloat("MoveY", Random.Range(-1, 2));
            }
            else
            {
                animator.SetFloat("MoveX", animDirectionX);
                animator.SetFloat("MoveY", animDirectionY);
            }

            if(moveDirectionY == 0)
            {
                if (moveDirectionX > 0)
                {
                    TriggerSpin(1);
                }
                else if(moveDirectionX < 0)
                {
                    TriggerSpin(3);
                }
            }
            else
            {
                if (moveDirectionX < 0)
                {
                    if (moveDirectionY < 0)
                        TriggerSpin(0);
                    else
                        TriggerSpin(2);
                }
                else if (moveDirectionX > 0)
                {
                    if (moveDirectionY < 0)
                        TriggerSpin(0);
                    else
                        TriggerSpin(2);
                }
                else
                {
                    if (moveDirectionY > 0)
                        TriggerSpin(2);
                    else if(moveDirectionY < 0)
                        TriggerSpin(0);

                }
            }


            if (moveDirectionX == 0 && moveDirectionY == 0)
            {
                animator.Play("Walk Tree", 0, 0f);
                animator.speed = 0;
            }
            else animator.speed = 1;
        }
        else
        {
            animator.Play("Walk Tree", 0, 0f);
            animator.speed = 0;
        }
    }
}