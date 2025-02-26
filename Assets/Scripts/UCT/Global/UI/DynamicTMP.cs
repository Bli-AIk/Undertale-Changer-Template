using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Global.UI
{
    public enum DynamicTMPType
    {
        /// <summary>
        /// 无效果
        /// </summary>
        None,
        /// <summary>
        /// 每个字符自行抖动
        /// </summary>
        RandomShake,
        /// <summary>
        /// 随机隔一段时间抖动单个字符
        /// </summary>
        RandomShakeSingle,
        /// <summary>
        /// 所有字符一起抖动
        /// </summary>
        RandomShakeAll,
        /// <summary>
        /// 所有字符抽搐的抖动
        /// </summary>
        CrazyShake,
        /// <summary>
        /// 类似小幽灵的字符抖动
        /// </summary>
        NapShake,
        /// <summary>
        /// 类似小幽灵的字符漂浮
        /// </summary>
        NapFloat,
        Wave, 
        Explode, 
        Bounce 
    }
    
    /// <summary>
    ///     给字体添加各种奇奇怪怪的效果，如变形/位移/抖动。
    /// </summary>
    public class DynamicTMP : MonoBehaviour
    {
        public DynamicTMPType dynamicMode;
        private float _randomStart;
        private TMP_Text _tmp;

        private void Start()
        {
            _tmp = GetComponent<TMP_Text>();
            _randomStart = Random.Range(2, 2.5f);
        }

        private void FixedUpdate()
        {
            if (dynamicMode == DynamicTMPType.None)
            {
                return;
            }

            _tmp.ForceMeshUpdate();

            var textInfo = _tmp.textInfo;

            switch (dynamicMode)
            {
                case DynamicTMPType.RandomShake:
                {
                    for (var i = 0; i < textInfo.characterCount; i++)
                    {
                        var charInfo = textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                        {
                            continue;
                        }

                        var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                        var random = new Vector3(Random.Range(-0.025f, 0.025f), Random.Range(-0.025f, 0.025f), 0);
                        for (var j = 0; j < 4; j++)
                        {
                            var orig = verts[charInfo.vertexIndex + j];
                            //动画
                            verts[charInfo.vertexIndex + j] = orig + random;
                        }
                    }

                    break;
                }

                case DynamicTMPType.RandomShakeSingle:
                {
                    var random = Random.Range(0, 120);
                    if (random == 0)
                    {
                        var j = Random.Range(0, textInfo.characterCount);
                        var charInfo = textInfo.characterInfo[j];
                        if (charInfo.isVisible)
                        {
                            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                            var randomVector = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
                            for (var i = 0; i < 4; i++)
                            {
                                var orig = verts[charInfo.vertexIndex + i];
                                //动画
                                verts[charInfo.vertexIndex + i] = orig + randomVector;
                            }
                        }
                    }

                    break;
                }

                case DynamicTMPType.RandomShakeAll:
                {
                    var random = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);
                    for (var i = 0; i < textInfo.characterCount; i++)
                    {
                        var charInfo = textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                        {
                            continue;
                        }

                        var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                        for (var j = 0; j < 4; j++)
                        {
                            var orig = verts[charInfo.vertexIndex + j];
                            //动画
                            verts[charInfo.vertexIndex + j] = orig + random;
                        }
                    }

                    break;
                }

                case DynamicTMPType.CrazyShake: 
                {
                    for (var i = 0; i < textInfo.characterCount; i++)
                    {
                        var charInfo = textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                        {
                            continue;
                        }

                        var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                        for (var j = 0; j < 4; j++)
                        {
                            var orig = verts[charInfo.vertexIndex + j];
                            //动画
                            verts[charInfo.vertexIndex + j] = orig + new Vector3(0,
                                0.025f * Mathf.Sin(Random.Range(1, 2.5f) * Time.time + orig.x * 0.45f), 0);

                            orig = verts[charInfo.vertexIndex + j];
                            if (Random.Range(0, 5) != 0)
                            {
                                continue;
                            }

                            verts[charInfo.vertexIndex + j] = orig + new Vector3(Random.Range(-0.05f, 0.05f),
                                Random.Range(-0.05f, 0.05f), 0);
                        }
                    }

                    break;
                }

                case DynamicTMPType.NapShake: 
                {
                    for (var i = 0; i < textInfo.characterCount; i++)
                    {
                        var charInfo = textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                        {
                            continue;
                        }

                        var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                        for (var j = 0; j < 4; j++)
                        {
                            var orig = verts[charInfo.vertexIndex + j];
                            //动画
                            verts[charInfo.vertexIndex + j] = orig + new Vector3(0,
                                0.05f * Mathf.Cos(_randomStart * Time.time + orig.x * 0.45f), 0);
                            orig = verts[charInfo.vertexIndex + j];
                            verts[charInfo.vertexIndex + j] = orig + new Vector3(Random.Range(-0.01f, 0.01f),
                                Random.Range(-0.01f, 0.01f), 0);
                        }
                    }

                    break;
                }

                case DynamicTMPType.NapFloat:
                {
                    for (var i = 0; i < textInfo.characterCount; i++)
                    {
                        var charInfo = textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                        {
                            continue;
                        }

                        var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                        for (var j = 0; j < 4; j++)
                        {
                            var orig = verts[charInfo.vertexIndex + j];
                            verts[charInfo.vertexIndex + j] = orig + new Vector3(0,
                                0.05f * Mathf.Sin(_randomStart * Time.time + orig.x * 0.45f), 0);
                        }
                    }

                    break;
                }

                case DynamicTMPType.Wave:
                {
                    for (var i = 0; i < textInfo.characterCount; i++)
                    {
                        var charInfo = textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                        {
                            continue;
                        }

                        var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                        var waveOffset = new Vector3(0, Mathf.Sin(Time.time * 2f + i * 0.2f) * 0.05f, 0);
                        for (var j = 0; j < 4; j++)
                        {
                            var orig = verts[charInfo.vertexIndex + j];
                            verts[charInfo.vertexIndex + j] = orig + waveOffset;
                        }
                    }

                    break;
                }

                case DynamicTMPType.Explode:
                {
                    var center = new Vector3(0, 0, 0);
                    for (var i = 0; i < textInfo.characterCount; i++)
                    {
                        var charInfo = textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                        {
                            continue;
                        }

                        var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                        var explodeOffset = (verts[charInfo.vertexIndex] - center) * (Mathf.Sin(Time.time * 2f) * 0.1f);
                        for (var j = 0; j < 4; j++)
                        {
                            var orig = verts[charInfo.vertexIndex + j];
                            verts[charInfo.vertexIndex + j] = orig + explodeOffset;
                        }
                    }

                    break;
                }

                case DynamicTMPType.Bounce:
                {
                    for (var i = 0; i < textInfo.characterCount; i++)
                    {
                        var charInfo = textInfo.characterInfo[i];
                        if (!charInfo.isVisible)
                        {
                            continue;
                        }

                        var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                        var bounceOffset = new Vector3(0, Mathf.Abs(Mathf.Sin(Time.time * 2f + i * 0.1f)) * 0.1f, 0);
                        for (var j = 0; j < 4; j++)
                        {
                            var orig = verts[charInfo.vertexIndex + j];
                            verts[charInfo.vertexIndex + j] = orig + bounceOffset;
                        }
                    }

                    break;
                }
                case DynamicTMPType.None:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            for (var i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                _tmp.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}