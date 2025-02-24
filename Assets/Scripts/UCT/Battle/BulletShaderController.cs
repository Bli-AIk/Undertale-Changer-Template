using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    ///     µ¯Ä»Shader¿ØÖÆÆ÷
    /// </summary>
    public class BulletShaderController : MonoBehaviour
    {
        private Material _material;

        private void Start()
        {
            _material = Instantiate(Resources.Load<Material>("Materials/Bullet"));

            GetComponent<SpriteRenderer>().material = _material;
        }
    }
}