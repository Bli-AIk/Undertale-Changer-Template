using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volume
{
    public class CRTScreenRendererFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public Shader shader;
        }

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

        private CRTScreenComponent _crtScreenVolume;
        private Material _mat;
        private RenderTargetIdentifier _currentTarget;

        public CrtScreenPass(RenderPassEvent passEvent, Shader crtScreenShader)
        {
            renderPassEvent = passEvent;
            if (crtScreenShader == null)
            {
                UCT.Global.Other.Debug.Log("Shader不存在");
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
            if (_mat == null)
            {
                return;
            }
            if (!renderingData.cameraData.postProcessEnabled)
            {
                return;
            }
            var stack = VolumeManager.instance.stack;
            _crtScreenVolume = stack.GetComponent<CRTScreenComponent>();
            if (_crtScreenVolume == null)
            {
                return;
            }
            if (_crtScreenVolume.isShow.value == false)
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

            _mat.SetVector(Resolution1, _crtScreenVolume.resolution.value);
            _mat.SetVector(PixelScanlineBrightness, _crtScreenVolume.pixelScanlineBrightness.value);
            _mat.SetFloat(Speed, _crtScreenVolume.speed.value);

            cmd.SetGlobalTexture(MainTexId, source);
            cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, _mat, 0);
        }
    }
}