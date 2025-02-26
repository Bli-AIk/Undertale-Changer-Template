using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    ///     µ¯Ä»Shader¿ØÖÆÆ÷
    /// </summary>
    public class BulletShaderController : MonoBehaviour
    {
        private void Start()
        {
            var material = Instantiate(Resources.Load<Material>("Materials/Bullet"));
            GetComponent<SpriteRenderer>().material = material;
        }
    }
}