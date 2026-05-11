using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class GitUIController
{
    private VisualElement root;
    private ScrollView changes;
    private TextField output;

    public GitUIController(VisualElement root)
    {
        this.root = root;

        changes = root.Q<ScrollView>("changesList");
        output = root.Q<TextField>("outputLog");

        SetupButtons();
        LoadBranches();
        RefreshChanges();
    }

    void SetupButtons()
    {
        var refresh = root.Q<Button>("refreshBtn");
        var push = root.Q<Button>("pushBtn");
        var commit = root.Q<Button>("commitBtn");

        if (refresh != null)
            refresh.clicked += RefreshChanges;

        if (push != null)
            push.clicked += () => Run("push");

        if (commit != null)
            commit.clicked += Commit;
    }

    void Run(string cmd)
    {
        Append($"\n> git {cmd}\n");
        Append(GitExecutor.Run(cmd));

        RefreshChanges();
    }

    void Commit()
    {
        var msg = root.Q<TextField>("commitMessage")?.value;

        SmartCommit.Execute(msg, Append);
    }

    void Append(string text)
    {
        if (output != null)
            output.value += text;
    }

    // =========================
    // STATUS / CHANGES LIST
    // =========================
    public void RefreshChanges()
    {
        if (changes == null)
            return;

        changes.Clear();

        string status = GitExecutor.Run("status --porcelain");

        var lines = status.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // SAFETY CHECK
            if (line.Length < 4)
                continue;

            string code = line.Substring(0, 2).Trim();
            string file = line.Substring(3);

            var row = new VisualElement();
            row.AddToClassList("file-row");

            var toggle = new Toggle
            {
                value = true
            };

            toggle.AddToClassList("file-toggle");

            var statusLabel = new Label(code);
            statusLabel.AddToClassList("file-status");

            var fileLabel = new Label(file);
            fileLabel.AddToClassList("file-label");

            row.Add(toggle);
            row.Add(statusLabel);
            row.Add(fileLabel);

            changes.Add(row);
        }
    }

    // =========================
    // BRANCH DROPDOWN
    // =========================
    void LoadBranches()
    {
        var container = root.Q<VisualElement>("branchContainer");

        if (container == null)
            return;

        string result = GitExecutor.Run("branch");

        var lines = result.Split('\n');

        List<string> branches = new List<string>();

        string current = "";

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string name = line.Replace("*", "").Trim();

            if (string.IsNullOrEmpty(name))
                continue;

            branches.Add(name);

            if (line.StartsWith("*"))
                current = name;
        }

        // FALLBACK IF NO BRANCHES FOUND
        if (branches.Count == 0)
        {
            branches.Add("main");
            current = "main";
        }

        // FALLBACK IF CURRENT IS EMPTY
        if (string.IsNullOrEmpty(current))
        {
            current = branches[0];
        }

        var dropdown = new PopupField<string>(
            "",
            branches,
            current
        );

        dropdown.RegisterValueChangedCallback(evt =>
        {
            Run($"checkout {evt.newValue}");
        });

        container.Clear();
        container.Add(dropdown);
    }
}