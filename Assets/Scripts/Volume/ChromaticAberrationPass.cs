using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class ChromaticAberrationPass : ScriptableRenderPass
{
    static readonly string renderTag = "ChromaticAberration Effects";
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");


    private ChromaticAberrationComponent chromaticAberrationVolume;
    private Material mat;
    RenderTargetIdentifier currentTarget;
    public ChromaticAberrationPass(RenderPassEvent passEvent, Shader ChromaticAberrationShader)
    {
        renderPassEvent = passEvent;
        if (ChromaticAberrationShader == null)
        {
            Debug.LogError("Shader²»´æÔÚ");
            return;
        }
        mat = CoreUtils.CreateEngineMaterial(ChromaticAberrationShader);
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
        chromaticAberrationVolume = stack.GetComponent<ChromaticAberrationComponent>();
        if (chromaticAberrationVolume == null)
        {
            return;
        }
        if (chromaticAberrationVolume.isShow.value == false)
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

        mat.SetFloat("_Offset", chromaticAberrationVolume.offset.value);
        mat.SetFloat("_Speed", chromaticAberrationVolume.speed.value);
        mat.SetFloat("_Height", chromaticAberrationVolume.height.value);
        mat.SetFloat("_OnlyOri", Convert.ToInt32(chromaticAberrationVolume.onlyOri.value));


        cmd.SetGlobalTexture(MainTexId, source);
        cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(source, destination);
        cmd.Blit(destination, source, mat, 0);
    }
}



