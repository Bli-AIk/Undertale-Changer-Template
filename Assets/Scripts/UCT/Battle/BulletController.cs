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
                        throw new ArgumentOutOfRangeException();
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
            Vector3 startPosition = default,
            BattleControl.BulletColor inputBulletColor = default,
            SpriteMaskInteraction startMask = default,
            Vector3 startRotation = default,
            Vector3 startScale = default)
        {
            var path = "Assets/Bullets/" + bulletPathName;

            SetBullet((BulletControl)Resources.Load(path), objName, startPosition, inputBulletColor, startMask,
                startRotation, startScale);
        }

        private void SetBullet(BulletControl bulletControl,
            string objName = null,
            Vector3 startPosition = default,
            BattleControl.BulletColor inputBulletColor = default,
            SpriteMaskInteraction startMask = default,
            Vector3 startRotation = default,
            Vector3 startScale = default)
        {
            objName ??= bulletControl.objName;

            if (startPosition == default)
            {
                startPosition = bulletControl.startPosition;
            }

            if (inputBulletColor == default)
            {
                inputBulletColor = bulletControl.bulletColor;
            }

            if (startMask == default)
            {
                startMask = bulletControl.startMask;
            }

            if (startRotation == default)
            {
                startRotation = bulletControl.startRotation;
            }

            if (startScale == default)
            {
                startScale = bulletControl.startScale;
            }

            SetBullet(objName,
                bulletControl.typeName,
                bulletControl.layer,
                bulletControl.sprite,
                bulletControl.triggerSize,
                bulletControl.triggerHit,
                bulletControl.triggerOffset,
                startPosition,
                inputBulletColor,
                startMask,
                startRotation,
                startScale,
                bulletControl.triggerFollowMode);
        }

        /// <summary>
        ///     初始化弹幕（循环生成盒状碰撞模式）。
        /// </summary>
        /// <param name="objName">设置弹幕的Obj的名称，以便查找。</param>
        /// <param name="inputTypeName">设置弹幕的种类名称，如果种类名称与当前的弹幕一致，则保留原有的碰撞相关参数，反之清空。</param>
        /// <param name="layer">玩家为100，战斗框边缘为50。可参考。</param>
        /// <param name="sprite">一般在Resources内导入。</param>
        /// <param name="triggerSizes">设置判定箱大小，可设定多个List，但多数情况下需要避免其重叠。（NoFollow情况下设为(0,0)，会自动与sprite大小同步）</param>
        /// <param name="triggerHits">设定碰撞箱伤害，List大小必须与sizes相等。</param>
        /// <param name="triggerOffsets">设定判定箱偏移，List大小必须与sizes相等。</param>
        /// <param name="triggerFollowMode">设置碰撞箱跟随SpriteRenderer缩放的模式。</param>
        /// <param name="startMask">设置Sprite遮罩模式。</param>
        /// <param name="inputBulletColor">设置弹幕属性颜色数据</param>
        /// <param name="startPosition">设置起始位置（相对坐标）。</param>
        /// <param name="startRotation">设置旋转角度，一般只需更改Z轴。</param>
        /// <param name="startScale">若弹幕不需拉伸，StartScale一般设置(1,1,1)。检测到Z为0时会归位到(1,1,1)。</param>
        private void SetBullet(
            string objName,
            string inputTypeName,
            int layer,
            Sprite sprite,
            List<Vector2> triggerSizes,
            List<int> triggerHits,
            List<Vector2> triggerOffsets,
            Vector3 startPosition = new(),
            BattleControl.BulletColor inputBulletColor = BattleControl.BulletColor.White,
            SpriteMaskInteraction startMask = SpriteMaskInteraction.None,
            Vector3 startRotation = new(),
            Vector3 startScale = new(),
            FollowMode triggerFollowMode = FollowMode.NoFollow
        )
        {
            gameObject.name = objName;

            spriteRenderer.sortingOrder = layer;

            bulletColor = inputBulletColor;
            spriteRenderer.color = MainControl.Instance.BattleControl.bulletColorList[(int)bulletColor];

            transform.localPosition = startPosition;

            transform.rotation = Quaternion.Euler(startRotation);

            if (startScale.z == 0)
            {
                startScale = Vector3.one;
            }

            transform.localScale = startScale;

            SetMask(startMask);
            spriteRenderer.sprite = sprite;

            if (typeName != inputTypeName)
            {
                typeName = inputTypeName;
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

            boxColliderSizes = triggerSizes;
            boxHitList = triggerHits;

            followMode = triggerFollowMode;

            //循环生成box碰撞
            for (var i = 0; i < triggerSizes.Count; i++)
            {
                var save = gameObject.AddComponent<BoxCollider2D>();
                save.isTrigger = true;
                if (followMode == FollowMode.NoFollow)
                {
                    save.size = boxColliderSizes[i];
                }
                else
                {
                    save.size = boxColliderList[i].transform.GetComponent<SpriteRenderer>().size - boxColliderSizes[i];
                }

                save.offset = triggerOffsets[i];

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
                new Vector3(r * MathUtilityService.GetRandomUnit(), 0, r * MathUtilityService.GetRandomUnit()), v3Spin, 4,
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
}