using System.Diagnostics;
using UnityEngine;

public static class GitExecutor
{
    public static string Run(string args)
    {
        try
        {
            Process p = new Process();
            p.StartInfo.FileName = "git";
            p.StartInfo.Arguments = args;
            p.StartInfo.WorkingDirectory = Application.dataPath + "/..";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;

            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            p.WaitForExit();

            return output + error;
        }
        catch (System.Exception e)
        {
            return "Error: " + e.Message;
        }
    }
}