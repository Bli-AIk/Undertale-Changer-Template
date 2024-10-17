using UCT.Global.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UCT.Global.Other
{
    public class CameraLightController : MonoBehaviour
    {
        public float viewDistance = 10f; // 可视范围的半径

        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = GetComponent<Camera>();
            UpdateLightsVisibility();
        }

        private void Update()
        {
            UpdateLightsVisibility();
        }

        private void UpdateLightsVisibility()
        {
            if (MainControl.Instance.OverworldControl.noSfx)
                return;

            // 获取所有光源组件的引用
            Light2D[] lights = FindObjectsOfType<Light2D>();

            foreach (Light2D light in lights)
            {
                if (light.lightType == Light2D.LightType.Global)
                    return;

                light.enabled = Vector3.Distance(light.transform.position, _mainCamera.transform.position) <= viewDistance;
            }
        }
    }
}