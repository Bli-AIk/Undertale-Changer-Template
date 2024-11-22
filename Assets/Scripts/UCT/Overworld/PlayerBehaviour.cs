using System;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace UCT.Overworld
{
    /// <summary>
    ///     Overworld中的玩家控制器
    /// </summary>
    public class PlayerBehaviour : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        public Animator animator;
        public int moveDirectionX, moveDirectionY;
        public int animDirectionX, animDirectionY;
        public float speed; //玩家速度 编辑器标准为13 导出为5.5

        [Header("音效截取范围 int")] public Vector2 walk;

        [Header("开启倒影")] public bool isShadow;

        public OverworldObjTrigger saveOwObj;
        public float owTimer; //0.1秒，防止调查OW冲突

        private readonly AudioMixerGroup _mixer = null; //需要就弄上 整这个是因为有的项目里做了回音效果
        private BoxCollider2D _boxCollider;

        //public LayerMask mask;
        private BoxCollider2D _boxTrigger;
        private Rigidbody2D _rigidbody2D;

        private SpriteRenderer _shadowSprite;
        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _boxTrigger = transform.Find("Trigger").GetComponent<BoxCollider2D>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _boxTrigger.transform.localPosition = _boxCollider.offset;
            transform.position = MainControl.Instance.overworldControl.playerScenePos;
            animDirectionX = (int)MainControl.Instance.overworldControl.animDirection.x;
            animDirectionY = (int)MainControl.Instance.overworldControl.animDirection.y;

            if (isShadow)
                _shadowSprite = transform.Find("Dir/Shadow").GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (isShadow) _shadowSprite.sprite = _spriteRenderer.sprite;

            MainControl.Instance.overworldControl.playerDeadPos = transform.position;


            if (MainControl.Instance.overworldControl.isSetting || MainControl.Instance.overworldControl.pause) return;
            if (owTimer > 0) owTimer -= Time.deltaTime;

            if (saveOwObj && Mathf.Approximately(BackpackBehaviour.Instance.optionsBox.localPosition.z,
                    BackpackBehaviour.BoxZAxisInvisible))
                if (saveOwObj.isTriggerMode || (!saveOwObj.isTriggerMode &&
                                                GameUtilityService.ConvertKeyDownToControl(KeyCode.Z) &&
                                                (saveOwObj.playerDir == Vector2.one ||
                                                 Mathf.Approximately(saveOwObj.playerDir.x, animDirectionX) ||
                                                 Mathf.Approximately(saveOwObj.playerDir.y, animDirectionY)) &&
                                                BackpackBehaviour.Instance.select == 0))
                {
                    if (owTimer <= 0)
                    {
                        if (saveOwObj.changeScene)
                        {
                            if (saveOwObj.onlyDir == 0 || (saveOwObj.onlyDir == -1 && animDirectionX != 0) ||
                                (saveOwObj.onlyDir == 1 && animDirectionY != 0))
                            {
                                MainControl.Instance.overworldControl.playerScenePos = saveOwObj.newPlayerPos;
                                MainControl.Instance.overworldControl.animDirection =
                                    new Vector2(animDirectionX, animDirectionY);
                                GameUtilityService.FadeOutAndSwitchScene(saveOwObj.sceneName, Color.black,
                                    saveOwObj.banMusic);
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
                                if (MainControl.Instance.playerControl.hp < MainControl.Instance.playerControl.hpMax)
                                    MainControl.Instance.playerControl.hp = MainControl.Instance.playerControl.hpMax;
                            }
                        }
                    }

                    owTimer = 0.1f;
                }

            if (MainControl.Instance.overworldControl.isSetting || MainControl.Instance.overworldControl.pause)
                return;
            if (Input.GetKeyDown(KeyCode.B) && MainControl.Instance.playerControl.isDebug)
                GameUtilityService.FadeOutAndSwitchScene("Battle", Color.black);
        }

        private void FixedUpdate()
        {
            float realSpeed;
            if (GameUtilityService.ConvertKeyToControl(KeyCode.X))
                realSpeed = speed * 2;
            else realSpeed = speed;

            animator.SetFloat(Speed, Convert.ToInt32(GameUtilityService.ConvertKeyToControl(KeyCode.X)) + 1);

            if (MainControl.Instance.overworldControl.isSetting || MainControl.Instance.overworldControl.pause ||
                BackpackBehaviour.Instance.select > 0)
            {
                animator.Play("Walk Tree", 0, 0);
                if (MainControl.Instance.playerControl.canMove)
                    animator.enabled = false;
                return;
            }

            animator.enabled = true;

            if (MainControl.Instance.playerControl.canMove)
            {
                if (GameUtilityService.ConvertKeyToControl(KeyCode.RightArrow))
                    moveDirectionX = 1;
                else if (GameUtilityService.ConvertKeyToControl(KeyCode.LeftArrow))
                    moveDirectionX = -1;
                else moveDirectionX = 0;

                if (GameUtilityService.ConvertKeyToControl(KeyCode.RightArrow) &&
                    GameUtilityService.ConvertKeyToControl(KeyCode.LeftArrow))
                    moveDirectionX = 0;

                if (moveDirectionX != 0)
                    animDirectionX = moveDirectionX;

                if (GameUtilityService.ConvertKeyToControl(KeyCode.UpArrow))
                {
                    moveDirectionY = 1;
                    if (!GameUtilityService.ConvertKeyToControl(KeyCode.RightArrow) &&
                        !GameUtilityService.ConvertKeyToControl(KeyCode.LeftArrow))
                        animDirectionX = 0;
                }
                else if (GameUtilityService.ConvertKeyToControl(KeyCode.DownArrow))
                {
                    if (!GameUtilityService.ConvertKeyToControl(KeyCode.RightArrow) &&
                        !GameUtilityService.ConvertKeyToControl(KeyCode.LeftArrow))
                        animDirectionX = 0;
                    moveDirectionY = -1;
                }
                else
                {
                    moveDirectionY = 0;
                }

                if (moveDirectionX != 0 || moveDirectionY != 0)
                    animDirectionY = moveDirectionY;

                _rigidbody2D.MovePosition(new Vector2(
                    transform.position.x + realSpeed * Time.deltaTime * moveDirectionX,
                    transform.position.y + realSpeed * Time.deltaTime * moveDirectionY));

                if (GameUtilityService.ConvertKeyToControl(KeyCode.UpArrow) &&
                    GameUtilityService.ConvertKeyToControl(KeyCode.DownArrow) &&
                    MainControl.Instance.playerControl.isDebug) 
                {
                    //一个让玩家开杀戮光环的彩蛋
                    animator.SetFloat(MoveX, Random.Range(-1, 2));
                    animator.SetFloat(MoveY, Random.Range(-1, 2));
                }
                else
                {
                    animator.SetFloat(MoveX, animDirectionX);
                    animator.SetFloat(MoveY, animDirectionY);
                }

                if (moveDirectionY == 0)
                    switch (moveDirectionX)
                    {
                        case > 0:
                            TriggerSpin(1);
                            break;
                        case < 0:
                            TriggerSpin(3);
                            break;
                    }
                else
                    switch (moveDirectionX)
                    {
                        case < 0:
                            TriggerSpin(moveDirectionY < 0 ? 0 : 2);
                            break;
                        case > 0:
                        {
                            TriggerSpin(moveDirectionY < 0 ? 0 : 2);
                            break;
                        }
                        default:
                        {
                            switch (moveDirectionY)
                            {
                                case > 0:
                                    TriggerSpin(2);
                                    break;
                                case < 0:
                                    TriggerSpin(0);
                                    break;
                            }

                            break;
                        }
                    }

                if (moveDirectionX == 0 && moveDirectionY == 0)
                {
                    animator.Play("Walk Tree", 0, 0f);
                    animator.speed = 0;
                }
                else
                {
                    animator.speed = 1;
                }
            }
            else
            {
                animator.Play("Walk Tree", 0, 0f);
                animator.speed = 0;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.transform.CompareTag("owObjTrigger")) return;
            var owObj = collision.transform.GetComponent<OverworldObjTrigger>();
            saveOwObj = owObj;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.transform.CompareTag("owObjTrigger")) return;
            var owObj = collision.transform.GetComponent<OverworldObjTrigger>();
            if (owObj == saveOwObj) saveOwObj = null;
        }

        public void PlayWalkAudio() //动画器引用
        {
            AudioController.Instance.GetFx(Random.Range((int)walk.x, (int)walk.y),
                MainControl.Instance.AudioControl.fxClipWalk, 1, 1, _mixer);
        }

        private void TriggerSpin(int i)
        {
            _boxTrigger.transform.localRotation = Quaternion.Euler(0, 0, i * 90);

            if (i is 0 or 2)
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
    }
}