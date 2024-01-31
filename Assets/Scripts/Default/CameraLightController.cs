using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraLightController : MonoBehaviour
{
    public float viewDistance = 10f; // ���ӷ�Χ�İ뾶

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        UpdateLightsVisibility();
    }

    private void Update()
    {
        UpdateLightsVisibility();
    }

    private void UpdateLightsVisibility()
    {
        if (MainControl.instance.OverworldControl.noSFX)
            return;

        // ��ȡ���й�Դ���������
        Light2D[] lights = FindObjectsOfType<Light2D>();

        foreach (Light2D light in lights)
        {
            if (light.lightType == Light2D.LightType.Global)
                return;

            light.enabled = Vector3.Distance(light.transform.position, mainCamera.transform.position) <= viewDistance;
        }
    }
}