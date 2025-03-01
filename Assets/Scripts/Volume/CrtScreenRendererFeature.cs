using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volume
{
    public class CrtScreenRendererFeature : ScriptableRendererFeature
    {
        public Settings settings = new();
        private CrtScreenPass _pass;

        public override void Create()
        {
            name = "CRTScreenPass";
            _pass = new CrtScreenPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            _pass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(_pass);
        }

        [Serializable]
        public class Settings
        {
            public Shader shader;
        }
    }

    [Serializable]
    public class CrtScreenPass : ScriptableRenderPass
    {
        private const string RenderTag = "CRTScreen Effects";
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");
        private static readonly int Resolution1 = Shader.PropertyToID("_Resolution");
        private static readonly int PixelScanlineBrightness = Shader.PropertyToID("_PixelScanlineBrightness");
        private static readonly int Speed = Shader.PropertyToID("_Speed");

        private CrtScreenComponent _crtScreenVolume;
        private RenderTargetIdentifier _currentTarget;
        private Material _mat;

        public CrtScreenPass(RenderPassEvent passEvent, Shader crtScreenShader)
        {
            renderPassEvent = passEvent;
            if (!crtScreenShader)
            {
                UCT.Other.Debug.Log("Shader不存在");
                return;
            }

            _mat = CoreUtils.CreateEngineMaterial(crtScreenShader);
        }

        public void Setup(in RenderTargetIdentifier currentTarget)
        {
            _currentTarget = currentTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!_mat)
            {
                return;
            }

            if (!renderingData.cameraData.postProcessEnabled)
            {
                return;
            }

            var stack = VolumeManager.instance.stack;
            _crtScreenVolume = stack.GetComponent<CrtScreenComponent>();
            if (!_crtScreenVolume)
            {
                return;
            }

            if (!_crtScreenVolume.isShow.value)
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
            var source = _currentTarget;
            var destination = TempTargetId;

            _mat.SetVector(Resolution1, _crtScreenVolume.resolution.value);
            _mat.SetVector(PixelScanlineBrightness, _crtScreenVolume.pixelScanlineBrightness.value);
            _mat.SetFloat(Speed, _crtScreenVolume.speed.value);

            cmd.SetGlobalTexture(MainTexId, source);
            cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0,
                FilterMode.Trilinear, RenderTextureFormat.Default);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, _mat, 0);
        }
    }
}