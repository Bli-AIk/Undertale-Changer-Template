using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
/// <summary>
/// VolumeComponent, displayed in the add list
/// </summary>

[VolumeComponentMenuForRenderPipeline("Custom/CRT Screen", typeof(UniversalRenderPipeline))]
public class CRTScreenComponent : VolumeComponent, IPostProcessComponent
{
    public BoolParameter isShow = new BoolParameter(false, true);
    [Header("Settings")]
    public Vector2Parameter resolution = new Vector2Parameter(new Vector2(1000, 1000), true);
    public Vector4Parameter pixelScanlineBrightness = new Vector4Parameter(new Vector4(0.225f, 0.85f, 0.05f, 0.95f), true);
    public FloatParameter speed = new FloatParameter(1, true);


    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return false;
    }

}
