using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Volume
{
    public class ChromaticAberrationRendererFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public Shader shader;
        }

        public Settings settings = new Settings();
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
    }

    [Serializable]
    public class ChromaticAberrationPass : ScriptableRenderPass
    {
        private static readonly string RenderTag = "ChromaticAberration Effects";
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");

        private ChromaticAberrationComponent _chromaticAberrationVolume;
        private Material _mat;
        private RenderTargetIdentifier _currentTarget;

        public ChromaticAberrationPass(RenderPassEvent passEvent, Shader chromaticAberrationShader)
        {
            renderPassEvent = passEvent;
            if (chromaticAberrationShader == null)
            {
                UCT.Global.Other.Debug.Log("Shader不存在");
                return;
            }
            _mat = CoreUtils.CreateEngineMaterial(chromaticAberrationShader);
        }

        public void Setup(in RenderTargetIdentifier currentTarget)
        {
            this._currentTarget = currentTarget;
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
            _chromaticAberrationVolume = stack.GetComponent<ChromaticAberrationComponent>();
            if (_chromaticAberrationVolume == null)
            {
                return;
            }
            if (_chromaticAberrationVolume.isShow.value == false)
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

            _mat.SetFloat("_Offset", _chromaticAberrationVolume.offset.value);
            _mat.SetFloat("_Speed", _chromaticAberrationVolume.speed.value);
            _mat.SetFloat("_Height", _chromaticAberrationVolume.height.value);
            _mat.SetFloat("_OnlyOri", Convert.ToInt32(_chromaticAberrationVolume.onlyOri.value));

            cmd.SetGlobalTexture(MainTexId, source);
            cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, _mat, 0);
        }
    }
}