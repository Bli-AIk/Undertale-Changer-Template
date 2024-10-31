using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volume
{
    public class StretchPostRendererFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public Shader shader;
        }

        public Settings settings = new();
        private StretchPostPass _pass;

        public override void Create()
        {
            name = "StretchPostPass";
            _pass = new StretchPostPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            _pass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(_pass);
        }
    }

    [Serializable]
    public class StretchPostPass : ScriptableRenderPass
    {
        private const string RenderTag = "StretchPost Effects";
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");
        private static readonly int Draw = Shader.PropertyToID("_Draw");

        private StretchPostComponent _stretchPostVolume;
        private Material _mat;
        private RenderTargetIdentifier _currentTarget;

        public StretchPostPass(RenderPassEvent passEvent, Shader stretchPostShader)
        {
            renderPassEvent = passEvent;
            if (stretchPostShader == null)
            {
                UCT.Global.Other.Debug.Log("Shader不存在");
                return;
            }
            _mat = CoreUtils.CreateEngineMaterial(stretchPostShader);
        }

        public void Setup(in RenderTargetIdentifier currentTarget)
        {
            _currentTarget = currentTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_mat == null)
            {
                return;
            }
            if (!renderingData.cameraData.postProcessEnabled)
            {
                return;
            }
            var stack = VolumeManager.instance.stack;
            _stretchPostVolume = stack.GetComponent<StretchPostComponent>();
            if (_stretchPostVolume == null)
            {
                return;
            }
            if (_stretchPostVolume.isShow.value == false)
            {
                return;
            }
            var cmd = CommandBufferPool.Get(RenderTag);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;
            var source = _currentTarget;
            var destination = TempTargetId;

            _mat.SetVector(Draw, _stretchPostVolume.draw.value);

            cmd.SetGlobalTexture(MainTexId, source);
            cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, _mat, 0);
        }
    }
}