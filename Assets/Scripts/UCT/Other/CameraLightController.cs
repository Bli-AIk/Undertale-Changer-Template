using UCT.Global.Settings;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UCT.Other
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
            if (SettingsStorage.IsSimplifySfx)
            {
                return;
            }

            var lights = FindObjectsOfType<Light2D>();

            foreach (var item in lights)
            {
                if (item.lightType == Light2D.LightType.Global)
                {
                    return;
                }

                item.enabled = Vector3.Distance(item.transform.position, _mainCamera.transform.position) <=
                               viewDistance;
            }
        }
    }
}