using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class KnowledgeSearchWindow : EditorWindow
{
    private string searchQuery = "";

    private List<Entry> searchResults = new List<Entry>();

    private Vector2 scrollPosition;

    // FILTER SYSTEM
    private int filterIndex = 0;

    private string[] filters =
    {
        "All",
        "ID",
        "Title",
        "Category",
        "Tags",
        "Problem",
        "Solution"
    };

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

        // SEARCH FIELD
        searchQuery = EditorGUILayout.TextField(
            "Search",
            searchQuery
        );

        GUILayout.Space(10);

        // FILTER DROPDOWN
        filterIndex = EditorGUILayout.Popup(
            "Filter By",
            filterIndex,
            filters
        );

        GUILayout.Space(10);

        // BUTTONS
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Search"))
        {
            SearchKnowledge();
        }

        if (GUILayout.Button("Clear"))
        {
            searchQuery = "";
            searchResults.Clear();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        // RESULT COUNT
        GUILayout.Label(
            $"Results ({searchResults.Count})",
            EditorStyles.boldLabel
        );

        GUILayout.Space(5);

        // SCROLL VIEW
        scrollPosition = GUILayout.BeginScrollView(
            scrollPosition,
            GUILayout.Height(400)
        );

        foreach (var entry in searchResults)
        {
            DrawEntry(entry);
        }

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
            string query = searchQuery.ToLower();

            bool match = false;

            switch (filters[filterIndex])
            {
                case "All":
                    match =
                        entry.id.ToLower().Contains(query) ||
                        entry.title.ToLower().Contains(query) ||
                        entry.category.ToLower().Contains(query) ||
                        entry.tags.ToLower().Contains(query) ||
                        entry.problem.ToLower().Contains(query) ||
                        entry.solution.ToLower().Contains(query);
                    break;

                case "ID":
                    match = entry.id.ToLower().Contains(query);
                    break;

                case "Title":
                    match = entry.title.ToLower().Contains(query);
                    break;

                case "Category":
                    match = entry.category.ToLower().Contains(query);
                    break;

                case "Tags":
                    match = entry.tags.ToLower().Contains(query);
                    break;

                case "Problem":
                    match = entry.problem.ToLower().Contains(query);
                    break;

                case "Solution":
                    match = entry.solution.ToLower().Contains(query);
                    break;
            }

            if (match)
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

        GUILayout.Label(
            $"ID: {entry.id}",
            EditorStyles.boldLabel
        );

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