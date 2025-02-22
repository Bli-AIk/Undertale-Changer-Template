using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volume
{
    public class ChromaticAberrationRendererFeature : ScriptableRendererFeature
    {
        public Settings settings = new();
        private ChromaticAberrationPass _pass;

        public override void Create()
        {
            name = "ChromaticAberrationPass";
            _pass = new ChromaticAberrationPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader);
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
    public class ChromaticAberrationPass : ScriptableRenderPass
    {
        private const string RenderTag = "ChromaticAberration Effects";
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");
        private static readonly int Offset = Shader.PropertyToID("_Offset");
        private static readonly int Speed = Shader.PropertyToID("_Speed");
        private static readonly int Height = Shader.PropertyToID("_Height");
        private static readonly int OnlyOri = Shader.PropertyToID("_OnlyOri");

        private ChromaticAberrationComponent _chromaticAberrationVolume;
        private RenderTargetIdentifier _currentTarget;
        private Material _mat;

        public ChromaticAberrationPass(RenderPassEvent passEvent, Shader chromaticAberrationShader)
        {
            renderPassEvent = passEvent;
            if (chromaticAberrationShader == null)
            {
                UCT.Other.Debug.Log("Shader不存在");
                return;
            }

            _mat = CoreUtils.CreateEngineMaterial(chromaticAberrationShader);
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
            _chromaticAberrationVolume = stack.GetComponent<ChromaticAberrationComponent>();
            if (_chromaticAberrationVolume == null)
            {
                return;
            }

            if (_chromaticAberrationVolume.isShow.value == false)
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

            _mat.SetFloat(Offset, _chromaticAberrationVolume.offset.value);
            _mat.SetFloat(Speed, _chromaticAberrationVolume.speed.value);
            _mat.SetFloat(Height, _chromaticAberrationVolume.height.value);
            _mat.SetFloat(OnlyOri, Convert.ToInt32(_chromaticAberrationVolume.onlyOri.value));

            cmd.SetGlobalTexture(MainTexId, source);
            cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0,
                FilterMode.Trilinear, RenderTextureFormat.Default);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, _mat, 0);
        }
    }
}