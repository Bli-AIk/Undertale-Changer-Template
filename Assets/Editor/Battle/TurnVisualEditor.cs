using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 可视化回合编辑器
/// </summary>
public class TurnVisualEditor : EditorWindow
{
    [MenuItem("Window/UI Toolkit/TurnVisualEditor")]
    public static void ShowExample()
    {
        TurnVisualEditor wnd = GetWindow<TurnVisualEditor>();
        wnd.titleContent = new GUIContent("TurnVisualEditor");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Battle/TurnVisualEditor.uxml");
        visualTree.CloneTree(root);





        // 弹幕文件导入
        VisualElement bulletsContainer = root.Q("Bullets");

        if (bulletsContainer == null)
        {
            Debug.LogError("No element with name 'Bullets' found in the root.");
            return;
        }


        List<BulletControl> bullets = Resources.LoadAll<BulletControl>("Assets/Bullets/").ToList();
        Debug.Log(bullets.Count);

        foreach (var bullet in bullets)
        {
            Debug.Log(bullet.name);

            VisualElement bulletRoot = new VisualElement();

            var bulletTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Battle/Bullet.uxml");
            bulletTree.CloneTree(bulletRoot);

            bulletsContainer.Add(bulletRoot);

            DragBullets(bulletRoot, bullet);
        }
    }


    /// <summary>
    /// 处理把弹幕拽到时间轴的逻辑
    /// </summary>
    void DragBullets(VisualElement bulletRoot, BulletControl bulletControl)
    {
        VisualElement bullet = bulletRoot.Q<VisualElement>("Bullet");
        Label bulletText = bulletRoot.Q<Label>("BulletText");
        VisualElement bulletImage = bulletRoot.Q<VisualElement>("BulletImage");

        bulletText.text = bulletControl.typeName;
        bulletImage.style.backgroundImage = bulletControl.sprite.texture;

        // 添加鼠标进入事件监听器
        bullet.RegisterCallback<MouseEnterEvent>(evt =>
        {
            bulletText.style.color = new Color(173 / 255f, 216 / 255f, 230 / 255f);
            bulletImage.style.unityBackgroundImageTintColor = new Color(173 / 255f, 216 / 255f, 230 / 255f);
        });   // 添加鼠标进入事件监听器
        bullet.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            bulletText.style.color = Color.white;
            bulletImage.style.unityBackgroundImageTintColor = Color.white;
        });



    }
}
