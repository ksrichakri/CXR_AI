using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;

public static class SmartCommit
{
    public static async void Execute(string msg, System.Action<string> log)
    {
        GitUtility.Validate();

        if (string.IsNullOrEmpty(msg))
        {
            EditorUtility.DisplayDialog("Error", "Empty commit message", "OK");
            return;
        }

        log("Smart Commit Started...\n");

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();
        await Task.Delay(1200);

        GitExecutor.Run("add .");
        await Task.Delay(800);
        GitExecutor.Run("add .");

        string status = GitExecutor.Run("status");

        if (status.Contains("nothing to commit"))
        {
            log("Nothing to commit\n");
            return;
        }

        log(GitExecutor.Run($"commit -m \"{msg}\"") + "\n");
        log(GitExecutor.Run("push") + "\n");

        log("Done\n");
    }
}