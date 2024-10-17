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
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public Shader shader;
        }

        public Settings settings = new Settings();
        private CRTScreenPass pass;

        public override void Create()
        {
            name = "CRTScreenPass";
            pass = new CRTScreenPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            pass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(pass);
        }
    }

    [Serializable]
    public class CRTScreenPass : ScriptableRenderPass
    {
        private static readonly string renderTag = "CRTScreen Effects";
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");

        private CRTScreenComponent CRTScreenVolume;
        private Material mat;
        private RenderTargetIdentifier currentTarget;

        public CRTScreenPass(RenderPassEvent passEvent, Shader CRTScreenShader)
        {
            renderPassEvent = passEvent;
            if (CRTScreenShader == null)
            {
                UCT.Global.Other.Debug.Log("Shader不存在");
                return;
            }
            mat = CoreUtils.CreateEngineMaterial(CRTScreenShader);
        }

        public void Setup(in RenderTargetIdentifier currentTarget)
        {
            this.currentTarget = currentTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (mat == null)
            {
                return;
            }
            if (!renderingData.cameraData.postProcessEnabled)
            {
                return;
            }
            VolumeStack stack = VolumeManager.instance.stack;
            CRTScreenVolume = stack.GetComponent<CRTScreenComponent>();
            if (CRTScreenVolume == null)
            {
                return;
            }
            if (CRTScreenVolume.isShow.value == false)
            {
                return;
            }
            CommandBuffer cmd = CommandBufferPool.Get(renderTag);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            Camera camera = cameraData.camera;
            RenderTargetIdentifier source = currentTarget;
            int destination = TempTargetId;

            mat.SetVector("_Resolution", CRTScreenVolume.resolution.value);
            mat.SetVector("_PixelScanlineBrightness", CRTScreenVolume.pixelScanlineBrightness.value);
            mat.SetFloat("_Speed", CRTScreenVolume.speed.value);

            cmd.SetGlobalTexture(MainTexId, source);
            cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, mat, 0);
        }
    }
}