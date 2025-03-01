using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volume
{
    /// <summary>
    ///     VolumeComponent，显示在添加列表内
    /// </summary>
    [VolumeComponentMenuForRenderPipeline("Custom/CRT Screen", typeof(UniversalRenderPipeline))]
    public class CrtScreenComponent : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter isShow = new(false, true);

        [Header("Settings")]
        public Vector2Parameter resolution = new(new Vector2(1000, 1000), true);

        public Vector4Parameter pixelScanlineBrightness = new(new Vector4(0.225f, 0.85f, 0.05f, 0.95f), true);
        public FloatParameter speed = new(1, true);

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