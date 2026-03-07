// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
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
