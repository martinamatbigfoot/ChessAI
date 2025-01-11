using System.IO;
using UnityEditor;
using UnityEngine;

namespace RookWorks.Controller
{
    public static class FileLoader
    {
        public static string LoadFile()
        {
#if UNITY_EDITOR
            // File Picker for macOS/Windows in Unity Editor
            string path = EditorUtility.OpenFilePanel("Select File", "", "pgn");
            if (!string.IsNullOrEmpty(path))
            {
                string content = File.ReadAllText(path);
                Debug.Log($"File Content:\n{content}");
                return content;
            }
#endif
            return null;
        }
    }
}
