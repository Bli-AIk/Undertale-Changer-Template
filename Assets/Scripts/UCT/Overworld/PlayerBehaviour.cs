using System;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace UCT.Overworld
{
    /// <summary>
    /// Overworld中的玩家控制器
    /// </summary>
    public class PlayerBehaviour : MonoBehaviour
    {
        public Animator animator;
        private Rigidbody2D _rbody;
        private BoxCollider2D _boxCollider;
        private TypeWritter _typeWritter;
        public int moveDirectionX, moveDirectionY;
        public int animDirectionX, animDirectionY;
        public float distance;
        public float speed;//玩家速度 编辑器标准为13 导出为5.5

        [Header("音效截取范围 int")]
        public Vector2 walk;

        [Header("开启倒影")]
        public bool isShadow;

        private SpriteRenderer _shadowSprite;

        //public LayerMask mask;
        private BoxCollider2D _boxTrigger;

        public OverworldObjTrigger saveOwObj;
        private GameObject _backpackUI;
        private SpriteRenderer _spriteRenderer;
        public float owTimer;//0.1秒，防止调查OW冲突

        private AudioMixerGroup _mixer = null;//需要就弄上 整这个是因为有的项目里做了回音效果

        private void Awake()
        {
            _backpackUI = GameObject.Find("Main Camera/BackpackUI");
        }

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            _rbody = GetComponent<Rigidbody2D>();
            _boxTrigger = transform.Find("Trigger").GetComponent<BoxCollider2D>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _typeWritter = GameObject.Find("BackpackCanvas").GetComponent<TypeWritter>();
            _boxTrigger.transform.localPosition = _boxCollider.offset;
            //mask = 1 << 6;

            transform.position = MainControl.Instance.OverworldControl.playerScenePos;
            animDirectionX = (int)MainControl.Instance.OverworldControl.animDirection.x;
            animDirectionY = (int)MainControl.Instance.OverworldControl.animDirection.y;

            if (isShadow)
                _shadowSprite = transform.Find("Dir/Shadow").GetComponent<SpriteRenderer>();
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
                _shadowSprite.sprite = _spriteRenderer.sprite;
            }

            MainControl.Instance.OverworldControl.playerDeadPos = transform.position;
            if (_typeWritter.isTyping)
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

            if (MainControl.Instance.OverworldControl.isSetting || MainControl.Instance.OverworldControl.pause)
            {
                return;
            }
            if (owTimer > 0)
            {
                owTimer -= Time.deltaTime;
            }
            if (saveOwObj != null && _backpackUI.transform.localPosition.z < 0)
            {
                if (saveOwObj.isTriggerMode
                    || (!saveOwObj.isTriggerMode && MainControl.Instance.KeyArrowToControl(KeyCode.Z)
                                                 && ((saveOwObj.playerDir == Vector2.one) || (saveOwObj.playerDir.x == animDirectionX) || (saveOwObj.playerDir.y == animDirectionY))
                                                 && BackpackBehaviour.Instance.select == 0))
                {
                    if (owTimer <= 0)
                    {
                        if (saveOwObj.changeScene)
                        {
                            if ((saveOwObj.onlyDir == 0) || (saveOwObj.onlyDir == -1 && animDirectionX != 0) || (saveOwObj.onlyDir == 1 && animDirectionY != 0))
                            {
                                MainControl.Instance.OverworldControl.playerScenePos = saveOwObj.newPlayerPos;
                                MainControl.Instance.OverworldControl.animDirection = new Vector2(animDirectionX, animDirectionY);
                                MainControl.Instance.OutBlack(saveOwObj.sceneName, Color.black, saveOwObj.banMusic);
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
                                AudioController.Instance.GetFx(2, MainControl.Instance.AudioControl.fxClipUI);
                                if (MainControl.Instance.PlayerControl.hp < MainControl.Instance.PlayerControl.hpMax)
                                    MainControl.Instance.PlayerControl.hp = MainControl.Instance.PlayerControl.hpMax;
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

            if (MainControl.Instance.OverworldControl.isSetting || MainControl.Instance.OverworldControl.pause)
                return;
            if (Input.GetKeyDown(KeyCode.B) && MainControl.Instance.PlayerControl.isDebug)
                MainControl.Instance.OutBlack("Battle", Color.black);
        }

        public void PlayWalkAudio()//动画器引用
        {
            AudioController.Instance.GetFx(Random.Range((int)walk.x, (int)walk.y), MainControl.Instance.AudioControl.fxClipWalk, 1, 1, _mixer);
        }

        public void TriggerSpin(int i)
        {
            _boxTrigger.transform.localRotation = Quaternion.Euler(0, 0, i * 90);

            if (i == 0 || i == 2)
            {
                _boxTrigger.offset = new Vector2(0, -0.165f);
                _boxTrigger.size = new Vector2(0.5f, 0.4f);
            }
            else
            {
                _boxTrigger.offset = new Vector2(0, -0.255f);
                _boxTrigger.size = new Vector2(0.5f, 0.575f);
            }
        }

        private void FixedUpdate()
        {
            float speed;
            if (MainControl.Instance.KeyArrowToControl(KeyCode.X, 1))
                speed = this.speed * 2;
            else speed = this.speed;

            animator.SetFloat("Speed", Convert.ToInt32(MainControl.Instance.KeyArrowToControl(KeyCode.X, 1)) + 1);

            if (MainControl.Instance.OverworldControl.isSetting || MainControl.Instance.OverworldControl.pause || BackpackBehaviour.Instance.select > 0)
            {
                animator.Play("Walk Tree", 0, 0);
                if (MainControl.Instance.PlayerControl.canMove)
                    animator.enabled = false;
                return;
            }

            animator.enabled = true;

            if (MainControl.Instance.PlayerControl.canMove)
            {
                if (MainControl.Instance.KeyArrowToControl(KeyCode.RightArrow, 1))
                    moveDirectionX = 1;
                else if (MainControl.Instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                    moveDirectionX = -1;
                else moveDirectionX = 0;

                if (MainControl.Instance.KeyArrowToControl(KeyCode.RightArrow, 1) && MainControl.Instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                    moveDirectionX = 0;

                if (moveDirectionX != 0)
                    animDirectionX = moveDirectionX;

                if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow, 1))
                {
                    moveDirectionY = 1;
                    if (!MainControl.Instance.KeyArrowToControl(KeyCode.RightArrow, 1) && !MainControl.Instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        animDirectionX = 0;
                }
                else if (MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                {
                    if (!MainControl.Instance.KeyArrowToControl(KeyCode.RightArrow, 1) && !MainControl.Instance.KeyArrowToControl(KeyCode.LeftArrow, 1))
                        animDirectionX = 0;
                    moveDirectionY = -1;
                }
                else moveDirectionY = 0;

                //if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.instance.KeyArrowToControl(KeyCode.DownArrow, 1))
                //moveDirectionX = 0;

                if (moveDirectionX != 0 || moveDirectionY != 0)
                    animDirectionY = moveDirectionY;

                _rbody.MovePosition(new Vector2(transform.position.x + speed * Time.deltaTime * moveDirectionX, transform.position.y + speed * Time.deltaTime * moveDirectionY));

                if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow, 1) && MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow, 1))//&& !(MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1)))
                {
                    animator.SetFloat("MoveX", Random.Range(-1, 2));
                    animator.SetFloat("MoveY", Random.Range(-1, 2));
                }
                else
                {
                    animator.SetFloat("MoveX", animDirectionX);
                    animator.SetFloat("MoveY", animDirectionY);
                }

                if (moveDirectionY == 0)
                {
                    if (moveDirectionX > 0)
                    {
                        TriggerSpin(1);
                    }
                    else if (moveDirectionX < 0)
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
                        else if (moveDirectionY < 0)
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
}