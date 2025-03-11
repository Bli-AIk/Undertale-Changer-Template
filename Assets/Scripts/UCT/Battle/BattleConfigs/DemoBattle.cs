using System.Collections.Generic;
using DG.Tweening;
using MEC;
using UCT.Control;
using UCT.Core;
using UCT.Service;
using UnityEngine;
using UnityEngine.Rendering;

namespace UCT.Battle.BattleConfigs
{
    public class DemoBattle : IBattleConfig
    {
        public GameObject[] enemies { get; } =
        {
            Resources.Load<GameObject>("Prefabs/Enemies/NPC1"),
            Resources.Load<GameObject>("Prefabs/Enemies/NPC2")
        };

        public GameObject backGroundModel => Resources.Load<GameObject>("Prefabs/BackGround/Vaporwave");

        public Material skyBox => Resources.Load<Material>("SkyBox/Vaporwave");

        public VolumeProfile volumeProfile =>
            Resources.Load<VolumeProfile>("Volume/Vaporwave3DVolume");

        public AudioClip bgmClip => Resources.Load<AudioClip>("Audios/stranger-things-124008");
        public float volume => 0.5f;
        public float pitch => 0.5f;

        public IEnumerator<float> Turn(int turnNumber, ObjectPool bulletPool)
        {
            switch (turnNumber)
            {
                case 0: //示例回合
                {
                    Debug.Log("这是一个示例回合");
                    yield return Timing.WaitForSeconds(0.5f);
                    Debug.Log("请注意查看控制台发出的Debug文本介绍");
                    yield return Timing.WaitForSeconds(1.5f);

                    Debug.Log("战斗框缩放：更改四个点的坐标");

                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[0],
                            x => MainControl.Instance.mainBox.vertexPoints[0] = x,
                            new Vector2(1.4f, MainControl.Instance.mainBox.vertexPoints[0].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[1],
                            x => MainControl.Instance.mainBox.vertexPoints[1] = x,
                            new Vector2(1.4f, MainControl.Instance.mainBox.vertexPoints[1].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[2],
                            x => MainControl.Instance.mainBox.vertexPoints[2] = x,
                            new Vector2(-1.4f, MainControl.Instance.mainBox.vertexPoints[2].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[3],
                            x => MainControl.Instance.mainBox.vertexPoints[3] = x,
                            new Vector2(-1.4f, MainControl.Instance.mainBox.vertexPoints[3].y), 0.5f)
                        .SetEase(Ease.InOutSine);


                    yield return Timing.WaitForSeconds(1);

                    Debug.Log("通过更改点坐标实现的战斗框轴点旋转");
                    for (var i = 0; i < 4; i++)
                    {
                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[0],
                            x => MainControl.Instance.mainBox.vertexPoints[0] = x,
                            MainControl.Instance.mainBox.vertexPoints[3], 0.5f).SetEase(Ease.InOutSine);
                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[1],
                            x => MainControl.Instance.mainBox.vertexPoints[1] = x,
                            MainControl.Instance.mainBox.vertexPoints[0], 0.5f).SetEase(Ease.InOutSine);
                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[2],
                            x => MainControl.Instance.mainBox.vertexPoints[2] = x,
                            MainControl.Instance.mainBox.vertexPoints[1], 0.5f).SetEase(Ease.InOutSine);
                        DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[3],
                            x => MainControl.Instance.mainBox.vertexPoints[3] = x,
                            MainControl.Instance.mainBox.vertexPoints[2], 0.5f).SetEase(Ease.InOutSine);

                        yield return Timing.WaitForSeconds(0.5f);
                    }

                    Debug.Log("简单嵌套弹幕编写示例");
                    for (var i = 0; i < 25; i++)
                    {
                        Timing.RunCoroutine(_SimpleNestBullet(bulletPool));
                        yield return Timing.WaitForSeconds(0.2f);
                    }

                    yield return Timing.WaitForSeconds(2f);

                    Debug.Log("等待嵌套播放完毕的嵌套弹幕编写示例");
                    for (var i = 0; i < 3; i++)
                    {
                        yield return Timing.WaitUntilDone(Timing.RunCoroutine(_SimpleNestBullet(bulletPool)));
                    }

                    Debug.Log("战斗框缩放回初始坐标以结束回合");
                    yield return Timing.WaitForSeconds(1f);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[0],
                            x => MainControl.Instance.mainBox.vertexPoints[0] = x,
                            new Vector2(5.93f, MainControl.Instance.mainBox.vertexPoints[0].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[1],
                            x => MainControl.Instance.mainBox.vertexPoints[1] = x,
                            new Vector2(5.93f, MainControl.Instance.mainBox.vertexPoints[1].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[2],
                            x => MainControl.Instance.mainBox.vertexPoints[2] = x,
                            new Vector2(-5.93f, MainControl.Instance.mainBox.vertexPoints[2].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    DOTween.To(() => MainControl.Instance.mainBox.vertexPoints[3],
                            x => MainControl.Instance.mainBox.vertexPoints[3] = x,
                            new Vector2(-5.93f, MainControl.Instance.mainBox.vertexPoints[3].y), 0.5f)
                        .SetEase(Ease.InOutSine);
                    yield return Timing.WaitForSeconds(0.5f);

                    break;
                }
                case 1:
                {
                    Debug.Log("这是个测试回合。");
                    const string cupCake = "CupCake";

                    var obj = bulletPool.GetFromPool<BulletController>();
                    obj.SetBullet(cupCake, cupCake, new InitialTransform(new Vector3(1, -1.6f)),
                        (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);

                    var obj2 = bulletPool.GetFromPool<BulletController>();
                    obj2.SetBullet(cupCake, cupCake, new InitialTransform(new Vector3(-1, -1.6f)),
                        (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);


                    for (var i = 10; i > 0; i--)
                    {
                        if (i % 5 == 0)
                        {
                            Debug.Log($"{TextProcessingService.RandomStringColor(i.ToString())}秒后结束本回合");
                        }

                        yield return Timing.WaitForSeconds(1f);
                    }

                    bulletPool.ReturnPool(obj.gameObject, obj);

                    bulletPool.ReturnPool(obj2.gameObject, obj2);
                    break;
                }
                default:
                    break;
            }
        }

        private static IEnumerator<float> _SimpleNestBullet(ObjectPool bulletPool)
        {
            var obj = bulletPool.GetFromPool<BulletController>();
            const string cupCake = "CupCake";

            obj.SetBullet(cupCake, cupCake, new InitialTransform(new Vector3(0, -3.35f)),
                (BattleControl.BulletColor)Random.Range(0, 3), SpriteMaskInteraction.VisibleInsideMask);

            obj.transform.localPosition += new Vector3(Random.Range(-0.5f, 0.5f), 0);

            obj.transform.DOMoveY(0, 1).SetEase(Ease.OutSine).SetLoops(2, LoopType.Yoyo);

            obj.transform.DORotate(new Vector3(0, 0, 360), 2, RotateMode.WorldAxisAdd).SetEase(Ease.InOutSine);
            yield return Timing.WaitForSeconds(0.5f);

            obj.spriteRenderer.sortingOrder = 60;
            obj.SetMask(SpriteMaskInteraction.None);

            yield return Timing.WaitForSeconds(1f);

            obj.spriteRenderer.sortingOrder = 40;
            obj.SetMask(SpriteMaskInteraction.VisibleInsideMask);

            yield return Timing.WaitForSeconds(1f);

            bulletPool.ReturnPool(obj.gameObject, obj);
        }
    }
}