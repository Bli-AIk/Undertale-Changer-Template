using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volume
{
    /// <summary>
    /// VolumeComponent，显示在添加列表内
    /// </summary>

    [VolumeComponentMenuForRenderPipeline("Custom/Glitch Art", typeof(UniversalRenderPipeline))]
    public class GlitchArtComponent : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter isShow = new(false, true);

        [Header("AnalogGlitch")]
        public BoolParameter analogGlitchMode = new(true, true);

        public Vector2Parameter scanLineJitter = new(new Vector2(0.01f, 0.975f), true);

        [Header("HorizontalShake")]
        public BoolParameter horizontalShakeMode = new(true, true);

        public FloatParameter horizontalShake = new(0.001f, true);

        [Header("ColorDrift")]
        public BoolParameter colorDriftMode = new(true, true);

        public FloatParameter colorDrift = new(0.25f, true);

        [Header("VerticalJump")]
        public BoolParameter verticalJumpMode = new(true, true);

        public FloatParameter verticalJump = new(0.15f, true);

        public bool IsActive()
        {
            return true;
        }

        public bool IsTileCompatible()
        {
            return false;
        }
    }
}