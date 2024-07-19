using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// VolumeComponent，显示在添加列表内
/// </summary>

[VolumeComponentMenuForRenderPipeline("Custom/Glitch Art", typeof(UniversalRenderPipeline))]
public class GlitchArtComponent : VolumeComponent, IPostProcessComponent
{
    public BoolParameter isShow = new BoolParameter(false, true);

    [Header("AnalogGlitch")]
    public BoolParameter analogGlitchMode = new BoolParameter(true, true);

    public Vector2Parameter scanLineJitter = new Vector2Parameter(new Vector2(0.01f, 0.975f), true);

    [Header("HorizontalShake")]
    public BoolParameter horizontalShakeMode = new BoolParameter(true, true);

    public FloatParameter horizontalShake = new FloatParameter(0.001f, true);

    [Header("ColorDrift")]
    public BoolParameter colorDriftMode = new BoolParameter(true, true);

    public FloatParameter colorDrift = new FloatParameter(0.25f, true);

    [Header("VerticalJump")]
    public BoolParameter verticalJumpMode = new BoolParameter(true, true);

    public FloatParameter verticalJump = new FloatParameter(0.15f, true);

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}