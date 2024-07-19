using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// VolumeComponent，显示在添加列表内
/// </summary>

[VolumeComponentMenuForRenderPipeline("Custom/Chromatic Aberration", typeof(UniversalRenderPipeline))]
public class ChromaticAberrationComponent : VolumeComponent, IPostProcessComponent
{
    public BoolParameter isShow = new BoolParameter(false, true);

    [Header("Settings")]
    public FloatParameter offset = new FloatParameter(0.02f, true);

    public FloatParameter speed = new FloatParameter(10, true);
    public FloatParameter height = new FloatParameter(0.15f, true);
    public BoolParameter onlyOri = new BoolParameter(false, true);

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}