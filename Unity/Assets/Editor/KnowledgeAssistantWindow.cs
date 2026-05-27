using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public class KnowledgeAssistantWindow : EditorWindow
{
    private string entryTitle = "";
    private string category = "";
    private string tags = "";
    private string problem = "";
    private string solution = "";

    private string filePath;
    private const string API_URL =
        "http://127.0.0.1:8000/query";

    private bool sendToBackend = true;
    [MenuItem("Tools/Knowledge Assistant")]
    public static void ShowWindow()
    {
        GetWindow<KnowledgeAssistantWindow>("Knowledge Assistant");
    }

    private void OnEnable()
    {
        filePath = Path.Combine(Application.dataPath, "knowledge.json");
    }

    private void OnGUI()
    {
        GUILayout.Label("Knowledge Entry Form", EditorStyles.boldLabel);

        GUILayout.Space(5);

        entryTitle = EditorGUILayout.TextField("Title", entryTitle);

        category = EditorGUILayout.TextField("Category", category);

        tags = EditorGUILayout.TextField("Tags", tags);

        GUILayout.Space(5);

        sendToBackend =
            EditorGUILayout.Toggle(
                "Send To Backend",
                sendToBackend
            );

        GUILayout.Space(5);

        GUILayout.Label("Problem");
        problem = EditorGUILayout.TextArea(problem, GUILayout.Height(80));

        GUILayout.Space(5);

        GUILayout.Label("Solution");
        solution = EditorGUILayout.TextArea(solution, GUILayout.Height(80));

        GUILayout.Space(10);

        if (GUILayout.Button("Save Entry", GUILayout.Height(30)))
        {
            SaveEntry();
        }
    }

    // =========================================
    // SAVE ENTRY
    // =========================================
    void SaveEntry()
    {
        List<Entry> entries = new List<Entry>();

        // LOAD EXISTING JSON
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);

            Wrapper existingData = JsonUtility.FromJson<Wrapper>(json);

            if (existingData != null && existingData.entries != null)
            {
                entries = existingData.entries;
            }
        }

        // GENERATE UNIQUE ID
        string generatedID = GenerateID(entries.Count);

        // CREATE NEW ENTRY
        Entry newEntry = new Entry()
        {
            id = generatedID,
            title = entryTitle,
            category = category,
            tags = tags,
            problem = problem,
            solution = solution,
            createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        // ADD ENTRY
        entries.Add(newEntry);

        // CREATE WRAPPER
        Wrapper wrapper = new Wrapper();
        wrapper.entries = entries;

        // CONVERT TO JSON
        string newJson = JsonUtility.ToJson(wrapper, true);

        // SAVE FILE
        File.WriteAllText(filePath, newJson);

        // REFRESH UNITY
        AssetDatabase.Refresh();

        Debug.Log($"Knowledge Entry Saved: {generatedID}");

        if (sendToBackend)
        {
            SendEntryToBackend(newEntry);
        }

        // CLEAR FORM
        entryTitle = "";
        category = "";
        tags = "";
        problem = "";
        solution = "";
    }

    async void SendEntryToBackend(
    Entry entry
)
    {
        BackendEntry backendEntry =
            new BackendEntry();

        backendEntry.id =
            entry.id;

        backendEntry.title =
            entry.title;

        backendEntry.category =
            entry.category;

        backendEntry.problem =
            entry.problem;

        backendEntry.solution =
            entry.solution;

        backendEntry.codeSnippet = "";

        backendEntry.tags =
            new List<string>();

        foreach (string tag in entry.tags.Split(','))
        {
            backendEntry.tags.Add(tag.Trim());
        }

        string json =
            JsonUtility.ToJson(
                backendEntry,
                true
            );

        Debug.Log(
            "SENDING ENTRY TO BACKEND:\n" +
            json
        );

        byte[] bodyRaw =
            Encoding.UTF8.GetBytes(json);

        UnityWebRequest request =
            new UnityWebRequest(
                API_URL,
                "POST"
            );

        request.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        var operation =
            request.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            Debug.Log(
                "Backend Entry Saved Successfully"
            );

            Debug.Log(
                request.downloadHandler.text
            );
        }
        else
        {
            Debug.LogError(
                "BACKEND ERROR:\n" +
                request.responseCode +
                "\n" +
                request.downloadHandler.text
            );
        }
    }

    // =========================================
    // GENERATE UNIQUE ID
    // =========================================
    string GenerateID(int count)
    {
        return $"KB-{(count + 1).ToString("D4")}";
    }

    // =========================================
    // ENTRY MODEL
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
    public class BackendEntry
    {
        public string id;

        public string title;

        public string category;

        public List<string> tags;

        public string problem;

        public string solution;

        public string codeSnippet;
    }

    // =========================================
    // JSON WRAPPER
    // =========================================
    [System.Serializable]
    public class Wrapper
    {
        public List<Entry> entries;
    }
}