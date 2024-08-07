using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


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
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;


        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Battle/TurnVisualEditor.uxml");
        //VisualElement labelFromUXML = visualTree.Instantiate();
        //root.Add(labelFromUXML);
        visualTree.CloneTree(root);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/Default/TurnVisualEditor.uss");
        root.styleSheets.Add(styleSheet);
    }
}