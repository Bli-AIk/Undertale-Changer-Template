using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volume
{
    /// <summary>
    /// VolumeComponent，显示在添加列表内
    /// </summary>

    [VolumeComponentMenuForRenderPipeline("Custom/Stretch Post", typeof(UniversalRenderPipeline))]
    public class StretchPostComponent : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter isShow = new(false, true);

        [Header("Settings")]
        public Vector2Parameter draw = new(new Vector2(), true);

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
