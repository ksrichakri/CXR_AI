using UnityEngine;
using UnityEditor;

public static class GitStyles
{
    public static GUIStyle Header;
    public static GUIStyle Box;

    static GitStyles()
    {
        Header = new GUIStyle(EditorStyles.boldLabel);
        Header.fontSize = 14;

        Box = new GUIStyle("box");
        Box.padding = new RectOffset(10, 10, 10, 10);
    }
}