using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// VolumeComponent����ʾ������б���
/// </summary>

[VolumeComponentMenuForRenderPipeline("Custom/Stretch Post", typeof(UniversalRenderPipeline))]
public class StretchPostComponent : VolumeComponent, IPostProcessComponent
{
    public BoolParameter isShow = new BoolParameter(false, true);

    [Header("Settings")]
    public Vector2Parameter draw = new Vector2Parameter(new Vector2(), true);

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}