using System;
using UnityEngine;

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
        if (MainControl.instance.sceneState != MainControl.SceneState.InBattle)
            return;
        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;
    }
}