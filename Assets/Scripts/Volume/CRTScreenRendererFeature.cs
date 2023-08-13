using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlitchArtRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }
    public Settings settings = new Settings();
    GlitchArtPass pass;
    public override void Create()
    {
        this.name = "GlitchArtPass";
        pass = new GlitchArtPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(pass);
    }

}
[System.Serializable]
public class GlitchArtPass : ScriptableRenderPass
{
    static readonly string renderTag = "GlitchArt Effects";
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");


    private GlitchArtComponent GlitchArtVolume;
    private Material mat;
    RenderTargetIdentifier currentTarget;
    public GlitchArtPass(RenderPassEvent passEvent, Shader GlitchArtShader)
    {
        renderPassEvent = passEvent;
        if (GlitchArtShader == null)
        {
            Debug.LogError("Shader does not exist");
            return;
        }
        mat = CoreUtils.CreateEngineMaterial(GlitchArtShader);
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
        GlitchArtVolume = stack.GetComponent<GlitchArtComponent>();
        if (GlitchArtVolume == null)
        {
            return;
        }
        if (GlitchArtVolume.isShow.value == false)
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

        mat.SetFloat("_AnalogGlitchMode", System.Convert.ToInt32(GlitchArtVolume.analogGlitchMode.value));
        mat.SetVector("_ScanLineJitter", GlitchArtVolume.scanLineJitter.value); 
        mat.SetFloat("_HorizontalShakeMode", System.Convert.ToInt32(GlitchArtVolume.horizontalShakeMode.value));
        mat.SetFloat("_HorizontalShake", GlitchArtVolume.horizontalShake.value);
        mat.SetFloat("_ColorDriftMode", System.Convert.ToInt32(GlitchArtVolume.colorDriftMode.value));
        mat.SetFloat("_ColorDrift", GlitchArtVolume.colorDrift.value);
        mat.SetFloat("_VerticalJumpMode", System.Convert.ToInt32(GlitchArtVolume.verticalJumpMode.value));
        mat.SetFloat("_VerticalJump", GlitchArtVolume.verticalJump.value);


        cmd.SetGlobalTexture(MainTexId, source);
        cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(source, destination);
        cmd.Blit(destination, source, mat, 0);
    }
}

