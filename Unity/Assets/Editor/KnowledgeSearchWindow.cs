using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class KnowledgeSearchWindow : EditorWindow
{
    private string searchQuery = "";

    private List<Entry> searchResults = new List<Entry>();

    private Vector2 scrollPosition;
    private List<string> searchHistory =
        new List<string>();
    private List<string> favoriteQueries =
        new List<string>();
    private int totalEntries = 0;

    private string topTag = "None";
    private Dictionary<string, int>
        categoryStats =
        new Dictionary<string, int>();
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
    "Relevance",
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
        var window =
            GetWindow<KnowledgeSearchWindow>(
                "Knowledge Search"
            );

        window.minSize =
            new Vector2(500, 800);
    }

    private void OnEnable()
    {
        filePath = Path.Combine(
            Application.dataPath,
            "knowledge.json"
        );
    }

    private void OnGUI()
    {
        // SAFE GUI STYLE INIT
        if (richLabelStyle == null)
        {
            richLabelStyle =
                new GUIStyle(EditorStyles.label);

            richLabelStyle.richText = true;

            richLabelStyle.wordWrap = true;
        }

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

        // =========================
        // BUTTONS
        // =========================
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(
            "Search",
            GUILayout.Height(35)
        ))
        {
            SearchKnowledge();
        }

        if (GUILayout.Button(
            "Clear",
            GUILayout.Height(35)
        ))
        {
            searchQuery = "";
            searchResults.Clear();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        // =========================
        // SAVE FAVORITE SEARCH
        // =========================
        if (GUILayout.Button("Save Current Search"))
        {
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                if (!favoriteQueries.Contains(searchQuery))
                {
                    favoriteQueries.Add(searchQuery);

                    Debug.Log(
                        $"Saved Favorite Query: {searchQuery}"
                    );
                }
            }
        }

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button(
            "Export TXT",
            GUILayout.Height(30)
        ))
        {
            ExportResults();
        }

        if (GUILayout.Button(
            "Export CSV",
            GUILayout.Height(30)
        ))
        {
            ExportCSV();
        }

        GUILayout.EndHorizontal();

        // =========================
        // SEARCH HISTORY
        // =========================
        if (searchHistory.Count > 0)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label(
                "Recent Searches",
                EditorStyles.boldLabel
            );

            GUILayout.BeginHorizontal();

            foreach (string historyItem in searchHistory)
            {
                if (GUILayout.Button(
                    historyItem,
                    GUILayout.Height(25)
                ))
                {
                    searchQuery = historyItem;

                    SearchKnowledge();

                    Debug.Log(
                        $"History Search: {historyItem}"
                    );
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.EndVertical();
        }

        // =========================
        // FAVORITE QUERIES
        // =========================
        if (favoriteQueries.Count > 0)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label(
                "Favorite Queries",
                EditorStyles.boldLabel
            );

            GUILayout.BeginHorizontal();

            foreach (string favorite in favoriteQueries)
            {
                if (GUILayout.Button(
                    $"★ {favorite}",
                    GUILayout.Height(25)
                ))
                {
                    searchQuery = favorite;

                    SearchKnowledge();

                    Debug.Log(
                        $"Favorite Query Loaded: {favorite}"
                    );
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.EndVertical();
        }

        // =========================
        // ANALYTICS PANEL
        // =========================
        GUILayout.BeginVertical("box");

        GUILayout.Label(
            "Retrieval Analytics",
            EditorStyles.boldLabel
        );
        GUILayout.Space(5);
        GUILayout.Label(
            $"Total Entries: {totalEntries}"
        );

        GUILayout.Label(
            $"Search Results: {searchResults.Count}"
        );

        GUILayout.Label(
            $"Favorite Queries: {favoriteQueries.Count}"
        );

        GUILayout.Label(
            $"Most Used Tag: {topTag}"
        );

        GUILayout.EndVertical();

        // =========================
        // KNOWLEDGE STATISTICS
        // =========================
        GUILayout.BeginVertical("box");

        GUILayout.Label(
            "Knowledge Statistics",
            EditorStyles.boldLabel
        );

        GUILayout.Space(5);

        foreach (var pair in categoryStats)
        {
            GUILayout.Label(
                $"{pair.Key} Entries: {pair.Value}"
            );
        }

        GUILayout.EndVertical();

        GUILayout.Space(15);


        GUILayout.Space(15);

        // =========================
        // RESULT COUNT
        // =========================
        GUILayout.Label(
            $"Results ({searchResults.Count})",
            EditorStyles.boldLabel
        );

        GUILayout.Space(5);

        // =========================
        // SCROLL VIEW
        // =========================
        scrollPosition = GUILayout.BeginScrollView(
            scrollPosition,
            GUILayout.Height(position.height - 500)
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

        // TOTAL ENTRIES
        totalEntries = data.entries.Count;

        // TAG ANALYTICS
        Dictionary<string, int> tagCounts =
            new Dictionary<string, int>();

        foreach (var entry in data.entries)
        {
            string[] tags =
                entry.tags.Split(',');

            foreach (string rawTag in tags)
            {
                string tag = rawTag.Trim();

                if (string.IsNullOrEmpty(tag))
                    continue;

                if (!tagCounts.ContainsKey(tag))
                {
                    tagCounts[tag] = 0;
                }

                tagCounts[tag]++;
            }
        }

        // FIND TOP TAG
        int maxCount = 0;

        topTag = "None";

        // CATEGORY STATISTICS
        categoryStats.Clear();

        foreach (var entry in data.entries)
        {
            string category =
                entry.category.Trim();

            if (string.IsNullOrEmpty(category))
                continue;

            if (!categoryStats.ContainsKey(category))
            {
                categoryStats[category] = 0;
            }

            categoryStats[category]++;
        }

        foreach (var pair in tagCounts)
        {
            if (pair.Value > maxCount)
            {
                maxCount = pair.Value;

                topTag = pair.Key;
            }
        }

        if (data == null ||
            data.entries == null)
        {
            Debug.LogError(
                "No entries found"
            );
            return;
        }
        string query = searchQuery.ToLower();
        // TOKENIZE QUERY
        string[] tokens =
            query.Split(
            ' ',
            System.StringSplitOptions.RemoveEmptyEntries
            );
        if (string.IsNullOrWhiteSpace(query))
        {
            Debug.Log("Empty search query");
            return;
        }
        // ADD TO SEARCH HISTORY
        if (!searchHistory.Contains(searchQuery))
        {
            searchHistory.Insert(0, searchQuery);

            // LIMIT HISTORY SIZE
            if (searchHistory.Count > 10)
            {
                searchHistory.RemoveAt(10);
            }
        }
        foreach (var entry in data.entries)
        {

            bool match = false;

            int score = 0;
            entry.matchedFields.Clear();

            // =========================
            // TITLE
            // =========================
            foreach (string token in tokens)
            {
                if (entry.title.ToLower().Contains(token))
                {
                    match = true;

                    score += 5;

                    if (!entry.matchedFields.Contains("Title"))
                    {
                        entry.matchedFields.Add("Title");
                    }
                }
            }

            // =========================
            // TAGS
            // =========================
            foreach (string token in tokens)
            {
                if (entry.tags.ToLower().Contains(token))
                {
                    match = true;

                    score += 4;

                    if (!entry.matchedFields.Contains("Tags"))
                    {
                        entry.matchedFields.Add("Tags");
                    }
                }
            }

            // =========================
            // CATEGORY
            // =========================
            foreach (string token in tokens)
            {
                if (entry.category.ToLower().Contains(token))
                {
                    match = true;

                    score += 3;

                    if (!entry.matchedFields.Contains("Category"))
                    {
                        entry.matchedFields.Add("Category");
                    }
                }
            }

            // =========================
            // PROBLEM
            // =========================
            foreach (string token in tokens)
            {
                if (entry.problem.ToLower().Contains(token))
                {
                    match = true;

                    score += 2;

                    if (!entry.matchedFields.Contains("Problem"))
                    {
                        entry.matchedFields.Add("Problem");
                    }
                }
            }

            // =========================
            // SOLUTION
            // =========================
            foreach (string token in tokens)
            {
                if (entry.solution.ToLower().Contains(token))
                {
                    match = true;

                    score += 1;

                    if (!entry.matchedFields.Contains("Solution"))
                    {
                        entry.matchedFields.Add("Solution");
                    }
                }
            }

            // =========================
            // FILTER OVERRIDES
            // =========================
            switch (filters[filterIndex])
            {
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

            // STORE SCORE
            entry.relevanceScore = score;

            if (match)
            {
                searchResults.Add(entry);
            }
        }
        // =========================
        // SORT RESULTS
        switch (sortOptions[sortIndex])
        {
            case "Relevance":

                searchResults.Sort((a, b) =>
                    b.relevanceScore.CompareTo(
                        a.relevanceScore
                    )
                );

                break;

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

                // TAGS LABEL
                GUILayout.Label("Tags");

                // SPLIT TAGS
                string[] splitTags =
                    entry.tags.Split(',');

                // TAG BUTTON ROW
                GUILayout.BeginHorizontal();

                foreach (string rawTag in splitTags)
                {
                    string tag = rawTag.Trim();

                    if (GUILayout.Button(
                        HighlightMatch(tag),
                        richLabelStyle,
                        GUILayout.Height(25)
                    ))
                    {
                        searchQuery = tag;

                        // TAG FILTER INDEX
                        filterIndex = 4;

                        SearchKnowledge();

                        Debug.Log(
                            $"Quick Filter Tag: {tag}"
                        );
                    }
                }

                GUILayout.EndHorizontal();

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
            GUILayout.Label(
                $"Relevance Score: {entry.relevanceScore}"
            );
            GUILayout.Space(5);

            GUILayout.Label(
                "Matched Fields:",
                EditorStyles.boldLabel
            );

            foreach (string field in entry.matchedFields)
            {
                GUILayout.Label($"• {field}");
            }

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

        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        string highlightedText = text;

        string lowerText =
            text.ToLower();

        string lowerQuery =
            searchQuery.ToLower();

        int startIndex = 0;

        while (true)
        {
            int matchIndex =
                lowerText.IndexOf(
                    lowerQuery,
                    startIndex
                );

            if (matchIndex < 0)
            {
                break;
            }

            string originalMatch =
                highlightedText.Substring(
                    matchIndex,
                    searchQuery.Length
                );

            string highlightedMatch =
                $"<color=yellow><b>{originalMatch}</b></color>";

            highlightedText =
                highlightedText.Remove(
                    matchIndex,
                    searchQuery.Length
                );

            highlightedText =
                highlightedText.Insert(
                    matchIndex,
                    highlightedMatch
                );

            // UPDATE LOWER TEXT
            lowerText =
                highlightedText.ToLower();

            startIndex =
                matchIndex +
                highlightedMatch.Length;
        }

        return highlightedText;
    }
    void ExportResults()
    {
        if (searchResults.Count == 0)
        {
            Debug.Log("No search results to export");
            return;
        }

        string exportPath =
            Path.Combine(
                Application.dataPath,
                "knowledge_export.txt"
            );

        List<string> lines =
            new List<string>();

        foreach (var entry in searchResults)
        {
            lines.Add(
                "=================================="
            );

            lines.Add($"ID: {entry.id}");

            lines.Add($"Title: {entry.title}");

            lines.Add($"Category: {entry.category}");

            lines.Add($"Tags: {entry.tags}");

            lines.Add($"Problem: {entry.problem}");

            lines.Add($"Solution: {entry.solution}");

            lines.Add(
                $"Relevance Score: {entry.relevanceScore}"
            );

            lines.Add(
                $"Matched Fields: {string.Join(", ", entry.matchedFields)}"
            );

            lines.Add(
                $"Created: {entry.createdAt}"
            );

            lines.Add("");
        }

        File.WriteAllLines(exportPath, lines);

        AssetDatabase.Refresh();

        Debug.Log(
            $"Exported Results: {exportPath}"
        );
    }
    void ExportCSV()
    {
        if (searchResults.Count == 0)
        {
            Debug.Log(
                "No search results to export"
            );

            return;
        }

        string exportPath =
            Path.Combine(
                Application.dataPath,
                "knowledge_export.csv"
            );

        List<string> lines =
            new List<string>();

        // CSV HEADER
        lines.Add(
            "ID,Title,Category,Tags,Problem,Solution,RelevanceScore,MatchedFields,CreatedAt"
        );

        foreach (var entry in searchResults)
        {
            string line =
                $"\"{entry.id}\"," +
                $"\"{entry.title}\"," +
                $"\"{entry.category}\"," +
                $"\"{entry.tags}\"," +
                $"\"{entry.problem}\"," +
                $"\"{entry.solution}\"," +
                $"\"{entry.relevanceScore}\"," +
                $"\"{string.Join(" | ", entry.matchedFields)}\"," +
                $"\"{entry.createdAt}\"";

            lines.Add(line);
        }

        File.WriteAllLines(exportPath, lines);

        AssetDatabase.Refresh();

        Debug.Log(
            $"CSV Exported: {exportPath}"
        );
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
        [System.NonSerialized]
        public int relevanceScore;
        [System.NonSerialized]
        public List<string> matchedFields =
        new List<string>();
    }

    [System.Serializable]
    public class Wrapper
    {
        public List<Entry> entries;
    }
}