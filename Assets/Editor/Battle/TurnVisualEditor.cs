using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;
using UCT.Control;

/// <summary>
/// ���ӻ��غϱ༭��
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





        // ��Ļ�ļ�����
        VisualElement bulletsContainer = root.Q("Bullets");

        if (bulletsContainer == null)
        {
            UCT.Global.Other.Debug.LogError("No element with name 'Bullets' found in the root.");
            return;
        }


        List<BulletControl> bullets = Resources.LoadAll<BulletControl>("Assets/Bullets/").ToList();
        UCT.Global.Other.Debug.Log(bullets.Count);

        foreach (var bullet in bullets)
        {
            UCT.Global.Other.Debug.Log(bullet.name);

            VisualElement bulletRoot = new VisualElement();

            var bulletTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Battle/Bullet.uxml");
            bulletTree.CloneTree(bulletRoot);

            bulletsContainer.Add(bulletRoot);

            DragBullets(bulletRoot, bullet);
        }
    }


    /// <summary>
    /// �����ѵ�Ļק��ʱ������߼�
    /// </summary>
    void DragBullets(VisualElement bulletRoot, BulletControl bulletControl)
    {
        VisualElement bullet = bulletRoot.Q<VisualElement>("Bullet");
        Label bulletText = bulletRoot.Q<Label>("BulletText");
        VisualElement bulletImage = bulletRoot.Q<VisualElement>("BulletImage");

        bulletText.text = bulletControl.typeName;
        bulletImage.style.backgroundImage = bulletControl.sprite.texture;

        // �����������¼�������
        bullet.RegisterCallback<MouseEnterEvent>(evt =>
        {
            bulletText.style.color = new Color(173 / 255f, 216 / 255f, 230 / 255f);
            bulletImage.style.unityBackgroundImageTintColor = new Color(173 / 255f, 216 / 255f, 230 / 255f);
        });   // �����������¼�������
        bullet.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            bulletText.style.color = Color.white;
            bulletImage.style.unityBackgroundImageTintColor = Color.white;
        });



    }
}
