using UCT.Global.Core;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    /// µ¯Ä»Shader¿ØÖÆÆ÷
    /// </summary>
    public class BulletShaderController : MonoBehaviour
    {
        private Material _material;
        private void Start()
        {
            _material = Instantiate(Resources.Load<Material>("Materials/Bullet"));

            GetComponent<SpriteRenderer>().material = _material;
        }

        private void Update()
        {
            if (MainControl.Instance.sceneState != MainControl.SceneState.InBattle)
                return;
            if (MainControl.Instance.overworldControl.isSetting || MainControl.Instance.overworldControl.pause)
                return;
        }
    }
}