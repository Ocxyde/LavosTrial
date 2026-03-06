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
// DoorSFXManager.cs
// Generates and manages 8-bit style door sound effects
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Location: Assets/Art/DoorSFX/
//
// Plug-in-and-Out: Works with DoorsEngine

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Door sound effects manager.
    /// Generates 8-bit style procedural sounds for doors.
    /// No external audio files needed - all synthesized!
    /// </summary>
    public static class DoorSFXManager
    {
        #region Cached AudioClips

        private static AudioClip _openNormalClip;
        private static AudioClip _closeNormalClip;
        private static AudioClip _openWoodClip;
        private static AudioClip _closeWoodClip;
        private static AudioClip _openStoneClip;
        private static AudioClip _closeStoneClip;
        private static AudioClip _openMetalClip;
        private static AudioClip _closeMetalClip;
        private static AudioClip _openMagicClip;
        private static AudioClip _closeMagicClip;
        private static AudioClip _openIronClip;
        private static AudioClip _closeIronClip;
        private static AudioClip _lockedClip;
        private static AudioClip _unlockClip;
        private static AudioClip _creakClip;

        #endregion

        #region Public Getters

        public static AudioClip GetOpenSound(DoorVariant variant)
        {
            return variant switch
            {
                DoorVariant.Normal => GetOpenNormalSound(),
                DoorVariant.Locked => GetOpenWoodSound(),
                DoorVariant.Trapped => GetOpenStoneSound(),
                DoorVariant.Secret => GetOpenMagicSound(),
                DoorVariant.Blessed => GetOpenMagicSound(),
                DoorVariant.Cursed => GetOpenIronSound(),
                DoorVariant.Boss => GetOpenIronSound(),
                DoorVariant.OneWay => GetOpenWoodSound(),
                _ => GetOpenNormalSound()
            };
        }

        public static AudioClip GetCloseSound(DoorVariant variant)
        {
            return variant switch
            {
                DoorVariant.Normal => GetCloseNormalSound(),
                DoorVariant.Locked => GetCloseWoodSound(),
                DoorVariant.Trapped => GetCloseStoneSound(),
                DoorVariant.Secret => GetCloseMagicSound(),
                DoorVariant.Blessed => GetCloseMagicSound(),
                DoorVariant.Cursed => GetCloseIronSound(),
                DoorVariant.Boss => GetCloseIronSound(),
                DoorVariant.OneWay => GetCloseWoodSound(),
                _ => GetCloseNormalSound()
            };
        }

        public static AudioClip GetLockedSound() => _lockedClip ??= GenerateLockedSound();
        public static AudioClip GetUnlockSound() => _unlockClip ??= GenerateUnlockSound();
        public static AudioClip GetCreakSound() => _creakClip ??= GenerateCreakSound();

        #endregion

        #region Normal Door Sounds

        private static AudioClip GetOpenNormalSound()
        {
            if (_openNormalClip == null)
            {
                _openNormalClip = GenerateDoorSound(0.3f, 440, 330, "door_open_normal");
            }
            return _openNormalClip;
        }

        private static AudioClip GetCloseNormalSound()
        {
            if (_closeNormalClip == null)
            {
                _closeNormalClip = GenerateDoorSound(0.25f, 330, 220, "door_close_normal");
            }
            return _closeNormalClip;
        }

        #endregion

        #region Wood Door Sounds

        private static AudioClip GetOpenWoodSound()
        {
            if (_openWoodClip == null)
            {
                _openWoodClip = GenerateWoodDoorSound(true, "door_open_wood");
            }
            return _openWoodClip;
        }

        private static AudioClip GetCloseWoodSound()
        {
            if (_closeWoodClip == null)
            {
                _closeWoodClip = GenerateWoodDoorSound(false, "door_close_wood");
            }
            return _closeWoodClip;
        }

        #endregion

        #region Stone Door Sounds

        private static AudioClip GetOpenStoneSound()
        {
            if (_openStoneClip == null)
            {
                _openStoneClip = GenerateStoneDoorSound(true, "door_open_stone");
            }
            return _openStoneClip;
        }

        private static AudioClip GetCloseStoneSound()
        {
            if (_closeStoneClip == null)
            {
                _closeStoneClip = GenerateStoneDoorSound(false, "door_close_stone");
            }
            return _closeStoneClip;
        }

        #endregion

        #region Metal Door Sounds

        private static AudioClip GetOpenMetalSound()
        {
            if (_openMetalClip == null)
            {
                _openMetalClip = GenerateMetalDoorSound(true, "door_open_metal");
            }
            return _openMetalClip;
        }

        private static AudioClip GetCloseMetalSound()
        {
            if (_closeMetalClip == null)
            {
                _closeMetalClip = GenerateMetalDoorSound(false, "door_close_metal");
            }
            return _closeMetalClip;
        }

        #endregion

        #region Magic Door Sounds

        private static AudioClip GetOpenMagicSound()
        {
            if (_openMagicClip == null)
            {
                _openMagicClip = GenerateMagicDoorSound(true, "door_open_magic");
            }
            return _openMagicClip;
        }

        private static AudioClip GetCloseMagicSound()
        {
            if (_closeMagicClip == null)
            {
                _closeMagicClip = GenerateMagicDoorSound(false, "door_close_magic");
            }
            return _closeMagicClip;
        }

        #endregion

        #region Iron Door Sounds

        private static AudioClip GetOpenIronSound()
        {
            if (_openIronClip == null)
            {
                _openIronClip = GenerateIronDoorSound(true, "door_open_iron");
            }
            return _openIronClip;
        }

        private static AudioClip GetCloseIronSound()
        {
            if (_closeIronClip == null)
            {
                _closeIronClip = GenerateIronDoorSound(false, "door_close_iron");
            }
            return _closeIronClip;
        }

        #endregion

        #region Special Sounds

        private static AudioClip GenerateLockedSound()
        {
            // Short high-pitched "clunk"
            return GenerateToneSound(0.15f, 800, 600, 0.1f, "door_locked");
        }

        private static AudioClip GenerateUnlockSound()
        {
            // Satisfying "click" with rising pitch
            return GenerateToneSound(0.2f, 600, 900, 0.05f, "door_unlock");
        }

        private static AudioClip GenerateCreakSound()
        {
            // Old door creak - noise based
            return GenerateNoiseSound(0.4f, 200, 400, "door_creak");
        }

        #endregion

        #region Sound Generation

        /// <summary>
        /// Generate generic door opening/closing sound.
        /// </summary>
        private static AudioClip GenerateDoorSound(float duration, float startFreq, float endFreq, string name)
        {
            int sampleRate = 22050;
            int samples = Mathf.FloorToInt(duration * sampleRate);

            float[] samplesData = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);

                // Square wave for 8-bit feel
                float wave = GenerateSquareWave(freq, i, sampleRate);

                // Envelope (attack-decay)
                float envelope = 1f - t;
                envelope *= Mathf.Pow(t * 3, 0.5f); // Quick attack

                // Add some noise for texture
                float noise = (Random.value - 0.5f) * 0.15f;

                samplesData[i] = (wave * 0.7f + noise) * envelope * 0.5f;
            }

            return CreateAudioClip(samplesData, sampleRate, name);
        }

        /// <summary>
        /// Generate wood door sound with creak.
        /// </summary>
        private static AudioClip GenerateWoodDoorSound(bool isOpen, string name)
        {
            float duration = isOpen ? 0.5f : 0.35f;
            int sampleRate = 22050;
            int samples = Mathf.FloorToInt(duration * sampleRate);

            float[] samplesData = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;

                // Low wood creak frequency
                float freq = isOpen ? Mathf.Lerp(150, 100, t) : Mathf.Lerp(200, 150, t);

                // Sawtooth wave for wood texture
                float wave = GenerateSawtoothWave(freq, i, sampleRate);

                // Add creak modulation
                float creak = Mathf.Sin(t * 50) * 0.1f;

                // Envelope
                float envelope = isOpen ? (1f - t) : Mathf.Pow(1f - t, 2);

                samplesData[i] = (wave * 0.6f + creak) * envelope * 0.4f;
            }

            return CreateAudioClip(samplesData, sampleRate, name);
        }

        /// <summary>
        /// Generate stone door sound (grinding).
        /// </summary>
        private static AudioClip GenerateStoneDoorSound(bool isOpen, string name)
        {
            float duration = isOpen ? 0.7f : 0.5f;
            int sampleRate = 22050;
            int samples = Mathf.FloorToInt(duration * sampleRate);

            float[] samplesData = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;

                // Very low frequency for heavy stone
                float freq = isOpen ? Mathf.Lerp(80, 50, t) : Mathf.Lerp(100, 70, t);

                // Triangle wave
                float wave = GenerateTriangleWave(freq, i, sampleRate);

                // Add grinding noise
                float grind = (Random.value - 0.5f) * 0.3f;

                // Heavy envelope
                float envelope = Mathf.Pow(1f - t, 3);

                samplesData[i] = (wave * 0.5f + grind) * envelope * 0.6f;
            }

            return CreateAudioClip(samplesData, sampleRate, name);
        }

        /// <summary>
        /// Generate metal door sound (clang).
        /// </summary>
        private static AudioClip GenerateMetalDoorSound(bool isOpen, string name)
        {
            float duration = isOpen ? 0.4f : 0.3f;
            int sampleRate = 22050;
            int samples = Mathf.FloorToInt(duration * sampleRate);

            float[] samplesData = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;

                // High metallic frequency
                float freq = isOpen ? Mathf.Lerp(600, 400, t) : Mathf.Lerp(700, 500, t);

                // Sine wave with harmonics
                float wave = Mathf.Sin(2 * Mathf.PI * freq * i / sampleRate);
                wave += Mathf.Sin(2 * Mathf.PI * freq * 2 * i / sampleRate) * 0.5f;

                // Metallic ring
                float ring = Mathf.Sin(t * 200) * Mathf.Exp(-t * 5);

                // Sharp envelope
                float envelope = Mathf.Exp(-t * 8);

                samplesData[i] = (wave * 0.4f + ring) * envelope * 0.5f;
            }

            return CreateAudioClip(samplesData, sampleRate, name);
        }

        /// <summary>
        /// Generate magic door sound (ethereal).
        /// </summary>
        private static AudioClip GenerateMagicDoorSound(bool isOpen, string name)
        {
            float duration = isOpen ? 0.6f : 0.45f;
            int sampleRate = 22050;
            int samples = Mathf.FloorToInt(duration * sampleRate);

            float[] samplesData = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;

                // Ethereal frequencies
                float freq1 = isOpen ? Mathf.Lerp(440, 880, t) : Mathf.Lerp(660, 440, t);
                float freq2 = freq1 * 1.5f; // Perfect fifth

                // Dual sine waves
                float wave = Mathf.Sin(2 * Mathf.PI * freq1 * i / sampleRate);
                wave += Mathf.Sin(2 * Mathf.PI * freq2 * i / sampleRate) * 0.5f;

                // Shimmer effect
                float shimmer = Mathf.Sin(t * 30) * 0.2f;

                // Smooth envelope
                float envelope = Mathf.Sin(t * Mathf.PI);

                samplesData[i] = (wave * 0.5f + shimmer) * envelope * 0.4f;
            }

            return CreateAudioClip(samplesData, sampleRate, name);
        }

        /// <summary>
        /// Generate iron door sound (heavy clang).
        /// </summary>
        private static AudioClip GenerateIronDoorSound(bool isOpen, string name)
        {
            float duration = isOpen ? 0.55f : 0.4f;
            int sampleRate = 22050;
            int samples = Mathf.FloorToInt(duration * sampleRate);

            float[] samplesData = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;

                // Heavy iron frequency
                float freq = isOpen ? Mathf.Lerp(300, 200, t) : Mathf.Lerp(350, 250, t);

                // Square wave for harsh iron sound
                float wave = GenerateSquareWave(freq, i, sampleRate);

                // Add rust/grind
                float rust = (Random.value - 0.5f) * 0.2f;

                // Heavy decay
                float envelope = Mathf.Exp(-t * 6);

                samplesData[i] = (wave * 0.5f + rust) * envelope * 0.55f;
            }

            return CreateAudioClip(samplesData, sampleRate, name);
        }

        /// <summary>
        /// Generate simple tone sound.
        /// </summary>
        private static AudioClip GenerateToneSound(float duration, float startFreq, float endFreq, float noiseAmount, string name)
        {
            int sampleRate = 22050;
            int samples = Mathf.FloorToInt(duration * sampleRate);

            float[] samplesData = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);

                float wave = Mathf.Sin(2 * Mathf.PI * freq * i / sampleRate);
                float noise = (Random.value - 0.5f) * noiseAmount;

                samplesData[i] = (wave * 0.8f + noise) * 0.5f;
            }

            return CreateAudioClip(samplesData, sampleRate, name);
        }

        /// <summary>
        /// Generate noise-based sound.
        /// </summary>
        private static AudioClip GenerateNoiseSound(float duration, float minFreq, float maxFreq, string name)
        {
            int sampleRate = 22050;
            int samples = Mathf.FloorToInt(duration * sampleRate);

            float[] samplesData = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;

                // Filtered noise
                float noise = (Random.value - 0.5f) * 2f;

                // Frequency-based filtering
                float filter = Mathf.Sin(t * minFreq * Mathf.PI) * Mathf.Cos(t * maxFreq * Mathf.PI);

                samplesData[i] = noise * filter * 0.4f;
            }

            return CreateAudioClip(samplesData, sampleRate, name);
        }

        #endregion

        #region Wave Generators

        private static float GenerateSquareWave(float freq, int sample, int sampleRate)
        {
            float t = sample / (float)sampleRate;
            return Mathf.Sign(Mathf.Sin(2 * Mathf.PI * freq * t));
        }

        private static float GenerateSawtoothWave(float freq, int sample, int sampleRate)
        {
            float t = sample / (float)sampleRate;
            return 2f * (t * freq - Mathf.Floor(t * freq + 0.5f));
        }

        private static float GenerateTriangleWave(float freq, int sample, int sampleRate)
        {
            float t = sample / (float)sampleRate;
            return 2f * Mathf.Abs(2f * (t * freq - Mathf.Floor(t * freq + 0.5f))) - 1f;
        }

        #endregion

        #region AudioClip Creation

        private static AudioClip CreateAudioClip(float[] samples, int sampleRate, string name)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        #endregion

        #region Playback

        /// <summary>
        /// Play door sound at position.
        /// </summary>
        public static void PlayDoorSound(DoorVariant variant, bool isOpen, Vector3 position, float volume = 1f)
        {
            AudioClip clip = isOpen ? GetOpenSound(variant) : GetCloseSound(variant);

            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, volume);
            }
        }

        /// <summary>
        /// Play locked sound.
        /// </summary>
        public static void PlayLockedSound(Vector3 position, float volume = 1f)
        {
            AudioSource.PlayClipAtPoint(GetLockedSound(), position, volume);
        }

        /// <summary>
        /// Play unlock sound.
        /// </summary>
        public static void PlayUnlockSound(Vector3 position, float volume = 1f)
        {
            AudioSource.PlayClipAtPoint(GetUnlockSound(), position, volume);
        }

        #endregion

        #region Cache Management

        public static void ClearCache()
        {
            _openNormalClip = null;
            _closeNormalClip = null;
            _openWoodClip = null;
            _closeWoodClip = null;
            _openStoneClip = null;
            _closeStoneClip = null;
            _openMetalClip = null;
            _closeMetalClip = null;
            _openMagicClip = null;
            _closeMagicClip = null;
            _openIronClip = null;
            _closeIronClip = null;
            _lockedClip = null;
            _unlockClip = null;
            _creakClip = null;
        }

        #endregion
    }
}
