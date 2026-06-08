using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class KnowledgeSearchWindow : EditorWindow
{
    private string searchQuery = "";
    private Vector2 resultsScrollPosition;
    private List<Entry> searchResults = new List<Entry>();
  
    // COLLAPSIBLE STATE
    private Dictionary<int, bool> expandedEntries =
        new Dictionary<int, bool>();
    private int currentlyEditingID = -1;

    // FILTER SYSTEM
    private int filterIndex = 0;

    private string[] filters =
    {
        "All",
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
    "Title"
    };
    private string filePath;
    private Vector2 mainScrollPosition;
    private GUIStyle richLabelStyle;
    private bool useBackendSearch = true;
    private bool isSearching = false;
    private string backendStatus = "Unknown";
    private string backendErrorMessage = "";
    private string lastSynced = "Never";
    [MenuItem("Tools/Knowledge Search")]
    public static void ShowWindow()
    {
        var window =
            GetWindow<KnowledgeSearchWindow>(
                "Knowledge Search"
            );

        window.minSize =
            new Vector2(700, 700);
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
        mainScrollPosition =
            EditorGUILayout.BeginScrollView(
                mainScrollPosition
            );
        GUILayout.Space(10);
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
        GUILayout.Label(
            "Search",
            EditorStyles.boldLabel
        );

        searchQuery = EditorGUILayout.TextField(
            searchQuery,
            GUILayout.Height(28),
            GUILayout.ExpandWidth(true)
        );

        GUILayout.Space(10);

        useBackendSearch =
            EditorGUILayout.Toggle(
            "Use Backend Search",
            useBackendSearch
            );

        GUILayout.Space(10);

        if (isSearching)
        {
            EditorGUILayout.HelpBox(
                "Searching Backend...",
                MessageType.Info
            );
        }
        if (useBackendSearch)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(
                $"Backend: {backendStatus}"
            );

            GUILayout.Label(
                $"Last Sync: {lastSynced}"
            );

            GUILayout.EndHorizontal();
        }
        if (!string.IsNullOrEmpty(
            backendErrorMessage))
        {
            EditorGUILayout.HelpBox(
                backendErrorMessage,
                MessageType.Error
            );
        }
        // FILTER DROPDOWN
        GUILayout.BeginHorizontal();

        filterIndex = EditorGUILayout.Popup(
            "Filter",
            filterIndex,
            filters
        );
        //SORT DROPDOWN
        sortIndex = EditorGUILayout.Popup(
            "Sort",
            sortIndex,
            sortOptions
        );

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // =========================
        // BUTTONS
        // =========================
        GUILayout.BeginHorizontal(
            GUILayout.ExpandWidth(true)
        );
        GUILayout.Space(5);
        if (GUILayout.Button(
            "Search",
            GUILayout.Height(35),
            GUILayout.ExpandWidth(true)
        ))
        {
            SearchKnowledge();
        }
        GUILayout.Space(5);
        if (GUILayout.Button(
            "Clear",
            GUILayout.Height(35),
            GUILayout.ExpandWidth(true)
        ))
        {
            searchQuery = "";
            searchResults.Clear();
        }

        GUILayout.EndHorizontal();

        

        GUILayout.Space(10);

        GUILayout.BeginHorizontal(
            GUILayout.ExpandWidth(true)
        );

        if (GUILayout.Button(
            "Export TXT",
            GUILayout.Height(30),
            GUILayout.ExpandWidth(true)
        ))
        {
            ExportResults();
        }

        if (GUILayout.Button(
            "Export CSV",
            GUILayout.Height(30),
            GUILayout.ExpandWidth(true)
        ))
        {
            ExportCSV();
        }

        GUILayout.EndHorizontal();


        // =========================
        // RESULT COUNT
        // =========================
        GUILayout.Space(10);

        GUILayout.Box(
            "",
            GUILayout.ExpandWidth(true),
            GUILayout.Height(1)
        );

        GUILayout.Space(10);

        GUILayout.Label(
            $"Search Results ({searchResults.Count})",
            EditorStyles.boldLabel
        );
        GUILayout.Space(5);

        // =========================
        // RESULTS SECTION
        // =========================
        GUILayout.BeginVertical(
            "box",
            GUILayout.ExpandWidth(true)
        );

        if (searchResults.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No search results found.",
                MessageType.Info
            );
        }
        else
        {
            resultsScrollPosition =
                EditorGUILayout.BeginScrollView(
                    resultsScrollPosition,
                    GUILayout.Height(400)
                );

            foreach (var entry in searchResults)
            {
                DrawEntry(entry);
            }

            EditorGUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
        void GenerateSearchRequestPayload()
    {
        SearchRequest request =
            new SearchRequest();

        request.query =
            searchQuery;

        request.filter =
            filters[filterIndex];

        request.sort =
            sortOptions[sortIndex];

        //request.timestamp =
        //    System.DateTime.Now.ToString(
        //        "yyyy-MM-dd HH:mm:ss"
        //    );

        string json =
            JsonUtility.ToJson(
                request,
                true
            );

        Debug.Log(
            "SEARCH REQUEST JSON:\n" +
            json
        );
    }

    async void SendSearchRequestToBackend()
    {
        isSearching = true;
        backendErrorMessage = "";
        Repaint();
        SearchRequest request =
            new SearchRequest();

        request.query = searchQuery;

        request.filter =
            filters[filterIndex];

        request.sort =
            sortOptions[sortIndex];

        //request.timestamp =
        //    System.DateTime.Now.ToString(
        //        "yyyy-MM-dd HH:mm:ss"
        //    );

        string json =
            JsonUtility.ToJson(
                request,
                true
            );


        byte[] bodyRaw =
            Encoding.UTF8.GetBytes(json);

        UnityWebRequest webRequest =
            new UnityWebRequest(
                "http://172.17.31.36:8000/search",
                "POST"
            );

        webRequest.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        webRequest.downloadHandler =
            new DownloadHandlerBuffer();

        webRequest.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        var operation =
            webRequest.SendWebRequest();

        while (!operation.isDone)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        if (webRequest.result ==
            UnityWebRequest.Result.Success)
        {
            string responseJson =
                webRequest.downloadHandler.text;

            Debug.Log(
                "BACKEND RESPONSE:\n" +
                responseJson
            );

            string wrappedJson =
                "{ \"entries\": " +
                responseJson +
                "}";

            BackendEntryList parsedResults =
                JsonUtility.FromJson<BackendEntryList>(
                    wrappedJson
                );

            if (parsedResults != null &&
                parsedResults.entries != null)
            {
                searchResults.Clear();

                foreach (var result in parsedResults.entries)
                {
                    searchResults.Add(result);
                }

                Repaint();

                Debug.Log(
                    $"Backend Returned: {searchResults.Count} result(s)"
                );
                backendStatus = "Connected";
                backendErrorMessage = "";
                lastSynced =
                    System.DateTime.Now.ToString(
                        "HH:mm:ss"
                    );
            }

            
        }
        else
        {
            Debug.LogError(
                "BACKEND ERROR:\n" +
                "Status Code: " +
                webRequest.responseCode +
                "\nResponse:\n" +
                webRequest.downloadHandler.text
            );
            backendStatus = "Offline";
            backendErrorMessage =
                "Backend unavailable";
        }
        isSearching = false;
        Repaint();
        
    }
    // =========================================
    // SEARCH LOGIC
    // =========================================
    void SearchKnowledge()
    {
        searchResults.Clear();

        if (useBackendSearch)
        {
            SendSearchRequestToBackend();
            return;
        }

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
        string query =
    searchQuery.ToLower();

        string[] tokens =
            query.Split(
                ' ',
                System.StringSplitOptions.RemoveEmptyEntries
            );
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
                if (string.Join(",", entry.tags).ToLower().Contains(token))
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
                        string.Join(",", entry.tags).ToLower().Contains(query);
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
            $"{arrow} {HighlightMatch(entry.title)}",
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

                string tagsString =
                    string.Join(",", entry.tags);

                tagsString =
                    EditorGUILayout.TextField(tagsString);

                entry.tags =
                    new List<string>(
                        tagsString.Split(',')
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
                List<string> splitTags =
                    entry.tags;

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

                    currentlyEditingID = -1;

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
        GUILayout.Space(8);
    }

    void DeleteEntry(int entryID)
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
            lines.Add($"Title: {entry.title}");

            lines.Add($"Category: {entry.category}");

            lines.Add($"Tags: {string.Join(",", entry.tags)}");

            lines.Add($"Problem: {entry.problem}");

            lines.Add($"Solution: {entry.solution}");

            

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

    BackendEntry ConvertToBackendEntry(
    Entry entry
)
    {
        BackendEntry backendEntry =
            new BackendEntry();


        backendEntry.title =
            entry.title;

        backendEntry.category =
            entry.category;

        backendEntry.problem =
            entry.problem;

        backendEntry.solution =
            entry.solution;

        backendEntry.codeSnippet = "";

        // CONVERT TAGS STRING → LIST
        backendEntry.tags =
            new List<string>();

        backendEntry.tags =
            new List<string>(entry.tags);

        return backendEntry;
    }

    void GenerateBackendPayload(
    Entry entry
    )
    {
        BackendEntry backendEntry =
            ConvertToBackendEntry(entry);

        string json =
            JsonUtility.ToJson(
                backendEntry,
                true
            );

        Debug.Log(
            "BACKEND PAYLOAD:\n" +
            json
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
            "Title,Category,Tags,Problem,Solution,CreatedAt"
        );

        foreach (var entry in searchResults)
        {
            string line =
                $"\"{entry.title}\"," +
                $"\"{entry.category}\"," +
                $"\"{string.Join(",", entry.tags)}\"," +
                $"\"{entry.problem}\"," +
                $"\"{entry.solution}\"," +
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
        public int id;

        public string title;

        public string category;

        public List<string> tags;

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
    public class BackendEntry
    {
        public string title;

        public string category;

        public string problem;

        public string solution;

        public string codeSnippet;

        public List<string> tags;
    }

    [System.Serializable]
    public class SearchRequest
    {
        public string query;

        public string filter;

        public string sort;

        //public string timestamp;
    }

    [System.Serializable]
    public class BackendEntryList
    {
        public List<Entry> entries;
    }

    [System.Serializable]
    public class Wrapper
    {
        public List<Entry> entries;
    }
}