using System;
using System.Collections.Generic;
using DG.Tweening;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.Settings;
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

        private static readonly int OpenMask = Shader.PropertyToID("_OpenMask");
        private static readonly int OutSide = Shader.PropertyToID("_OutSide");

        public string typeName;

        public SpriteRenderer spriteRenderer;
        public List<BoxCollider2D> boxColliderList = new();
        public List<Vector2> boxColliderSizes = new();
        public List<int> boxHitList = new();
        public BattleControl.BulletColor bulletColor;

        public FollowMode followMode;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
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
            name = "Bullet";
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collision.transform.CompareTag("Player") ||
                collision.name[.."CheckCollider".Length] != "CheckCollider")
            {
                return;
            }

            for (var i = 0; i < boxColliderList.Count; i++)
            {
                if (!boxColliderList[i].IsTouching(collision))
                {
                    continue;
                }

                if (bulletColor == BattleControl.BulletColor.White
                    || (bulletColor == BattleControl.BulletColor.Orange &&
                        !MainControl.Instance.battlePlayerController.isMoving)
                    || (bulletColor == BattleControl.BulletColor.Blue &&
                        MainControl.Instance.battlePlayerController.isMoving))
                {
                    HitPlayer(i);
                }

                break;
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

        private void HitPlayer(int i)
        {
            if (MainControl.Instance.playerControl.missTime >= 0)
            {
                return;
            }

            MainControl.Instance.playerControl.hp -= boxHitList[i];
            MainControl.Instance.playerControl.missTime = MainControl.Instance.playerControl.missTimeMax;
            AudioController.Instance.PlayFx(5, MainControl.Instance.AudioControl.fxClipUI);

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
            if (MainControl.Instance.playerControl.hp <= 0)
            {
                MainControl.Instance.battlePlayerController.KillPlayer(MainControl.Instance);
            }


            if (!SettingsStorage.IsSimplifySfx)
            {
                MainControl.Instance.battlePlayerController.hitVolume.weight = 1;
            }
        }

        public void SetMask(SpriteMaskInteraction spriteMaskInteraction)
        {
            switch (spriteMaskInteraction)
            {
                case SpriteMaskInteraction.None:
                    spriteRenderer.material.SetFloat(OpenMask, 0);
                    break;

                case SpriteMaskInteraction.VisibleInsideMask:
                    spriteRenderer.material.SetFloat(OpenMask, 1);
                    spriteRenderer.material.SetFloat(OutSide, 0);
                    break;

                case SpriteMaskInteraction.VisibleOutsideMask:
                    spriteRenderer.material.SetFloat(OpenMask, 1);
                    spriteRenderer.material.SetFloat(OutSide, 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(spriteMaskInteraction), spriteMaskInteraction, null);
            }
        }
    }
    public static class BulletResourceManager
    {
        private static readonly Dictionary<string, BulletControl> BulletCache = new();

        public static BulletControl LoadBullet(string bulletPathName)
        {
            if (BulletCache.TryGetValue(bulletPathName, out var bullet))
            {
                return bullet;
            }

            var path = "Assets/Bullets/" + bulletPathName;
            bullet = Resources.Load<BulletControl>(path);
        
            if (bullet)
            {
                BulletCache[bulletPathName] = bullet;
            }
            else
            {
                Other.Debug.LogError($"Bullet resource not found: {path}");
            }

            return bullet;
        }
    }

    public readonly struct InitialTransform : IEquatable<InitialTransform>
    {
        public Vector3? Position { get; }
        public Quaternion? Rotation { get; }
        public Vector3? Scale { get; }

        public InitialTransform(Vector3 position)
        {
            Position = position;
            Rotation = null;
            Scale = null;
        }

        public InitialTransform(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
            Scale = null;
        }

        public InitialTransform(Vector3 position, Vector3 scale)
        {
            Position = position;
            Rotation = null;
            Scale = scale;
        }

        public InitialTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public bool Equals(InitialTransform other)
        {
            return Position.Equals(other.Position) && Rotation.Equals(other.Rotation) && Scale.Equals(other.Scale);
        }

        public override bool Equals(object obj)
        {
            return obj is InitialTransform other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Rotation, Scale);
        }
    }
}