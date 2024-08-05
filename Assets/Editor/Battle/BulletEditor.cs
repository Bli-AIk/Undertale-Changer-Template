using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class BulletEditor : EditorWindow
{
    [MenuItem("UCT Window/BulletEditor")]
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

        rootVisualElement.Add(new MaskField());

    }
}