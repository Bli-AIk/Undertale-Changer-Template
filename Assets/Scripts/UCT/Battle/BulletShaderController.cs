using UCT.Global.Core;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    /// µ¯Ä»Shader¿ØÖÆÆ÷
    /// </summary>
    public class BulletShaderController : MonoBehaviour
    {
        private Material material;
        private void Start()
        {
            material = Instantiate(Resources.Load<Material>("Materials/Bullet"));

            GetComponent<SpriteRenderer>().material = material;
        }

        private void Update()
        {
            if (MainControl.Instance.sceneState != MainControl.SceneState.InBattle)
                return;
            if (MainControl.Instance.OverworldControl.isSetting || MainControl.Instance.OverworldControl.pause)
                return;
        }
    }
}