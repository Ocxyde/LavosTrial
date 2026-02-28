// DatabaseConfig.cs
// Lightweight config for SQLite DB path in Unity project
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System.IO;
using UnityEngine;

namespace Code.Lavos
{
    public static class DatabaseConfig
    {
    // Relative path within Unity project for the SQLite database
    public static string DbFolder => Path.Combine(Application.persistentDataPath, "DB_SQLite");
    public static string DbPath => Path.Combine(DbFolder, "GameData.db");

    // Editor path for easier debugging (optional)
    public static string EditorDbPath => Path.Combine(Application.dataPath, "DB_SQLite", "GameData.db");

    public static void EnsureFolderExists()
    {
        if (!Directory.Exists(DbFolder))
        {
            Directory.CreateDirectory(DbFolder);
            Debug.Log($"[DatabaseConfig] Created database folder: {DbFolder}");
        }
    }

    public static void EnsureEditorFolderExists()
    {
        string editorFolder = Path.Combine(Application.dataPath, "DB_SQLite");
        if (!Directory.Exists(editorFolder))
        {
            Directory.CreateDirectory(editorFolder);
            Debug.Log($"[DatabaseConfig] Created editor database folder: {editorFolder}");
        }
    }
}
}
