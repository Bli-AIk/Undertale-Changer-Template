using UnityEngine;

/// <summary>
/// µ¯Ä»Shader¿ØÖÆÆ÷
/// </summary>
public class BulletShaderController : MonoBehaviour
{
    private Material material;

    [Header("ID¼ì²â£ºÊ¹ÓÃ_Point (0)")]
    public bool useBracketId;

    private void Start()
    {
        material = Instantiate(Resources.Load<Material>("Materials/SpriteBattleMask"));

        GetComponent<SpriteRenderer>().material = material;
    }

    private void Update()
    {
        if (MainControl.instance.sceneState != MainControl.SceneState.InBattle)
            return;
        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;

        Set();
    }

    private void Set()
    {
        for (int i = 0; i < MainControl.instance.drawFrameController.points.Count; i++)
        {
            string id;
            if (!useBracketId)
                id = "_Point" + i;
            else
                id = "_Point_" + i;
            material.SetVector(id, MainControl.instance.drawFrameController.points[i].transform.position);
        }
    }
}