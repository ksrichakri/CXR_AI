using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class KnowledgeAssistantWindow : EditorWindow
{
    private string entryTitle = "";
    private string category = "";
    private string tags = "";
    private string problem = "";
    private string solution = "";

    private string filePath;

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

        entryTitle = EditorGUILayout.TextField("Title", entryTitle);

        category = EditorGUILayout.TextField("Category", category);

        tags = EditorGUILayout.TextField("Tags", tags);

        GUILayout.Label("Problem");
        problem = EditorGUILayout.TextArea(problem, GUILayout.Height(80));

        GUILayout.Label("Solution");
        solution = EditorGUILayout.TextArea(solution, GUILayout.Height(80));

        GUILayout.Space(10);

        if (GUILayout.Button("Save Entry"))
        {
            SaveEntry();
        }
    }

    void SaveEntry()
    {
        Entry newEntry = new Entry()
        {
            title = entryTitle,
            category = category,
            tags = tags,
            problem = problem,
            solution = solution
        };

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

        // ADD NEW ENTRY
        entries.Add(newEntry);

        // CREATE WRAPPER
        Wrapper wrapper = new Wrapper();
        wrapper.entries = entries;

        // SAVE JSON
        string newJson = JsonUtility.ToJson(wrapper, true);

        File.WriteAllText(filePath, newJson);

        AssetDatabase.Refresh();

        Debug.Log("Knowledge Entry Saved");

        // CLEAR FIELDS
        entryTitle = "";
        category = "";
        tags = "";
        problem = "";
        solution = "";
    }

    [System.Serializable]
    public class Entry
    {
        public string title;
        public string category;
        public string tags;
        public string problem;
        public string solution;
    }

    [System.Serializable]
    public class Wrapper
    {
        public List<Entry> entries;
    }
}