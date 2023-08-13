using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
/// <summary>
/// Overworld�е���ҿ�����
/// </summary>
public class PlayerBehaviour : MonoBehaviour
{
    public Animator animator;
    Rigidbody2D rbody;
    BoxCollider2D boxCollider;
    TypeWritter typeWritter;
    public int moveDirectionX, moveDirectionY;
    public int animDirectionX, animDirectionY;
    public float distance;
    public float speed;//����ٶ� �༭����׼Ϊ13 ����Ϊ5.5
    
    [Header("��Ч��ȡ��Χ int")]
    public Vector2 walk;

    [Header("������Ӱ")]
    public bool isShadow;
    SpriteRenderer shadowSprite;

    //public LayerMask mask;
    BoxCollider2D boxTrigger;

    public OverworldObjTrigger saveOwObj;
    GameObject backpackUI;
    SpriteRenderer spriteRenderer;
    public float owTimer;//0.1�룬��ֹ����OW��ͻ

    AudioMixerGroup mixer = null;//��Ҫ��Ū�� ���������Ϊ�е���Ŀ�����˻���Ч��
    
    private void Awake()
    {
        backpackUI = GameObject.Find("Main Camera/BackpackUI");
    }
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody2D>();
        boxTrigger = transform.Find("Trigger").GetComponent<BoxCollider2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        typeWritter = GameObject.Find("BackpackCanvas").GetComponent<TypeWritter>();
        boxTrigger.transform.localPosition = boxCollider.offset;
        //mask = 1 << 6;

        transform.position = MainControl.instance.OverworldControl.playerScenePos;
        animDirectionX = (int)MainControl.instance.OverworldControl.animDirection.x;
        animDirectionY = (int)MainControl.instance.OverworldControl.animDirection.y;

        if (isShadow)
            shadowSprite = transform.Find("Dir/Shadow").GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("owObjTrigger"))
        {
            OverworldObjTrigger owObj = collision.transform.GetComponent<OverworldObjTrigger>();
            saveOwObj = owObj;
            
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("owObjTrigger"))
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

        if (isShadow)
        {
            shadowSprite.sprite = spriteRenderer.sprite;
        }


        MainControl.instance.OverworldControl.playerDeadPos = transform.position;
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

        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
        {
            return;
        }
        if (owTimer > 0)
        {
            owTimer -= Time.deltaTime;
        }
        if (saveOwObj != null && backpackUI.transform.localPosition.z < 0)
        {
            if (saveOwObj.isTriggerMode
                || (!saveOwObj.isTriggerMode && MainControl.instance.KeyArrowToControl(KeyCode.Z)
                && ((saveOwObj.playerDir == Vector2.one) || (saveOwObj.playerDir.x == animDirectionX) || (saveOwObj.playerDir.y == animDirectionY))
                && BackpackBehaviour.instance.select == 0)) 
            {
                if (owTimer <= 0)
                {
                    if (saveOwObj.changeScene)
                    {
                        if ((saveOwObj.onlyDir == 0) || (saveOwObj.onlyDir == -1 && animDirectionX != 0) || (saveOwObj.onlyDir == 1 && animDirectionY != 0))
                        {
                            MainControl.instance.OverworldControl.playerScenePos = saveOwObj.newPlayerPos;
                            MainControl.instance.OverworldControl.animDirection = new Vector2(animDirectionX, animDirectionY);
                            MainControl.instance.OutBlack(saveOwObj.sceneName, Color.black, saveOwObj.banMusic);
                            saveOwObj.gameObject.SetActive(false);
                            saveOwObj = null;
                        }
                    }
                    else
                    {
                        bool isUp;
                        if (saveOwObj.setIsUp)
                            isUp = saveOwObj.isUp;
                        else isUp = transform.position.y < saveOwObj.mainCamera.transform.position.y - 1.25f;
                        if (saveOwObj.openAnim)
                            saveOwObj.AnimTypeText(isUp);
                        else
                            saveOwObj.TypeText(isUp);

                        if (saveOwObj.isSave)
                        {
                            AudioController.instance.GetFx(MainControl.instance.AudioControl.fxClipUI[2]);
                            if (MainControl.instance.PlayerControl.hp < MainControl.instance.PlayerControl.hpMax)
                                MainControl.instance.PlayerControl.hp = MainControl.instance.PlayerControl.hpMax;

                        }

                    }
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
            //Debug.Log(obj.transform.tag);
            if (obj.transform.CompareTag("owObjTrigger")
            {
                OverworldObjTrigger owObj = obj.transform.GetComponent<OverworldObjTrigger>();
                if (!owObj.isTriggerMode)
                {
                    owObj.TypeText(true);
                }
            }
        }
        */

        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;
        if (Input.GetKeyDown(KeyCode.B) && MainControl.instance.PlayerControl.isDebug)
            MainControl.instance.OutBlack("Battle", Color.black);
    }
    public void PlayWalkAudio()//����������
    {
        AudioController.instance.GetFx(Random.Range((int)walk.x, (int)walk.y), MainControl.instance.AudioControl.fxClipWalk, 1, 1, mixer);
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
    
    void FixedUpdate()
    {
        float speed;
        if (MainControl.instance.KeyArrowToControl(KeyCode.X, 1))
            speed = this.speed * 2;
        else speed = this.speed;

        animator.SetFloat("Speed", System.Convert.ToInt32(MainControl.instance.KeyArrowToControl(KeyCode.X, 1)) + 1);

        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause || BackpackBehaviour.instance.select > 0)
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

            //if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                //moveDirectionX = 0;



            if (moveDirectionX != 0 || moveDirectionY != 0)
                animDirectionY = moveDirectionY;

            rbody.MovePosition(new Vector2(transform.position.x + speed * Time.deltaTime * moveDirectionX, transform.position.y + speed * Time.deltaTime * moveDirectionY));

            if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))//&& !(MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1)))
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