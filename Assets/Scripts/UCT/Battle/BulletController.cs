using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UCT.Control;
using UCT.Core;
using UCT.Overworld;
using UCT.Service;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Battle
{
    /// <summary>
    ///     弹幕控制器
    /// </summary>
    public class BulletController : MonoBehaviour
    {
        /// <summary>
        ///     设置碰撞箱跟随SpriteRenderer缩放的模式。
        ///     CutFollow:切去boxColliderSizes内存储的数据；
        ///     NoFollow:不跟随缩放。
        /// </summary>
        public enum FollowMode
        {
            CutFollow,
            NoFollow
        }

        private static readonly int Mode = Shader.PropertyToID("_Mode");

        public string typeName;

        public SpriteRenderer spriteRenderer;
        public List<BoxCollider2D> boxColliderList = new();
        public List<Vector2> boxColliderSizes = new();
        public List<int> boxHitList = new();
        public BattleControl.BulletColor bulletColor;

        public FollowMode followMode;
        private Action _onGreenArrowHit;
        private Action _onYellowBulletHit;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            DetectingYellowBullet();
            DetectingGreenArrow();

            if (followMode == FollowMode.NoFollow)
            {
                return;
            }

            for (var i = 0; i < boxColliderList.Count; i++)
            {
                switch (followMode)
                {
                    case FollowMode.CutFollow:
                        boxColliderList[i].size = boxColliderList[i].transform.GetComponent<SpriteRenderer>().size -
                                                  boxColliderSizes[i];
                        break;
                    case FollowMode.NoFollow:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unexpected followMode value: {followMode}");
                }
            }
        }

        private void OnDisable()
        {
            gameObject.name = "Bullet";
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            var isPassInBattle = collision.transform.CompareTag("Player") &&
                                 (collision.name.Length < "CheckCollider".Length ||
                                  collision.name[.."CheckCollider".Length] == "CheckCollider");

            var isPassInOverworld = collision.transform.CompareTag("Player") &&
                                    collision.gameObject.name.Length >= "Heart".Length &&
                                    collision.gameObject.name[.."Heart".Length] == "Heart" &&
                                    collision.gameObject.activeSelf;

            if ((!isPassInBattle || MainControl.Instance.sceneState != MainControl.SceneState.Battle) &&
                (!isPassInOverworld || MainControl.Instance.sceneState != MainControl.SceneState.Overworld))
            {
                return;
            }

            for (var i = 0; i < boxColliderList.Count; i++)
            {
                if (!boxColliderList[i].IsTouching(collision))
                {
                    continue;
                }

                HitPlayerCheckState(i);
                break;
            }
        }

        private void HitPlayerCheckState(int i)
        {
            switch (MainControl.Instance.sceneState)
            {
                case MainControl.SceneState.Normal:
                {
                    break;
                }
                case MainControl.SceneState.Overworld:
                {
                    if (MainControl.Instance.playerControl.missTime <= 0)
                    {
                        CameraFollowPlayer.Instance.ShakeCamera();
                    }

                    MainControl.Instance.HitPlayer(boxHitList[i]);
                    break;
                }
                case MainControl.SceneState.Battle:
                {
                    if (bulletColor == BattleControl.BulletColor.White
                        || (bulletColor == BattleControl.BulletColor.Orange &&
                            !MainControl.Instance.battlePlayerController.isMoving)
                        || (bulletColor == BattleControl.BulletColor.Blue &&
                            MainControl.Instance.battlePlayerController.isMoving))
                    {
                        HitPlayerInBattle(i);
                    }

                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(
                        $"Unexpected sceneState value: {MainControl.Instance.sceneState}");
                }
            }
        }

        public void SetBullet(string bulletPathName,
            string objName = null,
            InitialTransform? initialTransform = null,
            BattleControl.BulletColor? inputBulletColor = null,
            SpriteMaskInteraction? initialMask = null)
        {
            var bullet = BulletResourceManager.LoadBullet(bulletPathName);
            if (!bullet)
            {
                return;
            }

            SetBullet(bullet, objName, initialTransform, inputBulletColor, initialMask);
        }


        private void SetBullet(BulletControl bulletControl,
            string objName = null,
            InitialTransform? initialTransform = null,
            BattleControl.BulletColor? inputBulletColor = null,
            SpriteMaskInteraction? initialMask = null)
        {
            objName ??= bulletControl.objName;

            var startPosition = new Vector3();
            var startRotation = new Quaternion();
            var startScale = new Vector3();
            if (initialTransform != null)
            {
                var notNullInitialTransform = initialTransform.Value;

                if (notNullInitialTransform.Position != null)
                {
                    startPosition = notNullInitialTransform.Position.Value;
                }

                if (notNullInitialTransform.Rotation != null)
                {
                    startRotation = notNullInitialTransform.Rotation.Value;
                }

                if (notNullInitialTransform.Scale != null)
                {
                    startScale = notNullInitialTransform.Scale.Value;
                }
            }

            inputBulletColor ??= bulletControl.bulletColor;

            initialMask ??= bulletControl.startMask;

            gameObject.name = objName;
            spriteRenderer.sortingOrder = bulletControl.layer;
            bulletColor = inputBulletColor.Value;
            spriteRenderer.color = MainControl.Instance.BattleControl.bulletColorList[(int)bulletColor];
            transform.localPosition = startPosition;
            transform.rotation = startRotation;

            if (Mathf.Approximately(startScale.z, 0))
            {
                startScale = Vector3.one;
            }

            transform.localScale = startScale;

            SetMask(initialMask.Value);

            spriteRenderer.sprite = bulletControl.sprite;

            if (typeName != bulletControl.typeName)
            {
                typeName = bulletControl.typeName;
            }
            else
            {
                return;
            }


            for (var i = 0; i < boxColliderList.Count; i++)
            {
                Destroy(boxColliderList[0]);
                boxColliderList.RemoveAt(0);
            }

            boxColliderSizes.Clear();
            boxHitList.Clear();

            boxColliderSizes = bulletControl.triggerSize;
            boxHitList = bulletControl.triggerHit;

            followMode = bulletControl.triggerFollowMode;

            //循环生成box碰撞
            SetBulletBoxCollider2D(bulletControl);
        }

        public void SetYellowBulletHit(Action onYellowBulletHit)
        {
            _onYellowBulletHit = onYellowBulletHit;
        }

        public void SetGreenArrowHit(Action onGreenArrowHit)
        {
            _onGreenArrowHit = onGreenArrowHit;
        }

        private void SetBulletBoxCollider2D(BulletControl bulletControl)
        {
            for (var i = 0; i < bulletControl.triggerSize.Count; i++)
            {
                var save = gameObject.AddComponent<BoxCollider2D>();
                save.isTrigger = true;
                if (followMode == FollowMode.NoFollow)
                {
                    save.size = boxColliderSizes[i] * spriteRenderer.size;
                }
                else
                {
                    save.size = spriteRenderer.size - boxColliderSizes[i];
                }

                save.offset = bulletControl.triggerOffset[i];

                boxColliderList.Add(save);
            }
        }

        private void HitPlayerInBattle(int i)
        {
            if (!MainControl.Instance.HitPlayer(boxHitList[i]))
            {
                return;
            }

            MainControl.Instance.selectUIController.UITextUpdate(SelectUIController.UITextMode.Hit);

            var r = Random.Range(0, 0.025f);
            var v3Spin = MathUtilityService.RandomPointOnSphereSurface(2.5f, new Vector3());
            MainControl.Instance.cameraShake.Shake(
                new Vector3(r * MathUtilityService.GetRandomUnit(), r * MathUtilityService.GetRandomUnit(), 0),
                new Vector3(0, 0, v3Spin.z), 4, 1f / 60f * 4f * 1.5f, "", Ease.OutElastic);
            MainControl.Instance.cameraShake3D.Shake(
                new Vector3(r * MathUtilityService.GetRandomUnit(), 0, r * MathUtilityService.GetRandomUnit()), v3Spin,
                4,
                1f / 60f * 4f * 1.5f, "3D CameraPoint", Ease.OutElastic);
        }

        public void SetMask(SpriteMaskInteraction spriteMaskInteraction)
        {
            spriteRenderer.material.SetFloat(Mode, (int)spriteMaskInteraction);
        }

        private void DetectCollision(string inputTag, Action onHitAction)
        {
            if (onHitAction == null)
            {
                return;
            }

            var results = new Collider2D[10];
            var filter = new ContactFilter2D { useTriggers = true };

            foreach (var count in boxColliderList.Select(item => item.OverlapCollider(filter, results)))
            {
                for (var i = 0; i < count; i++)
                {
                    if (!results[i].CompareTag(inputTag))
                    {
                        continue;
                    }

                    onHitAction.Invoke();
                }
            }
        }

        private void DetectingYellowBullet()
        {
            DetectCollision("SoulsBullet", _onYellowBulletHit);
        }

        private void DetectingGreenArrow()
        {
            DetectCollision("PlayerArrow", _onGreenArrowHit);
        }
    }
}