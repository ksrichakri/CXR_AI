using UnityEditor;
using UnityEngine;

public class KnowledgeAssistantWindow : EditorWindow
{
    private string entryTitle = "";
    private string category = "";
    private string tags = "";
    private string problem = "";
    private string solution = "";
    private string codeSnippet = "";

    private Vector2 scrollPos;

    [MenuItem("Tools/Knowledge Assistant")]
    public static void ShowWindow()
    {
        GetWindow<KnowledgeAssistantWindow>("Knowledge Assistant");
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Space(10);

        GUILayout.Label("Knowledge Entry Form", EditorStyles.boldLabel);

        GUILayout.Space(5);

        entryTitle = EditorGUILayout.TextField("Title", entryTitle);

        category = EditorGUILayout.TextField("Category", category);

        tags = EditorGUILayout.TextField("Tags", tags);

        GUILayout.Space(10);

        GUILayout.Label("Problem");
        problem = EditorGUILayout.TextArea(problem, GUILayout.Height(100));

        GUILayout.Space(10);

        GUILayout.Label("Solution");
        solution = EditorGUILayout.TextArea(solution, GUILayout.Height(100));

        GUILayout.Space(10);

        GUILayout.Label("Code Snippet");
        codeSnippet = EditorGUILayout.TextArea(codeSnippet, GUILayout.Height(120));

        GUILayout.Space(15);

        if (GUILayout.Button("Save Entry", GUILayout.Height(35)))
        {
            Debug.Log("=== Entry Saved ===");

            Debug.Log($"Title: {entryTitle}");
            Debug.Log($"Category: {category}");
            Debug.Log($"Tags: {tags}");
            Debug.Log($"Problem: {problem}");
            Debug.Log($"Solution: {solution}");
            Debug.Log($"Code Snippet: {codeSnippet}");
        }

        EditorGUILayout.EndScrollView();
    }
}