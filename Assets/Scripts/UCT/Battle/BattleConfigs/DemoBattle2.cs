using System.Collections.Generic;
using DG.Tweening;
using MEC;
using UCT.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace UCT.Battle.BattleConfigs
{
    public class DemoBattle2 : IBattleConfig
    {
        public GameObject[] enemies { get; } =
        {
            Resources.Load<GameObject>("Prefabs/Enemies/NPC1")
        };

        public GameObject backGroundModel => Resources.Load<GameObject>("Prefabs/BackGround/Vaporwave");

        public Material skyBox => Resources.Load<Material>("SkyBox/Sunset");

        public VolumeProfile volumeProfile =>
            Resources.Load<VolumeProfile>("Volume/Vaporwave3DVolume");

        public AudioClip bgmClip => Resources.Load<AudioClip>("Audios/far-away-lo-fi-hip-hop-background-music-151495");
        public float volume => 0.5f;
        public float pitch => 0.5f;

        public IEnumerator<float> Turn(int turnNumber, ObjectPool bulletPool)
        {
            switch (turnNumber)
            {
                case 0: //示例回合
                {
                    Debug.Log("这是另一个示例回合");
                    yield return Timing.WaitForSeconds(0.5f);
                    Debug.Log("我就随便赛点东西得了");
                    yield return Timing.WaitForSeconds(1.5f);

                    Debug.Log("战斗框缩放：更改四个点的坐标");
                    for (var i = 0; i < 5; i++)
                    {
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


                        Debug.Log("战斗框缩放回初始坐标");
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
                    }

                    break;
                }
                default:
                    break;
            }
        }
    }
}