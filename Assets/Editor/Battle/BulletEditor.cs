using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class BulletEditor : EditorWindow
{
    [MenuItem("Window/UI Toolkit/BulletEditor")]
    public static void ShowExample()
    {
        BulletEditor wnd = GetWindow<BulletEditor>();
        wnd.titleContent = new GUIContent("BulletEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Battle/BulletEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

    }
}