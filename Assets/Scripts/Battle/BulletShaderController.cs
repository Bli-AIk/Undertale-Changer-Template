using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
/// <summary>
/// BulletShaderController
/// </summary>
public class BulletShaderController : MonoBehaviour
{

    Material material;
    void Start()
    {
        material = Instantiate(Resources.Load<Material>("Materials/SpriteBattleMask"));

        GetComponent<SpriteRenderer>().material = material;


    }

    void Update()
    {
        if (MainControl.instance.sceneState != MainControl.SceneState.InBattle)
            return;
        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;

        Set();
    }

    void Set()
    {

        for (int i = 0; i < MainControl.instance.drawFrameController.points.Count; i++)
        {
            material.SetVector("_Point" + i, MainControl.instance.drawFrameController.points[i].transform.position);
        }
    }
}


