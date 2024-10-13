using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class StretchPostRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }

    public Settings settings = new Settings();
    private StretchPostPass pass;

    public override void Create()
    {
        this.name = "StretchPostPass";
        pass = new StretchPostPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(pass);
    }
}

[System.Serializable]
public class StretchPostPass : ScriptableRenderPass
{
    private static readonly string renderTag = "StretchPost Effects";
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");

    private StretchPostComponent StretchPostVolume;
    private Material mat;
    private RenderTargetIdentifier currentTarget;

    public StretchPostPass(RenderPassEvent passEvent, Shader StretchPostShader)
    {
        renderPassEvent = passEvent;
        if (StretchPostShader == null)
        {
            DebugLogger.Log("Shader²»´æÔÚ", DebugLogger.Type.err);
            return;
        }
        mat = CoreUtils.CreateEngineMaterial(StretchPostShader);
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
        StretchPostVolume = stack.GetComponent<StretchPostComponent>();
        if (StretchPostVolume == null)
        {
            return;
        }
        if (StretchPostVolume.isShow.value == false)
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

        mat.SetVector("_Draw", StretchPostVolume.draw.value);

        cmd.SetGlobalTexture(MainTexId, source);
        cmd.GetTemporaryRT(destination, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);
        cmd.Blit(source, destination);
        cmd.Blit(destination, source, mat, 0);
    }
}