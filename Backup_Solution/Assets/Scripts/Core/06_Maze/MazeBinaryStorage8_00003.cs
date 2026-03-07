// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Code.Lavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Code.Lavos.  If not, see <https://www.gnu.org/licenses/>.
// MazeBinaryStorage8.cs
// Byte-exact binary persistence for MazeData8
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System;
using System.IO;
using UnityEngine;

namespace Code.Lavos.Core
{
    // ─────────────────────────────────────────────────────────────
    //  MazeBinaryStorage8
    //
    //  Byte-exact binary persistence for MazeData8.
    //  Each cell = 1 ushort (2 bytes, little-endian).
    //  DifficultyFactor stored in header (float32).
    //
    //  Save path:
    //    EDITOR : <ProjectRoot>/Runtimes/Mazes/
    //    BUILD  : Application.persistentDataPath/Mazes/
    //
    //  File name: maze8_L{level:D3}_S{seed}.lvm
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
    //  │ 34      │  4     │ DifficultyFactor (float32)         │
    //  │ 38      │ W×H×2  │ Cell data — ushort per cell (LE)   │
    //  │ 38+W*H*2│  4     │ Checksum XOR-fold (uint32)         │
    //  └─────────┴────────┴────────────────────────────────────┘
    //  Total: 42 + (W × H × 2) bytes
    //    Level  0 (12×12) →   330 bytes
    //    Level 39 (51×51) → 5,244 bytes
    // ─────────────────────────────────────────────────────────────
    public static class MazeBinaryStorage8
    {
        // ── Directory ─────────────────────────────────────────────
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
                using var bw = new BinaryWriter(ms,
                                   System.Text.Encoding.UTF8, leaveOpen: true);

                // ── Header (38 bytes) ─────────────────────────────
                bw.Write(MazeData8.MAGIC);                   //  5  "LAV8S"
                bw.Write(MazeData8.VERSION);                 //  1
                bw.Write((short)data.Width);                 //  2
                bw.Write((short)data.Height);                //  2
                bw.Write(data.Seed);                         //  4
                bw.Write(data.Level);                        //  4
                bw.Write(data.Timestamp);                    //  8
                bw.Write((short)data.SpawnCell.x);           //  2
                bw.Write((short)data.SpawnCell.z);           //  2
                bw.Write((short)data.ExitCell.x);            //  2
                bw.Write((short)data.ExitCell.z);            //  2
                bw.Write(data.DifficultyFactor);             //  4  float32
                // Header total: 38 bytes ✓

                // ── Cell payload (W × H × 2 bytes) ───────────────
                uint checksum = 0;
                for (int z = 0; z < data.Height; z++)
                for (int x = 0; x < data.Width;  x++)
                {
                    ushort cell = (ushort)data.GetCell(x, z);
                    bw.Write(cell);
                    checksum ^=  (uint)( cell       & 0xFF);
                    checksum ^= ((uint)((cell >> 8) & 0xFF)) << 8;
                }

                // ── Checksum ──────────────────────────────────────
                checksum ^= (uint)(data.Width  * 0x1F);
                checksum ^= (uint)(data.Height * 0x3D);
                checksum ^= (uint) data.Seed;
                checksum ^= 0xCAFE8888u;   // LAV8S signature
                bw.Write(checksum);

                bw.Flush();
                File.WriteAllBytes(path, ms.ToArray());
                Debug.Log($"[MazeBinaryStorage8] Saved → {path}  ({ms.Length} bytes)  " +
                          $"factor={data.DifficultyFactor:F3}");
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
                Debug.LogWarning($"[MazeBinaryStorage8] Not found: {path}");
                return null;
            }

            try
            {
                byte[] raw = File.ReadAllBytes(path);
                using var ms = new MemoryStream(raw);
                using var br = new BinaryReader(ms,
                                   System.Text.Encoding.UTF8, leaveOpen: true);

                // ── Validate magic ────────────────────────────────
                byte[] magic = br.ReadBytes(5);
                for (int i = 0; i < 5; i++)
                    if (magic[i] != MazeData8.MAGIC[i])
                        throw new InvalidDataException("Bad magic — not a LAV8S file.");

                byte version = br.ReadByte();
                if (version != MazeData8.VERSION)
                    throw new InvalidDataException(
                        $"Unsupported version {version} (expected {MazeData8.VERSION}).");

                // ── Header ────────────────────────────────────────
                int   width    = br.ReadInt16();
                int   height   = br.ReadInt16();
                int   fileSeed = br.ReadInt32();
                int   fileLvl  = br.ReadInt32();
                long  ts       = br.ReadInt64();
                int   spawnX   = br.ReadInt16();
                int   spawnZ   = br.ReadInt16();
                int   exitX    = br.ReadInt16();
                int   exitZ    = br.ReadInt16();
                float factor   = br.ReadSingle();   // DifficultyFactor

                if (fileSeed != seed || fileLvl != level)
                    Debug.LogWarning("[MazeBinaryStorage8] Header seed/level mismatch.");

                // ── Cell payload ──────────────────────────────────
                var data = new MazeData8(width, height, fileSeed, fileLvl)
                {
                    DifficultyFactor = factor,
                };

                uint checksum = 0;
                for (int z = 0; z < height; z++)
                for (int x = 0; x < width;  x++)
                {
                    ushort cell = br.ReadUInt16();
                    data.SetCell(x, z, (CellFlags8)cell);
                    checksum ^=  (uint)( cell       & 0xFF);
                    checksum ^= ((uint)((cell >> 8) & 0xFF)) << 8;
                }

                // ── Verify checksum ───────────────────────────────
                checksum ^= (uint)(width  * 0x1F);
                checksum ^= (uint)(height * 0x3D);
                checksum ^= (uint)fileSeed;
                checksum ^= 0xCAFE8888u;

                uint stored = br.ReadUInt32();
                if (checksum != stored)
                    Debug.LogWarning("[MazeBinaryStorage8] Checksum mismatch — file may be corrupt.");

                data.SetSpawn(spawnX, spawnZ);
                data.SetExit(exitX, exitZ);

                Debug.Log($"[MazeBinaryStorage8] Loaded ← {path}  ({raw.Length} bytes)  " +
                          $"factor={factor:F3}");
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
        public static bool   Exists(int level, int seed) => File.Exists(FullPath(level, seed));

        public static bool Delete(int level, int seed)
        {
            string p = FullPath(level, seed);
            if (!File.Exists(p)) return false;
            File.Delete(p);
            Debug.Log($"[MazeBinaryStorage8] Deleted: {p}");
            return true;
        }

        public static void PurgeAll()
        {
            if (!Directory.Exists(SaveDirectory)) return;
            foreach (string f in Directory.GetFiles(SaveDirectory, "*.lvm"))
                File.Delete(f);
            Debug.Log("[MazeBinaryStorage8] Cache purged.");
        }

        public static string FileInfo(int level, int seed)
        {
            string p = FullPath(level, seed);
            if (!File.Exists(p)) return "not cached";
            var fi = new FileInfo(p);
            return $"{fi.Length} bytes | modified {fi.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
