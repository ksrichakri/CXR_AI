using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GitPanelWindow : EditorWindow
{
    [MenuItem("Tools/Git Panel")]
    public static void Open()
    {
        GetWindow<GitPanelWindow>("Git Panel");
    }

    public void CreateGUI()
    {
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/GitPanel/UI/GitPanel.uxml");

        var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/GitPanel/UI/GitPanel.uss");

        if (tree == null || style == null)
        {
            Debug.LogError("UI files missing");
            return;
        }

        rootVisualElement.Clear();
        rootVisualElement.Add(tree.CloneTree());
        rootVisualElement.styleSheets.Add(style);

        new GitUIController(rootVisualElement);
    }
}