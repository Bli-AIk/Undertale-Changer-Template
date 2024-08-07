using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class SpriteField : EditorWindow
{
    [MenuItem("UCT Window/SpriteField")]
    public static void ShowExample()
    {
        SpriteField wnd = GetWindow<SpriteField>();
        wnd.titleContent = new GUIContent("SpriteField");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Default/SpriteField.uxml");
        //VisualElement labelFromUXML = visualTree.Instantiate();
        //root.Add(labelFromUXML);
        visualTree.CloneTree(root);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/Default/SpriteField.uss");
        root.styleSheets.Add(styleSheet);
    }
}