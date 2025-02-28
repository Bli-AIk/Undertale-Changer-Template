using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace UCT.Global.UI
{
    public enum DynamicTmpType
    {
        None,
        RandomShake,
        RandomShakeSingle,
        RandomShakeAll,
        CrazyShake,
        NapShake,
        NapFloat,
        Wave,
        Explode,
        Bounce
    }

    /// <summary>
    ///     Applies various dynamic effects to TextMeshPro text components.
    /// </summary>
    public class DynamicTmp : MonoBehaviour
    {
        private const int SingleShakeProbability = 120;
        private const float MaxShakeIntensity = 0.05f;
        private const float BaseFrequency = 2.5f;

        [FormerlySerializedAs("dynamicMode")] public DynamicTmpType effectType = DynamicTmpType.None;

        private float _initialRandomOffset;

        private TMP_Text _textMeshPro;

        private void Start()
        {
            _textMeshPro = GetComponent<TMP_Text>();
            _initialRandomOffset = Random.Range(2f, 2.5f);
        }

        private void FixedUpdate()
        {
            if (effectType == DynamicTmpType.None)
            {
                return;
            }

            _textMeshPro.ForceMeshUpdate();
            var textInfo = _textMeshPro.textInfo;

            switch (effectType)
            {
                case DynamicTmpType.RandomShake:
                    ApplyRandomShake(textInfo);
                    break;
                case DynamicTmpType.RandomShakeSingle:
                    ApplyRandomSingleShake(textInfo);
                    break;
                case DynamicTmpType.RandomShakeAll:
                    ApplyRandomShakeAll(textInfo);
                    break;
                case DynamicTmpType.CrazyShake:
                    ApplyCrazyShake(textInfo);
                    break;
                case DynamicTmpType.NapShake:
                    ApplyNapShake(textInfo);
                    break;
                case DynamicTmpType.NapFloat:
                    ApplyNapFloat(textInfo);
                    break;
                case DynamicTmpType.Wave:
                    ApplyWaveEffect(textInfo);
                    break;
                case DynamicTmpType.Explode:
                    ApplyExplodeEffect(textInfo);
                    break;
                case DynamicTmpType.Bounce:
                    ApplyBounceEffect(textInfo);
                    break;
                case DynamicTmpType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected EffectType value: {effectType}");
            }

            UpdateAllMeshes(textInfo);
        }

        private void UpdateAllMeshes(TMP_TextInfo textInfo)
        {
            for (var i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                _textMeshPro.UpdateGeometry(meshInfo.mesh, i);
            }
        }

        private static void ForEachVisibleCharacter(TMP_TextInfo textInfo, Action<TMP_CharacterInfo, Vector3[]> action)
        {
            for (var i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                {
                    continue;
                }

                var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                action(charInfo, vertices);
            }
        }

        private static void ApplyRandomShake(TMP_TextInfo textInfo)
        {
            ForEachVisibleCharacter(textInfo, (charInfo, vertices) =>
            {
                var randomOffset = new Vector3(
                    Random.Range(-MaxShakeIntensity, MaxShakeIntensity),
                    Random.Range(-MaxShakeIntensity, MaxShakeIntensity),
                    0);

                ApplyVertexOffset(charInfo, vertices, randomOffset);
            });
        }

        private static void ApplyRandomSingleShake(TMP_TextInfo textInfo)
        {
            if (Random.Range(0, SingleShakeProbability) != 0)
            {
                return;
            }

            var randomIndex = Random.Range(0, textInfo.characterCount);
            var charInfo = textInfo.characterInfo[randomIndex];
            if (!charInfo.isVisible)
            {
                return;
            }

            var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            var randomOffset = new Vector3(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f),
                0);

            ApplyVertexOffset(charInfo, vertices, randomOffset);
        }

        private static void ApplyRandomShakeAll(TMP_TextInfo textInfo)
        {
            var sharedOffset = new Vector3(
                Random.Range(-MaxShakeIntensity, MaxShakeIntensity),
                Random.Range(-MaxShakeIntensity, MaxShakeIntensity),
                0);

            ForEachVisibleCharacter(textInfo,
                (charInfo, vertices) => { ApplyVertexOffset(charInfo, vertices, sharedOffset); });
        }

        private static void ApplyCrazyShake(TMP_TextInfo textInfo)
        {
            ForEachVisibleCharacter(textInfo, (charInfo, vertices) =>
            {
                for (var j = 0; j < 4; j++)
                {
                    var orig = vertices[charInfo.vertexIndex + j];
                    var sinOffset = new Vector3(
                        0f,
                        0.025f * Mathf.Sin(Random.Range(1f, BaseFrequency) * Time.time + orig.x * 0.45f),
                        0f);

                    vertices[charInfo.vertexIndex + j] = orig + sinOffset;

                    if (Random.Range(0, 5) == 0)
                    {
                        vertices[charInfo.vertexIndex + j] += new Vector3(
                            Random.Range(-MaxShakeIntensity, MaxShakeIntensity),
                            Random.Range(-MaxShakeIntensity, MaxShakeIntensity),
                            0f);
                    }
                }
            });
        }

        private void ApplyNapShake(TMP_TextInfo textInfo)
        {
            ForEachVisibleCharacter(textInfo, (charInfo, vertices) =>
            {
                for (var j = 0; j < 4; j++)
                {
                    var orig = vertices[charInfo.vertexIndex + j];
                    var cosOffset = new Vector3(
                        0f,
                        0.05f * Mathf.Cos(_initialRandomOffset * Time.time + orig.x * 0.45f),
                        0f);

                    var randomOffset = new Vector3(
                        Random.Range(-0.01f, 0.01f),
                        Random.Range(-0.01f, 0.01f),
                        0f);

                    vertices[charInfo.vertexIndex + j] = orig + cosOffset + randomOffset;
                }
            });
        }

        private void ApplyNapFloat(TMP_TextInfo textInfo)
        {
            ForEachVisibleCharacter(textInfo, (charInfo, vertices) =>
            {
                for (var j = 0; j < 4; j++)
                {
                    var orig = vertices[charInfo.vertexIndex + j];
                    var sinOffset = new Vector3(
                        0f,
                        0.05f * Mathf.Sin(_initialRandomOffset * Time.time + orig.x * 0.45f),
                        0f);

                    vertices[charInfo.vertexIndex + j] = orig + sinOffset;
                }
            });
        }

        private static void ApplyWaveEffect(TMP_TextInfo textInfo)
        {
            ForEachVisibleCharacter(textInfo, (charInfo, vertices) =>
            {
                var waveOffset = new Vector3(
                    0f,
                    Mathf.Sin(Time.time * 2f + charInfo.vertexIndex * 0.2f) * MaxShakeIntensity,
                    0f);

                ApplyVertexOffset(charInfo, vertices, waveOffset);
            });
        }

        private static void ApplyExplodeEffect(TMP_TextInfo textInfo)
        {
            var center = Vector3.zero;
            ForEachVisibleCharacter(textInfo, (charInfo, vertices) =>
            {
                var basePosition = vertices[charInfo.vertexIndex];
                var explodeOffset = (basePosition - center) * (Mathf.Sin(Time.time * 2f) * 0.1f);
                ApplyVertexOffset(charInfo, vertices, explodeOffset);
            });
        }

        private static void ApplyBounceEffect(TMP_TextInfo textInfo)
        {
            ForEachVisibleCharacter(textInfo, (charInfo, vertices) =>
            {
                var bounceOffset = new Vector3(
                    0f,
                    Mathf.Abs(Mathf.Sin(Time.time * 2f + charInfo.vertexIndex * 0.1f)) * 0.1f,
                    0f);

                ApplyVertexOffset(charInfo, vertices, bounceOffset);
            });
        }

        private static void ApplyVertexOffset(TMP_CharacterInfo charInfo, Vector3[] vertices, Vector3 offset)
        {
            for (var j = 0; j < 4; j++)
            {
                vertices[charInfo.vertexIndex + j] += offset;
            }
        }
    }
}