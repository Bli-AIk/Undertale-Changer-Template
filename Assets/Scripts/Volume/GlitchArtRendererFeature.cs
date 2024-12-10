using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volume
{
    public class GlitchArtRendererFeature : ScriptableRendererFeature
    {
        public Settings settings = new();
        private GlitchArtPass _pass;

        public override void Create()
        {
            name = "GlitchArtPass";
            _pass = new GlitchArtPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader);
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
    public class GlitchArtPass : ScriptableRenderPass
    {
        private static readonly string RenderTag = "GlitchArt Effects";
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");
        private static readonly int AnalogGlitchMode = Shader.PropertyToID("_AnalogGlitchMode");
        private static readonly int ScanLineJitter = Shader.PropertyToID("_ScanLineJitter");
        private static readonly int HorizontalShakeMode = Shader.PropertyToID("_HorizontalShakeMode");
        private static readonly int HorizontalShake = Shader.PropertyToID("_HorizontalShake");
        private static readonly int ColorDriftMode = Shader.PropertyToID("_ColorDriftMode");
        private static readonly int ColorDrift = Shader.PropertyToID("_ColorDrift");
        private static readonly int VerticalJumpMode = Shader.PropertyToID("_VerticalJumpMode");
        private static readonly int VerticalJump = Shader.PropertyToID("_VerticalJump");
        private RenderTargetIdentifier _currentTarget;

        private GlitchArtComponent _glitchArtVolume;
        private Material _mat;

        public GlitchArtPass(RenderPassEvent passEvent, Shader glitchArtShader)
        {
            renderPassEvent = passEvent;
            if (glitchArtShader == null)
            {
                UCT.Other.Debug.Log("Shader不存在");
                return;
            }

            _mat = CoreUtils.CreateEngineMaterial(glitchArtShader);
        }

        public void Setup(in RenderTargetIdentifier currentTarget)
        {
            _currentTarget = currentTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_mat == null) return;
            if (!renderingData.cameraData.postProcessEnabled) return;
            var stack = VolumeManager.instance.stack;
            _glitchArtVolume = stack.GetComponent<GlitchArtComponent>();
            if (_glitchArtVolume == null) return;
            if (_glitchArtVolume.isShow.value == false) return;
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

            _mat.SetFloat(AnalogGlitchMode, Convert.ToInt32(_glitchArtVolume.analogGlitchMode.value));
            _mat.SetVector(ScanLineJitter, _glitchArtVolume.scanLineJitter.value);
            _mat.SetFloat(HorizontalShakeMode, Convert.ToInt32(_glitchArtVolume.horizontalShakeMode.value));
            _mat.SetFloat(HorizontalShake, _glitchArtVolume.horizontalShake.value);
            _mat.SetFloat(ColorDriftMode, Convert.ToInt32(_glitchArtVolume.colorDriftMode.value));
            _mat.SetFloat(ColorDrift, _glitchArtVolume.colorDrift.value);
            _mat.SetFloat(VerticalJumpMode, Convert.ToInt32(_glitchArtVolume.verticalJumpMode.value));
            _mat.SetFloat(VerticalJump, _glitchArtVolume.verticalJump.value);

            cmd.SetGlobalTexture(MainTexId, source);
            cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0,
                FilterMode.Trilinear, RenderTextureFormat.Default);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, _mat, 0);
        }
    }
}