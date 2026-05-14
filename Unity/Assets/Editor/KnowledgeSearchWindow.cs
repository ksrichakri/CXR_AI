using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class KnowledgeSearchWindow : EditorWindow
{
    private string searchQuery = "";

    private List<Entry> searchResults = new List<Entry>();

    private Vector2 scrollPosition;

    // COLLAPSIBLE STATE
    private Dictionary<string, bool> expandedEntries =
        new Dictionary<string, bool>();
    private string currentlyEditingID = "";

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
    private int sortIndex = 0;

    private string[] sortOptions =
    {
    "Newest",
    "Oldest",
    "ID",
    "Title"
    };
    private string filePath;

    private GUIStyle richLabelStyle;

    [MenuItem("Tools/Knowledge Search")]
    public static void ShowWindow()
    {
        GetWindow<KnowledgeSearchWindow>(
            "Knowledge Search"
        );
    }

    private void OnEnable()
    {
        filePath = Path.Combine(
            Application.dataPath,
            "knowledge.json"
        );

        // RICH TEXT STYLE
        richLabelStyle =
            new GUIStyle(EditorStyles.label);

        richLabelStyle.richText = true;

        richLabelStyle.wordWrap = true;
    }

    private void OnGUI()
    {
        GUILayout.Label(
            "Knowledge Search",
            EditorStyles.boldLabel
        );

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

        // SORT DROPDOWN
        sortIndex = EditorGUILayout.Popup(
            "Sort By",
            sortIndex,
            sortOptions
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
            GUILayout.ExpandHeight(true)
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
            Debug.LogError(
                "knowledge.json not found"
            );
            return;
        }

        string json =
            File.ReadAllText(filePath);

        Wrapper data =
            JsonUtility.FromJson<Wrapper>(json);

        if (data == null ||
            data.entries == null)
        {
            Debug.LogError(
                "No entries found"
            );
            return;
        }

        foreach (var entry in data.entries)
        {
            string query =
                searchQuery.ToLower();

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
                    match =
                        entry.id.ToLower().Contains(query);
                    break;

                case "Title":
                    match =
                        entry.title.ToLower().Contains(query);
                    break;

                case "Category":
                    match =
                        entry.category.ToLower().Contains(query);
                    break;

                case "Tags":
                    match =
                        entry.tags.ToLower().Contains(query);
                    break;

                case "Problem":
                    match =
                        entry.problem.ToLower().Contains(query);
                    break;

                case "Solution":
                    match =
                        entry.solution.ToLower().Contains(query);
                    break;
            }

            if (match)
            {
                searchResults.Add(entry);
            }
        }
        // SORT RESULTS
        switch (sortOptions[sortIndex])
        {
            case "Newest":
                searchResults.Sort((a, b) =>
                    b.createdAt.CompareTo(a.createdAt));
                break;

            case "Oldest":
                searchResults.Sort((a, b) =>
                    a.createdAt.CompareTo(b.createdAt));
                break;

            case "ID":
                searchResults.Sort((a, b) =>
                    a.id.CompareTo(b.id));
                break;

            case "Title":
                searchResults.Sort((a, b) =>
                    a.title.CompareTo(b.title));
                break;
        }
        Debug.Log(
            $"Found {searchResults.Count} result(s)"
        );
    }

    // =========================================
    // DRAW ENTRY
    // =========================================
    void DrawEntry(Entry entry)
    {
        GUILayout.BeginVertical("box");

        // INITIALIZE STATE
        if (!expandedEntries.ContainsKey(entry.id))
        {
            expandedEntries[entry.id] = false;
        }

        bool expanded =
            expandedEntries[entry.id];

        // HEADER
        string arrow =
            expanded ? "▼" : "▶";

        GUIStyle headerStyle =
            new GUIStyle(EditorStyles.boldLabel);

        headerStyle.richText = true;

        if (GUILayout.Button(
            $"{arrow} {entry.id} - {HighlightMatch(entry.title)}",
            headerStyle
        ))
        {
            expandedEntries[entry.id] =
                !expanded;
        }

        // EXPANDED CONTENT
        // EXPANDED CONTENT
        if (expandedEntries[entry.id])
        {
            bool isEditing =
                currentlyEditingID == entry.id;

            GUILayout.Space(5);

            // =====================================
            // EDIT MODE
            // =====================================
            if (isEditing)
            {
                GUILayout.Label("Title");
                entry.title = EditorGUILayout.TextField(
                    entry.title
                );

                GUILayout.Label("Category");
                entry.category = EditorGUILayout.TextField(
                    entry.category
                );

                GUILayout.Label("Tags");
                entry.tags = EditorGUILayout.TextField(
                    entry.tags
                );

                // DYNAMIC HEIGHTS
                float problemHeight =
                    Mathf.Max(
                        60,
                        EditorStyles.textArea.CalcHeight(
                            new GUIContent(entry.problem),
                            position.width - 40
                        )
                    );

                float solutionHeight =
                    Mathf.Max(
                        60,
                        EditorStyles.textArea.CalcHeight(
                            new GUIContent(entry.solution),
                            position.width - 40
                        )
                    );

                // PROBLEM FIELD
                GUILayout.Label("Problem");

                entry.problem =
                    EditorGUILayout.TextArea(
                        entry.problem,
                        GUILayout.Height(problemHeight)
                    );

                // SOLUTION FIELD
                GUILayout.Label("Solution");

                entry.solution =
                    EditorGUILayout.TextArea(
                        entry.solution,
                        GUILayout.Height(solutionHeight)
                    );  
            }

            // =====================================
            // NORMAL VIEW MODE
            // =====================================
            else
            {
                GUILayout.Label(
                    $"Category: {HighlightMatch(entry.category)}",
                    richLabelStyle
                );

                GUILayout.Label(
                    $"Tags: {HighlightMatch(entry.tags)}",
                    richLabelStyle
                );

                GUILayout.Label(
                    $"Problem: {HighlightMatch(entry.problem)}",
                    richLabelStyle
                );

                GUILayout.Label(
                    $"Solution: {HighlightMatch(entry.solution)}",
                    richLabelStyle
                );
            }

            GUILayout.Label(
                $"Created: {entry.createdAt}"
            );

            GUILayout.Space(10);

            // =====================================
            // ACTION BUTTONS
            // =====================================
            GUILayout.BeginHorizontal();

            // =========================
            // EDIT BUTTON
            // =========================
            if (!isEditing)
            {
                if (GUILayout.Button("Edit"))
                {
                    currentlyEditingID = entry.id;

                    Debug.Log(
                        $"Editing Mode Enabled: {entry.id}"
                    );
                }
            }

            // =========================
            // SAVE BUTTON
            // =========================
            if (isEditing)
            {
                if (GUILayout.Button("Save Changes"))
                {
                    SaveEditedEntry(entry);

                    currentlyEditingID = "";

                    Debug.Log(
                        $"Saved Changes: {entry.id}"
                    );
                }
            }

            // =========================
            // DELETE BUTTON
            // =========================
            if (GUILayout.Button("Delete"))
            {
                DeleteEntry(entry.id);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();

        GUILayout.Space(5);
    }

    void DeleteEntry(string entryID)
    {

        if (!File.Exists(filePath))
        {
            Debug.LogError("knowledge.json not found");
            return;
        }

        string json = File.ReadAllText(filePath);

        Wrapper data =
            JsonUtility.FromJson<Wrapper>(json);

        if (data == null || data.entries == null)
        {
            Debug.LogError("No entries found");
            return;
        }

        // REMOVE ENTRY
        data.entries.RemoveAll(
            entry => entry.id == entryID
        );

        // SAVE UPDATED JSON
        string updatedJson =
            JsonUtility.ToJson(data, true);

        File.WriteAllText(filePath, updatedJson);

        AssetDatabase.Refresh();

        // REMOVE FROM SEARCH RESULTS
        searchResults.RemoveAll(
            entry => entry.id == entryID
        );

        Debug.Log($"Deleted Entry: {entryID}");
    }
    void SaveEditedEntry(Entry updatedEntry)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError(
                "knowledge.json not found"
            );
            return;
        }

        string json =
            File.ReadAllText(filePath);

        Wrapper data =
            JsonUtility.FromJson<Wrapper>(json);

        if (data == null ||
            data.entries == null)
        {
            Debug.LogError(
                "No entries found"
            );
            return;
        }

        // FIND ENTRY
        for (int i = 0; i < data.entries.Count; i++)
        {
            if (data.entries[i].id ==
                updatedEntry.id)
            {
                data.entries[i] = updatedEntry;
                break;
            }
        }

        // SAVE UPDATED JSON
        string updatedJson =
            JsonUtility.ToJson(data, true);

        File.WriteAllText(
            filePath,
            updatedJson
        );

        AssetDatabase.Refresh();
    }
    string HighlightMatch(string text)
    {
        if (string.IsNullOrEmpty(searchQuery))
        {
            return text;
        }

        string lowerText =
            text.ToLower();

        string lowerQuery =
            searchQuery.ToLower();

        int index =
            lowerText.IndexOf(lowerQuery);

        if (index < 0)
        {
            return text;
        }

        string matchedPart =
            text.Substring(
                index,
                searchQuery.Length
            );

        string highlighted =
            text.Replace(
                matchedPart,
                $"<color=yellow><b>{matchedPart}</b></color>"
            );

        return highlighted;
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