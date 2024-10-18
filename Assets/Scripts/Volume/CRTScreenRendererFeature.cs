using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volume
{
    public class GlitchArtRendererFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public Shader shader;
        }

        public Settings settings = new Settings();
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
    }

    [Serializable]
    public class GlitchArtPass : ScriptableRenderPass
    {
        private static readonly string RenderTag = "GlitchArt Effects";
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");

        private GlitchArtComponent _glitchArtVolume;
        private Material _mat;
        private RenderTargetIdentifier _currentTarget;

        public GlitchArtPass(RenderPassEvent passEvent, Shader glitchArtShader)
        {
            renderPassEvent = passEvent;
            if (glitchArtShader == null)
            {
                UCT.Global.Other.Debug.Log("Shader不存在");
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
            if (_mat == null)
            {
                return;
            }
            if (!renderingData.cameraData.postProcessEnabled)
            {
                return;
            }
            VolumeStack stack = VolumeManager.instance.stack;
            _glitchArtVolume = stack.GetComponent<GlitchArtComponent>();
            if (_glitchArtVolume == null)
            {
                return;
            }
            if (_glitchArtVolume.isShow.value == false)
            {
                return;
            }
            CommandBuffer cmd = CommandBufferPool.Get(RenderTag);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            Camera camera = cameraData.camera;
            RenderTargetIdentifier source = _currentTarget;
            int destination = TempTargetId;

            _mat.SetFloat("_AnalogGlitchMode", Convert.ToInt32(_glitchArtVolume.analogGlitchMode.value));
            _mat.SetVector("_ScanLineJitter", _glitchArtVolume.scanLineJitter.value);
            _mat.SetFloat("_HorizontalShakeMode", Convert.ToInt32(_glitchArtVolume.horizontalShakeMode.value));
            _mat.SetFloat("_HorizontalShake", _glitchArtVolume.horizontalShake.value);
            _mat.SetFloat("_ColorDriftMode", Convert.ToInt32(_glitchArtVolume.colorDriftMode.value));
            _mat.SetFloat("_ColorDrift", _glitchArtVolume.colorDrift.value);
            _mat.SetFloat("_VerticalJumpMode", Convert.ToInt32(_glitchArtVolume.verticalJumpMode.value));
            _mat.SetFloat("_VerticalJump", _glitchArtVolume.verticalJump.value);

            cmd.SetGlobalTexture(MainTexId, source);
            cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, _mat, 0);
        }
    }
}