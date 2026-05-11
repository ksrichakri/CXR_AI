using UnityEditor;
using UnityEngine;
using System.IO;

public static class GitUtility
{
    public static void Validate()
    {
        if (!GitExecutor.Run("--version").Contains("git"))
        {
            EditorUtility.DisplayDialog("Error", "Git not installed", "OK");
            throw new System.Exception("Git missing");
        }

        if (!Directory.Exists(Application.dataPath + "/../.git"))
        {
            EditorUtility.DisplayDialog("Error", "Not a Git repository", "OK");
            throw new System.Exception("Not repo");
        }
    }
}