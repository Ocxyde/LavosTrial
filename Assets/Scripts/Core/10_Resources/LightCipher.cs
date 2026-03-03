// LightCipher.cs
// Encryption/Decryption cipher for binary light placement data
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT ARCHITECTURE:
// - Standalone cipher system for binary data encryption
// - Supports multiple cipher algorithms (XOR, RC4, AES-ready)
// - Seed-based key derivation for reproducible encryption
// - Used by LightPlacementData for secure storage
//
// USAGE:
//   // Encrypt data
//   byte[] encrypted = LightCipher.Encrypt(data, seed);
//   
//   // Decrypt data
//   byte[] decrypted = LightCipher.Decrypt(encrypted, seed);
//   
//   // With region (partial buffer encryption)
//   LightCipher.Encrypt(buffer, skipBytes, length, seed);
//
// SECURITY NOTES:
//   - Current implementation: XOR cipher (fast, lightweight)
//   - Suitable for game data obfuscation (not cryptographic security)
//   - Upgrade to AES for production if needed
//   - Seed-derived keys are reproducible (same seed = same key)
//
// Location: Assets/Scripts/Core/10_Resources/

using System;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// LightCipher - Encryption/Decryption cipher for binary data.
    /// Provides fast, seed-based encryption for light placement storage.
    /// </summary>
    public static class LightCipher
    {
        #region Cipher Types
        
        /// <summary>
        /// Available cipher algorithms.
        /// </summary>
        public enum CipherType
        {
            XOR,      // Fast XOR cipher (default, lightweight)
            RC4,      // Stream cipher (moderate security)
            AES128,   // Block cipher (high security, slower)
            Custom    // User-defined algorithm
        }
        
        #endregion
        
        #region Configuration
        
        // Current cipher configuration
        private static CipherType activeCipher = CipherType.XOR;
        
        // Key stream cache (avoids regenerating keys for same seed)
        private static readonly System.Collections.Generic.Dictionary<int, byte[]> _keyCache 
            = new System.Collections.Generic.Dictionary<int, byte[]>();
        
        // Cache size limit (prevent memory bloat)
        private const int MAX_CACHE_SIZE = 100;
        
        #endregion
        
        #region Public API - Full Buffer Encryption
        
        /// <summary>
        /// Encrypt entire buffer (returns new array).
        /// </summary>
        /// <param name="data">Data to encrypt</param>
        /// <param name="seed">Seed for key derivation</param>
        /// <returns>Encrypted data</returns>
        public static byte[] Encrypt(byte[] data, int seed)
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogWarning("[LightCipher] No data to encrypt");
                return data;
            }
            
            byte[] result = new byte[data.Length];
            Array.Copy(data, result, data.Length);
            
            EncryptInPlace(result, seed);
            return result;
        }
        
        /// <summary>
        /// Decrypt entire buffer (returns new array).
        /// </summary>
        public static byte[] Decrypt(byte[] data, int seed)
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogWarning("[LightCipher] No data to decrypt");
                return data;
            }
            
            byte[] result = new byte[data.Length];
            Array.Copy(data, result, data.Length);
            
            DecryptInPlace(result, seed);
            return result;
        }
        
        #endregion
        
        #region Public API - In-Place Encryption
        
        /// <summary>
        /// Encrypt buffer in-place (modifies original array).
        /// </summary>
        /// <param name="buffer">Buffer to encrypt</param>
        /// <param name="seed">Seed for key derivation</param>
        public static void EncryptInPlace(byte[] buffer, int seed)
        {
            EncryptInPlace(buffer, 0, buffer.Length, seed);
        }
        
        /// <summary>
        /// Decrypt buffer in-place (modifies original array).
        /// </summary>
        public static void DecryptInPlace(byte[] buffer, int seed)
        {
            DecryptInPlace(buffer, 0, buffer.Length, seed);
        }
        
        /// <summary>
        /// Encrypt buffer in-place with region control.
        /// </summary>
        /// <param name="buffer">Buffer to encrypt</param>
        /// <param name="skipBytes">Bytes to skip at start (for headers)</param>
        /// <param name="length">Number of bytes to encrypt</param>
        /// <param name="seed">Seed for key derivation</param>
        public static void EncryptInPlace(byte[] buffer, int skipBytes, int length, int seed)
        {
            if (buffer == null || buffer.Length == 0) return;
            if (skipBytes < 0 || skipBytes >= buffer.Length) return;
            if (length <= 0 || skipBytes + length > buffer.Length) return;
            
            byte[] key = GetKeyForSeed(seed);
            
            switch (activeCipher)
            {
                case CipherType.XOR:
                    XorCipher(buffer, skipBytes, length, key);
                    break;
                    
                case CipherType.RC4:
                    RC4Cipher(buffer, skipBytes, length, key);
                    break;
                    
                case CipherType.AES128:
                    // AES placeholder - requires System.Security.Cryptography
                    // For now, fall back to XOR
                    XorCipher(buffer, skipBytes, length, key);
                    break;
            }
        }
        
        /// <summary>
        /// Decrypt buffer in-place with region control.
        /// </summary>
        public static void DecryptInPlace(byte[] buffer, int skipBytes, int length, int seed)
        {
            // For symmetric ciphers (XOR, RC4), decrypt = encrypt
            EncryptInPlace(buffer, skipBytes, length, seed);
        }
        
        #endregion
        
        #region Cipher Algorithms
        
        /// <summary>
        /// XOR Cipher - Fast, lightweight, symmetric.
        /// Each byte is XORed with corresponding key byte.
        /// </summary>
        private static void XorCipher(byte[] buffer, int offset, int length, byte[] key)
        {
            int keyIndex = 0;
            int keyLength = key.Length;
            
            for (int i = offset; i < offset + length; i++)
            {
                buffer[i] ^= key[keyIndex];
                keyIndex = (keyIndex + 1) % keyLength;
            }
        }
        
        /// <summary>
        /// RC4 Stream Cipher - Moderate security, still fast.
        /// Implements standard RC4 KSA + PRGA.
        /// </summary>
        private static void RC4Cipher(byte[] buffer, int offset, int length, byte[] key)
        {
            // Key-scheduling algorithm (KSA)
            byte[] S = new byte[256];
            byte[] T = new byte[256];
            
            // Initialize S
            for (int i = 0; i < 256; i++)
            {
                S[i] = (byte)i;
                T[i] = key[i % key.Length];
            }
            
            // Initial permutation of S
            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + S[i] + T[i]) % 256;
                byte temp = S[i];
                S[i] = S[j];
                S[j] = temp;
            }
            
            // Pseudo-random generation algorithm (PRGA) + encrypt
            int i_idx = 0;
            j = 0;
            
            for (int k = offset; k < offset + length; k++)
            {
                i_idx = (i_idx + 1) % 256;
                j = (j + S[i_idx]) % 256;
                
                // Swap S[i] and S[j]
                byte temp = S[i_idx];
                S[i_idx] = S[j];
                S[j] = temp;
                
                // Generate keystream byte
                int t = (S[i_idx] + S[j]) % 256;
                byte K = S[t];
                
                // Encrypt/decrypt
                buffer[k] ^= K;
            }
        }
        
        #endregion
        
        #region Key Derivation
        
        /// <summary>
        /// Get or generate encryption key for seed.
        /// Uses caching to avoid regenerating keys for same seed.
        /// </summary>
        private static byte[] GetKeyForSeed(int seed)
        {
            // Check cache first
            if (_keyCache.TryGetValue(seed, out byte[] cachedKey))
            {
                return cachedKey;
            }
            
            // Generate new key
            byte[] key = GenerateKeyFromSeed(seed);
            
            // Add to cache (with size limit)
            if (_keyCache.Count >= MAX_CACHE_SIZE)
            {
                // Remove oldest entry
                var enumerator = _keyCache.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    _keyCache.Remove(enumerator.Current.Key);
                }
            }
            
            _keyCache[seed] = key;
            return key;
        }
        
        /// <summary>
        /// Generate 256-byte encryption key from seed.
        /// Uses linear congruential generator for reproducibility.
        /// </summary>
        private static byte[] GenerateKeyFromSeed(int seed)
        {
            byte[] key = new byte[256];
            int state = seed;
            
            // LCG parameters (same as glibc)
            const int A = 1103515245;
            const int C = 12345;
            
            for (int i = 0; i < 256; i++)
            {
                // Generate next pseudo-random number
                state = checked(state * A + C);
                
                // Extract byte from middle bits (better distribution)
                key[i] = (byte)((state >> 16) & 0xFF);
            }
            
            // Additional mixing for better distribution
            MixKey(key);
            
            return key;
        }
        
        /// <summary>
        /// Mix key bytes for better distribution.
        /// Simple XOR folding to reduce patterns.
        /// </summary>
        private static void MixKey(byte[] key)
        {
            for (int i = 0; i < key.Length / 2; i++)
            {
                key[i] ^= key[key.Length - 1 - i];
            }
        }
        
        #endregion
        
        #region Utility Functions
        
        /// <summary>
        /// Clear the key cache (for security or memory management).
        /// </summary>
        public static void ClearKeyCache()
        {
            _keyCache.Clear();
            Debug.Log("[LightCipher] Key cache cleared");
        }
        
        /// <summary>
        /// Get cache statistics.
        /// </summary>
        public static int GetCacheSize() => _keyCache.Count;
        
        /// <summary>
        /// Set active cipher algorithm.
        /// </summary>
        public static void SetCipher(CipherType type)
        {
            activeCipher = type;
            Debug.Log($"[LightCipher] Cipher set to {type}");
        }
        
        /// <summary>
        /// Get current cipher algorithm.
        /// </summary>
        public static CipherType GetCipher() => activeCipher;
        
        /// <summary>
        /// Validate encryption/decryption round-trip.
        /// </summary>
        public static bool ValidateRoundTrip(byte[] testData, int testSeed)
        {
            byte[] encrypted = Encrypt(testData, testSeed);
            byte[] decrypted = Decrypt(encrypted, testSeed);
            
            if (decrypted.Length != testData.Length)
            {
                Debug.LogError("[LightCipher] Round-trip failed: length mismatch");
                return false;
            }
            
            for (int i = 0; i < testData.Length; i++)
            {
                if (decrypted[i] != testData[i])
                {
                    Debug.LogError($"[LightCipher] Round-trip failed: byte {i} mismatch");
                    return false;
                }
            }
            
            Debug.Log("[LightCipher] Round-trip validation successful");
            return true;
        }
        
        #endregion
        
        #region Debug
        
        /// <summary>
        /// Print buffer info for debugging.
        /// </summary>
        public static void DebugBuffer(byte[] buffer, string label, int maxBytes = 32)
        {
            if (buffer == null)
            {
                Debug.Log($"[LightCipher] {label}: null");
                return;
            }
            
            string hex = BitConverter.ToString(buffer, 0, Math.Min(buffer.Length, maxBytes));
            Debug.Log($"[LightCipher] {label}: {buffer.Length} bytes | {hex}{(buffer.Length > maxBytes ? "..." : "")}");
        }
        
        #endregion
    }
}
