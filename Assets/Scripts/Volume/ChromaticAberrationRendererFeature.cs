
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticAberrationRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }

    public Settings settings = new Settings();
    private ChromaticAberrationPass pass;

    public override void Create()
    {
        this.name = "ChromaticAberrationPass";
        pass = new ChromaticAberrationPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(pass);
    }
}

[System.Serializable]
public class ChromaticAberrationPass : ScriptableRenderPass
{
    private static readonly string renderTag = "ChromaticAberration Effects";
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");

    private ChromaticAberrationComponent chromaticAberrationVolume;
    private Material mat;
    private RenderTargetIdentifier currentTarget;

    public ChromaticAberrationPass(RenderPassEvent passEvent, Shader ChromaticAberrationShader)
    {
        renderPassEvent = passEvent;
        if (ChromaticAberrationShader == null)
        {
            DebugLogger.Log("Shader²»´æÔÚ", DebugLogger.Type.err);
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
        mat.SetFloat("_OnlyOri", System.Convert.ToInt32(chromaticAberrationVolume.onlyOri.value));

        cmd.SetGlobalTexture(MainTexId, source);
        cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(source, destination);
        cmd.Blit(destination, source, mat, 0);
    }
}