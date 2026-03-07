// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using System;
using System.IO;
using UnityEngine;

namespace LavosTrial.Core.Maze
{
    // ─────────────────────────────────────────────────────────────
    //  MazeBinaryStorage8
    //
    //  Byte-for-byte binary persistence for MazeData8.
    //  Each cell is stored as a ushort (2 bytes, little-endian).
    //
    //  Save directory (resolved at runtime):
    //    EDITOR : <ProjectRoot>/Runtimes/Mazes/
    //    BUILD  : Application.persistentDataPath/Mazes/
    //
    //  File name: maze8_L{level:D3}_S{seed}.lvm
    //  Example  : maze8_L000_S-847291034.lvm
    //
    //  Binary layout (all fields little-endian):
    //  ┌─────────┬────────┬────────────────────────────────────┐
    //  │ Offset  │ Bytes  │ Field                              │
    //  ├─────────┼────────┼────────────────────────────────────┤
    //  │  0      │  5     │ Magic  "LAV8S"                     │
    //  │  5      │  1     │ Version (2)                        │
    //  │  6      │  2     │ Width   (int16)                    │
    //  │  8      │  2     │ Height  (int16)                    │
    //  │ 10      │  4     │ Seed    (int32)                    │
    //  │ 14      │  4     │ Level   (int32)                    │
    //  │ 18      │  8     │ Timestamp (int64, UTC unix secs)   │
    //  │ 26      │  2     │ SpawnX  (int16)                    │
    //  │ 28      │  2     │ SpawnZ  (int16)                    │
    //  │ 30      │  2     │ ExitX   (int16)                    │
    //  │ 32      │  2     │ ExitZ   (int16)                    │
    //  │ 34      │ W×H×2  │ Cell data — ushort per cell (LE)   │
    //  │ 34+W*H*2│  4     │ Checksum XOR-fold (uint32)         │
    //  └─────────┴────────┴────────────────────────────────────┘
    //  Total: 38 + (W * H * 2) bytes
    //    Level  0 (12×12) →   326 bytes
    //    Level  9 (21×21) →   924 bytes
    //    Level 39 (51×51) → 5,240 bytes
    // ─────────────────────────────────────────────────────────────
    public static class MazeBinaryStorage8
    {
        // ── Directory resolution ──────────────────────────────────
        private static string SaveDirectory
        {
            get
            {
#if UNITY_EDITOR
                string root = Application.dataPath.Replace("/Assets", "");
                return Path.Combine(root, "Runtimes", "Mazes");
#else
                return Path.Combine(Application.persistentDataPath, "Mazes");
#endif
            }
        }

        private static string FileName(int level, int seed)
            => $"maze8_L{level:D3}_S{seed}.lvm";

        public static string FullPath(int level, int seed)
            => Path.Combine(SaveDirectory, FileName(level, seed));

        // ─────────────────────────────────────────────────────────
        //  SAVE
        // ─────────────────────────────────────────────────────────
        public static bool Save(MazeData8 data)
        {
            try
            {
                Directory.CreateDirectory(SaveDirectory);
                string path = FullPath(data.Level, data.Seed);

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true);

                // ── Header (34 bytes) ─────────────────────────────
                bw.Write(MazeData8.MAGIC);                   //  5 bytes  "LAV8S"
                bw.Write(MazeData8.VERSION);                 //  1 byte
                bw.Write((short)data.Width);                 //  2 bytes
                bw.Write((short)data.Height);                //  2 bytes
                bw.Write(data.Seed);                         //  4 bytes
                bw.Write(data.Level);                        //  4 bytes
                bw.Write(data.Timestamp);                    //  8 bytes
                bw.Write((short)data.SpawnCell.x);           //  2 bytes
                bw.Write((short)data.SpawnCell.z);           //  2 bytes
                bw.Write((short)data.ExitCell.x);            //  2 bytes
                bw.Write((short)data.ExitCell.z);            //  2 bytes
                // ── Total header: 34 bytes ✓

                // ── Cell payload (W × H × 2 bytes) ───────────────
                // Row-major: z outer loop, x inner loop
                uint checksum = 0;
                for (int z = 0; z < data.Height; z++)
                for (int x = 0; x < data.Width;  x++)
                {
                    ushort cell = (ushort)data.GetCell(x, z);
                    bw.Write(cell);
                    // XOR fold both bytes into checksum
                    checksum ^= (uint)( cell        & 0xFF);
                    checksum ^= (uint)((cell >> 8)  & 0xFF) << 8;
                }

                // ── Checksum (mix in structural values) ───────────
                checksum ^= (uint)(data.Width  * 0x1F);
                checksum ^= (uint)(data.Height * 0x3D);
                checksum ^= (uint) data.Seed;
                checksum ^= 0xCAFE8888u;   // 8-axis signature constant

                bw.Write(checksum);

                // ── Flush to disk ─────────────────────────────────
                bw.Flush();
                File.WriteAllBytes(path, ms.ToArray());

                Debug.Log($"[MazeBinaryStorage8] Saved → {path}  ({ms.Length} bytes)");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MazeBinaryStorage8] Save failed: {ex.Message}");
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────
        //  LOAD
        // ─────────────────────────────────────────────────────────
        public static MazeData8 Load(int level, int seed)
        {
            string path = FullPath(level, seed);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[MazeBinaryStorage8] File not found: {path}");
                return null;
            }

            try
            {
                byte[] raw = File.ReadAllBytes(path);
                using var ms = new MemoryStream(raw);
                using var br = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true);

                // ── Validate magic ────────────────────────────────
                byte[] magic = br.ReadBytes(5);
                for (int i = 0; i < 5; i++)
                    if (magic[i] != MazeData8.MAGIC[i])
                        throw new InvalidDataException("Bad magic — not a LAV8S file.");

                byte version = br.ReadByte();
                if (version != MazeData8.VERSION)
                    throw new InvalidDataException($"Unsupported version: {version} (expected {MazeData8.VERSION}).");

                // ── Header ────────────────────────────────────────
                int  width     = br.ReadInt16();
                int  height    = br.ReadInt16();
                int  fileSeed  = br.ReadInt32();
                int  fileLevel = br.ReadInt32();
                long ts        = br.ReadInt64();   // timestamp kept for audit
                int  spawnX    = br.ReadInt16();
                int  spawnZ    = br.ReadInt16();
                int  exitX     = br.ReadInt16();
                int  exitZ     = br.ReadInt16();

                if (fileSeed != seed || fileLevel != level)
                    Debug.LogWarning("[MazeBinaryStorage8] Header seed/level mismatch.");

                // ── Cell payload ──────────────────────────────────
                var data = new MazeData8(width, height, fileSeed, fileLevel);

                uint checksum = 0;
                for (int z = 0; z < height; z++)
                for (int x = 0; x < width;  x++)
                {
                    ushort cell = br.ReadUInt16();
                    data.SetCell(x, z, (CellFlags8)cell);
                    checksum ^= (uint)( cell        & 0xFF);
                    checksum ^= (uint)((cell >> 8)  & 0xFF) << 8;
                }

                // ── Verify checksum ───────────────────────────────
                checksum ^= (uint)(width  * 0x1F);
                checksum ^= (uint)(height * 0x3D);
                checksum ^= (uint)fileSeed;
                checksum ^= 0xCAFE8888u;

                uint stored = br.ReadUInt32();
                if (checksum != stored)
                    Debug.LogWarning("[MazeBinaryStorage8] Checksum mismatch — file may be corrupt.");

                // ── Restore spawn / exit ──────────────────────────
                data.SetSpawn(spawnX, spawnZ);
                data.SetExit(exitX, exitZ);

                Debug.Log($"[MazeBinaryStorage8] Loaded ← {path}  ({raw.Length} bytes)");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MazeBinaryStorage8] Load failed: {ex.Message}");
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Utility
        // ─────────────────────────────────────────────────────────

        /// <summary>Returns true if a cached .lvm exists for this level/seed.</summary>
        public static bool Exists(int level, int seed)
            => File.Exists(FullPath(level, seed));

        /// <summary>Delete one cached maze file.</summary>
        public static bool Delete(int level, int seed)
        {
            string path = FullPath(level, seed);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            Debug.Log($"[MazeBinaryStorage8] Deleted: {path}");
            return true;
        }

        /// <summary>Wipe the entire Mazes cache directory.</summary>
        public static void PurgeAll()
        {
            if (!Directory.Exists(SaveDirectory)) return;
            foreach (string f in Directory.GetFiles(SaveDirectory, "*.lvm"))
                File.Delete(f);
            Debug.Log("[MazeBinaryStorage8] Cache purged.");
        }

        /// <summary>Human-readable file info for editor debugging.</summary>
        public static string FileInfo(int level, int seed)
        {
            string path = FullPath(level, seed);
            if (!File.Exists(path)) return "not cached";
            var fi = new FileInfo(path);
            return $"{fi.Length} bytes | modified {fi.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
