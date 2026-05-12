using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class KnowledgeSearchWindow : EditorWindow
{
    private string searchQuery = "";

    private List<Entry> searchResults = new List<Entry>();
    private Vector2 scrollPosition;

    private string filePath;

    [MenuItem("Tools/Knowledge Search")]
    public static void ShowWindow()
    {
        GetWindow<KnowledgeSearchWindow>("Knowledge Search");
    }

    private void OnEnable()
    {
        filePath = Path.Combine(Application.dataPath, "knowledge.json");
    }

    private void OnGUI()
    {
        GUILayout.Label("Knowledge Search", EditorStyles.boldLabel);

        GUILayout.Space(10);

        searchQuery = EditorGUILayout.TextField("Search", searchQuery);

        GUILayout.Space(10);

        if (GUILayout.Button("Search"))
        {
            SearchKnowledge();
        }

        GUILayout.Space(15);

        GUILayout.Label("Results", EditorStyles.boldLabel);

        GUILayout.Space(5);

        // SCROLL VIEW START
        scrollPosition = GUILayout.BeginScrollView(
            scrollPosition,
            GUILayout.Height(400)
        );

        foreach (var entry in searchResults)
        {
            DrawEntry(entry);
        }

        // SCROLL VIEW END
        GUILayout.EndScrollView();
    }

    // =========================================
    // SEARCH LOGIC
    // =========================================
    void SearchKnowledge()
    {
        searchResults.Clear();

        if (!File.Exists(filePath))
        {
            Debug.LogError("knowledge.json not found");
            return;
        }

        string json = File.ReadAllText(filePath);

        Wrapper data = JsonUtility.FromJson<Wrapper>(json);

        if (data == null || data.entries == null)
        {
            Debug.LogError("No entries found");
            return;
        }

        foreach (var entry in data.entries)
        {
            if (
                entry.id.ToLower().Contains(searchQuery.ToLower()) ||
                entry.title.ToLower().Contains(searchQuery.ToLower()) ||
                entry.category.ToLower().Contains(searchQuery.ToLower()) ||
                entry.tags.ToLower().Contains(searchQuery.ToLower()) ||
                entry.problem.ToLower().Contains(searchQuery.ToLower()) ||
                entry.solution.ToLower().Contains(searchQuery.ToLower())
            )
            {
                searchResults.Add(entry);
            }
        }

        Debug.Log($"Found {searchResults.Count} result(s)");
    }

    // =========================================
    // DRAW RESULT ENTRY
    // =========================================
    void DrawEntry(Entry entry)
    {
        GUILayout.BeginVertical("box");

        GUILayout.Label($"ID: {entry.id}", EditorStyles.boldLabel);

        GUILayout.Label($"Title: {entry.title}");

        GUILayout.Label($"Category: {entry.category}");

        GUILayout.Label($"Tags: {entry.tags}");

        GUILayout.Label($"Problem: {entry.problem}");

        GUILayout.Label($"Solution: {entry.solution}");

        GUILayout.Label($"Created: {entry.createdAt}");

        GUILayout.EndVertical();

        GUILayout.Space(5);
    }

    // =========================================
    // DATA MODEL
    // =========================================
    [System.Serializable]
    public class Entry
    {
        public string id;

        public string title;

        public string category;

        public string tags;

        public string problem;

        public string solution;

        public string createdAt;
    }

    [System.Serializable]
    public class Wrapper
    {
        public List<Entry> entries;
    }
}