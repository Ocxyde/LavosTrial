// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details

using System;
using System.IO;
using UnityEngine;

namespace LavosTrial.Core.Maze
{
    // ─────────────────────────────────────────────────────────────
    //  MazeBinaryStorage
    //
    //  Writes / reads MazeData as a compact binary blob.
    //
    //  Save path (priority order):
    //    1. <ProjectRoot>/Runtimes/Mazes/  (editor / dev builds)
    //    2. Application.persistentDataPath/Mazes/  (shipping build)
    //
    //  File name: maze_L{level:D3}_S{seed}.lvm   (.lvm = LaVos Maze)
    //
    //  Binary layout  (all values little-endian):
    //  ┌──────────┬───────┬────────────────────────────────────┐
    //  │ Offset   │ Bytes │ Field                              │
    //  ├──────────┼───────┼────────────────────────────────────┤
    //  │  0       │  5    │ Magic  "LAVOS"                     │
    //  │  5       │  1    │ Version (1)                        │
    //  │  6       │  2    │ Width   (int16)                    │
    //  │  8       │  2    │ Height  (int16)                    │
    //  │ 10       │  4    │ Seed    (int32)                    │
    //  │ 14       │  4    │ Level   (int32)                    │
    //  │ 18       │  8    │ Timestamp (int64, UTC unix secs)   │
    //  │ 26       │  2    │ SpawnX  (int16)                    │
    //  │ 28       │  2    │ SpawnZ  (int16)                    │
    //  │ 30       │  2    │ ExitX   (int16)                    │
    //  │ 32       │  2    │ ExitZ   (int16)                    │
    //  │ 34       │ W×H   │ Cell flags (1 byte per cell)       │
    //  │ 34+W×H   │  4    │ Checksum XOR-fold (uint32)         │
    //  └──────────┴───────┴────────────────────────────────────┘
    //  Total header = 38 bytes  |  payload = W*H bytes
    // ─────────────────────────────────────────────────────────────
    public static class MazeBinaryStorage
    {
        // ── Directory resolution ──────────────────────────────────
        private static string SaveDirectory
        {
            get
            {
#if UNITY_EDITOR
                // In editor: save next to the project's Runtimes/ folder
                string projectRoot = Application.dataPath.Replace("/Assets", "");
                string dir = Path.Combine(projectRoot, "Runtimes", "Mazes");
#else
                string dir = Path.Combine(Application.persistentDataPath, "Mazes");
#endif
                return dir;
            }
        }

        private static string FileName(int level, int seed)
            => $"maze_L{level:D3}_S{seed}.lvm";

        public static string FullPath(int level, int seed)
            => Path.Combine(SaveDirectory, FileName(level, seed));

        // ─────────────────────────────────────────────────────────
        //  SAVE
        // ─────────────────────────────────────────────────────────
        public static bool Save(MazeData data)
        {
            try
            {
                // Ensure directory exists (already allocated folder re-used)
                Directory.CreateDirectory(SaveDirectory);

                string path = FullPath(data.Level, data.Seed);

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);

                // ── Header ────────────────────────────────────────
                bw.Write(MazeData.MAGIC);                    //  5 bytes
                bw.Write(MazeData.VERSION);                  //  1 byte
                bw.Write((short)data.Width);                 //  2 bytes
                bw.Write((short)data.Height);                //  2 bytes
                bw.Write(data.Seed);                         //  4 bytes
                bw.Write(data.Level);                        //  4 bytes
                bw.Write(data.Timestamp);                    //  8 bytes
                bw.Write((short)data.SpawnCell.x);           //  2 bytes
                bw.Write((short)data.SpawnCell.z);           //  2 bytes
                bw.Write((short)data.ExitCell.x);            //  2 bytes
                bw.Write((short)data.ExitCell.z);            //  2 bytes
                // Header total: 34 bytes ✓

                // ── Cell payload ──────────────────────────────────
                // Row-major order: for z in [0..H), for x in [0..W)
                uint checksum = 0;
                for (int z = 0; z < data.Height; z++)
                for (int x = 0; x < data.Width;  x++)
                {
                    byte b = (byte)data.GetCell(x, z);
                    bw.Write(b);
                    checksum ^= b;
                }

                // ── Checksum (simple XOR fold into uint32) ─────────
                // Mix in width, height, seed for extra integrity
                checksum ^= (uint)(data.Width  * 0x1F);
                checksum ^= (uint)(data.Height * 0x3D);
                checksum ^= (uint) data.Seed;
                bw.Write(checksum);

                // ── Flush to disk ─────────────────────────────────
                File.WriteAllBytes(path, ms.ToArray());

                Debug.Log($"[MazeBinaryStorage] Saved → {path}  ({ms.Length} bytes)");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MazeBinaryStorage] Save failed: {ex.Message}");
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────
        //  LOAD
        // ─────────────────────────────────────────────────────────
        public static MazeData Load(int level, int seed)
        {
            string path = FullPath(level, seed);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[MazeBinaryStorage] File not found: {path}");
                return null;
            }

            try
            {
                byte[] raw = File.ReadAllBytes(path);
                using var ms = new MemoryStream(raw);
                using var br = new BinaryReader(ms);

                // ── Validate magic ────────────────────────────────
                byte[] magic = br.ReadBytes(5);
                for (int i = 0; i < 5; i++)
                    if (magic[i] != MazeData.MAGIC[i])
                        throw new InvalidDataException("Bad magic bytes — not a .lvm file.");

                byte version = br.ReadByte();
                if (version != MazeData.VERSION)
                    throw new InvalidDataException($"Unsupported version {version}.");

                // ── Header ────────────────────────────────────────
                int width     = br.ReadInt16();
                int height    = br.ReadInt16();
                int fileSeed  = br.ReadInt32();
                int fileLevel = br.ReadInt32();
                long ts       = br.ReadInt64();
                int spawnX    = br.ReadInt16();
                int spawnZ    = br.ReadInt16();
                int exitX     = br.ReadInt16();
                int exitZ     = br.ReadInt16();

                if (fileSeed != seed || fileLevel != level)
                    Debug.LogWarning("[MazeBinaryStorage] Seed/Level mismatch in file header.");

                // ── Cell payload ──────────────────────────────────
                var data = new MazeData(width, height, fileSeed, fileLevel);

                uint checksum = 0;
                for (int z = 0; z < height; z++)
                for (int x = 0; x < width;  x++)
                {
                    byte b = br.ReadByte();
                    data.SetCell(x, z, (CellFlags)b);
                    checksum ^= b;
                }

                // ── Verify checksum ───────────────────────────────
                checksum ^= (uint)(width  * 0x1F);
                checksum ^= (uint)(height * 0x3D);
                checksum ^= (uint)fileSeed;

                uint storedChecksum = br.ReadUInt32();
                if (checksum != storedChecksum)
                    Debug.LogWarning("[MazeBinaryStorage] Checksum mismatch — file may be corrupt.");

                // ── Restore spawn / exit ──────────────────────────
                data.SetSpawn(spawnX, spawnZ);
                data.SetExit(exitX, exitZ);

                Debug.Log($"[MazeBinaryStorage] Loaded ← {path}  ({raw.Length} bytes)");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MazeBinaryStorage] Load failed: {ex.Message}");
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────
        //  EXISTS  — check cache before re-generating
        // ─────────────────────────────────────────────────────────
        public static bool Exists(int level, int seed)
            => File.Exists(FullPath(level, seed));

        // ─────────────────────────────────────────────────────────
        //  DELETE — remove a specific cached maze
        // ─────────────────────────────────────────────────────────
        public static bool Delete(int level, int seed)
        {
            string path = FullPath(level, seed);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            Debug.Log($"[MazeBinaryStorage] Deleted: {path}");
            return true;
        }

        // ─────────────────────────────────────────────────────────
        //  PURGE — clear the entire Mazes cache folder
        // ─────────────────────────────────────────────────────────
        public static void PurgeAll()
        {
            if (!Directory.Exists(SaveDirectory)) return;
            foreach (string f in Directory.GetFiles(SaveDirectory, "*.lvm"))
                File.Delete(f);
            Debug.Log("[MazeBinaryStorage] Cache purged.");
        }

        // ─────────────────────────────────────────────────────────
        //  INFO — human-readable file stats (editor debug)
        // ─────────────────────────────────────────────────────────
        public static string FileInfo(int level, int seed)
        {
            string path = FullPath(level, seed);
            if (!File.Exists(path)) return "not cached";
            var fi = new FileInfo(path);
            return $"{fi.Length} bytes | modified {fi.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
