using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UCT.Control;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Global.Core
{
    public static class TypeWritterDynamicController
    {
        public static IEnumerator DynamicTmp(TypeWritter typeWritter,
            TMP_Text tmpText,
            int number,
            OverworldControl.DynamicType inputDynamicType)
        {
            if (inputDynamicType == OverworldControl.DynamicType.None)
            {
                yield return 0;
                tmpText.ForceMeshUpdate();
                yield break;
            }

            var textInfo = tmpText.textInfo;
            tmpText.ForceMeshUpdate(); // 提前更新文本信息，减少重复调用

            var charInfo = textInfo.characterInfo[number];
            if (!charInfo.isVisible)
            {
                yield return 0;
                tmpText.ForceMeshUpdate();
                yield break;
            }

            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            yield return inputDynamicType switch
            {
                OverworldControl.DynamicType.Shake => ApplyShakeEffect(typeWritter, tmpText, textInfo, charInfo),
                OverworldControl.DynamicType.Fade => ApplyFadeEffect(typeWritter, tmpText, textInfo, charInfo),
                OverworldControl.DynamicType.Up => ApplyUpEffect(typeWritter, tmpText, textInfo, charInfo),
                _ => throw new ArgumentOutOfRangeException(nameof(inputDynamicType), inputDynamicType, null)
            };

            tmpText.ForceMeshUpdate(); // 确保动画结束后文本最终更新
        }

        private static IEnumerator ApplyShakeEffect(TypeWritter typeWritter,
            TMP_Text tmpText,
            TMP_TextInfo textInfo,
            TMP_CharacterInfo charInfo)
        {
            const int iterations = 30;

            for (var i = 0; i < iterations; i++)
            {
                if (typeWritter.isSkip || typeWritter.isJumpingText)
                {
                    break;
                }

                var offset = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);
                ApplyVertexOffset(textInfo, charInfo, offset);
                UpdateTextMesh(tmpText, textInfo);

                yield return 0;
            }
        }

        private static IEnumerator ApplyFadeEffect(TypeWritter typeWritter,
            TMP_Text tmpText,
            TMP_TextInfo textInfo,
            TMP_CharacterInfo charInfo)
        {
            const float fadeDuration = 0.1f;
            var elapsedTime = 0f;

            var startColor = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32[charInfo.vertexIndex];
            var transparentColor = new Color32(startColor.r, startColor.g, startColor.b, 0);
            var targetColor = new Color32(startColor.r, startColor.g, startColor.b, 255);

            ApplyVertexColor(textInfo, charInfo, transparentColor);
            tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            while (elapsedTime < fadeDuration)
            {
                if (typeWritter.isSkip || typeWritter.isJumpingText)
                {
                    break;
                }

                elapsedTime += Time.deltaTime;
                var alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                var currentColor = Color32.Lerp(transparentColor, targetColor, alpha);

                ApplyVertexColor(textInfo, charInfo, currentColor);
                tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                yield return 0;
            }
        }

        private static IEnumerator<float> ApplyUpEffect(TypeWritter typeWritter,
            TMP_Text tmpText,
            TMP_TextInfo textInfo,
            TMP_CharacterInfo charInfo)
        {
            const int iterations = 30;
            var direction = new Vector3(0, -0.1f, 0);

            for (var i = 0; i < iterations; i++)
            {
                if (typeWritter.isSkip || typeWritter.isJumpingText)
                {
                    break;
                }

                var progress = 1 - (float)i / iterations;
                ApplyVertexOffset(textInfo, charInfo, direction * progress);
                UpdateTextMesh(tmpText, textInfo);

                yield return 0;
            }
        }

        private static void ApplyVertexOffset(TMP_TextInfo textInfo, TMP_CharacterInfo charInfo, Vector3 offset)
        {
            var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            for (var j = 0; j < 4; j++)
            {
                vertices[charInfo.vertexIndex + j] += offset;
            }
        }

        private static void ApplyVertexColor(TMP_TextInfo textInfo, TMP_CharacterInfo charInfo, Color32 color)
        {
            var colors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;

            for (var j = 0; j < 4; j++)
            {
                colors[charInfo.vertexIndex + j] = color;
            }
        }

        private static void UpdateTextMesh(TMP_Text tmpText, TMP_TextInfo textInfo)
        {
            for (var i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                tmpText.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}