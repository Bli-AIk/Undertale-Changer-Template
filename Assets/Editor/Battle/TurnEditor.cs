using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class TurnEditor : EditorWindow
{
    [MenuItem("UCT Window/TurnEditor")]
    public static void ShowExample()
    {
        TurnEditor wnd = GetWindow<TurnEditor>();
        wnd.titleContent = new GUIContent("TurnEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TurnEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

    }
}